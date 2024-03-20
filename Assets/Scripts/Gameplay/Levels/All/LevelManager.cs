using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LevelManager : MonoBehaviour
{
    #region Fields

    public static LevelManager instance;

    protected PlayerScore[] playersScore;
    protected int currentNbPlayerAlive;
    protected Transform charParent;
    private float lastTimeBeginLevel = -10f;
    private GameObject currentMap;
    private int currentMapIndex;
    protected SelectionMapOldSceneData selectionMapOldSceneData;

    [Header("Level Management")]
    [SerializeField] private string levelName;
    [SerializeField] protected float durationToWaitAtBegining = 3f;
    [SerializeField] protected int nbKillsToWin = 7;
    [SerializeField] protected float waitingTimeAfterLastKill = 2f;

    [Header("Maps")]
    [SerializeField] private bool suffleMapWhenLevelStart = true;
    [SerializeField] private bool playFirstMapAtLevelStart;
    [SerializeField] protected GameObject[] mapsPrefabs;

    #if UNITY_EDITOR

    [Header("Level initialiser")]
    public bool enableBehaviour = true;

    #endif

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

    #region Start/Restart level

    protected virtual void Start()
    {
        this.InvokeWaitAFrame(nameof(StartLevel));
    }

    protected virtual void StartLevel()
    {
 #if UNITY_EDITOR

        if (!enableBehaviour)
        {
            DEBUG_SetCharControlInputsAndOldSceneData();
            currentMap = GameObject.FindGameObjectWithTag("Map");
            HandleCharPosAndCallback();
        }
        else
        {
            NormalStartLevel();
        }

#else

        NormalStartLevel();

#endif

        void NormalStartLevel()
        {
            FillCharData();

            HandleMap();

            InstanciateChar();

            HandleCharPosAndCallback();
        }

        void FillCharData()
        {
            SelectionMapOldSceneData oldSceneData = TransitionManager.instance.GetOldSceneData("Selection Map") as SelectionMapOldSceneData;
            currentNbPlayerAlive = oldSceneData.charData.Length;
            selectionMapOldSceneData = oldSceneData;
        }

        void HandleMap()
        {
            //Get Map and Spawn config
            GameObject map = GameObject.FindGameObjectWithTag("Map");
            if (map != null)
                Destroy(map);

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
        }

        void InstanciateChar()
        {
            List<SpawnConfigsData.SpawnConfigPoints> spawnConfigsData = currentMap.GetComponent<LevelMapData>().LoadSpawnPoint(selectionMapOldSceneData.charData.Length);
            List<Vector2> spawnPoints = spawnConfigsData.GetRandom().points.ToList();

            //spawn char
            charParent = GameObject.FindGameObjectWithTag("CharsParent").transform;
            charParent.DestroyChildren();
            playersScore = new PlayerScore[selectionMapOldSceneData.charData.Length];
            for (int i = 0; i < playersScore.Length; i++)
            {
                playersScore[i].playerIndex = selectionMapOldSceneData.charData[i].playerIndex;
                playersScore[i].nbKills = 0;
            }

            SpawnChar(spawnPoints);
        }

        void HandleCharPosAndCallback()
        {
            lastTimeBeginLevel = Time.time;

            BlockPlayers();

            EventManager.instance.callbackOnPlayerDeath += OnPlayerDie;
            EventManager.instance.callbackOnPlayerDeathByEnvironnement += OnPlayerDieByEnvironnement;
            PlayerScore.nbKillsToWin = nbKillsToWin;
            StartCoroutine(CallbackOnLevelStart());
        }
    }

    private IEnumerator CallbackOnLevelStart()
    {
        yield return null;
        yield return null;
        EventManager.instance.OnLevelStart(levelName);
    }

    public void OnEndDisplayEndMenu()
    {
        PauseManager.instance.DisablePause();
        RestartLevel();
    }

    protected virtual void RestartLevel()
    {
        charParent.DestroyChildren();

        CloneParent.cloneParent.DestroyChildren();
        currentMapIndex = (currentMapIndex + 1) % mapsPrefabs.Length;
        Destroy(currentMap);
        currentMap = Instantiate(mapsPrefabs[currentMapIndex]);
        List<SpawnConfigsData.SpawnConfigPoints> spawnConfigsData = currentMap.GetComponent<LevelMapData>().LoadSpawnPoint(selectionMapOldSceneData.charData.Length);
        List<Vector2> spawnPoints = spawnConfigsData.GetRandom().points.ToList();

        SpawnChar(spawnPoints);

        lastTimeBeginLevel = Time.time;
        BlockPlayers();

        StartCoroutine(CallbackOnLevelRestart());
    }

    private IEnumerator CallbackOnLevelRestart()
    {
        yield return null;
        yield return null;
        EventManager.instance.OnLevelRestart(levelName);
    }

    private void SpawnChar(List<Vector2> spawnPoints, bool randomiseSpawnPoints = true)
    {
        uint idCount = 0;
        for (int i = 0; i < selectionMapOldSceneData.charData.Length; i++)
        {
            //get random position
            Vector2 spawnPoint = randomiseSpawnPoints ? spawnPoints.GetRandom() : spawnPoints[0];
            spawnPoints.Remove(spawnPoint);

            CharData playerData = selectionMapOldSceneData.charData[i];
            GameObject tmpGO = Instantiate(playerData.charPrefabs, charParent);
            PlayerCommon playerCommon = tmpGO.GetComponent<PlayerCommon>();
            playerCommon.playerIndex = playerData.playerIndex;
            playerCommon.id = idCount;
            idCount++;
            tmpGO.GetComponent<CustomPlayerInput>().controllerType = playerData.controllerType;

            if(InputManager.IsAGamepadController(playerData.controllerType))
            {
                InputManager.SetCurrentControllerForGamepad(playerData.playerIndex, playerData.controllerType, false);
            }
            else
            {
                InputManager.SetCurrentController(playerData.playerIndex, BaseController.Keyboard, false);
            }

            for (int j = 0; j < playersScore.Length; j++)
            {
                if (playersScore[j].playerIndex == playerData.playerIndex)
                {
                    playersScore[j].playerCommon = playerCommon;
                    break;
                }
            }

            tmpGO.GetComponent<Movement>().Teleport(spawnPoint);
        }
    }

#if UNITY_EDITOR

    private void DEBUG_SetCharControlInputsAndOldSceneData()
    {
        charParent = GameObject.FindGameObjectWithTag("CharsParent").transform;

        PlayerIndex[] playerIndexes = new PlayerIndex[4]
        {
            PlayerIndex.One, PlayerIndex.Two, PlayerIndex.Three, PlayerIndex.Four
        };

        ControllerType[] controllerTypes = new ControllerType[4]
        {
            ControllerType.Keyboard, ControllerType.Gamepad1, ControllerType.Gamepad2, ControllerType.Gamepad3
        };

        int nbChar = 0;
        List<CharData> lstCharData = new List<CharData>();
        List<Vector2> spawnPoint = new List<Vector2>();
        for (int i = 0; i < charParent.childCount; i++)
        {
            Transform charI = charParent.GetChild(i);
            if(!charI.gameObject.activeSelf)
                continue;

            lstCharData.Add(new CharData(playerIndexes[nbChar], controllerTypes[nbChar], charI.GetComponent<PlayerCommon>().prefabs));
            spawnPoint.Add(charI.transform.position);
            nbChar++;
        }

        charParent.DestroyChildren();

        selectionMapOldSceneData = new SelectionMapOldSceneData(lstCharData.ToArray());
        playersScore = new PlayerScore[selectionMapOldSceneData.charData.Length];
        for (int i = 0; i < playersScore.Length; i++)
        {
            playersScore[i].playerIndex = selectionMapOldSceneData.charData[i].playerIndex;
            playersScore[i].nbKills = 0;
        }

        SpawnChar(spawnPoint, false);
    }

#endif

    #endregion

    #endregion

    #region Block/Release Player

    private void BlockPlayers()
    {
        foreach (Transform t in charParent)
        {
            t.GetComponent<Movement>().DisableMovement(durationToWaitAtBegining);
        }
    }

    #endregion

    #region OnPlayerDie

    protected void OnPlayerDie(GameObject player, GameObject killer)
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
            OnEndLevelTurn();
        }
    }

    protected void OnPlayerDieByEnvironnement(GameObject player, GameObject env)
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
            OnEndLevelTurn();
        }
    }

    protected void OnEndLevelTurn()
    {
        StartCoroutine(EndLevelCorout());
    }

    private IEnumerator EndLevelCorout()
    {
        float time = 0f;

        while (time < waitingTimeAfterLastKill)
        {
            yield return null;
            time += Time.deltaTime;

            while (PauseManager.instance.isPauseEnable)
            {
                yield return null; ;
            }
        }

        EndLevel();
    }

    private void EndLevel()
    {
        List<PlayerScore> playerWin = new List<PlayerScore>();

        for (int i = 0; i < playersScore.Length; i++)
        {
            if (playersScore[i].nbKills >= PlayerScore.nbKillsToWin)
            {
                playerWin.Add(playersScore[i]);
            }
        }

        if (playerWin.Count == 1)
        {
            EventManager.instance.OnLevelFinish(new FinishLevelData(levelName, playersScore));
            return;
        }
        else if (playerWin.Count > 1)
        {
            for (int i = 0; i < playersScore.Length; i++)
            {
                if (playersScore[i].nbKills >= PlayerScore.nbKillsToWin)
                {
                    playersScore[i].nbKills = PlayerScore.nbKillsToWin - 1;
                }
            }
        }

        PauseManager.instance.EnablePause();
        EventManager.instance.OnLevelEnd(new EndLevelData(levelName, playersScore));
    }

    #endregion

    #region OnDestroy

    protected virtual void OnDestroy()
    {
        EventManager.instance.callbackOnPlayerDeath -= OnPlayerDie;
        EventManager.instance.callbackOnPlayerDeathByEnvironnement -= OnPlayerDieByEnvironnement;
    }

    #endregion

    #region Struts

    public struct PlayerScore
    {
        public static int nbKillsToWin = 7;
        public int nbKills;
        public PlayerIndex playerIndex;
        public PlayerCommon playerCommon;

        public PlayerScore(PlayerIndex playerIndex, int nbKills, PlayerCommon playerCommon)
        {
            this.playerIndex = playerIndex;
            this.nbKills = nbKills;
            this.playerCommon = playerCommon;
        }
    }

    public struct EndLevelData
    {
        public PlayerScore[] playersScore;
        public string levelName;

        public EndLevelData(string levelName, PlayerScore[] playersScore)
        {
            this.levelName = levelName;
            this.playersScore = playersScore;
        }
    }

    public struct FinishLevelData
    {
        public PlayerScore[] playersScore;
        public string levelName;

        public FinishLevelData(string levelName, PlayerScore[] playersScore)
        {
            this.levelName = levelName;
            this.playersScore = playersScore;
        }
    }
    #endregion
}
