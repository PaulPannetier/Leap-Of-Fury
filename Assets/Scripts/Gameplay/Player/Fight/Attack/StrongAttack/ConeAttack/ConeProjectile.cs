using Collision2D;
using UnityEngine;
using System.Collections.Generic;
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
    private float lastTimeLaunch, lastTimeBeginToFall;
    private PlayerCommon playerCommon;
    private Rigidbody2D rb;
    private CircleCollider2D circleCollider;
    private List<uint> charAlreadyTouch;

    [SerializeField] private Vector2 pickColliderSize;
    [SerializeField] private Vector2 pickColliderOffset;
    [SerializeField] private Vector2 charColliderOffset;
    [SerializeField] private float charColliderRadius;
    [SerializeField] private float maxDurationInAir;
    [SerializeField] private float minDurationBeforePick;
    [SerializeField] private float gravityScale = 1f;
    [SerializeField] private float minDurationToFall = 0.5f;

#if UNITY_EDITOR
    [SerializeField] private bool drawGizmos;
#endif

    private void Awake()
    {
        this.transform = base.transform;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        charAlreadyTouch = new List<uint>(4);
        lastTimeBeginToFall = -10f;
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
        lastTimeLaunch = Time.time;
    }

    private Circle GetCharCollider()
    {
        float offsetAngle = Useful.AngleHori(Vector2.zero, charColliderOffset);
        float dirAngle = transform.rotation.z * Mathf.Deg2Rad;
        float totalAngle = Useful.WrapAngle(offsetAngle + dirAngle);
        Vector2 offset = Useful.Vector2FromAngle(totalAngle, charColliderOffset.magnitude);
        Vector2 center = (Vector2)transform.position + offset;
        return new Circle(center, charColliderRadius);
    }

    private void FixedUpdate()
    {
        if(PauseManager.instance.isPauseEnable)
        {
            lastTimeLaunch += Time.fixedDeltaTime;
            lastTimeBeginToFall = lastTimeBeginToFall >= 0f ? lastTimeBeginToFall + Time.fixedDeltaTime : lastTimeBeginToFall;
            return;
        }

        if(isFlying)
        {
            if(Time.time - lastTimeLaunch > maxDurationInAir)
            {
                DestroyProjectile();
                return;
            }
        }

        if(Time.time - lastTimeLaunch > minDurationBeforePick)
        {
            Collider2D[] cols = PhysicsToric.OverlapBoxAll((Vector2)transform.position + pickColliderOffset, pickColliderSize, transform.rotation.z * Mathf.Deg2Rad, charMask);
            foreach (Collider2D col in cols)
            {
                if (col.CompareTag("Char"))
                {
                    GameObject player = col.GetComponent<ToricObject>().original;
                    uint id = player.GetComponent<PlayerCommon>().id;
                    if (id == playerCommon.id)
                    {
                        Pick();
                        return;
                    }
                }
            }
        }

        if (isFlying || isFalling)
        {
            Circle charCol = GetCharCollider();
            Collider2D[] cols = PhysicsToric.OverlapCircleAll(charCol, charMask);

            foreach (Collider2D col in cols)
            {
                if (col.CompareTag("Char"))
                {
                    GameObject player = col.GetComponent<ToricObject>().original;
                    uint id = player.GetComponent<PlayerCommon>().id;
                    if (id != playerCommon.id && !charAlreadyTouch.Contains(id))
                    {
                        PlayerTouch(player);
                        charAlreadyTouch.Add(id);
                    }
                }
            }
        }

        if (isLanding)
        {
            if (rb.linearVelocity.y < -0.5f)
            {
                if(lastTimeBeginToFall < 0f)
                {
                    lastTimeBeginToFall = Time.time;
                }

                if(Time.time - lastTimeBeginToFall >= minDurationToFall)
                {
                    Fall();
                }
            }
            else
            {
                lastTimeBeginToFall = -10f;
            }
        }
    }

    //collision.otherCollider <=> this.collider, collision.collider <=> collider this gameobject collide with
    private void OnCollisionEnter2D(UnityEngine.Collision2D collision)
    {
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
        lastTimeBeginToFall = -10f;
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
        pickColliderSize = new Vector2(Mathf.Max(pickColliderSize.x, 0f), Mathf.Max(pickColliderSize.y, 0f));
        charColliderRadius = Mathf.Max(charColliderRadius, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        if(!drawGizmos)
            return;
        Hitbox h = new Hitbox((Vector2)transform.position + pickColliderOffset, pickColliderSize);
        h.Rotate(transform.rotation.z * Mathf.Deg2Rad);
        Hitbox.GizmosDraw(h, Color.red);
        Circle.GizmosDraw((Vector2)transform.position + charColliderOffset, charColliderRadius, Color.red);
    }

#endif

    #endregion
}
