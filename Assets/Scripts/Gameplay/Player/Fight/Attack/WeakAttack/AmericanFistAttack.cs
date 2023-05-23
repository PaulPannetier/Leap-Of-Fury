using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmericanFistAttack : WeakAttack
{
    private Movement movement;
    private CustomPlayerInput playerInput;
    private Rigidbody2D rb;
    private CloneAttack cloneAttack;//null if isAClone == true
    [HideInInspector] public CloneAttack originalCloneAttack;
    private AmericanFistAttack originalAmericanFistAttack;
    private bool isAttackEnable = false, onLaunchAttack = false, isDashing = false, wantDash = false;
    private int indexDash = 0;
    private Vector2 lastDir, initSpeed;
    private float lastTimeDash = -10;
    private LayerMask groundMask, enemiesMask;
    private bool alreadyCreateExplosionWinthThisDash;

    [SerializeField] private bool drawGizmos = true;
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
    [HideInInspector] public bool activateWallExplosion;//true one frame when the original char dash
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

    protected override void Awake()
    {
        if (isAClone)
        {
            return;
        }
        base.Awake();
        playerInput = GetComponent<CustomPlayerInput>();
        movement = GetComponent<Movement>();
        rb = GetComponent<Rigidbody2D>();
        cloneAttack = GetComponent<CloneAttack>();
        originalCloneAttack = cloneAttack;
    }

    protected override void Start()
    {
        if (isAClone)
            return;

        base.Start();
        groundMask = LayerMask.GetMask("Floor", "WallProjectile");
        enemiesMask = LayerMask.GetMask("Char");
    }

    #region Update

    protected override void Update()
    {
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
                initSpeed = rb.velocity;
                movement.enableBehaviour = false;
            }

            if (isDashing)
            {
                if (Time.time - lastTimeDash > dashDuration)
                {
                    isDashing = false;
                    lastTimeDash = Time.time;
                    indexDash++;
                    movement.enableBehaviour = movement.enableInput = true;
                    rb.velocity = initSpeed;

                    if (indexDash >= nbDash)
                    {
                        indexDash = 0;
                        isAttackEnable = false;
                    }
                }

                rb.velocity = lastDir * (dashSpeedCurve.Evaluate((Time.time - lastTimeDash) / dashDuration) * dashSpeed);

                if (CollideWithEnemy(out GameObject[] enemies))
                {
                    foreach (GameObject enemy in enemies)
                    {
                        OnTouchEnemy(enemy);
                    }
                }

                if(!alreadyCreateExplosionWinthThisDash && CollideWithGround(out Vector2 collisionPoint))
                {
                    alreadyCreateExplosionWinthThisDash = true;
                    cloneAttack.originalCreateExplosionThisFrame = true;
                    cloneAttack.originalExplosionPosition = collisionPoint;
                    CreateExplosion(collisionPoint);
                }
            }
            else if (playerInput.attackWeakPressedDown || wantDash)
            {
                if (Time.time - lastTimeDash > minTimeBetweenDash)
                {
                    cloneAttack.originalDashThisFrame = true;
                    isDashing = true;
                    wantDash = false;
                    lastDir = movement.GetCurrentDirection(true);
                    alreadyCreateExplosionWinthThisDash = false;
                    lastTimeDash = Time.time;
                    initSpeed = rb.velocity;
                    movement.enableBehaviour = false;
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
                movement.enableBehaviour = movement.enableInput = true;
            }
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
        StartCoroutine(ApplyAttackCorout(movement.GetCurrentDirection(true), callbackEnableOtherAttack, callbackEnableThisAttack));
        return true;
    }

    private IEnumerator ApplyAttackCorout(Vector2 dir, Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
        callbackEnableOtherAttack.Invoke();
        callbackEnableThisAttack.Invoke();
        initSpeed = rb.velocity;
        movement.Freeze();
        yield return Useful.GetWaitForSeconds(castDuration);
        movement.UnFreeze();
        lastDir = dir;
        isAttackEnable = onLaunchAttack = true;
    }

    private IEnumerator ApplyAttackCloneCorout()
    {
        yield return Useful.GetWaitForSeconds(castDuration);
        lastTimeDash = Time.time;
        isDashing = true;
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
        Collider2D[] cols = PhysicsToric.OverlapBoxAll((Vector2)transform.position + colliderOffset, colliderSize, 0f, enemiesMask);

        if (cols.Length <= 0)
        {
            enemies = null;
            return false;
        }

        List<GameObject> players = new List<GameObject>();
        foreach (Collider2D col in cols)
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
                Collider2D col = PhysicsToric.OverlapCircle(circle, groundMask);
                if(col != null)
                {
                    CustomCollider2D customCol = CustomCollider2D.FromUnityCollider2D(col);
                    if(CustomCollider2D.Collide(circle, customCol, out collisionPoint))
                    {
                        return true;
                    }
                    else
                    {
                        Debug.LogWarning("Debug pls!");
                    }
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

    private void OnExplosionTouchEnemy(Collider2D collider)
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

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos)
            return;
        Gizmos.color = Color.green;
        Hitbox.GizmosDraw((Vector2)transform.position + colliderOffset, colliderSize);

        Vector2 center = (Vector2)transform.position + colliderOffset + new Vector2(colliderSize.x * 0.5f, 0f);
        Circle.GizmosDraw(center, groundDetectionRadius);
        center = (Vector2)transform.position + colliderOffset + new Vector2(0f, colliderSize.y * 0.5f);
        Circle.GizmosDraw(center, groundDetectionRadius);
    }

    private void OnValidate()
    {
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
                    afa.enemiesMask = enemiesMask;
                    afa.colliderSize = colliderSize;
                    afa.colliderOffset = colliderOffset;
                }
            }
        }
    }

    #endregion
}
