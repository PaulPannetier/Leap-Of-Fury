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
            if(_currentMap == null)
            {
                _currentMap = GameObject.FindGameObjectWithTag("Map").GetComponent<LevelMapData>();
            }
#endif
            return _currentMap;
        }
        private set
        {
            _currentMap = value;
        }
    }

    private SpawnConfigsData spawnConfigs;
    private Grid grid;
    private Collider2D[] mapColliders;

#if UNITY_EDITOR

    [SerializeField] private bool drawGizmos = true;
    [Header("Save spawn Point")]
    [SerializeField] private bool addSpawnConfig;
    [SerializeField] private bool loadSpawnConfig;
    [SerializeField] private bool clearSpawnConfig;
    [SerializeField] private Vector2[] spawnConfig;

#endif

    [SerializeField] private string relatifSpawnConfigsPath;
    [SerializeField] private Transform[] collidersGameObject; 

    public Vector2 mapSize = new Vector2(32f, 18f);

    public Vector2 cellSize
    {
        get
        {
            if(grid == null)
            {
                grid = GetComponentInChildren<Grid>();
            }
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
        grid = tilemapGo.GetComponent<Grid>();

        List<Collider2D> mapColliders = new List<Collider2D>();
        foreach (Transform t in collidersGameObject)
        {
            foreach (Collider2D col in t.GetComponents<Collider2D>())
            {
                if(!mapColliders.Contains(col))
                {
                    mapColliders.Add(col);
                }
            }
        }
        this.mapColliders = mapColliders.ToArray();

        EventManager.instance.OnMapChanged(this);
    }

    #endregion

    #region PathFinding

    public Map GetPathfindingMap(int accuracy = 1)
    {
        if (accuracy < 1)
        {
            accuracy = 1;
            LogManager.instance.WriteLog("Accuracy must be >= 1 in LevelMapData.GetPathfindingMap(int accuracy = 1)", accuracy);
        }

        List<PathFindingBlocker> blockers = PathFindingBlocker.GetPathFindingBlockers();

        List<MapPoint> blockedPoints = new List<MapPoint>();

        Vector2Int mapCellsSize = new Vector2Int((mapSize.x / cellSize.x).Round(), (mapSize.y / cellSize.y).Round());
        int[,] costMap = new int[mapCellsSize.x * accuracy, mapCellsSize.y * accuracy];
        Map res = new Map(costMap, accuracy);

        foreach (PathFindingBlocker blocker in blockers)
        {
            foreach (MapPoint mapPoint in blocker.GetBlockedCells(res))
            {
                if(!blockedPoints.Contains(mapPoint))
                {
                    blockedPoints.Add(mapPoint);
                }
            }
        }

        Vector2 offset = -0.5f * mapSize;
        Vector2 cellCenter;
        Vector2 cellSizeWithAccuracy = cellSize / accuracy;

        for (int x = 0; x < costMap.GetLength(0); x++)
        {
            for (int y = 0; y < costMap.GetLength(1); y++)
            {
                cellCenter = new Vector2(offset.x + (x + 0.5f) * cellSizeWithAccuracy.x, offset.y + (y + 0.5f) * cellSizeWithAccuracy.y);
                costMap[x, y] = GetCostAtPoint(cellCenter, new MapPoint(x, y));
            }
        }

        int GetCostAtPoint(in Vector2 point, MapPoint mapPoint)
        {
            if(blockedPoints.Contains(mapPoint))
            {
                return -1;
            }

            foreach (Collider2D col in mapColliders)
            {
                if(col.OverlapPoint(point))
                {
                    return -1;
                }
            }
            return 1;
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

    private static Dictionary<int, float> convertNbCharToRadius = new Dictionary<int, float>
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
                    Gizmos.color = colors[colorIndex];
                    foreach (Vector2 point in points)
                    {
                        Circle.GizmosDraw(point, convertNbCharToRadius[points.points.Length]);
                    }
                    colorIndex = (colorIndex + 1) % colors.Length;
                }
            }
        }

        Gizmos.color = Color.white;
        Hitbox.GizmosDraw(Vector2.zero, mapSize * cellSize);
    }

    private void OnValidate()
    {
        currentMap = this;
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