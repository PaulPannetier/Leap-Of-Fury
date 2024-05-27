using UnityEngine;

public class SpikeTrap : PendulumActivable
{
    private new Transform transform;
    private BoxCollider2D hitbox;
    private LayerMask charMask;
    private Animator[] animators;
    private int activateAnim, desactivateAnim;

    private void Awake()
    {
        this.transform = base.transform;
        hitbox = GetComponentInChildren<BoxCollider2D>();
        animators = GetComponentsInChildren<Animator>();
        activateAnim = Animator.StringToHash("spikeUp");
        desactivateAnim = Animator.StringToHash("spikeDown");
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
                    ec.OnBeenTouchByEnvironnement(gameObject, FightController.DamageType.AlwaysKill);
                }
            }
        }
    }

    protected override void OnActivated()
    {
        hitbox.enabled = true;

        if(!isActivated)
        {
            foreach (Animator animator in animators)
            {
                animator.CrossFade(activateAnim, 0, 0);
            }
        }
    }

    protected override void OnDesactivated()
    {
        hitbox.enabled = false;
        if (isActivated)
        {
            foreach (Animator animator in animators)
            {
                animator.CrossFade(desactivateAnim, 0, 0);
            }
        }
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
