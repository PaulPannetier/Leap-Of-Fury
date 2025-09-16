using Collision2D;
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
    private CharacterInputs inputs;
    private BoxCollider2D hitbox;
    private State state = State.None;
    private LayerMask groundMask;
    private new Transform transform;
    private float lastTimeExplode;
    private int explosionIndex;
    private Vector2 explosionDir;
    private Action callbackEnableOtherAttack, callbackEnableThisAttack;
    private ToricObject toricObject;
    private Vector2 speed;

    [SerializeField] private float castDuration;
    [SerializeField] private float maxJumpSpeed;
    [SerializeField] private float jumpDuration;
    [SerializeField] private AnimationCurve jumpSpeedOverTime;
    [SerializeField] private float ceilingBoxHeight;
    [SerializeField] private Explosion explosionPrefab;
    [SerializeField] private Explosion.ExplosionData[] explosionData = Array.Empty<Explosion.ExplosionData>();
    [SerializeField] private float[] explosionDistances = Array.Empty<float>();
    [SerializeField] private float durationBetweenExplosion;

    [Header("Horizontal control"), SerializeField, Range(0f, 1f)]
    private float initSpeed;
    [SerializeField] private float horizontalSpeed;
    [SerializeField] private float horizontalSpeedLerp;
    [SerializeField] private Vector2 hitboxOffset;
    [SerializeField] private float hitboxWidth;


    protected override void Awake()
    {
        base.Awake();
        this.transform = base.transform;
        state = State.None;
    }

    protected override void Start()
    {
        base.Start();
        charController = GetComponent<CharacterController>();
        inputs = GetComponent<CharacterInputs>();
        toricObject = GetComponent<ToricObject>();
        hitbox = GetComponent<BoxCollider2D>();
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
        this.callbackEnableOtherAttack = callbackEnableOtherAttack;
        this.callbackEnableThisAttack = callbackEnableThisAttack;
        charController.Freeze();

        yield return PauseManager.instance.Wait(castDuration);

        lastTimeStartJumping = Time.time;
        state = State.Jump;
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
            default:
                return;
        }

        toricObject.CustomUpdate();
    }

    private void HandleJump()
    {
        Vector2 center = (Vector2)transform.position + (0.5f * ceilingBoxHeight * Vector2.up);
        bool isCeilingUp = PhysicsToric.OverlapBox(center, new Vector2(hitbox.size.x, ceilingBoxHeight), 0f, groundMask) != null;
        if (isCeilingUp || Time.time - lastTimeStartJumping >= jumpDuration)
        {
            StartExplode();
            return;
        }

        // vertical Speed
        float percent = Mathf.Clamp01((Time.time - lastTimeStartJumping) / jumpDuration);
        speed.y = maxJumpSpeed * jumpSpeedOverTime.Evaluate(percent);

        // horizontal speed
        if(inputs.rawX != speed.x.Sign())
        {
            speed.x = initSpeed * horizontalSpeed * inputs.rawX;
        }
        speed.x = Mathf.MoveTowards(speed.x, horizontalSpeed * inputs.rawX, Time.deltaTime * horizontalSpeedLerp);

        bool collideSide = PhysicsToric.OverlapBox((Vector2)transform.position + hitboxOffset, new Vector2(hitboxWidth, hitbox.size.y), 0f, groundMask);
        collideSide = collideSide || PhysicsToric.OverlapBox((Vector2)transform.position + new Vector2(-hitboxOffset.x, hitboxOffset.y), new Vector2(hitboxWidth, hitbox.size.y), 0f, groundMask);
        if (collideSide)
            speed.x = 0f;

        transform.position = (Vector2)transform.position + (Time.deltaTime * speed);
    }

    private void OnPlayerTouchByExplosion(UnityEngine.Collider2D collider)
    {
        print($"Touch!{collider.gameObject.name}");
        OnTouchEnemy(collider.gameObject, damageType);
    }

    private void CreateExplosion()
    {
        lastTimeExplode = Time.time;
        Vector2 pos = (Vector2)transform.position + (explosionDir * explosionDistances[explosionIndex]);
        Quaternion rot = Quaternion.Euler(0f, 0f, Random.RandExclude(0f, 360f));
        Explosion explosion = Instantiate(explosionPrefab, pos, rot, CloneParent.cloneParent);
        explosion.Launch(explosionData[explosionIndex]);
        explosion.callbackOnTouch += OnPlayerTouchByExplosion;
        explosion.transform.localScale *= 2f * explosionData[explosionIndex].radius;
    }

    private void StartExplode()
    {
        explosionIndex = 0;
        explosionDir = charController.GetCurrentDirection(true);
        state = State.Explode;
        CreateExplosion();
    }

    private void FinishAttack()
    {
        callbackEnableOtherAttack.Invoke();
        callbackEnableThisAttack.Invoke();
        state = State.None;
        cooldown.Reset();
        charController.UnFreeze();
    }

    private void HandleExplode()
    {
        if(Time.time - lastTimeExplode >= durationBetweenExplosion)
        {
            explosionIndex++;
            if(explosionIndex >= explosionData.Length)
            {
                FinishAttack();
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
        hitbox = GetComponent<BoxCollider2D>();
        Vector2 center = (Vector2)transform.position + (0.5f * ceilingBoxHeight * Vector2.up);
        Hitbox.GizmosDraw(center, new Vector2(hitbox.size.x, ceilingBoxHeight), Color.red, true);
        Hitbox.GizmosDraw((Vector2)transform.position + hitboxOffset, new Vector2(hitboxWidth, hitbox.size.y), Color.red, true);
        Hitbox.GizmosDraw((Vector2)transform.position + new Vector2(-hitboxOffset.x, hitboxOffset.y), new Vector2(hitboxWidth, hitbox.size.y), Color.red, true);


        for (int i = 0; i < Mathf.Min(explosionDistances.Length, explosionData.Length); i++)
        {
            Circle.GizmosDraw((Vector2)transform.position + (Vector2.right * explosionDistances[i]), explosionData[i].radius, Color.green, true);
        }
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        this.transform = base.transform;
        hitbox = GetComponent<BoxCollider2D>();
        castDuration = Mathf.Max(0f, castDuration);
        maxJumpSpeed = Mathf.Max(0f, maxJumpSpeed);
        jumpDuration = Mathf.Max(0f, jumpDuration);
        ceilingBoxHeight = Mathf.Max(0f, ceilingBoxHeight);
        durationBetweenExplosion = Mathf.Max(0f, durationBetweenExplosion);
        horizontalSpeedLerp = Mathf.Max(0f, horizontalSpeedLerp);
        horizontalSpeed = Mathf.Max(0f, horizontalSpeed);
        hitboxWidth = Mathf.Max(0f, hitboxWidth);

        float totalDuration = 0f;
        for (int i = explosionData.Length - 1; i >= 0; i--)
        {
            totalDuration += durationBetweenExplosion;
            explosionData[i].duration = totalDuration;
        }
    }

#endif

    #endregion
}
