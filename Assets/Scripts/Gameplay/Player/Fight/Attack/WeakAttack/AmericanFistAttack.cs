using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Collision2D;

public class AmericanFistAttack : WeakAttack
{
    private CharacterController charController;
    private CustomPlayerInput playerInput;
    private CloneAttack cloneAttack;//null if isAClone == true
    [HideInInspector] public CloneAttack originalCloneAttack;
    private AmericanFistAttack originalAmericanFistAttack;
    private bool isAttackEnable = false, onLaunchAttack = false, isDashing = false, wantDash = false;
    private int indexDash = 0;
    private Vector2 lastDir, currentSpeed, lastVelocityForPause;
    private float lastTimeDash = -10;
    private LayerMask groundMask, charMask;
    private bool alreadyCreateExplosionWinthThisDash;

#if UNITY_EDITOR

    [SerializeField] private bool drawGizmos = true;

#endif

    [SerializeField] private bool isAClone;
    [SerializeField] private float dashSpeed = 10f, dashDuration = 0.4f, minTimeBetweenDash = 0.2f, maxTimeBetweenDash = 0.7f, dashBufferTime = 0.1f;
    [SerializeField] private AnimationCurve dashSpeedCurve;
    [SerializeField] private int nbDash = 3;
    [SerializeField] private float explosionForce = 1.1f;
    [SerializeField] private Explosion explosionPrefabs;

    [Header("Collission")]
    [SerializeField] private Vector2 colliderOffset;
    [SerializeField] private Vector2 colliderSize;
    [SerializeField] private float groundDetectionRadius = 0.1f;

    [HideInInspector] public bool activateCloneDash;//true one frame when the original char dash
    [HideInInspector] public bool activateWallExplosion;
    [HideInInspector] public Vector2 cloneExplosionPosition;

    private GameObject _original;
    [HideInInspector] public GameObject original
    {
        get => _original;
        set
        {
            _original = value;
            playerCommon = _original.GetComponent<PlayerCommon>();
            originalAmericanFistAttack = _original.GetComponent<AmericanFistAttack>();
        }
    }

    #region Awake/Start

    protected override void Awake()
    {
        if (isAClone)
            return;
        base.Awake();

        playerInput = GetComponent<CustomPlayerInput>();
        charController = GetComponent<CharacterController>();
        cloneAttack = GetComponent<CloneAttack>();
        originalCloneAttack = cloneAttack;
    }

    protected override void Start()
    {
        if (isAClone)
            return;

        base.Start();
        groundMask = LayerMask.GetMask("Floor", "WallProjectile");
        charMask = LayerMask.GetMask("Char");
        PauseManager.instance.callBackOnPauseDisable += OnPauseDisable;
        PauseManager.instance.callBackOnPauseEnable += OnPauseEnable;
    }

    #endregion

    #region Update

    protected override void Update()
    {
        if(PauseManager.instance.isPauseEnable)
        {
            lastTimeDash += Time.deltaTime;
            return;
        }

        if (isAClone)
        {
            UpdateClone();
        }
        else
        {
            UpdateOriginal();
        }
    }

    private void UpdateClone()
    {
        if (isDashing)
        {
            if(originalCloneAttack.isCloneAttackEnable)
            {
                if (CollideWithEnemy(out GameObject[] enemies))
                {
                    foreach (GameObject enemy in enemies)
                    {
                        OnTouchEnemy(enemy);
                    }
                }
            }

            if (Time.time - lastTimeDash > dashDuration)
            {
                isDashing = false;
            }
        }
        else
        {
            if(activateCloneDash)
            {
                StartCoroutine(ApplyAttackCloneCorout());
                activateCloneDash = false;
            }
        }

        if (activateWallExplosion)
        {
            activateWallExplosion = false;
            CreateExplosion(cloneExplosionPosition, !originalCloneAttack.isCloneAttackEnable);
        }
    }

    private void UpdateOriginal()
    {
        base.Update();

        if (isAttackEnable)
        {
            if (onLaunchAttack)
            {
                isDashing = true;
                lastTimeDash = Time.time;
                onLaunchAttack = false;
                alreadyCreateExplosionWinthThisDash = false;
                indexDash = 0;
                charController.Freeze();
            }

            if (isDashing)
            {
                if (Time.time - lastTimeDash > dashDuration)
                {
                    EndCurrentDash();
                }
                else
                {
                    currentSpeed = lastDir * dashSpeed * dashSpeedCurve.Evaluate(Mathf.Clamp01((Time.time - lastTimeDash) / dashDuration));
                    transform.position += (Vector3)(Time.deltaTime * currentSpeed);

                    if (CollideWithEnemy(out GameObject[] enemies))
                    {
                        foreach (GameObject enemy in enemies)
                        {
                            OnTouchEnemy(enemy);
                        }
                    }

                    if (!alreadyCreateExplosionWinthThisDash && CollideWithGround(out Vector2 collisionPoint))
                    {
                        alreadyCreateExplosionWinthThisDash = true;
                        cloneAttack.originalCreateExplosionThisFrame = true;
                        cloneAttack.originalExplosionPosition = collisionPoint;
                        CreateExplosion(collisionPoint);
                        EndCurrentDash();
                    }
                }
            }
            else if (playerInput.attackWeakPressedDown || wantDash)
            {
                if (Time.time - lastTimeDash > minTimeBetweenDash)
                {
                    cloneAttack.originalDashThisFrame = true;
                    isDashing = true;
                    wantDash = false;
                    lastDir = charController.GetCurrentDirection(true);
                    alreadyCreateExplosionWinthThisDash = false;
                    lastTimeDash = Time.time;
                    charController.Freeze();
                }
                else if (Time.time - lastTimeDash <= dashBufferTime)
                {
                    wantDash = true;
                }
                else
                {
                    wantDash = false;
                }
            }
            else if (Time.time - lastTimeDash > maxTimeBetweenDash)
            {
                indexDash = 0;
                isAttackEnable = false;
                charController.UnFreeze();
            }
        }
    }

    private void EndCurrentDash()
    {
        isDashing = false;
        lastTimeDash = Time.time;
        indexDash++;
        charController.UnFreeze();
        charController.ForceApplyVelocity(currentSpeed);
        currentSpeed = Vector2.zero;

        if (indexDash >= nbDash)
        {
            indexDash = 0;
            isAttackEnable = false;
        }
    }

    #endregion

    #region Launch

    public override bool Launch(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
        if (!cooldown.isActive)
        {
            callbackEnableOtherAttack.Invoke();
            callbackEnableThisAttack.Invoke();
            return false;
        }
        base.Launch(callbackEnableOtherAttack, callbackEnableThisAttack);

        cooldown.Reset();
        StartCoroutine(ApplyAttackCorout(charController.GetCurrentDirection(true), callbackEnableOtherAttack, callbackEnableThisAttack));
        return true;
    }

    private IEnumerator ApplyAttackCorout(Vector2 dir, Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
        callbackEnableOtherAttack.Invoke();
        callbackEnableThisAttack.Invoke();

        charController.Freeze();

        float timeCounter = 0f;
        while (timeCounter < castDuration)
        {
            yield return null;
            if (!PauseManager.instance.isPauseEnable)
            {
                timeCounter += Time.deltaTime;
            }
        }

        lastDir = dir;
        isAttackEnable = onLaunchAttack = true;
    }

    private IEnumerator ApplyAttackCloneCorout()
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

        lastTimeDash = Time.time;
        isDashing = true;
    }

    #endregion

    #region Pause

    private void OnPauseEnable()
    {
        lastVelocityForPause = charController.velocity;
        charController.Freeze();
    }

    private void OnPauseDisable()
    {
        charController.UnFreeze();
        charController.ForceApplyVelocity(lastVelocityForPause);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        PauseManager.instance.callBackOnPauseDisable -= OnPauseDisable;
        PauseManager.instance.callBackOnPauseEnable -= OnPauseEnable;
    }

    #endregion

    #region Ennemies/Wall Collisions

    public override void OnTouchEnemy(GameObject enemy)
    {
        if(isAClone)
        {
            originalAmericanFistAttack.OnTouchEnemy(enemy);
        }
        else
        {
            base.OnTouchEnemy(enemy);
        }
    }

    private bool CollideWithEnemy(out GameObject[] enemies)
    {
        UnityEngine.Collider2D[] cols = PhysicsToric.OverlapBoxAll((Vector2)transform.position + colliderOffset, colliderSize, 0f, charMask);

        if (cols.Length <= 0)
        {
            enemies = null;
            return false;
        }

        List<GameObject> players = new List<GameObject>();
        foreach (UnityEngine.Collider2D col in cols)
        {
            if (col.CompareTag("Char"))
            {
                GameObject player = col.GetComponent<ToricObject>().original;
                if(playerCommon.id != player.GetComponent<PlayerCommon>().id)
                {
                    players.Add(player);
                }
            }
        }
        
        if(players.Count > 0)
        {
            enemies = players.ToArray();
            return true;
        }

        enemies = null;
        return false;
    }

    private bool CollideWithGround(out Vector2 collisionPoint)
    {
        Circle[] circles = new Circle[2];

        if (Mathf.Abs(lastDir.x) > 1e-6f)
        {
            Vector2 center = (Vector2)transform.position + colliderOffset + new Vector2(lastDir.x.Sign() * colliderSize.x * 0.5f, 0f);
            circles[0] = new Circle(center, groundDetectionRadius);
        }
        if (Mathf.Abs(lastDir.y) > 1e-6f)
        {
            Vector2 center = (Vector2)transform.position + colliderOffset + new Vector2(0f, lastDir.y.Sign() * colliderSize.y * 0.5f);
            circles[1] = new Circle(center, groundDetectionRadius);
        }

        for (int i = 0; i < circles.Length; i++)
        {
            Circle circle = circles[i];
            if(circle != null)
            {
                UnityEngine.Collider2D col = PhysicsToric.OverlapCircle(circle, groundMask);
                if(col != null)
                {
                    Collision2D.Collider2D customCol = Collision2D.Collider2D.FromUnityCollider2D(col);
                    if(Collision2D.Collider2D.Collide(circle, customCol, out collisionPoint))
                    {
                        return true;
                    }
#if UNITY_EDITOT || ADVANCE_DEBUG
                    else
                    {
                        Debug.LogWarning("Debug pls!");
                        LogManager.instance.WriteLog("Unity trigger a collision but Collion2D no.", customCol, col);
                    }
#endif
                }
            }
        }

        collisionPoint = Vector2.zero;
        return false;
    }

    private void CreateExplosion(in Vector2 collisionPoint, bool disableExplosionEffet = false)
    {
        Explosion explosion = Instantiate(explosionPrefabs, collisionPoint, Quaternion.identity, CloneParent.cloneParent);
        if(disableExplosionEffet)
        {
            explosion.enableBehaviour = false;
            explosion.SetColor(originalCloneAttack.GetComponentInChildren<SpriteRenderer>().color * originalCloneAttack.cloneTransparency);
        }
        else
        {
            explosion.callbackOnTouch += OnExplosionTouchEnemy;
        }
        explosion.Launch();
        ExplosionManager.instance.CreateExplosion(collisionPoint, explosionForce);
    }

    private void OnExplosionTouchEnemy(UnityEngine.Collider2D collider)
    {
        if(collider.CompareTag("Char"))
        {
            GameObject player = collider.GetComponent<ToricObject>().original;
            if(player.GetComponent<PlayerCommon>().id != playerCommon.id)
            {
                OnTouchEnemy(player);
            }
        }
    }

    #endregion

    #region OnValidate/Gizmos

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos)
            return;

        Hitbox.GizmosDraw((Vector2)transform.position + colliderOffset, colliderSize, Color.red);

        Vector2 center = (Vector2)transform.position + colliderOffset + new Vector2(colliderSize.x * 0.5f, 0f);
        Circle.GizmosDraw(center, groundDetectionRadius, Color.red);
        center = (Vector2)transform.position + colliderOffset + new Vector2(0f, colliderSize.y * 0.5f);
        Circle.GizmosDraw(center, groundDetectionRadius, Color.red);
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        nbDash = Mathf.Max(0, nbDash);
        colliderSize = new Vector2(Mathf.Max(0f, colliderSize.x), Mathf.Max(0f, colliderSize.y));

        if(!isAClone)
        {
            if(cloneAttack == null)
            {
                cloneAttack = GetComponent<CloneAttack>();
            }
            if(cloneAttack.clonePrefabs != null)
            {
                AmericanFistAttack afa = cloneAttack.clonePrefabs.GetComponent<AmericanFistAttack>();
                if(afa != null)
                {
                    afa.dashSpeed = dashSpeed;
                    afa.dashDuration = dashDuration;
                    afa.minTimeBetweenDash = minTimeBetweenDash;
                    afa.maxTimeBetweenDash = maxTimeBetweenDash;
                    afa.dashBufferTime = dashBufferTime;
                    afa.nbDash = nbDash;
                    afa.charMask = charMask;
                    afa.colliderSize = colliderSize;
                    afa.colliderOffset = colliderOffset;
                }
            }
        }
    }

#endif

#endregion
}
