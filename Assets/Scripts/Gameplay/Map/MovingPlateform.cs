using UnityEngine;
using PathFinding;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlateform : PathFindingBlocker
{
    private Rigidbody2D rb;
    private BoxCollider2D hitbox;

    public bool enableBehaviour = true;

    [SerializeField] private float speedLerp;

    public Vector2 targetVelocity = Vector2.up;

    protected override void Awake()
    {
        base.Awake();
        hitbox = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        PauseManager.instance.callBackOnPauseDisable += Enable;
        PauseManager.instance.callBackOnPauseEnable += Disable;
    }

    public override List<MapPoint> GetBlockedCells()
    {
        return GetBlockedCellsInRectangle(transform.position, hitbox.size);
    }

    private void FixedUpdate()
    {
        if (!enableBehaviour)
            return;

        rb.velocity = Vector2.MoveTowards(rb.velocity, targetVelocity, speedLerp * Time.fixedDeltaTime);
    }

    private void Disable()
    {
        enableBehaviour = false;
    }

    private void Enable()
    {
        enableBehaviour = true;
    }

    private void OnDestroy()
    {
        PauseManager.instance.callBackOnPauseEnable -= Disable;
        PauseManager.instance.callBackOnPauseDisable -= Enable;
    }
}
