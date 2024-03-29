#if UNITY_EDITOR

using Collision2D;
using UnityEngine;

public class PhysicToricTest : MonoBehaviour
{
    private Color color;
    [SerializeField] private bool useCircle = true;
    [SerializeField] private LayerMask mask;
    [SerializeField] private float radius;
    [SerializeField] private Vector2 size;

    private Vector2 GetMousePos() => Useful.mainCamera.ScreenToWorldPoint(InputManager.mousePosition);


    private void Update()
    {
        Vector2 mousePos = GetMousePos();
        UnityEngine.Collider2D collider = null;

        if (InputManager.GetKeyDown(KeyCode.G))
        {
            int a = 12;
        }

        if (useCircle)
        {
            collider = PhysicsToric.OverlapCircle(mousePos, radius, mask);
        }
        else
        {
            collider = PhysicsToric.OverlapBox(mousePos, size, 0f, mask);
        }
        color = collider != null ? Color.red : Color.green;
    }

    private void OnValidate()
    {
        radius = Mathf.Max(0f, radius);
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 mousePos = PhysicsToric.GetPointInsideBounds(GetMousePos());
        Gizmos.color = color;
        if (useCircle)
        {
            Circle.GizmosDraw(mousePos, radius);
        }
        else
        {
            Hitbox.GizmosDraw(mousePos, size);
        }
    }
}

#endif