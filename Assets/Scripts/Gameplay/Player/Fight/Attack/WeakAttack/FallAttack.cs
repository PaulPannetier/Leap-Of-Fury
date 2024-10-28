using Collision2D;
using System;
using UnityEngine;
using Collider2D = UnityEngine.Collider2D;

public class FallAttack : WeakAttack
{
    private enum FallAttackPhase
    {
        None,
        Freeze,//Phase 1
        Fall,//Phase 2
    }

    private BoxCollider2D hitbox;
    private CharacterController charController;
    private ToricObject toricObject;
    private Rigidbody2D rb;
    private LayerMask charMask, groundMask;
    private new Transform transform;
    private FallAttackPhase state;
    private float LastTimeBeginAPhase = -10f;
    private Action callbackEnableOtherAttack, callbackEnableThisAttack;

#if UNITY_EDITOR
    [SerializeField] private bool drawGizmos = true;
#endif

    [SerializeField] private float castDuration = 1f;
    [SerializeField] private float explosionRadius = 1f;
    [SerializeField] private Vector2 groundDetectionHitboxOffset = Vector2.zero;
    [SerializeField] private float groundDetectionHeight = 0.3f;
    [SerializeField] private float fallSpeed = 3f;
    [SerializeField] private float maxFallDuration = 3f;
    [SerializeField] private float upForceWhenCancelFalling = 10f;
    [SerializeField] private float explosionForce = 1.2f;
    [SerializeField] private GameObject floorShockWavePrefaps;
    [SerializeField] private Vector2 floorShockWaveRaycastOffset;
    [SerializeField] private float floorShockWaveRaycastLenght;
    [SerializeField] private float shockWaveHoriOffset = 0.2f;
    [SerializeField] private float shockWaveSpeed = 10f;

    #region Awake/Start

    protected override void Awake()
    {
        base.Awake();
        this.transform = base.transform;
        charController = GetComponent<CharacterController>();
        hitbox = GetComponent<BoxCollider2D>();
        toricObject = GetComponent<ToricObject>();
        rb = GetComponent<Rigidbody2D>();
        state = FallAttackPhase.None;
    }

    protected override void Start()
    {
        base.Start();
        charMask = LayerMask.GetMask("Char");
        groundMask = LayerMask.GetMask("Floor", "WallProjectile");
    }

    #endregion

    private void LateUpdate()
    {
        if (state == FallAttackPhase.None)
            return;

        if(PauseManager.instance.isPauseEnable)
        {
            LastTimeBeginAPhase += Time.deltaTime;
            return;
        }

        if (state == FallAttackPhase.Freeze)
            HandleFreeze();
        else
            HandleFall();

        toricObject.CustomUpdate();
    }

    private void HandleFreeze()
    {
        charController.Freeze();
        if(Time.time - LastTimeBeginAPhase > castDuration)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            LastTimeBeginAPhase = Time.time;
            state = FallAttackPhase.Fall;
        }
    }

    private void HandleFall()
    {
        Vector2 size = new Vector2(hitbox.size.x, groundDetectionHeight);
        bool hitGround = PhysicsToric.OverlapBox((Vector2)transform.position + groundDetectionHitboxOffset, size, 0f, groundMask) != null;

        //Collision avec les autre personnages
        Collider2D[] cols = PhysicsToric.OverlapBoxAll((Vector2)transform.position + hitbox.offset, hitbox.size, 0f, charMask);
        foreach (Collider2D col in cols)
        {
            if (col.CompareTag("Char"))
            {
                GameObject player = col.GetComponent<ToricObject>().original;
                if (playerCommon.id != player.GetComponent<PlayerCommon>().id)
                {
                    OnTouchEnemy(player, damageType);
                }
            }
        }

        if (hitGround || Time.time - LastTimeBeginAPhase > maxFallDuration)
        {
            state = FallAttackPhase.None;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            charController.UnFreeze();
            Vector2 newVel = hitGround ? fallSpeed * Vector2.down : upForceWhenCancelFalling * Vector2.up;
            charController.ForceApplyVelocity(newVel);
        }

        if(hitGround)
        {
            cols = PhysicsToric.OverlapCircleAll(new Vector2(transform.position.x, transform.position.y + charController.groundRaycastOffset.y), explosionRadius, charMask);
            foreach (Collider2D col in cols)
            {
                if (col.gameObject.CompareTag("Char"))
                {
                    GameObject player = col.GetComponent<ToricObject>().original;
                    if (playerCommon.id != player.GetComponent<PlayerCommon>().id)
                    {
                        OnTouchEnemy(player, damageType);
                    }
                }
            }

            ExplosionManager.instance.CreateExplosion(transform.position, explosionForce);

            //Instantiate wave attack
            //right
            ToricRaycastHit2D ray = PhysicsToric.Raycast((Vector2)transform.position + floorShockWaveRaycastOffset, Vector2.down, floorShockWaveRaycastLenght, groundMask);
            if(ray.collider != null)
            {
                //right
                Vector2 shockWavePos = (Vector2)transform.position + Vector2.right * shockWaveHoriOffset;
                GameObject shockWaveGO = Instantiate(floorShockWavePrefaps, shockWavePos, Quaternion.identity, CloneParent.cloneParent);
                FloorShockWave shockWave = shockWaveGO.GetComponent<FloorShockWave>();
                shockWave.Launch(true, shockWaveSpeed, this);
            }

            //Left
            Vector2 start = new Vector2(transform.position.x - floorShockWaveRaycastOffset.x, transform.position.y  + floorShockWaveRaycastOffset.y);
            ray = PhysicsToric.Raycast(start, Vector2.down, floorShockWaveRaycastLenght, groundMask);
            if(ray.collider != null)
            {
                Vector2 shockWavePos = (Vector2)transform.position + Vector2.left * shockWaveHoriOffset;
                GameObject shockWaveGO = Instantiate(floorShockWavePrefaps, shockWavePos, Quaternion.identity, CloneParent.cloneParent);
                FloorShockWave shockWave = shockWaveGO.GetComponent<FloorShockWave>();
                shockWave.Launch(false, shockWaveSpeed, this);
            }

            callbackEnableOtherAttack.Invoke();
            callbackEnableThisAttack.Invoke();
        }
        else
        {
            rb.MovePosition((Vector2)transform.position + Time.deltaTime * fallSpeed * Vector2.down);
        }
    }

    public override bool Launch(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
        bool IsEnoughtHight()
        {
            Vector2 size = new Vector2(hitbox.size.x, groundDetectionHeight);
            Collider2D collider = PhysicsToric.OverlapBox((Vector2)transform.position + groundDetectionHitboxOffset, size, 0f, groundMask);
            return collider == null;
        }

        if (!cooldown.isActive || charController.isGrounded)
        {
            callbackEnableOtherAttack.Invoke();
            callbackEnableThisAttack.Invoke();
            return false;
        }
        base.Launch(callbackEnableOtherAttack, callbackEnableThisAttack);

        if(!IsEnoughtHight())
        {
            callbackEnableOtherAttack.Invoke();
            callbackEnableThisAttack.Invoke();
            return false;
        }

        cooldown.Reset();
        this.callbackEnableOtherAttack = callbackEnableOtherAttack;
        this.callbackEnableThisAttack = callbackEnableThisAttack;
        state = FallAttackPhase.Freeze;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        LastTimeBeginAPhase = Time.time;
        return true;
    }

    public void OnTouchEnemyByShockWave(GameObject enemy, FloorShockWave shockWave)
    {
        OnTouchEnemy(enemy, damageType);
    }

    #region Gizmos/OnValidate

#if UNITY_EDITOR

    protected override void OnValidate()
    {
        base.OnValidate();
        this.transform = base.transform;
        fallSpeed = Mathf.Max(0f, fallSpeed);
        groundDetectionHeight = Mathf.Max(0f, groundDetectionHeight);
        explosionRadius = Mathf.Max(0f, explosionRadius);
        explosionForce = Mathf.Max(explosionForce, 0f);
        castDuration = Mathf.Max(0f, castDuration);
        floorShockWaveRaycastLenght = Mathf.Max(0f, floorShockWaveRaycastLenght);
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos)
            return;

        this.transform = base.transform;
        hitbox = GetComponent<BoxCollider2D>();
        charController = GetComponent<CharacterController>();

        Vector2 size = new Vector2(hitbox.size.x, groundDetectionHeight);
        Hitbox.GizmosDraw((Vector2)transform.position + groundDetectionHitboxOffset, size, Color.red);
        Circle.GizmosDraw(new Vector2(transform.position.x, transform.position.y + charController.groundRaycastOffset.y), explosionRadius, Color.black, true);

        Gizmos.color = Color.red;
        Gizmos.DrawLine((Vector2)transform.position + floorShockWaveRaycastOffset, (Vector2)transform.position + floorShockWaveRaycastOffset + floorShockWaveRaycastLenght * Vector2.down);
        Vector2 start = new Vector2(transform.position.x - floorShockWaveRaycastOffset.x, transform.position.y  + floorShockWaveRaycastOffset.y);
        Gizmos.DrawLine(start, start + floorShockWaveRaycastLenght * Vector2.down);

    }

#endif

    #endregion
}
