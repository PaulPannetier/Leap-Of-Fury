using UnityEngine;

public class BouncingBall : MonoBehaviour
{
    private Vector2 speed;
    private int maxBounce, nbBounce;
    private float maxDuration, timeLaunch;
    private BouncingBallAttack bouncingBallAttack;
    private PlayerCommon playerCommon;
    private ToricObject toricObj;

    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private Vector2 colliderOffset;
    [SerializeField] private float colliderRadius;
    [SerializeField] private float rayCastLength = 1f;

    private void Awake()
    {
        toricObj = GetComponent<ToricObject>();
    }

    public void Launch(in Vector2 dir, in float speed, in int maxBounce, in float maxDuration, BouncingBallAttack bouncingBallAttack)
    {
        this.speed = dir * speed;
        this.maxBounce = maxBounce;
        this.maxDuration = maxDuration;
        timeLaunch = Time.time;
        nbBounce = 0;
        this.bouncingBallAttack = bouncingBallAttack;
        playerCommon = bouncingBallAttack.GetComponent<PlayerCommon>();
    }

    private void Update()
    {
        if(!toricObj.isAClone && Time.time - timeLaunch > maxDuration)
        {
            Destroy();
            return;
        }

        if(!toricObj.isAClone)
        {
            Collider2D col = Physics2D.OverlapCircle((Vector2)transform.position + colliderOffset, colliderRadius, groundMask);
            if (col != null)
            {
                nbBounce++;
                if (nbBounce >= maxBounce)
                {
                    Destroy();
                    return;
                }

                RaycastHit2D raycast1, raycast2;
                Vector2 beg = (Vector2)transform.position - speed * (Time.deltaTime * 2f);
                float ang = Useful.AngleHori(beg, beg + speed);
                const float angleStep = Mathf.Deg2Rad * 11.25f;
                int i = -1;
                do
                {
                    i++;
                    raycast1 = Physics2D.Raycast(beg, new Vector2(Mathf.Cos(ang + i * angleStep), Mathf.Sin(ang + i * angleStep)), rayCastLength, groundMask);
                    raycast2 = Physics2D.Raycast(beg, new Vector2(Mathf.Cos(ang - i * angleStep), Mathf.Sin(ang - i * angleStep)), rayCastLength, groundMask);

                } while ((raycast1.collider != col && raycast2.collider != col) && i < 32);

                RaycastHit2D raycast = raycast1.collider == col ? raycast1 : raycast2;

                transform.position += (Vector3)(speed * (-Time.deltaTime * 1.3f));

                float v = speed.magnitude;
                Vector2 M = Droite.Symetric(raycast.point - speed, new Droite(raycast.point, raycast.point + raycast.normal));
                speed = (M - raycast.point).normalized * v;
            }
        }

        Collider2D[] cols = Physics2D.OverlapCircleAll((Vector2)transform.position + colliderOffset, colliderRadius, playerMask);
        foreach (Collider2D col in cols)
        {
            if(col.CompareTag("Char"))
            {
                GameObject player = col.GetComponent<ToricObject>().original;
                if (player.GetComponent<PlayerCommon>().id != playerCommon.id)
                {
                    toricObj.original.GetComponent<BouncingBall>().bouncingBallAttack.OnTouchEnemy(player);
                    Destroy();
                }
            }
        }

        if (!toricObj.isAClone)
        {
            transform.position += (Vector3)(speed * Time.deltaTime);
        }
    }

    private void Destroy()
    {
        toricObj.RemoveClones();
        Destroy(gameObject);
    }

    private void OnValidate()
    {
        colliderRadius = Mathf.Max(0f, colliderRadius);
        rayCastLength = Mathf.Max(0f, rayCastLength, 1.1f * colliderRadius);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Circle.GizmosDraw((Vector2)transform.position + colliderOffset, colliderRadius);
        Gizmos.DrawLine((Vector2)transform.position, (Vector2)transform.position + Vector2.right * rayCastLength);
    }
}
