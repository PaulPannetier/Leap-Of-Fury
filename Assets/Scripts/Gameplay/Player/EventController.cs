using System;
using UnityEngine;

public class EventController : MonoBehaviour
{
    public Action<string, float> callBackAnimatorSetFloat;
    public Action<string, bool> callBackAnimatorSetBool;
    public Action<string> callBackAnimatorSetTrigger;
    public Action<Attack> callBackLauchAttack;
    public Action<Attack, GameObject> callBackTouchAttack;
    public Action<Attack> callBackBeenTouchAttack;

    public Action<Attack, GameObject> callBackHitAttack;
    public Action<Attack> callBackBeenHitAttack;

    public Action<Attack> callBackBlockAttack;
    public Action<Attack, GameObject> callBackBeenBlockAttack;
    public Action<GameObject> callBackKill;
    public Action callBackDeath;
    public Action<TimePortal> callBackEnterTimePortal;
    public Action<TimePortal> callBackExitTimePortal;

    public Action<GameObject> callBackTouchByEnvironnement;
    public Action<GameObject> callBackKillByEnvironnement;

    private void Awake()
    {
        Reset();
    }

    private void Reset()
    {
        callBackAnimatorSetFloat = new Action<string, float>((string arg1, float arg2) => { });
        callBackAnimatorSetBool = new Action<string, bool>((string arg1, bool arg2) => { });
        callBackAnimatorSetTrigger = new Action<string>((string arg1) => { });
        callBackLauchAttack = new Action<Attack>((Attack arg1) => { });
        callBackTouchAttack = new Action<Attack, GameObject>((Attack arg1, GameObject arg2) => { });
        callBackBeenTouchAttack = new Action<Attack>((Attack arg1) => { });
        callBackHitAttack = new Action<Attack, GameObject>((Attack arg1, GameObject arg2) => { });
        callBackBeenHitAttack = new Action<Attack>((Attack arg1) => { });
        callBackBlockAttack = new Action<Attack>((Attack arg1) => { });
        callBackBeenBlockAttack = new Action<Attack, GameObject>((Attack p, GameObject b) => { });
        callBackKill = new Action<GameObject>((GameObject player) => { });
        callBackDeath = new Action(() => { });
        callBackEnterTimePortal = new Action<TimePortal>((TimePortal arg1) => { });
        callBackExitTimePortal = new Action<TimePortal>((TimePortal arg1) => { });
        callBackTouchByEnvironnement = new Action<GameObject>((GameObject arg1) => { });
        callBackKillByEnvironnement = new Action<GameObject>((GameObject arg1) => { });
    }

    public void OnTriggerAnimatorSetFloat(string name, in float value)
    {
        callBackAnimatorSetFloat.Invoke(name, value);
    }

    public void OnTriggerAnimatorSetBool(string name, in bool value)
    {
        callBackAnimatorSetBool.Invoke(name, value);
    }

    public void OnTriggerAnimatorSetTrigger(string name)
    {
        callBackAnimatorSetTrigger.Invoke(name);
    }

    //Quand le char lance une attaque
    public void OnLauchAttack(Attack attack)
    {
        callBackLauchAttack.Invoke(attack);
    }

    //quand une attack touche une cible
    public void OnTouchAttack(Attack attack, GameObject other)
    {
        callBackTouchAttack.Invoke(attack, other);
    }

    //quand on est touché par un attaque
    public void OnBeenTouchAttack(Attack attack)
    {
        callBackBeenTouchAttack.Invoke(attack);
    }

    public void OnHitAttack(Attack attack, GameObject other)
    {
        callBackHitAttack.Invoke(attack, other);
    }

    public void OnBeenHitAttack(Attack attack)
    {
        callBackBeenHitAttack.Invoke(attack);
    }

    public void OnBlockAttack(Attack attack)
    {
        callBackBlockAttack.Invoke(attack);
    }

    //quand l'attaque en param à été bloquer
    public void OnBeenBlockAttack(Attack attack, GameObject blocker)
    {
        callBackBeenBlockAttack.Invoke(attack, blocker);
    }

    public void OnKill(GameObject player)
    {
        callBackKill.Invoke(player);
    }

    public void OnDeath()
    {
        callBackDeath.Invoke();
    }

    public void OnEnterTimePortal(TimePortal timePortal)
    {
        callBackEnterTimePortal.Invoke(timePortal);
    }

    public void OnExitTimePortal(TimePortal timePortal)
    {
        callBackExitTimePortal.Invoke(timePortal);
    }

    public void OnBeenTouchByEnvironnement(GameObject go)
    {
        callBackTouchByEnvironnement.Invoke(go);
    }

    public void OnBeenKillByEnvironnement(GameObject go)
    {
        callBackKillByEnvironnement.Invoke(go);
    }
}
