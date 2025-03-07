using System;
using System.Collections.Generic;
using UnityEngine;

public class ElectricBallAttack : WeakAttack
{
    private ElectricFieldPassif electricFieldPassif;

    [SerializeField] private ElectricBall electricBallPrefab;

    public int maxBall;
    [HideInInspector] public List<ElectricBall> currentBalls {  get; private set; }

    protected override void Awake()
    {
        base.Awake();
        currentBalls = new List<ElectricBall>(maxBall);
        electricFieldPassif = GetComponent<ElectricFieldPassif>();
    }

    private void InstanciateBall()
    {
        if(currentBalls.Count > maxBall)
        {
            Destroy(currentBalls[0]);
            currentBalls.RemoveAt(0);
        }

        ElectricBall electricBall = Instantiate(electricBallPrefab, transform.position, Quaternion.identity, CloneParent.cloneParent);
        electricBall.Launch(this);
        electricFieldPassif.OnElectricBallCreate(electricBall);
        currentBalls.Add(electricBall);
    }

    public void OnCharTouchByElectricBall(GameObject charTouch, ElectricBall electricBall)
    {
        base.OnTouchEnemy(charTouch, damageType);
    }

    public void OnElectricBallDestroy(ElectricBall electricBall)
    {
        currentBalls.Remove(electricBall);
    }

    public override bool Launch(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
        if (!cooldown.isActive)
        {
            callbackEnableOtherAttack.Invoke();
            callbackEnableThisAttack.Invoke();
            return false;
        }

        cooldown.Reset();
        callbackEnableOtherAttack.Invoke();
        callbackEnableThisAttack.Invoke();

        InstanciateBall();

        return true;
    }

#if UNITY_EDITOR

    protected override void OnValidate()
    {
        base.OnValidate();
        maxBall = Mathf.Max(1, maxBall);
    }

#endif
}
