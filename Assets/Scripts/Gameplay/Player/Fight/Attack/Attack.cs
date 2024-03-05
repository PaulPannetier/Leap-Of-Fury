using System;
using UnityEngine;

[Serializable]
public abstract class Attack : MonoBehaviour
{
    protected EventController eventController;
    protected PlayerCommon playerCommon;

    [Tooltip("Dur�e l'immobilit� apr�s avoir lanc� le sort en sec")] public float castDuration;
    [SerializeField] protected Cooldown cooldown;

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

    public virtual void OnTouchEnemy(GameObject enemy)
    {
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

    protected virtual void OnDestroy()
    {

    }
}
