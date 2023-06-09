using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightController : MonoBehaviour
{
    private WeakAttack attackWeak;
    private StrongAttack attackStrong;
    private EventController eventController;
    private PlayerCommon playerCommon;
    private CustomPlayerInput playerInput;
    private Movement movement;

    private int nbKill = 0;
    private float lastInputLauchingWeakAttack = -10f, lastInputLauchingStrongAttack;
    private bool isLaunchingWeakAttack, isLaunchingStrongAttack, wantLauchWeakAttack, wantLaunchStrongAttack;
    private ToricObject toricObject;
    [HideInInspector] public bool canLauchWeakAttack = true, canLaunchStrongAttack = true;
    private bool isInvicible => invicibilityCounter > 0;
    private int invicibilityCounter, canLaunchWeakAttackCounter, canLaunchStrongAttackCounter;

    private List<uint> charAlreadyTouchByDash = new List<uint>();
    private int dashCollisionCounter = 0;
    private bool isDashCollisionEnable => dashCollisionCounter <= 0;

    [SerializeField] private bool drawGizmos = true;

    [Header("Fight")]
    [SerializeField] private float dashBumpSpeed = 10f;
    [SerializeField] private float dashInvicibilityOffset = 0.1f;
    [SerializeField, Tooltip("Time during you are invicible and you attack with your dash attack")] private float invicibilityDurationWhenDashing = 0.2f;
    [SerializeField] private Vector2 dashHitboxOffset;
    [SerializeField] private Vector2 dashHitboxSize;
    private int charMask;

    [Header("Inputs")]
    public bool enableAttackWeak = true;
    public bool enableAttackStrong = true;
    public bool enableInvisibilityWhenDashing = true;
    [Tooltip("Temps ou l'on consid�re l'appuye sur un touche alors que le perso est occup�/ne peut pas lanc� l'attaque")][SerializeField] private float castCoyoteTime = 0.2f;

    [HideInInspector] public bool isDashing { get; private set; }

    #region Awake and Start

    private void Awake()
    {
        attackWeak = GetComponent<WeakAttack>();
        attackStrong = GetComponent<StrongAttack>();
        eventController = GetComponent<EventController>();
        playerCommon = GetComponent<PlayerCommon>();
        toricObject = GetComponent<ToricObject>();
        playerInput = GetComponent<CustomPlayerInput>();
        movement = GetComponent<Movement>();
        nbKill = 0;
    }

    private void Start()
    {
        eventController.callBackTouchAttack += OnTouchAttack;
        eventController.callBackHitAttack += OnKillPlayerAttack;
        eventController.callBackBeenTouchAttack += OnBeenTouchAttack;
        eventController.callBackKillByDash += OnKillPlayerByDash;
        eventController.callBackTouchByEnvironnement += OnBeenTouchByEnvironnement;
        eventController.callBackKillByEnvironnement += OnBeenKillByEnvironnement;
        eventController.callBackBeenKillInstant += OnBeenKillInstant;
        eventController.callBackBeenKillByDash += OnBeenKillByDash;
        charMask = LayerMask.GetMask("Char");
        dashCollisionCounter = 0;
    }

    #endregion

    #region Update

    private void Update()
    {
        //attaques
        if(playerInput.attackWeakPressedDown && enableAttackWeak)
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
        if (!isDashing || !isDashCollisionEnable)
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
                DisableDashCollision(0.2f);

                //on applique le bump
                Vector2 bumpDir = (((Vector2)transform.position + dashHitboxOffset) - ((Vector2)fc.transform.position + fc.dashHitboxOffset)).normalized;
                movement.ApplyBump(bumpDir * dashBumpSpeed);
            }
        }
    }

    private void OnDashCollision(FightController fc)
    {
        if(isInvicible)
        {
            DisableDashCollision(0.2f);

            //on applique le bump
            Vector2 bumpDir = (((Vector2)transform.position + dashHitboxOffset) - ((Vector2)fc.transform.position + fc.dashHitboxOffset)).normalized;
            movement.ApplyBump(bumpDir * dashBumpSpeed);
        }
        else
        {
            eventController.OnBeenKillByDash(fc.gameObject);
            fc.GetComponent<EventController>().OnKillByDash(gameObject);
        }
    }

    private void DisableDashCollision(float duration)
    {
        StartCoroutine(DisableDashCollisionCorout(duration));   
    }

    private IEnumerator DisableDashCollisionCorout(float duration)
    {
        dashCollisionCounter++;
        yield return Useful.GetWaitForSeconds(duration);
        dashCollisionCounter--;
    }

    public void StartDashing()
    {
        StopCoroutine(nameof(StartDashing));
        StartCoroutine(StartDashingCorout());
    }

    private IEnumerator StartDashingCorout()
    {
        yield return Useful.GetWaitForSeconds(dashInvicibilityOffset);

        EnableInvicibility();
        isDashing = true;
        yield return Useful.GetWaitForSeconds(invicibilityDurationWhenDashing);
        isDashing = false;
        charAlreadyTouchByDash.Clear();
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
        yield return Useful.GetWaitForSeconds(duration);
        DisableInvicibility();
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

    private void DisableLauchingAttack(in float duration)
    {
        StartCoroutine(Dl(duration));
    }

    private IEnumerator Dl(float duration)
    {
        int weakAttackCount = canLaunchWeakAttackCounter + 1;
        int strongAttackCount = canLaunchStrongAttackCounter + 1;
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

    private void OnTouchAttack(Attack attack, GameObject other)
    {
        if(toricObject.isAClone)
        {
            toricObject.cloner.GetComponent<EventController>().OnTouchAttack(attack, other);
        }
    }

    private void OnKillPlayerAttack(Attack attack, GameObject player) { OnKillPlayer(); }

    private void OnKillPlayerByDash(GameObject playerKilled) => OnKillPlayer();

    private void OnKillPlayer()
    {
        nbKill++;
    }

    private void OnBeenTouchAttack(Attack attack)
    {
        if (isInvicible || attack.gameObject.GetComponent<PlayerCommon>().id == playerCommon.id)
            return;

        EventController otherEventController = attack.GetComponent<EventController>();
        otherEventController.OnHitAttack(attack, gameObject);
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

    private void Death()
    {
        toricObject.RemoveClones();
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos)
            return;

        Gizmos.color = Color.green;
        Hitbox.GizmosDraw((Vector2)transform.position + dashHitboxOffset, dashHitboxSize);
    }
}
