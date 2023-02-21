using System.Collections.Generic;
using UnityEngine;

public class Firework : MonoBehaviour
{
    private ToricObject toricObject; 
    private FireworkAttack fireworkAttack;
    private PlayerCommon playerCommon;
    private Rigidbody2D rb;
    private Capsule capsuleCollider;
    private float angle;
    private Vector2 dir;
    private float timeWhenIsLaunch;
    private List<uint> charAlreadyTouch = new List<uint>();
    private Animator animator;
    private bool isExploding = false;
    private float explosionAnimationLength;

    [Header("first phase")]
    [SerializeField] private float maxSpeed = 2f;
    [SerializeField] private float accelerationDuration = 1f;
    [SerializeField] private AnimationCurve speedCurve;
    [SerializeField] Vector2 capsuleOffset;
    [SerializeField] Vector2 capsuleSize;
    [SerializeField] private float maxDuration = 5f;

    [Header("Explosion")]
    [SerializeField] private float explosionDuration = 1f;
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private string explosionAnimName = "Explode";
    [SerializeField] private float explosioForce = 10f;

    [Header("Collision")]
    [SerializeField] private LayerMask charMask;
    [SerializeField] private LayerMask groundMask;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        toricObject = GetComponent<ToricObject>();
        animator = GetComponent<Animator>();
        capsuleCollider = new Capsule((Vector2)transform.position + capsuleOffset, capsuleSize);
    }

    private void Start()
    {
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        for(int i = 0; i < clips.Length; i++)
        {
            if (clips[i].name == explosionAnimName)
            {
                explosionAnimationLength = clips[i].length;
                break;
            }
        }
    }

    public void Launch(float angle, PlayerCommon playerCommon, FireworkAttack fireworkAttack)
    {
        this.fireworkAttack = fireworkAttack;
        this.playerCommon = playerCommon;
        this.angle = angle;

        capsuleCollider = new Capsule((Vector2)transform.position + capsuleOffset, capsuleSize);
        capsuleCollider.Rotate(angle);
        dir = Useful.Vector2FromAngle(angle);
        rb.velocity = dir * (maxSpeed * speedCurve.Evaluate(0f));
        timeWhenIsLaunch = Time.time;
    }

    private void Update()
    {
        if(isExploding)
        {
            if(Time.time - timeWhenIsLaunch <= explosionDuration)
            {
                Collider2D[] cols = PhysicsToric.OverlapCircleAll(transform.position, explosionRadius, charMask);
                foreach (Collider2D col in cols)
                {
                    TouchChar(col);
                }
            }
        }
        else
        {
            capsuleCollider = new Capsule((Vector2)transform.position + capsuleOffset, capsuleSize);
            capsuleCollider.Rotate(angle);
            Collider2D[] cols = PhysicsToric.OverlapCapsuleAll(capsuleCollider, 0f, charMask);
            foreach (Collider2D col in cols)
            {
                TouchChar(col);
            }

            Collider2D colGround = PhysicsToric.OverlapCapsule(capsuleCollider, 0f, groundMask);
            if (colGround != null)
            {
                StartExplode();
            }

            if (toricObject.isAClone)
                return;

            if (Time.time - timeWhenIsLaunch > maxDuration)
            {
                StartExplode();
            }
        }
    }

    private void TouchChar(Collider2D col)
    {
        if(toricObject.isAClone)
        {
            toricObject.original.GetComponent<Firework>().TouchChar(col);
            return;
        }

        GameObject player = col.GetComponent<ToricObject>().original;
        uint id = player.GetComponent<PlayerCommon>().id;

        if (id != playerCommon.id && !charAlreadyTouch.Contains(id))
        {
            charAlreadyTouch.Add(id);
            fireworkAttack.OnFireworkTouchEnnemy(this, player);
            if(!isExploding)
                StartExplode();
        }
    }

    private void FixedUpdate()
    {
        if(!isExploding)
        {
            if (Time.time - timeWhenIsLaunch < accelerationDuration)
            {
                rb.velocity = dir * (maxSpeed * speedCurve.Evaluate((Time.time - timeWhenIsLaunch) / accelerationDuration));
            }
            else
            {
                rb.velocity = dir * maxSpeed;
            }
        }
    }

    private void StartExplode()
    {
        isExploding = true;
        animator.SetTrigger("Explode");
        timeWhenIsLaunch = Time.time;
        ExplosionManager.instance.CreateExplosion(transform.position, explosioForce);
        Invoke(nameof(Destroy), Mathf.Max(explosionAnimationLength * 1.1f, explosionDuration));
    }

    private void Destroy()
    {
        if (toricObject.isAClone)
        {
            toricObject.original.GetComponent<Firework>().Destroy();
            return;
        }

        toricObject.RemoveClones();
        Destroy(gameObject);
    }

    private void OnValidate()
    {
        maxSpeed = Mathf.Max(maxSpeed, 0f);
        accelerationDuration = Mathf.Max(accelerationDuration, 0f);
        explosionDuration = Mathf.Max(explosionDuration, 0f);
        explosionRadius = Mathf.Max(explosionRadius, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        capsuleCollider = new Capsule((Vector2)transform.position + capsuleOffset, capsuleSize);
        capsuleCollider.Rotate(angle);
        Capsule.GizmosDraw(capsuleCollider);
        Circle.GizmosDraw(transform.position, explosionRadius);
    }
}
