using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

public class GameStatisticManager : MonoBehaviour
{
#if UNITY_EDITOR

    public static void SetStat(string id, string value)
    {
        GameStatisticManager instance = FindAnyObjectByType<GameStatisticManager>();
        instance.SetStatInternal(id, value);
    }

#endif

    private const string GAME_STAT_PATH = "/Save/GameData/gameStat" + SettingsManager.saveFileExtension;

    public static GameStatisticManager instance;

    private Dictionary<string, string> currentStat;

#if UNITY_EDITOR

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
        if (!Save.ReadJSONData<GameStatData>(GAME_STAT_PATH, out GameStatData gameStatData))
        {
            string errorMessage = $"Can't load GameStatData at the path : {GAME_STAT_PATH}.";
            LogManager.instance.AddLog(errorMessage);
            throw new IOException(errorMessage);
        }

        currentStat = new Dictionary<string, string>();
        foreach (StatData statData in gameStatData.statDatas)
        {
            if(currentStat.ContainsKey(statData.id))
            {
                currentStat[statData.id] = statData.value;
            }
            else
            {
                currentStat.Add(statData.id, statData.value);
            }
        }

#if UNITY_EDITOR

        gameStat = new GameStatData(gameStatData.statDatas);

#endif
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
        if (!Save.WriteJSONData(gameStat, GAME_STAT_PATH))
        {
            print($"Can't write GameStatData at the path : {GAME_STAT_PATH}.");
        }
    }

    private void SetStatInternal(string id, string value)
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

        if (!found)
        {
            gameStat.statDatas.Add(new StatData(id, value));
        }

        WriteStats();
    }

    private void OnValidate()
    {
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
