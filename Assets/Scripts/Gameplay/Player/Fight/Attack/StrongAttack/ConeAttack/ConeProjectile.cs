using Collision2D;
using UnityEngine;
using System.Collections.Generic;
using Collider2D = UnityEngine.Collider2D;

public class ConeProjectile : MonoBehaviour
{
    private new Transform transform;
    private LayerMask charMask, groundMask, wallProjectileMask;
    private bool isFlying, isInTheWall, isLanding, isFalling;
    private ConeProjectileAttack attack;
    private Rigidbody2D rb;
    private Animator animator;
    private float lastTimeLaunch, lastTimeBeginToFall, lastTimeEnterInWall = -10f;
    private float durationToStayInTheWall;
    private Vector2 wallNormal;
    private PlayerCommon playerCommon;
    private List<PlayerTouch> charAlreadyTouch;
    private FloorShockWave explosionShockWave;

    [SerializeField] private Vector2 pickColliderSize;
    [SerializeField] private Vector2 charColliderOffset;
    [SerializeField] private float charColliderRadius;
    [SerializeField] private float minDurationBeforePick;
    [SerializeField] private float flyGravityScale = 1f;
    [SerializeField, Range(0f, 180f)] private float angleAfterTouchPlayer = 0f;
    [SerializeField] private float speedAfterTouchPlayer = 0f;
    [SerializeField] private float angularSpeedAfterTouchPlayer = 0f;
    [SerializeField] private float fallGravityScale = 1f;
    [SerializeField] private float maxFallSpeed = 1f;
    [SerializeField] private float speedExitingWall = 0.5f;
    [SerializeField] private float minDurationInTheWall = 0.5f;
    [SerializeField] private float maxDurationInTheWall = 1.5f;
    [SerializeField] private float minTorgueAfterExitWall = 1f;
    [SerializeField] private float maxTorgueAfterExitWall = 1f;
    [SerializeField] private float otherPlayerInvisibilityDurationAfterTouch = 0.5f;
    [SerializeField] private float explosionRadius = 0.5f;
    [SerializeField] private Explosion.ExplosionData explosionData;
    [SerializeField] private Explosion explosionPrefabs;
    [SerializeField] private Component[] componentsToDestroyWhenExplode;


#if UNITY_EDITOR
    [SerializeField] private bool drawGizmos;
#endif

    private void Awake()
    {
        this.transform = base.transform;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        charAlreadyTouch = new List<PlayerTouch>(4);
        lastTimeBeginToFall = -10f;
    }

    private void Start()
    {
        charMask = LayerMask.GetMask("Char");
        groundMask = LayerMask.GetMask("Floor");
        wallProjectileMask = LayerMask.GetMask("WallProjectile");
        PauseManager.instance.callBackOnPauseEnable += OnPauseEnable;
        PauseManager.instance.callBackOnPauseDisable += OnPauseDisable;
    }

    public void Launch(float speed, in Vector2 dir, ConeProjectileAttack attack)
    {
        this.attack = attack;
        isFlying = true;
        playerCommon = attack.GetComponent<PlayerCommon>();
        rb.linearVelocity = dir * speed;
        rb.gravityScale = flyGravityScale;

        float angleDeg = Useful.AngleHori(Vector2.zero, dir) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angleDeg);
        lastTimeLaunch = Time.time;
    }

    private void Update()
    {
        if (PauseManager.instance.isPauseEnable)
        {
            lastTimeLaunch += Time.deltaTime;
            lastTimeBeginToFall = lastTimeBeginToFall >= 0f ? lastTimeBeginToFall + Time.fixedDeltaTime : lastTimeBeginToFall;
            lastTimeEnterInWall = lastTimeEnterInWall >= 0f ? lastTimeEnterInWall + Time.fixedDeltaTime : lastTimeEnterInWall;

            for (int i = 0; i < charAlreadyTouch.Count; i++)
            {
                PlayerTouch current = charAlreadyTouch[i];
                charAlreadyTouch[i] = new PlayerTouch(current.time + Time.deltaTime, current.id);
            }
            return;
        }

        for (int i = charAlreadyTouch.Count - 1; i >= 0; i--)
        {
            if (Time.time - charAlreadyTouch[i].time >= otherPlayerInvisibilityDurationAfterTouch)
            {
                charAlreadyTouch.RemoveAt(i);
            }
        }
    }

    private void FixedUpdate()
    {
        HandleFly();

        HandleProjectileInTheWall();

        HandleFall();

        HandleLand();
    }

    private Circle GetCharCollider()
    {
        float offsetAngle = Useful.AngleHori(Vector2.zero, charColliderOffset);
        float dirAngle = transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
        float totalAngle = Useful.WrapAngle(offsetAngle + dirAngle);
        Vector2 offset = Useful.Vector2FromAngle(totalAngle, charColliderOffset.magnitude);
        Vector2 center = (Vector2)transform.position + offset;
        return new Circle(center, charColliderRadius);
    }

    private bool IsPickUp()
    {
        Collider2D[] cols = PhysicsToric.OverlapBoxAll((Vector2)transform.position, pickColliderSize, transform.rotation.eulerAngles.z * Mathf.Deg2Rad, charMask);
        foreach (Collider2D col in cols)
        {
            if (col.CompareTag("Char"))
            {
                GameObject player = col.GetComponent<ToricObject>().original;
                uint id = player.GetComponent<PlayerCommon>().id;
                if (id == playerCommon.id)
                    return true;
            }
        }
        return false;
    }

    private bool HaveAlreadyTouch(uint id)
    {
        foreach (PlayerTouch playerTouch in charAlreadyTouch)
        {
            if(playerTouch.id == id)
                return true;
        }
        return false;
    }

    private bool IsTouchingOtherChar(out Collider2D[] playersTouch)
    {
        Circle charCollider = GetCharCollider();
        Collider2D[] cols = PhysicsToric.OverlapCircleAll(charCollider.center, charCollider.radius, charMask);
        List<Collider2D> charTouch = new List<Collider2D>(cols.Length);

        foreach (Collider2D col in cols)
        {
            if (col.CompareTag("Char"))
            {
                GameObject player = col.GetComponent<ToricObject>().original;
                uint id = player.GetComponent<PlayerCommon>().id;
                if (id != playerCommon.id && !HaveAlreadyTouch(id))
                {
                    charTouch.Add(col);
                    charAlreadyTouch.Add(new PlayerTouch(Time.time, id));
                }
            }
        }

        playersTouch = charTouch.ToArray();
        return playersTouch.Length > 0;
    }

    private void HandleFly()
    {
        if (!isFlying)
            return;

        if (Time.time - lastTimeLaunch > minDurationBeforePick)
        {
            if (IsPickUp())
            {
                attack.PickProjectile(this);
                DestroyProjectile();
                return;
            }
        }

        if (IsTouchingOtherChar(out Collider2D[] otherChar))
        {
            foreach (Collider2D collider in otherChar)
            {
                attack.OnProjectileTouchPlayer(collider.GetComponent<ToricObject>().original);
            }

            float angle = 90f + (rb.linearVelocity.x >= 0f ? angleAfterTouchPlayer : -angleAfterTouchPlayer);
            float angularSpeed = rb.linearVelocity.x >= 0f ? angularSpeedAfterTouchPlayer : -angularSpeedAfterTouchPlayer;
            Vector2 newVel = Useful.Vector2FromAngle(angle * Mathf.Deg2Rad, speedAfterTouchPlayer);
            rb.linearVelocity = newVel;
            rb.angularVelocity = angularSpeed;
            rb.gravityScale = fallGravityScale;
            isFlying = false;
            isFalling = true;
            charAlreadyTouch.Clear();
        }

        rb.SetRotation(rb.linearVelocity.Angle(Vector2.right) * Mathf.Rad2Deg);
    }

    private void HandleProjectileInTheWall()
    {
        if (!isInTheWall)
            return;

        if(Time.time - lastTimeEnterInWall >= durationToStayInTheWall)
        {
            rb.constraints = RigidbodyConstraints2D.None;
            rb.gravityScale = fallGravityScale;
            rb.linearVelocity = wallNormal * speedExitingWall;
            float torgue = Random.Rand(minTorgueAfterExitWall, maxTorgueAfterExitWall);
            torgue *= Random.Rand() >= 0.5f ? 1f : -1f;
            rb.AddTorque(torgue);
            animator.CrossFade("idle", 0);

            isInTheWall = false;
            charAlreadyTouch.Clear();
            isFalling = true;
        }
    }

    private void HandleFall()
    {
        if(!isFalling) 
            return;

        if(IsTouchingOtherChar(out Collider2D[] players))
        {
            foreach(Collider2D collider in players)
            {
                GameObject player = collider.GetComponent<ToricObject>().original;
                attack.OnProjectileTouchPlayer(player);
            }
        }

        if(IsPickUp())
        {
            attack.PickProjectile(this);
            DestroyProjectile();
            return;
        }

        float currentSpeed = rb.linearVelocity.sqrMagnitude;
        if(currentSpeed > maxFallSpeed * maxFallSpeed)
        {
            rb.linearVelocity *= (maxFallSpeed / Mathf.Sqrt(currentSpeed));
        }
    }

    private void HandleLand()
    {
        if(!isLanding) 
            return;

        if (IsPickUp())
        {
            attack.PickProjectile(this);
            DestroyProjectile();
        }
    }

    private void Land()
    {
        Collider2D col = PhysicsToric.OverlapPoint(transform.position, wallProjectileMask);
        if (col != null)
        {
            attack.PickProjectile(this);
            DestroyProjectile();
            return;
        }

        isLanding = true;
        isFlying = false;
        isFalling = false;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        animator.CrossFade("land", 0);
    }

    private void OnExplosionTouchPlayer(Collider2D collider)
    {
        GameObject player = collider.GetComponent<ToricObject>().original;
        PlayerCommon playerCommon = player.GetComponent<PlayerCommon>();

        if(playerCommon.id != this.playerCommon.id)
        {
            explosionShockWave.OnKunaiExplosionTouchPlayer(player);
        }
    }

    private void OnExplosionDestoy(Explosion explosion)
    {
        DestroyProjectile();
    }

    public void ExplodeByShockWave(FloorShockWave shockWave)
    {
        isFalling = isFlying = isInTheWall = isLanding = false;

        foreach (Component component in componentsToDestroyWhenExplode)
        {
            Destroy(component);
        }

        explosionShockWave = shockWave;
        Explosion explosion = Instantiate(explosionPrefabs, transform.position, Quaternion.identity, CloneParent.cloneParent);
        explosion.Launch(explosionData);
        explosion.callbackOnTouch += OnExplosionTouchPlayer;
        explosion.callbackOnDestroy += OnExplosionDestoy;
        attack.PickProjectile(this);
    }

    private void DestroyProjectile()
    {
        Destroy(gameObject);
    }

    //collision.otherCollider <=> this.collider, collision.collider <=> collider this gameobject collide with
    private void OnCollisionEnter2D(UnityEngine.Collision2D collision)
    {
        bool isGroundCollision = groundMask.Contain(collision.collider.gameObject.layer);
        if(isGroundCollision)
        {
            if (isFlying)
            {
                Vector2 normal = Vector2.zero;
                Collision2D.Collider2D groundCol = Collision2D.Collider2D.FromUnityCollider2D(collision.collider);
                if (!groundCol.Normal(collision.contacts[0].point, out normal))
                {
                    normal = Vector2.zero;
                    foreach (ContactPoint2D contact in collision.contacts)
                    {
                        normal += contact.normal;
                    }
                    normal = (normal / collision.contacts.Length).normalized;
                }

                bool isWallCollsion = !(Mathf.Abs(normal.x) < Mathf.Abs(normal.y) && normal.y > 0f);
                if(isWallCollsion)
                {
                    lastTimeEnterInWall = Time.time;
                    wallNormal = normal;
                    durationToStayInTheWall = Random.Rand(minDurationInTheWall, maxDurationInTheWall);
                    rb.linearVelocity = Vector2.zero;
                    rb.gravityScale = 0f;
                    rb.constraints = RigidbodyConstraints2D.FreezeAll;
                    isInTheWall = true;
                    isFlying = false;
                    animator.CrossFade("land", 0);
                }
                else
                {
                    Land();
                }
            }
            else if (isFalling)
            {
                Land();
            }
        }
    }

    private void OnPauseEnable()
    {
        animator.speed = 0f;
    }

    private void OnPauseDisable()
    {
        animator.speed = 1f;
    }

    #region Gizmos / OnValidate

    private void OnDestroy()
    {
        PauseManager.instance.callBackOnPauseEnable -= OnPauseEnable;
        PauseManager.instance.callBackOnPauseDisable -= OnPauseDisable;
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        this.transform = base.transform;
        minDurationBeforePick = Mathf.Max(minDurationBeforePick, 0f);
        pickColliderSize = new Vector2(Mathf.Max(pickColliderSize.x, 0f), Mathf.Max(pickColliderSize.y, 0f));
        charColliderRadius = Mathf.Max(charColliderRadius, 0f);
        minDurationInTheWall = Mathf.Max(minDurationInTheWall, 0f);
        maxDurationInTheWall = Mathf.Max(maxDurationInTheWall, 0f);
        speedAfterTouchPlayer = Mathf.Max(speedAfterTouchPlayer, 0f);
        fallGravityScale = Mathf.Max(fallGravityScale, 0f);
        maxFallSpeed = Mathf.Max(maxFallSpeed, 0f);
        minTorgueAfterExitWall = Mathf.Max(minTorgueAfterExitWall, 0f);
        maxTorgueAfterExitWall = Mathf.Max(maxTorgueAfterExitWall, 0f);
        explosionRadius = Mathf.Max(explosionRadius, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        if(!drawGizmos)
            return;
        Hitbox h = new Hitbox((Vector2)transform.position, pickColliderSize);
        h.Rotate(transform.rotation.eulerAngles.z * Mathf.Deg2Rad);
        Hitbox.GizmosDraw(h, Color.red);
        Circle charCollider = GetCharCollider();
        Circle.GizmosDraw(charCollider, Color.red);
        Circle.GizmosDraw(transform.position, explosionRadius, Color.black);
    }

#endif

    #endregion

    private struct PlayerTouch
    {
        public float time;
        public uint id;

        public PlayerTouch(float time, uint id) 
        {
            this.time = time;
            this.id = id;
        }
    }
}
