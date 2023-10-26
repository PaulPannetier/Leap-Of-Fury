using Collision2D;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class BezierUtility
{
    #region Static

    private static float cache0;

    private static Hitbox ComputeBezierHitbox(in Vector2 start, in Vector2 handle1, in Vector2 handle2, in Vector2 end)
    {
        //Search t €[0,1] | P'(t).x == 0 || P'(t).y == 0
        float[] t = new float[4] { -1f, -1f, -1f, -1f };
        Vector2 a = -3f * start + 9f * handle1 - 9f * handle2 + 3f * end;
        Vector2 b = 6f * start - 12f * handle1 + 6f * handle2;
        Vector2 c = -3f * start + 3f * handle1;

        cache0 = (b.x * b.x) - (4f * a.x * c.x);
        if (cache0 >= 0f)
        {
            if (cache0 <= Mathf.Epsilon)
            {
                t[0] = -b.x / (2f * a.x);
                VerifyTValue(ref t, 0);
            }
            else
            {
                float sqrDelta = Mathf.Sqrt(cache0);
                t[0] = (-b.x + sqrDelta) / (2f * a.x);
                t[1] = (-b.x - sqrDelta) / (2f * a.x);
                VerifyTValue(ref t, 0);
                VerifyTValue(ref t, 1);
            }
        }

        cache0 = (b.y * b.y) - (4f * a.y * c.y);
        if (cache0 >= 0f)
        {
            if (cache0 <= Mathf.Epsilon)
            {
                t[2] = -b.y / (2f * a.y);
                VerifyTValue(ref t, 2);
            }
            else
            {
                float sqrDelta = Mathf.Sqrt(cache0);
                t[2] = (-b.y + sqrDelta) / (2f * a.y);
                t[3] = (-b.y - sqrDelta) / (2f * a.y);
                VerifyTValue(ref t, 2);
                VerifyTValue(ref t, 3);
            }
        }

        void VerifyTValue(ref float[] t, int index)
        {
            if (t[index] < 0f || t[index] > 1f)
                t[index] = -1f;
        }

        List<Vector2> extremaPoints = new List<Vector2>()
        {
            start, end
        };

        for (int i = 0; i < 4; i++)
        {
            if (t[i] >= 0f)
            {
                cache0 = t[i] * t[i] * t[i];
                Vector2 evaluatePoint = start * (-cache0 + 3f * t[i] * t[i] - 3f * t[i] + 1f) + handle1 * (3f * cache0 - 6f * t[i] * t[i] + 3f * t[i]) + handle2 * (-3f * cache0 + 3f * t[i] * t[i]) + end * cache0;
                extremaPoints.Add(evaluatePoint);
            }
        }

        //on crée la boite de collision avec les points extremes
        float xMin = extremaPoints[0].x, xMax = extremaPoints[0].x, yMin = extremaPoints[0].y, yMax = extremaPoints[0].y;

        for (int i = 1; i < extremaPoints.Count; i++)
        {
            xMin = Mathf.Min(xMin, extremaPoints[i].x);
            xMax = Mathf.Max(xMax, extremaPoints[i].x);
            yMin = Mathf.Min(yMin, extremaPoints[i].y);
            yMax = Mathf.Max(yMax, extremaPoints[i].y);
        }

        return new Hitbox(new Vector2((xMin + xMax) * 0.5f, (yMin + yMax) * 0.5f), new Vector2(xMax - xMin, yMax - yMin));
    }

    #endregion

    #region Spline class

    public abstract class Spline
    {
        public abstract Vector2 Evaluate(float t);
        public abstract Vector2 Velocity(float t);
        public virtual Vector2 Normal(float t) => Velocity(t).NormalVector();
        public abstract Vector2[] EvaluateFullCurve(int nbPoints);
        public abstract Hitbox Hitbox(); 
        public abstract Hitbox[] Hitboxes();
    }

    #endregion

    #region CubicBezierCurve

    public class CubicBezierCurve : Spline
    {
        private Vector2 start, end, handle1, handle2;

        public CubicBezierCurve(in Vector2 start, in Vector2 end, in Vector2 handle1, in Vector2 handle2)
        {
            this.start = start;
            this.end = end;
            this.handle1 = handle1;
            this.handle2 = handle2;
        }

        public override Vector2 Evaluate(float t)
        {
            t = Mathf.Clamp01(t);
            cache0 = t * t * t;
            return start * (-cache0 + 3f * t * t - 3f * t + 1f) + handle1 * (3f * cache0 - 6f * t * t + 3f * t) + handle2 * (-3f * cache0 + 3f * t * t) + end * cache0;
        }

        public override Vector2[] EvaluateFullCurve(int nbPoints)
        {
            Vector2 P1 = -3f * start + 3f * handle1;
            Vector2 P2 = 3f * start - 6f * handle1 + 3f * handle2;
            Vector2 P3 = -start + 3f * handle1 - 3f * handle2 + end;

            Vector2[] res = new Vector2[nbPoints];
            float t;
            float oneOnbPointM1 = 1f / (nbPoints - 1);
            for (int i = 0; i < nbPoints; i++)
            {
                t = (float) i * oneOnbPointM1;
                res[i] = start + t * P1 + (t * t) * P2 + (t * t * t) * P3;
            }
            return res;
        }

        public override Vector2 Velocity(float t)
        {
            t = Mathf.Clamp01(t);
            cache0 = t * t;
            return start * (-3f + 6f * t - 3f * cache0) + handle1 * (9f * cache0 - 12f * t + 3f) + handle2 * (6f * t - 9f * cache0) + 3f * cache0 * end;
        }

        public override Hitbox Hitbox()
        {
            return ComputeBezierHitbox(start, handle1, handle2, end);
        }

        public override Hitbox[] Hitboxes()
        {
            return new Hitbox[1] { Hitbox() };
        }
    }

    #endregion

    #region CubicBezierSpline

    public class CubicBezierSpline : Spline
    {
        private Vector2[] points;
        private Vector2[] handles;

        public CubicBezierSpline(Vector2[] points, Vector2[] handles)
        {
            if (points == null || points.Length < 2)
            {
                int p = points == null ? 0 : points.Length;
                throw new Exception($"A Bezier spline have at least 2 controls points, got {p}");
            }

            if (2 * points.Length - 2 != handles.Length)
            {
                int a = 2 * points.Length - 2;
                throw new Exception($"The number of point doesn't match the number of handles, 2 * points.Length - 2 must be equal to handles.Length, or got {a} != {handles.Length}");
            }

            this.points = points;
            this.handles = handles;
        }

        private (Vector2 handle1, Vector2 handle2) GetHandles(int i)
        {
            int c = 2 * i;
            return (handles[c], handles[c + 1]);
        }

        public override Vector2 Evaluate(float t)
        {
            t = Mathf.Clamp01(t);
            float interLength = 1f / (points.Length - 1);
            int i = t < 1f ? (t / interLength).Floor() : points.Length - 2;
            float newT = (t - (i * interLength)) / interLength;
            (Vector2 h1, Vector2 h2) = GetHandles(i);

            cache0 = newT * newT;
            return points[i] + newT * 3f * (h1 - points[i]) + cache0 * (3f * points[i] - 6f * h1 + 3f * h2) + cache0 * newT * (3f * h1 - points[i] - 3f * h2 + points[i + 1]);
        }

        public override Vector2[] EvaluateFullCurve(int nbPoints)
        {
            Vector2[] res = new Vector2[nbPoints];

            int nbPointByCurve = ((float)(nbPoints - 1) / (points.Length - 1)).Round();
            float oneOnbPointByCurveM1 = 1f / (nbPointByCurve - 1);//cache
            int indexRes = 0;
            Vector2 P0, P1, P2, P3, h1, h2;
            for (int i = 0; i < points.Length - 2; i++)
            {
                (h1, h2) = GetHandles(i);

                P0 = points[i];
                P1 = 3f * (h1 - P0);
                P2 = 3f * P0 - 6f * h1 + 3f * h2;
                P3 = 3f * (h1 - h2) - P0 + points[i + 1];

                for (int j = 0; j < nbPointByCurve; j++)
                {
                    float t = j * oneOnbPointByCurveM1;
                    cache0 = t * t;
                    res[indexRes] = P0 + t * P1 + cache0 * P2 + cache0 * t * P3;
                    indexRes++;
                }
            }

            //last Curve
            int max = nbPoints - indexRes - 1;
            float maxM1 = max - 1;

            (h1, h2) = GetHandles(points.Length - 2);
            P0 = points[points.Length - 2];
            P1 = 3f * (h1 - P0);
            P2 = 3f * P0 - 6f * h1 + 3f * h2;
            P3 = 3f * (h1 - h2) - P0 + points[points.Length - 1];
            for (int i = 0; i < max; i++)
            {
                float t = i / maxM1;
                cache0 = t * t;
                res[indexRes] = P0 + t * P1 + cache0 * P2 + cache0 * t * P3;
                indexRes++;
            }

            res[nbPoints - 1] = points[points.Length - 1];
            return res;
        }

        public override Hitbox Hitbox()
        {
            Hitbox[] hitboxes = Hitboxes();
            float xMin = float.MaxValue, xMax = float.MinValue, yMin = float.MaxValue, yMax = float.MinValue;

            foreach (Hitbox hitbox in hitboxes)
            {
                xMin = Mathf.Min(xMin, hitbox.center.x - 0.5f * hitbox.size.x);
                xMax = Mathf.Max(xMax, hitbox.center.x + 0.5f * hitbox.size.x);
                yMin = Mathf.Min(yMin, hitbox.center.y - 0.5f * hitbox.size.y);
                yMax = Mathf.Max(yMax, hitbox.center.y + 0.5f * hitbox.size.y);
            }
            return new Hitbox(new Vector2((xMin + xMax) * 0.5f, (yMin + yMax) * 0.5f), new Vector2(xMax - xMin, yMax - yMin));
        }

        public override Hitbox[] Hitboxes()
        {
            Hitbox[] res = new Hitbox[points.Length - 1];
            Vector2 h1, h2;
            for (int i = 0; i < res.Length; i++)
            {
                (h1, h2) = GetHandles(i);
                res[i] = ComputeBezierHitbox(points[i], h1, h2, points[i + 1]);
            }
            return res;
        }

        public override Vector2 Velocity(float t)
        {
            t = Mathf.Clamp01(t);
            float interLength = 1f / (points.Length - 1);
            int i = t < 1f ? (t / interLength).Floor() : points.Length - 2;
            float newT = (t - (i * interLength)) / interLength;
            (Vector2 h1, Vector2 h2) = GetHandles(i);
            cache0 = newT * newT;
            return points[i] * (-3f * cache0 + 6f * newT - 3f) + h1 * (9f * cache0 - 12f * newT + 3f) +
                h2 * (-9f * cache0 + 6f * newT) + points[i + 1 ] * (3f * cache0);
        }
    }

    #endregion

    #region HermiteSpline

    public class HermiteSpline : Spline
    {
        private const float oneO3 = 1f / 3f;

        protected Vector2[] points;
        protected Vector2[] velocities;

        protected HermiteSpline()
        {

        }

        public HermiteSpline(Vector2[] points)
        {
            if(points == null)
            {
                throw new ArgumentNullException("Points cannot be null");
            }
            if (points.Length < 2)
            {
                throw new Exception($"A Hermite Spline have at least two points, got only {points.Length} point.");
            }

            this.points = points;
            velocities = new Vector2[points.Length];

            for (int i = 1; i < points.Length - 1; i++)
            {
                velocities[i] = points[i + 1] - points[i - 1];
            }

            velocities[0] = 2f * (points[1] - points[0]);
            velocities[points.Length - 1] = 2f * (points[points.Length - 1] - points[points.Length - 2]);
        }

        public override Vector2 Evaluate(float t)
        {
            t = Mathf.Clamp01(t);
            float interLength = 1f / (points.Length - 1);
            int i = t < 1f ? (t / interLength).Floor() : points.Length - 2;
            float newT = (t - (i * interLength)) / interLength;

            cache0 = newT * newT;
            return points[i] + newT * velocities[i] + cache0 * (3f * (points[i + 1] - points[i]) - 2f * velocities[i] - velocities[i + 1]) +
                cache0 * newT * (2f * (points[i] - points[i + 1]) + velocities[i] + velocities[i + 1]);
        }

        public override Vector2[] EvaluateFullCurve(int nbPoints)
        {
            Vector2[] res = new Vector2[nbPoints];

            int nbPointByCurve = ((float)(nbPoints - 1) / (points.Length - 1)).Round();
            float oneOnbPointByCurveM1 = 1f / (nbPointByCurve - 1);//cache
            int indexRes = 0;
            float t;
            Vector2 P0, P1, P2, P3;
            for (int i = 0; i < points.Length - 2; i++)
            {
                P0 = points[i];
                P1 = velocities[i];
                P2 = 3f * (points[i + 1] - points[i]) - 2f * velocities[i] - velocities[i + 1];
                P3 = 2f * (points[i] - points[i + 1]) + velocities[i] + velocities[i + 1];
                for (int j = 0; j < nbPointByCurve; j++)
                {
                    t = j * oneOnbPointByCurveM1;
                    //res[indexRes] = EvaluateHermiteCurve(points[i], velocities[i], points[i + 1], velocities[i + 1], t);
                    cache0 = t * t;
                    res[indexRes] = P0 + t * P1 + cache0 * P2 + cache0 * t * P3;
                    indexRes++;
                }
            }

            //last Curve
            int max = nbPoints - indexRes - 1;
            float oneOmaxM1 = 1f / (max - 1);
            P0 = points[points.Length - 2];
            P1 = velocities[velocities.Length - 2];
            P2 = 3f * (points[points.Length - 1] - P0) - 2f * P1 - velocities[velocities.Length - 1];
            P3 = 2f * (P0 - points[points.Length - 1]) + P1 + velocities[velocities.Length - 1];

            for (int i = 0; i < max; i++)
            {
                t = i * oneOmaxM1;
                //res[indexRes] = EvaluateHermiteCurve(points[points.Length - 2], velocities[velocities.Length - 2], points[points.Length - 1], velocities[velocities.Length - 1], t);
                cache0 = t * t;
                res[indexRes] = P0 + t * P1 + cache0 * P2 + cache0 * t * P3;
                indexRes++;
            }

            res[nbPoints - 1] = points[points.Length - 1];
            return res;
        }

        public override Hitbox Hitbox()
        {
            Hitbox[] hitboxes = Hitboxes();
            float xMin = float.MaxValue, xMax = float.MinValue, yMin = float.MaxValue, yMax = float.MinValue;

            foreach (Hitbox hitbox in hitboxes)
            {
                xMin = Mathf.Min(xMin, hitbox.center.x - 0.5f * hitbox.size.x);
                xMax = Mathf.Max(xMax, hitbox.center.x + 0.5f * hitbox.size.x);
                yMin = Mathf.Min(yMin, hitbox.center.y - 0.5f * hitbox.size.y);
                yMax = Mathf.Max(yMax, hitbox.center.y + 0.5f * hitbox.size.y);
            }
            return new Hitbox(new Vector2((xMin + xMax) * 0.5f, (yMin + yMax) * 0.5f), new Vector2(xMax - xMin, yMax - yMin));
        }

        public override Hitbox[] Hitboxes()
        {
            Hitbox[] res = new Hitbox[points.Length - 1];
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = ComputeBezierHitbox(points[i], points[i] + (velocities[i] * oneO3), points[i + 1] - (velocities[i + 1] * oneO3), points[i + 1]);
            }
            return res;
        }

        public override Vector2 Velocity(float t)
        {
            t = Mathf.Clamp01(t);
            float interLength = 1f / (points.Length - 1);
            int i = t < 1f ? (t / interLength).Floor() : points.Length - 2;
            float newT = (t - (i * interLength)) / interLength;

            return velocities[i] + 2f * newT * (3f * (points[i + 1] - points[i]) - 2f * velocities[i] - velocities[i + 1]) + 
                3f * newT * newT * (2f * (points[i] - points[i + 1]) + velocities[i] + velocities[i + 1]);
        }
    }

    #endregion

    #region CatmulRom

    public class CatmulRomSpline : HermiteSpline
    {
        public CatmulRomSpline(Vector2[] points) : base()
        {
            if (points == null)
            {
                throw new ArgumentNullException("Points cannot be null");
            }
            if (points.Length < 2)
            {
                throw new Exception($"A CatmulRom Spline have at least two points, got only {points.Length} point.");
            }

            this.points = points;
            velocities = new Vector2[points.Length];

            for (int i = 1; i < points.Length - 1; i++)
            {
                velocities[i] = 0.5f * (points[i + 1] - points[i - 1]);
            }

            velocities[0] = points[1] - points[0];
            velocities[points.Length - 1] = points[points.Length - 1] - points[points.Length - 2];
        }
    }

    #endregion

    #region Cardinal

    public class CardinalSpline : HermiteSpline
    {
        private float s;

        public CardinalSpline(Vector2[] points, float tension) : base()
        {
            if (points == null)
            {
                throw new ArgumentNullException("Points cannot be null");
            }
            if (points.Length < 2)
            {
                throw new Exception($"A Cardinal Spline have at least two points, got only {points.Length} point.");
            }

            this.points = points;
            this.s = Mathf.Clamp01(tension);
            velocities = new Vector2[points.Length];

            for (int i = 1; i < points.Length - 1; i++)
            {
                velocities[i] = s * (points[i + 1] - points[i - 1]);
            }

            velocities[0] = 2f * s * (points[1] - points[0]);
            velocities[points.Length - 1] = 2f * s * (points[points.Length - 1] - points[points.Length - 2]);
        }
    }

    #endregion

    #region Custom Spline

    public abstract class CustomSpline : Spline
    {
        private Matrix4x4 M;

        protected Vector2[] points;

        public CustomSpline(Matrix4x4 caracteristicMatrix, Vector2[] points)
        {
            if(points == null || points.Length < 2)
            {
                throw new Exception("A Spline must have at least 2 points");
            }
            this.M = caracteristicMatrix;
            this.points = points;
        }

        protected (Vector2 C0, Vector2 C1, Vector2 C2, Vector2 C3) PrecomputePolynomialValues(in Vector2 P0, in Vector2 P1, in Vector2 P2, in Vector2 P3)
        {
            Vector2 C0 = M[0,0] * P0 + M[0,1] * P1 + M[0,2] * P2 + M[0,3] * P3;
            Vector2 C1 = M[1,0] * P0 + M[1,1] * P1 + M[1,2] * P2 + M[1,3] * P3;
            Vector2 C2 = M[2,0] * P0 + M[2,1] * P1 + M[2,2] * P2 + M[2,3] * P3;
            Vector2 C3 = M[3,0] * P0 + M[3,1] * P1 + M[3,2] * P2 + M[3,3] * P3;
            return (C0, C1, C2, C3);
        }

        public override Vector2 Evaluate(float t)
        {
            return default(Vector2);
        }

        public override Vector2 Velocity(float t)
        {
            return default(Vector2);
        }

        public override Vector2[] EvaluateFullCurve(int nbPoints)
        {
            return default(Vector2[]);
        }

        public override Hitbox Hitbox()
        {
            return default(Hitbox);
        }

        public override Hitbox[] Hitboxes()
        {
            return default(Hitbox[]);
        }
    }

    #endregion

    #region BSpline

    public class BSpline : CustomSpline
    {
        private static readonly float oneO6 = 1f / 6f;

        public BSpline(Vector2[] points) : base(default, points)
        {

        }

        public override Hitbox Hitbox()
        {
            Hitbox[] hitboxes = Hitboxes();
            float xMin = float.MaxValue, xMax = float.MinValue, yMin = float.MaxValue, yMax = float.MinValue;

            foreach (Hitbox hitbox in hitboxes)
            {
                xMin = Mathf.Min(xMin, hitbox.center.x - hitbox.size.x);
                xMax = Mathf.Max(xMax, hitbox.center.x + hitbox.size.x);
                yMin = Mathf.Min(yMin, hitbox.center.y - hitbox.size.y);
                yMax = Mathf.Max(yMax, hitbox.center.y + hitbox.size.y);
            }
            return new Hitbox(new Vector2((xMin + xMax) * 0.5f, (yMin + yMax) * 0.5f), new Vector2(xMax - xMin, yMax - yMin));
        }

        public override Hitbox[] Hitboxes()
        {
            Hitbox[] res = new Hitbox[points.Length - 1];
            for (int i = 0; i < res.Length; i++)
            {
                //res[i] = HitboxBCurve(points[i], points[i] + velocities[i], points[i + 1] - velocities[i + 1], points[i + 1]);
            }
            return res;
        }

        private Hitbox HitboxBCurve(in Vector2 start, in Vector2 handle1, in Vector2 handle2, in Vector2 end)
        {
            //Search t €[0,1] | P'(t).x == 0 || P'(t).y == 0
            float[] t = new float[4] { -1f, -1f, -1f, -1f };
            Vector2 a = 0.5f * (3f * (handle1 - handle2) - start + end);
            Vector2 b = start + handle2 - 2f * handle1;
            Vector2 c = 0.5f * (handle2 - start);

            cache0 = (b.x * b.x) - (4f * a.x * c.x);
            if (cache0 >= 0f)
            {
                if (cache0 <= Mathf.Epsilon)
                {
                    t[0] = -b.x / (2f * a.x);
                    VerifyTValue(ref t, 0);
                }
                else
                {
                    float sqrDelta = Mathf.Sqrt(cache0);
                    t[0] = (-b.x + sqrDelta) / (2f * a.x);
                    t[1] = (-b.x - sqrDelta) / (2f * a.x);
                    VerifyTValue(ref t, 0);
                    VerifyTValue(ref t, 1);
                }
            }

            cache0 = (b.y * b.y) - (4f * a.y * c.y);
            if (cache0 >= 0f)
            {
                if (cache0 <= Mathf.Epsilon)
                {
                    t[2] = -b.y / (2f * a.y);
                    VerifyTValue(ref t, 2);
                }
                else
                {
                    float sqrDelta = Mathf.Sqrt(cache0);
                    t[2] = (-b.y + sqrDelta) / (2f * a.y);
                    t[3] = (-b.y - sqrDelta) / (2f * a.y);
                    VerifyTValue(ref t, 2);
                    VerifyTValue(ref t, 3);
                }
            }

            void VerifyTValue(ref float[] t, int index)
            {
                if (t[index] < 0f || t[index] > 1f)
                    t[index] = -1f;
            }

            List<Vector2> extremaPoints = new List<Vector2>
            {
                start,
                end
            };

            for (int i = 0; i < 4; i++)
            {
                if (t[i] >= 0f)
                {
                    cache0 = t[i] * t[i] * t[i];
                    Vector2 evaluatePoint = start * (-cache0 + 3f * t[i] * t[i] - 3f * t[i] + 1f) + handle1 * (3f * cache0 - 6f * t[i] * t[i] + 3f * t[i]) + handle2 * (-3f * cache0 + 3f * t[i] * t[i]) + end * cache0;
                    extremaPoints.Add(evaluatePoint);
                }
            }

            //on crée la boite de collision avec les points extremes
            float xMin = extremaPoints[0].x, xMax = extremaPoints[0].x, yMin = extremaPoints[0].y, yMax = extremaPoints[0].y;

            for (int i = 1; i < extremaPoints.Count; i++)
            {
                xMin = Mathf.Min(xMin, extremaPoints[i].x);
                xMax = Mathf.Max(xMax, extremaPoints[i].x);
                yMin = Mathf.Min(yMin, extremaPoints[i].y);
                yMax = Mathf.Max(yMax, extremaPoints[i].y);
            }

            return new Hitbox(new Vector2((xMin + xMax) * 0.5f, (yMin + yMax) * 0.5f), new Vector2(xMax - xMin, yMax - yMin));
        }
    }

    #endregion

    /*
    public class LeonardSpline : Spline
    {

    }
    */
}
