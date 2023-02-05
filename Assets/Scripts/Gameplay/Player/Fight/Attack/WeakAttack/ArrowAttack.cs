using System;
using System.Collections;
using UnityEngine;

public class ArrowAttack : WeakAttack
{
    private Movement movement;
    private int nbArrow;
    private bool arrowIsFlying = false;
    private Arrow arrowWhoFly;

    [SerializeField] private GameObject arrowPrefab;

    [SerializeField] private float arrowLaunchDistance = 0.2f;
    [SerializeField] private float arrowInitSpeed = 4f;
    [SerializeField] private int initArrow = 1;
    [SerializeField, Tooltip("L'angle entre les arrow lors de la réactivation")] private float arrowActivationAngle = 15f;

    protected override void Awake()
    {
        base.Awake();
        movement = GetComponent<Movement>();
        nbArrow = initArrow;
    }

    public override bool Launch(Action callbackEnd)
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
            newArrow2.Launch(this, Useful.Vector2FromAngle(a2 * Mathf.Deg2Rad), speed, false);

            arrowWhoFly.OnRelaunch();

            arrowWhoFly = null;
            arrowIsFlying = false;
            callbackEnd.Invoke();
            return false;
        }

        if(!cooldown.isActive)
        {
            callbackEnd.Invoke();
            return false;
        }
        if(nbArrow > 0)
        {
            base.Launch(callbackEnd);
            Vector2 dir = movement.GetCurrentDirection(true);
            arrowIsFlying = true;
            GameObject newArrow = Instantiate(arrowPrefab, transform.position + (Vector3)(dir * arrowLaunchDistance), Quaternion.identity, CloneParent.cloneParent);
            arrowWhoFly = newArrow.GetComponent<Arrow>();
            arrowWhoFly.Launch(this, dir, arrowInitSpeed);
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

    public void OnArrowLand()
    {
        arrowIsFlying = false;
        arrowWhoFly = null;
    }

    public void OnArrowTouchChar(GameObject player)
    {
        arrowWhoFly = null;
    }

    private void OnValidate()
    {
        arrowLaunchDistance = Mathf.Max(0f, arrowLaunchDistance);
        initArrow = Mathf.Max(0, initArrow);
    }
}
