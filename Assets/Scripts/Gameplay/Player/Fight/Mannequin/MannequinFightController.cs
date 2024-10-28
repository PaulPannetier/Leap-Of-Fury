#if UNITY_EDITOR

using UnityEngine;

public class MannequinFightController : FightController
{
    private GameObject otherChar;
    [Space, Space, SerializeField, ShowOnly] private string otherCharName = "null";
    [SerializeField] private bool enableIsDashing;
    [SerializeField] EffectType effectType;
    [SerializeField] bool applyEffect;
    [SerializeField] FightState currentFightState;
    [SerializeField] DamageType damageType;
    [SerializeField] private bool touch;
    [SerializeField] private DamageProtectionType damageProtection;

    protected override void Awake()
    {
        attackWeak = GetComponent<WeakAttack>();
        attackStrong = GetComponent<StrongAttack>();
        playerCommon = GetComponent<PlayerCommon>();
        eventController = GetComponent<EventController>();
    }

    protected override void Start()
    {
        eventController.callbackBeenAttackApplyEffect += OnBeenApplyEffect;
        eventController.callBackBeenTouchAttack += OnBeenTouch;
        eventController.callBackBeenKill += OnBeenKilled;
        eventController.callBackTouchByEnvironnement += OnBeenTouchByEnvironement;
        eventController.callBackBeenApplyEffectByEnvironnement += OnBeenApplyEffectByEnvironnement;
        eventController.callBackBeenKillByDash += OnBeenKillByDash;
        charMask = LayerMask.GetMask("Char");
        this.InvokeWaitAFrame(nameof(GetOtherCharacter));
    }

    private void GetOtherCharacter()
    {
        PlayerCommon[] players = GameObject.FindObjectsOfType<PlayerCommon>();
        foreach (PlayerCommon pc in players)
        {
            if (pc.id != playerCommon.id)
            {
                otherChar = pc.gameObject;
                break;
            }
        }
        otherCharName = otherChar.name;
    }

    protected override void Update()
    {

    }

    protected override void StartDashing(Vector2 dir)
    {

    }

    protected override void OnBeenApplyEffect(Attack attack, GameObject enemy, EffectType effectType, EffectParams effectParams)
    {
        print($"Apply effect : {effectType.ToString()} from {enemy.name}");
    }

    protected override void OnBeenApplyEffectByEnvironnement(GameObject go, EffectType effectType, EffectParams effectParams)
    {
        print($"Apply effect : {effectType.ToString()} from environnement");
    }

    protected override void OnBeenTouch(Attack attack, GameObject enemy, DamageType damageType)
    {
        print($"Touch by {enemy.name} and applying {damageType.ToString()} damage");
        base.OnBeenTouch(attack, enemy, damageType);
    }

    protected override void OnBeenTouchByEnvironement(GameObject go, DamageType damageType)
    {
        print($"Touch by Environement {go.name} and applaying {damageType.ToString()} damage");
        base.OnBeenTouchByEnvironement(go, damageType);
    }

    protected override void OnBeenKillByDash(GameObject dasher)
    {
        print($"Kill by enemy {dasher.name} using dash");
    }

    protected override void OnBeenKilled(GameObject killer)
    {
        print($"Kill by ennemi {killer.name}");
    }

    protected override void ApplyDashBump(FightController other)
    {
        print($"Apply dash from {other.gameObject.name}");
    }

    protected override void OnDrawGizmosSelected() { }

    protected override void OnDestroy() 
    {
        eventController.callbackBeenAttackApplyEffect -= OnBeenApplyEffect;
        eventController.callBackBeenTouchAttack -= OnBeenTouch;
        eventController.callBackBeenKill -= OnBeenKilled;
        eventController.callBackTouchByEnvironnement -= OnBeenTouchByEnvironement;
        eventController.callBackBeenApplyEffectByEnvironnement -= OnBeenApplyEffectByEnvironnement;
        eventController.callBackBeenKillByDash -= OnBeenKillByDash;
    }

    protected override void OnValidate()
    {
        isDashing = enableIsDashing;
        fightState = currentFightState;
        damageProtectionType = damageProtection;

        if (!Application.isPlaying)
            otherCharName = "null";

        if (applyEffect)
        {
            applyEffect = false;
            otherChar.GetComponent<ToricObject>().original.GetComponent<EventController>().OnAttackApplyEffect(attackWeak, gameObject, effectType, new StunEffectParams(2f));
        }

        if(touch)
        {
            touch = false;
            otherChar.GetComponent<ToricObject>().original.GetComponent<EventController>().OnTouchAttack(attackWeak, gameObject, damageType);
        }
    }
}   

#endif
