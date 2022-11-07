using System;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager instance;

    public Action<GameObject, GameObject> callbackOnPlayerDeath;
    public Action<GameObject, GameObject> callbackOnPlayerDeathByEnvironnement;
    public Action callbackPreUpdate;
    public Action<string> callbackOnLevelRestart;

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

    public void OnLevelRestart(string levelName)
    {
        callbackOnLevelRestart.Invoke(levelName);
    }
}
