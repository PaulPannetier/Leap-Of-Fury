using UnityEngine;
using PathFinding;
using Collision2D;
using Collider2D = UnityEngine.Collider2D;
using static PathFinderToric;
using static BezierUtility;
using System.Collections.Generic;

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
    private Vector2 dir, targetDir;
    private AnimationCurve speedCurvePhase1, accelerationCurvePhase2;
    private float maxSpeedPhase1, durationPhase1, accelerationDurationPhase2;
    private float maxSpeedPhase2, recuperationRange, notRepurableDuration;
    private float rotationSpeed;
    private State state;
    private float timeLaunch = -10f;
    private Vector2 velocity;
    private float startPhase2Velocity;
    private LayerMask groundMask, charMask;
    private bool isDestroy;
    private bool isTargetingSender;
    private SplinePath path;
    private MapPoint oldSenderMapPoint = null;
    private float reachDist;
    private PlayerCommon playerCommon;
    private List<uint> charAlreadyTouch;

    [SerializeField] private Vector2 groundCircleOffset;
    [SerializeField] private float groundCircleRadius;
    [SerializeField] private Vector2 charCircleOffset;
    [SerializeField] private float charCircleRadius;

    private void Awake()
    {
        this.transform = base.transform;
        toricObject = GetComponent<ToricObject>();
        animator = GetComponent<Animator>();
        charAlreadyTouch = new List<uint>();
    }

    private void Start()
    {
        groundMask = LayerMask.GetMask("Floor");
        charMask = LayerMask.GetMask("Char");
    }

    public void Launch(in BoomerangLaunchData boomerangLauchData)
    {
        Builder(boomerangLauchData);

        state = State.go;
        velocity = maxSpeedPhase1 * speedCurvePhase1.Evaluate(0f) * dir;
        timeLaunch = Time.time;
        playerCommon = sender.GetComponent<PlayerCommon>();

        void Builder(in BoomerangLaunchData data)
        {
            targetDir = dir = data.dir; 
            speedCurvePhase1 = data.speedCurvePhase1;
            maxSpeedPhase1 = data.maxSpeedPhase1;
            durationPhase1 = data.durationPhase1;
            sender = data.sender;
            maxSpeedPhase2 = data.maxSpeedPhase2;
            accelerationCurvePhase2 = data.accelerationCurvePhase2;
            maxSpeedPhase2 = data.maxSpeedPhase2;
            recuperationRange = data.recuperationRange;
            notRepurableDuration = data.notRecuperableDuration;
            rotationSpeed = data.rotationSpeed;
            accelerationDurationPhase2 = data.accelerationDurationPhase2;
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

        CheckCharCollission();
    }

    private void CheckCharCollission()
    {
        Circle circle = GetCharCircleCollider();

        Collider2D[] cols = PhysicsToric.OverlapCircleAll(circle, charMask);
        foreach (Collider2D col in cols)
        {
            if(col.CompareTag("Char"))
            {
                GameObject player = col.GetComponent<ToricObject>().original;
                PlayerCommon playerTouchCommon = player.GetComponent<PlayerCommon>();
                if(playerCommon.id != playerTouchCommon.id)
                {
                    if(!charAlreadyTouch.Contains(playerTouchCommon.id))
                    {
                        charAlreadyTouch.Add(playerTouchCommon.id);
                        sender.OnTouchEnemy(player);
                    }
                }
            }
        }
    }

    private void HandleGoState()
    {
        Circle circleCollider = GetGroundCircleCollider();
        Collider2D groundCol = PhysicsToric.OverlapCircle(circleCollider, groundMask);

        if(groundCol != null)
        {
            transform.position -= (Vector3)(velocity * (Time.deltaTime * 2f));
        }

        if(Time.time - timeLaunch > notRepurableDuration)
        {
            if (groundCol != null || Time.time - timeLaunch > durationPhase1)
            {
                state = State.getBack;
                startPhase2Velocity = velocity.magnitude;
                oldSenderMapPoint = null;
                timeLaunch = Time.time;
                return;
            }
            targetDir = dir;
        }
        else
        {
            if(groundCol != null)
            {
                targetDir = dir;
                dir *= -1f;
            }
        }

        float ang = Useful.AngleHori(Vector2.zero, dir) * Mathf.Rad2Deg;
        float targetAng = Useful.AngleHori(Vector2.zero, targetDir) * Mathf.Rad2Deg;
        dir = Useful.Vector2FromAngle(Mathf.MoveTowardsAngle(ang, targetAng, rotationSpeed * Time.deltaTime) * Mathf.Deg2Rad).normalized;

        velocity = maxSpeedPhase1 * speedCurvePhase1.Evaluate((Time.time - timeLaunch) / durationPhase1) * dir;
        Vector3 newPos = (Vector2)transform.position + velocity * Time.deltaTime;
        transform.SetPositionAndRotation(newPos, Quaternion.Euler(0f, 0f, (velocity.Angle(Vector2.right) + Mathf.PI) * Mathf.Rad2Deg));
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
            float currentSpeed = velocity.magnitude;
            float delta = currentSpeed - startPhase2Velocity;
            speed = currentSpeed + delta * accelerationCurvePhase2.Evaluate((Time.time - timeLaunch) / accelerationDurationPhase2);
        }
        else
        {
            speed = maxSpeedPhase2;
        }

        //Compute direction
        Vector2 direction;
        if(isTargetingSender)
        {
            direction = PhysicsToric.Direction(transform.position, sender.transform.position);
        }
        else
        {
            reachDist += speed * Time.deltaTime;
            float reachDistPercent = reachDist / path.length;
            Vector2 targetPosition = path.EvaluateDistance(reachDistPercent);
            direction = PhysicsToric.Direction(transform.position, targetPosition);
        }

        //Move and Rotate
        velocity = speed * direction;
        Vector3 newPos = (Vector2)transform.position + velocity * Time.deltaTime;
        float newAngle = (direction.Angle(Vector2.right) + Mathf.PI) * Mathf.Rad2Deg;
        float currentAngle = transform.rotation.eulerAngles.z;
        transform.SetPositionAndRotation(newPos, Quaternion.Euler(0f, 0f, Mathf.MoveTowardsAngle(currentAngle, newAngle, Time.deltaTime * rotationSpeed)));

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

    private Circle GetGroundCircleCollider()
    {
        float ang = Useful.AngleHori(Vector2.zero, groundCircleOffset) + transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
        return new Circle((Vector2)transform.position + Useful.Vector2FromAngle(ang, groundCircleOffset.magnitude), groundCircleRadius);
    }

    private Circle GetCharCircleCollider()
    {
        float ang = Useful.AngleHori(Vector2.zero, charCircleOffset) + transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
        return new Circle((Vector2)transform.position + Useful.Vector2FromAngle(ang, charCircleOffset.magnitude), charCircleRadius);
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
        groundCircleRadius = Mathf.Max(groundCircleRadius, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Circle.GizmosDraw(GetGroundCircleCollider());
        Circle.GizmosDraw(GetCharCircleCollider());

        if(path != null && state == State.getBack)
        {
            Vector2[] points = path.EvaluateDistance(200);
            Vector2 beg = points[0];

            foreach(Vector2 point in points)
            {
                PhysicsToric.GizmosDrawRaycast(beg, point);
                beg = point;
            }
        }
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
        public float notRecuperableDuration;
        public float rotationSpeed;

        public BoomerangLaunchData(in Vector2 dir, AnimationCurve speedCurvePhase1, AnimationCurve accelerationCurvePhase2, float maxSpeedPhase1,
            float durationPhase1, float accelerationDurationPhase2, BoomerangAttack sender, float maxSpeedPhase2, float recuperationRange,
            float notRecuperableDuration, float rotationSpeed)
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
            this.notRecuperableDuration = notRecuperableDuration;
            this.rotationSpeed = rotationSpeed;
        }
    }

    #endregion
}
