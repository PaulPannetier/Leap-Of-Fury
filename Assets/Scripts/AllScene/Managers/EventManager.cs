using System;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager instance;

    public Action<GameObject, GameObject> callbackOnPlayerDeath;
    public Action<GameObject, GameObject> callbackOnPlayerDeathByEnvironnement;
    public Action callbackPreUpdate;
    public Action<string> callbackOnLevelStart;
    public Action<LevelManager.EndLevelData> callbackOnLevelEnd;
    public Action<string> callbackOnLevelFinish;
    public Action<string> callbackOnLevelRestart;
    public Action<LevelMapData> callbackOnMapChanged;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(this);
            return;
        }
        instance = this;

        callbackOnPlayerDeath = new Action<GameObject, GameObject>((GameObject p1, GameObject p2) => { });
        callbackOnPlayerDeathByEnvironnement = new Action<GameObject, GameObject>((GameObject p1, GameObject p2) => { });
        callbackPreUpdate = new Action(() => { });
        callbackOnLevelStart = new Action<string>((string arg1) => { });
        callbackOnLevelEnd = new Action<LevelManager.EndLevelData>((LevelManager.EndLevelData arg1) => { });
        callbackOnLevelRestart = new Action<string>((string arg1) => { });
    }

    private void Update()
    {
        callbackPreUpdate.Invoke();
    }

    public void OnTriggerPlayerDeath(GameObject player, GameObject killer)
    {
        callbackOnPlayerDeath.Invoke(player, killer);
    }

    public void OnTriggerPlayerDeathByEnvironnement(GameObject player, GameObject killer)
    {
        callbackOnPlayerDeathByEnvironnement.Invoke(player, killer);
    }

    public void OnLevelStart(string levelName)
    {
        callbackOnLevelStart.Invoke(levelName);
    }

    public void OnLevelRestart(string levelName)
    {
        callbackOnLevelRestart.Invoke(levelName);
    }

    public void OnLevelEnd(in LevelManager.EndLevelData endLevelData)
    {
        callbackOnLevelEnd.Invoke(endLevelData);
    }

    public void OnLevelFinish(string levelName)
    {
        callbackOnLevelFinish.Invoke(levelName);
    }

    public void OnMapChanged(LevelMapData levelMapData)
    {
        callbackOnMapChanged.Invoke(levelMapData);
    }
}
