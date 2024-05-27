using System;
using UnityEngine;
using static FightController;

[Serializable]
public abstract class Attack : MonoBehaviour
{
    protected EventController eventController;
    protected PlayerCommon playerCommon;

    [SerializeField] protected Cooldown cooldown;
    [SerializeField] protected DamageType damageType = DamageType.Normal;
    [SerializeField] protected EffectType effectType;

#if UNITY_EDITOR
    [SerializeField] private bool saveAttackStats;
#endif

    protected virtual void Awake()
    {
        eventController = GetComponent<EventController>();
        playerCommon = GetComponent<PlayerCommon>();
    }

    protected virtual void Start()
    {
        eventController.callBackAnimatorSetFloat += OnTriggerAnimatorSetFloat;
        eventController.callBackAnimatorSetBool += OnTriggerAnimatorSetBool;
        eventController.callBackAnimatorSetTrigger += OnTriggerAnimatorSetTrigger;
        eventController.callBackLauchAttack += OnLauchAttack;

        cooldown.ForceActivate();
    }

    protected virtual void Update()
    {
        cooldown.Update();
    }

    protected virtual void FixedUpdate()
    {

    }

    public virtual bool Launch(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
        eventController.OnLauchAttack(this);
        return true;
    }

    protected virtual void ApplyEffect(GameObject enemy, EffectType effectType, EffectParams effectParams)
    {
        enemy = enemy.GetComponent<ToricObject>().original;
        eventController.OnAttackApplyEffect(this, enemy, effectType, effectParams);
        enemy.GetComponent<EventController>().OnBeenAttackApplyEffect(this, gameObject, effectType, effectParams);
    }

    protected virtual void OnTouchEnemy(GameObject enemy, DamageType damageType)
    {
        enemy = enemy.GetComponent<ToricObject>().original;
        eventController.OnTouchAttack(this, enemy, damageType);
        enemy.GetComponent<ToricObject>().original.GetComponent<EventController>().OnBeenTouchAttack(this, gameObject, damageType);
    }

    protected virtual void OnTriggerAnimatorSetFloat(string name, float value)
    {

    }

    protected virtual void OnTriggerAnimatorSetBool(string name, bool value)
    {

    }

    protected virtual void OnTriggerAnimatorSetTrigger(string name)
    {

    }

    protected virtual void OnLauchAttack(Attack attack)
    {

    }

    protected virtual void OnDestroy()
    {

    }

#if UNITY_EDITOR

    protected virtual void SaveAttackStats()
    {

    }

    protected virtual void OnValidate()
    {
        if(saveAttackStats)
        {
            saveAttackStats = false;
            SaveAttackStats();
        }
    }

#endif
}
