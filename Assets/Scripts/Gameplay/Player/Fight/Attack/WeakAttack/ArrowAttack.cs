using System;
using System.Collections;
using UnityEngine;

public class ArrowAttack : WeakAttack
{
    private Movement movement;
    private int nbArrow;

    [SerializeField] private GameObject arrowPrefab;

    [SerializeField] private float arrowLaunchDistance = 0.2f;
    [SerializeField] private float arrowInitSpeed = 4f;
    [SerializeField] private int initArrow = 1;

    protected override void Awake()
    {
        base.Awake();
        movement = GetComponent<Movement>();
        nbArrow = initArrow;
    }

    public override bool Launch(Action callbackEnd)
    {
        if(!cooldown.isActive)
        {
            callbackEnd.Invoke();
            return false;
        }
        if(nbArrow > 0)
        {
            base.Launch(callbackEnd);
            Vector2 dir = movement.GetCurrentDirection();
            GameObject newArrow = Instantiate(arrowPrefab, transform.position + (Vector3)(dir * arrowLaunchDistance), Quaternion.identity, CloneParent.cloneParent);
            Arrow arrowComp = newArrow.GetComponent<Arrow>();
            arrowComp.Launch(this, dir, arrowInitSpeed);
            nbArrow--;
            cooldown.Reset();
            StartCoroutine(WaitEndAttack(callbackEnd));
            return true;
        }
        return false;
    }

    private IEnumerator WaitEndAttack(Action callbackEnd)
    {
        yield return new WaitForSeconds(castDuration);
        callbackEnd.Invoke();
    }

    public void RecoverArrow()
    {
        nbArrow++;
    }

    private void OnValidate()
    {
        arrowLaunchDistance = Mathf.Max(0f, arrowLaunchDistance);
        initArrow = Mathf.Max(0, initArrow);
    }
}
