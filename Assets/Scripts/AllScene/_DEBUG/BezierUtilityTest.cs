#if UNITY_EDITOR

using Collision2D;
using System.Linq;
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
    [SerializeField, ShowOnly] float lengthCB;

    [Header("Cubic Bezier Spline")]
    [SerializeField] private bool enableBS;
    [SerializeField] private Vector2 offsetBS;
    [SerializeField] private Vector2[] bsPoints;
    [SerializeField] private Vector2[] bsHandles;
    [SerializeField] private Color colorBS = Color.green;
    [SerializeField, ShowOnly] float lengthBS;

    [Header("Hermite Spline")]
    [SerializeField] private bool enableH;
    [SerializeField] private Vector2 offsetH;
    [SerializeField] private Vector2[] hPoints;
    [SerializeField] private Color colorH = Color.blue;
    [SerializeField, ShowOnly] float lengthH;

    [Header("CatmulRom Spline")]
    [SerializeField] private bool enableCAT;
    [SerializeField] private Vector2 offsetCAT;
    [SerializeField] private Vector2[] catPoints;
    [SerializeField] private Color colorCAT = Color.red;
    [SerializeField, ShowOnly] float lengthCAT;

    [Header("Cardinal Spline")]
    [SerializeField] private bool enableCAR;
    [SerializeField] private Vector2 offsetCAR;
    [SerializeField] private Vector2[] carPoints;
    [SerializeField, Range(0f, 1f)] private float tension;
    [SerializeField] private Color colorCAR = Color.magenta;
    [SerializeField, ShowOnly] float lengthCAR;

    [Header("B Spline")]
    [SerializeField] private bool enableB;
    [SerializeField] private Vector2 offsetB;
    [SerializeField] private Vector2[] bPoints;
    [SerializeField] private Color colorB = Color.cyan;
    [SerializeField, ShowOnly] float lengthB;

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
        lengthCB = cubicBezierCurve.length;
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

    private void HandleCubicBezierSpline()
    {
        if(bsPoints.Length < 2 || (2 * bsPoints.Length - 2 != bsHandles.Length))
            return;

        Vector2[] controlsPoints = (Vector2[])bsPoints.Clone();
        Vector2[] handles = (Vector2[])bsHandles.Clone();
        for (int i = 0; i < controlsPoints.Length; i++)
        {
            controlsPoints[i] += offsetBS;
        }
        for (int i = 0; i < handles.Length; i++)
        {
            handles[i] += offsetBS;
        }

        float[] times = GetTimes();
        CubicBezierSpline cubicBezierspline = new CubicBezierSpline(controlsPoints, handles);
        Vector2[] points = cubicBezierspline.EvaluateFullCurve(times);
        Vector2 vel = cubicBezierspline.Velocity(t);
        Vector2 acc = cubicBezierspline.Acceleration(t);
        lengthBS = cubicBezierspline.length;
        Vector2 distancePoint = cubicBezierspline.EvaluateDistance(distance);
        float curvatureRadius = cubicBezierspline.CurvatureRadius(t);
        Hitbox hitbox = cubicBezierspline.Hitbox();
        Hitbox[] hitboxes = cubicBezierspline.Hitboxes();

        Circle.GizmosDraw(distancePoint, distancePointRadius, distancePointColor, true);

        Gizmos.color = colorBS;
        Vector2 point = points[0];
        for (int i = 1; i < points.Length; i++)
        {
            Gizmos.DrawLine(point, points[i]);
            point = points[i];
        }

        Gizmos.DrawLine(controlsPoints[0], handles[0]);
        Gizmos.DrawLine(controlsPoints.Last(), handles.Last());

        int indexHandle = 1;
        for (int i = 1; i < controlsPoints.Length - 1; i++)
        {
            Gizmos.DrawLine(controlsPoints[i], handles[indexHandle]);
            Gizmos.DrawLine(controlsPoints[i], handles[indexHandle + 1]);
            indexHandle += 2;
        }

        foreach (Vector2 controlPoint in controlsPoints)
        {
            Circle.GizmosDraw(controlPoint, controlPointRadius, colorBS, true);
        }
        foreach (Vector2 handle in handles)
        {
            Circle.GizmosDraw(handle, controlPointRadius, colorBS, true);
        }

        Gizmos.color = velocityColor;
        Vector2 currentPoint = cubicBezierspline.Evaluate(t);
        Useful.GizmoDrawVector(currentPoint, vel.normalized, 1f);
        Gizmos.color = accelerationColor;
        Useful.GizmoDrawVector(currentPoint, acc.normalized, 1f);

        if (showCurvature)
        {
            Circle.GizmosDraw(currentPoint + vel.NormalVector() * curvatureRadius, curvatureRadius, curvatureColor);
        }
        if (showHitbox)
        {
            Hitbox.GizmosDraw(hitbox, colorBS);
        }
        if (showHitboxes)
        {
            foreach (Hitbox h in hitboxes)
            {
                Hitbox.GizmosDraw(h, colorBS);
            }
        }
    }

    private void HandleHermiteSpline()
    {
        if (hPoints.Length < 2)
            return;

        Vector2[] controlsPoints = (Vector2[])hPoints.Clone();
        for (int i = 0; i < controlsPoints.Length; i++)
        {
            controlsPoints[i] += offsetH;
        }

        float[] times = GetTimes();
        HermiteSpline hermiteSpline = new HermiteSpline(controlsPoints);
        Vector2[] points = hermiteSpline.EvaluateFullCurve(times);
        Vector2 vel = hermiteSpline.Velocity(t);
        Vector2 acc = hermiteSpline.Acceleration(t);
        lengthH = hermiteSpline.length;
        Vector2 distancePoint = hermiteSpline.EvaluateDistance(distance);
        float curvatureRadius = hermiteSpline.CurvatureRadius(t);
        Hitbox hitbox = hermiteSpline.Hitbox();
        Hitbox[] hitboxes = hermiteSpline.Hitboxes();

        Circle.GizmosDraw(distancePoint, distancePointRadius, distancePointColor, true);

        Gizmos.color = colorH;
        Vector2 point = points[0];
        for (int i = 1; i < points.Length; i++)
        {
            Gizmos.DrawLine(point, points[i]);
            point = points[i];
        }

        foreach (Vector2 controlPoint in controlsPoints)
        {
            Circle.GizmosDraw(controlPoint, controlPointRadius, colorH, true);
        }

        Gizmos.color = velocityColor;
        Vector2 currentPoint = hermiteSpline.Evaluate(t);
        Useful.GizmoDrawVector(currentPoint, vel.normalized, 1f);
        Gizmos.color = accelerationColor;
        Useful.GizmoDrawVector(currentPoint, acc.normalized, 1f);

        if (showCurvature)
        {
            Circle.GizmosDraw(currentPoint + vel.NormalVector() * curvatureRadius, curvatureRadius, curvatureColor);
        }
        if (showHitbox)
        {
            Hitbox.GizmosDraw(hitbox, colorH);
        }
        if (showHitboxes)
        {
            foreach (Hitbox h in hitboxes)
            {
                Hitbox.GizmosDraw(h, colorH);
            }
        }
    }

    private void HandleCatmulRomSpline()
    {
        if (catPoints.Length < 2)
            return;

        Vector2[] controlsPoints = (Vector2[])catPoints.Clone();
        for (int i = 0; i < controlsPoints.Length; i++)
        {
            controlsPoints[i] += offsetCAT;
        }

        float[] times = GetTimes();
        CatmulRomSpline catmulRomSpline = new CatmulRomSpline(controlsPoints);
        Vector2[] points = catmulRomSpline.EvaluateFullCurve(nbPointPerCurve);
        Vector2 currentPoint = catmulRomSpline.Evaluate(t);
        Vector2 vel = catmulRomSpline.Velocity(t);
        Vector2 acc = catmulRomSpline.Acceleration(t);
        lengthCAT = catmulRomSpline.length;
        Vector2 distancePoint = catmulRomSpline.EvaluateDistance(distance);
        float curvatureRadius = catmulRomSpline.CurvatureRadius(t);
        Hitbox hitbox = catmulRomSpline.Hitbox();
        Hitbox[] hitboxes = catmulRomSpline.Hitboxes();

        Circle.GizmosDraw(distancePoint, distancePointRadius, distancePointColor, true);

        Gizmos.color = colorCAT;
        Vector2 point = points[0];
        for (int i = 1; i < points.Length; i++)
        {
            Gizmos.DrawLine(point, points[i]);
            point = points[i];
        }

        foreach (Vector2 controlPoint in controlsPoints)
        {
            Circle.GizmosDraw(controlPoint, controlPointRadius, colorCAT, true);
        }

        Gizmos.color = velocityColor;
        Useful.GizmoDrawVector(currentPoint, vel.normalized, 1f);
        Gizmos.color = accelerationColor;
        Useful.GizmoDrawVector(currentPoint, acc.normalized, 1f);

        if (showCurvature)
        {
            Circle.GizmosDraw(currentPoint + vel.NormalVector() * curvatureRadius, curvatureRadius, curvatureColor);
        }
        if (showHitbox)
        {
            Hitbox.GizmosDraw(hitbox, colorCAT);
        }
        if (showHitboxes)
        {
            foreach (Hitbox h in hitboxes)
            {
                Hitbox.GizmosDraw(h, colorCAT);
            }
        }
    }

    private void HandleCardinalSpline()
    {
        if (carPoints.Length < 2)
            return;

        Vector2[] controlsPoints = (Vector2[])carPoints.Clone();
        for (int i = 0; i < controlsPoints.Length; i++)
        {
            controlsPoints[i] += offsetCAR;
        }

        float[] times = GetTimes();
        CardinalSpline cardinaleSpline = new CardinalSpline(controlsPoints, tension);
        Vector2[] points = cardinaleSpline.EvaluateFullCurve(nbPointPerCurve);
        Vector2 currentPoint = cardinaleSpline.Evaluate(t);
        Vector2 vel = cardinaleSpline.Velocity(t);
        Vector2 acc = cardinaleSpline.Acceleration(t);
        lengthCAR = cardinaleSpline.length;
        Vector2 distancePoint = cardinaleSpline.EvaluateDistance(distance);
        float curvatureRadius = cardinaleSpline.CurvatureRadius(t);
        Hitbox hitbox = cardinaleSpline.Hitbox();
        Hitbox[] hitboxes = cardinaleSpline.Hitboxes();

        Circle.GizmosDraw(distancePoint, distancePointRadius, distancePointColor, true);

        Gizmos.color = colorCAR;
        Vector2 point = points[0];
        for (int i = 1; i < points.Length; i++)
        {
            Gizmos.DrawLine(point, points[i]);
            point = points[i];
        }

        foreach (Vector2 controlPoint in controlsPoints)
        {
            Circle.GizmosDraw(controlPoint, controlPointRadius, colorCAR, true);
        }

        Gizmos.color = velocityColor;
        Useful.GizmoDrawVector(currentPoint, vel.normalized, 1f);
        Gizmos.color = accelerationColor;
        Useful.GizmoDrawVector(currentPoint, acc.normalized, 1f);

        if (showCurvature)
        {
            Circle.GizmosDraw(currentPoint + vel.NormalVector() * curvatureRadius, curvatureRadius, curvatureColor);
        }
        if (showHitbox)
        {
            Hitbox.GizmosDraw(hitbox, colorCAR);
        }
        if (showHitboxes)
        {
            foreach (Hitbox h in hitboxes)
            {
                Hitbox.GizmosDraw(h, colorCAR);
            }
        }
    }

    private void HandleBSpline()
    {
        if (carPoints.Length < 2)
            return;

        Vector2[] controlsPoints = (Vector2[])bPoints.Clone();
        for (int i = 0; i < controlsPoints.Length; i++)
        {
            controlsPoints[i] += offsetB;
        }

        float[] times = GetTimes();
        BSpline bSpline = new BSpline(controlsPoints);
        Vector2[] points = bSpline.EvaluateFullCurve(times);
        Vector2 currentPoint = bSpline.Evaluate(t);
        Vector2 vel = bSpline.Velocity(t);
        Vector2 acc = bSpline.Acceleration(t);
        lengthB = bSpline.length;
        Vector2 distancePoint = bSpline.EvaluateDistance(distance);
        float curvatureRadius = bSpline.CurvatureRadius(t);
        Hitbox hitbox = bSpline.Hitbox();
        Hitbox[] hitboxes = bSpline.Hitboxes();

        Circle.GizmosDraw(distancePoint, distancePointRadius, distancePointColor, true);

        Gizmos.color = colorB;
        Vector2 point = points[0];
        for (int i = 1; i < points.Length; i++)
        {
            Gizmos.DrawLine(point, points[i]);
            point = points[i];
        }

        foreach (Vector2 controlPoint in controlsPoints)
        {
            Circle.GizmosDraw(controlPoint, controlPointRadius, colorB, true);
        }

        Gizmos.color = velocityColor;
        Useful.GizmoDrawVector(currentPoint, vel.normalized, 1f);
        Gizmos.color = accelerationColor;
        Useful.GizmoDrawVector(currentPoint, acc.normalized, 1f);

        if (showCurvature)
        {
            Circle.GizmosDraw(currentPoint + vel.NormalVector() * curvatureRadius, curvatureRadius, curvatureColor);
        }
        if (showHitbox)
        {
            Hitbox.GizmosDraw(hitbox, colorB);
        }
        if (showHitboxes)
        {
            foreach (Hitbox h in hitboxes)
            {
                Hitbox.GizmosDraw(h, colorB);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if(enableCB)
        {
            HandleCubicBezierCurve();
        }
        if (enableBS)
        {
            HandleCubicBezierSpline();
        }
        if(enableH)
        {
            HandleHermiteSpline();
        }
        if (enableCAT)
        {
            HandleCatmulRomSpline();
        }
        if(enableCAR)
        {
            HandleCardinalSpline();
        }
        if(enableB)
        {
            HandleBSpline();
        }
    }

    private void OnValidate()
    {
        distance = Mathf.Max(distance, 0f);
    }
}

#endif