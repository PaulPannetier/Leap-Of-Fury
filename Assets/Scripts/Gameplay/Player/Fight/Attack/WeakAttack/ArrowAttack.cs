using System;
using System.Collections;
using UnityEngine;

public class ArrowAttack : WeakAttack
{
    private Movement movement;
    [SerializeField] private int nbArrow;
    private bool arrowIsFlying = false;
    private Arrow arrowWhoFly;

    [SerializeField] private GameObject arrowPrefab;

    [SerializeField] private float arrowLaunchDistance = 0.2f;
    public float delayBetweenLauchAndRecoverArrow = 0.7f;
    [SerializeField] private float arrowInitSpeed = 4f;
    [SerializeField] private int initArrow = 1;
    [SerializeField, Tooltip("L'angle entre les arrow lors de la réactivation"), Range(0f, 180f)] private float arrowActivationAngle = 15f;

    protected override void Awake()
    {
        base.Awake();
        movement = GetComponent<Movement>();
        nbArrow = initArrow;
    }

    public override bool Launch(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
        if(arrowIsFlying)
        {
            Vector2 arrowPos = arrowWhoFly.transform.position;
            float currentAngle = Useful.AngleHori(Vector2.zero, arrowWhoFly.rb.velocity) * Mathf.Rad2Deg;
            float a2 = currentAngle + arrowActivationAngle;
            float a3 = currentAngle - arrowActivationAngle;
            float speed = arrowWhoFly.rb.velocity.magnitude;

            Arrow newArrow1 = Instantiate(arrowPrefab, arrowPos, Quaternion.Euler(0f, 0f, a2), CloneParent.cloneParent).GetComponent<Arrow>();
            Arrow newArrow2 = Instantiate(arrowPrefab, arrowPos, Quaternion.Euler(0f, 0f, a3), CloneParent.cloneParent).GetComponent<Arrow>();
            newArrow1.Launch(this, Useful.Vector2FromAngle(a2 * Mathf.Deg2Rad), speed, false);
            newArrow2.Launch(this, Useful.Vector2FromAngle(a3 * Mathf.Deg2Rad), speed, false);

            arrowWhoFly.OnRelaunch();

            arrowWhoFly = null;
            arrowIsFlying = false;
            callbackEnableOtherAttack.Invoke();
            callbackEnableThisAttack.Invoke();
            return true;
        }

        if (!cooldown.isActive)
        {
            callbackEnableOtherAttack.Invoke();
            callbackEnableThisAttack.Invoke();
            return false;
        }

        if (nbArrow > 0)
        {
            base.Launch(callbackEnableOtherAttack, callbackEnableThisAttack);
            Vector2 dir = movement.GetCurrentDirection(true);
            arrowIsFlying = true;
            GameObject newArrow = Instantiate(arrowPrefab, transform.position + (Vector3)(dir * arrowLaunchDistance), Quaternion.identity, CloneParent.cloneParent);
            arrowWhoFly = newArrow.GetComponent<Arrow>();
            arrowWhoFly.Launch(this, dir, arrowInitSpeed);
            nbArrow--;
            cooldown.Reset();
            StartCoroutine(WaitEndAttack(callbackEnableOtherAttack, callbackEnableThisAttack));
            return true;
        }

        callbackEnableOtherAttack.Invoke();
        callbackEnableThisAttack.Invoke();
        return false;
    }

    private IEnumerator WaitEndAttack(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
        float timeCounter = 0f;
        while (timeCounter < castDuration)
        {
            yield return null;
            if (!PauseManager.instance.isPauseEnable)
            {
                timeCounter += Time.deltaTime;
            }
        }

        callbackEnableOtherAttack.Invoke();
        callbackEnableThisAttack.Invoke();
    }

    public void RecoverArrow()
    {
        nbArrow++;
    }

    public void RecoverArrowInAir()
    {
        OnArrowLand();
        RecoverArrow();
    }

    public void OnArrowLand()
    {
        arrowIsFlying = false;
        arrowWhoFly = null;
    }

    public void OnArrowTouchChar(GameObject player)
    {
        arrowWhoFly = null;
        arrowIsFlying = false;
    }

    #region OnValidate

#if UNITY_EDITOR

    private void OnValidate()
    {
        arrowLaunchDistance = Mathf.Max(0f, arrowLaunchDistance);
        initArrow = Mathf.Max(0, initArrow);
    }

#endif

#endregion
}
