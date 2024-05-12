using UnityEngine;

public class ClosableDoor : ActivableObject
{
    private LayerMask charMask;
    private BoxCollider2D doorHitbox;

    [SerializeField] private GameObject doorGo;

    private void Awake()
    {
        doorHitbox = GetComponentInChildren<BoxCollider2D>();
    }

    protected override void Start()
    {
        base.Start();
        charMask = LayerMask.GetMask("Char");
    }

    protected override void OnActivated()
    {
        doorGo.SetActive(false);
    }

    protected override void OnDesactivated()
    {
        doorGo.SetActive(true);

        Collider2D[] cols = PhysicsToric.OverlapBoxAll((Vector2)transform.position + doorHitbox.offset, doorHitbox.size, 0f, charMask);
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
