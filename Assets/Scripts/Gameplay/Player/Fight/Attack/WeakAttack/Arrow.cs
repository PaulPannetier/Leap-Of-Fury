using UnityEngine;

public class Arrow : MonoBehaviour
{
    [HideInInspector] public Rigidbody2D rb;
    private Animator animator;
    private ArrowAttack arrowAttack;
    private PlayerCommon playerCommon;
    private ToricObject toricObject;
    private bool isFlying;//true si la flèche vole, false si elle est a terre.
    private bool isDestroy = false;
    private bool isMainArrow;

    [SerializeField] private CapsuleCollider2D capsuleCollider;
    [SerializeField] private LayerMask wallProjectileMask;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        toricObject = GetComponent<ToricObject>();
    }

    private void FixedUpdate()
    {
        if(isFlying && !toricObject.isAClone)
        {
            SetRotation();
        }
    }

    public void Launch(ArrowAttack physicAttack, in Vector2 dir, in float initSpeed, bool isMainArrow = true)
    {
        arrowAttack = physicAttack;
        isFlying = true;
        rb.velocity = dir * initSpeed;
        SetRotation();
        playerCommon = physicAttack.GetComponent<PlayerCommon>();
        capsuleCollider.enabled = false;
        this.isMainArrow = isMainArrow;
    }

    public void OnRelaunch()
    {

    }

    private void SetRotation()
    {
        rb.SetRotation(rb.velocity.Angle(Vector2.right) * Mathf.Rad2Deg);
    }

    private void HitPlayer(GameObject otherPlayer)
    {
        if(toricObject.isAClone)
        {
            toricObject.original.GetComponent<Arrow>().HitPlayer(otherPlayer);
            return;
        }

        arrowAttack.OnTouchEnemy(otherPlayer);
        arrowAttack.OnArrowTouchChar(otherPlayer);
        if(isMainArrow)
        {
            PickUp();
            //print("PickUp because hiting other player");
        }
        else
        {
            isDestroy = true;
            toricObject.RemoveClones();
            Destroy(gameObject);
        }
    }

    private void PickUp()
    {
        if(toricObject.isAClone)
        {
            toricObject.original.GetComponent<Arrow>().PickUp();
            return;
        }

        if(isDestroy)
        {
            //Debug.LogWarning("PTDR unity est destroy ca pue");
            return;
        }
        arrowAttack.RecoverArrow();
        isDestroy = true;
        toricObject.RemoveClones();
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
            toricObject.RemoveClones();
            Destroy(gameObject);
            return;
        }
        arrowAttack.OnArrowLand();

        //On verif que la fleche n'est pas coincé dans un wall projectile
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
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        isFlying = false;
        animator.SetTrigger("Land");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
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

    private void OnDrawGizmosSelected()
    {
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
    }
}
