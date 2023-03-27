using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    #region Fields

    public static LevelManager instance;

    private enum LevelType
    {
        YetisCave,
        MayasTemple,
        IntoTheJungle,
        MaxwellHouse
    }

    private PlayerScore[] playersScore;
    private int currentNbPlayer;
    private SpawnConfigsData charsSpawnPoints;
    private uint idCount = 0;
    private Transform charParent;
    private float lastTimeBeginLevel = -10f;

    [Header("Level Management")]
    [SerializeField] private LevelType levelType;
    [SerializeField] private float durationToWaitAtBegining = 3f;
    [SerializeField] private int nbKillsToWin = 7;
    [SerializeField] private float waitingTimeAfterLastKill = 2f;
    [SerializeField] private float scoreMenuDuration = 5f;
    [SerializeField] private GameObject scoreCanvas;

    [Header("Level initialiser")]
    [SerializeField] private bool enableBehaviour = true;

    [Header("Spawn Configurations")]
    [SerializeField] private Vector2[] spawnConfig;
    [SerializeField] private bool saveSpawnConfig = false;
    [SerializeField] private bool clearSpawnConfig = false;

    [Header("YetisCave")]
    [SerializeField] private float YetisCaveVar;

    [Header("MayasTemple")]
    [SerializeField] private float MayasTempleVar;

    [Header("IntoTheJungle")]
    [SerializeField] private GameObject playerLightPrefabs;

    [Header("MaxwellHouse")]
    [SerializeField] private float MaxwellHouseVar;

    #endregion

    #region Awake/Start/Restart

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
        if (!enableBehaviour)
            return;

        EventManager.instance.OnLevelStart(SceneManager.GetActiveScene().name);
        lastTimeBeginLevel = Time.time;
        InitLevelAll();

        switch (levelType)
        {
            case LevelType.YetisCave:
                InitYetisCave();
                break;
            case LevelType.MayasTemple:
                InitMayasTemple();
                break;
            case LevelType.IntoTheJungle:
                InitIntoTheJungle();
                break;
            case LevelType.MaxwellHouse:
                InitMaxwellHouse();
                break;
            default:
                break;
        }

        EventManager.instance.callbackOnPlayerDeath += OnPlayerDie;
        EventManager.instance.callbackOnPlayerDeathByEnvironnement += OnPlayerDieByEnvironnement;
        PlayerScore.nbKillsToWin = nbKillsToWin;
    }

    private void Restart()
    {
        if (!enableBehaviour)
            return;

        EventManager.instance.OnLevelRestart(SceneManager.GetActiveScene().name);
        lastTimeBeginLevel = Time.time;
        CloneParent.cloneParent.DestroyChildren();
        charParent.DestroyChildren();
        InitLevelAll();
        switch (levelType)
        {
            case LevelType.YetisCave:
                InitYetisCave();
                break;
            case LevelType.MayasTemple:
                InitMayasTemple();
                break;
            case LevelType.IntoTheJungle: 
                InitIntoTheJungle();
                break;
            case LevelType.MaxwellHouse:
                InitMaxwellHouse();
                break;
            default:
                break;
        }
    }

    #endregion

    #region Init

    private void InitLevelAll()
    {
        object[] playersData = TransitionManager.instance.GetOldSceneData("Selection Char");

        LoadSpawnPoint();
        List<Vector2> spawnPoints = charsSpawnPoints.spawnConfigPoints.GetRandom().points.ToList();
        idCount = 0;

        currentNbPlayer = playersData.Length;
        if(playersScore == default(PlayerScore[]))
        {
            playersScore = new PlayerScore[currentNbPlayer];
        }

        charParent = GameObject.FindGameObjectWithTag("CharsParent").transform;
        for (int i = 0; i < playersData.Length; i++)
        {
            //get random position
            Vector2 spawnPoint = spawnPoints.GetRandom();
            spawnPoints.Remove(spawnPoint);

            CharSelectorItemData playerData = (CharSelectorItemData)playersData[i];
            GameObject tmpGO = Instantiate(playerData.charPrefabs, charParent);
            PlayerCommon tmpPC = tmpGO.GetComponent<PlayerCommon>();
            tmpPC.playerIndex = playerData.playerIndex;
            tmpPC.id = idCount;
            idCount++;
            tmpGO.GetComponent<CustomPlayerInput>().controllerType = playerData.controllerType;

            CustomInput.LoadDefaultController(playerData.playerIndex, playerData.controllerType);

            playersScore[i].playerIndex = playerData.playerIndex;

            tmpGO.GetComponent<Movement>().Teleport(spawnPoint);
        }

        BlockPlayer();
    }

    private void InitYetisCave()
    {

    }

    private void InitMayasTemple()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Char");
        foreach (GameObject player in players)
        {
            FightController fc = player.GetComponent<FightController>();
            fc.enableAttackStrong = fc.enableAttackWeak = false;
        }

        Totem.ResetAllTotems();
    }

    private void InitIntoTheJungle()
    {
        GameObject[] chars = GameObject.FindGameObjectsWithTag("Char");
        foreach (GameObject charGO in chars)
        {
            GameObject lightTouchFloor = Instantiate(playerLightPrefabs, charGO.transform.position, Quaternion.identity, charGO.transform);
            lightTouchFloor.transform.localPosition = Vector3.zero;
        }
    }

    private void InitMaxwellHouse()
    {

    }

    private void LoadSpawnPoint()
    {
        string path = string.Empty;
        switch (levelType)
        {
            case LevelType.YetisCave:
                path = @"/Save/YetisCaveSpawnPoints" + SettingsManager.saveFileExtension;
                break;
            case LevelType.MayasTemple:
                path = @"/Save/MayasTempleSpawnPoints" + SettingsManager.saveFileExtension;
                break;
            case LevelType.IntoTheJungle:
                path = @"/Save/IntoTheJungleSpawnPoints" + SettingsManager.saveFileExtension;
                break;
            case LevelType.MaxwellHouse:
                path = @"/Save/MaxwellHousePoints" + SettingsManager.saveFileExtension;
                break;
            default:
                break;
        }

        Save.ReadJSONData(path, out charsSpawnPoints);
    }

    private void BlockPlayer()
    {
        foreach (Transform t in charParent)
        {
            t.GetComponent<Movement>().canMove = false;
        }
    }

    public void ReleasePlayer()
    {
        StopCoroutine(nameof(ReleasePlayerCorout));
        StartCoroutine(ReleasePlayerCorout());
    }

    private IEnumerator ReleasePlayerCorout()
    {
        while (Time.time - lastTimeBeginLevel < durationToWaitAtBegining)
        {
            yield return null;
        }

        foreach (Transform t in charParent)
        {
            t.GetComponent<Movement>().canMove = true;
        }
    }

    #endregion

    #region OnPlayerDie

    private void OnPlayerDie(GameObject player, GameObject killer)
    {
        currentNbPlayer--;
        PlayerIndex killerIndex = killer.GetComponent<PlayerCommon>().playerIndex;
        for (int i = 0; i < playersScore.Length; i++)
        {
            if(killerIndex == playersScore[i].playerIndex)
            {
                playersScore[i].nbKills++;
                break;
            }
        }

        if(currentNbPlayer <= 1)
        {
            StartCoroutine(LaunchEndMatchMenu());
        }
    }

    private void OnPlayerDieByEnvironnement(GameObject player, GameObject env)
    {
        currentNbPlayer--;

        PlayerIndex playerIndex = player.GetComponent<PlayerCommon>().playerIndex;
        for (int i = 0; i < playersScore.Length; i++)
        {
            if (playerIndex == playersScore[i].playerIndex)
            {
                playersScore[i].nbKills = Mathf.Max(0, playersScore[i].nbKills);
                break;
            }
        }

        if (currentNbPlayer <= 1)
        {
            StartCoroutine(LaunchEndMatchMenu());
        }
    }

    #endregion

    #region Menu

    private IEnumerator LaunchEndMatchMenu()
    {
        yield return Useful.GetWaitForSeconds(waitingTimeAfterLastKill);
        //TODO : lancer music fin de menu
        int indexPlayerWin = -1;
        for (int i = 0; i < playersScore.Length; i++)
        {
            if(playersScore[i].nbKills >= nbKillsToWin)
            {
                indexPlayerWin = i;
                break;
            }
        }

        if(indexPlayerWin >= 0)
            StartCoroutine(LaunchEndGameMenu(indexPlayerWin));
        else
            StartCoroutine(LaunchScoreGameMenu());
    }

    private IEnumerator LaunchScoreGameMenu()
    {
        PauseManager.instance.EnablePause();

        scoreCanvas.GetComponent<ScoreMenu>().SetPlayersKills(playersScore, scoreMenuDuration * 0.5f);

        float time = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - time < scoreMenuDuration)
        {
            yield return null;
        }

        scoreCanvas.GetComponent<ScoreMenu>().DisableVisual();
        PauseManager.instance.DisablePause();
        Restart();
    }

    private IEnumerator LaunchEndGameMenu(int indexWinner)
    {
        PauseManager.instance.EnablePause();

        yield return Useful.GetWaitForSeconds(2f);

        PauseManager.instance.DisablePause();

        TransitionManager.instance.LoadScene("Selection Map", null);
    }

    #endregion

    #region SpawnConfigsData

    [System.Serializable]
    public class SpawnConfigsData : IEnumerable<SpawnConfigsData.SpawnConfigPoints>
    {
        public List<SpawnConfigPoints> spawnConfigPoints;

        public SpawnConfigsData()
        {
            spawnConfigPoints = new List<SpawnConfigPoints>();
        }

        public SpawnConfigsData(List<SpawnConfigPoints> spawnConfigPoints)
        {
            this.spawnConfigPoints = spawnConfigPoints;
        }

        public IEnumerator<SpawnConfigPoints> GetEnumerator() => spawnConfigPoints.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => spawnConfigPoints.GetEnumerator();

        [System.Serializable]
        public struct SpawnConfigPoints : IEnumerable
        {
            public Vector2[] points;

            public SpawnConfigPoints(Vector2[] points)
            {
                this.points = points;
            }

            public IEnumerator GetEnumerator() => points.GetEnumerator();
        }
    }

    #endregion

    #region OnValidate/OnDrawGizmos

    private void OnValidate()
    {
        if (saveSpawnConfig && spawnConfig != null && spawnConfig.Length > 0)
        {
            string path = string.Empty;
            switch (levelType)
            {
                case LevelType.YetisCave:
                    path = @"/Save/YetisCaveSpawnPoints" + SettingsManager.saveFileExtension;
                    break;
                case LevelType.MayasTemple:
                    path = @"/Save/MayasTempleSpawnPoints" + SettingsManager.saveFileExtension;
                    break;
                case LevelType.IntoTheJungle:
                    path = @"/Save/IntoTheJungleSpawnPoints" + SettingsManager.saveFileExtension;
                    break;
                default:
                    break;
            }

            //Save info
            if (!Save.ReadJSONData<SpawnConfigsData>(path, out charsSpawnPoints))
            {
                charsSpawnPoints = new SpawnConfigsData();
            }

            charsSpawnPoints.spawnConfigPoints.Add(new SpawnConfigsData.SpawnConfigPoints(spawnConfig));

            Save.WriteJSONData(charsSpawnPoints, path);
        }
        saveSpawnConfig = false;
        if(clearSpawnConfig)
        {
            string path = string.Empty;
            switch (levelType)
            {
                case LevelType.YetisCave:
                    path = @"/Save/YetisCaveSpawnPoints" + SettingsManager.saveFileExtension;
                    break;
                case LevelType.MayasTemple:
                    path = @"/Save/MayasTempleSpawnPoints" + SettingsManager.saveFileExtension;
                    break;
                case LevelType.IntoTheJungle:
                    path = @"/Save/IntoTheJungleSpawnPoints" + SettingsManager.saveFileExtension;
                    break;
                default:
                    break;
            }
            charsSpawnPoints = new SpawnConfigsData();
            Save.WriteJSONData(charsSpawnPoints, path);
        }
        clearSpawnConfig = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        foreach (Vector2 v in spawnConfig)
        {
            Circle.GizmosDraw(v, 0.45f);
        }

        if (charsSpawnPoints != null)
        {
            Color[] colors = new Color[6] { Color.red, Color.yellow, Color.blue, Color.cyan, Color.magenta, Color.grey };
            int colorIndex = 0;
            foreach (SpawnConfigsData.SpawnConfigPoints points in charsSpawnPoints)
            {
                Gizmos.color = colors[colorIndex];
                foreach (Vector2 v in points)
                {
                    Circle.GizmosDraw(v, 0.4f);
                }
                colorIndex = (colorIndex + 1) % colors.Length;
            }
        }
    }

    #endregion
}

#region PlayerScore

public struct PlayerScore
{
    public static int nbKillsToWin = 7;
    public int nbKills;
    public PlayerIndex playerIndex;

    public PlayerScore(in PlayerIndex playerIndex, in int nbKills = 0)
    {
        this.playerIndex = playerIndex;
        this.nbKills = nbKills;
    }
}

#endregion
