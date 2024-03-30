using System;
using System.Collections;
using UnityEngine;
using Collision2D;

public class FallAttack : WeakAttack
{
    private Rigidbody2D rb;
    private BoxCollider2D hitbox;
    private CharacterController movement;
    private LayerMask charMask, groundMask;

#if UNITY_EDITOR

    [SerializeField] private bool drawGizmos = true;

#endif

    [SerializeField] private float explosionRadius = 1f;
    [SerializeField] private float minDistanceFromGround = 0.2f;
    [SerializeField] private float speedFall = 3f;
    [SerializeField] private float maxFallDuration = 3f;
    [SerializeField] private float upForceWhenCancelFalling = 10f;
    [SerializeField] private float explosionForce = 1.2f;
    [SerializeField] private GameObject floorShockWavePrefaps;
    [SerializeField] private float shockWaveHoriOffset = 0.2f;
    [SerializeField] private float shockWaveSpeed = 10f;

    protected override void Awake()
    {
        base.Awake();
        movement = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody2D>();
        hitbox = GetComponent<BoxCollider2D>();
    }

    protected override void Start()
    {
        base.Start();
        charMask = LayerMask.GetMask("Char");
        groundMask = LayerMask.GetMask("Floor", "WallProjectile");
    }

    public override bool Launch(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
        if(!cooldown.isActive || movement.isGrounded)
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
        StartCoroutine(ApplyFallAttack(callbackEnableOtherAttack, callbackEnableThisAttack));
        return true;
    }

    private bool IsEnoughtHight()
    {
        RaycastHit2D raycast = PhysicsToric.Raycast((Vector2)transform.position + movement.groundOffset, Vector2.down, minDistanceFromGround, groundMask);
        return raycast.collider == null;
    }

    private IEnumerator ApplyFallAttack(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
        movement.Freeze();

        float timeCounter = 0f;
        while(timeCounter < castDuration)
        {
            yield return null;
            if(!PauseManager.instance.isPauseEnable)
            {
                timeCounter += Time.deltaTime;
            }
        }

        movement.UnFreeze();
        movement.enableBehaviour = false;

        //phase tombante
        UnityEngine.Collider2D[] cols;
        bool hitGround = false;
        Vector2 fallSpeed = Vector2.down * speedFall;
        float timeFallCounter = 0f;
        bool fallStopByOvertime = false;

        while(!hitGround)
        {
            if(PauseManager.instance.isPauseEnable)
            {
                movement.Freeze();

                while(PauseManager.instance.isPauseEnable)
                {
                    yield return null;
                }
                movement.UnFreeze();
            }

            //collision avec le sol
            rb.velocity = fallSpeed;
            hitGround = Physics2D.OverlapCircle((Vector2)transform.position + movement.groundOffset, movement.groundCollisionRadius, groundMask) != null;
            //Collision avec les autre personnages
            cols = Physics2D.OverlapBoxAll((Vector2)transform.position, hitbox.size, transform.rotation.eulerAngles.z, charMask);
            foreach (UnityEngine.Collider2D col in cols)
            {
                if(col.CompareTag("Char"))
                {
                    GameObject player = col.GetComponent<ToricObject>().original;
                    if(playerCommon.id != player.GetComponent<PlayerCommon>().id)
                    {
                        OnTouchEnemy(player);
                    }
                }
            }

            if(timeFallCounter > maxFallDuration)
            {
                fallStopByOvertime = true;
                break;
            }
            timeFallCounter += Time.deltaTime;

            yield return null;
        }

        //EXPLOSION
        cols = Physics2D.OverlapCircleAll((Vector2)transform.position + movement.groundOffset, explosionRadius, charMask);

        foreach (UnityEngine.Collider2D col in cols)
        {
            if (col.gameObject.CompareTag("Char"))
            {
                GameObject player = col.GetComponent<ToricObject>().original;
                if (playerCommon.id != player.GetComponent<PlayerCommon>().id)
                {
                    OnTouchEnemy(player);
                }
            }
        }

        ExplosionManager.instance.CreateExplosion(transform.position, explosionForce);

        if (fallStopByOvertime)
        {
            rb.velocity = Vector2.zero;
            movement.AddForce(Vector2.up * upForceWhenCancelFalling);
        }
        else
        {
            //Instantiate wave attack
            //right
            Vector2 shockWavePos = (Vector2)transform.position + Vector2.right * shockWaveHoriOffset;
            GameObject shockWaveGO = Instantiate(floorShockWavePrefaps, shockWavePos, Quaternion.identity, CloneParent.cloneParent);
            FloorShockWave shockWave = shockWaveGO.GetComponent<FloorShockWave>();
            shockWave.Launch(transform.position, true, shockWaveSpeed, this);

            //Left
            shockWavePos = (Vector2)transform.position + Vector2.left * shockWaveHoriOffset;
            shockWaveGO = Instantiate(floorShockWavePrefaps, shockWavePos, Quaternion.identity, CloneParent.cloneParent);
            shockWave = shockWaveGO.GetComponent<FloorShockWave>();
            shockWave.Launch(transform.position, false, shockWaveSpeed, this);
        }
        movement.UnFreeze();
        callbackEnableOtherAttack.Invoke();
        callbackEnableThisAttack.Invoke();
    }

    public void OnTouchEnemyByShockWave(GameObject enemy, FloorShockWave shockWave)
    {
        OnTouchEnemy(enemy);
    }

    #region Gizmos/OnValidate

#if UNITY_EDITOR

    protected override void OnValidate()
    {
        base.OnValidate();
        minDistanceFromGround = Mathf.Max(0f, minDistanceFromGround);
        explosionRadius = Mathf.Max(0f, explosionRadius);
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos)
            return;

        if(movement == null)
            movement = GetComponent<CharacterController>();

        Gizmos.color = Color.black;
        Circle.GizmosDraw((Vector2)transform.position + movement.groundOffset, explosionRadius);
        Gizmos.color = Color.red;
        Circle.GizmosDraw((Vector2)transform.position + movement.groundOffset, movement.groundCollisionRadius);
        Gizmos.color = Color.black;
        Gizmos.DrawLine((Vector2)transform.position + movement.groundOffset, (Vector2)transform.position + movement.groundOffset + Vector2.down * minDistanceFromGround);
    }

#endif

    #endregion
}
