using UnityEngine;
using Collision2D;
using Collider2D = UnityEngine.Collider2D;

public class Boomerang : MonoBehaviour
{
    private enum State
    {
        go,
        getBack
    }

    private new Transform transform;
    private ToricObject toricObject;
    private Animator animator;
    private BoomerangAttack sender;
    private Vector2 dir;
    private AnimationCurve speedCurvePhase1, accelerationCurvePhase2;
    private float maxSpeedPhase1, durationPhase1, accelerationDurationPhase2;
    private float maxSpeedPhase2, recuperationRange;
    private float rotationSpeed;
    private State state;
    private float timeLaunch = -10f;
    private Vector2 velocity;
    private LayerMask groundMask;
    private bool isDestroy;

    [SerializeField] private Vector2 circleOffset;
    [SerializeField] private float circleRadius;

    private void Awake()
    {
        this.transform = base.transform;
        toricObject = GetComponent<ToricObject>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        groundMask = LayerMask.GetMask("Floor");
    }

    public void Launch(in BoomerangLaunchData boomerangLauchData)
    {
        Builder(boomerangLauchData);

        state = State.go;
        velocity = maxSpeedPhase1 * speedCurvePhase1.Evaluate(0f) * dir;
        timeLaunch = Time.time;

        void Builder(in BoomerangLaunchData data)
        {
            dir = data.dir; 
            speedCurvePhase1 = data.speedCurvePhase1;
            maxSpeedPhase1 = data.maxSpeedPhase1;
            durationPhase1 = data.durationPhase1;
            sender = data.sender;
            maxSpeedPhase2 = data.maxSpeedPhase2;
            accelerationCurvePhase2 = data.accelerationCurvePhase2;
            maxSpeedPhase2 = data.maxSpeedPhase2;
            recuperationRange = data.recuperationRange;
            rotationSpeed = data.rotationSpeed;
        }
    }

    private void Update()
    {
        if (isDestroy)
            return;

        switch (state)
        {
            case State.go:
                HandleGoState();
                break;
            case State.getBack:
                HandleGetBackState();
                break;
            default:
                break;
        }

        Vector2 pos = transform.position + (Vector3)(velocity * Time.deltaTime);
        transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, 0f, (velocity.Angle(Vector2.right) + Mathf.PI) * Mathf.Rad2Deg));
    }

    private void HandleGoState()
    {
        Circle circleCollider = GetCircleCollider();
        Collider2D groundCol = PhysicsToric.OverlapCircle(circleCollider, groundMask);

        if(groundCol != null)
        {
            transform.position -= (Vector3)(velocity * Time.deltaTime);
        }

        if (groundCol != null || Time.time - timeLaunch > durationPhase1)
        {
            state = State.getBack;
            velocity = velocity.normalized * speedCurvePhase1.Evaluate(1f);
            //velocity = PhysicsToric.Direction(transform.position, sender.transform.position) * speedCurvePhase1.Evaluate(1f);
            timeLaunch = Time.time;
            return;
        }

        velocity = maxSpeedPhase1 * speedCurvePhase1.Evaluate((Time.time - timeLaunch) / durationPhase1) * dir;
    }

    private void HandleGetBackState()
    {
        Vector2 targetDir = PhysicsToric.Direction(transform.position, sender.transform.position);

        Circle circleCollider = GetCircleCollider();
        Collider2D groundCol = PhysicsToric.OverlapCircle(circleCollider, groundMask);
        if (groundCol != null)
        {
            Collision2D.Collider2D col = Collision2D.Collider2D.FromUnityCollider2D(groundCol);

            Vector2 collisionPoint, normal1, normal2;
            if(!Collision2D.Collider2D.Collide(col, circleCollider, out collisionPoint, out normal1, out normal2))
            {
                collisionPoint = (circleCollider.center + col.center) * 0.5f;
                normal1 = (circleCollider.center - col.center).normalized;
                normal2 = -normal1;
            }
            dir = Collision2D.Ray2D.Symetric(-velocity, new Collision2D.Ray2D(Vector2.zero, normal1)).normalized;
            transform.position -= (Vector3)(velocity * Time.deltaTime);
        }

        float currentAng = Useful.AngleHori(Vector2.zero, dir);
        float targetAng = Useful.AngleHori(Vector2.zero, targetDir);
        float ang = Mathf.MoveTowards(currentAng, targetAng, Time.deltaTime * rotationSpeed);
        dir = Useful.Vector2FromAngle(ang);

        if (Time.time - timeLaunch < accelerationDurationPhase2)
        {
            velocity = maxSpeedPhase2 * accelerationCurvePhase2.Evaluate(1f) * dir;
        }
        else
        {
            velocity = maxSpeedPhase2 * dir;
        }

        if (PhysicsToric.Distance(transform.position, sender.transform.position) < recuperationRange)
        {
            sender.GetBack();
            animator.SetTrigger("destroy");
            velocity = Vector2.zero;
            isDestroy = true;
            if (animator.GetAnimationLength("destroy", out float length))
            {
                Invoke(nameof(Destroy), length);
            }
            else
                Destroy();

            return;
        }
    }

    private Circle GetCircleCollider()
    {
        float ang = Useful.AngleHori(Vector2.zero, circleOffset) + transform.rotation.z * Mathf.Deg2Rad;
        return new Circle((Vector2)transform.position + Useful.Vector2FromAngle(ang, circleOffset.magnitude), circleRadius);
    }

    private void Destroy()
    {
        toricObject.RemoveClones();
        Destroy(gameObject);
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        this.transform = base.transform;
        circleRadius = Mathf.Max(circleRadius, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Circle.GizmosDraw((Vector2)transform.position + circleOffset, circleRadius);
    }

#endif

    #region struct

    public struct BoomerangLaunchData
    {
        public Vector2 dir;
        public AnimationCurve speedCurvePhase1, accelerationCurvePhase2;
        public float maxSpeedPhase1, durationPhase1, accelerationDurationPhase2;
        public BoomerangAttack sender;
        public float maxSpeedPhase2;
        public float recuperationRange;
        public float rotationSpeed;

        public BoomerangLaunchData(in Vector2 dir, AnimationCurve speedCurvePhase1, AnimationCurve accelerationCurvePhase2, float maxSpeedPhase1, float durationPhase1, float accelerationDurationPhase2, BoomerangAttack sender, float maxSpeedPhase2, float recuperationRange, float rotationSpeed)
        {
            this.dir = dir;
            this.speedCurvePhase1 = speedCurvePhase1;
            this.accelerationCurvePhase2 = accelerationCurvePhase2;
            this.maxSpeedPhase1 = maxSpeedPhase1;
            this.durationPhase1 = durationPhase1;
            this.accelerationDurationPhase2 = accelerationDurationPhase2;
            this.sender = sender;
            this.maxSpeedPhase2 = maxSpeedPhase2;
            this.recuperationRange = recuperationRange;
            this.rotationSpeed = rotationSpeed;
        }
    }

    #endregion
}
