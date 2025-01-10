#if UNITY_EDITOR

using Collision2D;
using UnityEngine;
using static BezierUtility;

public class BezierUtilityTest : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] private int nbPointPerCurve = 250;
    [SerializeField] private Color velocityColor = Color.yellow;
    [SerializeField] private Color accelerationColor = Color.white;
    [SerializeField] private Color distancePointColor = Color.gray;
    [SerializeField] private float distancePointRadius = 0.1f;
    [SerializeField] private bool drawOnlyBeforeT = false;
    [SerializeField] private float controlPointRadius = 0.2f;
    [SerializeField] private Color curvatureColor = Color.blue;
    [SerializeField] private bool showCurvature = true;
    [SerializeField] private bool showHitbox = true;
    [SerializeField] private bool showHitboxes = true;

    [Header("Cubic Bezier Curve")]
    [SerializeField] private bool enableCB;
    [SerializeField] private Vector2 offsetCB;
    [SerializeField] private Vector2 start;
    [SerializeField] private Vector2 handle1;
    [SerializeField] private Vector2 handle2;
    [SerializeField] private Vector2 end;
    [SerializeField] private Color colorBC = Color.green;
    [SerializeField, ShowOnly] float lengthCBC;

    [Header("Cubic Bezier Spline")]
    [SerializeField] private Vector2[] starts;
    [SerializeField] private Vector2[] handles1;
    [SerializeField] private Vector2[] handles2;
    [SerializeField] private Vector2[] ends;
    [SerializeField] private Color colorBS = Color.green;

    [Header("Hermite Spline")]
    [SerializeField] private Vector2[] hPoints;
    [SerializeField] private Color colorH = Color.blue;

    [Header("CatmulRom Spline")]
    [SerializeField] private Vector2[] catPoints;
    [SerializeField] private Color colorCAT = Color.red;

    [Header("Cardinal Spline")]
    [SerializeField] private Vector2[] carPoints;
    [SerializeField] private float tension;
    [SerializeField] private Color colorCAR = Color.magenta;

    [Header("B Spline")]
    [SerializeField] private Vector2[] bPoints;
    [SerializeField] private Color colorB = Color.cyan;

    [SerializeField, Range(0f, 1f)] float t;
    [SerializeField, Range(0f, 1f)] float distance;

    private float[] GetTimes()
    {
        float[] times = new float[nbPointPerCurve];
        float coeff = drawOnlyBeforeT ? t : 1f;
        for (int i = 0; i < nbPointPerCurve; i++)
        {
            times[i] = coeff * Mathf.Clamp01((float)i / (float)((nbPointPerCurve - 1)));
        }
        return times;
    }

    private void HandleCubicBezierCurve()
    {
        float[] times = GetTimes();
        CubicBezierCurve cubicBezierCurve = new CubicBezierCurve(start + offsetCB, end + offsetCB, handle1 + offsetCB, handle2 + offsetCB);
        Vector2[] points = cubicBezierCurve.EvaluateFullCurve(times);
        Vector2 vel = cubicBezierCurve.Velocity(t);
        Vector2 acc = cubicBezierCurve.Acceleration(t);
        lengthCBC = cubicBezierCurve.length;
        Vector2 distancePoint = cubicBezierCurve.EvaluateDistance(distance);
        float curvatureRadius = cubicBezierCurve.CurvatureRadius(t);
        Hitbox hitbox = cubicBezierCurve.Hitbox();
        Hitbox[] hitboxes = cubicBezierCurve.Hitboxes();

        Circle.GizmosDraw(distancePoint, distancePointRadius, distancePointColor, true);

        Gizmos.color = colorBC;
        Vector2 point = points[0];
        for (int i = 1; i < points.Length; i++)
        {
            Gizmos.DrawLine(point, points[i]);
            point = points[i];
        }
        Gizmos.DrawLine(start + offsetCB, handle1 + offsetCB);
        Gizmos.DrawLine(end + offsetCB, handle2 + offsetCB);
        Circle.GizmosDraw(start + offsetCB, controlPointRadius, colorBC, true);
        Circle.GizmosDraw(handle1 + offsetCB, controlPointRadius, colorBC, true);
        Circle.GizmosDraw(handle2 + offsetCB, controlPointRadius, colorBC, true);
        Circle.GizmosDraw(end + offsetCB, controlPointRadius, colorBC, true);

        Gizmos.color = velocityColor;
        Vector2 currentPoint = cubicBezierCurve.Evaluate(t);
        Useful.GizmoDrawVector(currentPoint, vel.normalized, 1f);
        Gizmos.color = accelerationColor;
        Useful.GizmoDrawVector(currentPoint, acc.normalized, 1f);

        if (showCurvature)
        {
            Circle.GizmosDraw(currentPoint + vel.NormalVector() * curvatureRadius, curvatureRadius, curvatureColor);
        }
        if (showHitbox)
        {
            Hitbox.GizmosDraw(hitbox, colorBC);
        }
        if (showHitboxes)
        {
            foreach (Hitbox h in hitboxes)
            {
                Hitbox.GizmosDraw(h, colorBC);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if(enableCB)
        {
            HandleCubicBezierCurve();
        }
    }

    private void OnValidate()
    {
        distance = Mathf.Max(distance, 0f);
    }
}

#endif