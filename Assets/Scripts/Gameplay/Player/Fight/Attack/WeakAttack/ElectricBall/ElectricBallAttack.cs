using System;
using System.Collections.Generic;
using UnityEngine;

public class ElectricBallAttack : WeakAttack
{
    private LayerMask groundMask;
    private ElectricFieldPassif electricFieldPassif;

    [SerializeField] private ElectricBall electricBallPrefab;
    [SerializeField] private float ballsSpawnDistance;

    public int maxBall;
    [HideInInspector] public List<ElectricBall> currentBalls {  get; private set; }

    protected override void Awake()
    {
        base.Awake();
        currentBalls = new List<ElectricBall>(maxBall);
        electricFieldPassif = GetComponent<ElectricFieldPassif>();
    }

    protected override void Start()
    {
        base.Start();
        groundMask = LayerMask.GetMask("Floor", "WallProjectile");
    }

    private void InstanciateBall()
    {
        if(currentBalls.Count > maxBall)
        {
            currentBalls[0].DestroyBall();
            currentBalls.RemoveAt(0);
        }

        ElectricBall electricBall = Instantiate(electricBallPrefab, transform.position, Quaternion.identity, CloneParent.cloneParent);

        ToricRaycastHit2D raycast = PhysicsToric.CircleCast(transform.position, Vector2.down, electricBall.visualRadius, ballsSpawnDistance, groundMask);
        Vector2 ballPos = raycast ? raycast.centroid : new Vector2(transform.position.x, transform.position.y - ballsSpawnDistance);
        electricBall.transform.position = ballPos;
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

    #region OnValidate

#if UNITY_EDITOR

    protected override void OnValidate()
    {
        base.OnValidate();
        maxBall = Mathf.Max(1, maxBall);
        ballsSpawnDistance = Mathf.Max(0f, ballsSpawnDistance);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * ballsSpawnDistance);
    }

#endif

    #endregion
}
