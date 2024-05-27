using UnityEngine;

public class ClosableDoor : PendulumActivable
{
    private LayerMask charMask;
    private BoxCollider2D doorHitbox;
    private Animator animator;
    private int closeAnim, openAnim;

    private void Awake()
    {
        doorHitbox = GetComponent<BoxCollider2D>();
        animator = GetComponentInChildren<Animator>();
        openAnim = Animator.StringToHash("open");
        closeAnim = Animator.StringToHash("close");
    }

    protected override void Start()
    {
        base.Start();
        charMask = LayerMask.GetMask("Char");
    }

    protected override void OnActivated()
    {
        doorHitbox.enabled = false;
        if(!isActivated)
        {
            animator.CrossFade(openAnim, 0, 0);
        }
    }

    protected override void OnDesactivated()
    {
        doorHitbox.enabled = true;

        if (isActivated)
        {
            animator.CrossFade(closeAnim, 0, 0);

            Collider2D[] cols = PhysicsToric.OverlapBoxAll((Vector2)transform.position + doorHitbox.offset, doorHitbox.size, 0f, charMask);
            Collider2D currentCol;
            for (int i = 0; i < cols.Length; i++)
            {
                currentCol = cols[i];
                if (currentCol.CompareTag("Char"))
                {
                    GameObject player = currentCol.GetComponent<ToricObject>().original;
                    EventController ec = player.GetComponent<EventController>();
                    ec.OnBeenTouchByEnvironnement(gameObject, FightController.DamageType.AlwaysKill);
                }
            }
        }
    }
}
