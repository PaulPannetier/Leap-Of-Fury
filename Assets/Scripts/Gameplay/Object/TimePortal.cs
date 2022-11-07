using System.Collections.Generic;
using UnityEngine;

public class TimePortal : MonoBehaviour
{
    //before entering
    private List<PlayerCommon> charInFront;
    private CustomPlayerInput playerInPortal;
    private float lastTimeCreated;
    private float lastTimePlayerEnter;
    private List<GameObject> visualsPortals;

    //after entering
    private GameObject charToTP;

    [Header("Stats")]
    [SerializeField] private float sleepingTimeWhenEnterPortal = 2f;
    [SerializeField] private float timeToEnterPortal = 3f;
    [SerializeField] private float timeToValidateInPortal = 5f;
    [SerializeField] private float sleepTimeafterTP = 3f;
    [SerializeField] private float invicibilityDuration = 3f;

    [Header("Collision")]
    [SerializeField] private Vector2 offset;
    [SerializeField] private Vector2 size;
    [SerializeField] private LayerMask charMask;

    [HideInInspector] public bool tpChar = false;

    private void Start()
    {
        lastTimeCreated = Time.time;
        charInFront = new List<PlayerCommon>();
    }

    private void Update()
    {
        if (tpChar)
            UpdateAfterEntering();
        else
            UpdateBeforeEntering();
    }

    #region UpdateBefore

    private void UpdateBeforeEntering()
    {
        if (playerInPortal == null)
        {
            if(Time.time - lastTimeCreated > timeToEnterPortal)
            {
                TimePortalManager.instance.isLastPortalActivated = false;
                Destroy(gameObject);
                return;
            }

            //update charInFront
            Collider2D[] cols = PhysicsToric.OverlapCapsuleAll((Vector2)transform.position + offset, size, 0f, charMask);
            charInFront.Clear();
            foreach (Collider2D col in cols)
            {
                if (col.CompareTag("Char"))
                {
                    PlayerCommon pc = col.GetComponent<PlayerCommon>();
                    bool contain = false;
                    foreach (PlayerCommon inPC in charInFront)
                    {
                        if (inPC.id == pc.id)
                        {
                            contain = true;
                            break;
                        }
                    }
                    if (!contain)
                    {
                        charInFront.Add(pc);
                    }
                }
            }

            foreach (PlayerCommon pc in charInFront)
            {
                CustomPlayerInput cpi = pc.GetComponent<CustomPlayerInput>();
                if (cpi.upPressedDown)
                {
                    playerInPortal = cpi;
                    lastTimePlayerEnter = Time.time;
                    //configure playerStatus
                    DisablePlayerStatus();
                    break;
                }
            }
        }
        else
        {
            if (Time.time - lastTimePlayerEnter > sleepingTimeWhenEnterPortal)
            {
                if (visualsPortals == null)
                {
                    visualsPortals = TimePortalManager.instance.CreateVisualTPPortal();

                    foreach (GameObject vp in visualsPortals)
                    {
                        vp.GetComponentInChildren<SpriteRenderer>().color = Color.red;
                    }
                }

                if (Time.time - lastTimePlayerEnter > sleepingTimeWhenEnterPortal + timeToValidateInPortal)
                {
                    //on choisit un portail au pif ou se TP
                    ActivateTpPortal(Random.RandExclude(0, visualsPortals.Count));
                    return;
                }

                int indexPortalChoice = -1;
                if (playerInPortal.rawX != 0 || playerInPortal.rawY != 0)
                {
                    indexPortalChoice = 0;
                    Vector2 inputDir = new Vector2(playerInPortal.x, playerInPortal.y).normalized;
                    float minSqrDistance = visualsPortals[0].transform.position.normalized.SqrDistance(inputDir);
                    for (int i = 1; i < visualsPortals.Count; i++)
                    {
                        float sqrDist = visualsPortals[i].transform.position.normalized.SqrDistance(inputDir);
                        if (sqrDist < minSqrDistance)
                        {
                            minSqrDistance = sqrDist;
                            indexPortalChoice = i;
                        }
                    }

                    if (playerInPortal.dashPressedDown)
                    {
                        ActivateTpPortal(indexPortalChoice);
                        return;
                    }
                }

                void ActivateTpPortal(int index)
                {
                    GameObject portalChoose = TimePortalManager.instance.ActivateTpPortal(index);
                    portalChoose.GetComponent<TimePortal>().charToTP = playerInPortal.gameObject;
                    playerInPortal.GetComponent<EventController>().OnEnterTimePortal(this);
                    Destroy(gameObject);
                }
            }
        }
    }

    #endregion

    #region UpdateAferEntering

    private void UpdateAfterEntering()
    {
        if(Time.time - lastTimeCreated > sleepTimeafterTP)
        {
            EnablePlayerStatus();
            charToTP.GetComponent<Movement>().Teleport((Vector2)transform.position);
            charToTP.GetComponent<FightController>().EnableInvicibility(invicibilityDuration);
            charToTP.GetComponent<EventController>().OnExitTimePortal(this);
            TimePortalManager.instance.isLastPortalActivated = false;
            Destroy(gameObject);
        }
    }

    #endregion

    private void DisablePlayerStatus()
    {
        playerInPortal.GetComponent<SpriteRenderer>().color = Color.clear;
        playerInPortal.GetComponent<Movement>().enableBehaviour = false;
    }

    private void EnablePlayerStatus()
    {
        charToTP.GetComponent<SpriteRenderer>().color = charToTP.GetComponent<PlayerCommon>().color;
        Movement movement = charToTP.GetComponent<Movement>();
        movement.enableBehaviour = movement.enableInput = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Capsule.GizmosDraw(new Capsule((Vector2)transform.position + offset, size));
    }

    private void OnValidate()
    {
        size = new Vector2(Mathf.Max(0f, size.x), Mathf.Max(0f, size.y));
    }
}
