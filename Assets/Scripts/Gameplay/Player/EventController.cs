using System;
using UnityEngine;
using static FightController;

public class EventController : MonoBehaviour
{
    public Action<string, float> callBackAnimatorSetFloat;
    public Action<string, bool> callBackAnimatorSetBool;
    public Action<string> callBackAnimatorSetTrigger;
    public Action<Attack> callBackLauchAttack;
    public Action<Attack, GameObject, EffectType, EffectParams> callbackAttackApplyEffect;
    public Action<Attack, GameObject, EffectType, EffectParams> callbackBeenAttackApplyEffect;
    public Action<Attack, GameObject, DamageType> callBackTouchAttack;
    public Action<Attack, GameObject, DamageType> callBackBeenTouchAttack;

    public Action<Attack, GameObject> callBackBlockAttack;
    public Action<Attack, GameObject> callBackBeenBlockAttack;
    public Action<GameObject> callBackKill;
    public Action<GameObject> callBackBeenKill;

    public Action<GameObject> callBackKillByDash;
    public Action<GameObject> callBackBeenKillByDash;
    public Action<GameObject, EffectType, EffectParams> callBackBeenApplyEffectByEnvironnement;
    public Action<GameObject, DamageType> callBackTouchByEnvironnement;
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
        callbackAttackApplyEffect = new Action<Attack, GameObject, EffectType, EffectParams>((Attack arg1, GameObject arg2,  EffectType arg3, EffectParams arg4) => { });
        callbackBeenAttackApplyEffect = new Action<Attack, GameObject, EffectType, EffectParams>((Attack arg1, GameObject arg2, EffectType arg3, EffectParams arg4) => { });
        callBackTouchAttack = new Action<Attack, GameObject, DamageType>((Attack arg1, GameObject arg, DamageType arg3) => { });
        callBackBeenTouchAttack = new Action<Attack, GameObject, DamageType>((Attack arg1, GameObject arg, DamageType arg3) => { });
        callBackBlockAttack = new Action<Attack, GameObject>((Attack arg1, GameObject arg2) => { });
        callBackBeenBlockAttack = new Action<Attack, GameObject>((Attack p, GameObject b) => { });
        callBackKill = new Action<GameObject>((GameObject player) => { });
        callBackBeenKill = new Action<GameObject>((GameObject arg1) => { });
        callBackKillByDash = new Action<GameObject>((GameObject arg) => { });
        callBackBeenKillByDash = new Action<GameObject>((GameObject arg) => { });
        callBackBeenApplyEffectByEnvironnement = new Action<GameObject, EffectType, EffectParams>((GameObject arg1, EffectType arg2, EffectParams arg3) => { });
        callBackTouchByEnvironnement = new Action<GameObject, DamageType>((GameObject arg1, DamageType arg2) => { });
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

    public void OnLauchAttack(Attack attack)
    {
        callBackLauchAttack.Invoke(attack);
    }

    //When our attack apply an effect on another target
    public void OnAttackApplyEffect(Attack attack, GameObject other, EffectType effectType, EffectParams effectParams)
    {
        callbackAttackApplyEffect.Invoke(attack, other, effectType, effectParams);
    }

    //When an attack apply an effect on this player
    public void OnBeenAttackApplyEffect(Attack attack, GameObject enemy, EffectType effectType, EffectParams effectParams)
    {
        callbackBeenAttackApplyEffect.Invoke(attack, enemy, effectType, effectParams);
    }

    //When our attack touch a target
    public void OnTouchAttack(Attack attack, GameObject other, DamageType damageType)
    {
        callBackTouchAttack.Invoke(attack, other, damageType);
    }

    //When an attack hit this player
    public void OnBeenTouchAttack(Attack attack, GameObject enemy, DamageType damageType)
    {
        callBackBeenTouchAttack.Invoke(attack, enemy, damageType);
    }

    //When this player has been hit by an attack and block it
    public void OnBlockAttack(Attack attack, GameObject enemy)
    {
        callBackBlockAttack.Invoke(attack, enemy);
    }

    //When we hit another target and our attack has been blocked
    public void OnBeenBlockAttack(Attack attack, GameObject blocker)
    {
        callBackBeenBlockAttack.Invoke(attack, blocker);
    }

    //When this player kill an ennemi
    public void OnKill(GameObject player)
    {
        callBackKill.Invoke(player);
    }

    //When this player has been killed by an ennemi
    public void OnBeenKill(GameObject killer)
    {
        callBackBeenKill.Invoke(killer);
    }

    //When this player kill an ennemy with a dash
    public void OnKillByDash(GameObject playerKilled)
    {
        callBackKillByDash.Invoke(playerKilled);
    }

    //When this player has been killed by ennemy with a dash
    public void OnBeenKillByDash(GameObject dasher)
    {
        callBackBeenKillByDash.Invoke(dasher);
    }

    //When a non ennemi things apply effect on this player
    public void OnBeenApplyEffectByEnvironnement(GameObject go, EffectType effectType, EffectParams effectParams)
    {
        callBackBeenApplyEffectByEnvironnement.Invoke(go, effectType, effectParams);
    }

    //When a non ennemi things hit this player
    public void OnBeenTouchByEnvironnement(GameObject go, DamageType damageType)
    {
        callBackTouchByEnvironnement.Invoke(go, damageType);
    }

    //When a non ennemi things kill this player
    public void OnBeenKillByEnvironnement(GameObject go)
    {
        callBackKillByEnvironnement.Invoke(go);
    }
}
