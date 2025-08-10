using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class LevelManager : MonoBehaviour
{
    #region Fields

    public static LevelManager instance;

    protected PlayerScore[] playersScore;
    protected int currentNbPlayerAlive;
    private float lastTimeBeginLevel = -10f;
    private GameObject currentMap;
    private int currentMapIndex;
    protected SelectionMapOldSceneData selectionMapOldSceneData;

    [Header("Level Management")]
    [SerializeField] private string levelName;
    [SerializeField] protected float durationToWaitAtBegining = 3f;
    [SerializeField] protected int nbKillsToWin = 7;
    [SerializeField] protected float waitingTimeAfterLastKill = 2f;
    [SerializeField] protected Transform charParent; 

    [Header("Maps")]
    [SerializeField] private bool suffleMapWhenLevelStart = true;
    [SerializeField] private bool playFirstMapAtLevelStart;
    [SerializeField] protected GameObject[] mapsPrefabs;

    #if UNITY_EDITOR

    [Header("Level initialiser")]
    public bool enableDebugMode = true;
    public bool disableDeathCount;

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

#if UNITY_EDITOR

        //Avoid call Awake / Start method when run the level whith character
        if(enableDebugMode)
        {
            for(int i = 0; i < charParent.childCount; i++)
            {
                GameObject charGO = charParent.GetChild(i).gameObject;
                for(int j = 0; j < charGO.GetComponentCount(); j++)
                {
                    Component component = charGO.GetComponentAtIndex(j);
                    if(component is MonoBehaviour monoBehaviour)
                    {
                        monoBehaviour.enabled = false;
                    }
                }
            }
        }

#endif
    }

    #region Start/Restart level

    protected virtual void Start()
    {
        this.InvokeWaitAFrame(nameof(StartLevel));
    }

    protected virtual void StartLevel()
    {
        #if UNITY_EDITOR

        if (enableDebugMode)
        {
            PlayerIndex[] playerIndexes = new PlayerIndex[4]
            {
                PlayerIndex.One, PlayerIndex.Two, PlayerIndex.Three, PlayerIndex.Four
            };

            ControllerType[] controllerTypes = new ControllerType[4]
            {
                ControllerType.Keyboard, ControllerType.Gamepad1, ControllerType.Gamepad2, ControllerType.Gamepad3
            };

            CharData[] charsData = new CharData[charParent.childCount];
            for (int i = 0; i < charsData.Length; i++)
            {
                Transform charGO = charParent.GetChild(i);
                PlayerCommon pc = charGO.GetComponent<PlayerCommon>();
                charsData[i] = new CharData(playerIndexes[i], controllerTypes[i], pc.prefabs);
            }

            SelectionMapOldSceneData selectionMapData = new SelectionMapOldSceneData(charsData);
            TransitionManager.instance.SetOldSceneData(selectionMapData);
        }

        #endif

        FillCharData();

        HandleMap();

        InstanciateChar();

        HandleCharPosAndCallback();

        void FillCharData()
        {
            SelectionMapOldSceneData oldSceneData = (SelectionMapOldSceneData)TransitionManager.instance.GetOldSceneData("Selection Map");
            currentNbPlayerAlive = oldSceneData.charData.Length;
            selectionMapOldSceneData = oldSceneData;
        }

        void HandleMap()
        {
            #if UNITY_EDITOR

            GameObject map = GameObject.FindGameObjectWithTag("Map");
            if (map != null)
                Destroy(map);

            #endif

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
            tmpGO.GetComponent<CharacterInputs>().controllerType = playerData.controllerType;

            InputManager.SetCurrentController(playerData.playerIndex, playerData.controllerType, false);

            for (int j = 0; j < playersScore.Length; j++)
            {
                if (playersScore[j].playerIndex == playerData.playerIndex)
                {
                    playersScore[j].playerCommon = playerCommon;
                    break;
                }
            }

            tmpGO.GetComponent<CharacterController>().Teleport(spawnPoint);
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

    #endregion

    #endregion

    #region Block/Release Player

    private void BlockPlayers()
    {
        foreach (Transform t in charParent)
        {
            t.GetComponent<CharacterController>()?.DisableInputs(durationToWaitAtBegining);
        }
    }

    #endregion

    #region OnPlayerDie

    protected void OnPlayerDie(GameObject player, GameObject killer)
    {
#if UNITY_EDITOR
        if (disableDeathCount)
            return;
#endif
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
#if UNITY_EDITOR
        if (disableDeathCount)
            return;
#endif
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
        yield return PauseManager.instance.Wait(waitingTimeAfterLastKill);
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

    #region Structs

    public struct PlayerScore
    {
        public static int nbKillsToWin;
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
