using UnityEngine;
using System.Collections.Generic;

public class FloorShockWave : MonoBehaviour
{
    private float speedX, timeCreated;
    private bool right;
    private FallAttack fallAttack;
    private PlayerCommon playerCommon;
    private LayerMask playersMask, groundMask;
    private List<uint> charAlreadyTouch;

    [SerializeField] private Vector2 colliderOffset, colliderSize;
    [SerializeField] private float distanceFromFloor = 1f;
    [Tooltip("Horizontale detection")]
    [SerializeField] private float rayLengthHori = 1f;
    [SerializeField] private Vector2 offsetHoriRaycast = Vector2.right;
    [Tooltip("Vertical detection")]
    [SerializeField] private float rayLengthVerti = 1f;
    [SerializeField] private Vector2 offsetVertiRaycast = new Vector2(1f, 0.2f);
    [SerializeField] private float maxDuration = 5f;

    private void Awake()
    {
        playersMask = LayerMask.GetMask("Char");
        groundMask = LayerMask.GetMask("Floor", "WallProjectile");
        charAlreadyTouch = new List<uint>(4);
    }

    public void Launch(bool right, float maxSpeed, FallAttack fallAttack)
    {
        ToricRaycastHit2D raycast = PhysicsToric.Raycast(transform.position, Vector2.down, LevelMapData.currentMap.mapSize.y * LevelMapData.currentMap.cellSize.y, groundMask);
        if(raycast.collider == null)
        {
            print("Debug pls");
            LogManager.instance.AddLog("Raycast must collide with the ground!", new object[] { raycast });
            Destroy(gameObject);
            return;
        }

        transform.position = raycast.point + Vector2.up * distanceFromFloor;
        speedX = right ? maxSpeed : -maxSpeed;
        this.fallAttack = fallAttack;
        this.right = right;
        playerCommon = fallAttack.GetComponent<PlayerCommon>();
        timeCreated = Time.time;
        charAlreadyTouch.Clear();
    }

    private void Update()
    {
        if (PauseManager.instance.isPauseEnable)
        {
            timeCreated += Time.deltaTime;
            return;
        }

        if(Time.time - timeCreated > maxDuration)
        {
            Destroy(gameObject);
            return;
        }

        Collider2D[] cols = PhysicsToric.OverlapBoxAll((Vector2)transform.position + colliderOffset, colliderSize, 0f, playersMask);
        foreach (Collider2D col in cols)
        {
            if(col.CompareTag("Char"))
            {
                GameObject player = col.GetComponent<ToricObject>().original;
                uint playerId = player.GetComponent<PlayerCommon>().id;
                if (playerCommon.id != playerId && !charAlreadyTouch.Contains(playerId))
                {
                    fallAttack.OnTouchEnemyByShockWave(player, this);
                    charAlreadyTouch.Add(playerId);
                }
            }
        }

        Vector2 beg = (Vector2)transform.position + (right ? offsetVertiRaycast : new Vector2(-offsetVertiRaycast.x, offsetVertiRaycast.y));
        ToricRaycastHit2D raycast = PhysicsToric.Raycast(beg, Vector2.down, rayLengthVerti, groundMask);
        bool hitground = raycast.collider != null;

        beg = (Vector2)transform.position + (right ? offsetHoriRaycast : new Vector2(-offsetHoriRaycast.x, offsetHoriRaycast.y));
        raycast = PhysicsToric.Raycast(beg, right ? Vector2.right : Vector2.left, rayLengthHori, groundMask);
        bool hitWall = raycast.collider != null;

        if (hitWall || !hitground)
        {
            Destroy(gameObject);
            return;
        }

        transform.position += (Vector3)(Vector2.right * (speedX * Time.deltaTime));
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

    protected void OnValidate()
    {
        rayLengthHori = Mathf.Max(0f, rayLengthHori);
        rayLengthVerti = Mathf.Max(0f, rayLengthVerti);
        distanceFromFloor = Mathf.Max(0f, distanceFromFloor);
        colliderSize = new Vector2(Mathf.Max(0f, colliderSize.x), Mathf.Max(0f, colliderSize.y));
        maxDuration = Mathf.Max(0f, maxDuration);
    }

#endif

    #endregion
}