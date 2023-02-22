using System.Collections;
using UnityEngine;

public class FightController : MonoBehaviour
{
    private WeakAttack attackWeak;
    private StrongAttack attackStrong;
    private EventController eventController;
    private PlayerCommon playerCommon;
    private CustomPlayerInput playerInput;

    private int nbKill = 0;
    private float lastInputLauchingWeakAttack = -10f, lastInputLauchingStrongAttack;
    private bool isLaunchingWeakAttack, isLaunchingStrongAttack, wantLauchWeakAttack, wantLaunchStrongAttack;
    private ToricObject toricObject;
    [HideInInspector] public bool canLauchAttackWeakAttack = true, canLaunchStrongAttack = true;
    private bool isInvicible = false;
    private int invicibilityCounter, canLaunchWeakAttackCounter, canLaunchStrongAttackCounter;

    [Header("Inputs")]
    public bool enableAttackWeak = true;
    public bool enableAttackStrong = true;
    [Tooltip("Temps ou l'on considère l'appuye sur un touche alors que le perso est occupé/ne peut pas lancé l'attaque")][SerializeField] private float castCoyoteTime = 0.2f;

    #region Awake and Start

    private void Awake()
    {
        attackWeak = GetComponent<WeakAttack>();
        attackStrong = GetComponent<StrongAttack>();
        eventController = GetComponent<EventController>();
        playerCommon = GetComponent<PlayerCommon>();
        toricObject = GetComponent<ToricObject>();
        playerInput = GetComponent<CustomPlayerInput>();
        nbKill = 0;
    }

    private void Start()
    {
        eventController.callBackTouchAttack += OnTouchAttack;
        eventController.callBackHitAttack += OnKillPlayerAttack;
        eventController.callBackBeenTouchAttack += OnBeenTouchAttack;
        eventController.callBackTouchByEnvironnement += OnBeenTouchByEnvironnement;
        eventController.callBackKillByEnvironnement += OnBeenKillByEnvironnement;
        eventController.callBackBeenKillInstant += OnBeenKillInstant;
    }

    #endregion

    #region Update

    private void Update()
    {
        //attaques
        if(playerInput.attackWeakPressedDown && enableAttackWeak)
        {
            if(canLauchAttackWeakAttack)
            {
                OnBeginWeakAttack();
                if (!attackWeak.Launch(OnEndStrongAttack, OnEndWeakAttack))
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
                if (!attackStrong.Launch(OnEndWeakAttack, OnEndStrongAttack))
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
                if (canLauchAttackWeakAttack)
                {
                    wantLauchWeakAttack = false;
                    OnBeginWeakAttack();
                    if (!attackWeak.Launch(OnEndStrongAttack, OnEndWeakAttack))
                    {
                        canLauchAttackWeakAttack = true;
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
                    if (!attackStrong.Launch(OnEndWeakAttack, OnEndWeakAttack))
                    {
                        canLaunchStrongAttack = true;
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

    #endregion

    #region Invicibility

    public void EnableInvicibility()
    {
        invicibilityCounter++;
        isInvicible = true;
    }

    public void DisableInvicibility()
    {
        invicibilityCounter--;
        isInvicible = invicibilityCounter <= 0;
    }

    public void EnableInvicibility(in float duration)
    {
        StartCoroutine(EnableInvicibility(duration));
    }

    private IEnumerator EnableInvicibility(float duration)
    {
        invicibilityCounter++;
        isInvicible = true;
        yield return Useful.GetWaitForSeconds(duration);
        invicibilityCounter--;
        isInvicible = invicibilityCounter <= 0;
    }

    #endregion

    #region OnBegin/end Strong/Weak attack

    private void OnBeginStrongAttack()
    {
        isLaunchingStrongAttack = true;
        canLaunchStrongAttackCounter--;
        canLaunchStrongAttack = canLaunchStrongAttackCounter >= 0;
    }

    private void OnBeginWeakAttack()
    {
        isLaunchingWeakAttack = true;
        canLaunchWeakAttackCounter--;
        canLauchAttackWeakAttack = canLaunchWeakAttackCounter >= 0;
    }

    private void OnEndStrongAttack()
    {
        if (!isLaunchingStrongAttack)
            return;
        isLaunchingStrongAttack = false;
        canLaunchStrongAttackCounter++;
        canLaunchStrongAttack = canLaunchStrongAttackCounter >= 0;
    }

    private void OnEndWeakAttack()
    {
        if (!isLaunchingWeakAttack)
            return;
        isLaunchingWeakAttack = false;
        canLaunchWeakAttackCounter++;
        canLauchAttackWeakAttack = canLaunchWeakAttackCounter >= 0;
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
        canLauchAttackWeakAttack = canLaunchStrongAttack = false;
        yield return Useful.GetWaitForSeconds(duration);
        canLaunchWeakAttackCounter += weakAttackCount;
        canLaunchStrongAttackCounter += strongAttackCount;
        canLauchAttackWeakAttack = canLaunchWeakAttackCounter >= 0;
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

    private void OnBeenKillInstant(GameObject killer)
    {
        Death();
    }

    private void Death()
    {
        toricObject.RemoveClones();
        Destroy(gameObject);
    }
}
