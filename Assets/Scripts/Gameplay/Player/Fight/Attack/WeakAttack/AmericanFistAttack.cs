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
    private AmericanFistAttack originalAmericanFistAttack;
    private bool isAttackEnable = false, onLaunchAttack = false, isDashing = false, wantDash = false;
    private int indexDash = 0;
    private Vector2 lastDir, initSpeed;
    private float lastTimeDash = -10;

    [SerializeField] private bool isAClone;
    [SerializeField] private float dashSpeed = 10f, dashDuration = 0.4f, minTimeBetweenDash = 0.2f, maxTimeBetweenDash = 0.7f, dashBufferTime = 0.1f;
    [SerializeField] private AnimationCurve dashSpeedCurve;
    [SerializeField] private int nbDash = 3;
    [SerializeField] private Vector2 colliderOffset;
    [SerializeField] private Vector2 colliderSize;
    [SerializeField] private LayerMask enemiesMask;

    [HideInInspector] public bool activateCloneDash;//true one frame when the original char dash
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
    }

    protected override void Start()
    {
        if (!isAClone)
            base.Start();
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
            if (CollideWithEnemy(out GameObject[] enemies))
            {
                foreach (GameObject enemy in enemies)
                {
                    OnTouchEnemy(enemy);
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
                indexDash = 0;
                initSpeed = rb.velocity;
                movement.Freeze();
            }

            if (isDashing)
            {
                if (Time.time - lastTimeDash > dashDuration)
                {
                    isDashing = false;
                    lastTimeDash = Time.time;
                    indexDash++;
                    movement.UnFreeze();
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
            }
            else if (playerInput.attackWeakPressedDown || wantDash)
            {
                if (Time.time - lastTimeDash > minTimeBetweenDash)
                {
                    cloneAttack.originalDashThisFrame = true;
                    isDashing = true;
                    wantDash = false;
                    lastDir = movement.GetCurrentDirection();
                    lastTimeDash = Time.time;
                    initSpeed = rb.velocity;
                    movement.Freeze();
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
        StartCoroutine(ApplyAttackCorout(movement.GetCurrentDirection(), callbackEnableOtherAttack, callbackEnableThisAttack));
        return true;
    }

    private IEnumerator ApplyAttackCorout(Vector2 dir, Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
        callbackEnableOtherAttack.Invoke();
        callbackEnableThisAttack.Invoke();
        initSpeed = rb.velocity;
        movement.Freeze();
        yield return Useful.GetWaitForSeconds(castDuration);
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

    #region Ennemies Collisions

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

    #endregion

    #region OnValidate/Gizmos

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube((Vector2)transform.position + colliderOffset, colliderSize);
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
