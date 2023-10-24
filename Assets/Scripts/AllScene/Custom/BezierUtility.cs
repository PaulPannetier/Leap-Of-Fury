using Collision2D;
using System.Collections.Generic;
using UnityEngine;

public static class BezierUtility
{
    #region Static

    private static float cache0;

    private static Hitbox ComputeBezierHitbox(in Vector2 start, in Vector2 handle1, in Vector2 handle2, in Vector2 end)
    {
        //oSearch t €[0,1] | P'(t).x == 0 || P'(t).y == 0
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
            return start * (-3f * cache0 + 6f * t - 3f) + handle1 * (9f * cache0 - 12f * t + 3f) + handle2 * (-9f * cache0 + 6f * t) + end * (3f * cache0);
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
            if(points.Length != handles.Length)
            {
                throw new System.Exception("points and handles must have the same length");
            }

            this.points = points;
            this.handles = handles;
        }

        public override Vector2 Evaluate(float t)
        {
            t = Mathf.Clamp01(t);
            float interLength = 1f / (points.Length - 1);
            int indexCurve = (t / interLength).Floor();
            float newT = (t - (indexCurve * interLength)) / interLength;

            cache0 = newT * newT * newT;
            return points[indexCurve] * (-cache0 + 3f * newT * newT - 3f * t + 1f) + handles[indexCurve] * (3f * cache0 - 6f * newT * newT + 3f * t) + handles[indexCurve + 1] * (-3f * cache0 + 3f * newT * newT) + points[indexCurve + 1] * cache0;
        }

        public override Vector2[] EvaluateFullCurve(int nbPoints)
        {
            Vector2[] res = new Vector2[nbPoints];

            int nbPointByCurve = ((float)(nbPoints - 1) / (points.Length - 1)).Round();
            float oneOnbPointByCurveM1 = 1f / (nbPointByCurve - 1);//cache
            int indexRes = 0;
            Vector2 P1, P2, P3;
            for (int i = 0; i < points.Length - 2; i++)
            {
                P1 = -3f * points[i] + 3f * handles[i];
                P2 = 3f * points[i] - 6f * handles[i] + 3f * handles[i + 1];
                P3 = -points[i] + 3f * handles[i] - 3f * handles[i + 1] + points[i + 1];

                for (int j = 0; j < nbPointByCurve; j++)
                {
                    float t = j * oneOnbPointByCurveM1;
                    cache0 = t * t;
                    res[indexRes] = points[i] + t * P1 + cache0 * P2 + cache0 * t * P3;
                    indexRes++;
                }
            }

            //last Curve
            int max = nbPoints - indexRes - 1;
            int maxM1 = max - 1;

            P1 = -3f * points[points.Length - 2] + 3f * handles[points.Length - 2];
            P2 = 3f * points[points.Length - 2] - 6f * handles[points.Length - 2] + 3f * handles[points.Length - 1];
            P3 = -points[points.Length - 2] + 3f * handles[points.Length - 2] - 3f * handles[points.Length - 1] + points[points.Length - 1];
            for (int i = 0; i < max; i++)
            {
                float t = i / maxM1;
                //res[indexRes] = EvaluateHermiteCurve(points[points.Length - 2], velocities[velocities.Length - 2], points[points.Length - 1], velocities[velocities.Length - 1], t);
                cache0 = t * t;
                res[indexRes] = points[i] + t * P1 + cache0 * P2 + cache0 * t * P3;
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
                res[i] = ComputeBezierHitbox(points[i], handles[i], points[i + 1], handles[i + 1]);
            }
            return res;
        }

        public override Vector2 Velocity(float t)
        {
            t = Mathf.Clamp01(t);
            float interLength = 1f / (points.Length - 1);
            int indexCurve = (t / interLength).Floor();
            float newT = (t - (indexCurve * interLength)) / interLength;
            cache0 = newT * newT;
            return points[indexCurve] * (-3f * cache0 + 6f * newT - 3f) + handles[indexCurve] * (9f * cache0 - 12f * newT + 3f) +
                handles[indexCurve + 1] * (-9f * cache0 + 6f * newT) + points[indexCurve + 1 ] * (3f * cache0);
        }
    }

    #endregion

    #region HermiteSpline

    public class HermiteSpline : Spline
    {
        private static readonly float oneO3 = 1f / 3f;

        private static Vector2 EvaluateHermiteCurve(in Vector2 start, in Vector2 v0, in Vector2 v1, in Vector2 end, float t)
        {
            cache0 = t * t * t;
            return start * (1f - 3f * t * t + 2f * cache0) + v0 * (t - 2f * t * t + cache0) + end * (3f * t * t - 2f * cache0) + v1 * (-t * t + cache0);
        }

        private Vector2[] points;
        private Vector2[] velocities;

        public HermiteSpline(Vector2[] points)
        {
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
            int indexCurve = (t / interLength).Floor();
            float newT = (t - (indexCurve * interLength)) / interLength;

            return EvaluateHermiteCurve(points[indexCurve], velocities[indexCurve], points[indexCurve + 1], velocities[indexCurve + 1], newT);
        }

        public override Vector2[] EvaluateFullCurve(int nbPoints)
        {
            Vector2[] res = new Vector2[nbPoints];

            int nbPointByCurve = ((float)(nbPoints - 1) / (points.Length - 1)).Round();
            float oneOnbPointByCurveM1 = 1f / (nbPointByCurve - 1);//cache
            int indexRes = 0;
            float t;
            Vector2 P1, P2, c1, c2, c3;
            for (int i = 0; i < points.Length - 2; i++)
            {
                P1 = points[i] + velocities[i];
                P2 = points[i + 1] + velocities[i + 1];

                c1 = P2 - points[i];
                c2 = 2f * points[i] - 2f * P1 + P2 - points[i + 1];
                c3 = P1 - points[i] - P2 + points[i + 1];

                for (int j = 0; j < nbPointByCurve; j++)
                {
                    t = j * oneOnbPointByCurveM1;
                    //res[indexRes] = EvaluateHermiteCurve(points[i], velocities[i], points[i + 1], velocities[i + 1], t);
                    cache0 = t * t;
                    res[indexRes] = P1 + t * c1 + cache0 * c2 + cache0 * t * c3;
                    indexRes++;
                }
            }

            //last Curve
            int max = nbPoints - indexRes - 1;
            float oneOmaxM1 = 1f / (max - 1);
            P1 = points[points.Length - 2] + velocities[velocities.Length - 2];
            P2 = points[points.Length - 1] + velocities[velocities.Length - 1];

            c1 = P2 - points[points.Length - 2];
            c2 = 2f * points[points.Length - 2] - 2f * P1 + P2 - points[points.Length - 1];
            c3 = P1 - points[points.Length - 2] - P2 + points[points.Length - 1];
            for (int i = 0; i < max; i++)
            {
                t = i * oneOmaxM1;
                //res[indexRes] = EvaluateHermiteCurve(points[points.Length - 2], velocities[velocities.Length - 2], points[points.Length - 1], velocities[velocities.Length - 1], t);
                cache0 = t * t;
                res[indexRes] = P1 + t * c1 + cache0 * c2 + cache0 * t * c3;
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
                res[i] = ComputeBezierHitbox(points[i], points[i] + velocities[i] * oneO3, points[i + 1] + velocities[i + 1] * oneO3, points[i + 1]);
            }
            return res;
        }

        public override Vector2 Velocity(float t)
        {
            t = Mathf.Clamp01(t);
            float interLength = 1f / (points.Length - 1);
            int indexCurve = (t / interLength).Floor();
            float newT = (t - (indexCurve * interLength)) / interLength;

            cache0 = newT * newT;
            return points[indexCurve] * (-3f * cache0 + 6f * newT - 3f) + (points[indexCurve] + (velocities[indexCurve] * oneO3)) * (9f * cache0 - 12f * newT + 3f) +
                (points[indexCurve + 1] + (velocities[indexCurve + 1] * oneO3)) * (-9f * cache0 + 6f * newT) + points[indexCurve + 1] * (3f * cache0);
        }
    }

    #endregion

    #region CatmulRom

    public class CatmulRomSpline : Spline
    {
        private static readonly float twoO3 = 2f / 3f;

        private Vector2[] points;
        private Vector2[] handles;

        public CatmulRomSpline(Vector2[] points)
        {
            this.points = points;

            handles = new Vector2[points.Length];

            for (int i = 1; i < points.Length - 1; i++)
            {
                handles[i] = points[i] + points[i + 1] - points[i - 1];
            }

            handles[0] = 2f * points[1] - points[0];
            handles[points.Length - 1] = points[points.Length - 1] + 2f * (points[points.Length - 1] - points[points.Length - 2]);
        }

        public override Vector2 Evaluate(float t)
        {
            t = Mathf.Clamp01(t);
            float interLength = 1f / (points.Length - 1);
            int indexCurve = (t / interLength).Floor();
            float newT = (t - (indexCurve * interLength)) / interLength;

            cache0 = newT * newT;
            return points[indexCurve] * (cache0 - 0.5f * cache0 * newT) + handles[indexCurve] * (1f - 0.5f * newT - 2.5f * cache0 + 1.5f * cache0 * newT) + 
                points[indexCurve + 1] * (0.5f * newT + 2f * cache0 - 1.5f * cache0 * newT) + handles[indexCurve + 1] * (0.5f * (cache0 * newT - cache0));
        }

        public override Vector2[] EvaluateFullCurve(int nbPoints)
        {
            Vector2[] res = new Vector2[nbPoints];

            int nbPointByCurve = ((float)(nbPoints - 1) / (points.Length - 1)).Round();
            float oneOnbPointByCurveM1 = 1f / (nbPointByCurve - 1);//cache
            float t;
            int indexRes = 0;
            Vector2 P0, P1, P2, P3;
            for (int i = 0; i < points.Length - 2; i++)
            {
                P0 = handles[i];
                P1 = 0.5f * (points[i + 1] - handles[i]);
                P2 = points[i] - 2.5f * handles[i] + 2f * points[i + 1] - 0.5f * handles[i + 1];
                P3 = 1.5f * (handles[i] - points[i + 1]) + 0.5f * (points[i + 1] - points[i]);

                for (int j = 0; j < nbPointByCurve; j++)
                {
                    t = j * oneOnbPointByCurveM1;
                    cache0 = t * t;
                    res[indexRes] = P0 + t * P1 + cache0 * P2 + cache0 * t * P3;
                    indexRes++;
                }
            }

            //last Curve
            int max = nbPoints - indexRes - 1;
            int maxM1 = max - 1;

            P0 = handles[points.Length - 2];
            P1 = 0.5f * (points[points.Length - 1] - handles[points.Length - 2]);
            P2 = points[points.Length - 2] - 2.5f * handles[points.Length - 2] + 2f * points[points.Length - 1] - 0.5f * handles[points.Length - 1];
            P3 = 1.5f * (handles[points.Length - 2] - points[points.Length - 1]) + 0.5f * (points[points.Length - 1] - points[points.Length - 2]);

            for (int i = 0; i < max; i++)
            {
                t = i / maxM1;
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
                res[i] = ComputeBezierHitbox(points[i], points[i] + (handles[i] - points[i]) * twoO3, points[i + 1] + (handles[i + 1] - points[i + 1]) * twoO3, points[i + 1]);
            }
            return res;
        }

        public override Vector2 Velocity(float t)
        {
            t = Mathf.Clamp01(t);
            float interLength = 1f / (points.Length - 1);
            int indexCurve = (t / interLength).Floor();
            float newT = (t - (indexCurve * interLength)) / interLength;

            cache0 = newT * newT;
            return points[indexCurve] * (2f * newT - 1.5f * cache0) + handles[indexCurve] * (4.5f * cache0 - 5f * newT - 0.5f) +
                points[indexCurve + 1] * (0.5f + 4f * newT - 4.5f * cache0) + handles[indexCurve + 1] * (1.5f * cache0 - newT);
        }
    }

    #endregion

    #region Cardinal

    public class CardinalSpline : Spline
    {
        private static readonly float oneO3 = 1f / 3f;

        private Vector2[] points;
        private Vector2[] handles;
        float s;

        public CardinalSpline(Vector2[] points, float tension)
        {
            this.points = points;
            this.s = tension;

            handles = new Vector2[points.Length];

            for (int i = 1; i < points.Length - 1; i++)
            {
                handles[i] = points[i] + (points[i + 1] - points[i - 1]) * s;
            }

            handles[0] = points[0] + 2f * s * (points[1] - points[0]);
            handles[points.Length - 1] = points[points.Length - 1] + 2f * s * (points[points.Length - 1] - points[points.Length - 2]);
        }

        public override Vector2 Evaluate(float t)
        {
            t = Mathf.Clamp01(t);
            float interLength = 1f / (points.Length - 1);
            int indexCurve = (t / interLength).Floor();
            float newT = (t - (indexCurve * interLength)) / interLength;

            cache0 = newT * newT;
            return handles[indexCurve] + newT * s * (points[indexCurve + 1] - points[indexCurve]) +
                cache0 * (2f * s * points[indexCurve] + (s - 3f) * handles[indexCurve] + (3f - 2f * s) * points[indexCurve + 1] - s * handles[indexCurve + 1]) +
                cache0 * newT * ((2f - s) * handles[indexCurve] - s * points[indexCurve] + (s - 2f) * points[indexCurve + 1] + s * handles[indexCurve + 1]);
        }

        public override Vector2[] EvaluateFullCurve(int nbPoints)
        {
            Vector2[] res = new Vector2[nbPoints];

            int nbPointByCurve = ((float)(nbPoints - 1) / (points.Length - 1)).Round();
            float oneOnbPointByCurveM1 = 1f / (nbPointByCurve - 1);//cache
            float t;
            int indexRes = 0;
            Vector2 P0, P1, P2, P3;
            for (int i = 0; i < points.Length - 2; i++)
            {
                P0 = handles[i];
                P1 = s * (points[i + 1] - points[i]);
                P2 = 2f * s * points[i] + (s - 3f) * handles[i] + (3f - 2f * s) * points[i + 1] - s * handles[i + 1];
                P3 = (2f - s) * handles[i] + - s * points[i] + (s - 2f) * points[i + 1] + s * handles[i + 1];

                for (int j = 0; j < nbPointByCurve; j++)
                {
                    t = j * oneOnbPointByCurveM1;
                    cache0 = t * t;
                    res[indexRes] = P0 + t * P1 + cache0 * P2 + cache0 * t * P3;
                    indexRes++;
                }
            }

            //last Curve
            int max = nbPoints - indexRes - 1;
            float oneOmaxM1 = 1f / (max - 1);

            P0 = handles[points.Length - 2];
            P1 = 0.5f * (points[points.Length - 1] - handles[points.Length - 2]);
            P2 = points[points.Length - 2] - 2.5f * handles[points.Length - 2] + 2f * points[points.Length - 1] - 0.5f * handles[points.Length - 1];
            P3 = 1.5f * (handles[points.Length - 2] - points[points.Length - 1]) + 0.5f * (points[points.Length - 1] - points[points.Length - 2]);

            for (int i = 0; i < max; i++)
            {
                t = i * oneOmaxM1;
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
            float twoO3s = 2f / (3f * s);
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = ComputeBezierHitbox(points[i], points[i] + (handles[i] - points[i]) * twoO3s, points[i + 1] + (handles[i + 1] - points[i + 1]) * twoO3s, points[i + 1]);
            }
            return res;
        }

        public override Vector2 Velocity(float t)
        {
            t = Mathf.Clamp01(t);
            float interLength = 1f / (points.Length - 1);
            int indexCurve = (t / interLength).Floor();
            float newT = (t - (indexCurve * interLength)) / interLength;

            return s * (points[indexCurve + 1] - points[indexCurve]) +
                2f * newT * (2f * s * points[indexCurve] + (s - 3f) * handles[indexCurve] + (3f - 2f * s) * points[indexCurve + 1] - s * handles[indexCurve + 1]) +
                3f * newT * newT * ((2f - s) * handles[indexCurve] - s * points[indexCurve] + (s - 2f) * points[indexCurve + 1] + s * handles[indexCurve + 1]);
        }
    }

    #endregion

    #region BSpline

    public class BSpline : Spline
    {
        public override Vector2 Evaluate(float t)
        {
            throw new System.NotImplementedException();
        }

        public override Vector2[] EvaluateFullCurve(int nbPoints)
        {
            throw new System.NotImplementedException();
        }

        public override Hitbox Hitbox()
        {
            throw new System.NotImplementedException();
        }

        public override Hitbox[] Hitboxes()
        {
            throw new System.NotImplementedException();
        }

        public override Vector2 Velocity(float t)
        {
            throw new System.NotImplementedException();
        }
    }

    #endregion

    /*
    public class LeonardSpline : Spline
    {

    }
    */
}
