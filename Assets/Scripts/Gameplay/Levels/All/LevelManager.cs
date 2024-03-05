using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LevelManager : MonoBehaviour
{
    protected enum LevelType
    {
        YetisCave,
        MayasTemple,
        IntoTheJungle,
        MaxwellHouse,
        ClockMaker,
        ThePlain
    }

    #region Fields

    public static LevelManager instance;

    protected PlayerScore[] playersScore;
    protected int currentNbPlayerAlive;
    protected Transform charParent;
    private float lastTimeBeginLevel = -10f;
    private GameObject currentMap;
    private int currentMapIndex;
    protected SelectionCharOldSceneData selectionCharOldSceneData;

    [Header("Level Management")]
    [SerializeField] protected LevelType levelType;
    [SerializeField] private string levelName;
    [SerializeField] protected float durationToWaitAtBegining = 3f;
    [SerializeField] protected int nbKillsToWin = 7;
    [SerializeField] protected float waitingTimeAfterLastKill = 2f;
    [SerializeField] protected ScoreMenu scoreMenu;
    [SerializeField] protected EndLevelMenu endLevelMenu;

    [Header("Maps")]
    [SerializeField] private bool suffleMapWhenLevelStart = true;
    [SerializeField] private bool playFirstMapAtLevelStart;
    [SerializeField] protected GameObject[] mapsPrefabs;

    #if UNITY_EDITOR

    [Header("Level initialiser")]
    public bool enableBehaviour = true;
    [SerializeField] private bool _DEBUGendLevel;

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
            SelectionCharOldSceneData oldSceneData = TransitionManager.instance.GetOldSceneData("Selection Char") as SelectionCharOldSceneData;
            currentNbPlayerAlive = oldSceneData.charData.Length;
            selectionCharOldSceneData = oldSceneData;
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
            List<SpawnConfigsData.SpawnConfigPoints> spawnConfigsData = currentMap.GetComponent<LevelMapData>().LoadSpawnPoint(selectionCharOldSceneData.charData.Length);
            List<Vector2> spawnPoints = spawnConfigsData.GetRandom().points.ToList();

            //spawn char
            charParent = GameObject.FindGameObjectWithTag("CharsParent").transform;
            charParent.DestroyChildren();
            playersScore = new PlayerScore[selectionCharOldSceneData.charData.Length];

            SpawnChar(spawnPoints);
        }

        void HandleCharPosAndCallback()
        {
            BlockPlayers();
            Invoke(nameof(ReleasePlayers), durationToWaitAtBegining);

            playersScore = new PlayerScore[selectionCharOldSceneData.charData.Length];
            for (int i = 0; i < playersScore.Length; i++)
            {
                playersScore[i].playerIndex = selectionCharOldSceneData.charData[i].playerIndex;
                playersScore[i].nbKills = 0;
            }

            lastTimeBeginLevel = Time.time;
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

    protected virtual void RestartLevel()
    {
        charParent.DestroyChildren();

        CloneParent.cloneParent.DestroyChildren();
        currentMapIndex = (currentMapIndex + 1) % mapsPrefabs.Length;
        Destroy(currentMap);
        currentMap = Instantiate(mapsPrefabs[currentMapIndex]);
        List<SpawnConfigsData.SpawnConfigPoints> spawnConfigsData = currentMap.GetComponent<LevelMapData>().LoadSpawnPoint(selectionCharOldSceneData.charData.Length);
        List<Vector2> spawnPoints = spawnConfigsData.GetRandom().points.ToList();

        SpawnChar(spawnPoints);

        lastTimeBeginLevel = Time.time;
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
        for (int i = 0; i < selectionCharOldSceneData.charData.Length; i++)
        {
            //get random position
            Vector2 spawnPoint = randomiseSpawnPoints ? spawnPoints.GetRandom() : spawnPoints[0];
            spawnPoints.Remove(spawnPoint);

            SelectionCharOldSceneData.CharData playerData = selectionCharOldSceneData.charData[i];
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
        List<SelectionCharOldSceneData.CharData> lstCharData = new List<SelectionCharOldSceneData.CharData>();
        List<Vector2> spawnPoint = new List<Vector2>();
        for (int i = 0; i < charParent.childCount; i++)
        {
            Transform charI = charParent.GetChild(i);
            if(!charI.gameObject.activeSelf)
                continue;

            lstCharData.Add(new SelectionCharOldSceneData.CharData(playerIndexes[nbChar], controllerTypes[nbChar], charI.GetComponent<PlayerCommon>().prefabs));
            spawnPoint.Add(charI.transform.position);
            nbChar++;
        }

        charParent.DestroyChildren();

        selectionCharOldSceneData = new SelectionCharOldSceneData(lstCharData.ToArray());
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

    #endregion

    #region Menu

    protected void OnEndLevelTurn()
    {
        StartCoroutine(OnEndLevelTurnCorout());
    }

    protected IEnumerator OnEndLevelTurnCorout()
    {
        yield return Useful.GetWaitForSeconds(waitingTimeAfterLastKill);
        List<int> indexesPlayerWin = new List<int>();

        for (int i = 0; i < playersScore.Length; i++)
        {
            if (playersScore[i].nbKills >= nbKillsToWin)
                indexesPlayerWin.Add(i);
        }

        if(indexesPlayerWin.Count == 1)
        {
            LaunchEndGameMenu(indexesPlayerWin[0]);
        }
        else if(indexesPlayerWin.Count > 1)
        {
            foreach (int indexWin in indexesPlayerWin)
            {
                playersScore[indexWin].nbKills--;
            }
            LaunchScoreMenu();
        }
        else
        {
            LaunchScoreMenu();
        }
    }

    private void LaunchScoreMenu()
    {
        PauseManager.instance.EnablePause();
        scoreMenu.DisplayScoreMenu(playersScore, OnEndDisplayScoreMenu);
    }

    private void OnEndDisplayScoreMenu()
    {
        PauseManager.instance.DisablePause();
        RestartLevel();
    }

    private void LaunchEndGameMenu(int indexWinner)
    {
        PauseManager.instance.EnablePause();
        EventManager.instance.OnLevelEnd(levelName);
        endLevelMenu.DisplayEndLevelMenu(OnEndDisplayEndLevelMenu);

        void OnEndDisplayEndLevelMenu()
        {
            PauseManager.instance.DisablePause();
            TransitionManager.instance.LoadSceneAsync("Selection Map");
        }
    }

#if UNITY_EDITOR

    private void DEBUG_EndLevel()
    {
        LaunchEndGameMenu(0);
    }
        
#endif

#endregion

    #region OnDestroy

    protected virtual void OnDestroy()
    {
        EventManager.instance.callbackOnPlayerDeath -= OnPlayerDie;
        EventManager.instance.callbackOnPlayerDeathByEnvironnement -= OnPlayerDieByEnvironnement;
    }

    #endregion

#if UNITY_EDITOR

    private void OnValidate()
    {
        if(_DEBUGendLevel)
        {
            _DEBUGendLevel = false;
            DEBUG_EndLevel();
        }
    }

#endif

    #region Struts

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
}
