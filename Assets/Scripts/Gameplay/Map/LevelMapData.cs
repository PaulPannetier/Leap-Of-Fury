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
    private Dictionary<int, Map> _staticPathfindingMap;
    private Dictionary<int, Map> staticPathfindingMap
    {
        get
        {
            if(_staticPathfindingMap == null)
                _staticPathfindingMap = new Dictionary<int, Map>();
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

        staticPathfindingMap = new Dictionary<int, Map>();
        grid = tilemapGo.GetComponent<Grid>();
        Collider2D[] collliders = tilemap.GetComponentsInChildren<Collider2D>();
        List<Collider2D> staticCol = new();
        List<Collider2D> nonStaticCol = new();
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
        //GetStaticPathfindingMap(1);
    }

    #endregion

    #region PathFinding

    private List<MapPoint> GetColliderBlockedPoints(Map map, Collider2D collider)
    {
        if (collider is BoxCollider2D boxCollider)
        {
            return PathFindingBlocker.GetBlockedCellsInRectangle(map, (Vector2)boxCollider.transform.position + boxCollider.offset, boxCollider.size);
        }
        else if (collider is CircleCollider2D circleCollider)
        {
            return PathFindingBlocker.GetBlockedCellsInCircle(map, (Vector2)circleCollider.transform.position + circleCollider.offset, circleCollider.radius);
        }
        Hitbox hitbox = Collision2D.Collider2D.FromUnityCollider2D(collider).ToHitbox();
        return PathFindingBlocker.GetBlockedCellsInRectangle(map, hitbox.center, hitbox.size);
    }

    private Map GetStaticPathfindingMap(int accuracy = 1)
    {
        if (accuracy < 1)
        {
            LogManager.instance.AddLog("Accuracy must be >= 1 in LevelMapData.GetPathfindingMap(int accuracy = 1)", accuracy, "LevelMapData::GetPathfindingMap");
            accuracy = 1;
        }

        if (staticPathfindingMap.TryGetValue(accuracy, out Map map))
            return map;

        Vector2Int mapCellsSize = new Vector2Int((mapSize.x / cellSize.x).Round(), (mapSize.y / cellSize.y).Round());
        int[,] costMap = new int[mapCellsSize.x * accuracy, mapCellsSize.y * accuracy];
        Map res = new Map(costMap, accuracy);

        HashSet<MapPoint> blockedPoints = new HashSet<MapPoint>();
        for (int i = 0; i < staticMapColliders.Length; i++)
        {
            List<MapPoint> colPoints = GetColliderBlockedPoints(res, staticMapColliders[i]);
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

        staticPathfindingMap.Add(accuracy, res);
        return res;
    }

    public Map GetPathfindingMap(int accuracy = 1)
    {
        if (accuracy < 1)
        {
            LogManager.instance.AddLog("Accuracy must be >= 1 in LevelMapData.GetPathfindingMap(int accuracy = 1)", accuracy, "LevelMapData::GetPathfindingMap");
            accuracy = 1;
        }

        Map staticMap = GetStaticPathfindingMap(accuracy);

        List<PathFindingBlocker> blockers = PathFindingBlocker.GetPathFindingBlockers();
        HashSet<MapPoint> blockedPoints = new HashSet<MapPoint>();

        Vector2Int mapCellsSize = new Vector2Int((mapSize.x / cellSize.x).Round(), (mapSize.y / cellSize.y).Round());
        int[,] costMap = new int[mapCellsSize.x * accuracy, mapCellsSize.y * accuracy];
        Map res = new Map(costMap, accuracy);

        foreach (PathFindingBlocker blocker in blockers)
        {
            foreach (MapPoint mapPoint in blocker.GetBlockedCells(res))
            {
                blockedPoints.Add(mapPoint);
            }
        }

        for (int i = 0; i < nonStaticMapColliders.Length; i++)
        {
            List<MapPoint> colPoints = GetColliderBlockedPoints(res, nonStaticMapColliders[i]);
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
                if (staticMap.GetCost(mapPoint) < 0)
                {
                    costMap[x, y] = -1;
                }
                else
                {
                    costMap[x, y] = blockedPoints.Contains(mapPoint) ? -1 : 1;
                }
            }
        }

        return res;
    }

    public MapPoint GetMapPointAtPosition(Map map, in Vector2 position)
    {
        Vector2 origin = PhysicsToric.GetPointInsideBounds(position) + mapSize * 0.5f;
        Vector2 cellSizeWithAccuracy = cellSize / map.accuracy;

        Vector2Int coord = new Vector2Int((int)(origin.x / cellSizeWithAccuracy.x), (int)(origin.y / cellSizeWithAccuracy.y));
        return new MapPoint(coord.x, coord.y);
    }

    public MapPoint GetMapPointAtPosition(in Vector2 position)
    {
        Vector2 origin = PhysicsToric.GetPointInsideBounds(position) + mapSize * 0.5f;
        Vector2Int coord = new Vector2Int((int)(origin.x / cellSize.x), (int)(origin.y / cellSize.y));
        return new MapPoint(coord.x, coord.y);
    }

    public Vector2 GetPositionOfMapPoint(Map map, MapPoint mapPoint)
    {
        Vector2 cellSizeWithAccuracy = cellSize / map.accuracy;
        return new Vector2((mapPoint.X + 0.5f) * cellSizeWithAccuracy.x - (0.5f * mapSize.x), (mapPoint.Y + 0.5f) * cellSizeWithAccuracy.y - (0.5f * mapSize.y));
    }

    public Vector2 GetPositionOfMapPoint(MapPoint mapPoint)
    {
        return new Vector2((mapPoint.X + 0.5f) * cellSize.x - (0.5f * mapSize.x), (mapPoint.Y + 0.5f) * cellSize.y - (0.5f * mapSize.y));
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
        Debug.LogWarning("Can't open SpawnConfigsData's object at the path : " + relatifSpawnConfigsPath + SettingsManager.saveFileExtension);
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

        if(applyCurrentConfig)
        {
            applyCurrentConfig = false;
            Save.WriteJSONData(spawnConfigs, relatifSpawnConfigsPath + SettingsManager.saveFileExtension);
        }

        if (addSpawnConfig && spawnConfig != null && spawnConfig.Length >= 2 && spawnConfig.Length <= 4)
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

            Save.WriteJSONData(spawnConfigs, relatifSpawnConfigsPath + SettingsManager.saveFileExtension);
            spawnConfig = new Vector2[0];
        }
        addSpawnConfig = false;
        if(clearSpawnConfig)
        {
            spawnConfigs = new SpawnConfigsData();
            Save.WriteJSONData(spawnConfigs, relatifSpawnConfigsPath + SettingsManager.saveFileExtension);
            spawnConfig = new Vector2[0];
        }
        clearSpawnConfig = false;

        if(loadSpawnConfig)
        {
            LoadSpawnPoint();
        }
        loadSpawnConfig = false;
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