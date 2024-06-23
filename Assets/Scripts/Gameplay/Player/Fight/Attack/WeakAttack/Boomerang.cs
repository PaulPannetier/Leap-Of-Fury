using UnityEngine;
using PathFinding;
using Collision2D;
using Collider2D = UnityEngine.Collider2D;
using static PathFinderToric;
using static BezierUtility;
using System.Collections.Generic;
using System.Collections;
using System;

public class Boomerang : MonoBehaviour
{
    private enum State : byte
    {
        go,
        getBack
    }

    private new Transform transform;
    private Animator animator;
    private BoomerangAttack sender;
    private Vector2 dir;
    private AnimationCurve speedCurvePhase1, speedCurvePhase2;
    private float maxSpeedPhase1, durationPhase1, accelerationDurationPhase2;
    private float maxSpeedPhase2, recuperationRange;
    private State state;
    private float timeLaunch = -10f, lastPathFindingSearch = -10f;
    private Vector2 velocity;
    private LayerMask groundMask, charMask;
    private bool isTargetingSender, requestSearchPath, isDestroy;
    private SplinePath path;
    private float reachDist;
    private PlayerCommon playerCommon;
    private List<uint> charAlreadyTouch;
    private Map pathFindingMap;
    private float minDelayBetweenPathfindingSearch;

    [SerializeField] private Vector2 groundCircleOffset;
    [SerializeField] private float groundCircleRadius;
    [SerializeField] private Vector2 charCircleOffset;
    [SerializeField] private float charCircleRadius;

    private void Awake()
    {
        this.transform = base.transform;
        animator = GetComponent<Animator>();
        charAlreadyTouch = new List<uint>(4);
    }

    private void Start()
    {
        groundMask = LayerMask.GetMask("Floor");
        charMask = LayerMask.GetMask("Char");
        PauseManager.instance.callBackOnPauseDisable += OnPauseDisable;
        PauseManager.instance.callBackOnPauseEnable += OnPauseEnable;
    }

    public void Launch(in BoomerangLaunchData boomerangLauchData)
    {
        dir = boomerangLauchData.dir;
        speedCurvePhase1 = boomerangLauchData.speedCurvePhase1;
        maxSpeedPhase1 = boomerangLauchData.maxSpeedPhase1;
        durationPhase1 = boomerangLauchData.durationPhase1;
        sender = boomerangLauchData.sender;
        maxSpeedPhase2 = boomerangLauchData.maxSpeedPhase2;
        speedCurvePhase2 = boomerangLauchData.speedCurvePhase2;
        recuperationRange = boomerangLauchData.recuperationRange;
        accelerationDurationPhase2 = boomerangLauchData.accelerationDurationPhase2;
        minDelayBetweenPathfindingSearch = boomerangLauchData.minDelayBetweenPathfindingSearch;

        state = State.go;
        velocity = maxSpeedPhase1 * speedCurvePhase1.Evaluate(0f) * dir;
        timeLaunch = Time.time;
        playerCommon = sender.GetComponent<PlayerCommon>();
    }

    private void Update()
    {
        if (isDestroy)
            return;

        if(PauseManager.instance.isPauseEnable)
        {
            lastPathFindingSearch += Time.deltaTime;
            timeLaunch += Time.deltaTime;
            return;
        }

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

        if (groundCol != null || Time.time - timeLaunch > durationPhase1)
        {
            LaunchGetBackState();
            return;
        }

        velocity = maxSpeedPhase1 * speedCurvePhase1.Evaluate((Time.time - timeLaunch) / durationPhase1) * dir;
        Vector3 newPos = (Vector2)transform.position + velocity * Time.deltaTime;
        transform.SetPositionAndRotation(newPos, Quaternion.Euler(0f, 0f, (velocity.Angle(Vector2.right) + Mathf.PI) * Mathf.Rad2Deg));
    }

    private void LaunchGetBackState()
    {
        state = State.getBack;
        pathFindingMap = LevelMapData.currentMap.GetPathfindingMap();
        timeLaunch = lastPathFindingSearch = Time.time;
    }

    private void HandleGetBackState()
    {
        MapPoint currentSenderMapPoint = LevelMapData.currentMap.GetMapPointAtPosition(pathFindingMap, sender.transform.position);
        MapPoint currentMapPoint = LevelMapData.currentMap.GetMapPointAtPosition(pathFindingMap, transform.position);

        if (currentMapPoint == currentSenderMapPoint)
        {
            isTargetingSender = true;
            path = null;
        }

        if (isTargetingSender && (currentMapPoint != currentSenderMapPoint))
        {
            isTargetingSender = false;
        }

        if (requestSearchPath || (path == null && !isTargetingSender) || (Time.time - lastPathFindingSearch > minDelayBetweenPathfindingSearch))
        {
            pathFindingMap = LevelMapData.currentMap.GetPathfindingMap();

            Vector2 GetPositionOfMapPoint(MapPoint mapPoint)
            {
                return LevelMapData.currentMap.GetPositionOfMapPoint(pathFindingMap, mapPoint);
            }

            path = PathFinderToric.FindBestCurve(pathFindingMap, currentMapPoint, currentSenderMapPoint, GetPositionOfMapPoint,
                true, SplineType.Catmulrom, SmoothnessMode.ExtraSmoothness);

            //int maxIter = 20;
            //while (path == null && maxIter > 0)
            //{
            //    transform.position -= (Vector3)(velocity * Time.deltaTime);
            //    currentMapPoint = LevelMapData.currentMap.GetMapPointAtPosition(pathFindingMap, transform.position);
            //    path = PathFinderToric.FindBestCurve(pathFindingMap, currentMapPoint, currentSenderMapPoint, GetPositionOfMapPoint,
            //        true, SplineType.Catmulrom, SmoothnessMode.ExtraSmoothness);
            //    maxIter--;
            //}

            if (path == null)
            {
                LogManager.instance.AddLog("No valid path was found, destroying boomerang", "Boomerang::HandleGetBackState");
                StartDestroy();
                return;
            }

            lastPathFindingSearch = Time.time;
            requestSearchPath = false;
            reachDist = 0f;
        }

        float speed = 0f;
        if (Time.time - timeLaunch < accelerationDurationPhase2)
        {
            speed = maxSpeedPhase2 * speedCurvePhase2.Evaluate((Time.time - timeLaunch) / accelerationDurationPhase2);
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
            if(reachDistPercent >= 1f)
            {
                requestSearchPath = true;
            }
        }

        //Move and Rotate
        velocity = speed * direction;

        Vector3 newPos = (Vector2)transform.position + velocity * Time.deltaTime;
        transform.SetPositionAndRotation(newPos, Quaternion.Euler(0f, 0f, (velocity.Angle(Vector2.right) + Mathf.PI) * Mathf.Rad2Deg));

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
            PauseManager.instance.Invoke(Destroy, length);
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
        Destroy(gameObject);
    }

    #region Gizmos/OnValidate/Pause

    private void OnPauseEnable()
    {
        animator.speed = 0f;
    }

    private void OnPauseDisable()
    {
        animator.speed = 1f;
    }

    private void OnDestroy()
    {
        PauseManager.instance.callBackOnPauseDisable -= OnPauseDisable;
        PauseManager.instance.callBackOnPauseEnable-= OnPauseEnable;
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        this.transform = base.transform;
        groundCircleRadius = Mathf.Max(groundCircleRadius, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        Circle.GizmosDraw(GetGroundCircleCollider(), Color.green);
        Circle.GizmosDraw(GetCharCircleCollider(), Color.green);

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

    #endregion

    #region Struct

    public struct BoomerangLaunchData
    {
        public Vector2 dir;
        public AnimationCurve speedCurvePhase1, speedCurvePhase2;
        public float maxSpeedPhase1, durationPhase1, accelerationDurationPhase2;
        public BoomerangAttack sender;
        public float maxSpeedPhase2;
        public float recuperationRange;
        public float minDelayBetweenPathfindingSearch;

        public BoomerangLaunchData(in Vector2 dir, AnimationCurve speedCurvePhase1, AnimationCurve accelerationCurvePhase2, float maxSpeedPhase1,
            float durationPhase1, float accelerationDurationPhase2, BoomerangAttack sender, float maxSpeedPhase2, float recuperationRange,
            int pathFindingAccuracy, float minDelayBetweenPathfindingSearch)
        {
            this.dir = dir;
            this.speedCurvePhase1 = speedCurvePhase1;
            this.speedCurvePhase2 = accelerationCurvePhase2;
            this.maxSpeedPhase1 = maxSpeedPhase1;
            this.durationPhase1 = durationPhase1;
            this.accelerationDurationPhase2 = accelerationDurationPhase2;
            this.sender = sender;
            this.maxSpeedPhase2 = maxSpeedPhase2;
            this.recuperationRange = recuperationRange;
            this.minDelayBetweenPathfindingSearch = minDelayBetweenPathfindingSearch;
        }
    }

    #endregion
}
