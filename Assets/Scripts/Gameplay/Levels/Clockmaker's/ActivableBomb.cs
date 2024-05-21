using UnityEngine;

public class ActivableBomb : PendulumActivable
{
    private LayerMask charMask, charAndprojectileMask;
    private new Transform transform;
    private SpriteRenderer spriteRenderer;

    [Space(10)]
    [SerializeField] private float explosionRadius;
    [SerializeField] private Vector2 explosionOffset;
    [SerializeField] private float triggerRadius;
    [SerializeField] private Vector2 triggerOffset;
    [SerializeField] private Gradient colorGradient;
    [SerializeField] private GameObject explosionParticlesPrefabs;
    [SerializeField] private float explosionForce = 100f;

    protected override void Start()
    {
        startActivated = false;
        base.Start();
        this.transform = base.transform;
        charMask = LayerMask.GetMask("Char");
        charAndprojectileMask = LayerMask.GetMask("Char", "Projectile");
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        spriteRenderer.color = colorGradient.Evaluate(activationPercentageSmooth);

        Collider2D col = PhysicsToric.OverlapCircle((Vector2)transform.position + triggerOffset, triggerRadius, charAndprojectileMask);
        if(col != null)
        {
            TriggerExplosion();
        }
    }

    private void TriggerExplosion()
    {
        Collider2D[] cols = PhysicsToric.OverlapCircleAll((Vector2)transform.position + explosionOffset, explosionRadius, charMask);
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

        if (explosionParticlesPrefabs != null)
        {
            GameObject explosion = Instantiate(explosionParticlesPrefabs, (Vector2)transform.position + explosionOffset, Quaternion.identity, CloneParent.cloneParent);
            Destroy(explosion, 5f);
        }

        ExplosionManager.instance.CreateExplosion((Vector2)transform.position + explosionOffset, explosionForce);
        Destroy(gameObject);
    }

    protected override void OnActivated()
    {
        TriggerExplosion();
    }

    protected override void OnDesactivated()
    {
        
    }

    #region OnValidate/Gizmos

#if UNITY_EDITOR

    protected override void OnValidate()
    {
        base.OnValidate();
        this.transform = base.transform;
        explosionRadius = Mathf.Max(explosionRadius, 0f);
        triggerRadius = Mathf.Max(triggerRadius, 0f);
        explosionForce = Mathf.Max(explosionForce, 0f);
        startActivated = false;
    }

    private void OnDrawGizmosSelected()
    {
        Collision2D.Circle.GizmosDraw((Vector2)transform.position + explosionOffset, explosionRadius, Color.green, true);
        Collision2D.Circle.GizmosDraw((Vector2)transform.position + triggerOffset, triggerRadius, Color.red, true);
    }

#endif

    #endregion
}
