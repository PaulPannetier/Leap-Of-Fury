using UnityEngine;
using UnityEditor;
using Collision2D;
using static BezierUtility;
using System;
using System.Collections.Generic;

[ExecuteInEditMode]
public class CurveGenerator : MonoBehaviour
{
    public enum SplineType
    {
        Bezier,
        Hermite,
        Catmulrom,
        Cardinal,
        BSline
    }

#if UNITY_EDITOR

    private EdgeCollider2D edgeCollider;
    private Vector2[] curve;
    private Spline spline;

    [Header("Collider")]
    [SerializeField] private bool generateCollider = false;
    [SerializeField] private float colliderEdgeRadius;
    [SerializeField] private int colliderResolutionPerCurve = 7;

    [Header("Shape")]
    [SerializeField] private SplineType splineType = SplineType.Catmulrom;
    [SerializeField] private Vector2[] controlPoints;
    [SerializeField] private Vector2[] handles;
    [SerializeField, Range(0f, 1f)] private float tension;
    [SerializeField] private int pointsPerCurve = 70;
    [SerializeField] private bool showHitbox;
    [SerializeField] private bool showHitboxes;

    private void AddEdgeCollider()
    {
        edgeCollider = GetComponent<EdgeCollider2D>();
        if(edgeCollider == null)
        {
            edgeCollider = gameObject.AddComponent<EdgeCollider2D>();
            edgeCollider.edgeRadius = colliderEdgeRadius;
        }
    }

    private void CreateSpline()
    {
        spline = null;
        switch (splineType)
        {
            case SplineType.Bezier:
                spline = new CubicBezierSpline(controlPoints, handles);
                break;
            case SplineType.Hermite:
                spline = new HermiteSpline(controlPoints);
                break;
            case SplineType.Catmulrom:
                spline = new CatmulRomSpline(controlPoints);
                break;
            case SplineType.Cardinal:
                spline = new CardinalSpline(controlPoints, tension);
                break;
            case SplineType.BSline:
                spline = new BSpline(controlPoints);
                break;
            default:
                break;
        }

        curve = spline.EvaluateFullCurve(pointsPerCurve * (controlPoints.Length - 1));
    }

    private void GenerateCollider()
    {
        if (edgeCollider == null)
        {
            AddEdgeCollider();
        }
        edgeCollider.edgeRadius = colliderEdgeRadius;

        Vector2[] colliderPoints = new Vector2[(controlPoints.Length - 1) * colliderResolutionPerCurve];
        int indexColliderPoints = 0;
        int gap = Mathf.Max(pointsPerCurve / colliderResolutionPerCurve, 1);
        for (int i = 0; i < controlPoints.Length - 1; i++)
        {
            for (int j = 0; j < colliderResolutionPerCurve; j++)
            {
                if (indexColliderPoints >= colliderPoints.Length || i * pointsPerCurve + j * gap >= curve.Length)
                    break;
                colliderPoints[indexColliderPoints] = curve[i * pointsPerCurve + j * gap];
                indexColliderPoints++;
            }
        }
        colliderPoints[colliderPoints.Length - 1] = curve[curve.Length - 1];

        edgeCollider.points = colliderPoints;
    }

    #region Gizmos/OnValidate

    [SerializeField] private Vector2[] testPoints;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        for (int i = 0; i < testPoints.Length; i++)
        {
            Circle.GizmosDraw(testPoints[i], 0.3f);
        }

        Vector2[] bCurve = spline.EvaluateFullCurve(pointsPerCurve * (testPoints.Length - 1));

        if(bCurve != null && bCurve.Length >= 2)
        {
            Vector2 beg2 = bCurve[0];
            for (int i = 1; i < bCurve.Length; i++)
            {
                Gizmos.DrawLine(beg2, bCurve[i]);
                beg2 = bCurve[i];
            }
        }

        Gizmos.color = Color.red;
        int nbVel = pointsPerCurve * (testPoints.Length - 1) / 4;
        for (int i = 0; i <= nbVel; i++)
        {
            float t = (float)i / nbVel;
            Vector2 p = spline.Evaluate(t);
            Circle.GizmosDraw(p, 0.1f);
            Vector2 s = spline.Velocity(t);
            Useful.GizmoDrawVector(p, s.normalized);
        }

        //Hitbox.GizmosDraw(bezierSpline.Hitbox());
        foreach (Hitbox h in spline.Hitboxes())
        {
            Hitbox.GizmosDraw(h);
        }

        return;
        Gizmos.color = Color.green;
        foreach (Vector2 point in controlPoints)
        {
            Circle.GizmosDraw(point, 0.3f);
        }

        if(splineType == SplineType.Bezier)
        {
            Gizmos.color = Color.green;
            foreach (Vector2 point in handles)
            {
                Circle.GizmosDraw(point, 0.3f);
            }
        }

        if(curve != null && curve.Length > 0)
        {
            Vector2 beg = curve[0];
            for (int i = 1; i < curve.Length; i++)
            {
                Gizmos.DrawLine(beg, curve[i]);
                beg = curve[i];
            }
        }

        if(spline != null)
        {
            if (showHitbox)
            {
                Hitbox.GizmosDraw(spline.Hitbox());
            }

            if (showHitboxes)
            {
                Hitbox[] hitboxes = spline.Hitboxes();
                foreach (Hitbox hitbox in hitboxes)
                {
                    Hitbox.GizmosDraw(hitbox);
                }
            }
        }
    }

    private void OnValidate()
    {
        spline = new BSpline(testPoints);

        return;

        colliderEdgeRadius = Mathf.Max(0f, colliderEdgeRadius);
        colliderResolutionPerCurve = Mathf.Max(1, colliderResolutionPerCurve);
        pointsPerCurve = Mathf.Max(0, pointsPerCurve);
        if(controlPoints == null)
            controlPoints = Array.Empty<Vector2>();
        if(handles == null)
            handles = Array.Empty<Vector2>();

        if(splineType == SplineType.Bezier)
        {
            if(controlPoints.Length >= 2 && 2 * controlPoints.Length - 2 != handles.Length)
            {
                List<Vector2> tmp = new List<Vector2>(handles);
                while(tmp.Count > 2 * controlPoints.Length - 2)
                {
                    tmp.RemoveLast();
                }
                while (tmp.Count < 2 * controlPoints.Length - 2)
                {
                    tmp.Add(Vector2.zero);
                }
            }
        }
        else
        {
            handles = Array.Empty<Vector2>();
        }

        if(controlPoints.Length >= 2)
        {
            CreateSpline();
            if (generateCollider)
            {
                GenerateCollider();
            }
        }
    }

    #endregion

#endif
}