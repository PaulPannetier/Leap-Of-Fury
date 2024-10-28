using UnityEngine;
using PathFinding;
using System.Collections.Generic;

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

    public override List<MapPoint> GetBlockedCells(Map map)
    {
        return GetBlockedCellsInRectangle(map, transform.position, hitbox.size - LevelMapData.currentMap.cellSize * 0.1f);
    }

    private void FixedUpdate()
    {
        if (!enableBehaviour)
            return;

        rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, targetVelocity, speedLerp * Time.fixedDeltaTime);
    }

    private void Disable()
    {
        enableBehaviour = false;
    }

    private void Enable()
    {
        enableBehaviour = true;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        PauseManager.instance.callBackOnPauseEnable -= Disable;
        PauseManager.instance.callBackOnPauseDisable -= Enable;
    }
}
