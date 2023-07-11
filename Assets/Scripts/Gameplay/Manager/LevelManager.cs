using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public abstract class LevelManager : MonoBehaviour
{
    protected enum LevelType
    {
        YetisCave,
        MayasTemple,
        IntoTheJungle,
        MaxwellHouse,
        ClockMaker
    }

    #region Fields

    public static LevelManager instance;

    protected PlayerScore[] playersScore;
    protected int currentNbPlayerAlive;
    protected Transform charParent;
    private float lastTimeBeginLevel = -10f;
    private GameObject currentMap;
    private int currentMapIndex;

    [Header("Level Management")]
    [SerializeField] protected LevelType levelType;
    [SerializeField] private string levelName;
    [SerializeField] protected float durationToWaitAtBegining = 3f;
    [SerializeField] protected int nbKillsToWin = 7;
    [SerializeField] protected float waitingTimeAfterLastKill = 2f;
    [SerializeField] protected float scoreMenuDuration = 5f;
    [SerializeField] protected GameObject scoreCanvas;

    [Header("Maps")]
    [SerializeField] private bool suffleMapWhenLevelStart = true;
    [SerializeField] private bool playFirstMapAtLevelStart;
    [SerializeField] protected GameObject[] mapsPrefabs;

    [Header("Level initialiser")]
    [SerializeField] private bool enableBehaviour = true;

    #endregion

    #region Awake/Start/Restart

    protected virtual void Awake()
    {
        if (instance != null && GetType() == instance.GetType())
        {
            Destroy(this);
            return;
        }
        instance = this;
    }

    protected virtual void Start()
    {
        this.InvokeWaitAFrame(nameof(StartLevel));
    }

    protected virtual void StartLevel()
    {
        if (!enableBehaviour)
        {
            EventManager.instance.OnLevelStart(levelName);
            return;
        }

        //Get Map and Spawn config
        GameObject map = GameObject.FindGameObjectWithTag("Map");
        if (map != null)
            Destroy(map);
        object[] playersData = TransitionManager.instance.GetOldSceneData("Selection Char");
        currentNbPlayerAlive = playersData.Length;
        GameObject currentMapPrefaps;
        if (suffleMapWhenLevelStart)
        {
            if (playFirstMapAtLevelStart)
            {
                GameObject firstMap = mapsPrefabs[0];
                mapsPrefabs.Shuffle();
                int indexFirstMap = 0;
                for (int i = 0; i < mapsPrefabs.Length; i++)
                {
                    if (mapsPrefabs[i] == firstMap)
                    {
                        indexFirstMap = i;
                        break;
                    }
                }
                GameObject tmp = mapsPrefabs[0];
                mapsPrefabs[0] = firstMap;
                mapsPrefabs[indexFirstMap] = tmp;
            }
            else
            {
                mapsPrefabs.Shuffle();
            }
        }
        currentMapPrefaps = mapsPrefabs[0];
        currentMapIndex = 0;

        currentMap = Instantiate(currentMapPrefaps);
        List<SpawnConfigsData.SpawnConfigPoints> spawnConfigsData = currentMap.GetComponent<LevelMapData>().LoadSpawnPoint(playersData.Length);
        List<Vector2> spawnPoints = spawnConfigsData.GetRandom().points.ToList();

        //spawn char
        charParent = GameObject.FindGameObjectWithTag("CharsParent").transform;
        charParent.DestroyChildren();
        playersScore = new PlayerScore[playersData.Length];

        SpawnChar(spawnPoints, playersData);

        BlockPlayers();
        Invoke(nameof(ReleasePlayers), durationToWaitAtBegining);

        lastTimeBeginLevel = Time.time;
        EventManager.instance.callbackOnPlayerDeath += OnPlayerDie;
        EventManager.instance.callbackOnPlayerDeathByEnvironnement += OnPlayerDieByEnvironnement;
        PlayerScore.nbKillsToWin = nbKillsToWin;
        EventManager.instance.OnLevelStart(levelName);
    }

    protected virtual void RestartLevel()
    {
        if (!enableBehaviour)
            return;

        CloneParent.cloneParent.DestroyChildren();
        charParent.DestroyChildren();

        object[] playersData = TransitionManager.instance.GetOldSceneData("Selection Char");
        currentMapIndex = (currentMapIndex + 1) % mapsPrefabs.Length;
        currentMap = mapsPrefabs[currentMapIndex];
        List<SpawnConfigsData.SpawnConfigPoints> spawnConfigsData = currentMap.GetComponent<LevelMapData>().LoadSpawnPoint(playersData.Length);
        List<Vector2> spawnPoints = spawnConfigsData.GetRandom().points.ToList();

        SpawnChar(spawnPoints, playersData, false);

        lastTimeBeginLevel = Time.time;
        EventManager.instance.OnLevelStart(levelName);
    }

    private void SpawnChar(List<Vector2> spawnPoints, object[] playersData, bool OnStart = true)
    {
        uint idCount = 0;
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

            if(InputManager.IsAGamepadController(playerData.controllerType))
            {
                InputManager.SetCurrentControllerForGamepad(playerData.playerIndex, playerData.controllerType);
            }
            else
            {
                InputManager.SetCurrentController(playerData.playerIndex, BaseController.Keyboard);
            }

            if (OnStart)
            {
                playersScore[i].playerIndex = playerData.playerIndex;
            }

            tmpGO.GetComponent<Movement>().Teleport(spawnPoint);
        }
    }

    #endregion

    #region Block/Release Player

    private void BlockPlayers()
    {
        foreach (Transform t in charParent)
        {
            t.GetComponent<Movement>().canMove = false;
        }
    }

    public void ReleasePlayers()
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
        currentNbPlayerAlive--;
        PlayerIndex killerIndex = killer.GetComponent<PlayerCommon>().playerIndex;
        for (int i = 0; i < playersScore.Length; i++)
        {
            if(killerIndex == playersScore[i].playerIndex)
            {
                playersScore[i].nbKills++;
                break;
            }
        }

        if(currentNbPlayerAlive <= 1)
        {
            StartCoroutine(LaunchEndMatchMenu());
        }
    }

    private void OnPlayerDieByEnvironnement(GameObject player, GameObject env)
    {
        currentNbPlayerAlive--;

        PlayerIndex playerIndex = player.GetComponent<PlayerCommon>().playerIndex;
        for (int i = 0; i < playersScore.Length; i++)
        {
            if (playerIndex == playersScore[i].playerIndex)
            {
                playersScore[i].nbKills = Mathf.Max(0, playersScore[i].nbKills);
                break;
            }
        }

        if (currentNbPlayerAlive <= 1)
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
        RestartLevel();
    }

    private IEnumerator LaunchEndGameMenu(int indexWinner)
    {
        PauseManager.instance.EnablePause();

        yield return Useful.GetWaitForSeconds(2f);

        PauseManager.instance.DisablePause();

        TransitionManager.instance.LoadScene("Selection Map", null);
    }

    #endregion
}

#region PlayerScore

public struct PlayerScore
{
    public static int nbKillsToWin = 7;
    public int nbKills;
    public PlayerIndex playerIndex;

    public PlayerScore(PlayerIndex playerIndex, int nbKills = 0)
    {
        this.playerIndex = playerIndex;
        this.nbKills = nbKills;
    }
}

#endregion
