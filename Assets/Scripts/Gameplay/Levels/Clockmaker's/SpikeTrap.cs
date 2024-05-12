using UnityEngine;

public class SpikeTrap : ActivableObject
{
    private new Transform transform;
    private BoxCollider2D hitbox;
    private LayerMask charMask;

    [Space(10)]
    [SerializeField] private GameObject spikeGO;

    private void Awake()
    {
        this.transform = base.transform;
        hitbox = GetComponentInChildren<BoxCollider2D>();
    }

    protected override void Start()
    {
        base.Start();
        charMask = LayerMask.GetMask("Char");
    }

    private void Update()
    {
        if(isActivated)
        {
            Collider2D[] cols = PhysicsToric.OverlapBoxAll((Vector2)transform.position + hitbox.offset, hitbox.size, 0f, charMask);
            Collider2D currentCol;
            for (int i = 0; i < cols.Length; i++)
            {
                currentCol = cols[i];
                if (currentCol.CompareTag("Char"))
                {
                    GameObject player = currentCol.GetComponent<ToricObject>().original;
                    EventController ec = player.GetComponent<EventController>();
                    ec.OnBeenTouchByEnvironnement(gameObject);
                }
            }
        }
    }

    protected override void OnActivated()
    {
        spikeGO.SetActive(true);
    }

    protected override void OnDesactivated()
    {
        spikeGO.SetActive(false);
    }

    #region OnValidate

#if UNITY_EDITOR

    protected override void OnValidate()
    {
        base.OnValidate();
        this.transform = base.transform;
    }

#endif

    #endregion
}
