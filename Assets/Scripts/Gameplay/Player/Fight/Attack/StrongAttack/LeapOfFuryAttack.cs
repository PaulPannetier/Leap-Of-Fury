using Collision2D;
using System;
using System.Collections;
using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class LeapOfFuryAttack : StrongAttack
{
    private enum State : byte
    {
        None,
        Jump,
        Explode
    }

    private float lastTimeStartJumping;
    private CharacterController charController;
    private State state = State.None;
    private LayerMask groundMask;
    private new Transform transform;
    private float lastTimeExplode;
    private int explosionIndex;
    private Vector2 explosionDir;

    [SerializeField] private float castDuration;
    [SerializeField] private float maxJumpSpeed;
    [SerializeField] private float jumpDuration;
    [SerializeField] private AnimationCurve jumpSpeedOverTime;
    [SerializeField] private float ceilRaycastLength;
    [SerializeField] private Explosion explosionPrefab;
    [SerializeField] private Explosion.ExplosionData[] explosionData = Array.Empty<Explosion.ExplosionData>();
    [SerializeField] private float[] explosionDistances = Array.Empty<float>();
    [SerializeField] private float durationBetweenExplosion;

    protected override void Awake()
    {
        base.Awake();
        this.transform = base.transform;
    }

    protected override void Start()
    {
        base.Start();
        charController = GetComponent<CharacterController>();
        groundMask = LayerMask.GetMask("Floor", "WallProjectile");
    }

    public override bool Launch(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
        if (!cooldown.isActive)
        {
            callbackEnableOtherAttack.Invoke();
            callbackEnableThisAttack.Invoke();
            return false;
        }

        StartCoroutine(LaunchCoroutine(callbackEnableOtherAttack, callbackEnableThisAttack));
        return true;
    }

    private IEnumerator LaunchCoroutine(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
        charController.Freeze();

        yield return PauseManager.instance.Wait(castDuration);

        lastTimeStartJumping = Time.time;
        state = State.Jump;

        callbackEnableOtherAttack.Invoke();
        callbackEnableThisAttack.Invoke();
    }

    protected override void Update()
    {
        base.Update();
        if (PauseManager.instance.isPauseEnable)
        {
            lastTimeStartJumping += Time.deltaTime;
            lastTimeExplode += Time.deltaTime;
            return;
        }

        switch (state)
        {
            case State.Jump:
                HandleJump();
                break;
            case State.Explode:
                HandleExplode();
                break;
            case State.None:
                return;
            default:
                return;
        }
    }

    private void HandleJump()
    {
        ToricRaycastHit2D raycast = PhysicsToric.Raycast(transform.position, Vector2.up, ceilRaycastLength, groundMask);
        if(Time.time - lastTimeStartJumping >= jumpDuration || raycast)
        {
            StartExplode();
            return;
        }

        float percent = Mathf.Clamp01((Time.time - lastTimeStartJumping) / jumpDuration);
        Vector2 speed = maxJumpSpeed * jumpSpeedOverTime.Evaluate(percent) * Vector2.up;
        transform.position = (Vector2)transform.position + (Time.deltaTime * speed);
    }

    private void OnPlayerTouchByExplosion(UnityEngine.Collider2D collider)
    {
        OnTouchEnemy(collider.gameObject, damageType);
    }

    private void CreateExplosion()
    {
        lastTimeExplode = Time.time;
        Vector2 pos = (Vector2)transform.position + (explosionDir * explosionDistances[explosionIndex]);
        Quaternion rot = Quaternion.Euler(0f, 0f, Random.RandExclude(0f, 360f));
        Explosion explosion = Instantiate(explosionPrefab, pos, rot);
        explosion.Launch(explosionData[explosionIndex]);
        explosion.callbackOnTouch += OnPlayerTouchByExplosion;
        explosion.transform.localScale *= explosionData[explosionIndex].radius;
    }

    private void StartExplode()
    {
        explosionIndex = 0;
        explosionDir = charController.GetCurrentDirection(true);
        CreateExplosion();
    }

    private void HandleExplode()
    {
        if(Time.time - lastTimeExplode >= durationBetweenExplosion)
        {
            explosionIndex++;
            if(explosionIndex >= explosionData.Length)
            {
                state = State.None;
                charController.UnFreeze();
                return;
            }
            CreateExplosion();
        }
    }

    #region Gizmos / OnValidate

#if UNITY_EDITOR

    protected void OnDrawGizmosSelected()
    {
        this.transform = base.transform;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + (ceilRaycastLength * Vector2.up));

        for (int i = 0; i < Mathf.Min(explosionDistances.Length, explosionData.Length); i++)
        {
            Circle.GizmosDraw((Vector2)transform.position + (Vector2.right * explosionDistances[i]), explosionData[i].radius, Color.green, true);
        }
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        this.transform = base.transform;
        castDuration = Mathf.Max(0f, castDuration);
        maxJumpSpeed = Mathf.Max(0f, maxJumpSpeed);
        jumpDuration = Mathf.Max(0f, jumpDuration);
        ceilRaycastLength = Mathf.Max(0f, ceilRaycastLength);
        durationBetweenExplosion = Mathf.Max(0f, durationBetweenExplosion);
    }

#endif

    #endregion
}
