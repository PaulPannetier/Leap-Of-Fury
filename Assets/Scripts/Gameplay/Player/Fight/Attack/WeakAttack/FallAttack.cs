using System;
using System.Collections;
using UnityEngine;

public class FallAttack : WeakAttack
{
    private Rigidbody2D rb;
    private BoxCollider2D hitbox;
    private Movement movement;

    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private float explosionRadius = 1f;
    [SerializeField] private float minDistanceFromGround = 0.2f;
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float speedFall = 3f;
    [SerializeField] private GameObject floorShockWavePrefaps;
    [SerializeField] private float shockWaveHoriOffset = 0.2f;
    [SerializeField] private float shockWaveSpeed = 10f;

    protected override void Awake()
    {
        base.Awake();
        movement = GetComponent<Movement>();
        rb = GetComponent<Rigidbody2D>();
        hitbox = GetComponent<BoxCollider2D>();
    }

    public override bool Launch(Action callbackEnd)
    {
        if(!cooldown.isActive || movement.isGrounded)
        {
            callbackEnd.Invoke();
            return false;
        }
        base.Launch(callbackEnd);

        if(!IsEnoughtHight())
        {
            callbackEnd.Invoke();
            return false;
        }

        cooldown.Reset();
        StartCoroutine(ApplyFallAttack(callbackEnd));
        return true;
    }

    private bool IsEnoughtHight()
    {
        RaycastHit2D raycast = PhysicsToric.Raycast((Vector2)transform.position + movement.groundOffset, Vector2.down, minDistanceFromGround, groundMask);
        return raycast.collider == null;
    }

    private IEnumerator ApplyFallAttack(Action callbackEnd)
    {
        movement.Freeze();

        yield return Useful.GetWaitForSeconds(castDuration);

        //phase tombante
        Collider2D[] cols;
        bool hitGround = false;
        Vector2 fallSpeed = Vector2.down * speedFall;
        while(!hitGround)
        {
            //collision avec le sol
            rb.velocity = fallSpeed;
            hitGround = Physics2D.OverlapCircle((Vector2)transform.position + movement.groundOffset, movement.groundCollisionRadius, groundMask) != null;
            //Collision avec les autre personnages
            cols = Physics2D.OverlapBoxAll((Vector2)transform.position, hitbox.size, transform.rotation.eulerAngles.z, playerMask);
            foreach (Collider2D col in cols)
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
            yield return null;
        }

        //EXPLOSION
        cols = Physics2D.OverlapCircleAll((Vector2)transform.position + movement.groundOffset, explosionRadius, playerMask);

        foreach (Collider2D col in cols)
        {
            if(col.gameObject.CompareTag("Char"))
            {
                GameObject player = col.GetComponent<ToricObject>().original;
                if (playerCommon.id != player.GetComponent<PlayerCommon>().id)
                {
                    OnTouchEnemy(player);
                }
            }
        }

        movement.UnFreeze();
        callbackEnd.Invoke();

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

    public void OnTouchEnemyByShockWave(GameObject enemy, FloorShockWave shockWave)
    {
        float tmp = attackForce;
        attackForce = shockWave.attackForce;
        OnTouchEnemy(enemy);
        attackForce = tmp;
    }

    private void OnValidate()
    {
        minDistanceFromGround = Mathf.Max(0f, minDistanceFromGround);
        explosionRadius = Mathf.Max(0f, explosionRadius);
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos)
            return;

        if(movement == null)
            movement = GetComponent<Movement>();

        Gizmos.color = Color.black;
        Circle.GizmosDraw((Vector2)transform.position + movement.groundOffset, explosionRadius);
        Gizmos.color = Color.red;
        Circle.GizmosDraw((Vector2)transform.position + movement.groundOffset, movement.groundCollisionRadius);
        Gizmos.color = Color.black;
        Gizmos.DrawLine((Vector2)transform.position + movement.groundOffset, (Vector2)transform.position + movement.groundOffset + Vector2.down * minDistanceFromGround);
    }
}
