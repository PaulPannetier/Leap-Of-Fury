using UnityEngine;
using System.Collections.Generic;
using Collision2D;
using Collider2D = UnityEngine.Collider2D;

public class ConeProjectile : MonoBehaviour
{
    private new Transform transform;
    private Vector2 dir;
    private float speed;
    private LayerMask charMask, groundMask, wallProjectileMask;
    private bool isFlying;
    private ConeProjectileAttack attack;
    private Animator animator;
    private float lastTimeLauch;
    private List<uint> charAlreadyTouch;
    private PlayerCommon playerCommon;

    [SerializeField] private Vector2 collisionOffset;
    [SerializeField] private float collisionRadius;
    [SerializeField] private float maxDurationInAir;
    [SerializeField] private float minDurationBeforePick;

    private void Awake()
    {
        this.transform = base.transform;
        charAlreadyTouch = new List<uint>(4);
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        charMask = LayerMask.GetMask("Char");
        charMask = LayerMask.GetMask("Floor");
        wallProjectileMask = LayerMask.GetMask("WallProjectile");
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
        this.speed = speed;
        this.dir = dir;
        this.attack = attack;
        isFlying = true;
        playerCommon = attack.GetComponent<PlayerCommon>();

        float angleDeg = Useful.AngleHori(Vector2.zero, dir) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angleDeg);
        lastTimeLauch = Time.time;
    }

    private Circle GetCollider()
    {
        float offsetAngle = Useful.AngleHori(Vector2.zero, collisionOffset);
        float dirAngle = Useful.AngleHori(Vector2.zero, dir);
        float totalAngle = Useful.WrapAngle(offsetAngle + dirAngle);
        Vector2 offset = Useful.Vector2FromAngle(totalAngle, collisionOffset.magnitude);
        Vector2 center = (Vector2)transform.position + offset;
        return new Circle(center, collisionRadius);
    }

    private void Update()
    {
        if(PauseManager.instance.isPauseEnable)
        {
            lastTimeLauch += Time.deltaTime;
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

            transform.position += (Vector3)(dir * speed * Time.deltaTime);
        }

        Circle circleCollider = GetCollider();
        Collider2D[] cols = PhysicsToric.OverlapCircleAll(circleCollider, charMask);
        foreach (Collider2D col in cols)
        {
            if (col.CompareTag("Char"))
            {
                GameObject player = col.GetComponent<ToricObject>().original;
                uint playerId = player.GetComponent<PlayerCommon>().id;

                if (playerId == playerCommon.id)
                {
                    isLauncherCollide = Time.time - lastTimeLauch > minDurationBeforePick;
                }
                else if(isFlying)
                {
                    if (!charAlreadyTouch.Contains(playerId))
                    {
                        PlayerTouch(player);
                        charAlreadyTouch.Add(playerId);
                    }
                }
            }
        }

        if(isLauncherCollide)
        {
            Pick();
            return;
        }

        if(isFlying)
        {
            Collider2D col = PhysicsToric.OverlapCircle(circleCollider, groundMask);
            if(col != null)
            {
                Land();
                return;
            }
        }
    }

    private void Land()
    {
        isFlying = false;
        Circle circleCollider = GetCollider();
        Collider2D col = PhysicsToric.OverlapPoint(circleCollider.center, wallProjectileMask);
        if( col != null)
        {
            DestroyProjectile();
            return;
        }
        animator.CrossFade("land", 0);
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
        collisionRadius = Mathf.Max(collisionRadius, 0f);
        maxDurationInAir = Mathf.Max(maxDurationInAir, 0f);
        minDurationBeforePick = Mathf.Max(minDurationBeforePick, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        Circle.GizmosDraw((Vector2)transform.position + collisionOffset, collisionRadius, Color.green);
    }

#endif

    #endregion
}
