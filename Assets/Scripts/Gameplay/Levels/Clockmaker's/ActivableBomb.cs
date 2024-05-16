using UnityEngine;

public class ActivableBomb : ActivableObject
{
    private LayerMask charMask;
    private SpriteRenderer spriteRenderer;

    [Space(10)]
    [SerializeField] private float explosionRadius;
    [SerializeField] private Vector2 explosionOffset;
    [SerializeField] private Gradient colorGradient;
    [SerializeField] private GameObject explosionParticlesPrefabs;

    protected override void Start()
    {
        base.Start();
        charMask = LayerMask.GetMask("Char");
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    protected override void OnActivated()
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

        if(explosionParticlesPrefabs != null)
        {
            GameObject explosion = Instantiate(explosionParticlesPrefabs, (Vector2)transform.position + explosionOffset, Quaternion.identity, CloneParent.cloneParent);
            Destroy(explosion, 5f);
        }

        Destroy(gameObject);
    }

    protected override void OnDesactivated()
    {
        
    }

    private void Update()
    {
        spriteRenderer.color = colorGradient.Evaluate(activationPercentageSmooth);
    }

    #region OnValidate/Gizmos

#if UNITY_EDITOR

    protected override void OnValidate()
    {
        base.OnValidate();
        explosionRadius = Mathf.Max(explosionRadius, 0f);
        base.startActivated = false;
    }

    private void OnDrawGizmosSelected()
    {
        Collision2D.Circle.GizmosDraw((Vector2)transform.position + explosionOffset, explosionRadius, Color.green, true);
    }

#endif

    #endregion
}
