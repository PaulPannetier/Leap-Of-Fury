using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class ElectricLinkAttack : StrongAttack
{
    private List<ElectricLink> currentLink;
    private LayerMask charMask;
    private bool isLinking;
    private Action callbackEnableOtherAttack, callbackEnableThisAttack;
    private ElectricBallAttack electricBallAttack;

    [SerializeField] private float arcDuration = 0.5f;
    [SerializeField] private float electricBallExplosionRadius = 1f;
    [SerializeField] private float electricBallExplosionForce = 1f;
    [SerializeField] private LineRenderer arcPrefabs;

    protected override void Awake()
    {
        base.Awake();
        electricBallAttack = GetComponent<ElectricBallAttack>();
        currentLink = new List<ElectricLink>(electricBallAttack.maxBall);
        charMask = LayerMask.GetMask("Char");
    }

    protected override void Update()
    {
        base.Update();

        if(isLinking)
        {
            foreach (ElectricLink link in currentLink)
            {
                Vector2 start = link.start.transform.position;
                Vector2 dir = (Vector2)link.end.transform.position - start;
                float length = dir.magnitude;
                ToricRaycastHit2D[] raycasts = PhysicsToric.RaycastAll(start, dir, length, charMask);
                foreach (ToricRaycastHit2D raycast in raycasts)
                {
                    if(raycast.collider == null || !raycast.collider.CompareTag("Char")) 
                        continue;

                    GameObject player = raycast.collider.GetComponent<ToricObject>().original;
                    uint id = player.GetComponent<PlayerCommon>().id;
                    if(playerCommon.id != id && !link.charAlreadyTouch.Contains(id))
                    {
                        link.charAlreadyTouch.Add(id);
                        base.OnTouchEnemy(player, damageType);
                    }
                }
            }
        }
    }



    public override bool Launch(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
        List<ElectricBall> electricBalls = electricBallAttack.currentBalls;
        if (!cooldown.isActive || electricBalls.Count <= 1)
        {
            callbackEnableOtherAttack.Invoke();
            callbackEnableThisAttack.Invoke();
            return false;
        }

        this.callbackEnableOtherAttack = callbackEnableOtherAttack;
        this.callbackEnableThisAttack = callbackEnableThisAttack;
        StartCoroutine(EndAttackCorout());
        isLinking = true;

        CreateLink();

        return true;
    }

    private void CreateLink()
    {
        List<ElectricBall> electricBalls = electricBallAttack.currentBalls;
        currentLink.Clear();
        for (int i = 0; i < electricBalls.Count - 1; i++)
        {
            currentLink.Add(new ElectricLink(electricBalls[i], electricBalls[i + 1]));
            electricBalls[i].StartLinking();
        }
        electricBalls.Last().StartLinking();


    }

    private void HandleElectricBallExplosion(ElectricBall electricBall)
    {
        Vector2 explosionPosition = electricBall.transform.position;
        ExplosionManager.instance.CreateExplosion(explosionPosition, electricBallExplosionForce);

        Collider2D[] cols = PhysicsToric.OverlapCircleAll(explosionPosition, electricBallExplosionRadius, charMask);
        foreach (Collider2D col in cols)
        {
            if(col.CompareTag("Char"))
            {
                GameObject player = col.GetComponent<ToricObject>().original;
                uint playerId = player.GetComponent<PlayerCommon>().id;
                if(playerId != playerCommon.id)
                {
                    base.OnTouchEnemy(player, damageType);
                }
            }
        }
    }

    private IEnumerator EndAttackCorout()
    {
        yield return PauseManager.instance.Wait(arcDuration);

        currentLink.Clear();
        isLinking = false;
        foreach (ElectricBall electricBall in electricBallAttack.currentBalls)
        {
            HandleElectricBallExplosion(electricBall);
            electricBall.EndLinking();
        }

        callbackEnableOtherAttack.Invoke();
        callbackEnableThisAttack.Invoke();
        cooldown.Reset();
    }

    #region OnValidate

#if UNITY_EDITOR

    protected override void OnValidate()
    {
        base.OnValidate();
        arcDuration = Mathf.Max(0f, arcDuration);
        electricBallExplosionForce = Mathf.Max(0f, electricBallExplosionForce);
        electricBallExplosionRadius = Mathf.Max(0f, electricBallExplosionRadius);
    }

#endif

    #endregion

    #region Private Struct

    private struct ElectricLink
    {
        public ElectricBall start, end;
        public List<uint> charAlreadyTouch;

        public ElectricLink(ElectricBall start, ElectricBall end)
        {
            this.start = start;
            this.end = end;
            charAlreadyTouch = new List<uint>(4);
        }
    }

    #endregion

}
