using System;
using System.Collections;
using UnityEngine;

public class FallAttack : WeakAttack
{
    private BoxCollider2D hitbox;
    private CharacterController charController;
    private LayerMask charMask, groundMask;
    private new Transform transform;
    private bool isFalling;

#if UNITY_EDITOR

    [SerializeField] private bool drawGizmos = true;

#endif

    [SerializeField] private float castDuration = 1f;
    [SerializeField] private float explosionRadius = 1f;
    [SerializeField] private float minDistanceFromGround = 0.2f;
    [SerializeField] private float speedFall = 3f;
    [SerializeField] private float maxFallDuration = 3f;
    [SerializeField] private float upForceWhenCancelFalling = 10f;
    [SerializeField] private float explosionForce = 1.2f;
    [SerializeField] private GameObject floorShockWavePrefaps;
    [SerializeField] private float shockWaveHoriOffset = 0.2f;
    [SerializeField] private float shockWaveSpeed = 10f;

    #region Awake/Start

    protected override void Awake()
    {
        base.Awake();
        this.transform = base.transform;
        charController = GetComponent<CharacterController>();
        hitbox = GetComponent<BoxCollider2D>();
        isFalling = false;
    }

    protected override void Start()
    {
        base.Start();
        charMask = LayerMask.GetMask("Char");
        groundMask = LayerMask.GetMask("Floor", "WallProjectile");
    }

    #endregion

    protected override void Update()
    {
        base.Update();

        if(isFalling)
        {
            transform.position += speedFall * Time.deltaTime * Vector3.down;
        }
    }

    public override bool Launch(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
        if(!cooldown.isActive || charController.isGrounded)
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
        ToricRaycastHit2D raycast = PhysicsToric.Raycast(new Vector2(transform.position.x, transform.position.y + charController.groundRaycastOffset.y), Vector2.down, minDistanceFromGround, groundMask);
        return raycast.collider == null;
    }

    private IEnumerator ApplyFallAttack(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
        charController.Freeze();

        float timeCounter = 0f;
        while(timeCounter < castDuration)
        {
            yield return null;
            if(!PauseManager.instance.isPauseEnable)
            {
                timeCounter += Time.deltaTime;
            }
        }

        //phase tombante
        UnityEngine.Collider2D[] cols;
        bool hitGround = false;
        float timeFallCounter = 0f;
        bool fallStopByOvertime = false;
        isFalling = true;

        while(!hitGround)
        {
            while (PauseManager.instance.isPauseEnable)
            {
                yield return null;
            }

            //collision avec le sol
            hitGround = PhysicsToric.Raycast((Vector2)transform.position + charController.groundRaycastOffset, Vector2.down, charController.groundRaycastLength, groundMask);
            hitGround = hitGround || PhysicsToric.Raycast((Vector2)transform.position + new Vector2(-charController.groundRaycastOffset.x, charController.groundRaycastOffset.y), Vector2.down, charController.groundRaycastLength, groundMask);

            //Collision avec les autre personnages
            cols = PhysicsToric.OverlapBoxAll((Vector2)transform.position, hitbox.size, transform.rotation.eulerAngles.z, charMask);
            foreach (UnityEngine.Collider2D col in cols)
            {
                if(col.CompareTag("Char"))
                {
                    GameObject player = col.GetComponent<ToricObject>().original;
                    if(playerCommon.id != player.GetComponent<PlayerCommon>().id)
                    {
                        OnTouchEnemy(player, damageType);
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

        if (fallStopByOvertime)
        {
            charController.AddForce(Vector2.up * upForceWhenCancelFalling);
        }
        else
        {
            //EXPLOSION
            cols = PhysicsToric.OverlapCircleAll(new Vector2(transform.position.x, transform.position.y + charController.groundRaycastOffset.y), explosionRadius, charMask);

            foreach (UnityEngine.Collider2D col in cols)
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
            Vector2 shockWavePos = (Vector2)transform.position + Vector2.right * shockWaveHoriOffset;
            GameObject shockWaveGO = Instantiate(floorShockWavePrefaps, shockWavePos, Quaternion.identity, CloneParent.cloneParent);
            FloorShockWave shockWave = shockWaveGO.GetComponent<FloorShockWave>();
            shockWave.Launch(true, shockWaveSpeed, this);

            //Left
            shockWavePos = (Vector2)transform.position + Vector2.left * shockWaveHoriOffset;
            shockWaveGO = Instantiate(floorShockWavePrefaps, shockWavePos, Quaternion.identity, CloneParent.cloneParent);
            shockWave = shockWaveGO.GetComponent<FloorShockWave>();
            shockWave.Launch(false, shockWaveSpeed, this);
        }

        isFalling = false;
        charController.UnFreeze();
        callbackEnableOtherAttack.Invoke();
        callbackEnableThisAttack.Invoke();
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
        minDistanceFromGround = Mathf.Max(0f, minDistanceFromGround);
        explosionRadius = Mathf.Max(0f, explosionRadius);
        castDuration = Mathf.Max(0f, castDuration);
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos)
            return;

        charController = GetComponent<CharacterController>();

        Gizmos.color = Color.red;
        Gizmos.DrawLine((Vector2)transform.position + charController.groundRaycastOffset, (Vector2)transform.position + charController.groundRaycastOffset + Vector2.down * charController.groundRaycastLength);
        Gizmos.DrawLine((Vector2)transform.position + new Vector2(-charController.groundRaycastOffset.x, charController.groundRaycastOffset.y), (Vector2)transform.position + new Vector2(-charController.groundRaycastOffset.x, charController.groundRaycastOffset.y) + Vector2.down * charController.groundRaycastLength);
    }

#endif

    #endregion
}
