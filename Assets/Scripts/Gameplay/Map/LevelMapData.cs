using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;
using static SpawnConfigsData;
using System.Linq;

public class LevelMapData : MonoBehaviour
{
    private SpawnConfigsData spawnConfigs;

    [Header("Save spawn Point")]
    [SerializeField] private bool addSpawnConfig;
    [SerializeField] private bool loadSpawnConfig;
    [SerializeField] private bool clearSpawnConfig;
    [SerializeField] private string relatifSpawnConfigsPath;
    [SerializeField] private Vector2[] spawnConfig;

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

    public List<SpawnConfigPoints> LoadSpawnPoint(int nbChar)
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

    private static Dictionary<int, float> convertNbCharToRadius = new Dictionary<int, float>
    {
        { 2, 0.2f },
        { 3, 0.3f },
        { 4, 0.4f }
    };

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        foreach (Vector2 v in spawnConfig)
        {
            Circle.GizmosDraw(v, 0.45f);
        }

        if (spawnConfigs != null)
        {
            Color[] colors = new Color[] { Color.red, Color.yellow, Color.blue, Color.cyan, Color.magenta, Color.grey, Color.white };
            int colorIndex = 0;

            List<SpawnConfigsData.SpawnConfigPoints>[] spawnConfigsPerChars = new List<SpawnConfigsData.SpawnConfigPoints>[3]
            {
                spawnConfigs.spawnConfigPoints2Char, spawnConfigs.spawnConfigPoints3Char, spawnConfigs.spawnConfigPoints4Char
            };

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
    }

    private void OnValidate()
    {
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
    public struct SpawnConfigPoints : IEnumerable,IEnumerable<Vector2>
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