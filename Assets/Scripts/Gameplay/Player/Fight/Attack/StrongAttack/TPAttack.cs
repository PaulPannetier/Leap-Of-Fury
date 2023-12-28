using System;
using UnityEngine;
using Collision2D;

public class TPAttack : StrongAttack
{
    private Movement playerMovement;
    private LayerMask groundMask, charMask;
    private Explosion explosion;

    [SerializeField] private float tpRange = 1f;
    [SerializeField] private float explosionRadius = 1f;
    [SerializeField] private float detectionStep = 0.05f;
    [SerializeField] private Vector2 collisionOffset = Vector2.zero;
    [SerializeField] private Vector2 collisionSize = new Vector2(0.5f, 1f);
    [SerializeField] private float explosionForce;
    [SerializeField] private float explosionDuration;
    [SerializeField] private Explosion explosionPrefabs;

    protected override void Awake()
    {
        base.Awake();
        playerMovement = GetComponent<Movement>();
    }

    protected override void Start()
    {
        base.Start();
        groundMask = LayerMask.GetMask("Floor", "WallProjectile");
        charMask = LayerMask.GetMask("Char");
        PauseManager.instance.callBackOnPauseDisable += OnPauseDisable;
        PauseManager.instance.callBackOnPauseEnable += OnPauseEnable;
    }

    public override bool Launch(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
        if(!cooldown.isActive)
        {
            callbackEnableOtherAttack.Invoke();
            callbackEnableThisAttack.Invoke();
            return false;
        }

        base.Launch(callbackEnableOtherAttack, callbackEnableThisAttack);
        Teleport(callbackEnableOtherAttack, callbackEnableThisAttack);
        cooldown.Reset();
        return true;
    }

    private void Teleport(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
        Vector2 dir = playerMovement.GetCurrentDirection();
        Vector2 newPos = PhysicsToric.GetPointInsideBounds((Vector2)transform.position + dir * tpRange);

        UnityEngine.Collider2D groundCollider = PhysicsToric.OverlapBox(newPos, collisionSize, 0f, groundMask);
        if(groundCollider == null)
        {
            //tout est ok
            playerMovement.Teleport(newPos);
        }
        else
        {
            //on se bouffe un mur
            RaycastHit2D raycasts1 = PhysicsToric.Raycast(transform.position, dir, 2f * tpRange, groundMask);
            RaycastHit2D raycasts2;
            Hitbox mapHitbox = new Hitbox(Vector2.zero, LevelMapData.currentMap.mapSize);
            if (Collision2D.Collider2D.CollideHitboxLine(mapHitbox, transform.position, (Vector2)transform.position + (2f * tpRange) * dir, out Vector2 colP))
            {
                Vector2 step = new Vector2(colP.x - mapHitbox.center.x > 0f ? 0.01f : -0.01f, colP.y - mapHitbox.center.y > 0f ? 0.01f : -0.01f);
                while (mapHitbox.Contains(colP))
                {
                    colP += step;
                }
                float dist = colP.Distance(transform.position);
                colP = PhysicsToric.GetPointInsideBounds(colP) + Mathf.Abs(2f * tpRange - dist) * dir;
                raycasts2 = PhysicsToric.Raycast(colP, -dir, Mathf.Abs(2f * tpRange - dist), groundMask);
            }
            else
            {
                raycasts2 = PhysicsToric.Raycast((Vector2)transform.position + (2f * tpRange) * dir, -dir, 2f * tpRange, groundMask);
            }

            RaycastHit2D[] raycasts = new RaycastHit2D[2] { raycasts1, raycasts2 };

            if(raycasts[0].collider == null && raycasts[1].collider == null)
            {
                print("debug pls");
                callbackEnableOtherAttack.Invoke();
                callbackEnableThisAttack.Invoke();
                return;
            }
            int minIndex = 0;
            float minSqrDist = newPos.SqrDistance(raycasts[0].point);
            float d;

            for (int i = 1; i < raycasts.Length; i++)
            {
                d = newPos.SqrDistance(raycasts[i].point);
                if(d < minSqrDist)
                {
                    minIndex = i;
                    minSqrDist = d;
                }
            }

            if (minIndex.IsEven())
            {
                //nouveau point vers le joueur
                do
                {
                    newPos -= dir * detectionStep;
                    groundCollider = Physics2D.OverlapBox(newPos, collisionSize, 0f, groundMask);

                } while (groundCollider != null);
            }
            else
            {
                //nouveau point vers l'extérieur du joueur
                do
                {
                    newPos += dir * detectionStep;
                    groundCollider = Physics2D.OverlapBox(newPos, collisionSize, 0f, groundMask);

                } while (groundCollider != null);
            }
            playerMovement.Teleport(newPos);
        }

        ApplyDamage();
        ExplosionManager.instance.CreateExplosion(newPos, explosionForce);
        callbackEnableOtherAttack.Invoke();
        callbackEnableThisAttack.Invoke();
    }

    private void ApplyDamage()
    {
        explosion = Instantiate(explosionPrefabs, transform.position, Quaternion.Euler(0f, 0f, Random.RandExclude(0f, 360f)), CloneParent.cloneParent);

        Explosion.ExplosionData explosionData = new Explosion.ExplosionData(Vector2.zero, explosionForce, explosionRadius, 0f, explosionDuration, charMask);
        explosion.Launch(explosionData);
        explosion.callbackOnTouch += OnExplosionTouch;
        explosion.callbackOnDestroy += OnExplosionEnd;

        explosion.transform.localScale = new Vector3(explosionRadius, explosionRadius, 1f);
    }

    private void OnExplosionTouch(UnityEngine.Collider2D collider)
    {
        if (collider.CompareTag("Char"))
        {
            GameObject player = collider.GetComponent<ToricObject>().original;
            if (playerCommon.id != player.GetComponent<PlayerCommon>().id)
            {
                OnTouchEnemy(player);
            }
        }
    }

    private void OnExplosionEnd(Explosion explosion)
    {
        if(this.explosion == explosion)
        {
            this.explosion = null;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        PauseManager.instance.callBackOnPauseDisable -= OnPauseDisable;
        PauseManager.instance.callBackOnPauseEnable -= OnPauseEnable;
    }

    #region Gizmos/OnValidate/Pause

    private void OnPauseEnable()
    {
        if(explosion != null)
        {
            Animator explosionAnimator = explosion.GetComponentInChildren<Animator>();
            if (explosionAnimator != null)
            {
                explosionAnimator.speed = 0f;
            }
        }
    }

    private void OnPauseDisable()
    {
        if (explosion != null)
        {
            Animator explosionAnimator = explosion.GetComponentInChildren<Animator>();
            if (explosionAnimator != null)
            {
                explosionAnimator.speed = 1f;
            }
        }
    }

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Circle.GizmosDraw(transform.position, tpRange);
        Gizmos.DrawWireCube((Vector2)transform.position + collisionOffset, collisionSize);
        Circle.GizmosDraw((Vector2)transform.position + Vector2.up * tpRange, explosionRadius);
    }

    private void OnValidate()
    {
        detectionStep = Mathf.Max(0.000001f, detectionStep);
        collisionSize = new Vector2(Mathf.Max(0.0000001f, collisionSize.x), Mathf.Max(0.0000001f, collisionSize.y));
        explosionRadius = Mathf.Max(0f, explosionRadius);
        tpRange = Mathf.Max(0f, tpRange);
        explosionForce = Mathf.Max(explosionForce, 0f);
        explosionDuration = Mathf.Max(0f, explosionDuration);
    }

#endif

    #endregion
}
