using UnityEngine;
using System;
using System.Collections.Generic;

public class GameStatisticManager : MonoBehaviour
{
    public static GameStatisticManager instance;

    private Dictionary<string, string> currentStat;

#if UNITY_EDITOR

    [SerializeField] private bool refresh;
    [SerializeField] private GameStatData gameStat;
    [SerializeField] private bool writeCurrentStats;
    [SerializeField] private bool loadStats;

#endif

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(this);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        LoadStat();
    }

    private void LoadStat()
    {
        string path = "/Save/gameStat" + SettingsManager.saveFileExtension;
        if (!Save.ReadJSONData<GameStatData>(path, out GameStatData gameStatData))
        {
            LogManager.instance.WriteLog("Can't load GameStatData at the path : " + path + ".");
            throw new Exception("Can't load GameStatData at the path : " + path + ".");
        }

        currentStat = new Dictionary<string, string>();
        foreach (StatData statData in gameStatData.statDatas)
        {
            currentStat.Add(statData.id, statData.value);
        }
    }

    public string GetStat(string id)
    {
        if(currentStat.TryGetValue(id, out string result))
            return result;
        return string.Empty;
    }


#if UNITY_EDITOR

    public void WriteStats()
    {
        string path = "/Save/gameStat" + SettingsManager.saveFileExtension;
        if (!Save.WriteJSONData(gameStat, path))
        {
            print("Can't write GameStatData at the path : " + path + ".");
        }
    }

    public void SetStat(string id, string value)
    {
        LoadStat();

        bool found = false;
        for (int i = 0; i < gameStat.statDatas.Count; i++)
        {
            if (gameStat.statDatas[i].id == id)
            {
                gameStat.statDatas[i] = new StatData(id, value);
                found = true;
                break;
            }
        }

        if(!found)
        {
            gameStat.statDatas.Add(new StatData(id, value));
        }

        WriteStats();
    }

    private void OnValidate()
    {
        if(refresh)
            refresh = false;

        instance = this;

        if (writeCurrentStats)
        {
            writeCurrentStats = false;
            WriteStats();
        }
        if(loadStats)
        {
            loadStats = false;
            LoadStat();
        }

        for (int i = 0; i < gameStat.statDatas.Count; i++)
        {
            gameStat.statDatas[i] = new StatData(gameStat.statDatas[i].id.Replace(" ", string.Empty), gameStat.statDatas[i].value);
        }
    }

#endif

    #region Custom Struct

    [Serializable]
    private struct StatData
    {
        public string id;
        public string value;

        public StatData(string id, string value)
        {
            this.id = id;
            this.value = value;
        }
    }

    [Serializable]
    private struct GameStatData
    {
        public List<StatData> statDatas;

        public GameStatData(List<StatData> statDatas)
        {
            this.statDatas = statDatas;
        }
    }

    #endregion
}
