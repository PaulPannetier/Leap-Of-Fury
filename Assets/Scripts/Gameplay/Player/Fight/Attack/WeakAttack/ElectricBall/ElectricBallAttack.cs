using System;
using System.Collections.Generic;
using UnityEngine;

public class ElectricBallAttack : WeakAttack
{
    [SerializeField] private ElectricBall electricBallPrefab;
    [SerializeField] private int maxBall;

    public List<ElectricBall> currentBall {  get; private set; }

    protected override void Awake()
    {
        base.Awake();
        currentBall = new List<ElectricBall>(maxBall);
    }

    private void InstanciateBall()
    {
        if(currentBall.Count > maxBall)
        {
            Destroy(currentBall[0]);
            currentBall.RemoveAt(0);
        }

        ElectricBall electricBall = Instantiate(electricBallPrefab, transform.position, Quaternion.identity, CloneParent.cloneParent);
        electricBall.Launch(this);
        currentBall.Add(electricBall);
    }

    public void OnCharTouchByElectricBall(GameObject charTouch, ElectricBall electricBall)
    {
        base.OnTouchEnemy(charTouch, damageType);
    }

    public void OnElectricBallDestroy(ElectricBall electricBall)
    {
        currentBall.Remove(electricBall);
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
