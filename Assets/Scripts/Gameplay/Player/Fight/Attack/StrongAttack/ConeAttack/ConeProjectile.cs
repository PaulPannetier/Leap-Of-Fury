using UnityEngine;
using Collider2D = UnityEngine.Collider2D;

public class ConeProjectile : MonoBehaviour
{
    private new Transform transform;
    private LayerMask charMask, groundMask, wallProjectileMask;
    private bool isFlying;
    private bool isLanding;
    private bool isFalling;//true when the projectile fall and kill other player
    private ConeProjectileAttack attack;
    private Animator animator;
    private float lastTimeLauch;
    private PlayerCommon playerCommon;
    private Rigidbody2D rb;
    private CircleCollider2D circleCollider;

    [SerializeField] private float maxDurationInAir;
    [SerializeField] private float minDurationBeforePick;
    [SerializeField] private float gravityScale = 1f;

    private void Awake()
    {
        this.transform = base.transform;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
    }

    private void Start()
    {
        charMask = LayerMask.GetMask("Char");
        groundMask = LayerMask.GetMask("Floor");
        wallProjectileMask = LayerMask.GetMask("WallProjectile");
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
    }

    public void OnAttackReLaunch()
    {
        DestroyProjectile();
    }

    private void Pick()
    {
        attack.PickProjectile(this);
        DestroyProjectile();
    }

    public void Launch(float speed, in Vector2 dir, ConeProjectileAttack attack)
    {
        this.attack = attack;
        isFlying = true;
        playerCommon = attack.GetComponent<PlayerCommon>();
        rb.linearVelocity = dir * speed;

        float angleDeg = Useful.AngleHori(Vector2.zero, dir) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angleDeg);
        lastTimeLauch = Time.time;
    }

    private void FixedUpdate()
    {
        if(PauseManager.instance.isPauseEnable)
        {
            lastTimeLauch += Time.fixedDeltaTime;
            return;
        }

        bool isLauncherCollide = false;
        if(isFlying)
        {
            if(Time.time - lastTimeLauch > maxDurationInAir)
            {
                DestroyProjectile();
                return;
            }
        }

        if(isLauncherCollide)
        {
            Pick();
            return;
        }

        if(isLanding)
        {
            if (rb.linearVelocity.sqrMagnitude >= 0.25f)
            {
                Fall();
            }
        }
    }

    //collision.otherCollider <=> this.collider, collision.collider <=> collider this gameobject collide with
    private void OnCollisionEnter2D(UnityEngine.Collision2D collision)
    {
        bool isPlayerCollision = charMask.Contain(collision.collider.gameObject.layer);
        if(isPlayerCollision)
        {
            GameObject player = collision.collider.gameObject.GetComponent<ToricObject>().original;
            uint id = player.GetComponent<PlayerCommon>().id;
            if(id == playerCommon.id)
            {
                if (Time.time - lastTimeLauch > minDurationBeforePick)
                {
                    Pick();
                }
            }
            else
            {
                if(isFalling || isFlying)
                {
                    PlayerTouch(player);
                }
            }
            return;
        }

        bool isGroundCollision = groundMask.Contain(collision.collider.gameObject.layer);
        if(isGroundCollision)
        {
            if (isFlying)
            {
                rb.linearVelocity = Vector2.zero;
                Land(collision.otherCollider);
            }
            else if (isFalling)
            {
                Land(collision.otherCollider);
            }
            else if (isLanding)
            {
                //Nothing to do
            }
        }
    }

    private void Fall()
    {
        isFlying = isLanding = false;
        isFalling = true;
        animator.CrossFade("idle", 0);
    }

    private void Land(Collider2D grounCol)
    {
        Vector2 center = (Vector2)transform.position + circleCollider.offset;
        Collider2D col = PhysicsToric.OverlapPoint(center, wallProjectileMask);
        if (col != null)
        {
            DestroyProjectile();
            return;
        }

        isFlying = isFalling = false;
        isLanding = true;
        animator.CrossFade("land", 0);

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = gravityScale;
    }

    private void PlayerTouch(GameObject player)
    {
        attack.OnProjectileTouchPlayer(player);
    }

    private void DestroyProjectile()
    {
        attack.OnProjectileDestroy(this);
        Destroy(gameObject);
    }

    #region Gizmos / OnValidate

#if UNITY_EDITOR

    private void OnValidate()
    {
        this.transform = base.transform;
        maxDurationInAir = Mathf.Max(maxDurationInAir, 0f);
        minDurationBeforePick = Mathf.Max(minDurationBeforePick, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        
    }

#endif

    #endregion
}
