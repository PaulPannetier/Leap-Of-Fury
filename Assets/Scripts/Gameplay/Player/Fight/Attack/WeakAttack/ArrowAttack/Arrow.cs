using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Collision2D;

public class Arrow : MonoBehaviour
{
    [HideInInspector] public Rigidbody2D rb;
    private Animator animator;
    private ArrowAttack arrowAttack;
    private PlayerCommon playerCommon;
    private CapsuleCollider2D capsuleCollider;
    private ToricObject toricObject;
    private bool isFlying;//true si la fleche vole, false si elle est a terre.
    private bool isDestroy = false;
    private bool isMainArrow;
    private bool isGuiding = false;
    private float timeWhenLaunch = -10f;
    private LayerMask charMask, wallProjectileMask;

    public bool enableBehaviour = true;

    [SerializeField] private float charDetectionRange = 2f;
    [SerializeField, Range(0f, 180f)] private float charDetectionAngle = 2f;
    [SerializeField] private float rotationDetectionSpeed = 180f;
    [SerializeField] private float gravityScale = 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        toricObject = GetComponent<ToricObject>();
    }

    private void Start()
    {
        PauseManager.instance.callBackOnPauseEnable += OnPauseEnable;
        rb.gravityScale = gravityScale;
        charMask = LayerMask.GetMask("Char");
        wallProjectileMask = LayerMask.GetMask("WallProjectile");
    }

    #region FixedUpdate

    private void FixedUpdate()
    {
        if (!enableBehaviour)
            return;

        if(PauseManager.instance.isPauseEnable) 
        {
            timeWhenLaunch += Time.deltaTime;
            return;
        }

        if(isFlying && !toricObject.isAClone)
        {
            UnityEngine.Collider2D[] cols = PhysicsToric.OverlapCircleAll(transform.position, charDetectionRange, charMask);
            List<Vector2> playerAroundPos = new List<Vector2>();

            float currentAngle = transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
            foreach (UnityEngine.Collider2D col in cols)
            {
                if(col.CompareTag("Char"))
                {
                    float charAngle = Useful.AngleHori(transform.position, col.transform.position);
                    if(Useful.AngleDist(currentAngle, charAngle) < charDetectionAngle &&
                        col.GetComponent<ToricObject>().original.GetComponent<PlayerCommon>().id != playerCommon.id)
                    {
                        playerAroundPos.Add(col.transform.position);
                    }
                }
            }

            if(playerAroundPos.Count > 0)
            {
                isGuiding = true;
                Vector2 minPos = playerAroundPos[0];
                float minDist = PhysicsToric.Distance(playerAroundPos[0], transform.position);
                for (int i = 1; i < playerAroundPos.Count; i++)
                {
                    float dist = PhysicsToric.Distance(playerAroundPos[i], transform.position);
                    if(dist < minDist)
                    {
                        minDist = dist;
                        minPos = playerAroundPos[i];
                    }
                }

                float targetAngle = Useful.AngleHori(transform.position, minPos);
                float newAngle = Mathf.MoveTowardsAngle(currentAngle * Mathf.Rad2Deg, targetAngle * Mathf.Rad2Deg, rotationDetectionSpeed * Time.fixedDeltaTime);
                rb.SetRotation(newAngle);

                rb.linearVelocity = Useful.Vector2FromAngle(newAngle * Mathf.Deg2Rad, rb.linearVelocity.magnitude);
            }
            else
            {
                isGuiding = false;
            }

            if(!isGuiding)
            {
                SetRotation();
            }
        }
    }

    #endregion

    public void Launch(ArrowAttack physicAttack, in Vector2 dir, in float initSpeed, bool isMainArrow = true)
    {
        arrowAttack = physicAttack;
        isFlying = true;
        rb.linearVelocity = dir * initSpeed;
        SetRotation();
        playerCommon = physicAttack.GetComponent<PlayerCommon>();
        capsuleCollider.enabled = false;
        this.isMainArrow = isMainArrow;
        timeWhenLaunch = Time.time;
    }

    public void OnRelaunch()
    {

    }

    private void SetRotation()
    {
        rb.SetRotation(rb.linearVelocity.Angle(Vector2.right) * Mathf.Rad2Deg);
    }

    private void HitPlayer(GameObject otherPlayer)
    {
        if(toricObject.isAClone)
        {
            toricObject.original.GetComponent<Arrow>().HitPlayer(otherPlayer);
            return;
        }

        arrowAttack.OnArrowTouchChar(otherPlayer);
        if(isMainArrow)
        {
            PickUp();
            //print("PickUp because hiting other player");
        }
        else
        {
            isDestroy = true;
            Destroy(gameObject);
        }
    }

    private void PickUp(bool inAir = false)
    {
        if(toricObject.isAClone)
        {
            toricObject.original.GetComponent<Arrow>().PickUp(inAir);
            return;
        }

        if(isDestroy)
        {
            //Debug.LogWarning("PTDR unity est destroy ca pue");
            return;
        }

        if(inAir)
            arrowAttack.RecoverArrowInAir();
        else
            arrowAttack.RecoverArrow();

        isDestroy = true;
        Destroy(gameObject);
    }

    private void LandOf()
    {
        if(toricObject.isAClone)
        {
            toricObject.original.GetComponent<Arrow>().LandOf();
            return;
        }

        if(!isMainArrow)
        {
            isDestroy = true;
            Destroy(gameObject);
            return;
        }
        arrowAttack.OnArrowLand();

        //On verif que la fleche n'est pas coinc� dans un wall projectile
        capsuleCollider.enabled = true;
        float a = transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
        float l = capsuleCollider.size.magnitude * 0.4f;
        float teta = Mathf.Acos(Useful.ClampModulo(-1f, 1f, (capsuleCollider.size.x * 0.5f) / l));
        Vector2[] hotPosts = new Vector2[4]
        {
            (Vector2)transform.position + capsuleCollider.offset + new Vector2(l * Mathf.Cos(a + teta), l * Mathf.Sin(a + teta)),
            (Vector2)transform.position + capsuleCollider.offset + new Vector2(l * Mathf.Cos(a + Mathf.PI - teta), l * Mathf.Sin(a + Mathf.PI - teta)),
            (Vector2)transform.position + capsuleCollider.offset + new Vector2(l * Mathf.Cos(a + Mathf.PI + teta), l * Mathf.Sin(a + Mathf.PI + teta)),
            (Vector2)transform.position + capsuleCollider.offset + new Vector2(l * Mathf.Cos(a - teta), l * Mathf.Sin(a - teta))
        };

        int nbPointNotInWallProjectile = 0;
        foreach (Vector2 hotpoint in hotPosts)
        {
            if(Physics2D.OverlapPoint(hotpoint, wallProjectileMask) == null)
            {
                nbPointNotInWallProjectile++;
                if(nbPointNotInWallProjectile >= 2)
                    break;
            }
        }

        if(nbPointNotInWallProjectile < 2)
        {
            PickUp();
            //print("PickUp beacause stuck into wall projectile");
            return;
        }

        SetLandOfProperties();
        toricObject.ApplyToOther<Arrow>(nameof(SetLandOfProperties), 0f);
    }

    private void SetLandOfProperties()
    {
        capsuleCollider.enabled = true;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        isFlying = false;
        animator.SetTrigger("Land");
    }

    private void OnTriggerEnter2D(UnityEngine.Collider2D collision)
    {
        if(toricObject.isAClone)
        {
            toricObject.original.GetComponent<Arrow>().OnTriggerEnter2D(collision);
            return;
        }

        if (!enableBehaviour)
            return;

        if (isFlying)
        {
            if (collision.CompareTag("Char"))
            {
                GameObject player = collision.GetComponent<ToricObject>().original;
                if (player.GetComponent<PlayerCommon>().id != playerCommon.id)
                {
                    HitPlayer(player);
                    return;
                }
                else if(Time.time - timeWhenLaunch >= arrowAttack.delayBetweenLauchAndRecoverArrow)
                {
                    PickUp(true);
                    //print("PickUp in air");
                }
            }
            else if (collision.CompareTag("Floor"))
            {
                LandOf();
            }
        }
        else
        {
            if(collision.CompareTag("Char"))
            {
                GameObject player = collision.GetComponent<ToricObject>().original;
                if(player.GetComponent<PlayerCommon>().id == playerCommon.id)
                {
                    PickUp();
                    //print("PickUp player walk throw it");
                }
            }
        }
    }

    #region Gizmos/OnValidate/Pause
    
    private void OnPauseEnable()
    {
        StartCoroutine(PauseCorout());
;    }

    private IEnumerator PauseCorout()
    {
        Vector2 rbVel = rb.linearVelocity;
        float angularSpeed = rb.angularVelocity;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        animator.speed = 0f;

        while(PauseManager.instance.isPauseEnable)
        {
            yield return null;
        }

        animator.speed = 1f;
        rb.constraints = RigidbodyConstraints2D.None;
        rb.linearVelocity = rbVel;
        rb.angularVelocity = angularSpeed;
    }

    private void OnDestroy()
    {
        PauseManager.instance.callBackOnPauseEnable -= OnPauseEnable;
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        charDetectionRange = Mathf.Max(charDetectionRange, 0f);
        rotationDetectionSpeed = Mathf.Max(rotationDetectionSpeed, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        if(capsuleCollider == null)
            capsuleCollider = GetComponent<CapsuleCollider2D>();

        float a = transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
        float l = capsuleCollider.size.magnitude * 0.5f;
        float teta = Mathf.Acos(Useful.ClampModulo(-1f, 1f, (capsuleCollider.size.x * 0.5f) / l));
        Vector2[] hotPosts = new Vector2[4]
        {
            (Vector2)transform.position + capsuleCollider.offset + new Vector2(l * Mathf.Cos(a + teta), l * Mathf.Sin(a + teta)),
            (Vector2)transform.position + capsuleCollider.offset + new Vector2(l * Mathf.Cos(a + Mathf.PI - teta), l * Mathf.Sin(a + Mathf.PI - teta)),
            (Vector2)transform.position + capsuleCollider.offset + new Vector2(l * Mathf.Cos(a + Mathf.PI + teta), l * Mathf.Sin(a + Mathf.PI + teta)),
            (Vector2)transform.position + capsuleCollider.offset + new Vector2(l * Mathf.Cos(a - teta), l * Mathf.Sin(a - teta))
        };

        Gizmos.color = Color.green;
        foreach (Vector2 hotpoint in hotPosts)
        {
            Circle.GizmosDraw(hotpoint, 0.05f);
        }

        Circle.GizmosDraw(transform.position, charDetectionRange, -charDetectionAngle * Mathf.Deg2Rad, charDetectionAngle * Mathf.Deg2Rad);
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + Useful.Vector2FromAngle(charDetectionAngle * Mathf.Deg2Rad, charDetectionRange));
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + Useful.Vector2FromAngle(-charDetectionAngle * Mathf.Deg2Rad, charDetectionRange));
    }

#endif

#endregion
}
