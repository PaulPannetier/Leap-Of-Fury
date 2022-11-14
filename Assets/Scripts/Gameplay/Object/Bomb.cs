using UnityEngine;

public class Bomb : MonoBehaviour
{
    private Attack attacklauncher;
    [SerializeField] private float explosionDelay = 1f;
    [SerializeField] private float explosionRange = 3f;
    [SerializeField] private float shockWaveForce = 7f;
    [SerializeField] private LayerMask charMask;

    public void Lauch(Attack launcher)
    {
        this.attacklauncher = launcher;
        Invoke(nameof(Explode), explosionDelay);
    }

    private void Explode()
    {
        Collider2D[] cols = PhysicsToric.OverlapCircleAll(transform.position, explosionRange, charMask);
        foreach (Collider2D col in cols)
        {
            if(col.CompareTag("Char"))
            {
                GameObject charHit = col.GetComponent<ToricObject>().original;
                attacklauncher.OnTouchEnemy(charHit);
            }
        }
        ExplosionManager.instance.CreateExplosion(transform.position, shockWaveForce);
        Destroy(gameObject, 0.5f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Circle.GizmosDraw(transform.position, explosionRange);
    }

    private void OnValidate()
    {
        explosionDelay = Mathf.Max(0f, explosionDelay);
        explosionRange = Mathf.Max(0f, explosionRange);
    }
}
