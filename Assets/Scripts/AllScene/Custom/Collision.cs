using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

#region Line/Droite

public class Line
{
    public Vector2 A, B;
    public Line(in Vector2 A, in Vector2 B)
    {
        this.A = A;
        this.B = B;
    }

    public Line(in Vector2 start, in float angle, in float lenght)
    {
        A = start;
        B = new Vector2(A.x + lenght * Mathf.Cos(angle), A.y + lenght * Mathf.Sin(angle));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>true if point € [A,B], false otherwise</returns>
    public static bool Contain(in Vector2 A, in Vector2 B, in Vector2 point)
    {
        if (Mathf.Abs(A.x - B.x) <= CustomCollider.accuracy)
        {
            return Mathf.Abs(((A.x + B.x) * 0.5f) - point.x) <= CustomCollider.accuracy && Mathf.Min(A.y, B.y) <= point.y && Mathf.Max(A.y, B.y) >= point.y;
        }
        if (Mathf.Min(A.x, B.x) > point.x || Mathf.Max(A.x, B.x) < point.x || Mathf.Min(A.y, B.y) > point.y || Mathf.Max(A.y, B.y) < point.y)
        {
            return false;
        }
        //equetion de la droite (A,B)
        float a = (B.y - A.y) / (B.x - A.x);
        float b = A.y - a * A.x;
        return Mathf.Abs((a * point.x + b) - point.y) < CustomCollider.accuracy;
    }

    //vérif ok
    public static float Distance(in Vector2 A, in Vector2 B, in Vector2 point)
    {
        float r = (((point.x - A.x) * (B.x - A.x)) + ((point.y - A.y) * (B.y - A.y))) / A.SqrDistance(B);
        Vector2 P = A + r * (B - A);
        return (0f <= r && r <= 1f) ? P.Distance(point) : (r < 0f ? A.Distance(point) : B.Distance(point));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>A vector normal of the line</returns>
    public static Vector2 Normal(in Vector2 A, in Vector2 B)
    {
        if (Mathf.Abs(A.x - B.x) <= CustomCollider.accuracy)
        {
            return Vector2.right;
        }
        //equetion de la droite (A,B)
        return new Vector2((B.y - A.y) / (B.x - A.x), -1f).normalized;
    }
}

public class Droite
{
    public Vector2 A, B;
    public Droite(in Vector2 A, in Vector2 B)
    {
        this.A = A;
        this.B = B;
    }

    public static void GizmosDraw(Droite d) => GizmosDraw(d.A, d.B);
    public static void GizmosDraw(in Vector2 A, in Vector2 B)
    {
        float a = Useful.AngleHori(A, B);
        Vector2 dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a)).normalized;
        Vector2 s = (A + B) * 0.5f;
        Gizmos.DrawLine(s + dir * 100000f, s + dir * -100000f);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="M"></param>
    /// <param name="D"></param>
    /// <param name="h"></param>
    /// <returns>Le symétrique du point M par rapport à la droite D</returns>
    public static Vector2 Symetric(in Vector2 M, in Droite D)
    {
        if (Mathf.Abs(D.A.x - D.B.x) < CustomCollider.accuracy)
        {
            return new Vector2(M.x >= (D.A.x - D.B.x) * 0.5f ? M.x - 2f * Distance(D.A, D.B, M) : M.x + 2f * Distance(D.A, D.B, M), M.y);
        }
        return  2f * OrthogonalProjection(M, D) - M;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="M"></param>
    /// <param name="D"></param>
    /// <returns>le projeté orthogonal du point M sur la droite D</returns>
    public static Vector2 OrthogonalProjection(in Vector2 M, in Droite D)
    {
        if (Mathf.Abs(D.A.x - D.B.x) < CustomCollider.accuracy)
        {
            return new Vector2((D.A.x - D.B.x) * 0.5f, M.y);
        }

        float r = (((M.x - D.A.x) * (D.B.x - D.A.x)) + ((M.y - D.A.y) * (D.B.y - D.A.y))) / D.A.SqrDistance(D.B);
        Vector2 P = D.A + r * (D.B - D.A);
        return P;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>if point € (A,B)</returns>
    public static bool Contain(in Vector2 A, in Vector2 B, in Vector2 point)
    {
        if (Mathf.Abs(A.x - B.x) <= CustomCollider.accuracy)
        {
            return Mathf.Abs(((A.x + B.x) * 0.5f) - point.x) <= CustomCollider.accuracy && Mathf.Min(A.y, B.y) <= point.y && Mathf.Max(A.y, B.y) >= point.y;
        }
        //equetion de la droite (A,B)
        float a = (B.y - A.y) / (B.x - A.x);
        float b = A.y - a * A.x;
        return Mathf.Abs((a * point.x + b) - point.y) < CustomCollider.accuracy;
    }

    /// <summary>
    /// Vérif ok
    /// </summary>
    /// <returns> min(Dist(point, P)), P € (A,B)</returns>
    public static float Distance(in Vector2 A, in Vector2 B, in Vector2 point)
    {
        if (Mathf.Abs(A.x - B.x) <= CustomCollider.accuracy)
        {
            return Mathf.Abs((A.x + B.x) * 0.5f - point.x);
        }
        float a = (B.y - A.y) / (B.x - A.x);
        float b = A.y - a * A.x;
        return Mathf.Abs(a * point.x - point.y + b) / Mathf.Sqrt(a * a + 1f);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>A vector normal of the droite</returns>
    public static Vector2 Normal(in Vector2 A, in Vector2 B)
    {
        if (Mathf.Abs(A.x - B.x) <= CustomCollider.accuracy)
        {
            return Vector2.right;
        }
        return new Vector2((B.y - A.y) / (B.x - A.x), -1f).normalized;
    }
}

#endregion

public abstract class CustomCollider
{
    public const float accuracy = 1e-5f;

    #region Collision Functions

    private static readonly List<Vector2> cache = new List<Vector2>(), cache2 = new List<Vector2>(), cache3 = new List<Vector2>();

    #region General Collissions

    #region Dico

    private static readonly Dictionary<Type, Dictionary<Type, Func<CustomCollider, CustomCollider, bool>>> collisionFunc1 = new Dictionary<Type, Dictionary<Type, Func<CustomCollider, CustomCollider, bool>>>()
    {
        {
            typeof(Circle),
            new Dictionary<Type, Func<CustomCollider, CustomCollider, bool>>()
            {
                { typeof(Circle),  (CustomCollider c1, CustomCollider c2) => CollideCircles((Circle)c1, (Circle)c2) },
                { typeof(Polygone),  (CustomCollider c1, CustomCollider c2) => CollideCirclePolygone((Circle)c1, (Polygone)c2) },
                { typeof(Hitbox),  (CustomCollider c1, CustomCollider c2) => CollideCircleHitbox((Circle)c1, (Hitbox)c2) },
                { typeof(Capsule),  (CustomCollider c1, CustomCollider c2) => CollideCircleCapsule((Circle)c1, (Capsule)c2) },
                { typeof(Ellipse),  (CustomCollider c1, CustomCollider c2) => CollideCircleEllipse((Circle)c1, (Ellipse)c2) }
            }
        },
        {
            typeof(Polygone),
            new Dictionary<Type, Func<CustomCollider, CustomCollider, bool>>()
            {
                { typeof(Circle),  (CustomCollider c1, CustomCollider c2) => CollideCirclePolygone((Circle)c2, (Polygone)c1) },
                { typeof(Polygone),  (CustomCollider c1, CustomCollider c2) => CollidePolygones((Polygone)c1, (Polygone)c2) },
                { typeof(Hitbox),  (CustomCollider c1, CustomCollider c2) => CollidePolygoneHitbox((Polygone)c1, (Hitbox)c2) },
                { typeof(Capsule),  (CustomCollider c1, CustomCollider c2) => CollidePolygoneCapsule((Polygone)c1, (Capsule)c2) },
                { typeof(Ellipse),  (CustomCollider c1, CustomCollider c2) => CollidePolygoneEllipse((Polygone)c1, (Ellipse)c2) }
            }
        },
        {
            typeof(Hitbox),
            new Dictionary<Type, Func<CustomCollider, CustomCollider, bool>>()
            {
                { typeof(Circle),  (CustomCollider c1, CustomCollider c2) => CollideCircleHitbox((Circle)c2, (Hitbox)c1) },
                { typeof(Polygone),  (CustomCollider c1, CustomCollider c2) => CollidePolygoneHitbox((Polygone)c2, (Hitbox)c1) },
                { typeof(Hitbox),  (CustomCollider c1, CustomCollider c2) => CollideHitboxs((Hitbox)c1, (Hitbox)c2) },
                { typeof(Capsule),  (CustomCollider c1, CustomCollider c2) => CollideHitboxCapsule((Hitbox)c1, (Capsule)c2) },
                { typeof(Ellipse),  (CustomCollider c1, CustomCollider c2) => CollideHitboxEllipse((Hitbox)c1, (Ellipse)c2) }
            }
        },
        {
            typeof(Capsule),
            new Dictionary<Type, Func<CustomCollider, CustomCollider, bool>>()
            {
                { typeof(Circle),  (CustomCollider c1, CustomCollider c2) => CollideCircleCapsule((Circle)c2, (Capsule)c1) },
                { typeof(Polygone),  (CustomCollider c1, CustomCollider c2) => CollidePolygoneCapsule((Polygone)c2, (Capsule)c1) },
                { typeof(Hitbox),  (CustomCollider c1, CustomCollider c2) => CollideHitboxCapsule((Hitbox)c2, (Capsule)c1) },
                { typeof(Capsule),  (CustomCollider c1, CustomCollider c2) => CollideCapsules((Capsule)c1, (Capsule)c2) },
                { typeof(Ellipse),  (CustomCollider c1, CustomCollider c2) => CollideCapsuleEllipse((Capsule)c1, (Ellipse)c2) }
            }
        },
        {
            typeof(Ellipse),
            new Dictionary<Type, Func<CustomCollider, CustomCollider, bool>>()
            {
                { typeof(Circle),  (CustomCollider c1, CustomCollider c2) => CollideCircleEllipse((Circle)c2, (Ellipse)c1) },
                { typeof(Polygone),  (CustomCollider c1, CustomCollider c2) => CollidePolygoneEllipse((Polygone)c2, (Ellipse)c1) },
                { typeof(Hitbox),  (CustomCollider c1, CustomCollider c2) => CollideHitboxEllipse((Hitbox)c2, (Ellipse)c1) },
                { typeof(Capsule),  (CustomCollider c1, CustomCollider c2) => CollideCapsuleEllipse((Capsule)c1, (Ellipse)c2) },
                { typeof(Ellipse),  (CustomCollider c1, CustomCollider c2) => CollideEllipses((Ellipse)c1, (Ellipse)c2) }
            }
        }
    };

    private static readonly Dictionary<Type, Dictionary<Type, Func<CustomCollider, CustomCollider, (bool, Vector2)>>> collisionFunc2 = new Dictionary<Type, Dictionary<Type, Func<CustomCollider, CustomCollider, (bool, Vector2)>>>()
    {
        {
            typeof(Circle),
            new Dictionary<Type, Func<CustomCollider, CustomCollider, (bool, Vector2)>>()
            {
                { typeof(Circle),  (CustomCollider c1, CustomCollider c2) => (CollideCircles((Circle)c1, (Circle)c2, out Vector2 v), v) },
                { typeof(Polygone),  (CustomCollider c1, CustomCollider c2) => (CollideCirclePolygone((Circle)c1, (Polygone)c2, out Vector2 v),v) },
                { typeof(Hitbox),  (CustomCollider c1, CustomCollider c2) => (CollideCircleHitbox((Circle)c1, (Hitbox)c2, out Vector2 v),v) },
                { typeof(Capsule),  (CustomCollider c1, CustomCollider c2) => (CollideCircleCapsule((Circle)c1, (Capsule)c2, out Vector2 v),v) },
                { typeof(Ellipse),  (CustomCollider c1, CustomCollider c2) => (CollideCircleEllipse((Circle)c1, (Ellipse)c2, out Vector2 v),v) }
            }
        },
        {
            typeof(Polygone),
            new Dictionary<Type, Func<CustomCollider, CustomCollider, (bool, Vector2)>>()
            {
                { typeof(Circle),  (CustomCollider c1, CustomCollider c2) => (CollideCirclePolygone((Circle)c2, (Polygone)c1, out Vector2 v),v) },
                { typeof(Polygone),  (CustomCollider c1, CustomCollider c2) => (CollidePolygones((Polygone)c1, (Polygone)c2, out Vector2 v),v) },
                { typeof(Hitbox),  (CustomCollider c1, CustomCollider c2) => (CollidePolygoneHitbox((Polygone)c1, (Hitbox)c2, out Vector2 v),v) },
                { typeof(Capsule),  (CustomCollider c1, CustomCollider c2) => (CollidePolygoneCapsule((Polygone)c1, (Capsule)c2, out Vector2 v),v) },
                { typeof(Ellipse),  (CustomCollider c1, CustomCollider c2) => (CollidePolygoneEllipse((Polygone)c1, (Ellipse)c2, out Vector2 v),v) }
            }
        },
        {
            typeof(Hitbox),
            new Dictionary<Type, Func<CustomCollider, CustomCollider, (bool, Vector2)>>()
            {
                { typeof(Circle),  (CustomCollider c1, CustomCollider c2) => (CollideCircleHitbox((Circle)c2, (Hitbox)c1, out Vector2 v),v) },
                { typeof(Polygone),  (CustomCollider c1, CustomCollider c2) => (CollidePolygoneHitbox((Polygone)c2, (Hitbox)c1, out Vector2 v),v) },
                { typeof(Hitbox),  (CustomCollider c1, CustomCollider c2) => (CollideHitboxs((Hitbox)c1, (Hitbox)c2, out Vector2 v),v) },
                { typeof(Capsule),  (CustomCollider c1, CustomCollider c2) => (CollideHitboxCapsule((Hitbox)c1, (Capsule)c2, out Vector2 v),v) },
                { typeof(Ellipse),  (CustomCollider c1, CustomCollider c2) => (CollideHitboxEllipse((Hitbox)c1, (Ellipse)c2, out Vector2 v),v) }
            }
        },
        {
            typeof(Capsule),
            new Dictionary<Type, Func<CustomCollider, CustomCollider, (bool, Vector2)>>()
            {
                { typeof(Circle),  (CustomCollider c1, CustomCollider c2) => (CollideCircleCapsule((Circle)c2, (Capsule)c1, out Vector2 v),v) },
                { typeof(Polygone),  (CustomCollider c1, CustomCollider c2) => (CollidePolygoneCapsule((Polygone)c2, (Capsule)c1, out Vector2 v),v) },
                { typeof(Hitbox),  (CustomCollider c1, CustomCollider c2) => (CollideHitboxCapsule((Hitbox)c2, (Capsule)c1, out Vector2 v),v) },
                { typeof(Capsule),  (CustomCollider c1, CustomCollider c2) => (CollideCapsules((Capsule)c1, (Capsule)c2, out Vector2 v),v) },
                { typeof(Ellipse),  (CustomCollider c1, CustomCollider c2) => (CollideCapsuleEllipse((Capsule)c1, (Ellipse)c2, out Vector2 v),v) }
            }
        },
        {
            typeof(Ellipse),
            new Dictionary<Type, Func<CustomCollider, CustomCollider, (bool, Vector2)>>()
            {
                { typeof(Circle),  (CustomCollider c1, CustomCollider c2) => (CollideCircleEllipse((Circle)c2, (Ellipse)c1, out Vector2 v),v) },
                { typeof(Polygone),  (CustomCollider c1, CustomCollider c2) => (CollidePolygoneEllipse((Polygone)c2, (Ellipse)c1, out Vector2 v),v) },
                { typeof(Hitbox),  (CustomCollider c1, CustomCollider c2) => (CollideHitboxEllipse((Hitbox)c2, (Ellipse)c1, out Vector2 v),v) },
                { typeof(Capsule),  (CustomCollider c1, CustomCollider c2) => (CollideCapsuleEllipse((Capsule)c1, (Ellipse)c2, out Vector2 v),v) },
                { typeof(Ellipse),  (CustomCollider c1, CustomCollider c2) => (CollideEllipses((Ellipse)c1, (Ellipse)c2, out Vector2 v),v) }
            }
        }
    };

    private static readonly Dictionary<Type, Dictionary<Type, Func<CustomCollider, CustomCollider, (bool, Vector2, Vector2, Vector2)>>> collisionFunc3 = new Dictionary<Type, Dictionary<Type, Func<CustomCollider, CustomCollider, (bool, Vector2, Vector2, Vector2)>>>()
    {
        {
            typeof(Circle),
            new Dictionary<Type, Func<CustomCollider, CustomCollider, (bool, Vector2, Vector2, Vector2)>>()
            {
                { typeof(Circle),  (CustomCollider c1, CustomCollider c2) => (CollideCircles((Circle)c1, (Circle)c2, out Vector2 v, out Vector2 v2, out Vector2 v3), v, v2, v3) },
                { typeof(Polygone),  (CustomCollider c1, CustomCollider c2) => (CollideCirclePolygone((Circle)c1, (Polygone)c2, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) },
                { typeof(Hitbox),  (CustomCollider c1, CustomCollider c2) => (CollideCircleHitbox((Circle)c1, (Hitbox)c2, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) },
                { typeof(Capsule),  (CustomCollider c1, CustomCollider c2) => (CollideCircleCapsule((Circle)c1, (Capsule)c2, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) },
                { typeof(Ellipse),  (CustomCollider c1, CustomCollider c2) => (CollideCircleEllipse((Circle)c1, (Ellipse)c2, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) }
            }
        },
        {
            typeof(Polygone),
            new Dictionary<Type, Func<CustomCollider, CustomCollider, (bool, Vector2, Vector2, Vector2)>>()
            {
                { typeof(Circle),  (CustomCollider c1, CustomCollider c2) => (CollideCirclePolygone((Circle)c2, (Polygone)c1, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) },
                { typeof(Polygone),  (CustomCollider c1, CustomCollider c2) => (CollidePolygones((Polygone)c1, (Polygone)c2, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) },
                { typeof(Hitbox),  (CustomCollider c1, CustomCollider c2) => (CollidePolygoneHitbox((Polygone)c1, (Hitbox)c2, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) },
                { typeof(Capsule),  (CustomCollider c1, CustomCollider c2) => (CollidePolygoneCapsule((Polygone)c1, (Capsule)c2, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) },
                { typeof(Ellipse),  (CustomCollider c1, CustomCollider c2) => (CollidePolygoneEllipse((Polygone)c1, (Ellipse)c2, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) }
            }
        },
        {
            typeof(Hitbox),
            new Dictionary<Type, Func<CustomCollider, CustomCollider, (bool, Vector2, Vector2, Vector2)>>()
            {
                { typeof(Circle),  (CustomCollider c1, CustomCollider c2) => (CollideCircleHitbox((Circle)c2, (Hitbox)c1, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) },
                { typeof(Polygone),  (CustomCollider c1, CustomCollider c2) => (CollidePolygoneHitbox((Polygone)c2, (Hitbox)c1, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) },
                { typeof(Hitbox),  (CustomCollider c1, CustomCollider c2) => (CollideHitboxs((Hitbox)c1, (Hitbox)c2, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) },
                { typeof(Capsule),  (CustomCollider c1, CustomCollider c2) => (CollideHitboxCapsule((Hitbox)c1, (Capsule)c2, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) },
                { typeof(Ellipse),  (CustomCollider c1, CustomCollider c2) => (CollideHitboxEllipse((Hitbox)c1, (Ellipse)c2, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) }
            }
        },
        {
            typeof(Capsule),
            new Dictionary<Type, Func<CustomCollider, CustomCollider, (bool, Vector2, Vector2, Vector2)>>()
            {
                { typeof(Circle),  (CustomCollider c1, CustomCollider c2) => (CollideCircleCapsule((Circle)c2, (Capsule)c1, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) },
                { typeof(Polygone),  (CustomCollider c1, CustomCollider c2) => (CollidePolygoneCapsule((Polygone)c2, (Capsule)c1, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) },
                { typeof(Hitbox),  (CustomCollider c1, CustomCollider c2) => (CollideHitboxCapsule((Hitbox)c2, (Capsule)c1, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) },
                { typeof(Capsule),  (CustomCollider c1, CustomCollider c2) => (CollideCapsules((Capsule)c1, (Capsule)c2, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) },
                { typeof(Ellipse),  (CustomCollider c1, CustomCollider c2) => (CollideCapsuleEllipse((Capsule)c1, (Ellipse)c2, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) }
            }
        },
        {
            typeof(Ellipse),
            new Dictionary<Type, Func<CustomCollider, CustomCollider, (bool, Vector2, Vector2, Vector2)>>()
            {
                { typeof(Circle),  (CustomCollider c1, CustomCollider c2) => (CollideCircleEllipse((Circle)c2, (Ellipse)c1, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) },
                { typeof(Polygone),  (CustomCollider c1, CustomCollider c2) => (CollidePolygoneEllipse((Polygone)c2, (Ellipse)c1, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) },
                { typeof(Hitbox),  (CustomCollider c1, CustomCollider c2) => (CollideHitboxEllipse((Hitbox)c2, (Ellipse)c1, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) },
                { typeof(Capsule),  (CustomCollider c1, CustomCollider c2) => (CollideCapsuleEllipse((Capsule)c1, (Ellipse)c2, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) },
                { typeof(Ellipse),  (CustomCollider c1, CustomCollider c2) => (CollideEllipses((Ellipse)c1, (Ellipse)c2, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) }
            }
        }
    };

    private static readonly Dictionary<Type, Func<CustomCollider, Vector2, Vector2, bool>> collisionLine1 = new Dictionary<Type, Func<CustomCollider, Vector2, Vector2, bool>>()
    {
        { typeof(Circle), (CustomCollider c, Vector2 A, Vector2 B) => CollideCircleLine((Circle)c, A, B) },
        { typeof(Polygone), (CustomCollider c, Vector2 A, Vector2 B) => CollidePolygoneLine((Polygone)c, A, B) },
        { typeof(Hitbox), (CustomCollider c, Vector2 A, Vector2 B) => CollideHitboxLine((Hitbox)c, A, B) },
        { typeof(Capsule), (CustomCollider c, Vector2 A, Vector2 B) => CollideCapsuleLine((Capsule)c, A, B) },
        { typeof(Ellipse), (CustomCollider c, Vector2 A, Vector2 B) => CollideEllipseLine((Ellipse)c, A, B) },
    };

    private static readonly Dictionary<Type, Func<CustomCollider, Vector2, Vector2, (bool, Vector2)>> collisionLine2 = new Dictionary<Type, Func<CustomCollider, Vector2, Vector2, (bool, Vector2)>>()
    {
        { typeof(Circle), (CustomCollider c, Vector2 A, Vector2 B) => (CollideCircleLine((Circle)c, A, B, out Vector2 v), v) },
        { typeof(Polygone), (CustomCollider c, Vector2 A, Vector2 B) => (CollidePolygoneLine((Polygone)c, A, B, out Vector2 v), v) },
        { typeof(Hitbox), (CustomCollider c, Vector2 A, Vector2 B) => (CollideHitboxLine((Hitbox)c, A, B, out Vector2 v), v) },
        { typeof(Capsule), (CustomCollider c, Vector2 A, Vector2 B) => (CollideCapsuleLine((Capsule)c, A, B, out Vector2 v), v) },
        { typeof(Ellipse), (CustomCollider c, Vector2 A, Vector2 B) => (CollideEllipseLine((Ellipse)c, A, B, out Vector2 v), v) },
    };

    private static readonly Dictionary<Type, Func<CustomCollider, Vector2, Vector2, (bool, Vector2, Vector2)>> collisionLine3 = new Dictionary<Type, Func<CustomCollider, Vector2, Vector2, (bool, Vector2, Vector2)>>()
    {
        { typeof(Circle), (CustomCollider c, Vector2 A, Vector2 B) => (CollideCircleLine((Circle)c, A, B, out Vector2 v, out Vector2 v2), v, v2) },
        { typeof(Polygone), (CustomCollider c, Vector2 A, Vector2 B) => (CollidePolygoneLine((Polygone)c, A, B, out Vector2 v, out Vector2 v2), v, v2) },
        { typeof(Hitbox), (CustomCollider c, Vector2 A, Vector2 B) => (CollideHitboxLine((Hitbox)c, A, B, out Vector2 v, out Vector2 v2), v, v2) },
        { typeof(Capsule), (CustomCollider c, Vector2 A, Vector2 B) => (CollideCapsuleLine((Capsule)c, A, B, out Vector2 v, out Vector2 v2), v, v2) },
        { typeof(Ellipse), (CustomCollider c, Vector2 A, Vector2 B) => (CollideEllipseLine((Ellipse)c, A, B, out Vector2 v, out Vector2 v2), v, v2) },
    };

    private static readonly Dictionary<Type, Func<CustomCollider, Vector2, Vector2, bool>> collisionDroite1 = new Dictionary<Type, Func<CustomCollider, Vector2, Vector2, bool>>()
    {
        { typeof(Circle), (CustomCollider c, Vector2 A, Vector2 B) => CollideCircleDroite((Circle)c, A, B) },
        { typeof(Polygone), (CustomCollider c, Vector2 A, Vector2 B) => CollidePolygoneDroite((Polygone)c, A, B) },
        { typeof(Hitbox), (CustomCollider c, Vector2 A, Vector2 B) => CollideHitboxDroite((Hitbox)c, A, B) },
        { typeof(Capsule), (CustomCollider c, Vector2 A, Vector2 B) => CollideCapsuleDroite((Capsule)c, A, B) },
        { typeof(Ellipse), (CustomCollider c, Vector2 A, Vector2 B) => CollideEllipseDroite((Ellipse)c, A, B) },
    };

    private static readonly Dictionary<Type, Func<CustomCollider, Vector2, Vector2, (bool, Vector2)>> collisionDroite2 = new Dictionary<Type, Func<CustomCollider, Vector2, Vector2, (bool, Vector2)>>()
    {
        { typeof(Circle), (CustomCollider c, Vector2 A, Vector2 B) => (CollideCircleDroite((Circle)c, A, B, out Vector2 v), v) },
        { typeof(Polygone), (CustomCollider c, Vector2 A, Vector2 B) => (CollidePolygoneDroite((Polygone)c, A, B, out Vector2 v), v) },
        { typeof(Hitbox), (CustomCollider c, Vector2 A, Vector2 B) => (CollideHitboxDroite((Hitbox)c, A, B, out Vector2 v), v) },
        { typeof(Capsule), (CustomCollider c, Vector2 A, Vector2 B) => (CollideCapsuleDroite((Capsule)c, A, B, out Vector2 v), v) },
        { typeof(Ellipse), (CustomCollider c, Vector2 A, Vector2 B) => (CollideEllipseDroite((Ellipse)c, A, B, out Vector2 v), v) },
    };

    private static readonly Dictionary<Type, Func<CustomCollider, Vector2, Vector2, (bool, Vector2, Vector2)>> collisionDroite3 = new Dictionary<Type, Func<CustomCollider, Vector2, Vector2, (bool, Vector2, Vector2)>>()
    {
        { typeof(Circle), (CustomCollider c, Vector2 A, Vector2 B) => (CollideCircleDroite((Circle)c, A, B, out Vector2 v, out Vector2 v2), v, v2) },
        { typeof(Polygone), (CustomCollider c, Vector2 A, Vector2 B) => (CollidePolygoneDroite((Polygone)c, A, B, out Vector2 v, out Vector2 v2), v, v2) },
        { typeof(Hitbox), (CustomCollider c, Vector2 A, Vector2 B) => (CollideHitboxDroite((Hitbox)c, A, B, out Vector2 v, out Vector2 v2), v, v2) },
        { typeof(Capsule), (CustomCollider c, Vector2 A, Vector2 B) => (CollideCapsuleDroite((Capsule)c, A, B, out Vector2 v, out Vector2 v2), v, v2) },
        { typeof(Ellipse), (CustomCollider c, Vector2 A, Vector2 B) => (CollideEllipseDroite((Ellipse)c, A, B, out Vector2 v, out Vector2 v2), v, v2) },
    };

    #endregion

    private static bool FirstTestBeforeCollision(CustomCollider c1, CustomCollider c2) => CollideCircles(c1.inclusiveCircle, c2.inclusiveCircle);

    /// <returns>true if the both collider collide together, false otherwise</returns>
    public static bool Collide(CustomCollider c1, CustomCollider c2) => FirstTestBeforeCollision(c1, c2) && collisionFunc1[c1.GetType()][c2.GetType()](c1, c2);
    /// <param name="collisionPoint">The average point of collision of the two collider if true is return, (0,0) oterwise</param>
    /// <returns>true if the both collider collide together, false otherwise</returns>
    public static bool Collide(CustomCollider c1, CustomCollider c2, out Vector2 collisionPoint)
    {
        if (!FirstTestBeforeCollision(c1, c2))
        {
            collisionPoint = Vector2.zero;
            return false;
        }
        bool b;
        (b, collisionPoint) = collisionFunc2[c1.GetType()][c2.GetType()](c1, c2);
        return b;
    }
    /// <param name="collisionPoint">The average point of collision of the two collider if true is return, (0,0) oterwise</param>
    /// <param name="normal1">The vector normal at the surface of the first collider where the collission happend</param>
    /// <param name="normal2">The vector normal at the surface of the second collider where the collission happend</param>
    /// <returns>true if the both collider collide together, false otherwise</returns>
    public static bool Collide(CustomCollider c1, CustomCollider c2, out Vector2 collisionPoint, out Vector2 normal1, out Vector2 normal2)
    {
        if (!FirstTestBeforeCollision(c1, c2))
        {
            collisionPoint = normal1 = normal2 = Vector2.zero;
            return false;
        }
        bool b;
        (b, collisionPoint, normal1, normal2) = collisionFunc3[c1.GetType()][c2.GetType()](c1, c2);
        return b;
    }

    /// <returns>true if the collider collide together width the line, false otherwise</returns>
    public static bool CollideLine(CustomCollider c, in Vector2 A, in Vector2 B) => collisionLine1[c.GetType()](c, A, B);
    /// <returns>true if the collider collide together width the line, false otherwise</returns>
    public static bool CollideLine(CustomCollider c, Line l) => CollideLine(c, l.A, l.B);
    /// <param name="collisionPoint">The point at the surface of the collider where the collission happend</param>
    /// <returns>true if the collider collide together width the line, false otherwise</returns>
    public static bool CollideLine(CustomCollider c, in Vector2 A, in Vector2 B, out Vector2 collisionPoint)
    {
        bool b;
        (b, collisionPoint) = collisionLine2[c.GetType()](c, A, B);
        return b;
    }
    /// <param name="collisionPoint">The point at the surface of the collider where the collission happend</param>
    /// <returns>true if the collider collide together width the line, false otherwise</returns>
    public static bool CollideLine(CustomCollider c, Line l, out Vector2 collisionPoint) => CollideLine(c, l.A, l.B, out collisionPoint);
    /// <param name="collisionPoint">The point at the surface of the collider where the collission happend</param>
    /// <param name="normal">The vector normal tothe collider's surface wehere the collision happend</param>
    /// <returns>true if the collider collide together width the line, false otherwise</returns>
    public static bool CollideLine(CustomCollider c, in Vector2 A, in Vector2 B, out Vector2 collisionPoint, out Vector2 normal)
    {
        bool b;
        (b, collisionPoint, normal) = collisionLine3[c.GetType()](c, A, B);
        return b;
    }
    /// <param name="collisionPoint">The point at the surface of the collider where the collission happend</param>
    /// <param name="normal">The vector normal tothe collider's surface wehere the collision happend</param>
    /// <returns>true if the collider collide together width the line, false otherwise</returns>
    public static bool CollideLine(CustomCollider c, Line l, out Vector2 collisionPoint, out Vector2 normal) => CollideLine(c, l.A, l.B, out collisionPoint, out normal);

    /// <returns>true if the collider collide together width the droite, false otherwise</returns>
    public static bool CollideDroite(CustomCollider c, in Vector2 A, in Vector2 B) => collisionDroite1[c.GetType()](c, A, B);
    /// <returns>true if the collider collide together width the droite, false otherwise</returns>
    public static bool CollideDroite(CustomCollider c, Droite d) => CollideDroite(c, d.A, d.B);
    /// <param name="collisionPoint">The point at the surface of the collider where the collission happend</param>
    /// <returns>true if the collider collide together width the droite, false otherwise</returns>
    public static bool CollideDroite(CustomCollider c, in Vector2 A, in Vector2 B, out Vector2 collisionPoint)
    {
        bool b;
        (b, collisionPoint) = collisionDroite2[c.GetType()](c, A, B);
        return b;
    }
    /// <param name="collisionPoint">The point at the surface of the collider where the collission happend</param>
    /// <returns>true if the collider collide together width the droite, false otherwise</returns>
    public static bool CollideDroite(CustomCollider c, Droite d, out Vector2 collisionPoint) => CollideDroite(c, d.A, d.B, out collisionPoint);
    /// <param name="collisionPoint">The point at the surface of the collider where the collission happend</param>
    /// <param name="normal">The vector normal tothe collider's surface wehere the collision happend</param>
    /// <returns>true if the collider collide together width the droite, false otherwise</returns>
    public static bool CollideDroite(CustomCollider c, in Vector2 A, in Vector2 B, out Vector2 collisionPoint, out Vector2 normal)
    {
        bool b;
        (b, collisionPoint, normal) = collisionDroite3[c.GetType()](c, A, B);
        return b;
    }
    /// <param name="collisionPoint">The point at the surface of the collider where the collission happend</param>
    /// <param name="normal">The vector normal tothe collider's surface wehere the collision happend</param>
    /// <returns>true if the collider collide together width the droite, false otherwise</returns>
    public static bool CollideDroite(CustomCollider c, Droite d, out Vector2 collisionPoint, out Vector2 normal) => CollideDroite(c, d.A, d.B, out collisionPoint, out normal);

    #endregion

    #region Collide(Circle, other)

    public static bool CollideCircles(Circle c1, Circle c2) => c1.center.SqrDistance(c2.center) <= (c1.radius + c2.radius) * (c1.radius + c2.radius);//OK
    public static bool CollideCircles(Circle c1, Circle c2, out Vector2 collisionPoint)//ok
    {
        float sqrDist = c1.center.SqrDistance(c2.center);
        float rr = (c1.radius + c2.radius) * (c1.radius + c2.radius);
        if (sqrDist <= rr)// il y a collision
        {
            if(sqrDist < (c1.radius - c2.radius) * (c1.radius - c2.radius))//un cercle inclus dans l'autre
            {
                if(c1.radius <= c2.radius)
                {
                    float angle = Useful.AngleHori(c2.center, c1.center);
                    Vector2 collisionPoint2 = new Vector2(c2.center.x + c2.radius * Mathf.Cos(angle), c2.center.y + c2.radius * Mathf.Sin(angle));
                    Vector2 collisionPoint1 = new Vector2(c1.center.x + c1.radius * Mathf.Cos(angle + Mathf.PI), c1.center.y + c1.radius * Mathf.Sin(angle + Mathf.PI));
                    collisionPoint = (collisionPoint1 + collisionPoint2) * 0.5f;
                }
                else
                {
                    float angle = Useful.AngleHori(c1.center, c2.center);
                    Vector2 collisionPoint2 = new Vector2(c1.center.x + c1.radius * Mathf.Cos(angle), c1.center.y + c1.radius * Mathf.Sin(angle));
                    Vector2 collisionPoint1 = new Vector2(c2.center.x + c2.radius * Mathf.Cos(angle + Mathf.PI), c2.center.y + c2.radius * Mathf.Sin(angle + Mathf.PI));
                    collisionPoint = (collisionPoint1 + collisionPoint2) * 0.5f;
                }
                return true;
            }
            else//cas d'intersection normale
            {
                if(Mathf.Abs(c1.center.y - c2.center.y) < accuracy)
                {
                    float x = ((c2.radius * c2.radius) - (c1.radius * c1.radius) - (c2.center.x * c2.center.x) + (c1.center.x * c1.center.x)) / (2f * (c1.center.x - c2.center.x));
                    float b = -2f * c2.center.y;
                    float c = (c2.center.x * c2.center.x) + (x * x) - (2f * c2.center.x * x) + (c2.center.y * c2.center.y) - (c2.radius * c2.radius);
                    float sqrtDelta = Mathf.Sqrt((b * b) - (4f * c));
                    Vector2 i1 = new Vector2(x, (-b - sqrtDelta) * 0.5f);
                    Vector2 i2 = new Vector2(x, (-b + sqrtDelta) * 0.5f);
                    collisionPoint = (i1 + i2) * 0.5f;
                    return true;
                }
                else
                {
                    float N = ((c2.radius * c2.radius) - (c1.radius * c1.radius) - (c2.center.x * c2.center.x) + (c1.center.x * c1.center.x) - (c2.center.y * c2.center.y) + (c1.center.y * c1.center.y)) / (2f * (c1.center.y - c2.center.y));
                    float temps = ((c1.center.x - c2.center.x) / (c1.center.y - c2.center.y));
                    float a = (temps * temps) + 1;
                    float b = (2f * c1.center.y * temps) - (2f * N * temps) - (2f * c1.center.x);
                    float c = (c1.center.x * c1.center.x) + (c1.center.y * c1.center.y) + (N * N) - (c1.radius * c1.radius) - (2f * c1.center.y * N);
                    float sqrtDelta = Mathf.Sqrt((b * b) - (4f * a * c));
                    float x1 = (-b - sqrtDelta) / (2f * a);
                    float x2 = (-b + sqrtDelta) / (2f * a);
                    Vector2 i1 = new Vector2(x1, N - (x1 * temps));
                    Vector2 i2 = new Vector2(x2, N - (x2 * temps));
                    collisionPoint = (i1 + i2) * 0.5f;
                    return true;
                }
            }
        }
        collisionPoint = Vector2.zero;
        return false;
    }
    public static bool CollideCircles(Circle c1, Circle c2, out Vector2 collisionPoint, out Vector2 normal1, out Vector2 normal2)
    {
        if(CollideCircles(c1, c2, out collisionPoint))
        {
            normal1 = (collisionPoint - c1.center);
            normal1.Normalize();
            normal2 = (collisionPoint - c2.center);
            normal2.Normalize();
            return true;
        }
        normal1 = normal2 = Vector2.zero;
        return false;
    }

    public static bool CollideCirclePolygone(Circle circle, Polygone polygone)//OK
    {
        for (int i = 0; i < polygone.vertices.Count; i++)
        {
            if (circle.CollideLine(polygone.vertices[i], polygone.vertices[(i + 1) % polygone.vertices.Count]))
                return true;
        }
        return polygone.Contains(circle.center);
    }
    public static bool CollideCirclePolygone(Circle circle, Polygone polygone, out Vector2 collisionPoint)//OK
    {
        collisionPoint = Vector2.zero;
        for (int i = 0; i < polygone.vertices.Count; i++)
        {
            if (CollideCircleLineBothCol(circle, polygone.vertices[i], polygone.vertices[(i + 1) % polygone.vertices.Count], out Vector2 inter))
                cache.Add(inter);
            if (circle.Contains(polygone.vertices[i]))
                cache.Add(polygone.vertices[i]);
        }
        if(cache.Count > 0)
        {
            foreach (Vector2 pos in cache)
            {
                collisionPoint += pos;
            }
            collisionPoint /= cache.Count;
            cache.Clear();
            return true;
        }
        if(polygone.Contains(circle.center))
        {
            collisionPoint = circle.center;
            return true;
        }
        return false;

        bool CollideCircleLineBothCol(Circle c, in Vector2 A, in Vector2 B, out Vector2 collision)
        {
            if (!CollideCircleLine(c, A, B))
            {
                collision = Vector2.zero;
                return false;
            }
            //on regarde si la droite est verticale
            if (Mathf.Abs(A.x - B.x) < accuracy)
            {
                float srqtDelta = Mathf.Sqrt((c.radius * c.radius) - (((A.x + B.x) * 0.5f) - c.center.x) * (((A.x + B.x) * 0.5f) - c.center.x));
                Vector2 i1 = new Vector2((A.x + B.x) * 0.5f, -srqtDelta + c.center.y);
                Vector2 i2 = new Vector2((A.x + B.x) * 0.5f, +srqtDelta + c.center.y);
                //on verif que i1 et i2 appartienne au seg
                if (Mathf.Min(A.y, B.y) <= i1.y && Mathf.Max(A.y, B.y) >= i1.y && Mathf.Min(A.y, B.y) <= i2.y && Mathf.Max(A.y, B.y) >= i2.y)
                {
                    collision = (i1 + i2) * 0.5f;
                    Vector2 dir = collision - c.center;
                    if (dir.sqrMagnitude < accuracy)
                        return true;

                    if(CollideCircleLine(c, collision, collision + c.radius * dir.normalized, out Vector2 col2))
                    {
                        collision = (collision + col2) * 0.5f;
                    }
                    return true;
                }
                if (Mathf.Min(A.y, B.y) <= i1.y && Mathf.Max(A.y, B.y) >= i1.y)
                {
                    collision = i1;
                    return true;
                }
                if (Mathf.Min(A.y, B.y) <= i2.y && Mathf.Max(A.y, B.y) >= i2.y)
                {
                    collision = i2;
                    return true;
                }
                collision = (i1 + i2) * 0.5f;
                Vector2 dir2 = collision - c.center;
                if (dir2.sqrMagnitude < accuracy)
                    return true;

                if (CollideCircleLine(c, collision, collision + c.radius * dir2.normalized, out Vector2 col3))
                {
                    collision = (collision + col3) * 0.5f;
                }
                return true;
            }
            else
            {
                float m = (B.y - A.y) / (B.x - A.x);
                float p = A.y - m * A.x;
                float a = 1f + (m * m);
                float b = 2f * ((m * p) - c.center.x - (m * c.center.y));
                float C = ((c.center.x * c.center.x) + (p * p) - (2f * p * c.center.y) + (c.center.y * c.center.y) - (c.radius * c.radius));
                float sqrtDelta = Mathf.Sqrt((b * b) - (4f * a * C));
                Vector2 i1 = new Vector2((-b - sqrtDelta) / (2f * a), m * ((-b - sqrtDelta) / (2f * a)) + p);
                Vector2 i2 = new Vector2((-b + sqrtDelta) / (2f * a), m * ((-b + sqrtDelta) / (2f * a)) + p);

                //on verif que i1 et i2 appartienne au seg
                if (Mathf.Min(A.y, B.y) <= i1.y && Mathf.Max(A.y, B.y) >= i1.y && Mathf.Min(A.x, B.x) <= i1.x && Mathf.Max(A.x, B.x) >= i1.x &&
                    Mathf.Min(A.y, B.y) <= i2.y && Mathf.Max(A.y, B.y) >= i2.y && Mathf.Min(A.x, B.x) <= i2.x && Mathf.Max(A.x, B.x) >= i2.x)
                {
                    collision = (i1 + i2) * 0.5f;
                    Vector2 dir = collision - c.center;
                    if (dir.sqrMagnitude < accuracy)
                        return true;

                    if (CollideCircleLine(c, collision, collision + c.radius * dir.normalized, out Vector2 col2))
                    {
                        collision = (collision + col2) * 0.5f;
                    }
                    return true;
                }
                if (Mathf.Min(A.y, B.y) <= i1.y && Mathf.Max(A.y, B.y) >= i1.y && Mathf.Min(A.x, B.x) <= i1.x && Mathf.Max(A.x, B.x) >= i1.x)
                {
                    collision = i1;
                    return true;
                }
                if (Mathf.Min(A.y, B.y) <= i2.y && Mathf.Max(A.y, B.y) >= i2.y && Mathf.Min(A.x, B.x) <= i2.x && Mathf.Max(A.x, B.x) >= i2.x)
                {
                    collision = i2;
                    return true;
                }
                collision = (i1 + i2) * 0.5f;
                Vector2 dir2 = collision - c.center;
                if (dir2.sqrMagnitude < accuracy)
                    return true;

                if (CollideCircleLine(c, collision, collision + c.radius * dir2.normalized, out Vector2 col3))
                {
                    collision = (collision + col3) * 0.5f;
                }
                return true;
            }
        }
    }
    public static bool CollideCirclePolygone(Circle circle, Polygone polygone, out Vector2 collisionPoint, out Vector2 normal1, out Vector2 normal2)
    {
        if (CollideCirclePolygone(circle, polygone, out collisionPoint))
        {
            normal1 = (collisionPoint - circle.center);
            normal1.Normalize();
            normal2 = -normal1;
            return true;
        }
        normal1 = normal2 = Vector2.zero;
        return false;
    }
    public static bool CollideCircleHitbox(Circle circle, Hitbox hitbox) => CollideCirclePolygone(circle, hitbox.rec);//ok
    public static bool CollideCircleHitbox(Circle circle, Hitbox hitbox, out Vector2 collisionPoint) => CollideCirclePolygone(circle, hitbox.rec, out collisionPoint);//OK
    public static bool CollideCircleHitbox(Circle circle, Hitbox hitbox, out Vector2 collisionPoint, out Vector2 normal1, out Vector2 normal2) => CollideCirclePolygone(circle, hitbox.rec, out collisionPoint, out normal1, out normal2);
    public static bool CollideCircleLine(Circle c, in Vector2 A, in Vector2 B) => c.CollideLine(A, B);//ok
    public static bool CollideCircleLine(Circle c, Line l) => c.CollideLine(l.A, l.B);//ok
    public static bool CollideCircleLine(Circle c, in Vector2 A, in Vector2 B, out Vector2 collisionPoint)//ok
    {
        if(!CollideCircleLine(c, A, B))
        {
            collisionPoint = Vector2.zero;
            return false;
        }
        //on regarde si la droite est verticale
        if (Mathf.Abs(A.x - B.x) < accuracy)
        {
            float srqtDelta= Mathf.Sqrt((c.radius * c.radius) - (((A.x + B.x) * 0.5f) - c.center.x) * (((A.x + B.x) * 0.5f) - c.center.x));
            Vector2 i1 = new Vector2((A.x + B.x) * 0.5f, -srqtDelta + c.center.y);
            Vector2 i2 = new Vector2((A.x + B.x) * 0.5f, +srqtDelta + c.center.y);
            //on verif que i1 et i2 appartienne au seg
            if(Mathf.Min(A.y, B.y) <= i1.y && Mathf.Max(A.y, B.y) >= i1.y && Mathf.Min(A.y, B.y) <= i2.y && Mathf.Max(A.y, B.y) >= i2.y)
            {
                collisionPoint = (i1 + i2) * 0.5f;
                Vector2 dir = collisionPoint - c.center;
                if(dir.sqrMagnitude < accuracy)
                    return true;

                return CollideCircleLine(c, collisionPoint, collisionPoint + c.radius * dir.normalized, out collisionPoint);
            }
            if(Mathf.Min(A.y, B.y) <= i1.y && Mathf.Max(A.y, B.y) >= i1.y)
            {
                collisionPoint = i1;
                return true;
            }
            if (Mathf.Min(A.y, B.y) <= i2.y && Mathf.Max(A.y, B.y) >= i2.y)
            {
                collisionPoint = i2;
                return true;
            }
            collisionPoint = (i1 + i2) * 0.5f;
            Vector2 dir2 = collisionPoint - c.center;
            if (dir2.sqrMagnitude < accuracy)
                return true;

            return CollideCircleLine(c, collisionPoint, collisionPoint + c.radius * dir2.normalized, out collisionPoint);
        }
        else
        {
            float m = (B.y - A.y) / (B.x - A.x);
            float p = A.y - m * A.x;
            float a = 1f + (m * m);
            float b = 2f * ((m * p) - c.center.x - (m * c.center.y));
            float C = ((c.center.x * c.center.x) + (p * p) - (2f * p * c.center.y) + (c.center.y * c.center.y) - (c.radius * c.radius));
            float sqrtDelta = Mathf.Sqrt((b * b) - (4f * a * C));
            Vector2 i1 = new Vector2((-b - sqrtDelta) / (2f * a), m * ((-b - sqrtDelta) / (2f * a)) + p);
            Vector2 i2 = new Vector2((-b + sqrtDelta) / (2f * a), m * ((-b + sqrtDelta) / (2f * a)) + p);

            //on verif que i1 et i2 appartienne au seg
            if (Mathf.Min(A.y, B.y) <= i1.y && Mathf.Max(A.y, B.y) >= i1.y && Mathf.Min(A.x, B.x) <= i1.x && Mathf.Max(A.x, B.x) >= i1.x &&
                Mathf.Min(A.y, B.y) <= i2.y && Mathf.Max(A.y, B.y) >= i2.y && Mathf.Min(A.x, B.x) <= i2.x && Mathf.Max(A.x, B.x) >= i2.x)
            {
                collisionPoint = (i1 + i2) * 0.5f;
                Vector2 dir = collisionPoint - c.center;
                if (dir.sqrMagnitude < accuracy)
                    return true;

                return CollideCircleLine(c, collisionPoint, collisionPoint + c.radius * dir.normalized, out collisionPoint);
            }
            if (Mathf.Min(A.y, B.y) <= i1.y && Mathf.Max(A.y, B.y) >= i1.y && Mathf.Min(A.x, B.x) <= i1.x && Mathf.Max(A.x, B.x) >= i1.x)
            {
                collisionPoint = i1;
                return true;
            }
            if (Mathf.Min(A.y, B.y) <= i2.y && Mathf.Max(A.y, B.y) >= i2.y && Mathf.Min(A.x, B.x) <= i2.x && Mathf.Max(A.x, B.x) >= i2.x)
            {
                collisionPoint = i2;
                return true;
            }
            collisionPoint = (i1 + i2) * 0.5f;
            Vector2 dir2 = collisionPoint - c.center;
            if (dir2.sqrMagnitude < accuracy)
                return true;

            return CollideCircleLine(c, collisionPoint, collisionPoint + c.radius * dir2.normalized, out collisionPoint);
        }
    }
    public static bool CollideCircleLine(Circle c, Line l, out Vector2 collisionPoint) => CollideCircleLine(c, l.A, l.B, out collisionPoint);//ok
    public static bool CollideCircleLine(Circle c, in Vector2 A, in Vector2 B, out Vector2 collisionPoint, out Vector2 normal)
    {
        if(CollideCircleLine(c, A, B, out collisionPoint))
        {
            normal = (collisionPoint - c.center);
            normal.Normalize();
            return true;
        }
        normal = Vector2.zero;
        return false;
    }
    public static bool CollideCircleLine(Circle c, Line l, out Vector2 collisionPoint, out Vector2 normal) => CollideCircleLine(c, l.A, l.B, out collisionPoint, out normal);
    public static bool CollideCircleDroite(Circle c, Droite d) => c.CollideDroite(d);//ok
    public static bool CollideCircleDroite(Circle c, in Vector2 A, in Vector2 B) => c.CollideDroite(A, B);//ok
    public static bool CollideCircleDroite(Circle c, Droite d, out Vector2 collisionPoint) => CollideCircleDroite(c, d, out collisionPoint);//OK
    public static bool CollideCircleDroite(Circle c, in Vector2 A, in Vector2 B, out Vector2 collisionPoint)//ok
    {
        if (!CollideCircleDroite(c, A, B))
        {
            collisionPoint = Vector2.zero;
            return false;
        }
        Vector2 u = new Vector2(B.x - A.x, B.y - A.y);
        Vector2 AC = new Vector2(c.center.x - A.x, c.center.y - A.y);
        float ti = (u.x * AC.x + u.y * AC.y) / (u.x * u.x + u.y * u.y);
        collisionPoint = new Vector2(A.x + ti * u.x, A.y + ti * u.y);
        if (collisionPoint.SqrDistance(c.center) >= c.radius * c.radius - accuracy)
            return true;

        return CollideCircleLine(c, collisionPoint, collisionPoint + c.radius * (collisionPoint - c.center).normalized, out collisionPoint);
    }
    public static bool CollideCircleDroite(Circle c, in Vector2 A, in Vector2 B, out Vector2 collisionPoint, out Vector2 normal)
    {
        if (CollideCircleDroite(c, A, B, out collisionPoint))
        {
            normal = (collisionPoint - c.center);
            normal.Normalize();
            return true;
        }
        normal = Vector2.zero;
        return false;
    }
    public static bool CollideCircleDroite(Circle c, Droite d, out Vector2 collisionPoint, out Vector2 normal) => CollideCircleDroite(c, d.A, d.B, out collisionPoint, out normal);
    public static bool CollideCircleCapsule(Circle circle, Capsule caps)//ok
    {
        return CollideCircleHitbox(circle, caps.hitbox) || CollideCircles(circle, caps.c1) || CollideCircles(circle, caps.c2);
    }
    public static bool CollideCircleCapsule(Circle circle, Capsule caps, out Vector2 collisionPoint)//OK
    {
        if (CollideCircleHitbox(circle, caps.hitbox, out collisionPoint))
        {
            cache.Add(collisionPoint);
        }
        if (CollideCircles(circle, caps.c2, out collisionPoint))
        {
            cache.Add(collisionPoint);
        }
        if(CollideCircles(circle, caps.c1, out collisionPoint))
        {
            cache.Add(collisionPoint);
        }
            
        collisionPoint = Vector2.zero;
        if (cache.Count > 0)
        {
            foreach  (Vector2 pos in cache)
            {
                collisionPoint += pos;
            }
            collisionPoint /= cache.Count;
            cache.Clear();
            return true;
        }
        return false;
    }
    public static bool CollideCircleCapsule(Circle circle, Capsule caps, out Vector2 collisionPoint, out Vector2 normal1, out Vector2 normal2)
    {
        if (CollideCircleCapsule(circle, caps, out collisionPoint))
        {
            normal1 = collisionPoint - circle.center;
            normal1.Normalize();
            normal2 = -normal1;
            return true;
        }
        normal1 = normal2 = Vector2.zero;
        return false;
    }

    #region CollideCircleEllipse(Circle c, Ellipse e)

    private const int maxEllipseIter = 10;

    private static float[] innerPolygonCoef, outerPolygonCoef;
    private static bool Iterate(float x, float y, float c0x, float c0y, float c2x, float c2y, float rr)
    {
        if(innerPolygonCoef == null || outerPolygonCoef == null)
        {
            innerPolygonCoef = new float[maxEllipseIter + 1];
            outerPolygonCoef = new float[maxEllipseIter + 1];
            for (int t = 0; t <= maxEllipseIter; t++)
            {
                int numNodes = 4 << t;
                innerPolygonCoef[t] = 0.5f / Mathf.Cos(4f * Mathf.Acos(0.0f) / numNodes);
                outerPolygonCoef[t] = 0.5f / (Mathf.Cos(2f * Mathf.Acos(0.0f) / numNodes) * Mathf.Cos(2 * Mathf.Acos(0.0f) / numNodes));
            }
        }

        for (int t = 1; t <= maxEllipseIter; t++)
        {
            float c1x = (c0x + c2x) * innerPolygonCoef[t];
            float c1y = (c0y + c2y) * innerPolygonCoef[t];
            float tx = x - c1x;
            float ty = y - c1y;
            if (tx * tx + ty * ty <= rr)
            {
                return true;
            }
            float t2x = c2x - c1x;
            float t2y = c2y - c1y;
            if (tx * t2x + ty * t2y >= 0 && tx * t2x + ty * t2y <= t2x * t2x + t2y * t2y &&
            (ty * t2x - tx * t2y >= 0 || rr * (t2x * t2x + t2y * t2y) >= (ty * t2x - tx * t2y) * (ty * t2x - tx * t2y)))
            {
                return true;
            }
            float t0x = c0x - c1x;
            float t0y = c0y - c1y;
            if (tx * t0x + ty * t0y >= 0 && tx * t0x + ty * t0y <= t0x * t0x + t0y * t0y &&
            (ty * t0x - tx * t0y <= 0 || rr * (t0x * t0x + t0y * t0y) >= (ty * t0x - tx * t0y) * (ty * t0x - tx * t0y)))
            {
                return true;
            }
            float c3x = (c0x + c1x) * outerPolygonCoef[t];
            float c3y = (c0y + c1y) * outerPolygonCoef[t];
            if ((c3x - x) * (c3x - x) + (c3y - y) * (c3y - y) < rr)
            {
                c2x = c1x;
                c2y = c1y;
                continue;
            }
            float c4x = c1x - c3x + c1x;
            float c4y = c1y - c3y + c1y;
            if ((c4x - x) * (c4x - x) + (c4y - y) * (c4y - y) < rr)
            {
                c0x = c1x;
                c0y = c1y;
                continue;
            }
            float t3x = c3x - c1x;
            float t3y = c3y - c1y;
            if (ty * t3x - tx * t3y <= 0 || rr * (t3x * t3x + t3y * t3y) > (ty * t3x - tx * t3y) * (ty * t3x - tx * t3y))
            {
                if (tx * t3x + ty * t3y > 0)
                {
                    if (Mathf.Abs(tx * t3x + ty * t3y) <= t3x * t3x + t3y * t3y || (x - c3x) * (c0x - c3x) + (y - c3y) * (c0y - c3y) >= 0)
                    {
                        c2x = c1x;
                        c2y = c1y;
                        continue;
                    }
                }
                else if (-(tx * t3x + ty * t3y) <= t3x * t3x + t3y * t3y || (x - c4x) * (c2x - c4x) + (y - c4y) * (c2y - c4y) >= 0)
                {
                    c0x = c1x;
                    c0y = c1y;
                    continue;
                }
            }
            return false;
        }
        return false; // Out of iterations so it is unsure if there was a collision. But have to return something.
    }

    public static bool CollideCircleEllipse(Circle c, Ellipse e)//OK
    {
        Vector2 center = e.center;
        float angle = Useful.AngleHori(e.center, e.focus1.x >= e.focus2.x ? e.focus1 : e.focus2);
        e.Rotate(-angle);
        Vector2 circleCenter = c.center;
        float w = Useful.AngleHori(center, c.center) - angle; //use w to avoid allocate another float, data will be erase
        float h = c.center.Distance(center); //use h to avoid allocate another float, data will be erase
        c.MoveAt(new Vector2(center.x + h * Mathf.Cos(w), center.y + h * Mathf.Sin(w)));

        w = e.majorAxis * 0.5f;
        h = e.minorAxis * 0.5f;
        float x0 = center.x, y0 = center.y;
        float x1 = c.center.x, y1 = c.center.y, r = c.radius;
        float x = Mathf.Abs(x1 - x0);
        float y = Mathf.Abs(y1 - y0);

        bool res;
        if (x * x + (h - y) * (h - y) <= r * r || (w - x) * (w - x) + y * y <= r * r || x * h + y * w <= w * h
        || ((x * h + y * w - w * h) * (x * h + y * w - w * h) <= r * r * (w * w + h * h) && x * w - y * h >= -h * h && x * w - y * h <= w * w))
        {
            res = true;
        }
        else
        {
            if ((x - w) * (x - w) + (y - h) * (y - h) <= r * r || (x <= w && y - r <= h) || (y <= h && x - r <= w))
            {
                res = Iterate(x, y, w, 0f, 0f, h, r * r);
            }
            else
            {
                res = false;
            }
        }

        c.MoveAt(circleCenter);
        e.Rotate(angle);
        return res;
    }
    public static bool CollideCircleEllipse(Circle c, Ellipse e, out Vector2 collisionPoint)//pas ok du tout
    {
        throw new NotImplementedException();
        //on se ramène au cas d'ellipse horizontale et l'ellipse est centré en (0,0)
        /*
        Vector2 center = e.center;
        float angle = Useful.AngleHori(e.center, e.focus1.x >= e.focus2.x ? e.focus1 : e.focus2);
        e.Rotate(-angle);
        float distBetweenCenter = c.center.Distance(center);
        float angleBetweenCAndE = Useful.AngleHori(center, c.center);
        c.MoveAt(new Vector2(distBetweenCenter * Mathf.Cos(angleBetweenCAndE - angle), distBetweenCenter * Mathf.Sin(angleBetweenCAndE - angle)));
        e.MoveAt(Vector2.zero);

        //on pose l'équation en t (paramétrique)
        float aa = e.majorAxis * e.majorAxis * 0.25f, bb = e.minorAxis * e.minorAxis * 0.25f;
        float xc = c.center.x, yc = c.center.y, r = c.radius;

        float A = bb * xc - 2f * xc * r * bb + bb + aa * yc * yc - aa * bb;
        float B = 4f * aa * yc * r;
        float C = 2f * bb * xc + 2f * bb * xc * r - 2f * xc * r * bb - 2f * bb + 4f * aa - 2f * aa * bb;
        float D = 2f * aa * yc * yc + 4f * aa * yc * r;
        float E = bb * xc + 2f * bb * xc * r + bb + aa * yc * yc - aa * bb;
        //On résoud l'éqaution At^4 + Bt^3 + Ct² + Dt + E = 0
        

        List<Vector2> cols = new List<Vector2>();
        if(res != null)
        {
            for (int i = 0; i < res.Count; i++)
            {
                float t = res[i];
                if (t < float.MaxValue && t > float.MinValue && t != float.NaN)
                {
                    cols.Add(new Vector2(c.center.x + c.radius * ((1f - t * t) / (1f + t * t)), c.center.y + c.radius * (2f * t / (1f + t * t))));
                }
            }
        }

        Vector2 avg = Vector2.zero;
        for (int i = 0; i < cols.Count; i++)
        {
            aa = cols[i].magnitude;//use aa => tmpDist
            bb = Useful.AngleHori(Vector2.zero, cols[i]);//use bb => tmpAngle
            cols[i] = new Vector2(center.x + aa * Mathf.Cos(bb + angle), center.y + aa * Mathf.Sin(bb + angle));
            avg += cols[i];
        }

        e.Rotate(angle);
        e.MoveAt(center);
        c.MoveAt(new Vector2(center.x + distBetweenCenter * Mathf.Cos(angleBetweenCAndE), center.y + distBetweenCenter * Mathf.Sin(angleBetweenCAndE)));

        if(cols.Count > 0)
        {
            avg /= cols.Count;
            collisionPoint = avg;
            return true;
        }
        collisionPoint = Vector2.zero;
        return false;
        */
    }
    public static bool CollideCircleEllipse(Circle c, Ellipse e, out Vector2 collisionPoint, out Vector2 normal1, out Vector2 normal2)
    {
        if(CollideCircleEllipse(c, e, out collisionPoint))
        {
            c.Normal(collisionPoint, out normal1);
            e.Normal(collisionPoint, out normal2);
            return true;
        }
        normal1 = normal2 = Vector2.zero;
        return false;
    }

    #endregion

    #endregion

    #region Collide(Polygones, other)

    public static bool CollidePolygones(Polygone p1, Polygone p2)//OK
    {
        for (int i = 0; i < p1.vertices.Count; i++)
        {
            for (int j = 0; j < p2.vertices.Count; j++)
            {
                if (CollideLines(p1.vertices[i], p1.vertices[(i + 1) % p1.vertices.Count], p2.vertices[j], p2.vertices[(j + 1) % p2.vertices.Count]))
                {
                    return true;
                }
            }
        }
        return p1.Contains(p2.center) || p2.Contains(p1.center);
    }
    public static bool CollidePolygones(Polygone p1, Polygone p2, out Vector2 collisionPoint)
    {
        collisionPoint = Vector2.zero;
        for (int i = 0; i < p1.vertices.Count; i++)
        {
            for (int j = 0; j < p2.vertices.Count; j++)
            {
                if (CollideLines(p1.vertices[i], p1.vertices[(i + 1) % p1.vertices.Count], p2.vertices[j], p2.vertices[(j + 1) % p2.vertices.Count], out Vector2 intersec))
                {
                    cache.Add(intersec);
                }
            }
        }
        if(cache.Count > 0)
        {
            foreach (Vector2 pos in cache)
            {
                collisionPoint += pos;
            }
            collisionPoint /= cache.Count;
            cache.Clear();
            return true;
        }
        return false;
    }
    public static bool CollidePolygones(Polygone p1, Polygone p2, out Vector2 collisionPoint, out Vector2 normal1, out Vector2 normal2)
    {
        for (int i = 0; i < p1.vertices.Count; i++)
        {
            for (int j = 0; j < p2.vertices.Count; j++)
            {
                if (CollideLines(p1.vertices[i], p1.vertices[(i + 1) % p1.vertices.Count], p2.vertices[j], p2.vertices[(j + 1) % p2.vertices.Count], out Vector2 intersec))
                {
                    cache.Add(intersec);
                    Vector2 n1 = (p1.vertices[(i + 1) % p1.vertices.Count] - p1.vertices[i]).NormalVector();
                    //on regarde si on est dans le bon sens
                    Vector2 middle = (p1.vertices[i] + p1.vertices[(i + 1) % p1.vertices.Count]) * 0.5f;
                    if (p1.Contains(middle + n1))//tromper de sens
                    {
                        n1 *= -1f;
                    }
                    cache2.Add(n1);//Stocker le vecteur normal au coté de p1

                    Vector2 n2 = (p2.vertices[(j + 1) % p2.vertices.Count] - p2.vertices[j]).NormalVector();
                    n2.Normalize();
                    middle = (p2.vertices[j] + p2.vertices[(j + 1) % p2.vertices.Count]) * 0.5f;
                    if (p2.Contains(middle + n2))//tromper de sens
                    {
                        n2 *= -1f;
                    }
                    cache3.Add(n2);//Stocker le vecteur normal au coté de p2
                }
            }
        }
        collisionPoint = normal1 = normal2 = Vector2.zero;
        if (cache.Count > 0)
        {
            foreach (Vector2 pos in cache)
            {
                collisionPoint += pos;
            }
            collisionPoint /= cache.Count;
            cache.Clear();

            float averageAngle = 0f, averageAngle2;
            foreach (Vector2 n in cache2)
            {
                averageAngle += Useful.Angle(Vector2.zero, n); 
            }
            averageAngle /= cache2.Count;
            averageAngle2 = Useful.WrapAngle(averageAngle + Mathf.PI);
            float dist1 = 0f, dist2 = 0f;
            foreach (Vector2 n in cache2)
            {
                float angle = Useful.Angle(Vector2.zero, n);
                dist1 += Mathf.Abs(angle - averageAngle) % Mathf.PI;
                dist2 += Mathf.Abs(angle - averageAngle2) % Mathf.PI;
            }
            averageAngle = dist1 <= dist2 ? averageAngle : averageAngle2;
            normal1 = new Vector2(Mathf.Cos(averageAngle), Mathf.Sin(averageAngle));

            averageAngle = 0f;
            foreach (Vector2 n in cache3)
            {
                averageAngle += Useful.Angle(Vector2.zero, n);
            }
            averageAngle /= cache3.Count;
            averageAngle2 = Useful.WrapAngle(averageAngle + Mathf.PI);
            dist1 = dist2 = 0f;
            foreach (Vector2 n in cache3)
            {
                float angle = Useful.Angle(Vector2.zero, n);
                dist1 += Mathf.Abs(angle - averageAngle) % Mathf.PI;
                dist2 += Mathf.Abs(angle - averageAngle2) % Mathf.PI;
            }
            averageAngle = dist1 <= dist2 ? averageAngle : averageAngle2;
            normal2 = new Vector2(Mathf.Cos(averageAngle), Mathf.Sin(averageAngle));

            cache2.Clear();
            cache3.Clear();
            return true;
        }
        return false;
    }
    public static bool CollidePolygoneHitbox(Polygone p, Hitbox h) => CollidePolygones(h.rec, p);//OK
    public static bool CollidePolygoneHitbox(Polygone p, Hitbox h, out Vector2 collisionPoint) => CollidePolygones(p, h.rec, out collisionPoint);
    public static bool CollidePolygoneHitbox(Polygone p, Hitbox h, out Vector2 collisionPoint, out Vector2 normal1, out Vector2 normal2) => CollidePolygones(p, h.rec, out collisionPoint, out normal1, out normal2);
    public static bool CollidePolygoneLine(Polygone p, in Vector2 A, in Vector2 B) => p.CollideLine(A, B);//OK
    public static bool CollidePolygoneLine(Polygone p, Line l) => p.CollideLine(l.A, l.B);//OK
    public static bool CollidePolygoneLine(Polygone p, in Vector2 A, in Vector2 B, out Vector2 collisionPoint)
    {
        collisionPoint = Vector2.zero;
        for (int i = 0; i < p.vertices.Count; i++)
        {
            if (CollideLines(p.vertices[i], p.vertices[(i + 1) % p.vertices.Count], A, B, out Vector2 intersec))
            {
                cache.Add(intersec);
            }
        }
        if(cache.Count > 0)
        {
            foreach (Vector2 pos in cache)
            {
                collisionPoint += pos;
            }
            collisionPoint /= cache.Count;
            cache.Clear();
            return true;
        }
        return false;
    }
    public static bool CollidePolygoneLine(Polygone p, Line l, out Vector2 collisionPoint) => CollidePolygoneLine(p, l.A, l.B, out collisionPoint);
    public static bool CollidePolygoneLine(Polygone p, in Vector2 A, in Vector2 B, out Vector2 collisionPoint, out Vector2 normal)
    {
        collisionPoint = Vector2.zero;
        for (int i = 0; i < p.vertices.Count; i++)
        {
            if (CollideLines(p.vertices[i], p.vertices[(i + 1) % p.vertices.Count], A, B, out Vector2 intersec))
            {
                cache.Add(intersec);
                Vector2 n = (p.vertices[(i + 1) % p.vertices.Count] - p.vertices[i]).NormalVector();
                n.Normalize();
                //on regarde si on est dans le bon sens
                Vector2 middle = (p.vertices[i] + p.vertices[(i + 1) % p.vertices.Count]) * 0.5f;
                if (p.Contains(middle + n))//tromper de sens
                {
                    n *= -1f;
                }
                cache2.Add(n);//Stocker le vecteur normal au coté de p1
            }
        }
            
        if (cache.Count > 0)
        {
            foreach (Vector2 pos in cache)
            {
                collisionPoint += pos;
            }
            collisionPoint /= cache.Count;
            cache.Clear();
            
            if (cache2.Count == 1)
            {
                normal = cache2[0];
                cache2.Clear();
                return true;
            }
            //on vérif le sens
            float averageAngle = 0f, averageAngle2;
            foreach (Vector2 n in cache2)
            {
                averageAngle += Useful.Angle(Vector2.zero, n);
            }
            averageAngle /= cache2.Count;
            averageAngle2 = Useful.WrapAngle(averageAngle + Mathf.PI);
            float dist1 = 0f, dist2 = 0f;
            foreach (Vector2 n in cache2)
            {
                float angle = Useful.Angle(Vector2.zero, n);
                dist1 += Mathf.Abs(angle - averageAngle) % Mathf.PI;
                dist2 += Mathf.Abs(angle - averageAngle2) % Mathf.PI;
            }
            averageAngle = dist1 <= dist2 ? averageAngle : averageAngle2;
            normal = new Vector2(Mathf.Cos(averageAngle), Mathf.Sin(averageAngle));

            cache2.Clear();
            return true;
        }
        normal = Vector2.zero;
        return false;
    }
    public static bool CollidePolygoneLine(Polygone p, Line l, out Vector2 collisionPoint, out Vector2 normal) => CollidePolygoneLine(p, l.A, l.B, out collisionPoint, out normal);
    public static bool CollidePolygoneDroite(Polygone p, in Vector2 A, in Vector2 B) => p.CollideDroite(A, B);//OK
    public static bool CollidePolygoneDroite(Polygone p, Droite d) => p.CollideDroite(d);//OK
    public static bool CollidePolygoneDroite(Polygone p, in Vector2 A, in Vector2 B, out Vector2 collisionPoint)
    {
        collisionPoint = Vector2.zero;
        for (int i = 0; i < p.vertices.Count; i++)
        {
            if (CollideLineDroite(p.vertices[i], p.vertices[(i + 1) % p.vertices.Count], A, B, out Vector2 intersec))
            {
                cache.Add(intersec);
            }
        }
        if (cache.Count > 0)
        {
            foreach (Vector2 pos in cache)
            {
                collisionPoint += pos;
            }
            collisionPoint /= cache.Count;
            cache.Clear();
            return true;
        }
        return false;
    }
    public static bool CollidePolygoneDroite(Polygone p, Droite d, out Vector2 collisionPoint) => CollidePolygoneDroite(p, d.A, d.B, out collisionPoint);
    public static bool CollidePolygoneDroite(Polygone p, in Vector2 A, in Vector2 B, out Vector2 collisionPoint, out Vector2 normal)
    {
        collisionPoint = Vector2.zero;
        for (int i = 0; i < p.vertices.Count; i++)
        {
            if (CollideLineDroite(p.vertices[i], p.vertices[(i + 1) % p.vertices.Count], A, B, out Vector2 intersec))
            {
                cache.Add(intersec);
                Vector2 n = (p.vertices[(i + 1) % p.vertices.Count] - p.vertices[i]).NormalVector();
                n.Normalize();
                //on regarde si on est dans le bon sens
                Vector2 middle = (p.vertices[i] + p.vertices[(i + 1) % p.vertices.Count]) * 0.5f;
                if (p.Contains(middle + n))//tromper de sens
                {
                    n *= -1f;
                }
                cache2.Add(n);//Stocker le vecteur normal au coté de p1
            }
        }

        if (cache.Count > 0)
        {
            foreach (Vector2 pos in cache)
            {
                collisionPoint += pos;
            }
            collisionPoint /= cache.Count;
            cache.Clear();

            if (cache2.Count == 1)
            {
                normal = cache2[0];
                cache2.Clear();
                return true;
            }
            //on vérif le sens
            float averageAngle = 0f, averageAngle2;
            foreach (Vector2 n in cache2)
            {
                averageAngle += Useful.Angle(Vector2.zero, n);
            }
            averageAngle /= cache2.Count;
            averageAngle2 = Useful.WrapAngle(averageAngle + Mathf.PI);
            float dist1 = 0f, dist2 = 0f;
            foreach (Vector2 n in cache2)
            {
                float angle = Useful.Angle(Vector2.zero, n);
                dist1 += Mathf.Abs(angle - averageAngle) % Mathf.PI;
                dist2 += Mathf.Abs(angle - averageAngle2) % Mathf.PI;
            }
            averageAngle = dist1 <= dist2 ? averageAngle : averageAngle2;
            normal = new Vector2(Mathf.Cos(averageAngle), Mathf.Sin(averageAngle));
            cache2.Clear();
            return true;
        }
        normal = Vector2.zero;
        return false;
    }
    public static bool CollidePolygoneDroite(Polygone p, Droite d, out Vector2 collisionPoint, out Vector2 normal) => CollidePolygoneDroite(p, d.A, d.B, out collisionPoint, out normal);
    public static bool CollidePolygoneCapsule(Polygone p, Capsule caps) => CollidePolygoneHitbox(p, caps.hitbox) || CollideCirclePolygone(caps.c1, p) || CollideCirclePolygone(caps.c2, p);//OK
    public static bool CollidePolygoneCapsule(Polygone p, Capsule caps, out Vector2 collisionPoint)
    {
        if(CollidePolygoneHitbox(p, caps.hitbox, out collisionPoint))
        {
            cache.Add(collisionPoint);
        }
        if (CollideCirclePolygone(caps.c1, p, out collisionPoint))
        {
            cache.Add(collisionPoint);
        }
        if (CollideCirclePolygone(caps.c2, p, out collisionPoint))
        {
            cache.Add(collisionPoint);
        }
        collisionPoint = Vector2.zero;
        if(cache.Count > 0)
        {
            foreach (Vector2 pos in cache)
            {
                collisionPoint += pos;
            }
            collisionPoint /= cache.Count;
            cache.Clear();
            return true;
        }
        return false;
    }
    public static bool CollidePolygoneCapsule(Polygone p, Capsule caps, out Vector2 collisionPoint, out Vector2 normal1, out Vector2 normal2)
    {
        collisionPoint = normal1 = normal2 = Vector2.zero;
        if (CollideCirclePolygone(caps.c1, p, out Vector2 col, out Vector2 n1, out Vector2 n2))
        {
            cache.Add(col);
            cache2.Add(n1);
            cache3.Add(n2);
        }
        if (CollideCirclePolygone(caps.c2, p, out col, out n1, out n2))
        {
            cache.Add(col);
            cache2.Add(n1);
            cache3.Add(n2);
        }
        if (CollidePolygoneHitbox(p, caps.hitbox, out col, out n1, out n2))
        {
            cache.Add(col);
            //cache2.Add(n1);
            //cache3.Add(n2);
            normal1 = n1;
            normal2 = n2;
        }
        if(cache.Count > 0)
        {
            foreach (Vector2 pos in cache)
            {
                collisionPoint += pos;
            }
            collisionPoint /= cache.Count;
            cache.Clear();

            if (normal1 == Vector2.zero)
            {
                foreach (Vector2 n in cache2)
                {
                    normal1 += n;
                }
                normal1 /= cache2.Count;
                foreach (Vector2 n in cache3)
                {
                    normal2 += n;
                }
                normal2 /= cache3.Count;
            }
            cache2.Clear();
            cache3.Clear();
            return true;
        }
        return false;
    }
    public static bool CollidePolygoneEllipse(Polygone p, Ellipse e)//ok
    {
        for (int i = 0; i < p.vertices.Count; i++)
        {
            if (CollideEllipseLine(e, p.vertices[i], p.vertices[(i + 1) % p.vertices.Count]))
                return true;
        }
        return false;
    }
    public static bool CollidePolygoneEllipse(Polygone p, Ellipse e, out Vector2 collisionPoint)
    {
        return Collide(p, e.ToPolygone(), out collisionPoint);
    }
    public static bool CollidePolygoneEllipse(Polygone p, Ellipse e, out Vector2 collisionPoint, out Vector2 normal1, out Vector2 normal2)
    {
        return Collide(p, e.ToPolygone(), out collisionPoint, out normal1, out normal2);
    }

    #endregion

    #region Collide(Hitbox, other)

    public static bool CollideHitboxs(Hitbox h1, Hitbox h2) => CollidePolygones(h1.rec, h2.rec);
    public static bool CollideHitboxs(Hitbox h1, Hitbox h2, out Vector2 collisionPoint) => CollidePolygones(h1.rec, h2.rec, out collisionPoint);
    public static bool CollideHitboxs(Hitbox h1, Hitbox h2, out Vector2 collisionPoint, out Vector2 normal1, out Vector2 normal2) => CollidePolygones(h1.rec, h2.rec, out collisionPoint, out normal1, out normal2);
    public static bool CollideHitboxLine(Hitbox h, in Vector2 A, in Vector2 B) => h.rec.CollideLine(A, B);
    public static bool CollideHitboxLine(Hitbox h, Line l) => h.rec.CollideLine(l.A, l.B);
    public static bool CollideHitboxLine(Hitbox h, in Vector2 A, in Vector2 B, out Vector2 collisionPoint) => CollidePolygoneLine(h.rec, A, B, out collisionPoint);
    public static bool CollideHitboxLine(Hitbox h, Line l, out Vector2 collisionPoint) => CollidePolygoneLine(h.rec, l.A, l.B, out collisionPoint);
    public static bool CollideHitboxLine(Hitbox h, in Vector2 A, in Vector2 B, out Vector2 collisionPoint, out Vector2 normal) => CollidePolygoneLine(h.rec, A, B, out collisionPoint, out normal);
    public static bool CollideHitboxLine(Hitbox h, Line l, out Vector2 collisionPoint, out Vector2 normal) => CollidePolygoneLine(h.rec, l.A, l.B, out collisionPoint, out normal);
    public static bool CollideHitboxDroite(Hitbox h, Droite d) => h.rec.CollideDroite(d);
    public static bool CollideHitboxDroite(Hitbox h, in Vector2 A, in Vector2 B) => h.rec.CollideDroite(A, B);
    public static bool CollideHitboxDroite(Hitbox h, in Vector2 A, in Vector2 B, out Vector2 collisionPoint) => CollidePolygoneDroite(h.rec, A, B, out collisionPoint);
    public static bool CollideHitboxDroite(Hitbox h, Line l, out Vector2 collisionPoint) => CollidePolygoneDroite(h.rec, l.A, l.B, out collisionPoint);
    public static bool CollideHitboxDroite(Hitbox h, in Vector2 A, in Vector2 B, out Vector2 collisionPoint, out Vector2 normal) => CollidePolygoneDroite(h.rec, A, B, out collisionPoint, out normal);
    public static bool CollideHitboxDroite(Hitbox h, Line l, out Vector2 collisionPoint, out Vector2 normal) => CollidePolygoneDroite(h.rec, l.A, l.B, out collisionPoint, out normal);
    public static bool CollideHitboxCapsule(Hitbox hitbox, Capsule caps) => CollideHitboxs(hitbox, caps.hitbox) || CollideCircleHitbox(caps.c1, hitbox) || CollideCircleHitbox(caps.c2, hitbox);
    public static bool CollideHitboxCapsule(Hitbox hitbox, Capsule caps, out Vector2 collisionPoint) => CollidePolygoneCapsule(hitbox.rec, caps, out collisionPoint);
    public static bool CollideHitboxCapsule(Hitbox hitbox, Capsule caps, out Vector2 collisionPoint, out Vector2 normal1, out Vector2 normal2) => CollidePolygoneCapsule(hitbox.rec, caps, out collisionPoint, out normal1, out normal2);
    public static bool CollideHitboxEllipse(Hitbox hitbox, Ellipse e)
    {
        throw new NotImplementedException();
    }
    public static bool CollideHitboxEllipse(Hitbox hitbox, Ellipse e, out Vector2 collisionPoint)
    {
        throw new NotImplementedException();
    }
    public static bool CollideHitboxEllipse(Hitbox hitbox, Ellipse e, out Vector2 collisionPoint, out Vector2 normal1, out Vector2 normal2)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Collide(Capsule, other)

    public static bool CollideCapsules(Capsule caps1, Capsule caps2) => CollideHitboxCapsule(caps1.hitbox, caps2) || CollideCircleCapsule(caps1.c1, caps2) || CollideCircleCapsule(caps1.c2, caps2);
    public static bool CollideCapsules(Capsule caps1, Capsule caps2, out Vector2 collisionPoint)
    {
        if(CollideHitboxs(caps1.hitbox, caps2.hitbox, out collisionPoint))
        {
            cache.Add(collisionPoint);
        }
        if (CollideCircleHitbox(caps2.c1, caps1.hitbox, out collisionPoint))
        {
            cache.Add(collisionPoint);
        }
        if (CollideCircleHitbox(caps2.c2, caps1.hitbox, out collisionPoint))
        {
            cache.Add(collisionPoint);
        }
        if (CollideCircleHitbox(caps1.c1, caps2.hitbox, out collisionPoint))
        {
            cache.Add(collisionPoint);
        }
        if (CollideCircles(caps1.c1, caps2.c1, out collisionPoint))
        {
            cache.Add(collisionPoint);
        }
        if (CollideCircles(caps1.c1, caps2.c2, out collisionPoint))
        {
            cache.Add(collisionPoint);
        }
        if (CollideCircleHitbox(caps1.c2, caps2.hitbox, out collisionPoint))
        {
            cache.Add(collisionPoint);
        }
        if (CollideCircles(caps1.c2, caps2.c1, out collisionPoint))
        {
            cache.Add(collisionPoint);
        }
        if (CollideCircles(caps1.c2, caps2.c2, out collisionPoint))
        {
            cache.Add(collisionPoint);
        }
        collisionPoint = Vector2.zero;
        if(cache.Count > 0)
        {
            foreach (Vector2 pos in cache)
            {
                collisionPoint += pos;
            }
            collisionPoint /= cache.Count;
            cache.Clear();
            return true;
        }
        return false;
    }
    public static bool CollideCapsules(Capsule caps1, Capsule caps2, out Vector2 collisionPoint, out Vector2 normal1, out Vector2 normal2)
    {
        if(CollideCapsules(caps1, caps2, out collisionPoint))
        {
            normal1 = collisionPoint - caps1.center;
            normal1.Normalize();
            normal2 = collisionPoint - caps2.center;
            normal2.Normalize();
            return true;
        }
        normal1 = normal2 = Vector2.zero;
        return false;
    }
    public static bool CollideCapsuleLine(Capsule caps, Line l) => caps.CollideLine(l);
    public static bool CollideCapsuleLine(Capsule caps, in Vector2 A, in Vector2 B) => caps.CollideLine(A, B);
    public static bool CollideCapsuleLine(Capsule caps, in Vector2 A, in Vector2 B, out Vector2 collisionPoint)
    {
        if(CollideCircleLine(caps.c1, A, B, out collisionPoint))
        {
            cache.Add(collisionPoint);
        }
        if (CollideCircleLine(caps.c2, A, B, out collisionPoint))
        {
            cache.Add(collisionPoint);
        }
        if (CollideHitboxLine(caps.hitbox, A, B, out collisionPoint))
        {
            cache.Add(collisionPoint);
        }
        collisionPoint = Vector2.zero;
        if(cache.Count > 0)
        {
            foreach (Vector2 pos in cache)
            {
                collisionPoint += pos;
            }
            collisionPoint /= cache.Count;
            cache.Clear();
            return true;
        }
        return false;
    }
    public static bool CollideCapsuleLine(Capsule caps, Line l, out Vector2 collisionPoint) => CollideCapsuleLine(caps, l.A, l.B, out collisionPoint);
    public static bool CollideCapsuleLine(Capsule caps, in Vector2 A, in Vector2 B, out Vector2 collisionPoint, out Vector2 normal)
    {
        if(CollideCapsuleLine(caps, A, B, out collisionPoint))
        {
            normal = collisionPoint - caps.center;
            normal.Normalize();
            return true;
        }
        normal = Vector2.zero;
        return false;
    }
    public static bool CollideCapsuleLine(Capsule caps, Line l, out Vector2 collisionPoint, out Vector2 normal) => CollideCapsuleLine(caps, l.A, l.B, out collisionPoint, out normal);
    public static bool CollideCapsuleDroite(Capsule caps, Droite d) => caps.CollideDroite(d);
    public static bool CollideCapsuleDroite(Capsule caps, in Vector2 A, in Vector2 B) => caps.CollideDroite(A, B);
    public static bool CollideCapsuleDroite(Capsule caps, in Vector2 A, in Vector2 B, out Vector2 collisionPoint)
    {
        if (CollideCircleDroite(caps.c1, A, B, out collisionPoint))
        {
            cache.Add(collisionPoint);
        }
        if (CollideCircleDroite(caps.c2, A, B, out collisionPoint))
        {
            cache.Add(collisionPoint);
        }
        if (CollideHitboxDroite(caps.hitbox, A, B, out collisionPoint))
        {
            cache.Add(collisionPoint);
        }
        collisionPoint = Vector2.zero;
        if (cache.Count > 0)
        {
            foreach (Vector2 pos in cache)
            {
                collisionPoint += pos;
            }
            collisionPoint /= cache.Count;
            cache.Clear();
            return true;
        }
        return false;
    }
    public static bool CollideCapsuleDroite(Capsule caps, Line l, out Vector2 collisionPoint) => CollideCapsuleDroite(caps, l.A, l.B, out collisionPoint);
    public static bool CollideCapsuleDroite(Capsule caps, in Vector2 A, in Vector2 B, out Vector2 collisionPoint, out Vector2 normal)
    {
        if (CollideCapsuleDroite(caps, A, B, out collisionPoint))
        {
            normal = collisionPoint - caps.center;
            normal.Normalize();
            return true;
        }
        normal = Vector2.zero;
        return false;
    }
    public static bool CollideCapsuleDroite(Capsule caps, Line l, out Vector2 collisionPoint, out Vector2 normal) => CollideCapsuleDroite(caps, l.A, l.B, out collisionPoint, out normal);
    public static bool CollideCapsuleEllipse(Capsule caps, Ellipse e)
    {
        throw new NotImplementedException();
    }
    public static bool CollideCapsuleEllipse(Capsule caps, Ellipse e, out Vector2 collisionPoint)
    {
        throw new NotImplementedException();
    }
    public static bool CollideCapsuleEllipse(Capsule caps, Ellipse e, out Vector2 collisionPoint, out Vector2 normal1, out Vector2 normal2)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Collide(Ellipse, other)

    public static bool CollideEllipses(Ellipse e1, Ellipse e2)//OK
    {
        Vector2 center1 = e1.center, center2 = e2.center;
        float x0 = center1.x, y0 = center1.y, x1 = center2.x, y1 = center2.y;
        float angle = Useful.AngleHori(e1.center, e1.focus1.x >= e1.focus2.x ? e1.focus1 : e1.focus2);
        float wx0 = e1.majorAxis * 0.5f * Mathf.Cos(angle), wy0 = e1.majorAxis * 0.5f * Mathf.Sin(angle);
        angle = Useful.AngleHori(e2.center, e2.focus1.x >= e2.focus2.x ? e2.focus1 : e2.focus2);
        float wx1 = e2.majorAxis * 0.5f * Mathf.Cos(angle), wy1 = e2.majorAxis * 0.5f * Mathf.Sin(angle);
        float hw0 = e1.minorAxis / e1.majorAxis, hw1 = e2.minorAxis / e2.majorAxis;

        // Test for collision between two ellipses, "0" and "1". Ellipse is at (x, y) with major or minor radius 
        // vector (wx, wy) and the other major or minor radius perpendicular to that vector and hw times as long.
        float rr = hw1 * hw1 * (wx1 * wx1 + wy1 * wy1) * (wx1 * wx1 + wy1 * wy1) * (wx1 * wx1 + wy1 * wy1);
        float x = hw1 * wx1 * (wy1 * (y1 - y0) + wx1 * (x1 - x0)) - wy1 * (wx1 * (y1 - y0) - wy1 * (x1 - x0));
        float y = hw1 * wy1 * (wy1 * (y1 - y0) + wx1 * (x1 - x0)) + wx1 * (wx1 * (y1 - y0) - wy1 * (x1 - x0));
        float temp = wx0;
        wx0 = hw1 * wx1 * (wy1 * wy0 + wx1 * wx0) - wy1 * (wx1 * wy0 - wy1 * wx0);
        float temp2 = wy0;
        wy0 = hw1 * wy1 * (wy1 * wy0 + wx1 * temp) + wx1 * (wx1 * wy0 - wy1 * temp);
        float hx0 = hw1 * wx1 * (wy1 * (temp * hw0) - wx1 * temp2 * hw0) - wy1 * (wx1 * (temp * hw0) + wy1 * temp2 * hw0);
        float hy0 = hw1 * wy1 * (wy1 * (temp * hw0) - wx1 * temp2 * hw0) + wx1 * (wx1 * (temp * hw0) + wy1 * temp2 * hw0);

        if (wx0 * y - wy0 * x < 0f)
        {
            x = -x;
            y = -y;
        }

        if ((wx0 - x) * (wx0 - x) + (wy0 - y) * (wy0 - y) <= rr)
        {
            return true;
        }
        else if ((wx0 + x) * (wx0 + x) + (wy0 + y) * (wy0 + y) <= rr)
        {
            return true;
        }
        else if ((hx0 - x) * (hx0 - x) + (hy0 - y) * (hy0 - y) <= rr)
        {
            return true;
        }
        else if ((hx0 + x) * (hx0 + x) + (hy0 + y) * (hy0 + y) <= rr)
        {
            return true;
        }
        else if (x * (hy0 - wy0) + y * (wx0 - hx0) <= hy0 * wx0 - hx0 * wy0 &&
               y * (wx0 + hx0) - x * (wy0 + hy0) <= hy0 * wx0 - hx0 * wy0)
        {
            return true;
        }
        else if (x * (wx0 - hx0) - y * (hy0 - wy0) > hx0 * (wx0 - hx0) - hy0 * (hy0 - wy0)
               && x * (wx0 - hx0) - y * (hy0 - wy0) < wx0 * (wx0 - hx0) - wy0 * (hy0 - wy0)
               && (x * (hy0 - wy0) + y * (wx0 - hx0) - hy0 * wx0 + hx0 * wy0) * (x * (hy0 - wy0) + y * (wx0 - hx0) - hy0 * wx0 + hx0 * wy0)
               <= rr * ((wx0 - hx0) * (wx0 - hx0) + (wy0 - hy0) * (wy0 - hy0)))
        {
            return true;
        }
        else if (x * (wx0 + hx0) + y * (wy0 + hy0) > -wx0 * (wx0 + hx0) - wy0 * (wy0 + hy0)
               && x * (wx0 + hx0) + y * (wy0 + hy0) < hx0 * (wx0 + hx0) + hy0 * (wy0 + hy0)
               && (y * (wx0 + hx0) - x * (wy0 + hy0) - hy0 * wx0 + hx0 * wy0) * (y * (wx0 + hx0) - x * (wy0 + hy0) - hy0 * wx0 + hx0 * wy0)
               <= rr * ((wx0 + hx0) * (wx0 + hx0) + (wy0 + hy0) * (wy0 + hy0)))
        {
            return true;
        }
        else
        {
            if ((hx0 - wx0 - x) * (hx0 - wx0 - x) + (hy0 - wy0 - y) * (hy0 - wy0 - y) <= rr)
            {
                return Iterate(x, y, hx0, hy0, -wx0, -wy0, rr);
            }
            else if ((hx0 + wx0 - x) * (hx0 + wx0 - x) + (hy0 + wy0 - y) * (hy0 + wy0 - y) <= rr)
            {
                return Iterate(x, y, wx0, wy0, hx0, hy0, rr);
            }
            else if ((wx0 - hx0 - x) * (wx0 - hx0 - x) + (wy0 - hy0 - y) * (wy0 - hy0 - y) <= rr)
            {
                return Iterate(x, y, -hx0, -hy0, wx0, wy0, rr);
            }
            else if ((-wx0 - hx0 - x) * (-wx0 - hx0 - x) + (-wy0 - hy0 - y) * (-wy0 - hy0 - y) <= rr)
            {
                return Iterate(x, y, -wx0, -wy0, -hx0, -hy0, rr);
            }
            else if (wx0 * y - wy0 * x < wx0 * hy0 - wy0 * hx0 && Mathf.Abs(hx0 * y - hy0 * x) < hy0 * wx0 - hx0 * wy0)
            {
                if (hx0 * y - hy0 * x > 0)
                {
                    return Iterate(x, y, hx0, hy0, -wx0, -wy0, rr);
                }
                return Iterate(x, y, wx0, wy0, hx0, hy0, rr);
            }
            else if (wx0 * x + wy0 * y > wx0 * (hx0 - wx0) + wy0 * (hy0 - wy0) && wx0 * x + wy0 * y < wx0 * (hx0 + wx0) + wy0 * (hy0 + wy0)
               && (wx0 * y - wy0 * x - hy0 * wx0 + hx0 * wy0) * (wx0 * y - wy0 * x - hy0 * wx0 + hx0 * wy0) < rr * (wx0 * wx0 + wy0 * wy0))
            {
                if (wx0 * x + wy0 * y > wx0 * hx0 + wy0 * hy0)
                {
                    return Iterate(x, y, wx0, wy0, hx0, hy0, rr);
                }
                return Iterate(x, y, hx0, hy0, -wx0, -wy0, rr);
            }
            else
            {
                if (hx0 * y - hy0 * x < 0)
                {
                    x = -x;
                    y = -y;
                }
                if (hx0 * x + hy0 * y > -hx0 * (wx0 + hx0) - hy0 * (wy0 + hy0) && hx0 * x + hy0 * y < hx0 * (hx0 - wx0) + hy0 * (hy0 - wy0)
                    && (hx0 * y - hy0 * x - hy0 * wx0 + hx0 * wy0) * (hx0 * y - hy0 * x - hy0 * wx0 + hx0 * wy0) < rr * (hx0 * hx0 + hy0 * hy0))
                {
                    if (hx0 * x + hy0 * y > -hx0 * wx0 - hy0 * wy0)
                    {
                        return Iterate(x, y, hx0, hy0, -wx0, -wy0, rr);
                    }
                    return Iterate(x, y, -wx0, -wy0, -hx0, -hy0, rr);
                }
                return false;
            }
        }
    }
    public static bool CollideEllipses(Ellipse e1, Ellipse e2, out Vector2 collisionPoint)
    {
        return CollidePolygones(e1.ToPolygone(), e2.ToPolygone(), out collisionPoint);
    }
    public static bool CollideEllipses(Ellipse e1, Ellipse e2, out Vector2 collisionPoint, out Vector2 normal1, out Vector2 normal2)
    {
        if(CollideEllipses(e1, e2, out collisionPoint))
        {
            e1.Normal(collisionPoint, out normal1);
            e2.Normal(collisionPoint, out normal2);
            return true;
        }
        collisionPoint = normal1 = normal2 = Vector2.zero;
        return false;
    }

    public static bool CollideEllipseLine(Ellipse e1, in Vector2 A, in Vector2 B)
    {
        if(e1.Contains(A) || e1.Contains(B))
            return true;

        //on se ramène dans le cas ou l'ellipse est horizontale
        Vector2 center = e1.center;
        float angle = Useful.AngleHori(e1.center, e1.focus1.x >= e1.focus2.x ? e1.focus1 : e1.focus2);
        e1.Rotate(-angle);
        float length = center.Distance(A);
        float tmp = Useful.AngleHori(center, A);
        Vector2 A2 = new Vector2(center.x + length * Mathf.Cos(tmp - angle), center.y + length * Mathf.Sin(tmp - angle));
        length = center.Distance(B);
        tmp = Useful.AngleHori(center, B);
        Vector2 B2 = new Vector2(center.x + length * Mathf.Cos(tmp - angle), center.y + length * Mathf.Sin(tmp - angle));

        if (Mathf.Abs(A2.x - B2.x) <= accuracy)
        {
            float x = (A.x + B.x) * 0.5f;
            float xMin = center.x - e1.majorAxis * 0.5f;
            float xMax = center.x + e1.majorAxis * 0.5f;
            float yMin = center.y - e1.minorAxis * 0.5f;
            float yMax = center.y + e1.minorAxis * 0.5f;

            if (x >= xMin && x <= xMax && Mathf.Min(A2.y, B2.y) <= yMax && Mathf.Max(A2.y, B2.y) >= yMin)
            {
                e1.Rotate(angle);
                return true;
            }
            e1.Rotate(angle);
            return false;
        }
        else
        {
            float d = (B2.y - A2.y) / (B2.x - A2.x);
            float e = A2.y - (d * A2.x);
            float cx = center.x, cy = center.y, a = e1.majorAxis * 0.5f;
            float aa = a * a, b = e1.minorAxis * 0.5f;
            float bb = b * b;

            float C = bb * cx * cx + aa * e * e - 2f * aa * e * cy + aa * cy * cy - aa * bb;
            float Bcoeff = -2f * bb * cx + 2f * aa * d * e - 2f * aa * d * cy;
            float Acoeff = bb + aa * d * d;
            float delta = Bcoeff * Bcoeff - 4f * Acoeff * C;

            if (delta < 0f)
            {
                e1.Rotate(angle);
                return false;
            }
            if (Mathf.Abs(delta) <= accuracy)
            {
                float x = (-Bcoeff) / (2f * Acoeff);
                float y = d * x + e;
                if (x >= Mathf.Min(A2.x, B2.x) && x <= Mathf.Max(A2.x, B2.x) && y >= Mathf.Min(A2.y, B2.y) && y <= Mathf.Max(A2.y, B2.y))
                {
                    e1.Rotate(angle);
                    return true;
                }
                e1.Rotate(angle);
                return false;
            }
            //delta > 0
            delta = Mathf.Sqrt(delta);//use delta => sqrtDelta
            float x1 = (-Bcoeff - delta) / (2f * Acoeff);
            float x2 = (-Bcoeff + delta) / (2f * Acoeff);

            Vector2 col1 = new Vector2(x1, d * x1 + e);
            Vector2 col2 = new Vector2(x2, d * x2 + e);

            a = Mathf.Min(A2.x, B2.x);
            b = Mathf.Max(A2.x, B2.x);
            aa = Mathf.Min(A2.y, B2.y);
            bb = Mathf.Max(A2.y, B2.y);

            bool b1 = col1.x >= a && col1.x <= b && col1.y >= aa && col1.y <= bb;
            bool b2 = col2.x >= a && col2.x <= b && col2.y >= aa && col2.y <= bb;
            e1.Rotate(angle);
            return b1 || b2;
        }
    }
    public static bool CollideEllipseLine(Ellipse e1, Line l) => CollideEllipseLine(e1, l.A, l.B);
    public static bool CollideEllipseLine(Ellipse e1, in Vector2 A, in Vector2 B, out Vector2 collisionPoint)//OK
    {
        //on se ramène dans le cas ou l'ellipse est horizontale
        Vector2 center = e1.center;
        float angle = Useful.AngleHori(e1.center, e1.focus1.x >= e1.focus2.x ? e1.focus1 : e1.focus2);
        e1.Rotate(-angle);
        float length = center.Distance(A);
        float tmp = Useful.AngleHori(center, A);
        Vector2 A2 = new Vector2(center.x + length * Mathf.Cos(tmp - angle), center.y + length * Mathf.Sin(tmp - angle));
        length = center.Distance(B);
        tmp = Useful.AngleHori(center, B);
        Vector2 B2 = new Vector2(center.x + length * Mathf.Cos(tmp - angle), center.y + length * Mathf.Sin(tmp - angle));

        if (Mathf.Abs(A2.x - B2.x) <= accuracy)
        {
            float x = (A.x + B.x) * 0.5f;
            float xMin = center.x - e1.majorAxis * 0.5f;
            float xMax = center.x + e1.majorAxis * 0.5f;
            float yMin = center.y - e1.minorAxis * 0.5f;
            float yMax = center.y + e1.minorAxis * 0.5f;

            if (x >= xMin && x <= xMax && Mathf.Min(A2.y, B2.y) <= yMax && Mathf.Max(A2.y, B2.y) >= yMin)
            {
                tmp = angle + (x >= center.x ? 0f : Mathf.PI);
                collisionPoint = new Vector2(center.x + e1.majorAxis * 0.5f * Mathf.Cos(tmp), center.y + e1.majorAxis * 0.5f * Mathf.Sin(tmp)); ;
                e1.Rotate(angle);
                return true;
            }

            collisionPoint = Vector2.zero;
            e1.Rotate(angle);
            return false;
        }
        else
        {
            float d = (B2.y - A2.y) / (B2.x - A2.x);
            float e = A2.y - (d * A2.x);
            float cx = center.x, cy = center.y, a = e1.majorAxis * 0.5f;
            float aa = a * a, b = e1.minorAxis * 0.5f;
            float bb = b * b;

            float C = bb * cx * cx + aa * e * e - 2f * aa * e * cy + aa * cy * cy - aa * bb;
            float Bcoeff = -2f * bb * cx + 2f * aa * d * e - 2f * aa * d * cy;
            float Acoeff = bb + aa * d * d;
            float delta = Bcoeff * Bcoeff - 4f * Acoeff * C;

            if (delta < 0f)
            {
                collisionPoint = Vector2.zero;
                e1.Rotate(angle);
                return false;
            }
            if (Mathf.Abs(delta) <= accuracy && delta >= 0f)
            {
                float x = (-Bcoeff) / (2f * Acoeff);
                float y = d * x + e;
                collisionPoint = new Vector2(x, y);
                if(collisionPoint.x >= Mathf.Min(A2.x, B2.x) && collisionPoint.x <= Mathf.Max(A2.x, B2.x) && collisionPoint.y >= Mathf.Min(A2.y, B2.y) && collisionPoint.y <= Mathf.Max(A2.y, B2.y))
                {
                    a = center.Distance(collisionPoint);//use a => dist
                    aa = Useful.AngleHori(center, collisionPoint) + angle;//use aa => tmpAngle
                    collisionPoint = new Vector2(center.x + a * Mathf.Cos(aa), center.y + a * Mathf.Sin(aa));
                    e1.Rotate(angle);
                    return true;
                }
                e1.Rotate(angle);
                collisionPoint = Vector2.zero;
                return false;
            }
            //delta > 0
            delta = Mathf.Sqrt(delta);//use delta => sqrtDelta
            float x1 = (-Bcoeff - delta) / (2f * Acoeff);
            float x2 = (-Bcoeff + delta) / (2f * Acoeff);

            Vector2 col1 = new Vector2(x1, d * x1 + e);
            Vector2 col2 = new Vector2(x2, d * x2 + e);

            a = Mathf.Min(A2.x, B2.x);
            b = Mathf.Max(A2.x, B2.x);
            aa = Mathf.Min(A2.y, B2.y);
            bb = Mathf.Max(A2.y, B2.y);

            bool b1 = col1.x >= a && col1.x <= b && col1.y >= aa && col1.y <= bb;
            bool b2 = col2.x >= a && col2.x <= b && col2.y >= aa && col2.y <= bb;

            if(b1 && ! b2)
            {
                collisionPoint = col1;
                a = center.Distance(collisionPoint);//use a => dist
                aa = Useful.AngleHori(center, collisionPoint) + angle;//use aa => tmpAngle
                collisionPoint = new Vector2(center.x + a * Mathf.Cos(aa), center.y + a * Mathf.Sin(aa));
                e1.Rotate(angle);
                return true;
            }
            if (!b1 && b2)
            {
                collisionPoint = col2;
                a = center.Distance(collisionPoint);//use a => dist
                aa = Useful.AngleHori(center, collisionPoint) + angle;//use aa => tmpAngle
                collisionPoint = new Vector2(center.x + a * Mathf.Cos(aa), center.y + a * Mathf.Sin(aa));
                e1.Rotate(angle);
                return true;
            }
            if(!b1 && !b2)
            {
                collisionPoint = Vector2.zero;
                e1.Rotate(angle);
                return false;
            }
            //b1 == b2 == true
            Vector2 newA = (col1 + col2) * 0.5f;
            Vector2 dir = (col2 - col1).NormalVector();
            if (dir.Dot(newA - center) < 0f)
                dir *= -1f;

            Vector2 newB = newA + e1.majorAxis * dir;
            
            CollideEllipseLine(e1, newA, newB, out collisionPoint);

            a = center.Distance(collisionPoint);//use a => dist
            aa = Useful.AngleHori(center, collisionPoint) + angle;//use aa => tmpAngle
            collisionPoint = new Vector2(center.x + a * Mathf.Cos(aa), center.y + a * Mathf.Sin(aa));
            e1.Rotate(angle);
            return true;
        }
    }
    public static bool CollideEllipseLine(Ellipse e1, Line l, out Vector2 collisionPoint) => CollideEllipseLine(e1, l.A, l.B, out collisionPoint);
    public static bool CollideEllipseLine(Ellipse e1, in Vector2 A, in Vector2 B, out Vector2 collisionPoint, out Vector2 normal)//OK
    {
        if(CollideEllipseLine(e1, A, B, out collisionPoint))
        {
            e1.Normal(collisionPoint, out normal);
            return true;
        }
        collisionPoint = normal = Vector2.zero;
        return false;
    }
    public static bool CollideEllipseLine(Ellipse e1, Line l, out Vector2 collisionPoint, out Vector2 normal) => CollideEllipseLine(e1, l.A, l.B, out collisionPoint, out normal);

    public static bool CollideEllipseDroite(Ellipse e1, in Vector2 A, in Vector2 B)//ok
    {
        //on se ramène dans le cas ou l'ellipse est horizontale
        Vector2 center = e1.center;
        float angle = Useful.AngleHori(e1.center, e1.focus1.x >= e1.focus2.x ? e1.focus1 : e1.focus2);
        e1.Rotate(-angle);
        float length = center.Distance(A);
        float tmp = Useful.AngleHori(center, A);
        Vector2 A2 = new Vector2(center.x + length * Mathf.Cos(tmp - angle), center.y + length * Mathf.Sin(tmp - angle));
        length = center.Distance(B);
        tmp = Useful.AngleHori(center, B);
        Vector2 B2 = new Vector2(center.x + length * Mathf.Cos(tmp - angle), center.y + length * Mathf.Sin(tmp - angle));

        if (Mathf.Abs(A2.x - B2.x) <= accuracy)
        {
            float x = (A.x + B.x) * 0.5f;
            float xMin = center.x - e1.majorAxis * 0.5f;
            float xMax = center.x + e1.majorAxis * 0.5f;

            e1.Rotate(angle);
            return x >= xMin && x <= xMax;
        }
        else
        {
            float d = (B2.y - A2.y) / (B2.x - A2.x);
            float e = A2.y - (d * A2.x);
            float cx = center.x, cy = center.y, a = e1.majorAxis * 0.5f;
            float aa = a * a, b = e1.minorAxis * 0.5f;
            float bb = b * b;

            float C = bb * cx * cx + aa * e * e - 2f * aa * e * cy + aa * cy * cy - aa * bb;
            float Bcoeff = -2f * bb * cx + 2f * aa * d * e - 2f * aa * d * cy;
            float Acoeff = bb + aa * d * d;
            float delta = Bcoeff * Bcoeff - 4f * Acoeff * C;

            e1.Rotate(angle);
            return delta >= 0f;
        }
    }
    public static bool CollideEllipseDroite(Ellipse e1, Droite d) => CollideEllipseDroite(e1, d.A, d.B);
    public static bool CollideEllipseDroite(Ellipse e1, in Vector2 A, in Vector2 B, out Vector2 collisionPoint)//OK
    {
        //on se ramène dans le cas ou l'ellipse est horizontale
        Vector2 center = e1.center;
        float angle = Useful.AngleHori(e1.center, e1.focus1.x >= e1.focus2.x ? e1.focus1 : e1.focus2);
        e1.Rotate(-angle);
        float length = center.Distance(A);
        float tmp = Useful.AngleHori(center, A);
        Vector2 A2 = new Vector2(center.x + length * Mathf.Cos(tmp - angle), center.y + length * Mathf.Sin(tmp - angle));
        length = center.Distance(B);
        tmp = Useful.AngleHori(center, B);
        Vector2 B2 = new Vector2(center.x + length * Mathf.Cos(tmp - angle), center.y + length * Mathf.Sin(tmp - angle));

        if (Mathf.Abs(A2.x - B2.x) <= accuracy)
        {
            float x = (A.x + B.x) * 0.5f;
            float xMin = center.x - e1.majorAxis * 0.5f;
            float xMax = center.x + e1.majorAxis * 0.5f;

            if (x >= xMin && x <= xMax)
            {
                tmp = angle + (x >= center.x ? 0f : Mathf.PI);
                collisionPoint = new Vector2(center.x + e1.majorAxis * 0.5f * Mathf.Cos(tmp), center.y + e1.majorAxis * 0.5f * Mathf.Sin(tmp)); ;
                e1.Rotate(angle);
                return true;
            }

            collisionPoint = Vector2.zero;
            e1.Rotate(angle);
            return false;
        }
        else
        {
            float d = (B2.y - A2.y) / (B2.x - A2.x);
            float e = A2.y - (d * A2.x);
            float cx = center.x, cy = center.y, a = e1.majorAxis * 0.5f;
            float aa = a * a, b = e1.minorAxis * 0.5f;
            float bb = b * b;

            float C = bb * cx * cx + aa * e * e - 2f * aa * e * cy + aa * cy * cy - aa * bb;
            float Bcoeff = -2f * bb * cx + 2f * aa * d * e - 2f * aa * d * cy;
            float Acoeff = bb + aa * d * d;
            float delta = Bcoeff * Bcoeff - 4f * Acoeff * C;

            if (delta < 0f)
            {
                collisionPoint = Vector2.zero;
                e1.Rotate(angle);
                return false;
            }
            if (Mathf.Abs(delta) <= accuracy && delta >= 0f)
            {
                float x = (-Bcoeff) / (2f * Acoeff);
                float y = d * x + e;
                collisionPoint = new Vector2(x, y);
                a = center.Distance(collisionPoint);//use a => dist
                aa = Useful.AngleHori(center, collisionPoint) + angle;//use aa => tmpAngle
                collisionPoint = new Vector2(center.x + a * Mathf.Cos(aa), center.y + a * Mathf.Sin(aa));
                e1.Rotate(angle);
                return false;
            }
            //delta > 0
            delta = Mathf.Sqrt(delta);//use delta => sqrtDelta
            float x1 = (-Bcoeff - delta) / (2f * Acoeff);
            float x2 = (-Bcoeff + delta) / (2f * Acoeff);

            Vector2 col1 = new Vector2(x1, d * x1 + e);
            Vector2 col2 = new Vector2(x2, d * x2 + e);

            Vector2 newA = (col1 + col2) * 0.5f;
            Vector2 dir = (col2 - col1).NormalVector();
            if (dir.Dot(newA - center) < 0f)
                dir *= -1f;

            Vector2 newB = newA + e1.majorAxis * dir;

            CollideEllipseLine(e1, newA, newB, out collisionPoint);

            a = center.Distance(collisionPoint);//use a => dist
            aa = Useful.AngleHori(center, collisionPoint) + angle;//use aa => tmpAngle
            collisionPoint = new Vector2(center.x + a * Mathf.Cos(aa), center.y + a * Mathf.Sin(aa));
            e1.Rotate(angle);
            return true;
        }
    }
    public static bool CollideEllipseDroite(Ellipse e1, Droite d, out Vector2 collisionPoint) => CollideEllipseDroite(e1, d.A, d.B, out collisionPoint);
    public static bool CollideEllipseDroite(Ellipse e1, in Vector2 A, in Vector2 B, out Vector2 collisionPoint, out Vector2 normal)//OK
    {
        if(CollideEllipseDroite(e1, A, B, out collisionPoint))
        {
            e1.Normal(collisionPoint, out normal);
            return true;
        }
        collisionPoint = normal = Vector2.zero;
        return false;
    }
    public static bool CollideEllipseDroite(Ellipse e1, Droite d, out Vector2 collisionPoint, out Vector2 normal) => CollideEllipseDroite(e1, d.A, d.B, out collisionPoint, out normal);

    #endregion

    #region Collide(Lines, Droite)

    public static bool CollideDroites(Droite d1, Droite d2) => CollideDroites(d1.A, d1.B, d2.A, d2.B);//OK
    public static bool CollideDroites(Droite d1, Droite d2, out Vector2 collisionPoint) => CollideDroites(d1.A, d1.B, d2.A, d2.B, out collisionPoint);//OK
    public static bool CollideDroites(in Vector2 A, in Vector2 B, in Vector2 O, in Vector2 P)
    {
        return !(B - A).IsCollinear(P - O);
    }//OK
    public static bool CollideDroites(in Vector2 A, in Vector2 B, in Vector2 O, in Vector2 P, out Vector2 collisionPoint)
    {
        //on regarde si une des droites est verticale
        if (Mathf.Abs(B.x - A.x) <= accuracy || Mathf.Abs(P.x - O.x) <= accuracy)
        {
            //si les 2 sont verticales
            if (Mathf.Abs(B.x - A.x) <= accuracy && Mathf.Abs(P.x - O.x) <= accuracy)
            {
                if (Mathf.Abs(((A.x + B.x) * 0.5f) - ((O.x + P.x) * 0.5f)) > accuracy)
                {
                    collisionPoint = Vector2.zero;
                    return false;
                }
                collisionPoint = new Vector2((O.x + P.x + A.x + B.x) * 0.25f, (O.y + P.y + A.y + B.y) * 0.25f);
                return true;
            }
            float a, b, ySol;
            if (Mathf.Abs(B.x - A.x) <= accuracy)//(AB) verticale mais pas (OP)
            {
                a = (P.y - O.y) / (P.x - O.x);
                b = O.y - a * O.x;
                ySol = a * ((A.x + B.x) * 0.5f) + b;
                collisionPoint = new Vector2((A.x + B.x) * 0.5f, ySol);
                return true;
            }
            // on sait que (OP) verticale mais pas (AB)
            a = (B.y - A.y) / (B.x - A.x);
            b = A.y - a * A.x;
            ySol = a * ((O.x + P.x) * 0.5f) + b;
            collisionPoint = new Vector2((O.x + P.x) * 0.5f, ySol);
            return true;
        }

        //on regarde si une des droites est horizontale
        if (Mathf.Abs(A.y - B.y) <= accuracy || Mathf.Abs(O.y - P.y) <= accuracy)
        {
            if (Mathf.Abs(A.y - B.y) <= accuracy && Mathf.Abs(O.y - P.y) <= accuracy)//les 2 droites sont horizontales
            {
                if (Mathf.Abs(((A.y + B.y) * 0.5f) - ((O.y + P.y) * 0.5f)) <= accuracy)
                {
                    collisionPoint = new Vector2((A.x + B.x + O.x + P.x) * 0.25f, (A.y + B.y + O.y + P.y) * 0.25f);
                    return true;
                }
                collisionPoint = Vector2.zero;
                return false;
            }
            float a, b, xSol;
            if (Mathf.Abs(A.y - B.y) <= accuracy)//droite AB horizontal, seg OP non horizontal
            {
                a = (P.y - O.y) / (P.x - O.x);
                b = O.y - a * O.x;
                xSol = (((A.y + B.y) * 0.5f) - b) / a;
                collisionPoint = new Vector2(xSol, (A.y + B.y) * 0.5f);
                return true;
            }
            //OP horizontale
            a = (B.y - A.y) / (B.x - A.x);
            b = A.y - a * A.x;
            xSol = (((O.y + P.y) * 0.5f) - b) / a;
            collisionPoint = new Vector2(xSol, (O.y + P.y) * 0.5f);
            return true;
        }

        //les 2 droites sont quelconques (pas horizontales ni verticales)
        float a1, b1, a2, b2;
        //equetion de la droite (A,B)
        a1 = (B.y - A.y) / (B.x - A.x);
        b1 = A.y - a1 * A.x;
        //equation du droite (O,P)
        a2 = (P.y - O.y) / (P.x - O.x);
        b2 = O.y - a2 * O.x;
        //On regarde si les 2 sont !//
        if (Mathf.Abs(a1 - a2) >= 0.0025f)
        {
            float xSol = (b2 - b1) / (a1 - a2);
            float ySol = ((a1 * xSol) + b1 + (a2 * xSol) + b2) * 0.5f;
            collisionPoint = new Vector2(xSol, ySol);
            return true;
        }
        else if (Mathf.Abs(b2 - b1) <= accuracy)
        {
            collisionPoint = new Vector2((A.x + B.x + O.x + P.x) / 4f, ((a1 + a2) * 0.5f) * ((A.x + B.x + O.x + P.x) / 4f) + ((b1 + b2) * 0.5f));
            return true;
        }
        collisionPoint = Vector2.zero;
        return false;
    }
    public static bool CollideLines(Line l1, Line l2) => CollideLines(l1.A, l1.B, l2.A, l2.B);//OK
    public static bool CollideLines(in Vector2 A, in Vector2 B, in Vector2 O, in Vector2 P)//OK
    {
        return CollideLineDroite(A, B, O, P) && CollideLineDroite(O, P, A, B);
    }
    public static bool CollideLines(Line l1, Line l2, out Vector2 collisionPoint) => CollideLines(l1.A, l1.B, l2.A, l2.B, out collisionPoint);//OK
    public static bool CollideLines(in Vector2 A, in Vector2 B, in Vector2 O, in Vector2 P, out Vector2 collisionPoint)//OK
    {
        //on regarde si un des 2 segments est vertical
        if (Mathf.Abs(B.x - A.x) <= accuracy || Mathf.Abs(P.x - O.x) <= accuracy)
        {
            //si les 2 sont verticals
            if (Mathf.Abs(B.x - A.x) <= accuracy && Mathf.Abs(P.x - O.x) <= accuracy)
            {
                if (Mathf.Abs(A.x - O.x) > accuracy)
                {
                    collisionPoint = Vector2.zero;
                    return false;
                }
                float minDesMax = Mathf.Min(Mathf.Max(A.y, B.y), Mathf.Max(O.y, P.y));
                float maxDesMin = Mathf.Max(Mathf.Min(A.y, B.y), Mathf.Min(O.y, P.y));
                if(minDesMax >= maxDesMin)
                {
                    collisionPoint = new Vector2((A.x + B.x + O.x + P.x) * 0.25f, (maxDesMin + minDesMax) * 0.5f);
                    return true;
                }
                collisionPoint = Vector2.zero;
                return false;
            }
            float a, b, ySol;
            if (Mathf.Abs(B.x - A.x) <= accuracy)//AB vertical mais pas OP
            {
                if (!(((A.x + B.x) * 0.5f) >= Mathf.Min(O.x, P.x) && ((A.x + B.x) * 0.5f) <= Mathf.Max(O.x, P.x)))
                {
                    collisionPoint = Vector2.zero;
                    return false;
                }
                a = (P.y - O.y) / (P.x - O.x);
                b = O.y - a * O.x;
                ySol = a * ((A.x + B.x) * 0.5f) + b;
                if(Mathf.Min(A.y, B.y) <= ySol && Mathf.Max(A.y, B.y) >= ySol)
                {
                    collisionPoint = new Vector2((A.x + B.x) * 0.5f, ySol);
                    return true;
                }
                collisionPoint = Vector2.zero;
                return false;
            }
            // on sait que [OP] vertical
            if (!(((O.x + P.x) * 0.5f) >= Mathf.Min(A.x, B.x) && ((O.x + P.x) * 0.5f) <= Mathf.Max(A.x, B.x)))
            {
                collisionPoint = Vector2.zero;
                return false;
            }
            a = (B.y - A.y) / (B.x - A.x);
            b = A.y - a * A.x;
            ySol = a * ((O.x + P.x) * 0.5f) + b;
            if(Mathf.Min(O.y, P.y) <= ySol && Mathf.Max(O.y, P.y) >= ySol)
            {
                collisionPoint = new Vector2((O.x + P.x) * 0.5f, ySol);
                return true;
            }
            collisionPoint = Vector2.zero;
            return false;
        }
        //on regarde si un des 2 segment est horizontale
        if(Mathf.Abs(A.y - B.y) <= accuracy || Mathf.Abs(O.y - P.y) <= accuracy)
        {
            if(Mathf.Abs(A.y - B.y) < 1f && Mathf.Abs(O.y - P.y) < 1f)//les 2 segments sont horizontaux
            {
                if(Mathf.Abs(((A.y + B.y) * 0.5f) - ((O.y + P.y) * 0.5f)) <= accuracy)
                {
                    float minDesMax = Mathf.Min(Mathf.Max(A.x, B.x), Mathf.Max(O.x, P.x));
                    float maxDesMin = Mathf.Max(Mathf.Min(A.x, B.x), Mathf.Min(O.x, P.x));
                    if(minDesMax >= maxDesMin)//si il y a collision
                    {
                        collisionPoint = new Vector2((maxDesMin + minDesMax) * 0.5f, (A.y + B.y + O.y + P.y) * 0.25f);
                        return true;
                    }
                }
                collisionPoint = Vector2.zero;
                return false;
            }
            float a, b, xSol;
            if(Mathf.Abs(A.y - B.y) <= accuracy)//AB horizontal, OP non horizontal
            {
                a = (P.y - O.y) / (P.x - O.x);
                b = O.y - a * O.x;
                xSol = (((A.y + B.y) * 0.5f) - b) / a;
                if(Mathf.Min(A.x, B.x) <= xSol && Mathf.Max(A.x, B.x) >= xSol && Mathf.Min(O.x, P.x) <= xSol && Mathf.Max(O.x, P.x) >= xSol)
                {
                    collisionPoint = new Vector2(xSol, (A.y + B.y) * 0.5f);
                    return true;
                }
                collisionPoint = Vector2.zero;
                return false;
            }
            //OP horizontale
            a = (B.y - A.y) / (B.x - A.x);
            b = A.y - a * A.x;
            xSol = (((O.y + P.y) * 0.5f) - b) / a;
            if (Mathf.Min(O.x, P.x) <= xSol && Mathf.Max(O.x, P.x) >= xSol && Mathf.Min(A.x, B.x) <= xSol && Mathf.Max(A.x, B.x) >= xSol)
            {
                collisionPoint = new Vector2(xSol, (O.y + P.y) * 0.5f);
                return true;
            }
            collisionPoint = Vector2.zero;
            return false;
        }

        //les 2 segments sont quelconques (pas horizontaux ni verticaux)
        float a1, b1, a2, b2;
        //equetion de la droite (A,B)
        a1 = (B.y - A.y) / (B.x - A.x);
        b1 = A.y - a1 * A.x;
        //equation de la droite (O,P)
        a2 = (P.y - O.y) / (P.x - O.x);
        b2 = O.y - a2 * O.x;
        //On regarde si les 2 segment sont !//
        if (Mathf.Abs(a1 - a2) >= 0.001f)
        {
            float xSol = (b2 - b1) / (a1 - a2);
            float ySol = ((a1 * xSol) + b1 + (a2 * xSol) + b2) * 0.5f;
            if (Mathf.Min(A.x, B.x) <= xSol && Mathf.Max(A.x, B.x) >= xSol && Mathf.Min(A.y, B.y) <= ySol && Mathf.Max(A.y, B.y) >= ySol
                && Mathf.Min(O.x, P.x) <= xSol && Mathf.Max(O.x, P.x) >= xSol && Mathf.Min(O.y, P.y) <= ySol && Mathf.Max(O.y, P.y) >= ySol)
            {
                collisionPoint = new Vector2(xSol, ySol);
                return true;
            }
            collisionPoint = Vector2.zero;
            return false;
        }
        else if(Mathf.Abs(b2 - b1) < 1f)
        {
            collisionPoint = new Vector2((A.x + B.x + O.x + P.x) * 0.25f, ((a1 + a2) * 0.5f) * ((A.x + B.x + O.x + P.x) * 0.25f) + ((b1 + b2) * 0.5f));
            return true;
        }
        collisionPoint = Vector2.zero;
        return false;
    }
    public static bool CollideLineDroite(Line l, Droite d) => CollideLineDroite(l.A, l.B, d.A, d.B);//OK
    public static bool CollideLineDroite(in Vector2 O, in Vector2 P, in Vector2 A, in Vector2 B)
    {
        Vector2 AB = B - A;
        Vector2 AP = P - A;
        Vector2 AO = O - A;
        return (AB.x * AP.y - AB.y * AP.x) * (AB.x * AO.y - AB.y * AO.x) < 0f;
    }//OK
    public static bool CollideLineDroite(in Vector2 O, in Vector2 P, in Vector2 A, in Vector2 B, out Vector2 collisionPoint)//OK
    {
        //on regarde si le segment ou la droite est vertical
        if (Mathf.Abs(B.x - A.x) <= accuracy || Mathf.Abs(P.x - O.x) <= accuracy)
        {
            //si les 2 sont verticals
            if (Mathf.Abs(B.x - A.x) <= accuracy && Mathf.Abs(P.x - O.x) <= accuracy)
            {
                if (Mathf.Abs(A.x - O.x) > accuracy)
                {
                    collisionPoint = Vector2.zero;
                    return false;
                }
                collisionPoint = new Vector2((O.x + P.x + A.x + B.x) * 0.25f, Mathf.Min(O.y, P.y) + Mathf.Abs(O.y - P.y) * 0.5f);
                return true;
            }
            float a, b, ySol;
            if (Mathf.Abs(B.x - A.x) <= accuracy)//AB vertical mais pas OP
            {
                if (!(((A.x + B.x) * 0.5f) >= Mathf.Min(O.x, P.x) && ((A.x + B.x) * 0.5f) <= Mathf.Max(O.x, P.x)))
                {
                    collisionPoint = Vector2.zero;
                    return false;
                }

                a = (P.y - O.y) / (P.x - O.x);
                b = O.y - a * O.x;
                ySol = a * ((A.x + B.x) * 0.5f) + b;
                collisionPoint = new Vector2((A.x + B.x) * 0.5f, ySol);
                return true;
            }
            // on sait que [OP] vertical

            a = (B.y - A.y) / (B.x - A.x);
            b = A.y - a * A.x;
            ySol = a * ((O.x + P.x) * 0.5f) + b;
            if (Mathf.Min(O.y, P.y) <= ySol && Mathf.Max(O.y, P.y) >= ySol)
            {
                collisionPoint = new Vector2((O.x + P.x) * 0.5f, ySol);
                return true;
            }
            collisionPoint = Vector2.zero;
            return false;
        }

        //on regarde si le seg ou la droite est horizontale
        if (Mathf.Abs(A.y - B.y) <= accuracy || Mathf.Abs(O.y - P.y) <= accuracy)
        {
            if (Mathf.Abs(A.y - B.y) < 1f && Mathf.Abs(O.y - P.y) < 1f)//le segment et la droite sont horizontaux
            {
                if (Mathf.Abs(((A.y + B.y) * 0.5f) - ((O.y + P.y) * 0.5f)) <= accuracy)
                {
                    collisionPoint = new Vector2((O.x + P.x) * 0.5f, (A.y + B.y + O.y + P.y) * 0.25f);
                    return true;
                }
                collisionPoint = Vector2.zero;
                return false;
            }
            float a, b, xSol;
            if (Mathf.Abs(A.y - B.y) <= accuracy)//droite AB horizontal, seg OP non horizontal
            {
                a = (P.y - O.y) / (P.x - O.x);
                b = O.y - a * O.x;
                xSol = (((A.y + B.y) * 0.5f) - b) / a;
                if (Mathf.Min(O.x, P.x) <= xSol && Mathf.Max(O.x, P.x) >= xSol)
                {
                    collisionPoint = new Vector2(xSol, (A.y + B.y) * 0.5f);
                    return true;
                }
                collisionPoint = Vector2.zero;
                return false;
            }
            //OP horizontale
            a = (B.y - A.y) / (B.x - A.x);
            b = A.y - a * A.x;
            xSol = (((O.y + P.y) * 0.5f) - b) / a;
            if (Mathf.Min(O.x, P.x) <= xSol && Mathf.Max(O.x, P.x) >= xSol)
            {
                collisionPoint = new Vector2(xSol, (O.y + P.y) * 0.5f);
                return true;
            }
            collisionPoint = Vector2.zero;
            return false;
        }

        //le segment et la droite sont quelconque (pas horizontal ni vertical)
        float a1, b1, a2, b2;
        //equetion de la droite (A,B)
        a1 = (B.y - A.y) / (B.x - A.x);
        b1 = A.y - a1 * A.x;
        //equation du segment [O,P]
        a2 = (P.y - O.y) / (P.x - O.x);
        b2 = O.y - a2 * O.x;
        //On regarde si les 2 sont !=//
        if (Mathf.Abs(a1 - a2) >= 0.001f)
        {
            float xSol = (b2 - b1) / (a1 - a2);
            float ySol = ((a1 * xSol) + b1 + (a2 * xSol) + b2) * 0.5f;
            if (Mathf.Min(O.x, P.x) <= xSol && Mathf.Max(O.x, P.x) >= xSol && Mathf.Min(O.y, P.y) <= ySol && Mathf.Max(O.y, P.y) >= ySol)
            {
                collisionPoint = new Vector2(xSol, ySol);
                return true;
            }
            collisionPoint = Vector2.zero;
            return false;
        }
        else if (Mathf.Abs(b2 - b1) <= accuracy)
        {
            collisionPoint = new Vector2((O.x + P.x) * 0.5f, ((a1 + a2) * 0.5f) * ((O.x + P.x) * 0.5f) + ((b1 + b2) * 0.5f));
            return true;
        }
        collisionPoint = Vector2.zero;
        return false;
    }
    public static bool CollideLineDroite(Line l, Droite d, out Vector2 collisionPoint) => CollideLineDroite(l.A, l.B, d.A, d.B, out collisionPoint);//OK

    #endregion

    #endregion

    public virtual Vector2 center
    {
        get => new Vector2();
        protected set { }
    }

    protected Circle _inclusiveCircle;
    public virtual Circle inclusiveCircle
    {
        get => _inclusiveCircle;
        protected set { _inclusiveCircle = value; }
    }

    public virtual CustomCollider Clone() => null;
    public virtual bool Collide(CustomCollider c) => false;
    public virtual bool CollideLine(Line l) => false;
    public virtual bool CollideLine(in Vector2 A, in Vector2 B) => false;
    public virtual bool CollideDroite(Droite d) => false;
    public virtual bool CollideDroite(in Vector2 A, in Vector2 B) => false;
    public virtual bool Contains(in Vector2 p) => false;
    public virtual void MoveAt(in Vector2 position) { }
    public virtual void Rotate(in float angle) { }
    public virtual Hitbox ToHitbox() => null;
    public virtual Circle ToCircle() => null;
    public virtual void SetScale(in Vector2 newScale, in Vector2 oldScale) { }
    public virtual bool Normal(in Vector2 point, out Vector2 normal) { normal = Vector2.zero; return false; }
}

#region Polygone

public class Polygone : CustomCollider
{
    public static void GizmosDraw(List<Vector2> points)
    {
        for(int i = 0; i < points.Count; i++)
        {
            Gizmos.DrawLine(points[i], points[(i + 1) % points.Count]);
        }
    }

    public static void GizmosDraw(Polygone p) => GizmosDraw(p.vertices);

    public List<Vector2> vertices { get; private set; }
    private Vector2 _center;

    public override Vector2 center
    {
        get => _center;
        protected set
        {
            _center = value;
        }
    }

    #region Builder

    public Polygone(List<Vector2> vertices)
    {
        this.vertices = vertices.Clone();
        for (int i = this.vertices.Count - 1; i >= 0; i--)
        {
            if (this.vertices[i] == this.vertices[(i+1)%this.vertices.Count])
            {
                this.vertices.RemoveAt(i);
            }
        }
        center = Vector2.zero;
        foreach (Vector2 pos in vertices)
        {
            center += pos;
        }
        center /= vertices.Count;
        inclusiveCircle = ToCircle();
    }

    public Polygone(List<Vector2> vertices, in Vector2 center)
    {
        this.vertices = vertices.Clone();
        for (int i = this.vertices.Count - 1; i >= 0; i--)
        {
            if (this.vertices[i] == this.vertices[(i + 1) % this.vertices.Count])
            {
                this.vertices.RemoveAt(i);
            }
        }
        this.center = center;
        inclusiveCircle = ToCircle();
    }

    public override CustomCollider Clone() => new Polygone(vertices.Clone(), center);

    #endregion

    public override bool Collide(CustomCollider c) => CustomCollider.Collide(c, this);

    #region Contain

    public override bool Contains(in Vector2 P)
    {
        if (vertices == null || vertices.Count < 3)
            return false;

        int i;
        Vector2 I = ExternalPoint();
        int nbintersections = 0;
        for (i = 0; i < vertices.Count; i++)
        {
            Vector2 A = vertices[i];
            Vector2 B = vertices[(i + 1) % vertices.Count];
            int iseg = Intersectsegment(A, B, I, P);
            if (iseg == -1)
                return Contains(P);  // cas limite, on relance la fonction.
            nbintersections += iseg;
        }
        return Useful.IsOdd(nbintersections);

        Vector2 ExternalPoint()
        {
            float maxDist = 0;
            for (int i = 0; i < vertices.Count; i++)
            {
                maxDist = Mathf.Max(center.SqrDistance(vertices[i]), maxDist);
            }
            return center + Random.Vector2(maxDist);
        }

        int Intersectsegment(Vector2 A, Vector2 B, Vector2 I, Vector2 P)
        {
            Vector2 D = B - A;
            Vector2 E = P - I;
            float denom = D.x * E.y - D.y * E.x;
            if (denom == 0f)
                return -1;   // erreur, cas limite
            float t = -(A.x * E.y - I.x * E.y - E.x * A.y + E.x * I.y) / denom;
            if (t < 0f || t >= 1f)
                return 0;
            float u = -(-D.x * A.y + D.x * I.y + D.y * A.x - D.y * I.x) / denom;
            if (u < 0f || u >= 1f)
                return 0;
            return 1;
        }
    }

    #endregion

    public override bool CollideLine(Line l) => CollideLine(l.A, l.B);
    public override bool CollideLine(in Vector2 A, in Vector2 B)
    {
        if (!CollideCircleLine(inclusiveCircle, A, B))
            return false;

        for (int i = 0; i < vertices.Count; i++)
        {
            if (CollideLines(A, B, vertices[i], vertices[(i + 1) % vertices.Count]))
            {
                return true;
            }
        }
        return Contains(A) || Contains(B);
    }

    public override bool CollideDroite(Droite d) => CollideDroite(d.A, d.B);
    public override bool CollideDroite(in Vector2 A, in Vector2 B)
    {
        if (!CollideCircleDroite(inclusiveCircle, A, B))
            return false;
        for (int i = 0; i < vertices.Count; i++)
        {
            if (CollideLineDroite(vertices[i], vertices[(i + 1) % vertices.Count], A, B))
            {
                return true;
            }
        }
        return false;
    }

    public override void MoveAt(in Vector2 position)
    {
        Vector2 oldCenter = center;
        center = position;
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] = center + (vertices[i] - oldCenter);
        }
        inclusiveCircle.MoveAt(position);
    }

    public override Hitbox ToHitbox()
    {
        float distMaxX = 0;
        float distMaxY = 0;
        for (int i = 0; i < vertices.Count; i++)
        {
            Vector2 delta = center - vertices[i];
            distMaxX = Mathf.Max(delta.x, distMaxX);
            distMaxY = Mathf.Max(delta.y, distMaxY);
        }
        return new Hitbox(center, new Vector2(distMaxX, distMaxY));
    }

    public override Circle ToCircle()
    {
        float maxDist = 0;
        for (int i = 0; i < vertices.Count; i++)
        {
            maxDist = Mathf.Max(center.Distance(vertices[i]), maxDist);
        }
        return new Circle(center, maxDist);
    }

    public override void SetScale(in Vector2 newScale, in Vector2 oldScale)
    {
        Vector2 ratio = newScale / oldScale;
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] = center - (center - vertices[i]) * ratio;
        }
        inclusiveCircle.SetScale(newScale, oldScale);
    }

    public override void Rotate(in float angle)
    {
        Vector2 O = center;
        for (int i = 0; i < vertices.Count; i++)
        {
            float distOP = vertices[i].Distance(O);
            float newAngle = Useful.AngleHori(O, vertices[i]) + angle;
            vertices[i] = new Vector2(O.x + distOP * Mathf.Cos(newAngle), O.y + distOP * Mathf.Sin(newAngle));
        }
        base.Rotate(angle);
    }

    public override bool Normal(in Vector2 point, out Vector2 normal)
    {
        int minIndex = 0;
        float minDist = Line.Distance(vertices[0], vertices[1], point);
        float tmpDist;
        for (int i = 1; i < vertices.Count; i++)
        {
            tmpDist = Line.Distance(vertices[i], vertices[(i + 1) % vertices.Count], point);
            if(tmpDist < minDist)
            {
                minDist = tmpDist;
                minIndex = i;
            }
        }

        Vector2 A = vertices[minIndex]; Vector2 B = vertices[(minIndex + 1) % vertices.Count];
        float sqrDist = A.SqrDistance(B);
        if(sqrDist < accuracy * accuracy)
        {
            return base.Normal(point, out normal);
        }
        normal = Line.Normal(A, B);
        float r = (((point.x - A.x) * (B.x - A.x)) + ((point.y - A.y) * (B.y - A.y))) / sqrDist;
        Vector2 P = A + r * (B - A);
        if (normal.Dot(center - P) > 0f)
        {
            normal *= -1f;
        }
        return true;
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder("Vertices number : ");
        sb.AppendLine(vertices.Count.ToString());
        sb.AppendLine("Vertices : ");
        for (int i = 0; i < vertices.Count - 1; i++)
        {
            sb.Append("    ");
            sb.Append(vertices[i].ToString());
            sb.AppendLine(",");
        }
        sb.Append("    ");
        sb.AppendLine(vertices[vertices.Count - 1].ToString());
        return sb.ToString();
    }
}

#endregion

#region Hitbox

public class Hitbox : CustomCollider
{
    public static void GizmosDraw(Hitbox hitbox) => Polygone.GizmosDraw(hitbox.rec);

    public Polygone rec;
    public Vector2 size;
    public override Circle inclusiveCircle => rec.inclusiveCircle;

    public Hitbox(in Vector2 center, in Vector2 size)
    {
        this.size = size;
        List<Vector2> vertices = new List<Vector2>
        {
            new Vector2(center.x - size.x * 0.5f, center.y - size.y * 0.5f),
            new Vector2(center.x + size.x * 0.5f, center.y - size.y * 0.5f),
            new Vector2(center.x + size.x * 0.5f, center.y + size.y * 0.5f),
            new Vector2(center.x - size.x * 0.5f, center.y + size.y * 0.5f)
        };
        rec = new Polygone(vertices, center);
    }

    public override void Rotate(in float angle)
    {
        if (Mathf.Abs(angle) > Mathf.Epsilon)
        {
            rec.Rotate(angle);
        }
    }

    public override void MoveAt(in Vector2 position)
    {
        rec.MoveAt(position);
    }

    public override bool Collide(CustomCollider c) => CustomCollider.Collide(c, this);
    public override bool CollideDroite(Droite d) => CollideHitboxDroite(this, d);
    public override bool CollideDroite(in Vector2 A, in Vector2 B) => CollideHitboxDroite(this, A, B);
    public override bool CollideLine(Line l) => CollideHitboxLine(this, l);
    public override bool CollideLine(in Vector2 A, in Vector2 B) => CollideHitboxLine(this, A, B);

    public override Hitbox ToHitbox() => this;
    public override void SetScale(in Vector2 newScale, in Vector2 oldScale)
    {
        rec.SetScale(newScale, oldScale);
        size *= newScale / oldScale;
        inclusiveCircle.SetScale(newScale, oldScale);
    }

    public override Vector2 center
    {
        get => rec.center;
        protected set
        {
            MoveAt(value);
        }
    }

    public override bool Contains(in Vector2 p) => rec.Contains(p);
    public override CustomCollider Clone() => new Hitbox(center, size);
    public override Circle ToCircle() => (Circle)rec.inclusiveCircle.Clone();
    public override string ToString() => rec.ToString();
    public override bool Normal(in Vector2 point, out Vector2 normal) => rec.Normal(point, out normal);
}

#endregion

#region Circle

public class Circle : CustomCollider
{
    public static void GizmosDraw(in Vector2 center, in float radius)
    {
        int sides = ((radius + 10f) * 7f).Round();
        float angleStep = (2f * Mathf.PI) / sides;
        Vector2 lastPoint = center + radius * Vector2.right;
        Vector2 p;
        for (int i = 1; i <= sides; i++)
        {
            float ang = i * angleStep;
            p = center + new Vector2(radius * Mathf.Cos(ang), radius * Mathf.Sin(ang));
            Gizmos.DrawLine(lastPoint, p);
            lastPoint = p;
        }
    }

    public static void GizmosDraw(in Vector2 center, float radius, float begAngle, float endAngle)
    {
        int sides = Math.Max(((radius + 10f) * 7f * Mathf.Abs((endAngle - begAngle) / (2f * Mathf.PI))).Round(), 4);
        float angleStep = Mathf.Abs(endAngle - begAngle) / sides;
        Vector2 lastPoint = center + Useful.Vector2FromAngle(begAngle, radius);
        Vector2 p;
        for (int i = 1; i <= sides; i++)
        {
            float ang = begAngle + i * angleStep;
            p = center + Useful.Vector2FromAngle(ang, radius);
            Gizmos.DrawLine(lastPoint, p);
            lastPoint = p;
        }
    }

    public static void GizmosDraw(Circle circle) => GizmosDraw(circle.center, circle.radius);

    protected Vector2 _center;
    public override Vector2 center
    {
        get => _center;
        protected set
        {
            _center = value;
        }
    }
    public float radius;
    public override Circle inclusiveCircle => this;

    //Pour le CustomCollider
    public Circle(in Vector2 center, in float radius)
    {
        this.center = center;
        this.radius = radius;
    }

    #region CollideLine

    public override bool CollideLine(Line l) => CollideLine(l.A, l.B);
    public override bool CollideLine(in Vector2 A, in Vector2 B)
    {
        Vector2 u = new Vector2(B.x - A.x, B.y - A.y);
        Vector2 AC = new Vector2(center.x - A.x, center.y - A.y);
        float CI = Mathf.Abs(u.x * AC.y - u.y * AC.x) / u.magnitude;
        if (CI > radius)
            return false;
        else
        {
            Vector2 AB = new Vector2(B.x - A.x, B.y - A.y);
            AC = new Vector2(center.x - A.x, center.y - A.y);
            Vector2 BC = new Vector2(center.x - B.x, center.y - B.y);
            float pscal1 = AB.x * AC.x + AB.y * AC.y;  // produit scalaire
            float pscal2 = (-AB.x) * BC.x + (-AB.y) * BC.y;  // produit scalaire
            if (pscal1 >= 0 && pscal2 >= 0)
                return true;   // I entre A et B, ok.
            // dernière possibilité, A ou B dans le cercle
            return center.SqrDistance(A) < radius * radius || center.SqrDistance(B) < radius * radius;
        }
    }
    public override bool CollideDroite(Droite d) => CollideDroite(d.A, d.B);
    public override bool CollideDroite(in Vector2 A, in Vector2 B)
    {
        Vector2 u = new Vector2(B.x - A.x, B.y - A.y);
        Vector2 AC = new Vector2(center.x - A.x, center.y - A.y);
        float numerateur = Mathf.Abs(u.x * AC.y - u.y * AC.x);// norme du vecteur v
        return numerateur / u.magnitude < radius;
    }

    #endregion

    public override bool Collide(CustomCollider c) => CustomCollider.Collide(c, this);
    public override Hitbox ToHitbox() => new Hitbox(center, new Vector2(radius, radius));
    public override Circle ToCircle() => (Circle)Clone();

    public override void MoveAt(in Vector2 position)
    {
        center = position;
    }

    public override void Rotate(in float angle) { }

    public override void SetScale(in Vector2 newScale, in Vector2 olsScale)
    {
        radius *=(((newScale.x + newScale.y) * 0.5f) / ((olsScale.x + olsScale.y) * 0.5f));
    }

    public override bool Contains(in Vector2 p) => center.SqrDistance(p) <= radius * radius;
    public override string ToString() => ("center : " + center.ToString() + " Radius : " + radius.ToString());
    public override CustomCollider Clone() => new Circle(center, radius);

    public override bool Normal(in Vector2 point, out Vector2 normal)
    {
        if(center.SqrDistance(point) - radius * radius <= accuracy * accuracy)
        {
            normal = (point - center).normalized;
            return true;
        }
        return base.Normal(point, out normal);
    }

    public static Circle operator *(Circle c, float f)
    {
        c.radius *= f;
        return c;
    }
    public static Circle operator *(Circle c, int f)
    {
        c.radius *= f;
        return c;
    }
}

#endregion

#region Capsule

public class Capsule : CustomCollider
{
    public static void GizmosDraw(Capsule capsule)
    {
        Circle.GizmosDraw(capsule.c1);
        Circle.GizmosDraw(capsule.c2);
        Hitbox.GizmosDraw(capsule.hitbox);
    }

    public Circle c1, c2;
    public Hitbox hitbox;

    public override Vector2 center
    {
        get => hitbox.center;
        protected set { MoveAt(value); }
    }

    public CapsuleDirection2D direction;

    public Capsule(in Vector2 center, in Vector2 size)
    {
        hitbox = new Hitbox(center, size);
        direction = size.x >= size.y ? CapsuleDirection2D.Horizontal : CapsuleDirection2D.Vertical;
        if (direction == CapsuleDirection2D.Horizontal)
        {
            this.c1 = new Circle(new Vector2(center.x - size.x * 0.5f, center.y), size.y * 0.5f);
            this.c2 = new Circle(new Vector2(center.x + size.x * 0.5f, center.y), size.y * 0.5f);
        }
        else
        {
            this.c1 = new Circle(new Vector2(center.x, center.y - size.y * 0.5f), size.x * 0.5f);
            this.c2 = new Circle(new Vector2(center.x, center.y + size.y * 0.5f), size.x * 0.5f);
        }
        inclusiveCircle = ToCircle();
    }

    public Capsule(in Vector2 center, in Vector2 size, CapsuleDirection2D direction)
    {
        hitbox = new Hitbox(center, size);
        this.direction = direction;
        if(direction == CapsuleDirection2D.Horizontal)
        {
            this.c1 = new Circle(new Vector2(center.x - size.x * 0.5f, center.y), size.y * 0.5f);
            this.c2 = new Circle(new Vector2(center.x + size.x * 0.5f, center.y), size.y * 0.5f);
        }
        else
        {
            this.c1 = new Circle(new Vector2(center.x, center.y - size.y * 0.5f), size.x * 0.5f);
            this.c2 = new Circle(new Vector2(center.x, center.y + size.y * 0.5f), size.x * 0.5f);
        }
        inclusiveCircle = ToCircle();
    }
  
    public override CustomCollider Clone() => new Capsule(center, hitbox.size, direction);

    public override bool CollideLine(Line l) => CollideLine(l.A, l.B);
    public override bool CollideLine(in Vector2 A, in Vector2 B)
    {
        return CollideHitboxLine(hitbox, A, B) || CollideCircleLine(c1, A, B) || CollideCircleLine(c2, A, B);
    }
    public override bool CollideDroite(Droite d) => CollideDroite(d.A, d.B);
    public override bool CollideDroite(in Vector2 A, in Vector2 B)
    {
        return CollideHitboxDroite(hitbox, A, B) || CollideCircleDroite(c1, A, B) || CollideCircleDroite(c2, A, B);
    }

    public override bool Collide(CustomCollider c) => CustomCollider.Collide(c, this);

    public override void MoveAt(in Vector2 shift)
    {
        hitbox.MoveAt(shift);
        c1.MoveAt(hitbox.center);
        c2.MoveAt(hitbox.center);
        inclusiveCircle.MoveAt(shift);
    }

    public override void Rotate(in float angle)
    {
        Vector2 offsetC1 = c1.center - center;
        Vector2 offsetC2 = c2.center - center;
        float norme = offsetC1.magnitude;
        float angTotal = Useful.Angle(Vector2.zero, offsetC1) + angle;
        offsetC1 = new Vector2(norme * Mathf.Cos(angTotal), norme * Mathf.Sin(angTotal));
        c1.MoveAt(center + offsetC1);
        norme = offsetC2.magnitude;
        angTotal = Useful.Angle(Vector2.zero, offsetC2) + angle;
        offsetC2 = new Vector2((float)(norme * Mathf.Cos(angTotal)), (float)(norme * Mathf.Sin(angTotal)));
        c2.MoveAt(center + offsetC2);
        hitbox.Rotate(angle);
    }

    public override bool Contains(in Vector2 p) => hitbox.Contains(p) || c1.Contains(p) || c2.Contains(p);
    public override Hitbox ToHitbox() => hitbox;

    public override Circle ToCircle()
    {
        if(direction == CapsuleDirection2D.Horizontal)
            return new Circle(center, Mathf.Max(hitbox.size.x + c1.radius + c2.radius, hitbox.size.y) * 0.5f);
        else
            return new Circle(center, Mathf.Max(hitbox.size.x + c1.radius + c2.radius, hitbox.size.y) * 0.5f);
    }

    public override void SetScale(in Vector2 newScale, in Vector2 oldScale)
    {
        c1.SetScale(newScale, oldScale);
        c2.SetScale(newScale, oldScale);
        hitbox.SetScale(newScale, oldScale);

        Vector2 ratio = newScale / oldScale;
        c1.MoveAt(center + (c1.center - center) * ratio);
        c2.MoveAt(center + (c2.center - center) * ratio);

        inclusiveCircle.SetScale(newScale, oldScale);
    }

    public override bool Normal(in Vector2 point, out Vector2 normal)
    {
        if (c1.Normal(point, out normal))
            return true;
        if (c2.Normal(point, out normal))
            return true;
        if (hitbox.Normal(point, out normal))
            return true;
        return base.Normal(point, out normal);
    }
}

#endregion

#region Ellipse

public class Ellipse : CustomCollider
{
    #region Gizmos

    public static void GizmosDraw(Ellipse ellipse)
    {
        float aStep = Mathf.Min(1f / (ellipse.majorAxis * ellipse.minorAxis * 3f), 10f * Mathf.Deg2Rad);
        Vector2 center = ellipse.center;
        float angle = Useful.AngleHori(center, ellipse.focus1.x >= ellipse.focus2.x ? ellipse.focus1 : ellipse.focus2);

        float a = ellipse.majorAxis * 0.5f;
        float b = ellipse.minorAxis * 0.5f;
        float ab = a * b;
        float angleInit = Mathf.PI + angle;
        Vector2 oldUp = new Vector2(center.x + a * Mathf.Cos(angleInit), center.y + a * Mathf.Sin(angleInit));
        Vector2 oldDown = oldUp;

        //ellipse.Normal(oldUp, out Vector2 normal);
        //Useful.GizmoDrawVector(oldUp, normal);

        Vector2 newPoint;
        float length, bCosi, aSini;
        for (float i = aStep; i <= Mathf.PI; i += aStep)
        {
            bCosi = b * Mathf.Cos(i);
            aSini = a * Mathf.Sin(i);
            length = ab / Mathf.Sqrt(bCosi * bCosi + aSini * aSini);
            newPoint = new Vector2(center.x + length * Mathf.Cos(angleInit + i), center.y + length * Mathf.Sin(angleInit + i));
            Gizmos.DrawLine(oldUp, newPoint);

            //ellipse.Normal(newPoint, out normal);
            //Useful.GizmoDrawVector(newPoint, normal);

            oldUp = newPoint;
            newPoint = new Vector2(center.x + length * Mathf.Cos(angleInit - i), center.y + length * Mathf.Sin(angleInit - i));
            Gizmos.DrawLine(oldDown, newPoint);
            oldDown = newPoint;

            //ellipse.Normal(newPoint, out normal);
            //Useful.GizmoDrawVector(newPoint, normal);
        }
        newPoint = new Vector2(center.x + a * Mathf.Cos(angleInit - Mathf.PI), center.y + a * Mathf.Sin(angleInit - Mathf.PI));
        Gizmos.DrawLine(oldUp, newPoint);
        Gizmos.DrawLine(oldDown, newPoint);
        //Circle.GizmosDraw(ellipse.focus1, 0.3f);
        //Circle.GizmosDraw(ellipse.focus2, 0.3f);

        //ellipse.Normal(newPoint, out normal);
        //Useful.GizmoDrawVector(newPoint, normal);
    }

    #endregion

    private Vector2 _focus1, _focus2;
    public Vector2 focus1 { get => _focus1; private set { _focus1 = value; RecalculateAttribute(); } }
    public Vector2 focus2 { get => _focus2; private set { _focus2 = value; RecalculateAttribute(); } }
    public float majorAxis { get; private set; }

    public override Vector2 center { get => (focus1 + focus2) * 0.5f; protected set { } }

    public float c { get; private set; }
    public float minorAxis { get; private set; }

    public Ellipse(in Vector2 focus1, in Vector2 focus2, in float majorAxis) : base()
    {
        this.focus1 = focus1;
        this.focus2 = focus2;
        this.majorAxis = majorAxis;
        RecalculateAttribute();
        inclusiveCircle = ToCircle();
    }

    private void RecalculateAttribute()
    {
        c = focus1.Distance(focus2) * 0.5f;
        minorAxis = Mathf.Sqrt(majorAxis * majorAxis * 0.25f - c * c) * 2f;
    }

    public override CustomCollider Clone() => new Ellipse(focus1, focus2, majorAxis);
    public override Circle ToCircle() => new Circle(center, majorAxis * 0.5f);
    public Polygone ToPolygone()
    {
        List<Vector2> verticesUp = new List<Vector2>();
        List<Vector2> verticesDown = new List<Vector2>();
        float aStep = Mathf.Min(1f / (majorAxis * minorAxis), 10f * Mathf.Deg2Rad);
        Vector2 center = this.center;
        float angle = Useful.AngleHori(center, focus1.x >= focus2.x ? focus1 : focus2);

        float a = majorAxis * 0.5f;
        float b = minorAxis * 0.5f;
        float ab = a * b;
        float angleInit = Mathf.PI + angle;
        verticesUp.Add(new Vector2(center.x + a * Mathf.Cos(angleInit), center.y + a * Mathf.Sin(angleInit)));

        Vector2 newPoint;
        float length, bCosi, aSini;
        for (float i = aStep; i <= Mathf.PI; i += aStep)
        {
            bCosi = b * Mathf.Cos(i);
            aSini = a * Mathf.Sin(i);
            length = ab / Mathf.Sqrt(bCosi * bCosi + aSini * aSini);
            newPoint = new Vector2(center.x + length * Mathf.Cos(angleInit + i), center.y + length * Mathf.Sin(angleInit + i));
            verticesUp.Add(newPoint);
            newPoint = new Vector2(center.x + length * Mathf.Cos(angleInit - i), center.y + length * Mathf.Sin(angleInit - i));
            verticesDown.Add(newPoint);
        }
        newPoint = new Vector2(center.x + a * Mathf.Cos(angleInit - Mathf.PI), center.y + a * Mathf.Sin(angleInit - Mathf.PI));
        verticesUp.Add(newPoint);
        verticesDown.Reverse();
        verticesUp = verticesUp.Merge(verticesDown);

        return new Polygone(verticesUp, center);
    }

    public override void MoveAt(in Vector2 position)
    {
        Vector2 offset = position - center;
        center = position;
        focus1 += offset;
        focus2 += offset;
        inclusiveCircle.MoveAt(position);
    }

    public override void Rotate(in float angle)
    {
        Vector2 center = this.center;
        float a1 = Useful.AngleHori(center, focus1) + angle;
        float a2 = a1 + Mathf.PI;
        _focus1 = center + new Vector2(c * Mathf.Cos(a1), c * Mathf.Sin(a1));
        _focus2 = center + new Vector2(c * Mathf.Cos(a2), c * Mathf.Sin(a2));
    }

    public override void SetScale(in Vector2 newScale, in Vector2 oldScale)
    {
        float coeff = ((newScale.x + newScale.y) * 0.5f) / ((oldScale.x + oldScale.y) * 0.5f);
        Vector2 center = this.center;
        _focus1 = center + (focus1 - center) * coeff;
        _focus2 = center + (focus2 - center) * coeff;
        c *= coeff;
        majorAxis *= coeff;
        minorAxis *= coeff;
        inclusiveCircle.SetScale(newScale, oldScale);
    }

    public override bool Contains(in Vector2 p)
    {
        return p.Distance(focus1) + p.Distance(focus2) < majorAxis;
    }

    public override bool CollideLine(in Vector2 A, in Vector2 B) => CollideEllipseLine(this, A, B);
    public override bool CollideLine(Line l) => CollideLine(l.A, l.B);
    public override bool CollideDroite(in Vector2 A, in Vector2 B) => CollideEllipseDroite(this, A, B);
    public override bool CollideDroite(Droite d) => CollideDroite(d.A, d.B);

    public override Hitbox ToHitbox()
    {
        Hitbox h = new Hitbox(center, new Vector2(majorAxis, minorAxis));
        h.Rotate(Useful.AngleHori(center, focus1));
        return h;
    }

    public override bool Normal(in Vector2 point, out Vector2 normal)
    {
        if (Mathf.Abs(point.Distance(focus1) + point.Distance(focus2) - majorAxis) < accuracy)
        {
            Vector2 center = this.center;
            float angleInit = Useful.AngleHori(center, focus1.x >= focus2.x ? focus1 : focus2);

            float angle = Useful.AngleHori(center, point) - angleInit;
            float teta1 = angle + Mathf.Deg2Rad;
            float teta2 = angle - Mathf.Deg2Rad;
            float length1 = (majorAxis * minorAxis * 0.25f) / Mathf.Sqrt(Mathf.Pow(minorAxis * 0.5f * Mathf.Cos(teta1), 2) + Mathf.Pow(majorAxis * 0.5f * Mathf.Sin(teta1), 2));
            float length2 = (majorAxis * minorAxis * 0.25f) / Mathf.Sqrt(Mathf.Pow(minorAxis * 0.5f * Mathf.Cos(teta2), 2) + Mathf.Pow(majorAxis * 0.5f * Mathf.Sin(teta2), 2));

            Vector2 p1 = new Vector2(center.x + length1 * Mathf.Cos(teta1 + angleInit), center.y + length1 * Mathf.Sin(teta1 + angleInit));
            Vector2 p2 = new Vector2(center.x + length2 * Mathf.Cos(teta2 + angleInit), center.y + length2 * Mathf.Sin(teta2 + angleInit));

            normal = Useful.NormalVector(p2 - p1);
            if (normal.Dot(point - center) < 0f)
                normal *= -1f;
            return true;
        }
        return base.Normal(point, out normal);
    }

    public override bool Collide(CustomCollider c) => CustomCollider.Collide(c, this);
}

#endregion