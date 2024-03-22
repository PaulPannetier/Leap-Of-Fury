using UnityEngine;

public class FirePassif : PassifAttack
{
    private BoxCollider2D hitbox;
    private Movement movement;
    [HideInInspector] public float fireDecreasement;

    public bool drawFireGizmos = true;
    public Vector2 fireSize;
    [Tooltip("L'écart de strenght pour déclencher un dédoublement")][Range(0f, 100f)] public float firePropagationGap;
    [Range(0f, 1f)] public float firePropagationProbability;
    [Range(0f, 100f)] public float minStrenghtForPropagation;
    [Range(0f, 100f)] public float fireDecreasementAtPropagation;
    public float fireDuration;
    public LayerMask charMask;
    [SerializeField] private GameObject firePrefab;

    [Header("Raycast")]
    public LayerMask groundMask;

    protected override void Awake()
    {
        base.Awake();
        movement = GetComponent<Movement>();
        hitbox = GetComponent<BoxCollider2D>();
    }

    protected override void Start()
    {
        base.Start();
        fireDecreasement = 100f / fireDuration;
    }

    protected override void Update()
    {
        if (!enableBehaviour)
            return;
        base.Update();
        if(movement.isGrounded)
        {
            RaycastHit2D raycast1 = PhysicsToric.Raycast((Vector2)transform.position + Vector2.right * (hitbox.size * 0.45f), Vector2.down, 1f, groundMask);
            RaycastHit2D raycast2 = PhysicsToric.Raycast((Vector2)transform.position + Vector2.left  * (hitbox.size * 0.45f), Vector2.down, 1f, groundMask);
            Vector2 pos;
            bool createFire = true;
            if(raycast1.collider == null || raycast2.collider == null)
            {
                if(raycast1.collider == null && raycast2.collider == null)
                {
                    createFire = false;
                    pos = (Vector2)transform.position;
                }
                else
                {
                    pos = raycast1.collider != null ? raycast1.point : (raycast2.collider != null ? raycast2.point : (Vector2)transform.position);
                }
            }
            else
            {
                float d1 = raycast1.point.SqrDistance(transform.position);
                float d2 = raycast2.point.SqrDistance(transform.position);
                if(Mathf.Abs(d1 - d2) * Mathf.Abs(d1 - d2) <= (0.05f * (d1 + d2)) * (0.05f * (d1 + d2)))
                {
                    pos = (raycast1.point + raycast2.point) * 0.5f;
                }
                else
                {
                    pos = d1 <= d2 ? raycast1.point : raycast2.point;
                }
            }
            
            if(createFire)
            {
                pos += Vector2.up * (fireSize.y * 0.5f);
                Fire fire = null;
                if (!Fire.IsAFire(pos, out fire))
                {
                    if (!Fire.IsAFire(pos + Vector2.left * fireSize.x * 0.5f, out fire))
                    {
                        if (!Fire.IsAFire(pos + Vector2.right * fireSize.x * 0.5f, out fire))
                        {
                            CreateFireNextToPos(100f, pos);
                        }
                    }
                }
                if (fire != null)
                {
                    fire.toricObject.original.GetComponent<Fire>().OnWalk();
                }
            }
        }
    }

    /// <summary>
    /// On vérifie uniquement que le feu soit au dessus d'un sol, on ne vérif pas qu'il n'y a pas de feu a cette emplacement
    /// </summary>
    /// <param name="strenght"></param>
    /// <param name="position"></param>
    public void CreateFireNextToPos(in float strenght, in Vector2 position)
    {
        const float step = 0.1f;
        const int maxIter = 40;
        Vector2 newPos = position;
        bool r, l;
        int i = 0;
        while(i < maxIter && !Verif(out r, out l))
        {
            if(!r)
            {
                newPos = new Vector2(newPos.x - step, newPos.y);
            }
            if (!l)
            {
                newPos = new Vector2(newPos.x + step, newPos.y);
            }
            i++;
        }

        CreateFire(strenght, newPos);

        bool Verif(out bool right, out bool left)
        {
            right = PhysicsToric.Raycast(newPos + Vector2.right * (fireSize.x * 0.45f), Vector2.down, fireSize.y, groundMask).collider != null;
            left = PhysicsToric.Raycast(newPos + Vector2.left * (fireSize.x * 0.45f), Vector2.down, fireSize.y, groundMask).collider != null;
            return right && left;
        }
    }

    public void CreateFire(in float strenght, in Vector2 position)
    {
        GameObject fireGO = Instantiate(firePrefab, position, Quaternion.identity, CloneParent.cloneParent);
        fireGO.GetComponent<Fire>().OnCreate(this);
    }

#if UNITY_EDITOR

    protected override void OnValidate()
    {
        base.OnValidate();
        fireDuration = Mathf.Max(0f, fireDuration);
        fireDecreasement = Mathf.Max(0f, fireDecreasement);
    }

#endif
}
