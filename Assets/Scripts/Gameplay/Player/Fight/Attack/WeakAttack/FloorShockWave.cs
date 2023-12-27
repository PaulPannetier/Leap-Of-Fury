using UnityEngine;

public class FloorShockWave : MonoBehaviour
{
    private float speedX;
    private bool right;
    private bool isFinish = false;
    private FallAttack fallAttack;
    private PlayerCommon playerCommon;
    private ToricObject toricObject;

    [SerializeField] private Vector2 colliderOffset, colliderSize;
    [SerializeField] private LayerMask playersMask;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float distanceFromFloor = 1f;
    [Tooltip("Horizontale detection")]
    [SerializeField] private float rayLengthHori = 1f;
    [SerializeField] private Vector2 offsetHoriRaycast = Vector2.right;
    [Tooltip("Vertical detection")]
    [SerializeField] private float rayLengthVerti = 1f;
    [SerializeField] private Vector2 offsetVertiRaycast = new Vector2(1f, 0.2f);

    [Tooltip("Stat")]
    [Range(0f, 100f)] public float attackForce = 30f;

    private void Awake()
    {
        toricObject = GetComponent<ToricObject>();
    }

    public void Launch(in Vector2 impactPoint, in bool right, in float maxSpeed, FallAttack fallAttack)
    {
        RaycastHit2D raycast = Physics2D.Raycast(impactPoint, Vector2.down, LevelMapData.currentMap.mapSize.y, groundMask);
        if(raycast.collider == null)
        {
            print("Debug shock wave raycasting! plsss");
            return;
        }
        transform.position = raycast.point + Vector2.up * distanceFromFloor;
        speedX = right ? maxSpeed : -maxSpeed;
        this.fallAttack = fallAttack;
        this.right = right;
        isFinish = false;
        playerCommon = fallAttack.GetComponent<PlayerCommon>();
    }

    private void OnHitChar(GameObject player)
    {
        if(toricObject.isAClone)
        {
            toricObject.original.GetComponent<FloorShockWave>().OnHitChar(player);
            return;
        }

        if (playerCommon.id != player.GetComponent<PlayerCommon>().id)
        {
            fallAttack.OnTouchEnemyByShockWave(player, this);
        }
    }

    private void Update()
    {
        if (PauseManager.instance.isPauseEnable)
            return;

        Collider2D[] cols = PhysicsToric.OverlapBoxAll((Vector2)transform.position + colliderOffset, colliderSize, 0f, playersMask);
        foreach (Collider2D col in cols)
        {
            if(col.CompareTag("Char"))
            {
                OnHitChar(col.GetComponent<ToricObject>().original);
            }
        }

        if (toricObject.isAClone)
            return;

        Vector2 beg = (Vector2)transform.position + (right ? offsetVertiRaycast : new Vector2(-offsetVertiRaycast.x, offsetVertiRaycast.y));
        RaycastHit2D raycast = PhysicsToric.Raycast(beg, Vector2.down, rayLengthVerti, groundMask);//en bas
        if(raycast.collider == null)
        {
            //on arréte la shockWave ici
            isFinish = true;
        }
        beg = (Vector2)transform.position + (right ? offsetHoriRaycast : new Vector2(-offsetHoriRaycast.x, offsetHoriRaycast.y));
        raycast = PhysicsToric.Raycast(beg, right ? Vector2.right : Vector2.left, rayLengthHori, groundMask);
        if(raycast.collider != null)
        {
            isFinish = true;
        }

        if (isFinish)
        {
            toricObject.RemoveClones();
            Destroy(gameObject);
        }
        else
        {
            transform.position += (Vector3)(Vector2.right * (speedX * Time.deltaTime));
        }
    }

    #region Gizmos/OnValidate

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube((Vector2)transform.position + colliderOffset, colliderSize);

        //en bas
        Vector2 beg = (Vector2)transform.position + offsetVertiRaycast;
        Gizmos.DrawLine(beg, beg + Vector2.down * rayLengthVerti);
        beg = (Vector2)transform.position + new Vector2(-offsetVertiRaycast.x, offsetVertiRaycast.y);
        Gizmos.DrawLine(beg, beg + Vector2.down * rayLengthVerti);
        //sur les coté
        beg = (Vector2)transform.position + offsetHoriRaycast;
        Gizmos.DrawLine(beg, beg + Vector2.right * rayLengthHori);
        beg = (Vector2)transform.position + new Vector2(-offsetHoriRaycast.x, offsetHoriRaycast.y);
        Gizmos.DrawLine(beg, beg + Vector2.left * rayLengthHori);

        Gizmos.color = Color.red;
        Gizmos.DrawLine((Vector2)transform.position, (Vector2)transform.position + Vector2.down * distanceFromFloor);
    }

    private void OnValidate()
    {
        rayLengthHori = Mathf.Max(0f, rayLengthHori);
        rayLengthVerti = Mathf.Max(0f, rayLengthVerti);
        distanceFromFloor = Mathf.Max(0f, distanceFromFloor);
        colliderSize = new Vector2(Mathf.Max(0f, colliderSize.x), Mathf.Max(0f, colliderSize.y));
    }

#endif

    #endregion
}
