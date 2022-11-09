using System.Collections;
using UnityEngine;

public class FightController : MonoBehaviour
{
    private Shield shield;
    private WeakAttack attackWeak;
    private StrongAttack attackStrong;
    private EventController eventController;
    private PlayerCommon playerCommon;
    private CustomPlayerInput playerInput;

    private int nbKill = 0;
    private float lastInputLauchingWeakAttack = -10f, lastInputLauchingStrongAttack;
    private bool isLaunchingWeakAttack, isLaunchingStrongAttack, wantLauchWeakAttack, wantLaunchStrongAttack;
    private ToricObject toricObject;
    [HideInInspector] public bool canLauchAttack = true, canEnableShield = true;
    private bool isInvicible = false;
    private int invicibilityCounter = 0, canLaunchAttackCounter = 0, canEnableShieldCounter = 0;

    [Header("Inputs")]
    public bool enableAttackWeak = true;
    public bool enableAttackStrong = true, enableShield = true;
    [Tooltip("Temps ou l'on considère l'appuye sur un touche alors que le perso est occupé/ne peut pas lancé l'attaque")][SerializeField] private float castCoyoteTime = 0.2f;

    #region Awake and Start

    private void Awake()
    {
        shield = GetComponent<Shield>();
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
            if(canLauchAttack)
            {
                if(!attackWeak.Launch(OnEndWeakAttack))
                {
                    lastInputLauchingWeakAttack = Time.time;
                    wantLauchWeakAttack = true;
                }
                else
                {
                    OnBeginWeakAttack();
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
            if (canLauchAttack)
            {
                if (!attackStrong.Launch(OnEndStrongAttack))
                {
                    lastInputLauchingStrongAttack = Time.time;
                    wantLaunchStrongAttack = true;
                }
                else
                {
                    OnBeginStrongAttack();
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
            if((Time.time - lastInputLauchingWeakAttack) < castCoyoteTime)
            {
                if (canLauchAttack)
                {
                    wantLauchWeakAttack = false;
                    if (!attackWeak.Launch(OnEndWeakAttack))
                    {
                        canLauchAttack = true;
                        wantLauchWeakAttack = true;
                    }
                    else
                    {
                        OnBeginWeakAttack();
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
                if (canLauchAttack)
                {
                    wantLaunchStrongAttack = false;
                    if (!attackWeak.Launch(OnEndWeakAttack))
                    {
                        canLauchAttack = true;
                        wantLaunchStrongAttack = true;
                    }
                    else
                    {
                        OnBeginStrongAttack();
                    }
                }
            }
            else
            {
                wantLaunchStrongAttack = false;
            }
        }

        //boucliers
        if (canEnableShield)
        {
            if(playerInput.shieldPressed && enableShield)
            {
                shield.wantEnableShield = true;
            }
            else
            {
                shield.wantEnableShield = false;
            }
        }
        else
        {
            shield.wantEnableShield = false;
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
        canLaunchAttackCounter--;
        canEnableShieldCounter--;
        canLauchAttack = canLaunchAttackCounter <= 0;
        canEnableShield = canEnableShieldCounter <= 0;
    }

    private void OnBeginWeakAttack()
    {
        isLaunchingWeakAttack = true;
        canLaunchAttackCounter--;
        canEnableShieldCounter--;
        canLauchAttack = canLaunchAttackCounter <= 0;
        canEnableShield = canEnableShieldCounter <= 0;
    }

    private void OnEndStrongAttack()
    {
        if (!isLaunchingStrongAttack)
            return;
        isLaunchingStrongAttack = false;
        canLaunchAttackCounter++;
        canEnableShieldCounter++;
        canLauchAttack = canLaunchAttackCounter <= 0;
        canEnableShield = canEnableShieldCounter <= 0;
    }

    private void OnEndWeakAttack()
    {
        if (!isLaunchingWeakAttack)
            return;
        isLaunchingWeakAttack = false;
        canLaunchAttackCounter++;
        canEnableShieldCounter++;
        canLauchAttack = canLaunchAttackCounter <= 0;
        canEnableShield = canEnableShieldCounter <= 0;
    }

    #endregion

    #region Disable Shield/attacking

    private void DisableLauchingAttackAndEnablingShield(in float duration)
    {
        DisableLauchingAttack(duration);
        DisableEnablingShield(duration);
    }

    private void DisableLauchingAttack(in float duration)
    {
        StartCoroutine(Dl(duration));
    }

    private void DisableEnablingShield(in float duration)
    {
        StartCoroutine(Ds(duration));
    }

    private IEnumerator Dl(float duration)
    {
        canLauchAttack = false;
        yield return Useful.GetWaitForSeconds(duration);
        canLauchAttack = true;
    }

    private IEnumerator Ds(float duration)
    {
        canEnableShield = false;
        yield return Useful.GetWaitForSeconds(duration);
        canEnableShield = true;
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

        if (shield.TryBlockAttack(attack))
        {
            eventController.OnBlockAttack(attack);
            attack.GetComponent<EventController>().OnBeenBlockAttack(attack, gameObject);
        }
        else
        {
            EventController otherEventController = attack.GetComponent<EventController>();
            otherEventController.OnHitAttack(attack, gameObject);
            eventController.OnDeath();
            otherEventController.OnKill(gameObject);
            EventManager.instance.OnTriggerPlayerDeath(gameObject, attack.gameObject);
            Death();
        }
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
