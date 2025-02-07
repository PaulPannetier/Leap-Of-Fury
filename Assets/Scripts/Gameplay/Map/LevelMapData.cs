using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;
using Collision2D;
using Collider2D = UnityEngine.Collider2D;
using PathFinding;

public class LevelMapData : MonoBehaviour
{
    #region Fields

    private const int pathFindingBaseCost = 1000;
    private const int pathFindingWallPenaltyCost = 300;

    private static LevelMapData _currentMap;
    public static LevelMapData currentMap
    {
        get
        {
#if UNITY_EDITOR
            if (_currentMap == null)
                _currentMap = GameObject.FindGameObjectWithTag("Map").GetComponent<LevelMapData>();
#endif
            return _currentMap;
        }
        private set
        {
            _currentMap = value;
        }
    }

    private Grid grid;
    private Collider2D[] staticMapColliders;
    private Collider2D[] nonStaticMapColliders;
    private Map _staticPathfindingMap;
    private Map staticPathfindingMap
    {
        get
        {
            if(_staticPathfindingMap == null)
                _staticPathfindingMap = ComputeStaticPathfindingMap();
            return _staticPathfindingMap;
        }
        set { _staticPathfindingMap = value; }
    }

#if UNITY_EDITOR
    [SerializeField] private bool drawGizmos = true;
#endif

    [SerializeField] private SpawnConfigsData spawnConfigs;

#if UNITY_EDITOR

    [Header("Save spawn Point")]
    [SerializeField] private bool applyCurrentConfig;
    [SerializeField] private bool addSpawnConfig;
    [SerializeField] private bool loadSpawnConfig;
    [SerializeField] private bool unloadSpawnConfig;
    [SerializeField] private bool clearSpawnConfig;
    [SerializeField] private Vector2[] spawnConfig;

#endif

    [SerializeField] private string relatifSpawnConfigsPath;
    [SerializeField] private GameObject tilemap;

    [Tooltip("Number of cells in the horizontal and vertical axis")] public Vector2 mapSize = new Vector2(32f, 18f);

    public Vector2 cellSize
    {
        get
        {
#if UNITY_EDITOR
            if(grid == null)
            {
                grid = GetComponentInChildren<Grid>();
            }
#endif
            return grid.cellSize;
        }
    }

    [Header("Pathfinding")]
    [SerializeField, Range(1, 5)] private int pathfindingMapAccuracy;
    [SerializeField] private int maxWallDistancePenalty = 3;
    [HideInInspector] public Vector2 pathfindingCellsSize => cellSize / pathfindingMapAccuracy;

    #endregion

    #region Awake/Start

    private void Awake()
    {
        currentMap = this;
    }

    private void Start()
    {
        GameObject tilemapGo = null;
        foreach (Transform child in transform)
        {
            if(child.CompareTag("Tilemap"))
            {
                tilemapGo = child.gameObject;
                break;
            }
        }

        grid = tilemapGo.GetComponent<Grid>();
        Collider2D[] collliders = tilemap.GetComponentsInChildren<Collider2D>();
        List<Collider2D> staticCol = new List<Collider2D>();
        List<Collider2D> nonStaticCol = new List<Collider2D>();
        for (int i = 0; i < collliders.Length; i++)
        {
            MapColliderData colliderData = collliders[i].GetComponent<MapColliderData>();
            if (colliderData != null)
            {
                if (colliderData.isStatic)
                {
                    staticCol.Add(collliders[i]);
                }
                else
                {
                    nonStaticCol.Add(collliders[i]);
                }
            }
        }
        staticMapColliders = staticCol.ToArray();
        nonStaticMapColliders = nonStaticCol.ToArray();

        EventManager.instance.OnMapChanged(this);
        staticPathfindingMap = ComputeStaticPathfindingMap();
    }

    #endregion

    #region PathFinding

    private List<MapPoint> GetColliderBlockedPoints(Map map, Collider2D collider, bool isToricCollider)
    {
        List<MapPoint> HandleHitbox(Vector2 pos, Vector2 size)
        {
            if (!isToricCollider)
            {
                Vector2 halfMapLength = cellSize * mapSize * 0.5f;
                float xMin = Mathf.Max(pos.x - (size.x * 0.5f), -halfMapLength.x);
                float xMax = Mathf.Min(pos.x + (size.x * 0.5f), halfMapLength.x);
                float yMin = Mathf.Max(pos.y - (size.y * 0.5f), -halfMapLength.y);
                float yMax = Mathf.Min(pos.y + (size.y * 0.5f), halfMapLength.y);
                size = new Vector2(xMax - xMin, yMax - yMin);
                pos = new Vector2((xMin + xMax) * 0.5f, (yMin + yMax) * 0.5f);
            }
            return PathFindingBlocker.GetBlockedCellsInRectangle(map, pos, size * 0.999f);
        }

        if (collider is BoxCollider2D boxCollider)
        {
            return HandleHitbox((Vector2)boxCollider.transform.position + boxCollider.offset, boxCollider.size);
        }
        else if (collider is CircleCollider2D circleCollider)
        {
            return PathFindingBlocker.GetBlockedCellsInCircle(map, (Vector2)circleCollider.transform.position + circleCollider.offset, circleCollider.radius * 0.999f);
        }

        string errorMsg = $"The type {collider.GetType()} of Collider is unsuported, a cast into an Hitbox is apply";
        LogManager.instance.AddLog(errorMsg, collider, "LevelMapData::GetColliderBlockedPoints");
        Debug.LogWarning(errorMsg);

        Hitbox hitbox = Collision2D.Collider2D.FromUnityCollider2D(collider).ToHitbox();
        return HandleHitbox(hitbox.center, hitbox.size);
    }

    private void ApplyWallPenalty(ref int[,] cost, int maxWallDistance)
    {
        HashSet<MapPoint>[] wallDistPoint = new HashSet<MapPoint>[maxWallDistance + 1];
        for (int i = 0; i < wallDistPoint.Length; i++)
        {
            wallDistPoint[i] = new HashSet<MapPoint>();
        }

        Vector2Int mapSize = new Vector2Int(cost.GetLength(0), cost.GetLength(1));

        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                MapPoint mapPoint = new MapPoint(x, y);
                if (cost[mapPoint.X, mapPoint.Y] < 0)
                {
                    wallDistPoint[0].Add(mapPoint);
                }
            }
        }

        for (int i = 1; i <= maxWallDistance; i++)
        {
            foreach (MapPoint mapPoint in wallDistPoint[i - 1])
            {
                bool IsMapPointAlreadyAssign(MapPoint mapPoint)
                {
                    for (int j = 0; j < i; j++)
                    {
                        if (wallDistPoint[j].Contains(mapPoint))
                            return true;
                    }
                    return false;
                }

                MapPoint right = new MapPoint(mapPoint.X == mapSize.x - 1 ? 0 : mapPoint.X + 1, mapPoint.Y);
                MapPoint left = new MapPoint(mapPoint.X == 0 ? mapSize.x - 1 : mapPoint.X - 1, mapPoint.Y);
                MapPoint up = new MapPoint(mapPoint.X, mapPoint.Y == mapSize.y - 1 ? 0 : mapPoint.Y + 1);
                MapPoint down = new MapPoint(mapPoint.X, mapPoint.Y == 0 ? mapSize.y - 1 : mapPoint.Y - 1);

                if(cost[right.X, right.Y] > 0 && !IsMapPointAlreadyAssign(right))
                {
                    wallDistPoint[i].Add(right);
                }
                if (cost[left.X, left.Y] > 0 && !IsMapPointAlreadyAssign(left))
                {
                    wallDistPoint[i].Add(left);
                }
                if (cost[up.X, up.Y] > 0 && !IsMapPointAlreadyAssign(up))
                {
                    wallDistPoint[i].Add(up);
                }
                if (cost[down.X, down.Y] > 0 && !IsMapPointAlreadyAssign(down))
                {
                    wallDistPoint[i].Add(down);
                }
            }
        }

        wallDistPoint.Reverse();
        for (int i = 0; i < wallDistPoint.Length - 1; i++)
        {
            int extraCost = (i + 1) * pathFindingWallPenaltyCost;
            foreach (MapPoint mapPoint in wallDistPoint[i])
            {
                cost[mapPoint.X, mapPoint.Y] += extraCost;
            }
        }
    }

    private Map ComputeStaticPathfindingMap()
    {
        Vector2Int pathfindignSize = new Vector2Int((mapSize.x * pathfindingMapAccuracy).Round(), (mapSize.y * pathfindingMapAccuracy).Round());
        int[,] costMap = new int[pathfindignSize.x, pathfindignSize.y];
        Map res = new Map(costMap, pathfindingMapAccuracy);

        HashSet<MapPoint> blockedPoints = new HashSet<MapPoint>();
        for (int i = 0; i < staticMapColliders.Length; i++)
        {
            List<MapPoint> colPoints = GetColliderBlockedPoints(res, staticMapColliders[i], false);
            for (int j = 0; j < colPoints.Count; j++)
            {
                blockedPoints.Add(colPoints[j]);
            }
        }

        for (int x = 0; x < costMap.GetLength(0); x++)
        {
            for (int y = 0; y < costMap.GetLength(1); y++)
            {
                costMap[x, y] = blockedPoints.Contains(new MapPoint(x, y)) ? -1 : 1;
            }
        }

        staticPathfindingMap = res;
        return res;
    }

    public Map GetPathfindingMap()
    {
        List<PathFindingBlocker> blockers = PathFindingBlocker.GetPathFindingBlockers();
        HashSet<MapPoint> blockedPoints = new HashSet<MapPoint>();

        Vector2Int pathfindignSize = new Vector2Int((mapSize.x * pathfindingMapAccuracy).Round(), (mapSize.y * pathfindingMapAccuracy).Round());
        int[,] costMap = new int[pathfindignSize.x, pathfindignSize.y];
        Map res = new Map(costMap, pathfindingMapAccuracy);

        foreach (PathFindingBlocker blocker in blockers)
        {
            foreach (MapPoint mapPoint in blocker.GetBlockedCells(res))
            {
                blockedPoints.Add(mapPoint);
            }
        }

        for (int i = 0; i < nonStaticMapColliders.Length; i++)
        {
            List<MapPoint> colPoints = GetColliderBlockedPoints(res, nonStaticMapColliders[i], true);
            for (int j = 0; j < colPoints.Count; j++)
            {
                blockedPoints.Add(colPoints[j]);
            }
        }

        for (int x = 0; x < costMap.GetLength(0); x++)
        {
            for (int y = 0; y < costMap.GetLength(1); y++)
            {
                MapPoint mapPoint = new MapPoint(x, y);
                if (staticPathfindingMap.GetCost(mapPoint) < 0)
                {
                    costMap[x, y] = -1;
                }
                else
                {
                    costMap[x, y] = blockedPoints.Contains(mapPoint) ? -1 : pathFindingBaseCost;
                }
            }
        }

        int maxWallDistance = this.maxWallDistancePenalty * this.pathfindingMapAccuracy;
        if (maxWallDistance > 0)
            ApplyWallPenalty(ref costMap, maxWallDistance);

        return res;
    }

    public MapPoint GetMapPointAtPosition(Map pathfindingMap, Vector2 position)
    {
        Vector2 pathfindingMapSize = mapSize * pathfindingMap.accuracy;
        Vector2 size = mapSize * cellSize;
        position = PhysicsToric.GetPointInsideBounds(position) + size * 0.5f - (cellSize * 0.5f / pathfindingMap.accuracy);
        float x = Mathf.InverseLerp(0f, size.x, position.x);
        x = Mathf.Lerp(0f, pathfindingMapSize.x, x);
        float y = Mathf.InverseLerp(0f, size.y, position.y);
        y = Mathf.Lerp(0f, pathfindingMapSize.y, y);
        return new MapPoint(x.Round(), y.Round());
    }

    public Vector2 GetPositionOfMapPoint(Map pathfindingMap, MapPoint mapPoint)
    {
        Vector2 pathfindingMapSize = mapSize * pathfindingMap.accuracy;
        Vector2 size = mapSize * cellSize;
        float x = ((mapPoint.X / pathfindingMapSize.x) * size.x) - (size.x * 0.5f); 
        float y = ((mapPoint.Y / pathfindingMapSize.y) * size.y) - (size.y * 0.5f);
        return new Vector2(x + (cellSize.x * 0.5f / pathfindingMap.accuracy), y + (cellSize.y * 0.5f / pathfindingMap.accuracy));
    }

    #endregion

    #region SpawnPoint

#if UNITY_EDITOR

    public SpawnConfigsData LoadSpawnPoint()
    {
        if (Save.ReadJSONData<SpawnConfigsData>(relatifSpawnConfigsPath + SettingsManager.saveFileExtension, out spawnConfigs))
        {
            return spawnConfigs;
        }
        Debug.LogWarning("Can't open SpawnConfigsData's object at the path : " + relatifSpawnConfigsPath + SettingsManager.saveFileExtension);
        spawnConfigs = null;
        return null;
    }

#endif

    public List<SpawnConfigsData.SpawnConfigPoints> LoadSpawnPoint(int nbChar)
    {
        if (Save.ReadJSONData<SpawnConfigsData>(relatifSpawnConfigsPath + SettingsManager.saveFileExtension, out spawnConfigs))
        {
            switch (nbChar)
            {
                case 2:
                    return spawnConfigs.spawnConfigPoints2Char;
                case 3:
                    return spawnConfigs.spawnConfigPoints3Char;
                case 4:
                    return spawnConfigs.spawnConfigPoints4Char;
                default:
                    return null;
            }
        }

        string errorMsg = $"Can't open SpawnConfigsData's object at the path : {relatifSpawnConfigsPath + SettingsManager.saveFileExtension}.";
        LogManager.instance.AddLog(errorMsg, "LevelMapData.LoadSpawnPoint(int nbChar)");
        Debug.LogWarning(errorMsg);
        spawnConfigs = null;
        return null;
    }

    #endregion

    #region Gizmos/OnValidate

#if UNITY_EDITOR

    private static readonly Dictionary<int, float> convertNbCharToRadius = new Dictionary<int, float>
    {
        { 2, 0.2f },
        { 3, 0.3f },
        { 4, 0.4f }
    };

    private void OnDrawGizmosSelected()
    {
        if(!drawGizmos)
            return;

        Gizmos.color = Color.green;
        foreach (Vector2 v in spawnConfig)
        {
            Circle.GizmosDraw(v, 0.45f);
        }

        if (spawnConfigs != null)
        {
            Color[] colors = new Color[] { Color.red, Color.yellow, Color.blue, Color.cyan, Color.magenta, Color.grey, Color.white, Color.black, new Color(1f, 0.5f, 0f, 1f), new Color(52f/255, 131f/255f, 178f/255f, 1f), new Color(0f, 68f/255f, 16f/255f), new Color(180f/255f, 3f/255f, 39f/255f) };

            List<SpawnConfigsData.SpawnConfigPoints>[] spawnConfigsPerChars = new List<SpawnConfigsData.SpawnConfigPoints>[3]
            {
                spawnConfigs.spawnConfigPoints2Char, spawnConfigs.spawnConfigPoints3Char, spawnConfigs.spawnConfigPoints4Char
            };

            int colorIndex = 0;
            foreach (List<SpawnConfigsData.SpawnConfigPoints> configPerChar in spawnConfigsPerChars)
            {
                foreach (SpawnConfigsData.SpawnConfigPoints points in configPerChar)
                {
                    foreach (Vector2 point in points)
                    {
                        Circle.GizmosDraw(point, convertNbCharToRadius[points.points.Length], colors[colorIndex]);
                    }
                    colorIndex = (colorIndex + 1) % colors.Length;
                }
            }
        }

        Hitbox.GizmosDraw(Vector2.zero, mapSize * cellSize, Color.white);
        Circle.GizmosDraw(Vector2.zero, 0.1f, Color.white, true);
    }

    private void OnValidate()
    {
        currentMap = this;
        maxWallDistancePenalty = Mathf.Max(0, maxWallDistancePenalty);

        if (applyCurrentConfig)
        {
            applyCurrentConfig = false;
            if (!Save.WriteJSONData(spawnConfigs, relatifSpawnConfigsPath + SettingsManager.saveFileExtension, mkdir:true))
				Debug.LogWarning("Couldn't write ??<idk what tbh>?? to disk !!!");
        }

        if (addSpawnConfig)
        {
            addSpawnConfig = false;
            if(spawnConfig != null && spawnConfig.Length >= 2 && spawnConfig.Length <= 4)
            {
                //Save info
                if (!Save.ReadJSONData<SpawnConfigsData>(relatifSpawnConfigsPath + SettingsManager.saveFileExtension, out spawnConfigs))
                {
                    spawnConfigs = new SpawnConfigsData();
                }

                switch (spawnConfig.Length)
                {
                    case 2:
                        spawnConfigs.spawnConfigPoints2Char.Add(new SpawnConfigsData.SpawnConfigPoints(spawnConfig));
                        break;
                    case 3:
                        spawnConfigs.spawnConfigPoints3Char.Add(new SpawnConfigsData.SpawnConfigPoints(spawnConfig));
                        break;
                    case 4:
                        spawnConfigs.spawnConfigPoints4Char.Add(new SpawnConfigsData.SpawnConfigPoints(spawnConfig));
                        break;
                    default:
                        break;
                }

                if (!Save.WriteJSONData(spawnConfigs, relatifSpawnConfigsPath + SettingsManager.saveFileExtension, mkdir:true))
					Debug.LogWarning("Couldn't write ??<idk what tbh>?? to disk !!!");
                spawnConfig = new Vector2[0];
            }
        }

        if(clearSpawnConfig)
        {
            clearSpawnConfig = false;
            spawnConfigs = new SpawnConfigsData();
            if (!Save.WriteJSONData(spawnConfigs, relatifSpawnConfigsPath + SettingsManager.saveFileExtension, mkdir:true))
				Debug.LogWarning("Couldn't write ??<idk what tbh>?? to disk !!!");
            spawnConfig = new Vector2[0];
        }

        if(loadSpawnConfig)
        {
            loadSpawnConfig = false;
            LoadSpawnPoint();
        }

        if (unloadSpawnConfig)
        {
            unloadSpawnConfig = false;
            spawnConfigs = new SpawnConfigsData();
        }
    }

#endif

    #endregion
}

#region SpawnConfigsData

[Serializable]
public class SpawnConfigsData
{
    public List<SpawnConfigPoints> spawnConfigPoints2Char;
    public List<SpawnConfigPoints> spawnConfigPoints3Char;
    public List<SpawnConfigPoints> spawnConfigPoints4Char;

    public SpawnConfigsData()
    {
        spawnConfigPoints2Char = new List<SpawnConfigPoints>();
        spawnConfigPoints3Char = new List<SpawnConfigPoints>();
        spawnConfigPoints4Char = new List<SpawnConfigPoints>();
    }

    public SpawnConfigsData(List<SpawnConfigPoints> spawnConfigPoints2Char, List<SpawnConfigPoints> spawnConfigPoints3Char, List<SpawnConfigPoints> spawnConfigPoints4Char)
    {
        this.spawnConfigPoints2Char = spawnConfigPoints2Char;
        this.spawnConfigPoints3Char = spawnConfigPoints3Char;
        this.spawnConfigPoints4Char = spawnConfigPoints4Char;
    }

    [Serializable]
    public struct SpawnConfigPoints : IEnumerable, IEnumerable<Vector2>
    {
        public Vector2[] points;

        public SpawnConfigPoints(Vector2[] points)
        {
            this.points = points;
        }

        public IEnumerator GetEnumerator() => points.GetEnumerator();

        IEnumerator<Vector2> IEnumerable<Vector2>.GetEnumerator()
        {
            return (IEnumerator<Vector2>)points.GetEnumerator();
        }
    }
}

#endregion