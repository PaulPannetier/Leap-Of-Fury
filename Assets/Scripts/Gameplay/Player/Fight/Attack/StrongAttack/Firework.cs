using System.Collections.Generic;
using UnityEngine;

public class Firework : MonoBehaviour
{
    private ToricObject toricObject; 
    private FireworkAttack fireworkAttack;
    private PlayerCommon playerCommon;
    private Capsule capsuleCollider;
    private float angle, speed;
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
    [SerializeField] CapsuleDirection2D capsuleDirection;
    [SerializeField] private float maxDuration = 5f;

    [Header("Explosion")]
    [SerializeField] private float explosionDuration = 1f;
    [SerializeField] private float explosionRadius = 1f;
    [SerializeField] private string explosionAnimName = "Explode";
    [SerializeField] private float explosioForce = 10f;

    [Header("Collision")]
    [SerializeField] private LayerMask charMask;
    [SerializeField] private LayerMask groundMask;

    private void Awake()
    {
        toricObject = GetComponent<ToricObject>();
        animator = GetComponent<Animator>();
        capsuleCollider = new Capsule((Vector2)transform.position + capsuleOffset, capsuleSize, capsuleDirection);
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

        dir = Useful.Vector2FromAngle(angle);
        speed = maxSpeed * speedCurve.Evaluate(0f);
        timeWhenIsLaunch = Time.time;
    }

    private void Update()
    {
        if (isExploding)
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
            if (Time.time - timeWhenIsLaunch < accelerationDuration)
            {
                speed = (maxSpeed * speedCurve.Evaluate((Time.time - timeWhenIsLaunch) / accelerationDuration));
            }
            else
            {
                speed = maxSpeed * speedCurve.Evaluate(1);
            }

            transform.Translate(dir * (speed * Time.deltaTime), Space.World);
            capsuleCollider = new Capsule((Vector2)transform.position + capsuleOffset, capsuleSize, capsuleDirection);
            capsuleCollider.Rotate(transform.rotation.eulerAngles.z * Mathf.Deg2Rad);

            Collider2D[] cols = PhysicsToric.OverlapCapsuleAll(capsuleCollider, charMask);
            foreach (Collider2D col in cols)
            {
                TouchChar(col);
            }

            Collider2D colGround = PhysicsToric.OverlapCapsule(capsuleCollider, groundMask);
            if (colGround != null)
            {
                StartExplode();
            }

            if (toricObject.isAClone)
                return;

            if (Time.time - timeWhenIsLaunch > maxDuration)
            {
                StartExplode();
                print("explode max duration reach");
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
            {
                StartExplode();
                print("explode touch a char");
            }
        }
    }

    private void StartExplode()
    {
        if(toricObject.isAClone)
        {
            toricObject.original.GetComponent<Firework>().StartExplode();
            return;
        }
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
        if(Application.isPlaying)
        {
            capsuleCollider = new Capsule((Vector2)transform.position + capsuleOffset, capsuleSize, capsuleDirection);
            capsuleCollider.Rotate(angle);
        }
        else
        {
            capsuleCollider = new Capsule((Vector2)transform.position + capsuleOffset, capsuleSize, capsuleDirection);
            capsuleCollider.Rotate(transform.rotation.eulerAngles.z * Mathf.Deg2Rad);
        }

        Capsule.GizmosDraw(capsuleCollider);
        Circle.GizmosDraw(transform.position, explosionRadius);
    }
}
