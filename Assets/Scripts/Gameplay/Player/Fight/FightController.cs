using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Collision2D;
using Collider2D = UnityEngine.Collider2D;
using System.Runtime.CompilerServices;

public class FightController : MonoBehaviour
{
    private enum FightState : byte
    {
        Normal,
        Dash,//can kill whith dash
        Stun //cant attack and dash
    }

    public enum DamageProtectionType : byte
    {
        Normal, // die if DamageType >= 1
        Dash, // die if DamageType >= 2
        Invicible // die if DamageType >= 3
    }

    public enum DamageType : byte
    {
        NeverKill = 0,
        Normal = 1, //Kill is not dashing
        High = 2, //Kill is not invicible
        AlwaysKill = 3 //Always kill
    }

    public enum EffectType : byte
    {
        None,
        DisableDash,
        DisableAttacks,
        Stun
    }

    private WeakAttack attackWeak;
    private StrongAttack attackStrong;
    private EventController eventController;
    private PlayerCommon playerCommon;
    private CharacterInputs playerInput;
    private CharacterController charController;

    private FightState fightState;
    private float lastInputLauchingWeakAttack = -10f, lastInputLauchingStrongAttack = -10f, lastTimeDash = -10f;
    private bool isLaunchingWeakAttack, isLaunchingStrongAttack, wantLauchWeakAttack, wantLaunchStrongAttack;
    private short canLaunchWeakAttackCounter, canLaunchStrongAttackCounter, isStunCounter, isInvicibleCounter, disableDashCounter, disableWeakAttackCounter, disableStrongAttackCounter;
    private LayerMask charMask;
    private List<uint> charAlreadyTouchByDash = new List<uint>(4);

    private bool isStun => isStunCounter > 0;
    private bool isDashDisable => disableDashCounter > 0;
    private bool isWeakAttackDisable => disableWeakAttackCounter > 0;
    private bool isStrongAttackDisable => disableStrongAttackCounter > 0;
    private bool isDashing;//CharacterController is dashing
    private bool isInvicible => isInvicibleCounter > 0;
    private bool canLauchWeakAttack => canLaunchWeakAttackCounter >= 0;
    private bool canLaunchStrongAttack => canLaunchStrongAttackCounter >= 0;

#if UNITY_EDITOR
    [SerializeField] private bool drawGizmos = true;
#endif

    public bool enableBehavior = true;
    [Header("Fight")]
    [SerializeField] private float bufferDuration = 0.1f;
    [SerializeField] private float dashBumpSpeed = 10f;
    [SerializeField] private float dashInvicibilityTimeOffset = 0.1f;
    [SerializeField, Tooltip("Time during you are invicible and you attack with your dash attack")] private float dashInvicibilityDuration = 0.3f;
    [SerializeField, Tooltip("Time offset before enabling killing when dashing")] private float dashKillTimeOffset= 0.1f;
    [SerializeField, Tooltip("Kill duration when dashing")] private float dashKillDuration = 0.5f;
    public Vector2 dashHitboxOffset, dashHitboxSize;

    [HideInInspector] public DamageProtectionType damageProtectionType;

    #region Awake and Start

    private void Awake()
    {
        attackWeak = GetComponent<WeakAttack>();
        attackStrong = GetComponent<StrongAttack>();
        eventController = GetComponent<EventController>();
        playerCommon = GetComponent<PlayerCommon>();
        playerInput = GetComponent<CharacterInputs>();
        charController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        eventController.callbackBeenAttackApplyEffect += OnBeenApplyEffect;
        eventController.callBackBeenTouchAttack += OnBeenTouch;
        eventController.callBackBeenKill += OnBeenKilled;
        eventController.callBackTouchByEnvironnement += OnBeenTouchByEnvironement;
        eventController.callBackBeenApplyEffectByEnvironnement += OnBeenApplyEffectByEnvironnement;
        eventController.callBackBeenKillByDash += OnBeenKillByDash;
        charController.onDash += StartDashing;
        fightState = FightState.Normal;
        damageProtectionType = DamageProtectionType.Normal;
        charMask = LayerMask.GetMask("Char");
    }

    #endregion

    #region Update

    private void Update()
    {
        if (PauseManager.instance.isPauseEnable)
        {
            lastInputLauchingWeakAttack += Time.deltaTime;
            lastInputLauchingStrongAttack += Time.deltaTime;
            lastTimeDash += Time.deltaTime;
            return;
        }

        if (!enableBehavior)
            return;

        UpdateState();

        if(fightState != FightState.Stun)
        {
            if (playerInput.attackWeakPressedDown)
            {
                if (canLauchWeakAttack && !isWeakAttackDisable)
                {
                    OnBeginWeakAttack();
                    DisableStrongAttack();
                    if (!attackWeak.Launch(EnableStrongAttack, OnEndWeakAttack))
                    {
                        lastInputLauchingWeakAttack = Time.time;
                        wantLauchWeakAttack = true;
                    }
                }
                else
                {
                    lastInputLauchingWeakAttack = Time.time;
                    wantLauchWeakAttack = true;
                }
            }

            if (playerInput.attackStrongPressedDown)
            {
                if (canLaunchStrongAttack && !isStrongAttackDisable)
                {
                    OnBeginStrongAttack();
                    DisableWeakAttack();
                    if (!attackStrong.Launch(EnableWeakAttack, OnEndStrongAttack))
                    {
                        lastInputLauchingStrongAttack = Time.time;
                        wantLaunchStrongAttack = true;
                    }
                }
                else
                {
                    lastInputLauchingStrongAttack = Time.time;
                    wantLaunchStrongAttack = true;
                }
            }

            if (wantLauchWeakAttack)
            {
                if (Time.time - lastInputLauchingWeakAttack < bufferDuration)
                {
                    if (canLauchWeakAttack)
                    {
                        wantLauchWeakAttack = false;
                        OnBeginWeakAttack();
                        DisableStrongAttack();
                        if (!attackWeak.Launch(EnableStrongAttack, OnEndWeakAttack))
                        {
                            wantLauchWeakAttack = true;
                        }
                    }
                }
                else
                {
                    wantLauchWeakAttack = false;
                }
            }

            if (wantLaunchStrongAttack)
            {
                if ((Time.time - lastInputLauchingStrongAttack) < bufferDuration)
                {
                    if (canLaunchStrongAttack)
                    {
                        wantLaunchStrongAttack = false;
                        OnBeginStrongAttack();
                        DisableWeakAttack();
                        if (!attackStrong.Launch(EnableWeakAttack, OnEndStrongAttack))
                        {
                            wantLaunchStrongAttack = true;
                        }
                    }
                }
                else
                {
                    wantLaunchStrongAttack = false;
                }
            }
        }

        HandleDash();
    }

    #endregion

    #region State

    private void UpdateState()
    {
        HandleState();

        HandleDamageProtection();

        void HandleState()
        {
            if(fightState == FightState.Stun)
            {
                if(!isStun)
                {
                    fightState = FightState.Normal;
                }
            }
            else if(fightState == FightState.Dash)
            {
                // Dash => Stun
                if(isStun)
                {
                    fightState = FightState.Stun;
                }
                else if(Time.time - lastTimeDash >= dashKillTimeOffset + dashKillDuration) // dash => Normal
                {
                    fightState = FightState.Normal;
                }
            }
            else if(fightState == FightState.Normal)
            {
                // Normal => Stun
                if (isStun)
                {
                    fightState = FightState.Stun;
                }
                else if(isDashing)
                {
                    if(Time.time - lastTimeDash > dashKillTimeOffset)
                    {
                        fightState = FightState.Dash;
                    }
                }
            }
        }

        void HandleDamageProtection()
        {
            if(isInvicible)
            {
                damageProtectionType = DamageProtectionType.Invicible;
            }
            else if(isDashing && Time.time - lastTimeDash >= dashInvicibilityTimeOffset && Time.time - lastTimeDash < dashInvicibilityTimeOffset + dashInvicibilityDuration)
            {
                damageProtectionType = DamageProtectionType.Dash;
            }
            else
            {
                damageProtectionType = DamageProtectionType.Normal;
            }
        }
    }

    #endregion

    #region Attack

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsDashKillEnable() => isDashing && Time.time - lastTimeDash >= dashKillTimeOffset && Time.time - lastTimeDash < dashKillDuration + dashKillTimeOffset;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnableInvicibility()
    {
        isInvicibleCounter++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DisableInvicibility()
    {
        isInvicibleCounter--;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnableInvicibility(float duration)
    {
        StartCoroutine(EnableInvicibilityCorout(duration));
    }

    private IEnumerator EnableInvicibilityCorout(float duration)
    {
        isInvicibleCounter++;
        yield return PauseManager.instance.Wait(duration);
        isInvicibleCounter--;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DisableDash(float duration)
    {
        StartCoroutine(DisableDashCorout(duration));
    }

    private IEnumerator DisableDashCorout(float duration)
    {
        disableDashCounter++;

        if (isDashDisable)
        {
            charController.DisableInputs(duration);
        }

        yield return PauseManager.instance.Wait(duration);

        disableDashCounter--;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Stun(float duration)
    {
        charController.DisableInputs(duration);
        StartCoroutine(StunCorout(duration));
    }

    private IEnumerator StunCorout(float duration)
    {
        isStunCounter++;
        yield return PauseManager.instance.Wait(duration);
        isStunCounter--;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DisableWeakAttack(float duration)
    {
        StartCoroutine(DisableWeakAttackCorout(duration));
    }

    private IEnumerator DisableWeakAttackCorout(float duration)
    {
        disableWeakAttackCounter++;
        yield return PauseManager.instance.Wait(duration);
        disableWeakAttackCounter--;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DisableStrongAttack(float duration)
    {
        StartCoroutine(DisableStrongAttackCorout(duration));
    }

    private IEnumerator DisableStrongAttackCorout(float duration)
    {
        disableStrongAttackCounter++;
        yield return PauseManager.instance.Wait(duration);
        disableStrongAttackCounter--;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DisableAttack(float duration)
    {
        DisableWeakAttack(duration);
        DisableStrongAttack(duration);
    }

    #endregion

    #region Dash

    private void HandleDash()
    {
        if (!isDashing)
            return;

        if(Time.time - lastTimeDash > Mathf.Max(dashKillTimeOffset + dashKillDuration, dashInvicibilityTimeOffset + dashInvicibilityDuration))
        {
            isDashing = false;
            return;
        }

        if(Time.time - lastTimeDash >= dashKillTimeOffset && Time.time - lastTimeDash < dashKillTimeOffset + dashKillDuration)
        {
            Collider2D[] cols = PhysicsToric.OverlapBoxAll((Vector2)transform.position, dashHitboxSize, 0f, charMask);
            foreach (Collider2D col in cols)
            {
                if (col.CompareTag("Char"))
                {
                    GameObject player = col.GetComponent<ToricObject>().original;
                    uint playerID = player.GetComponent<PlayerCommon>().id;

                    if (playerID != playerCommon.id && !charAlreadyTouchByDash.Contains(playerID))
                    {
                        charAlreadyTouchByDash.Add(playerID);
                        HandleDashCollision(player);
                    }
                }
            }
        }

        void HandleDashCollision(GameObject player)
        {
            FightController fc = player.GetComponent<FightController>();
            fc.OnBeenTouchByDash(this);
            OnDashCollision(fc);
        }
    }

    private void OnDashCollision(FightController fc)
    {
        if ((int)fc.damageProtectionType >= 1)
        {
            ApplyBump(fc);
        }
    }

    private void OnBeenTouchByDash(FightController fc)
    {
        if(fightState == FightState.Dash)
        {
            charAlreadyTouchByDash.Add(fc.GetComponent<PlayerCommon>().id);
        }

        if ((int)damageProtectionType >= 1)
        {
            ApplyBump(fc);
        }
        else
        {
            KillByOtherWithDash(fc);
        }
    }

    private void ApplyBump(FightController fc)
    {
        Vector2 bumpDir = (((Vector2)transform.position + dashHitboxOffset) - ((Vector2)fc.transform.position + fc.dashHitboxOffset)).normalized;
        charController.ApplyBump(bumpDir * dashBumpSpeed);
    }

    private void KillByOtherWithDash(FightController other)
    {
        eventController.OnBeenKillByDash(other.gameObject);
        other.GetComponent<EventController>().OnKillByDash(gameObject);
        EventManager.instance.OnPlayerDie(gameObject, other.gameObject);
    }

    private void StartDashing(Vector2 dir)
    {
        isDashing = true;
        lastTimeDash = Time.time;
        charAlreadyTouchByDash.Clear();
    }

    #endregion

    #region OnBegin/end Strong/Weak attack

    private void DisableWeakAttack()
    {
        canLaunchWeakAttackCounter--;
    }

    private void DisableStrongAttack()
    {
        canLaunchStrongAttackCounter--;
    }

    private void EnableWeakAttack()
    {
        canLaunchWeakAttackCounter++;
    }

    private void EnableStrongAttack()
    {
        canLaunchStrongAttackCounter++;
    }

    private void OnBeginStrongAttack()
    {
        isLaunchingStrongAttack = true;
        DisableStrongAttack();
    }

    private void OnBeginWeakAttack()
    {
        isLaunchingWeakAttack = true;
        DisableWeakAttack();
    }

    private void OnEndStrongAttack()
    {
        if (!isLaunchingStrongAttack)
        {
            LogManager.instance.AddLog("FightController::OnEndStrongAttack can't be call if isLaunchingStrongAttack == false!");
            Debug.LogWarning("Debug pls");
            return;
        }
        isLaunchingStrongAttack = false;
        EnableStrongAttack();
    }

    private void OnEndWeakAttack()
    {
        if (!isLaunchingWeakAttack)
        {
            LogManager.instance.AddLog("FightController::OnEndWeakAttack can't be call if isLaunchingWeakAttack == false!");
            Debug.LogWarning("Debug pls");
            return;
        }
        isLaunchingWeakAttack = false;
        EnableWeakAttack();
    }

    #endregion

    #region Effect

    private void OnBeenApplyEffect(Attack attack, GameObject enemy, EffectType effectType, EffectParams effectParams)
    {
        ApplyEffect(effectType, effectParams);
    }

    private void OnBeenApplyEffectByEnvironnement(GameObject go, EffectType effectType, EffectParams effectParams)
    {
        ApplyEffect(effectType, effectParams);
    }

    private void ApplyEffect(EffectType effectType, EffectParams effectParams)
    {
        switch (effectType)
        {
            case EffectType.None:
                break;
            case EffectType.DisableDash:
                DisableDashEffectParams dashEffectParams = (DisableDashEffectParams)effectParams;
                DisableDash(dashEffectParams.duration);
                break;
            case EffectType.DisableAttacks:
                DisableAttacksEffectParams attackEffectParams = (DisableAttacksEffectParams)effectParams;
                if (attackEffectParams.disableAttack == DisableAttacksEffectParams.DisableAttack.Both)
                {
                    DisableAttack(attackEffectParams.duration);
                }
                else if (attackEffectParams.disableAttack == DisableAttacksEffectParams.DisableAttack.WeakAttack)
                {
                    DisableWeakAttack(attackEffectParams.duration);
                }
                else if (attackEffectParams.disableAttack == DisableAttacksEffectParams.DisableAttack.StrongAttack)
                {
                    DisableStrongAttack(attackEffectParams.duration);
                }
                break;
            case EffectType.Stun:
                StunEffectParams stunEffectParams = (StunEffectParams)effectParams;
                Stun(stunEffectParams.duration);
                break;
            default:
                break;
        }
    }

    #endregion

    #region Touch

    private void OnBeenTouch(Attack attack, GameObject enemy, DamageType damageType)
    {
        if(damageType != DamageType.NeverKill && (int)damageType > (int)damageProtectionType)
        {
            eventController.OnBeenKill(enemy);
            enemy.GetComponent<EventController>().OnKill(gameObject);
            EventManager.instance.OnPlayerDie(gameObject, enemy);
        }
        else
        {
            eventController.OnBlockAttack(attack, enemy);
            enemy.GetComponent<EventController>().OnBeenBlockAttack(attack, gameObject);
        }
    }

    private void OnBeenTouchByEnvironement(GameObject go, DamageType damageType)
    {
        if (damageType != DamageType.NeverKill && (int)damageType > (int)damageProtectionType)
        {
            eventController.OnBeenKill(go);
            EventManager.instance.OnPlayerDieByEnvironnement(gameObject, go);
        }
    }

    #endregion

    #region Kill

    private void OnBeenKillByDash(GameObject dasher)
    {
        OnBeenKilled(dasher);
    }

    private void OnBeenKilled(GameObject killer)
    {
        Useful.InvokeWaitAFrame(this, nameof(Death));
    }

    private void Death()
    {
        Destroy(gameObject);
    }

    #endregion

    #region OnDestroy/OnValidate/Gizmos

    private void OnDestroy()
    {
        eventController.callbackBeenAttackApplyEffect -= OnBeenApplyEffect;
        eventController.callBackBeenTouchAttack -= OnBeenTouch;
        eventController.callBackBeenKill -= OnBeenKilled;
        eventController.callBackTouchByEnvironnement -= OnBeenTouchByEnvironement;
        eventController.callBackBeenApplyEffectByEnvironnement -= OnBeenApplyEffectByEnvironnement;
        eventController.callBackBeenKillByDash -= OnBeenKillByDash;
        charController.onDash -= StartDashing;
    }

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos)
            return;

        Hitbox.GizmosDraw((Vector2)transform.position + dashHitboxOffset, dashHitboxSize, Color.green, true);
    }

    private void OnValidate()
    {
        dashBumpSpeed = Mathf.Max(0f, dashBumpSpeed);
        dashInvicibilityTimeOffset = Mathf.Max(0f, dashInvicibilityTimeOffset);
        dashInvicibilityDuration = Mathf.Max(0f, dashInvicibilityDuration);
        dashKillTimeOffset = Mathf.Max(0f, dashKillTimeOffset);
        dashKillDuration = Mathf.Max(0f, dashKillDuration);
        bufferDuration = Mathf.Max(0f, bufferDuration);
    }

#endif

    #endregion

}
