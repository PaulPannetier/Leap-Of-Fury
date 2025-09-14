using System;
using System.Collections;
using UnityEngine;

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

    [SerializeField] private float castDuration;
    [SerializeField] private float maxJumpSpeed;
    [SerializeField] private float jumpDuration;
    [SerializeField] private AnimationCurve jumpSpeedOverTime;
    [SerializeField] private float ceilRaycastLength;

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
            default:
                return;
        }
    }

    private void HandleJump()
    {
        ToricRaycastHit2D raycast = PhysicsToric.Raycast(transform.position, Vector2.up, ceilRaycastLength, groundMask);
        if(raycast)
        {

        }

        float percent = Mathf.Clamp01((Time.time - lastTimeStartJumping) / jumpDuration);
        Vector2 speed = maxJumpSpeed * jumpSpeedOverTime.Evaluate(percent) * Vector2.up;
        
    }

    private void HandleExplode()
    {

    }

    #region Gizmos / OnValidate

#if UNITY_EDITOR

    protected void OnDrawGizmosSelected()
    {
        this.transform = base.transform;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + (ceilRaycastLength * Vector2.up));
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        this.transform = base.transform;
        castDuration = Mathf.Max(0f, castDuration);
        maxJumpSpeed = Mathf.Max(0f, maxJumpSpeed);
        jumpDuration = Mathf.Max(0f, jumpDuration);
        ceilRaycastLength = Mathf.Max(0f, ceilRaycastLength);
    }

#endif

    #endregion
}
