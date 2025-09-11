using UnityEngine;

public class BoomerangAttractor : MonoBehaviour
{
    public float distanceFromGround;

    public void OnBeenReachByBoomerang()
    {
        Destroy(gameObject);
    }

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + (distanceFromGround * Vector2.down));
    }

    private void OnValidate()
    {
        distanceFromGround = Mathf.Max(distanceFromGround, 0f);
    }

#endif
}
