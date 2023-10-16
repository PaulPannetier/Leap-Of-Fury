using System;
using UnityEngine;
using PathFinding;
using Collision2D;
using Collider2D = UnityEngine.Collider2D;
using static Boomerang;
using System.Collections.Generic;

public class BoomerangAttack : WeakAttack
{
    private Movement movement;
    private Boomerang currentBoomerang;

    [SerializeField] private GameObject boomerangPrefab;
    [SerializeField] private float distanceToInstantiate = 0.4f;
    [SerializeField] private AnimationCurve speedCurvePhase1, accelerationCurvePhase2;
    [SerializeField] private float maxSpeedPhase1, maxSpeedPhase2, durationPhase1, accelerationDurationPhase2;
    [SerializeField] private float recuperationRange;

#if UNITY_EDITOR

    [SerializeField] private bool drawGizmos = true;

#endif

    protected override void Awake()
    {
        base.Awake();
        movement = GetComponent<Movement>();
    }


    public override bool Launch(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
        if (!cooldown.isActive)
        {
            callbackEnableOtherAttack.Invoke();
            callbackEnableThisAttack.Invoke();
            return false;
        }

        Vector2 dir = movement.GetCurrentDirection(true);
        Vector2 pos = PhysicsToric.GetPointInsideBounds((Vector2)transform.position + dir * distanceToInstantiate);
        currentBoomerang = Instantiate(boomerangPrefab, pos, Quaternion.identity, CloneParent.cloneParent).GetComponent<Boomerang>();

        currentBoomerang.Launch(CreateLaunchData(dir));

        cooldown.Reset();

        callbackEnableOtherAttack.Invoke();
        callbackEnableThisAttack.Invoke();
        return true;
    }

    private BoomerangLaunchData CreateLaunchData(in Vector2 dir)
    {
        return new BoomerangLaunchData(dir, speedCurvePhase1, accelerationCurvePhase2, maxSpeedPhase1, durationPhase1,
            accelerationDurationPhase2, this, maxSpeedPhase2, recuperationRange);
    }

    public void GetBack()
    {
        currentBoomerang = null;
    }

#if UNITY_EDITOR

    [SerializeField] private bool testPathFinder = false;
    [SerializeField] private Vector2 startPoint, endPoint;
    private bool drawPath = false;
    private MapPoint[] bestPath;
    private List<GameObject> primitives = new List<GameObject>();
    [SerializeField] private GameObject sqaurePrefabs;

    protected void OnValidate()
    {
        if (testPathFinder)
        {
            Map pathFindingMap = LevelMapData.currentMap.GetPathfindingMap();
            AStar aStar = new AStar(pathFindingMap);

            for (int i = 0; i < primitives.Count; i++)
            {
                DestroyImmediate(primitives[i]);
            }
            primitives.Clear();

            Vector2Int size = new Vector2Int((LevelMapData.currentMap.mapSize.x / LevelMapData.currentMap.cellSize.x).Round(), (LevelMapData.currentMap.mapSize.y / LevelMapData.currentMap.cellSize.y).Round());
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    GameObject prim = Instantiate(sqaurePrefabs);
                    prim.transform.localScale = new Vector3(LevelMapData.currentMap.cellSize.x, LevelMapData.currentMap.cellSize.y, 0.01f);

                    Color r = new Color(1f, 0f, 0f, 0.4f);
                    Color g = new Color(0f, 1f, 0f, 0.4f);
                    prim.GetComponent<SpriteRenderer>().material.color = pathFindingMap.IsWall(new MapPoint(x, y)) ? r : g;
                    Vector2 pos = LevelMapData.currentMap.GetPositionOfMapPoint(new MapPoint(x, y));
                    prim.transform.position = new Vector3(pos.x, pos.y, 0f);
                }
            }

            MapPoint endMapPoint = LevelMapData.currentMap.GetMapPointAtPosition(endPoint);
            MapPoint startMapPoint = LevelMapData.currentMap.GetMapPointAtPosition(startPoint);

            bestPath = aStar.CalculateBestPath(startMapPoint, endMapPoint);
            drawPath = true;
            testPathFinder = false;
        }

        durationPhase1 = Mathf.Max(0f, durationPhase1);
        accelerationDurationPhase2 = Mathf.Max(0f, accelerationDurationPhase2);
        maxSpeedPhase1 = Mathf.Max(0f, maxSpeedPhase1);
        maxSpeedPhase2 = Mathf.Max(0f, maxSpeedPhase2);
        distanceToInstantiate = Mathf.Max(0f, distanceToInstantiate);

        recuperationRange = Mathf.Max(0f, recuperationRange);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Circle.GizmosDraw(startPoint, 0.3f);
        Circle.GizmosDraw(endPoint, 0.3f);

        if(drawPath)
        {
            Vector2 beg = LevelMapData.currentMap.GetPositionOfMapPoint(bestPath[0]);
            for (int i = 1; i < bestPath.Length; i++)
            {
                Vector2 end = LevelMapData.currentMap.GetPositionOfMapPoint(bestPath[i]);
                Gizmos.DrawLine(beg, end);
                beg = end;
            }
        }

        if (!drawGizmos)
            return;

        Gizmos.color = Color.green;
        Circle.GizmosDraw(transform.position, recuperationRange);
        Gizmos.color = Color.black;
        Circle.GizmosDraw(transform.position, distanceToInstantiate);
    }

#endif
}
