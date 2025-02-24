using UnityEngine;
using System.Collections;
using System;

public class ElectricLinkAttack : StrongAttack
{
    private bool isLinking;
    private Action callbackEnableOtherAttack, callbackEnableThisAttack;

    [SerializeField] private float arcDuration = 0.5f;

    public override bool Launch(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
        if (!cooldown.isActive)
        {
            callbackEnableOtherAttack.Invoke();
            callbackEnableThisAttack.Invoke();
            return false;
        }

        this.callbackEnableOtherAttack = callbackEnableOtherAttack;
        this.callbackEnableThisAttack = callbackEnableThisAttack;
        StartCoroutine(EndAttackCorout());
        isLinking = true;

        return true;
    }

    private IEnumerator EndAttackCorout()
    {
        yield return PauseManager.instance.Wait(arcDuration);

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
    }

#endif

    #endregion

    #region Private Struct

    private struct Link
    {
        public ElectricBall start, end;

        public Link(ElectricBall start, ElectricBall end)
        {
            this.start = start;
            this.end = end;
        }
    }

    #endregion

}
