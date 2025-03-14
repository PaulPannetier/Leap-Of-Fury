using UnityEngine;
using Collision2D;
using Collider2D = UnityEngine.Collider2D;
using System.Collections.Generic;

public class ElectricBall : MonoBehaviour
{
    private bool isLinking;
    private PlayerCommon playerCommon;
    private ElectricBallAttack electricBallAttack;
    private ElectricFieldPassif electricFieldPassif;
    private List<uint> charAlreadyTouch;
    private LayerMask charMask;
    private float timeInstanciate = -10f;
    [SerializeField] private float damageRadius = 1f;
    [SerializeField] private float maxDuration = 10f;

    public float visualRadius = 1f;

#if UNITY_EDITOR
    [SerializeField] private bool drawGizmos = true;
#endif

    private void Awake()
    {
        charMask = LayerMask.GetMask("Char");
        charAlreadyTouch = new List<uint>(4);
    }

    public void Launch(ElectricBallAttack attack)
    {
        electricBallAttack = attack;
        playerCommon = electricBallAttack.GetComponent<PlayerCommon>();
        electricFieldPassif = electricBallAttack.GetComponent<ElectricFieldPassif>();
        timeInstanciate = Time.time;
    }

    private void Update()
    {
        if (PauseManager.instance.isPauseEnable)
        {
            timeInstanciate += Time.deltaTime;
            return;
        }

        Collider2D[] cols = PhysicsToric.OverlapCircleAll(transform.position, damageRadius, charMask);
        List<uint> idToKeep = new List<uint>(4);
        foreach (Collider2D col in cols)
        {
            if(col.CompareTag("Char"))
            {
                GameObject player = col.GetComponent<ToricObject>().original;
                uint playerId = player.GetComponent<PlayerCommon>().id;
                idToKeep.Add(playerId);
                if(playerCommon.id != playerId && !charAlreadyTouch.Contains(playerId))
                {
                    charAlreadyTouch.Add(playerId);
                    electricBallAttack.OnCharTouchByElectricBall(player, this);
                }
            }
        }

        for (int i = charAlreadyTouch.Count - 1; i >= 0; i--)
        {
            if (!idToKeep.Contains(charAlreadyTouch[i]))
            {
                charAlreadyTouch.RemoveAt(i);
            }
        }

        if(!isLinking && Time.time - timeInstanciate > maxDuration)
        {
            DestroyBall();
        }
    }

    public void StartLinking()
    {
        isLinking = true;
    }

    public void EndLinking()
    {
        isLinking = false;
        DestroyBall();
    }

    public void DestroyBall()
    {
        electricBallAttack.OnElectricBallDestroy(this);
        electricFieldPassif.OnElectricBallDestroy(this);
        Destroy(gameObject);
    }

    #region Gizmos/OnValidate

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        if(!drawGizmos)
            return;

        Circle.GizmosDraw(transform.position, damageRadius, Color.black);
        Circle.GizmosDraw(transform.position, visualRadius, Color.green);
    }

    private void OnValidate()
    {
        damageRadius = Mathf.Max(0f, damageRadius);
        visualRadius = Mathf.Max(0f, visualRadius);
        maxDuration = Mathf.Max(0f, maxDuration);
    }

#endif

    #endregion
}
