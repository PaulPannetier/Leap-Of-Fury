using System;
using UnityEngine;

public class StatisticsManager : MonoBehaviour
{
    public static StatisticsManager instance;

    private float lastTimeSave;
    private StatisticsData currentData;
    private bool isSaving, isCurrentGameplayScene;

    [SerializeField] private bool resetStats;
    [SerializeField] private bool autoSave = true;
    [SerializeField] private float saveInterval = 60f;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(this);
            return;
        }
        instance = this;
        Save.ReadJSONData("/Save/stats" + SettingsManager.saveFileExtension, out currentData);
    }

    private void Start()
    {
        EventManager.instance.callbackOnLevelStart += OnLevelStart;
        EventManager.instance.callbackOnLevelEnd += OnLevelEnd;
        EventManager.instance.callbackOnPlayerDeath += OnPlayerDeath;
    }

    private void OnLevelStart(string levelName)
    {
        isCurrentGameplayScene = true;
    }

    private void OnLevelEnd(string levelName)
    {
        isCurrentGameplayScene = false;
        SaveStats();
    }

    private void OnPlayerDeath(GameObject player, GameObject killer)
    {
        currentData.nbKills++;
    }

    private void Update()
    {
        if(autoSave && !isSaving && Time.time - lastTimeSave >= saveInterval)
        {
            SaveStats();
        }
        currentData.timeOpen += Time.deltaTime;
        if(isCurrentGameplayScene)
            currentData.timePlayed += Time.deltaTime;
    }

    private void SaveCallback(bool saveSucess)
    {
        if (!saveSucess)
            SaveStats();
        else
        {
            lastTimeSave = Time.time;
            isSaving = false;
        }
    }

    private void SaveStats()
    {
        isSaving = true;
        Save.WriteJSONDataAsync(currentData, "/Save/stats" + SettingsManager.saveFileExtension, SaveCallback).GetAwaiter();
    }

    private void OnValidate()
    {
        if(resetStats)
        {
            currentData = new StatisticsData();
            SaveStats();
            resetStats = false;
        }
        saveInterval = Mathf.Max(0f, saveInterval);
    }

    [Serializable]
    private struct StatisticsData
    {
        public float timePlayed, timeOpen;
        public int nbKills, levelComplete;//le nombre de marche terminé

        public StatisticsData(float timePlayed, float timeOpen, int nbKills, int levelComplete)
        {
            this.timePlayed = timePlayed;
            this.timeOpen = timeOpen;
            this.nbKills = nbKills;
            this.levelComplete = levelComplete;
        }
    }
}
