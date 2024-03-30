using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Collision2D;
using Collider2D = UnityEngine.Collider2D;

public class FightController : MonoBehaviour
{
    private WeakAttack attackWeak;
    private StrongAttack attackStrong;
    private EventController eventController;
    private PlayerCommon playerCommon;
    private CustomPlayerInput playerInput;
    private CharacterController movement;

    private float lastInputLauchingWeakAttack = -10f, lastInputLauchingStrongAttack;
    private bool isLaunchingWeakAttack, isLaunchingStrongAttack, wantLauchWeakAttack, wantLaunchStrongAttack;
    private ToricObject toricObject;
    private bool isInvicible => invicibilityCounter > 0;
    private short invicibilityCounter, canLaunchWeakAttackCounter, canLaunchStrongAttackCounter, canKillDashingCounter, isStunCounter;
    private int charMask;
    private bool isStun => isStunCounter > 0;
    private List<uint> charAlreadyTouchByDash = new List<uint>();

#if UNITY_EDITOR

    [SerializeField] private bool drawGizmos = true;

#endif

    [Header("Fight")]
    [SerializeField] private float dashBumpSpeed = 10f;
    [SerializeField] private float dashInvicibilityTimeOffset = 0.1f;
    [SerializeField, Tooltip("Time during you are invicible and you attack with your dash attack")] private float invicibilityDurationWhenDashing = 0.3f;
    [SerializeField, Tooltip("Time offset before enabling killing when dashing")] private float dashKillTimeOffset= 0.1f;
    [SerializeField, Tooltip("Kill duration when dashing")] private float dashKillDuration = 0.5f;
    public Vector2 dashHitboxOffset, dashHitboxSize;

    [Header("Inputs")]
    public bool enableAttackWeak = true;
    public bool enableAttackStrong = true;
    public bool enableInvisibilityWhenDashing = true;
    [Tooltip("Temps ou l'on considère l'appuye sur un touche alors que le perso est occupé/ne peut pas lancé l'attaque")][SerializeField] private float castCoyoteTime = 0.2f;

    [HideInInspector] public bool canKillDashing => canKillDashingCounter > 0;
    [HideInInspector] public bool canBeStun => !isInvicible;
    [HideInInspector] public bool canLauchWeakAttack = true, canLaunchStrongAttack = true;

    #region Awake and Start

    private void Awake()
    {
        attackWeak = GetComponent<WeakAttack>();
        attackStrong = GetComponent<StrongAttack>();
        eventController = GetComponent<EventController>();
        playerCommon = GetComponent<PlayerCommon>();
        toricObject = GetComponent<ToricObject>();
        playerInput = GetComponent<CustomPlayerInput>();
        movement = GetComponent<CharacterController>();
        canKillDashingCounter = 0;
    }

    private void Start()
    {
        eventController.callBackBeenTouchAttack += OnBeenTouchAttack;
        eventController.callBackTouchByEnvironnement += OnBeenTouchByEnvironnement;
        eventController.callBackKillByEnvironnement += OnBeenKillByEnvironnement;
        eventController.callBackBeenKillInstant += OnBeenKillInstant;
        eventController.callBackBeenKillByDash += OnBeenKillByDash;
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
            return;
        }

        if (isStun)
            return;

        //attaques
        if (playerInput.attackWeakPressedDown && enableAttackWeak)
        {
            if(canLauchWeakAttack)
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

        if(playerInput.attackStrongPressedDown && enableAttackStrong)
        {
            if (canLaunchStrongAttack)
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

        if(wantLauchWeakAttack)
        {
            if(Time.time - lastInputLauchingWeakAttack < castCoyoteTime)
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

        if(wantLaunchStrongAttack)
        {
            if ((Time.time - lastInputLauchingStrongAttack) < castCoyoteTime)
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

        //Dash
        HandleDash();
    }

    #endregion

    #region Dash/Invicibility

    private void HandleDash()
    {
        if (!canKillDashing)
            return;

        Collider2D[] cols = PhysicsToric.OverlapBoxAll((Vector2)transform.position, dashHitboxSize, 0f, charMask);
        foreach(Collider2D col in cols)
        {
            if(col.CompareTag("Char"))
            {
                GameObject player = col.GetComponent<ToricObject>().original;
                uint playerID = player.GetComponent<PlayerCommon>().id;
                
                if(playerID != playerCommon.id && !charAlreadyTouchByDash.Contains(playerID))
                {
                    charAlreadyTouchByDash.Add(playerID);
                    HandleDashCollision(player);
                }
            }
        }

        void HandleDashCollision(GameObject player)
        {
            FightController fc = player.GetComponent<FightController>();
            bool playerIsInvicible = fc.isInvicible;
            fc.OnDashCollision(this);
            if(playerIsInvicible)
            {
                if(isInvicible)
                {
                    //on applique le bump
                    ApplyBump(fc);
                }
                else
                {
                    KillByOtherWithDash(fc);
                }
                DisableDashCollision(dashKillDuration);
            }
        }
    }

    private void ApplyBump(FightController fc)
    {
        Vector2 bumpDir = (((Vector2)transform.position + dashHitboxOffset) - ((Vector2)fc.transform.position + fc.dashHitboxOffset)).normalized;
        movement.ApplyBump(bumpDir * dashBumpSpeed);
    }

    private void KillByOtherWithDash(FightController other)
    {
        eventController.OnBeenKillByDash(other.gameObject);
        other.GetComponent<EventController>().OnKillByDash(gameObject);
        EventManager.instance.OnTriggerPlayerDeath(gameObject, other.gameObject);
    }

    private void OnDashCollision(FightController fc)
    {
        if(isInvicible)
        {
            if(fc.isInvicible)
            {
                //on applique le bump
                ApplyBump(fc);
            }
            DisableDashCollision(dashKillDuration);
        }
        else
        {
            KillByOtherWithDash(fc);
        }
    }

    private void DisableDashCollision(float duration)
    {
        StartCoroutine(DisableDashCollisionCorout(duration));   
    }

    private IEnumerator DisableDashCollisionCorout(float duration)
    {
        canKillDashingCounter--;

        float timeCounter = 0f;
        while(timeCounter < duration)
        {
            yield return null;
            if(!PauseManager.instance.isPauseEnable)
            {
                timeCounter += Time.deltaTime;
            }
        }

        canKillDashingCounter++;
    }

    public void StartDashing()
    {
        StartCoroutine(StartDashingCorout());
        StartCoroutine(StartInvicibilityCorout());
    }

    private IEnumerator StartDashingCorout()
    {
        float timeCounter = 0f;
        while (timeCounter < dashKillTimeOffset)
        {
            yield return null;
            if (!PauseManager.instance.isPauseEnable)
            {
                timeCounter += Time.deltaTime;
            }
        }

        canKillDashingCounter++;

        timeCounter = 0f;
        while (timeCounter < dashKillDuration)
        {
            yield return null;
            if (!PauseManager.instance.isPauseEnable)
            {
                timeCounter += Time.deltaTime;
            }
        }

        canKillDashingCounter--;
        charAlreadyTouchByDash.Clear();
    }

    private IEnumerator StartInvicibilityCorout()
    {
        float timeCounter = 0f;
        while (timeCounter < dashInvicibilityTimeOffset)
        {
            yield return null;
            if (!PauseManager.instance.isPauseEnable)
            {
                timeCounter += Time.deltaTime;
            }
        }

        EnableInvicibility();

        timeCounter = 0f;
        while (timeCounter < invicibilityDurationWhenDashing)
        {
            yield return null;
            if (!PauseManager.instance.isPauseEnable)
            {
                timeCounter += Time.deltaTime;
            }
        }

        DisableInvicibility();
    }


    public void EnableInvicibility()
    {
        invicibilityCounter++;
    }

    public void DisableInvicibility()
    {
        invicibilityCounter--;
    }

    public void EnableInvicibility(float duration)
    {
        StartCoroutine(EnableInvicibilityCorout(duration));
    }

    private IEnumerator EnableInvicibilityCorout(float duration)
    {
        EnableInvicibility();

        float timeCounter = 0f;
        while (timeCounter < duration)
        {
            yield return null;
            if (!PauseManager.instance.isPauseEnable)
            {
                timeCounter += Time.deltaTime;
            }
        }

        DisableInvicibility();
    }

    #endregion

    #region Stun

    public bool RequestStun(float duration)
    {
        if(!canBeStun)
            return false;

        if(duration < 0f)
            StartCoroutine(StunCoroutInfinite());
        else
            StartCoroutine(StunCorout(duration));

        return true;
    }

    private IEnumerator StunCoroutInfinite()
    {
        isStunCounter++;
        movement.Freeze();

        while (true)
        {
            yield return null;
        }
    }

    private IEnumerator StunCorout(float duration)
    {
        isStunCounter++;
        if (isStun)
            movement.Freeze();

        float timeCounter = 0f;
        while (timeCounter < duration)
        {
            yield return null;
            if(!PauseManager.instance.isPauseEnable)
            {
                timeCounter += Time.deltaTime;
            }
        }

        isStunCounter--;

        if(!isStun)
            movement.UnFreeze();
    }

    #endregion

    #region OnBegin/end Strong/Weak attack

    private void DisableWeakAttack()
    {
        canLaunchWeakAttackCounter--;
        canLauchWeakAttack = canLaunchWeakAttackCounter >= 0;
    }

    private void DisableStrongAttack()
    {
        canLaunchStrongAttackCounter--;
        canLaunchStrongAttack = canLaunchStrongAttackCounter >= 0;
    }

    private void EnableWeakAttack()
    {
        canLaunchWeakAttackCounter++;
        canLauchWeakAttack = canLaunchWeakAttackCounter >= 0;
    }

    private void EnableStrongAttack()
    {
        canLaunchStrongAttackCounter++;
        canLaunchStrongAttack = canLaunchStrongAttackCounter >= 0;
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
            Debug.LogWarning("Debug pls");
            return;
        }
        isLaunchingWeakAttack = false;
        EnableWeakAttack();
    }

    #endregion

    #region Disable attacking

    private void DisableLauchingAttack(float duration)
    {
        StartCoroutine(Dl(duration));
    }

    private IEnumerator Dl(float duration)
    {
        short weakAttackCount = (short)(canLaunchWeakAttackCounter + 1);
        short strongAttackCount = (short)(canLaunchStrongAttackCounter + 1);
        canLaunchWeakAttackCounter -= weakAttackCount;
        canLaunchStrongAttackCounter -= strongAttackCount;
        canLauchWeakAttack = canLaunchStrongAttack = false;
        yield return Useful.GetWaitForSeconds(duration);
        canLaunchWeakAttackCounter += weakAttackCount;
        canLaunchStrongAttackCounter += strongAttackCount;
        canLauchWeakAttack = canLaunchWeakAttackCounter >= 0;
        canLaunchStrongAttack = canLaunchStrongAttackCounter >= 0;
    }

    #endregion

    #region OnBeen...

    private void OnBeenTouchAttack(Attack attack)
    {
        if (isInvicible || attack.gameObject.GetComponent<PlayerCommon>().id == playerCommon.id)
            return;

        EventController otherEventController = attack.GetComponent<EventController>();
        eventController.OnDeath();
        otherEventController.OnKill(gameObject);
        EventManager.instance.OnTriggerPlayerDeath(gameObject, attack.gameObject);
        Death();
    }

    private void OnBeenTouchByEnvironnement(GameObject go)
    {
        if (!isInvicible)
        {
            eventController.OnBeenKillByEnvironnement(go);
        }
    }

    private void OnBeenKillByEnvironnement(GameObject go)
    {
        EventManager.instance.OnTriggerPlayerDeathByEnvironnement(gameObject, go);
        Death();
    }

    private void OnBeenKillByDash(GameObject killer)
    {
        Death();
    }

    private void OnBeenKillInstant(GameObject killer)
    {
        Death();
    }

    #endregion

    #region OnDestroy/OnValidate/Gizmos

    private void Death()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        eventController.callBackBeenTouchAttack -= OnBeenTouchAttack;
        eventController.callBackTouchByEnvironnement -= OnBeenTouchByEnvironnement;
        eventController.callBackKillByEnvironnement -= OnBeenKillByEnvironnement;
        eventController.callBackBeenKillInstant -= OnBeenKillInstant;
        eventController.callBackBeenKillByDash -= OnBeenKillByDash;
    }

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos)
            return;

        Gizmos.color = Color.green;
        Hitbox.GizmosDraw((Vector2)transform.position + dashHitboxOffset, dashHitboxSize);
    }

    private void OnValidate()
    {
        dashBumpSpeed = Mathf.Max(0f, dashBumpSpeed);
        dashInvicibilityTimeOffset = Mathf.Max(0f, dashInvicibilityTimeOffset);
        invicibilityDurationWhenDashing = Mathf.Max(0f, invicibilityDurationWhenDashing);
        dashKillTimeOffset = Mathf.Max(0f, dashKillTimeOffset);
        dashKillDuration = Mathf.Max(0f, dashKillDuration);
    }

#endif

    #endregion
}
