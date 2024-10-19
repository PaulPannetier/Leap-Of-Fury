using System;
using UnityEngine;

public class StatisticsManager : MonoBehaviour
{
    public static StatisticsManager instance;

    private const string statsPath = @"/Save/UserSave/stats" + SettingsManager.saveFileExtension;

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

        if(!Save.ReadJSONData(statsPath, out currentData))
        {
            currentData = new StatisticsData(0f, 0f, 0, 0);
            SaveStats();
        }
    }

    private void Start()
    {
        EventManager.instance.callbackOnLevelStart += OnLevelStart;
        EventManager.instance.callbackOnLevelEnd += OnLevelEnd;
        EventManager.instance.callbackOnPlayerDeath += OnPlayerDeath;
        EventManager.instance.callbackOnLevelFinish += OnLevelFinish;
    }

    private void OnLevelStart(string levelName)
    {
        isCurrentGameplayScene = true;
    }

    private void OnLevelEnd(LevelManager.EndLevelData endLevelData)
    {
        isCurrentGameplayScene = false;
        SaveStats();
    }

    private void OnLevelFinish(LevelManager.FinishLevelData finishLevelData)
    {
        currentData.levelComplete++;
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
        Save.WriteJSONDataAsync(currentData, statsPath, SaveCallback).GetAwaiter();
    }

    private void OnDestroy()
    {
        EventManager.instance.callbackOnLevelStart -= OnLevelStart;
        EventManager.instance.callbackOnLevelEnd -= OnLevelEnd;
        EventManager.instance.callbackOnPlayerDeath -= OnPlayerDeath;
        EventManager.instance.callbackOnLevelFinish -= OnLevelFinish;
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        if(resetStats)
        {
            resetStats = false;
            currentData = new StatisticsData();
            SaveStats();
        }
        saveInterval = Mathf.Max(0f, saveInterval);
    }

#endif

    [Serializable]
    public struct StatisticsData
    {
        public float timePlayed, timeOpen;
        public int nbKills, levelComplete;//le nombre de manche terminé

        public StatisticsData(float timePlayed, float timeOpen, int nbKills, int levelComplete)
        {
            this.timePlayed = timePlayed;
            this.timeOpen = timeOpen;
            this.nbKills = nbKills;
            this.levelComplete = levelComplete;
        }
    }
}
