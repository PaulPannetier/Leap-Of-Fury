using UnityEngine;

public class Jumper : MonoBehaviour
{
    [Range(0f, 360f)] public float angleDir;
    public float force;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Useful.GizmoDrawVector((Vector2)transform.position, new Vector2(Mathf.Cos(angleDir * Mathf.Deg2Rad), Mathf.Sin(angleDir * Mathf.Deg2Rad)));
    }
}
