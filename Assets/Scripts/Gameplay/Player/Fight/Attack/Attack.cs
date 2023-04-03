using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class Attack : MonoBehaviour
{
    protected List<uint> charAlreadyTouch;

    protected EventController eventController;
    protected PlayerCommon playerCommon;

    [Range(0f, 100f)] public float attackForce;
    [Tooltip("Durée l'immobilité après avoir lancé le sort en sec")] public float castDuration;
    [SerializeField] protected Cooldown cooldown;

    protected virtual void Awake()
    {
        eventController = GetComponent<EventController>();
        playerCommon = GetComponent<PlayerCommon>();
        charAlreadyTouch = new List<uint>();
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

    public virtual void OnTouchEnemy(GameObject enemy)
    {
        charAlreadyTouch.Add(enemy.GetComponent<PlayerCommon>().id);
        eventController.OnTouchAttack(this, enemy);
        enemy.GetComponent<ToricObject>().original.GetComponent<EventController>().OnBeenTouchAttack(this);
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
}
