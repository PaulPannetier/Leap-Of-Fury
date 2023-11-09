using UnityEngine;
using PathFinding;
using Collision2D;
using Collider2D = UnityEngine.Collider2D;
using static PathFinderToric;
using static BezierUtility;

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
    private Vector2 targetPos;
    private bool isTargetingSender;
    private SplinePath path;
    private MapPoint oldSenderMapPoint = null;
    private float reachDist;

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

        transform.position += (Vector3)(velocity * Time.deltaTime);
    }

    private void HandleGoState()
    {
        Circle circleCollider = GetCircleCollider();
        Collider2D groundCol = PhysicsToric.OverlapCircle(circleCollider, groundMask);

        if(groundCol != null)
        {
            transform.position -= (Vector3)(velocity * (Time.deltaTime * 2f));
        }

        if (groundCol != null || Time.time - timeLaunch > durationPhase1)
        {
            state = State.getBack;
            velocity = velocity.normalized * speedCurvePhase1.Evaluate(1f);
            //velocity = PhysicsToric.Direction(transform.position, sender.transform.position) * speedCurvePhase1.Evaluate(1f);
            oldSenderMapPoint = null;
            timeLaunch = Time.time;
            return;
        }

        velocity = maxSpeedPhase1 * speedCurvePhase1.Evaluate((Time.time - timeLaunch) / durationPhase1) * dir;
        transform.rotation = Quaternion.Euler(0f, 0f, (velocity.Angle(Vector2.right) + Mathf.PI) * Mathf.Rad2Deg);
    }

    private void HandleGetBackState()
    {
        MapPoint currentSenderMapPoint = LevelMapData.currentMap.GetMapPointAtPosition(sender.transform.position);

        if(oldSenderMapPoint == null || oldSenderMapPoint != currentSenderMapPoint)
        {
            MapPoint start = LevelMapData.currentMap.GetMapPointAtPosition(transform.position);

            if (start == currentSenderMapPoint)
            {
                isTargetingSender = true;
                path = null;
            }
            else
            {
                Map pathFindingMap = LevelMapData.currentMap.GetPathfindingMap();
                path = PathFinderToric.FindBestCurve(pathFindingMap, start, currentSenderMapPoint, LevelMapData.currentMap.GetPositionOfMapPoint,
                    true, SplineType.Catmulrom, SmoothnessMode.ExtraSmoothness);

                int maxIter = 20;
                while(path == null && maxIter > 0)
                {
                    transform.position -= (Vector3)(velocity * Time.deltaTime);
                    start = LevelMapData.currentMap.GetMapPointAtPosition(transform.position);
                    path = PathFinderToric.FindBestCurve(pathFindingMap, start, currentSenderMapPoint, LevelMapData.currentMap.GetPositionOfMapPoint,
                        true, SplineType.Catmulrom, SmoothnessMode.ExtraSmoothness);
                    maxIter--;
                }

                if(path == null)
                {
                    StartDestroy();
                    return;
                }

                reachDist = 0f;
            }
        }
        oldSenderMapPoint = currentSenderMapPoint;

        MapPoint currentMapPoint = LevelMapData.currentMap.GetMapPointAtPosition(transform.position);
        if(currentMapPoint == currentSenderMapPoint)
        {
            isTargetingSender = true;
        }

        if(isTargetingSender && (currentMapPoint != currentSenderMapPoint))
        {
            isTargetingSender = false;
        }

        float speed = 0f;
        if (Time.time - timeLaunch < accelerationDurationPhase2)
        {
            speed = maxSpeedPhase2 * accelerationCurvePhase2.Evaluate((Time.time - timeLaunch) / accelerationDurationPhase2);
        }
        else
        {
            speed = maxSpeedPhase2;
        }

        Vector2 direction;
        //Move!
        if(isTargetingSender)
        {
            transform.position = Vector2.Lerp(transform.position, sender.transform.position, speed * Time.deltaTime);
            direction = PhysicsToric.Direction(transform.position, sender.transform.position);
        }
        else
        {
            reachDist += speed * Time.deltaTime;
            float reachDistPercent = reachDist / path.length;
            transform.position = path.EvaluateDistance(reachDistPercent);
            direction = path.Velocity(reachDistPercent).normalized;
        }

        //Rotate
        transform.rotation = Quaternion.Euler(0f, 0f, Useful.AngleHori(Vector2.zero, direction) * Mathf.Rad2Deg + 180f);

        if (PhysicsToric.Distance(transform.position, sender.transform.position) < recuperationRange)
        {
            StartDestroy();
            return;
        }
    }

    private void StartDestroy()
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
