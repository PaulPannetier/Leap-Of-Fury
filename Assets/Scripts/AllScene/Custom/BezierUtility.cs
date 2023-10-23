using Collision2D;
using System.Collections.Generic;
using UnityEngine;

public static class BezierUtility
{
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

    public abstract class Spline
    {
        public abstract Vector2 Evaluate(float t);
        public abstract Vector2 Velocity(float t);
        public virtual Vector2 Normal(float t) => Velocity(t).NormalVector();
        public abstract Vector2[] EvaluateFullCurve(int nbPoints);
        public abstract Hitbox Hitbox(); 
        public abstract Hitbox[] Hitboxes();
    }

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
            int nbPointM1 = nbPoints - 1;
            for (int i = 0; i < nbPoints; i++)
            {
                t = (float) i / nbPointM1;
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
            int nbPointByCurveM1 = nbPointByCurve - 1;//cache
            int indexRes = 0;
            Vector2 P1, P2, P3;
            for (int i = 0; i < points.Length - 2; i++)
            {
                P1 = -3f * points[i] + 3f * handles[i];
                P2 = 3f * points[i] - 6f * handles[i] + 3f * handles[i + 1];
                P3 = -points[i] + 3f * handles[i] - 3f * handles[i + 1] + points[i + 1];

                for (int j = 0; j < nbPointByCurve; j++)
                {
                    float t = j / nbPointByCurveM1;
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
            velocities[0] = 2f * (points[points.Length - 1] - points[points.Length - 2]);
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
            int nbPointByCurveM1 = nbPointByCurve - 1;//cache
            int indexRes = 0;
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
                    float t = j / nbPointByCurveM1;
                    //res[indexRes] = EvaluateHermiteCurve(points[i], velocities[i], points[i + 1], velocities[i + 1], t);
                    cache0 = t * t;
                    res[indexRes] = P1 + t * c1 + cache0 * c2 + cache0 * t * c3;
                    indexRes++;
                }
            }

            //last Curve
            int max = nbPoints - indexRes - 1;
            int maxM1 = max - 1;
            P1 = points[points.Length - 2] + velocities[velocities.Length - 2];
            P2 = points[points.Length - 1] + velocities[velocities.Length - 1];

            c1 = P2 - points[points.Length - 2];
            c2 = 2f * points[points.Length - 2] - 2f * P1 + P2 - points[points.Length - 1];
            c3 = P1 - points[points.Length - 2] - P2 + points[points.Length - 1];
            for (int i = 0; i < max; i++)
            {
                float t = i / maxM1;
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

    public class CatmulRomSpline : Spline
    {

    }

    public class CardinalSpline : Spline
    {

    }

    public class BSpline : Spline
    {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="controlPoints"></param>
    /// <param name="handlePoints"></param>
    /// <returns>La courbe de bézier passant par les points controlPoints et la courbe sera influencé par les handlePoints</returns>
    public static Vector2[] CubicBezierSpline(in Vector2[] controlPoints, in Vector2[] handlePoints, int pointsPerCurve)
    {
        //verif
        if (handlePoints.Length != 2 * controlPoints.Length - 2)
        {
            Debug.LogError("wrong parameter for calculate bezier spline : " + handlePoints.Length + " != " + (2 * controlPoints.Length - 2));
            return null;
        }

        Vector2[] res = new Vector2[(controlPoints.Length - 1) * pointsPerCurve - controlPoints.Length + 2];
        int resIndex = 0;
        float step = 1f / (pointsPerCurve - 1);
        float t;
        Vector2 start, end, handle1, handle2;

        for (int i = 0; i < controlPoints.Length - 1; i++)
        {
            start = controlPoints[i];
            end = controlPoints[i + 1];
            handle1 = handlePoints[2 * i];
            handle2 = handlePoints[2 * i + 1];
            res[resIndex] = start;
            resIndex++;
            for (int j = 1; j < pointsPerCurve - 1; j++)
            {
                t = step * j;
                res[resIndex] = CubicBezierCurve(start, handle1, handle2, end, t);
                resIndex++;
            }
        }
        res[resIndex] = controlPoints[controlPoints.Length - 1];
        return res;
    }

    public static Hitbox[] CubicBezierSplineHitboxes(in Vector2[] controlPoints, in Vector2[] handlePoints)
    {
        Hitbox[] res = new Hitbox[controlPoints.Length - 1];
        for (int i = 0; i < res.Length; i++)
        {
            res[i] = CubicBezierCurveHitbox(controlPoints[i], handlePoints[2 * i], handlePoints[2 * i + 1], controlPoints[i + 1]);
        }
        return res;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="controlPoints"></param>
    /// <param name="handlePoints"></param>
    /// <returns>La courbe de Catmull-Rom passant par les points controlPoints</returns>
    public static Vector2[] CatmullRomSpline(in Vector2[] controlPoints, int pointsPerCurve)
    {
        Vector2[] newControlPoints = new Vector2[controlPoints.Length + 2];
        newControlPoints[0] = 2f * controlPoints[0] - controlPoints[1];
        newControlPoints[newControlPoints.Length - 1] = 2f * controlPoints[controlPoints.Length - 1] - controlPoints[controlPoints.Length - 2];
        for (int i = 1; i < newControlPoints.Length - 1; i++)
        {
            newControlPoints[i] = controlPoints[i - 1];
        }

        Vector2[] handles = new Vector2[controlPoints.Length];
        for (int i = 0; i < handles.Length; i++)
        {
            handles[i] = (newControlPoints[i + 2] - newControlPoints[i]) * 0.5f;
        }

        Vector2[] res = new Vector2[(controlPoints.Length - 1) * pointsPerCurve - controlPoints.Length + 2];
        int resIndex = 0;
        float step = 1f / (pointsPerCurve - 1);
        float t;
        Vector2 start, end, v0, v1;

        for (int i = 0; i < controlPoints.Length - 1; i++)
        {
            start = controlPoints[i];
            end = controlPoints[i + 1];
            v0 = handles[i];
            v1 = handles[i + 1];
            res[resIndex] = start;
            resIndex++;
            for (int j = 1; j < pointsPerCurve - 1; j++)
            {
                t = step * j;
                res[resIndex] = HermiteCurve(start, v0, v1, end, t);
                resIndex++;
            }
        }
        res[resIndex] = controlPoints[controlPoints.Length - 1];
        return res;
    }

    public static Hitbox[] CatmullRomSplineHitboxes(in Vector2[] controlPoints)
    {
        Vector2[] newControlPoints = new Vector2[controlPoints.Length + 2];
        newControlPoints[0] = 2f * controlPoints[0] - controlPoints[1];
        newControlPoints[newControlPoints.Length - 1] = 2f * controlPoints[controlPoints.Length - 1] - controlPoints[controlPoints.Length - 2];
        for (int i = 1; i < newControlPoints.Length - 1; i++)
        {
            newControlPoints[i] = controlPoints[i - 1];
        }

        Vector2[] handles = new Vector2[controlPoints.Length];
        for (int i = 0; i < handles.Length; i++)
        {
            handles[i] = (newControlPoints[i + 2] - newControlPoints[i]) * 0.5f;
        }

        Hitbox[] res = new Hitbox[controlPoints.Length - 1];
        for (int i = 0; i < res.Length; i++)
        {
            res[i] = HermiteCurveHitbox(controlPoints[i], handles[i], handles[i + 1], controlPoints[i + 1]);
        }
        return res;
    }
}
