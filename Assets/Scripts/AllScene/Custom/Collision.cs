using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

#region 2D Collisions

namespace Collision2D
{
    #region Line/StraightLine/Ray

    public class Line2D
    {
        public Vector2 A, B;
        public Line2D(in Vector2 A, in Vector2 B)
        {
            this.A = A;
            this.B = B;
        }

        public Line2D(in Vector2 start, in float angle, in float lenght)
        {
            A = start;
            B = new Vector2(A.x + lenght * Mathf.Cos(angle), A.y + lenght * Mathf.Sin(angle));
        }

        public bool Contain(in Vector2 point) => Contain(A, B, point);
        /// <summary>
        /// 
        /// </summary>
        /// <returns>true if point € [A,B], false otherwise</returns>
        public static bool Contain(in Vector2 A, in Vector2 B, in Vector2 point)
        {
            if (Mathf.Abs(A.x - B.x) < 1e-2f)
            {
                return Mathf.Approximately((A.x + B.x) * 0.5f, point.x) && Mathf.Min(A.y, B.y) <= point.y && Mathf.Max(A.y, B.y) >= point.y;
            }
            if (Mathf.Min(A.x, B.x) > point.x || Mathf.Max(A.x, B.x) < point.x || Mathf.Min(A.y, B.y) > point.y || Mathf.Max(A.y, B.y) < point.y)
            {
                return false;
            }
            //equation of (A,B)
            float a = (B.y - A.y) / (B.x - A.x);
            float b = A.y - a * A.x;
            return Mathf.Abs(a * point.x + b - point.y) < 1e-3f;
        }

        public float Distance(in Vector2 point) => Distance(A, B, point);
        public static float Distance(in Vector2 A, in Vector2 B, in Vector2 point) => Mathf.Sqrt(SqrDistance(A, B, point));

        public float SqrDistance(in Vector2 point) => SqrDistance(A, B, point);
        public static float SqrDistance(in Vector2 A, in Vector2 B, in Vector2 point)
        {
            float r = (((point.x - A.x) * (B.x - A.x)) + ((point.y - A.y) * (B.y - A.y))) / A.SqrDistance(B);
            Vector2 P = A + r * (B - A);
            return (0f <= r && r <= 1f) ? P.SqrDistance(point) : (r < 0f ? A.SqrDistance(point) : B.SqrDistance(point));
        }

        public Vector2 Normal() => Normal(A, B);
        /// <summary>
        /// 
        /// </summary>
        /// <returns>A vector normal of the line</returns>
        public static Vector2 Normal(in Vector2 A, in Vector2 B)
        {
            if (Mathf.Abs(A.x - B.x) < 1e-2f)
            {
                return Vector2.right;
            }
            return (B - A).NormalVector();
        }

        public Vector2 ClosestPoint(in Vector2 point) => ClosestPoint(A, B, point);
        public static Vector2 ClosestPoint(in Vector2 A, in Vector2 B, in Vector2 point)
        {
            Vector2 orthProj = StraightLine2D.OrthogonalProjection(point, A, B);

            if(orthProj.x >= Mathf.Min(A.x, B.x) && orthProj.x <= Mathf.Max(A.x, B.x) && orthProj.y >= Mathf.Min(A.y, B.y) && orthProj.y <= Mathf.Max(A.y, B.y))
            {
                return orthProj;
            }

            return A.SqrDistance(point) <= B.SqrDistance(point) ? A: B;
        }
    }

    public class StraightLine2D
    {
        public Vector2 A, B;
        public StraightLine2D(in Vector2 A, in Vector2 B)
        {
            this.A = A;
            this.B = B;
        }

        public static void GizmosDraw(StraightLine2D d) => GizmosDraw(d.A, d.B);
        public static void GizmosDraw(in Vector2 A, in Vector2 B)
        {
            float a = Useful.AngleHori(A, B);
            Vector2 dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a)).normalized;
            Vector2 s = (A + B) * 0.5f;
            Gizmos.DrawLine(s + dir * 100000f, s + dir * -100000f);
        }

        public Vector2 Symetric(in Vector2 M) => Symetric(M, A, B);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="M"></param>
        /// <param name="D"></param>
        /// <param name="h"></param>
        /// <returns>Le symétrique du point M par rapport à la droite (A,B)</returns>
        public static Vector2 Symetric(in Vector2 M, in Vector2 A, in Vector2 B)
        {
            //custom version
            if (Mathf.Abs(A.x - B.x) < 1e-3f)
            {
                return new Vector2(M.x >= (A.x - B.x) * 0.5f ? M.x - 2f * Distance(A, B, M) : M.x + 2f * Distance(A, B, M), M.y);
            }
            return 2f * OrthogonalProjection(M, A, B) - M;
        }

        public static Vector2 Reflection(in Vector2 normal, in Vector2 interPointInDroite, in Vector2 initDir)
        {
            return initDir - 2f * Vector2.Dot(initDir, normal) * normal;
        }

        public Vector2 OrthogonalProjection(in Vector2 M) => OrthogonalProjection(M, A, B);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="M"></param>
        /// <param name="D"></param>
        /// <returns>The orthogonal projection of the point M on the straight line (A,B)</returns>
        public static Vector2 OrthogonalProjection(in Vector2 M, in Vector2 A, in Vector2 B)
        {
            if (Mathf.Abs(A.x - B.x) < 1e-2f)
            {
                return new Vector2((A.x + B.x) * 0.5f, M.y);
            }

            float r = (((M.x - A.x) * (B.x - A.x)) + ((M.y - A.y) * (B.y - A.y))) / A.SqrDistance(B);
            Vector2 P = A + r * (B - A);
            return P;
        }

        public bool Contain(in Vector2 point) => Contain(A, B, point);
        /// <summary>
        /// 
        /// </summary>
        /// <returns>if point € (A,B)</returns>
        public static bool Contain(in Vector2 A, in Vector2 B, in Vector2 point)
        {
            if (Mathf.Abs(A.x - B.x) < 1e-2f)
            {
                return Mathf.Abs(((A.x + B.x) * 0.5f) - point.x) < 1e-3f && Mathf.Min(A.y, B.y) <= point.y && Mathf.Max(A.y, B.y) >= point.y;
            }
            //equetion de la droite (A,B)
            float a = (B.y - A.y) / (B.x - A.x);
            float b = A.y - a * A.x;
            return Mathf.Abs(a * point.x + b - point.y) < 1e-2f;
        }

        public float Distance(in Vector2 point) => Distance(A, B, point);
        /// <summary>
        /// Vérif ok
        /// </summary>
        /// <returns> min(Dist(point, P)), P € (A,B)</returns>
        public static float Distance(in Vector2 A, in Vector2 B, in Vector2 point)
        {
            if (Mathf.Abs(A.x - B.x) < 1e-2f)
            {
                return Mathf.Abs((A.x + B.x) * 0.5f - point.x);
            }
            float a = (B.y - A.y) / (B.x - A.x);
            float b = A.y - a * A.x;
            return Mathf.Abs(a * point.x - point.y + b) / Mathf.Sqrt(a * a + 1f);
        }

        public Vector2 Normal() => Normal(A, B);
        /// <summary>
        /// 
        /// </summary>
        /// <returns>A vector normal of the droite</returns>
        public static Vector2 Normal(in Vector2 A, in Vector2 B)
        {
            if (Mathf.Abs(A.x - B.x) < 1e-2f)
            {
                return Vector2.right;
            }
            return new Vector2((B.y - A.y) / (B.x - A.x), -1f).normalized;
        }

        public Vector2 ClosestPoint(in Vector2 point) => ClosestPoint(A, B, point);
        public static Vector2 ClosestPoint(in Vector2 A, in Vector2 B, in Vector2 point) => OrthogonalProjection(point, A, B);
    }

    /// <summary>
    /// Describe a directional line2D
    /// </summary>
    public class Ray2D
    {
        public Vector2 start, end;

        public Ray2D(in Vector2 start, in Vector2 end)
        {
            this.start = start;
            this.end = end;
        }
    }

    #endregion

    #region Collider2D

    public abstract class Collider2D : ICloneable<Collider2D> 
    {
        public static Collider2D FromUnityCollider2D(UnityEngine.Collider2D collider)
        {
            if (collider is BoxCollider2D hitbox)
                return new Hitbox(hitbox);
            else if (collider is CircleCollider2D circle)
                return new Circle(circle);
            else if (collider is CapsuleCollider2D capsule)
                return new Capsule(capsule);
            else if (collider is PolygonCollider2D poly)
                return new Polygone(poly);
            Debug.LogWarning($"Cant convert a unity collider2D of type {collider.GetType()}!");
            LogManager.instance.AddLog($"Cant convert a unity collider2D of type {collider.GetType()}!", collider);
            return null;
        }

        #region Collision Functions

        private static readonly List<Vector2> cache0 = new List<Vector2>(), cache1 = new List<Vector2>(), cache2 = new List<Vector2>();

        #region General Collissions

        #region Dico

        private static readonly Dictionary<Type, Dictionary<Type, Func<Collider2D, Collider2D, bool>>> collisionFunc1 = new Dictionary<Type, Dictionary<Type, Func<Collider2D, Collider2D, bool>>>()
        {
            {
                typeof(Circle),
                new Dictionary<Type, Func<Collider2D, Collider2D, bool>>()
                {
                    { typeof(Circle),  (Collider2D c1, Collider2D c2) => CollideCircles((Circle)c1, (Circle)c2) },
                    { typeof(Polygone),  (Collider2D c1, Collider2D c2) => CollideCirclePolygone((Circle)c1, (Polygone)c2) },
                    { typeof(Hitbox),  (Collider2D c1, Collider2D c2) => CollideCircleHitbox((Circle)c1, (Hitbox)c2) },
                    { typeof(Capsule),  (Collider2D c1, Collider2D c2) => CollideCircleCapsule((Circle)c1, (Capsule)c2) },
                }
            },
            {
                typeof(Polygone),
                new Dictionary<Type, Func<Collider2D, Collider2D, bool>>()
                {
                    { typeof(Circle),  (Collider2D c1, Collider2D c2) => CollideCirclePolygone((Circle)c2, (Polygone)c1) },
                    { typeof(Polygone),  (Collider2D c1, Collider2D c2) => CollidePolygones((Polygone)c1, (Polygone)c2) },
                    { typeof(Hitbox),  (Collider2D c1, Collider2D c2) => CollidePolygoneHitbox((Polygone)c1, (Hitbox)c2) },
                    { typeof(Capsule),  (Collider2D c1, Collider2D c2) => CollidePolygoneCapsule((Polygone)c1, (Capsule)c2) },
                }
            },
            {
                typeof(Hitbox),
                new Dictionary<Type, Func<Collider2D, Collider2D, bool>>()
                {
                    { typeof(Circle),  (Collider2D c1, Collider2D c2) => CollideCircleHitbox((Circle)c2, (Hitbox)c1) },
                    { typeof(Polygone),  (Collider2D c1, Collider2D c2) => CollidePolygoneHitbox((Polygone)c2, (Hitbox)c1) },
                    { typeof(Hitbox),  (Collider2D c1, Collider2D c2) => CollideHitboxes((Hitbox)c1, (Hitbox)c2) },
                    { typeof(Capsule),  (Collider2D c1, Collider2D c2) => CollideHitboxCapsule((Hitbox)c1, (Capsule)c2) },
                }
            },
            {
                typeof(Capsule),
                new Dictionary<Type, Func<Collider2D, Collider2D, bool>>()
                {
                    { typeof(Circle),  (Collider2D c1, Collider2D c2) => CollideCircleCapsule((Circle)c2, (Capsule)c1) },
                    { typeof(Polygone),  (Collider2D c1, Collider2D c2) => CollidePolygoneCapsule((Polygone)c2, (Capsule)c1) },
                    { typeof(Hitbox),  (Collider2D c1, Collider2D c2) => CollideHitboxCapsule((Hitbox)c2, (Capsule)c1) },
                    { typeof(Capsule),  (Collider2D c1, Collider2D c2) => CollideCapsules((Capsule)c1, (Capsule)c2) },
                }
            },
        };

        private static readonly Dictionary<Type, Dictionary<Type, Func<Collider2D, Collider2D, (bool, Vector2)>>> collisionFunc2 = new Dictionary<Type, Dictionary<Type, Func<Collider2D, Collider2D, (bool, Vector2)>>>()
        {
            {
                typeof(Circle),
                new Dictionary<Type, Func<Collider2D, Collider2D, (bool, Vector2)>>()
                {
                    { typeof(Circle),  (Collider2D c1, Collider2D c2) => (CollideCircles((Circle)c1, (Circle)c2, out Vector2 v), v) },
                    { typeof(Polygone),  (Collider2D c1, Collider2D c2) => (CollideCirclePolygone((Circle)c1, (Polygone)c2, out Vector2 v),v) },
                    { typeof(Hitbox),  (Collider2D c1, Collider2D c2) => (CollideCircleHitbox((Circle)c1, (Hitbox)c2, out Vector2 v),v) },
                    { typeof(Capsule),  (Collider2D c1, Collider2D c2) => (CollideCircleCapsule((Circle)c1, (Capsule)c2, out Vector2 v),v) },
                }
            },
            {
                typeof(Polygone),
                new Dictionary<Type, Func<Collider2D, Collider2D, (bool, Vector2)>>()
                {
                    { typeof(Circle),  (Collider2D c1, Collider2D c2) => (CollideCirclePolygone((Circle)c2, (Polygone)c1, out Vector2 v),v) },
                    { typeof(Polygone),  (Collider2D c1, Collider2D c2) => (CollidePolygones((Polygone)c1, (Polygone)c2, out Vector2 v),v) },
                    { typeof(Hitbox),  (Collider2D c1, Collider2D c2) => (CollidePolygoneHitbox((Polygone)c1, (Hitbox)c2, out Vector2 v),v) },
                    { typeof(Capsule),  (Collider2D c1, Collider2D c2) => (CollidePolygoneCapsule((Polygone)c1, (Capsule)c2, out Vector2 v),v) },
                }
            },
            {
                typeof(Hitbox),
                new Dictionary<Type, Func<Collider2D, Collider2D, (bool, Vector2)>>()
                {
                    { typeof(Circle),  (Collider2D c1, Collider2D c2) => (CollideCircleHitbox((Circle)c2, (Hitbox)c1, out Vector2 v),v) },
                    { typeof(Polygone),  (Collider2D c1, Collider2D c2) => (CollidePolygoneHitbox((Polygone)c2, (Hitbox)c1, out Vector2 v),v) },
                    { typeof(Hitbox),  (Collider2D c1, Collider2D c2) => (CollideHitboxes((Hitbox)c1, (Hitbox)c2, out Vector2 v),v) },
                    { typeof(Capsule),  (Collider2D c1, Collider2D c2) => (CollideHitboxCapsule((Hitbox)c1, (Capsule)c2, out Vector2 v),v) },
                }
            },
            {
                typeof(Capsule),
                new Dictionary<Type, Func<Collider2D, Collider2D, (bool, Vector2)>>()
                {
                    { typeof(Circle),  (Collider2D c1, Collider2D c2) => (CollideCircleCapsule((Circle)c2, (Capsule)c1, out Vector2 v),v) },
                    { typeof(Polygone),  (Collider2D c1, Collider2D c2) => (CollidePolygoneCapsule((Polygone)c2, (Capsule)c1, out Vector2 v),v) },
                    { typeof(Hitbox),  (Collider2D c1, Collider2D c2) => (CollideHitboxCapsule((Hitbox)c2, (Capsule)c1, out Vector2 v),v) },
                    { typeof(Capsule),  (Collider2D c1, Collider2D c2) => (CollideCapsules((Capsule)c1, (Capsule)c2, out Vector2 v),v) },
                }
            },
        };

        private static readonly Dictionary<Type, Dictionary<Type, Func<Collider2D, Collider2D, (bool, Vector2, Vector2, Vector2)>>> collisionFunc3 = new Dictionary<Type, Dictionary<Type, Func<Collider2D, Collider2D, (bool, Vector2, Vector2, Vector2)>>>()
        {
            {
                typeof(Circle),
                new Dictionary<Type, Func<Collider2D, Collider2D, (bool, Vector2, Vector2, Vector2)>>()
                {
                    { typeof(Circle),  (Collider2D c1, Collider2D c2) => (CollideCircles((Circle)c1, (Circle)c2, out Vector2 v, out Vector2 v2, out Vector2 v3), v, v2, v3) },
                    { typeof(Polygone),  (Collider2D c1, Collider2D c2) => (CollideCirclePolygone((Circle)c1, (Polygone)c2, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) },
                    { typeof(Hitbox),  (Collider2D c1, Collider2D c2) => (CollideCircleHitbox((Circle)c1, (Hitbox)c2, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) },
                    { typeof(Capsule),  (Collider2D c1, Collider2D c2) => (CollideCircleCapsule((Circle)c1, (Capsule)c2, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) },
                }
            },
            {
                typeof(Polygone),
                new Dictionary<Type, Func<Collider2D, Collider2D, (bool, Vector2, Vector2, Vector2)>>()
                {
                    { typeof(Circle),  (Collider2D c1, Collider2D c2) => (CollideCirclePolygone((Circle)c2, (Polygone)c1, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v3, v2) },
                    { typeof(Polygone),  (Collider2D c1, Collider2D c2) => (CollidePolygones((Polygone)c1, (Polygone)c2, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) },
                    { typeof(Hitbox),  (Collider2D c1, Collider2D c2) => (CollidePolygoneHitbox((Polygone)c1, (Hitbox)c2, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) },
                    { typeof(Capsule),  (Collider2D c1, Collider2D c2) => (CollidePolygoneCapsule((Polygone)c1, (Capsule)c2, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) },
                }
            },
            {
                typeof(Hitbox),
                new Dictionary<Type, Func<Collider2D, Collider2D, (bool, Vector2, Vector2, Vector2)>>()
                {
                    { typeof(Circle),  (Collider2D c1, Collider2D c2) => (CollideCircleHitbox((Circle)c2, (Hitbox)c1, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v3, v2) },
                    { typeof(Polygone),  (Collider2D c1, Collider2D c2) => (CollidePolygoneHitbox((Polygone)c2, (Hitbox)c1, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v3, v2) },
                    { typeof(Hitbox),  (Collider2D c1, Collider2D c2) => (CollideHitboxes((Hitbox)c1, (Hitbox)c2, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) },
                    { typeof(Capsule),  (Collider2D c1, Collider2D c2) => (CollideHitboxCapsule((Hitbox)c1, (Capsule)c2, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) },
                }
            },
            {
                typeof(Capsule),
                new Dictionary<Type, Func<Collider2D, Collider2D, (bool, Vector2, Vector2, Vector2)>>()
                {
                    { typeof(Circle),  (Collider2D c1, Collider2D c2) => (CollideCircleCapsule((Circle)c2, (Capsule)c1, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v3 , v2) },
                    { typeof(Polygone),  (Collider2D c1, Collider2D c2) => (CollidePolygoneCapsule((Polygone)c2, (Capsule)c1, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v3 , v2) },
                    { typeof(Hitbox),  (Collider2D c1, Collider2D c2) => (CollideHitboxCapsule((Hitbox)c2, (Capsule)c1, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v3 , v2) },
                    { typeof(Capsule),  (Collider2D c1, Collider2D c2) => (CollideCapsules((Capsule)c1, (Capsule)c2, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) },
                }
            },
           
        };

        private static readonly Dictionary<Type, Func<Collider2D, Vector2, Vector2, bool>> collisionLine1 = new Dictionary<Type, Func<Collider2D, Vector2, Vector2, bool>>()
        {
            { typeof(Circle), (Collider2D c, Vector2 A, Vector2 B) => CollideCircleLine((Circle)c, A, B) },
            { typeof(Polygone), (Collider2D c, Vector2 A, Vector2 B) => CollidePolygoneLine((Polygone)c, A, B) },
            { typeof(Hitbox), (Collider2D c, Vector2 A, Vector2 B) => CollideHitboxLine((Hitbox)c, A, B) },
            { typeof(Capsule), (Collider2D c, Vector2 A, Vector2 B) => CollideCapsuleLine((Capsule)c, A, B) },
        };

        private static readonly Dictionary<Type, Func<Collider2D, Vector2, Vector2, (bool, Vector2)>> collisionLine2 = new Dictionary<Type, Func<Collider2D, Vector2, Vector2, (bool, Vector2)>>()
        {
            { typeof(Circle), (Collider2D c, Vector2 A, Vector2 B) => (CollideCircleLine((Circle)c, A, B, out Vector2 v), v) },
            { typeof(Polygone), (Collider2D c, Vector2 A, Vector2 B) => (CollidePolygoneLine((Polygone)c, A, B, out Vector2 v), v) },
            { typeof(Hitbox), (Collider2D c, Vector2 A, Vector2 B) => (CollideHitboxLine((Hitbox)c, A, B, out Vector2 v), v) },
            { typeof(Capsule), (Collider2D c, Vector2 A, Vector2 B) => (CollideCapsuleLine((Capsule)c, A, B, out Vector2 v), v) },
        };

        private static readonly Dictionary<Type, Func<Collider2D, Vector2, Vector2, (bool, Vector2, Vector2)>> collisionLine3 = new Dictionary<Type, Func<Collider2D, Vector2, Vector2, (bool, Vector2, Vector2)>>()
        {
            { typeof(Circle), (Collider2D c, Vector2 A, Vector2 B) => (CollideCircleLine((Circle)c, A, B, out Vector2 v, out Vector2 v2), v, v2) },
            { typeof(Polygone), (Collider2D c, Vector2 A, Vector2 B) => (CollidePolygoneLine((Polygone)c, A, B, out Vector2 v, out Vector2 v2), v, v2) },
            { typeof(Hitbox), (Collider2D c, Vector2 A, Vector2 B) => (CollideHitboxLine((Hitbox)c, A, B, out Vector2 v, out Vector2 v2), v, v2) },
            { typeof(Capsule), (Collider2D c, Vector2 A, Vector2 B) => (CollideCapsuleLine((Capsule)c, A, B, out Vector2 v, out Vector2 v2), v, v2) },
        };

        private static readonly Dictionary<Type, Func<Collider2D, Vector2, Vector2, bool>> collisionStraightLine1 = new Dictionary<Type, Func<Collider2D, Vector2, Vector2, bool>>()
        {
            { typeof(Circle), (Collider2D c, Vector2 A, Vector2 B) => CollideCircleStraightLine((Circle)c, A, B) },
            { typeof(Polygone), (Collider2D c, Vector2 A, Vector2 B) => CollidePolygoneStaightLine((Polygone)c, A, B) },
            { typeof(Hitbox), (Collider2D c, Vector2 A, Vector2 B) => CollideHitboxStraigthLine((Hitbox)c, A, B) },
            { typeof(Capsule), (Collider2D c, Vector2 A, Vector2 B) => CollideCapsuleStraightLine((Capsule)c, A, B) },
        };

        private static readonly Dictionary<Type, Func<Collider2D, Vector2, Vector2, (bool, Vector2)>> collisionStraightLine2 = new Dictionary<Type, Func<Collider2D, Vector2, Vector2, (bool, Vector2)>>()
        {
            { typeof(Circle), (Collider2D c, Vector2 A, Vector2 B) => (CollideCircleStraightLine((Circle)c, A, B, out Vector2 v), v) },
            { typeof(Polygone), (Collider2D c, Vector2 A, Vector2 B) => (CollidePolygoneStaightLine((Polygone)c, A, B, out Vector2 v), v) },
            { typeof(Hitbox), (Collider2D c, Vector2 A, Vector2 B) => (CollideHitboxStraigthLine((Hitbox)c, A, B, out Vector2 v), v) },
            { typeof(Capsule), (Collider2D c, Vector2 A, Vector2 B) => (CollideCapsuleStraightLine((Capsule)c, A, B, out Vector2 v), v) },
        };

        private static readonly Dictionary<Type, Func<Collider2D, Vector2, Vector2, (bool, Vector2, Vector2)>> collisionStraightLine3 = new Dictionary<Type, Func<Collider2D, Vector2, Vector2, (bool, Vector2, Vector2)>>()
        {
            { typeof(Circle), (Collider2D c, Vector2 A, Vector2 B) => (CollideCircleStraightLine((Circle)c, A, B, out Vector2 v, out Vector2 v2), v, v2) },
            { typeof(Polygone), (Collider2D c, Vector2 A, Vector2 B) => (CollidePolygoneStaightLine((Polygone)c, A, B, out Vector2 v, out Vector2 v2), v, v2) },
            { typeof(Hitbox), (Collider2D c, Vector2 A, Vector2 B) => (CollideHitboxStraigthLine((Hitbox)c, A, B, out Vector2 v, out Vector2 v2), v, v2) },
            { typeof(Capsule), (Collider2D c, Vector2 A, Vector2 B) => (CollideCapsuleStraightLine((Capsule)c, A, B, out Vector2 v, out Vector2 v2), v, v2) },
        };

        private static readonly Dictionary<Type, Func<Collider2D, Vector2, Vector2, bool>> collisionRay1 = new Dictionary<Type, Func<Collider2D, Vector2, Vector2, bool>>()
        {
            { typeof(Circle), (Collider2D c, Vector2 A, Vector2 B) => CollideCircleRay((Circle)c, A, B) },
            { typeof(Polygone), (Collider2D c, Vector2 A, Vector2 B) => CollidePolygoneRay((Polygone)c, A, B) },
            { typeof(Hitbox), (Collider2D c, Vector2 A, Vector2 B) => CollideHitboxRay((Hitbox)c, A, B) },
            { typeof(Capsule), (Collider2D c, Vector2 A, Vector2 B) => CollideCapsuleRay((Capsule)c, A, B) },
        };

        private static readonly Dictionary<Type, Func<Collider2D, Vector2, Vector2, (bool, Vector2)>> collisionRay2 = new Dictionary<Type, Func<Collider2D, Vector2, Vector2, (bool, Vector2)>>()
        {
            { typeof(Circle), (Collider2D c, Vector2 A, Vector2 B) => (CollideCircleRay((Circle)c, A, B, out Vector2 v), v) },
            { typeof(Polygone), (Collider2D c, Vector2 A, Vector2 B) => (CollidePolygoneRay((Polygone)c, A, B, out Vector2 v), v) },
            { typeof(Hitbox), (Collider2D c, Vector2 A, Vector2 B) => (CollideHitboxRay((Hitbox)c, A, B, out Vector2 v), v) },
            { typeof(Capsule), (Collider2D c, Vector2 A, Vector2 B) => (CollideCapsuleRay((Capsule)c, A, B, out Vector2 v), v) },
        };

        private static readonly Dictionary<Type, Func<Collider2D, Vector2, Vector2, (bool, Vector2, Vector2)>> collisionRay3 = new Dictionary<Type, Func<Collider2D, Vector2, Vector2, (bool, Vector2, Vector2)>>()
        {
            { typeof(Circle), (Collider2D c, Vector2 A, Vector2 B) => (CollideCircleRay((Circle)c, A, B, out Vector2 v, out Vector2 v2), v, v2) },
            { typeof(Polygone), (Collider2D c, Vector2 A, Vector2 B) => (CollidePolygoneRay((Polygone)c, A, B, out Vector2 v, out Vector2 v2), v, v2) },
            { typeof(Hitbox), (Collider2D c, Vector2 A, Vector2 B) => (CollideHitboxRay((Hitbox)c, A, B, out Vector2 v, out Vector2 v2), v, v2) },
            { typeof(Capsule), (Collider2D c, Vector2 A, Vector2 B) => (CollideCapsuleRay((Capsule)c, A, B, out Vector2 v, out Vector2 v2), v, v2) },
        };

        #endregion

        private static bool FirstTestBeforeCollision(Collider2D c1, Collider2D c2) => CollideCircles(c1.inclusiveCircle, c2.inclusiveCircle);
        private static bool FirstTestBeforeLineCollision(Collider2D c, in Vector2 A, in Vector2 B) => CollideCircleLine(c.inclusiveCircle, A, B);
        private static bool FirstTestBeforeStraightLineCollision(Collider2D c, in Vector2 A, in Vector2 B) => CollideCircleStraightLine(c.inclusiveCircle, A, B);
        private static bool FirstTestBeforeRayCollision(Collider2D c, in Vector2 A, in Vector2 B) => CollideCircleLine(c.inclusiveCircle, A, B);

        /// <returns>true if the both collider collide together, false otherwise</returns>
        public static bool Collide(Collider2D collider1, Collider2D collider2)
        {
            Type type1 = collider1.GetType();
            Type type2 = collider2.GetType();
            if(type1 == typeof(Circle) && type2 == typeof(Circle))
                return collisionFunc1[type1][type2](collider1, collider2);
            return FirstTestBeforeCollision(collider1, collider2) && collisionFunc1[type1][type2](collider1, collider2);
        }
        /// <param name="collisionPoint">The average point of collision of the two collider if true is return, (0,0) oterwise</param>
        /// <returns>true if the both collider collide together, false otherwise</returns>
        public static bool Collide(Collider2D collider1, Collider2D collider2, out Vector2 collisionPoint)
        {
            Type type1 = collider1.GetType();
            Type type2 = collider2.GetType();
            bool b;
            if (type1 == typeof(Circle) && type2 == typeof(Circle))
            {
                (b, collisionPoint) = collisionFunc2[type1][type2](collider1, collider2);
                return b;
            }

            if (!FirstTestBeforeCollision(collider1, collider2))
            {
                collisionPoint = Vector2.zero;
                return false;
            }
            (b, collisionPoint) = collisionFunc2[type1][type2](collider1, collider2);
            return b;
        }
        /// <param name="collisionPoint">The average point of collision of the two collider if true is return, (0,0) oterwise</param>
        /// <param name="normal1">The vector normal at the surface of the first collider where the collission happend</param>
        /// <param name="normal2">The vector normal at the surface of the second collider where the collission happend</param>
        /// <returns>true if the both collider collide together, false otherwise</returns>
        public static bool Collide(Collider2D collider1, Collider2D collider2, out Vector2 collisionPoint, out Vector2 normal1, out Vector2 normal2)
        {
            Type type1 = collider1.GetType();
            Type type2 = collider2.GetType();
            bool b;
            if (type1 == typeof(Circle) && type2 == typeof(Circle))
            {
                (b, collisionPoint, normal1, normal2) = collisionFunc3[type1][type2](collider1, collider2);
                return b;
            }

            if (!FirstTestBeforeCollision(collider1, collider2))
            {
                collisionPoint = normal1 = normal2 = Vector2.zero;
                return false;
            }
            (b, collisionPoint, normal1, normal2) = collisionFunc3[type1][type2](collider1, collider2);
            return b;
        }

        /// <returns>true if the collider collide together width the line, false otherwise</returns>
        public static bool CollideLine(Collider2D collider, in Vector2 A, in Vector2 B)
        {
            Type type = collider.GetType();
            if (type == typeof(Circle))
                return collisionLine1[type](collider, A, B);
            return FirstTestBeforeLineCollision(collider, A, B) && collisionLine1[type](collider, A, B);

        }
        /// <returns>true if the collider collide together width the line, false otherwise</returns>
        public static bool CollideLine(Collider2D collider, Line2D line) => CollideLine(collider, line.A, line.B);
        /// <param name="collisionPoint">The point at the surface of the collider where the collission happend</param>
        /// <returns>true if the collider collide together width the line, false otherwise</returns>
        public static bool CollideLine(Collider2D collider, in Vector2 A, in Vector2 B, out Vector2 collisionPoint)
        {
            Type type = collider.GetType();
            bool b;
            if (type == typeof(Circle))
            {
                (b, collisionPoint) = collisionLine2[type](collider, A, B);
                return b;
            }

            if(!FirstTestBeforeLineCollision(collider, A, B))
            {
                collisionPoint = Vector2.zero;
                return false;
            }

            (b, collisionPoint) = collisionLine2[type](collider, A, B);
            return b;
        }
        /// <param name="collisionPoint">The point at the surface of the collider where the collission happend</param>
        /// <returns>true if the collider collide together width the line, false otherwise</returns>
        public static bool CollideLine(Collider2D collider, Line2D line, out Vector2 collisionPoint) => CollideLine(collider, line.A, line.B, out collisionPoint);
        /// <param name="collisionPoint">The point at the surface of the collider where the collission happend</param>
        /// <param name="normal">The vector normal tothe collider's surface wehere the collision happend</param>
        /// <returns>true if the collider collide together width the line, false otherwise</returns>
        public static bool CollideLine(Collider2D collider, in Vector2 A, in Vector2 B, out Vector2 collisionPoint, out Vector2 normal)
        {
            Type type = collider.GetType();
            bool b;
            if (type == typeof(Circle))
            {
                (b, collisionPoint, normal) = collisionLine3[type](collider, A, B);
                return b;
            }

            if (!FirstTestBeforeLineCollision(collider, A, B))
            {
                collisionPoint = normal = Vector2.zero;
                return false;
            }
            (b, collisionPoint, normal) = collisionLine3[type](collider, A, B);
            return b;
        }
        /// <param name="collisionPoint">The point at the surface of the collider where the collission happend</param>
        /// <param name="normal">The vector normal tothe collider's surface wehere the collision happend</param>
        /// <returns>true if the collider collide together width the line, false otherwise</returns>
        public static bool CollideLine(Collider2D collider, Line2D line, out Vector2 collisionPoint, out Vector2 normal) => CollideLine(collider, line.A, line.B, out collisionPoint, out normal);

        /// <returns>true if the collider collide together width the droite, false otherwise</returns>
        public static bool CollideStraightLine(Collider2D collider, in Vector2 A, in Vector2 B)
        {
            Type type = collider.GetType();
            if(type == typeof(Circle))
            {
                return collisionStraightLine1[type](collider, A, B);
            }

            return FirstTestBeforeStraightLineCollision(collider, A, B) && collisionStraightLine1[type](collider, A, B);
        }
        /// <returns>true if the collider collide together width the droite, false otherwise</returns>
        public static bool CollideStraightLine(Collider2D ollider, StraightLine2D straightLine) => CollideStraightLine(ollider, straightLine.A, straightLine.B);
        /// <param name="collisionPoint">The point at the surface of the collider where the collission happend</param>
        /// <returns>true if the collider collide together width the droite, false otherwise</returns>
        public static bool CollideStraightLine(Collider2D collider, in Vector2 A, in Vector2 B, out Vector2 collisionPoint)
        {
            Type type = collider.GetType();
            bool b;
            if (type == typeof(Circle))
            {
                (b, collisionPoint) = collisionStraightLine2[type](collider, A, B);
                return b;
            }

            if (!FirstTestBeforeStraightLineCollision(collider, A, B))
            {
                collisionPoint = Vector2.zero;
                return false;
            }

            (b, collisionPoint) = collisionStraightLine2[type](collider, A, B);
            return b;
        }
        /// <param name="collisionPoint">The point at the surface of the collider where the collission happend</param>
        /// <returns>true if the collider collide together width the droite, false otherwise</returns>
        public static bool CollideStraightLine(Collider2D collider, StraightLine2D straightLine, out Vector2 collisionPoint) => CollideStraightLine(collider, straightLine.A, straightLine.B, out collisionPoint);
        /// <param name="collisionPoint">The point at the surface of the collider where the collission happend</param>
        /// <param name="normal">The vector normal tothe collider's surface wehere the collision happend</param>
        /// <returns>true if the collider collide together width the droite, false otherwise</returns>
        public static bool CollideStraightLine(Collider2D collider, in Vector2 A, in Vector2 B, out Vector2 collisionPoint, out Vector2 normal)
        {
            Type type = collider.GetType();
            bool b;
            if (type == typeof(Circle))
            {
                (b, collisionPoint, normal) = collisionStraightLine3[type](collider, A, B);
                return b;
            }

            if (!FirstTestBeforeStraightLineCollision(collider, A, B))
            {
                collisionPoint = normal = Vector2.zero;
                return false;
            }

            (b, collisionPoint, normal) = collisionStraightLine3[type](collider, A, B);
            return b;
        }
        /// <param name="collisionPoint">The point at the surface of the collider where the collission happend</param>
        /// <param name="normal">The vector normal tothe collider's surface wehere the collision happend</param>
        /// <returns>true if the collider collide together width the droite, false otherwise</returns>
        public static bool CollideStraightLine(Collider2D collider, StraightLine2D straightLine, out Vector2 collisionPoint, out Vector2 normal) => CollideStraightLine(collider, straightLine.A, straightLine.B, out collisionPoint, out normal);

        /// <returns>true if the collider collide together width the droite, false otherwise</returns>
        public static bool CollideRay(Collider2D collider, in Vector2 A, in Vector2 B)
        {
            Type type = collider.GetType();
            if(type == typeof(Circle))
                return collisionRay1[type](collider, A, B);

            return FirstTestBeforeRayCollision(collider, A, B) && collisionRay1[type](collider, A, B);
        }
        /// <returns>true if the collider collide together width the droite, false otherwise</returns>
        public static bool CollideRay(Collider2D ollider, Ray2D ray) => CollideRay(ollider, ray.start, ray.end);
        /// <param name="collisionPoint">The first point at the surface of the collider where the collission happend</param>
        /// <returns>true if the collider collide together width the droite, false otherwise</returns>
        public static bool CollideRay(Collider2D collider, in Vector2 A, in Vector2 B, out Vector2 collisionPoint)
        {
            Type type = collider.GetType();
            bool b;
            if (type == typeof(Circle))
            {
                (b, collisionPoint) = collisionRay2[type](collider, A, B);
                return b;
            }

            if (!FirstTestBeforeRayCollision(collider, A, B))
            {
                collisionPoint = Vector2.zero;
                return false;
            }

            (b, collisionPoint) = collisionRay2[type](collider, A, B);
            return b;
        }
        /// <param name="collisionPoint">The point at the surface of the collider where the collission happend</param>
        /// <returns>true if the collider collide together width the droite, false otherwise</returns>
        public static bool CollideRay(Collider2D collider, Ray2D ray, out Vector2 collisionPoint) => CollideRay(collider, ray.start, ray.end, out collisionPoint);
        /// <param name="collisionPoint">The point at the surface of the collider where the collission happend</param>
        /// <param name="normal">The vector normal tothe collider's surface wehere the collision happend</param>
        /// <returns>true if the collider collide together width the droite, false otherwise</returns>
        public static bool CollideRay(Collider2D collider, in Vector2 A, in Vector2 B, out Vector2 collisionPoint, out Vector2 normal)
        {
            Type type = collider.GetType();
            bool b;
            if (type == typeof(Circle))
            {
                (b, collisionPoint, normal) = collisionRay3[type](collider, A, B);
                return b;
            }

            if (!FirstTestBeforeRayCollision(collider, A, B))
            {
                collisionPoint = normal = Vector2.zero;
                return false;
            }

            (b, collisionPoint, normal) = collisionRay3[type](collider, A, B);
            return b;
        }
        /// <param name="collisionPoint">The point at the surface of the collider where the collission happend</param>
        /// <param name="normal">The vector normal tothe collider's surface wehere the collision happend</param>
        /// <returns>true if the collider collide together width the droite, false otherwise</returns>
        public static bool CollideRay(Collider2D collider, Ray2D ray, out Vector2 collisionPoint, out Vector2 normal) => CollideRay(collider, ray.start, ray.end, out collisionPoint, out normal);

        #endregion

        #region Collide(Circle, other)

        public static bool CollideCircles(Circle circle1, Circle circle2) => circle1.center.SqrDistance(circle2.center) <= (circle1.radius + circle2.radius) * (circle1.radius + circle2.radius);
        private static bool ComputeCirclesIntersections(Circle circle1, Circle circle2, out Vector2 intersection1, out Vector2 intersection2)
        {
            float sqrDist = circle1.center.SqrDistance(circle2.center);
            float rr = (circle1.radius + circle2.radius) * (circle1.radius + circle2.radius);
            if (sqrDist <= rr && sqrDist > (circle1.radius - circle2.radius) * (circle1.radius - circle2.radius))//collision!
            {
                if (Mathf.Abs(circle1.center.y - circle2.center.y) < 1e-3f)
                {
                    float x = ((circle2.radius * circle2.radius) - (circle1.radius * circle1.radius) - (circle2.center.x * circle2.center.x) + (circle1.center.x * circle1.center.x)) / (2f * (circle1.center.x - circle2.center.x));
                    float b = -2f * circle2.center.y;
                    float c = (circle2.center.x * circle2.center.x) + (x * x) - (2f * circle2.center.x * x) + (circle2.center.y * circle2.center.y) - (circle2.radius * circle2.radius);
                    float sqrtDelta = Mathf.Sqrt(b * b * (-4f * c));
                    intersection1 = new Vector2(x, (b + sqrtDelta) * -0.5f);
                    intersection2 = new Vector2(x, (sqrtDelta - b) * 0.5f);
                    return true;
                }
                else
                {
                    float N = ((circle2.radius * circle2.radius) - (circle1.radius * circle1.radius) - (circle2.center.x * circle2.center.x) + (circle1.center.x * circle1.center.x) - (circle2.center.y * circle2.center.y) + (circle1.center.y * circle1.center.y)) / (2f * (circle1.center.y - circle2.center.y));
                    float temps = ((circle1.center.x - circle2.center.x) / (circle1.center.y - circle2.center.y));
                    float a = (temps * temps) + 1;
                    float b = (2f * circle1.center.y * temps) - (2f * N * temps) - (2f * circle1.center.x);
                    float c = (circle1.center.x * circle1.center.x) + (circle1.center.y * circle1.center.y) + (N * N) - (circle1.radius * circle1.radius) - (2f * circle1.center.y * N);
                    float sqrtDelta = Mathf.Sqrt((b * b) - (4f * a * c));
                    c = 1f / (2f * a);
                    float x1 = -c * (b + sqrtDelta);
                    float x2 = c * (sqrtDelta - b);
                    intersection1 = new Vector2(x1, N - (x1 * temps));
                    intersection2 = new Vector2(x2, N - (x2 * temps));
                    return true;
                }
            }

            intersection1 = intersection2 = Vector2.zero;
            return false;
        }
        private static bool ComputeCircleStraightLineIntersections(Circle circle, in Vector2 A, in Vector2 B, out Vector2 intersection1, out Vector2 intersection2)
        {
            if (!CollideCircleStraightLine(circle, A, B))
            {
                intersection1 = intersection2 = Vector2.zero;
                return false;
            }

            //if vertical line
            if (Mathf.Abs(A.x - B.x) < 1e-2f)
            {
                float avg = (A.x + B.x) * 0.5f;
                float srqtDelta = Mathf.Sqrt((circle.radius * circle.radius) + (circle.center.x - avg) * (avg - circle.center.x));
                intersection1 = new Vector2(avg, circle.center.y - srqtDelta);
                intersection2 = new Vector2(avg, srqtDelta + circle.center.y);
            }
            else
            {
                float m = (B.y - A.y) / (B.x - A.x);
                float p = A.y - m * A.x;
                float a = 1f + (m * m);
                float b = 2f * ((m * p) - circle.center.x - (m * circle.center.y));
                float C = ((circle.center.x * circle.center.x) + (p * p) - (2f * p * circle.center.y) + (circle.center.y * circle.center.y) - (circle.radius * circle.radius));
                float sqrtDelta = Mathf.Sqrt((b * b) - (4f * a * C));
                C = 1f / (2f * a);
                m = m * C;
                intersection1 = new Vector2(-C * (b + sqrtDelta), -m * (b + sqrtDelta) + p);
                intersection2 = new Vector2(C * (sqrtDelta - b), m * (sqrtDelta - b) + p);
            }
            return true;
        }
        private static (Vector2 i1, Vector2 i2) ComputeCircleStraightLineIntersectionsUnchecked(Circle circle, in Vector2 A, in Vector2 B)
        {
            if (Mathf.Abs(A.x - B.x) < 1e-2f)
            {
                float avg = (A.x + B.x) * 0.5f;
                float srqtDelta = Mathf.Sqrt((circle.radius * circle.radius) + (circle.center.x - avg) * (avg - circle.center.x));
                return (new Vector2(avg, circle.center.y - srqtDelta), new Vector2(avg, srqtDelta + circle.center.y));
            }
            else
            {
                float m = (B.y - A.y) / (B.x - A.x);
                float p = A.y - m * A.x;
                float a = 1f + (m * m);
                float b = 2f * ((m * p) - circle.center.x - (m * circle.center.y));
                float C = ((circle.center.x * circle.center.x) + (p * p) - (2f * p * circle.center.y) + (circle.center.y * circle.center.y) - (circle.radius * circle.radius));
                float sqrtDelta = Mathf.Sqrt((b * b) - (4f * a * C));
                C = 1f / (2f * a);
                m = m * C;
                return (new Vector2(-C * (b + sqrtDelta), -m * (b + sqrtDelta) + p), new Vector2(C * (sqrtDelta - b), m * (sqrtDelta - b) + p));
            }
        }
        private static Vector2[] ComputeCircleLineIntersections(Circle circle, in Vector2 A, in Vector2 B)
        {
            if (!CollideCircleStraightLine(circle, A, B))
            {
                return Array.Empty<Vector2>();
            }

            //if vertical line
            Vector2 i1, i2;
            if (Mathf.Abs(A.x - B.x) < 1e-2f)
            {
                float avg = (A.x + B.x) * 0.5f;
                float srqtDelta = Mathf.Sqrt((circle.radius * circle.radius) + (circle.center.x - avg) * (avg - circle.center.x));
                i1 = new Vector2(avg, circle.center.y - srqtDelta);
                i2 = new Vector2(avg, srqtDelta + circle.center.y);

                float minY, maxY;
                if (A.y >= B.y)
                {
                    minY = B.y - 1e-4f;
                    maxY = A.y + 1e-4f;
                }
                else
                {
                    maxY = B.y + 1e-4f;
                    minY = A.y - 1e-4f;
                }

                bool containI1 = minY <= i1.y && i1.y <= maxY;
                bool containI2 = minY <= i2.y && i2.y <= maxY;

                if (containI1 && containI2)
                {
                    return new Vector2[2] { i1, i2 };
                }
                if(containI1)
                {
                    return new Vector2[1] { i1 };
                }
                if (containI2)
                {
                    return new Vector2[1] { i2 };
                }
                return Array.Empty<Vector2>();
            }
            else
            {
                float m = (B.y - A.y) / (B.x - A.x);
                float p = A.y - m * A.x;
                float a = 1f + (m * m);
                float b = 2f * ((m * p) - circle.center.x - (m * circle.center.y));
                float C = ((circle.center.x * circle.center.x) + (p * p) - (2f * p * circle.center.y) + (circle.center.y * circle.center.y) - (circle.radius * circle.radius));
                float sqrtDelta = Mathf.Sqrt((b * b) - (4f * a * C));
                C = 1f / (2f * a);
                m = m * C;
                i1 = new Vector2(-C * (b + sqrtDelta), -m * (b + sqrtDelta) + p);
                i2 = new Vector2(C * (sqrtDelta - b), m * (sqrtDelta - b) + p);

                float minX, maxX, minY, maxY; 
                if (A.x >= B.x)
                {
                    minX = B.x - 1e-4f;
                    maxX = A.x + 1e-4f;
                }
                else
                {
                    maxX = B.x + 1e-4f;
                    minX = A.x - 1e-4f;
                }
                if (A.y >= B.y)
                {
                    minY = B.y - 1e-4f;
                    maxY = A.y + 1e-4f;
                }
                else
                {
                    maxY = B.y + 1e-4f;
                    minY = A.y - 1e-4f;
                }

                bool containI1 = minX <= i1.x && i1.x <= maxX && minY <= i1.y && i1.y <= maxY;
                bool containI2 = minX <= i2.x && i2.x <= maxX && minY <= i2.y && i2.y <= maxY;

                if (containI1 && containI2)
                {
                    return new Vector2[2] { i1, i2 };
                }
                if (containI1)
                {
                    return new Vector2[1] { i1 };
                }
                if (containI2)
                {
                    return new Vector2[1] { i2 };
                }
                return Array.Empty<Vector2>();
            }
        }
        public static bool CollideCircles(Circle circle1, Circle circle2, out Vector2 collisionPoint)
        {
            float sqrDist = circle1.center.SqrDistance(circle2.center);
            if(sqrDist <= (circle1.radius + circle2.radius) * (circle1.radius + circle2.radius))
            {
                sqrDist = Mathf.Sqrt(sqrDist);
                float d = (circle1.radius + circle2.radius - sqrDist) * 0.5f;
                collisionPoint = circle1.center + ((circle2.center - circle1.center) * ((circle1.radius - d) / sqrDist));
                return true;
            }
            collisionPoint = Vector2.zero;
            return false;
        }
        public static bool CollideCircles(Circle circle1, Circle circle2, out Vector2 collisionPoint, out Vector2 normal1, out Vector2 normal2)
        {
            float sqrDist = circle1.center.SqrDistance(circle2.center);
            if (sqrDist <= (circle1.radius + circle2.radius) * (circle1.radius + circle2.radius))
            {
                sqrDist = Mathf.Sqrt(sqrDist);
                float d = (circle1.radius + circle2.radius - sqrDist) * 0.5f;
                collisionPoint = circle1.center + ((circle2.center - circle1.center) * ((circle1.radius - d) / sqrDist));

                normal1 = (collisionPoint - circle1.center) / (circle1.radius - d);
                normal2 = (collisionPoint - circle2.center) / (circle2.radius - d);
                return true;
            }
            collisionPoint = normal1 = normal2 = Vector2.zero;
            return false;
        }
        public static bool CollideCirclePolygone(Circle circle, Polygone polygone)
        {
            for (int i = 0; i < polygone.vertices.Length; i++)
            {
                if (CollideCircleLine(circle, polygone.vertices[i], polygone.vertices[(i + 1) % polygone.vertices.Length]))
                    return true;
            }
            return circle.Contains(polygone.center) || (polygone.inclusiveCircle.radius > circle.radius && polygone.Contains(circle.center));
        }
        public static bool CollideCirclePolygone(Circle circle, Polygone polygone, out Vector2 collisionPoint)
        {
            collisionPoint = Vector2.zero;
            Vector2 i1, i2;
            Vector2 A, B;
            for (int i = 0; i < polygone.vertices.Length; i++)
            {
                A = polygone.vertices[i];
                B = polygone.vertices[(i + 1) % polygone.vertices.Length];
                if (ComputeCircleStraightLineIntersections(circle, A, B, out i1, out i2))
                {
                    bool containI1 = Line2D.Contain(A, B, i1);
                    bool containI2 = Line2D.Contain(A, B, i2);
                    if(containI1 || containI2)
                    {
                        if(containI1 && containI2)
                        {
                            Vector2 tmp = (i1 + i2) * 0.5f;
                            float dist = circle.radius - ((circle.radius - circle.center.Distance(tmp)) * 0.5f);
                            cache0.Add(circle.center + ((tmp - circle.center).normalized * dist));
                        }
                        else if(containI1)
                        {
                            cache0.Add(i1);
                        }
                        else
                        {
                            cache0.Add(i2);
                        }
                    }
                }
            }
            if (cache0.Count > 0)
            {
                foreach (Vector2 pos in cache0)
                {
                    collisionPoint += pos;
                }
                collisionPoint /= cache0.Count;
                cache0.Clear();
                return true;
            }
            if (polygone.Contains(circle.center))
            {
                collisionPoint = (circle.center + polygone.center) * 0.5f;
                return true;
            }
            return false;
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
        public static bool CollideCircleHitbox(Circle circle, Hitbox hitbox) => CollideCirclePolygone(circle, hitbox.ToPolygone());
        public static bool CollideCircleHitbox(Circle circle, Hitbox hitbox, out Vector2 collisionPoint) => CollideCirclePolygone(circle, hitbox.ToPolygone(), out collisionPoint);
        public static bool CollideCircleHitbox(Circle circle, Hitbox hitbox, out Vector2 collisionPoint, out Vector2 normal1, out Vector2 normal2) => CollideCirclePolygone(circle, hitbox.ToPolygone(), out collisionPoint, out normal1, out normal2);
        public static bool CollideCircleLine(Circle circle, in Vector2 A, in Vector2 B)
        {
            return Line2D.SqrDistance(A, B, circle.center) <= circle.radius * circle.radius;

            //The code below also work fine
            //Vector2 u = B - A;
            //Vector2 AC = circle.center - A;
            //float CI = Mathf.Abs(u.x * AC.y - u.y * AC.x) / u.magnitude;
            //if (CI > circle.radius)
            //    return false;
            //else
            //{
            //    Vector2 BC = circle.center - B;
            //    float pscal1 = u.x * AC.x + u.y * AC.y;
            //    float pscal2 = -(u.x * BC.x + u.y * BC.y);
            //    if (pscal1 >= 0 && pscal2 >= 0)
            //        return true;   // I between A and B, ok.
            //    //last case, A or B in the circle
            //    return circle.center.SqrDistance(A) < circle.radius * circle.radius || circle.center.SqrDistance(B) < circle.radius * circle.radius;
            //}
        }
        public static bool CollideCircleLine(Circle circle, Line2D line) => CollideLine(circle, line.A, line.B);
        public static bool CollideCircleLine(Circle circle, in Vector2 A, in Vector2 B, out Vector2 collisionPoint)
        {
            float rr = circle.radius * circle.radius;
            bool containA = circle.center.SqrDistance(A) <= rr;
            bool containB = circle.center.SqrDistance(B) <= rr;

            if((containA && containB) || (!containA && !containB))
            {
                Vector2 closestPoint = !containA ? Line2D.ClosestPoint(A, B, circle.center) : StraightLine2D.ClosestPoint(A, B, circle.center);
                if(!containA && closestPoint.SqrDistance(circle.center) > rr)
                {
                    collisionPoint = Vector2.zero;
                    return false;
                }

                collisionPoint = circle.center + (closestPoint - circle.center).normalized * circle.radius;
                return true;
            }

            (Vector2 i1, Vector2 i2) = ComputeCircleStraightLineIntersectionsUnchecked(circle, A, B);
            collisionPoint = (Mathf.Min(A.x, B.x) <= i1.x && i1.x <= Mathf.Max(A.x, B.x) && Mathf.Min(A.y, B.y) <= i1.y && i1.y <= Mathf.Max(A.y, B.y)) ? i1 : i2;
            return true;
        }
        public static bool CollideCircleLine(Circle circle, Line2D line, out Vector2 collisionPoint) => CollideCircleLine(circle, line.A, line.B, out collisionPoint);
        public static bool CollideCircleLine(Circle circle, in Vector2 A, in Vector2 B, out Vector2 collisionPoint, out Vector2 normal)
        {
            if (CollideCircleLine(circle, A, B, out collisionPoint))
            {
                normal = (collisionPoint - circle.center);
                normal.Normalize();
                return true;
            }
            normal = Vector2.zero;
            return false;
        }
        public static bool CollideCircleLine(Circle cicle, Line2D line, out Vector2 collisionPoint, out Vector2 normal) => CollideCircleLine(cicle, line.A, line.B, out collisionPoint, out normal);
        public static bool CollideCircleStraightLine(Circle circle, StraightLine2D straightLine) => CollideCircleStraightLine(circle, straightLine.A, straightLine.B);
        public static bool CollideCircleStraightLine(Circle circle, in Vector2 A, in Vector2 B)
        {
            Vector2 u = B - A;
            Vector2 AC = circle.center - A;
            float numerateur = Mathf.Abs(u.x * AC.y - u.y * AC.x);
            return numerateur < circle.radius * u.magnitude;
        }
        public static bool CollideCircleStraightLine(Circle circle, StraightLine2D straightLine, out Vector2 collisionPoint) => CollideCircleStraightLine(circle, straightLine, out collisionPoint);
        public static bool CollideCircleStraightLine(Circle circle, in Vector2 A, in Vector2 B, out Vector2 collisionPoint)
        {
            if (!CollideCircleStraightLine(circle, A, B))
            {
                collisionPoint = Vector2.zero;
                return false;
            }

            Vector2 u = B - A;
            Vector2 AC = circle.center - A;
            float ti = (u.x * AC.x + u.y * AC.y) / (u.x * u.x + u.y * u.y);
            collisionPoint = new Vector2(A.x + ti * u.x, A.y + ti * u.y);
            ti = collisionPoint.SqrDistance(circle.center);
            if (ti > circle.radius * circle.radius)
                return true;

            return CollideCircleLine(circle, collisionPoint, collisionPoint + (circle.radius / Mathf.Sqrt(ti)) * (collisionPoint - circle.center), out collisionPoint);
        }
        public static bool CollideCircleStraightLine(Circle circle, in Vector2 A, in Vector2 B, out Vector2 collisionPoint, out Vector2 normal)
        {
            if (CollideCircleStraightLine(circle, A, B, out collisionPoint))
            {
                normal = (collisionPoint - circle.center);
                normal.Normalize();
                return true;
            }
            normal = Vector2.zero;
            return false;
        }
        public static bool CollideCircleStraightLine(Circle circle, StraightLine2D straightLine, out Vector2 collisionPoint, out Vector2 normal) => CollideCircleStraightLine(circle, straightLine.A, straightLine.B, out collisionPoint, out normal);
        public static bool CollideCircleRay(Circle circle, Ray2D ray) => CollideCircleLine(circle, ray.start, ray.end);
        public static bool CollideCircleRay(Circle circle, in Vector2 start, in Vector2 end) => CollideCircleLine(circle, start, end);
        public static bool CollideCircleRay(Circle circle, Ray2D ray, out Vector2 collisionPoint) => CollideCircleRay(circle, ray.start, ray.end, out collisionPoint);
        public static bool CollideCircleRay(Circle circle, in Vector2 start, in Vector2 end, out Vector2 collisionPoint)
        {
            if(circle.Contains(start))
            {
                collisionPoint = start;
                return true;
            }

            if (ComputeCircleStraightLineIntersections(circle, start, end, out Vector2 i1, out Vector2 i2))
            {
                float minX, minY, maxX, maxY;
                if(start.x >= end.x)
                {
                    minX = end.x;
                    maxX = start.x;
                }
                else
                {
                    maxX = end.x;
                    minX = start.x;
                }
                if (start.y >= end.y)
                {
                    minY = end.y;
                    maxY = start.y;
                }
                else
                {
                    maxY = end.y;
                    minY = start.y;
                }

                bool containI1 = minX <= i1.x && i1.x <= maxX && minY <= i1.y && i1.y <= maxY;
                bool containI2 = minX <= i2.x && i2.x <= maxX && minY <= i2.y && i2.y <= maxY;

                if(containI1 && containI2)
                {
                    collisionPoint = start.SqrDistance(i1) <= start.SqrDistance(i2) ? i1 : i2;
                    return true;
                }
                if(containI1)
                {
                    collisionPoint = i1;
                    return true;
                }
                if (containI2)
                {
                    collisionPoint = i2;
                    return true;
                }
            }
            collisionPoint = Vector2.zero;
            return false;
        }
        public static bool CollideCircleRay(Circle circle, Ray2D ray, out Vector2 collisionPoint, out Vector2 normal) => CollideCircleRay(circle, ray.start, ray.end, out collisionPoint, out normal);
        public static bool CollideCircleRay(Circle circle, in Vector2 start, in Vector2 end, out Vector2 collisionPoint, out Vector2 normal)
        {
            if (circle.Contains(start))
            {
                collisionPoint = start;
                normal = (start - circle.center).normalized;
                return true;
            }

            if (CollideCircleRay(circle, start, end, out collisionPoint))
            {
                normal = (collisionPoint - circle.center).normalized;
                return true;
            }

            collisionPoint = normal = Vector2.zero;
            return false;
        }
        public static bool CollideCircleCapsule(Circle circle, Capsule capsule)
        {
            float rr = (capsule.circle1.radius + circle.radius) * (capsule.circle1.radius + circle.radius); ;
            if (capsule.circle1.center.SqrDistance(circle.center) <= rr || capsule.circle2.center.SqrDistance(circle.center) <= rr)
                return true;

            float distance = capsule.circle1.center.Distance(capsule.circle2.center);
            if (distance >= 1e-5f)
            {
                Vector2 dirHori = (capsule.circle1.center - capsule.circle2.center) / distance;
                Vector2 dirVerti = dirHori.NormalVector();
                distance *= 0.5f;
                Line2D line1 = new Line2D(capsule.hitbox.center + (dirVerti * capsule.circle1.radius) - (dirHori * distance), capsule.hitbox.center + (dirVerti * capsule.circle1.radius) + (dirHori * distance));
                Line2D line2 = new Line2D(capsule.hitbox.center - (dirVerti * capsule.circle1.radius) - (dirHori * distance), capsule.hitbox.center - (dirVerti * capsule.circle1.radius) + (dirHori * distance));
                return CollideCircleLine(circle, line1) || CollideCircleLine(circle, line2);
            }
            return false;
        }
        public static bool CollideCircleCapsule(Circle circle, Capsule capsule, out Vector2 collisionPoint)
        {
            float distance = capsule.circle1.center.Distance(capsule.circle2.center);
            Vector2 i1, i2;
            if (distance >= 1e-3f)
            {
                Vector2 dirHori = (capsule.circle1.center - capsule.circle2.center) / distance;
                Vector2 dirVerti = dirHori.NormalVector();
                distance *= 0.5f;
                Line2D line1 = new Line2D(capsule.hitbox.center + (dirVerti * capsule.circle1.radius) - (dirHori * distance) , capsule.hitbox.center + (dirVerti * capsule.circle1.radius) + (dirHori * distance));
                Line2D line2 = new Line2D(capsule.hitbox.center - (dirVerti * capsule.circle1.radius) - (dirHori * distance), capsule.hitbox.center - (dirVerti * capsule.circle1.radius) + (dirHori * distance));

                PerformLine(line1);
                PerformLine(line2);

                void PerformLine(Line2D line)
                {
                    if (ComputeCircleStraightLineIntersections(circle, line.A, line.B, out i1, out i2))
                    {
                        bool containI1 = Line2D.Contain(line.A, line.B, i1);
                        bool containI2 = Line2D.Contain(line.A, line.B, i2);
                        if (containI1 || containI2)
                        {
                            if (containI1 && containI2)
                            {
                                Vector2 tmp = (i1 + i2) * 0.5f;
                                float dist = circle.radius - ((circle.radius - circle.center.Distance(tmp)) * 0.5f);
                                cache0.Add(circle.center + ((tmp - circle.center).normalized * dist));
                            }
                            else if (containI1)
                            {
                                cache0.Add(i1);
                            }
                            else
                            {
                                cache0.Add(i2);
                            }
                        }
                    }
                }
            }

            if (ComputeCirclesIntersections(circle, capsule.circle1, out i1, out i2))
            {
                if ((capsule.circle1.center - capsule.hitbox.center).Dot(i1 - capsule.circle1.center) >= 0f)
                {
                    cache0.Add(i1);
                }
                if ((capsule.circle1.center - capsule.hitbox.center).Dot(i2 - capsule.circle1.center) >= 0f)
                {
                    cache0.Add(i2);
                }
            }
            if (ComputeCirclesIntersections(circle, capsule.circle2, out i1, out i2))
            {
                if ((capsule.circle2.center - capsule.hitbox.center).Dot(i1 - capsule.circle2.center) >= 0f)
                {
                    cache0.Add(i1);
                }
                if ((capsule.circle2.center - capsule.hitbox.center).Dot(i2 - capsule.circle2.center) >= 0f)
                {
                    cache0.Add(i2);
                }
            }

            collisionPoint = Vector2.zero;
            if (cache0.Count > 0)
            {
                foreach (Vector2 pos in cache0)
                {
                    collisionPoint += pos;
                }
                collisionPoint /= cache0.Count;
                cache0.Clear();
                return true;
            }

            if(circle.Contains(capsule.center))
            {
                collisionPoint = (circle.center + capsule.center) * 0.5f;
                return true;
            }

            if (capsule.Contains(circle.center))
            {
                collisionPoint = (circle.center + capsule.center) * 0.5f;
                return true;
            }

            return false;
        }
        public static bool CollideCircleCapsule(Circle circle, Capsule capsule, out Vector2 collisionPoint, out Vector2 normal1, out Vector2 normal2)
        {
            if (CollideCircleCapsule(circle, capsule, out collisionPoint))
            {
                normal1 = collisionPoint - circle.center;
                normal1.Normalize();
                normal2 = -normal1;
                return true;
            }
            normal1 = normal2 = Vector2.zero;
            return false;
        }

        #endregion

        #region Collide(Polygones, other)

        public static bool CollidePolygones(Polygone polygone1, Polygone polygone2)
        {
            for (int i = 0; i < polygone1.vertices.Length; i++)
            {
                for (int j = 0; j < polygone2.vertices.Length; j++)
                {
                    if (CollideLines(polygone1.vertices[i], polygone1.vertices[(i + 1) % polygone1.vertices.Length], polygone2.vertices[j], polygone2.vertices[(j + 1) % polygone2.vertices.Length]))
                    {
                        return true;
                    }
                }
            }
            return polygone1.inclusiveCircle.radius >= polygone2.inclusiveCircle.radius ? polygone1.Contains(polygone2.center) : polygone2.Contains(polygone1.center);
        }
        public static bool CollidePolygones(Polygone polygone1, Polygone polygone2, out Vector2 collisionPoint)
        {
            for (int i = 0; i < polygone1.vertices.Length; i++)
            {
                for (int j = 0; j < polygone2.vertices.Length; j++)
                {
                    if (CollideLines(polygone1.vertices[i], polygone1.vertices[(i + 1) % polygone1.vertices.Length], polygone2.vertices[j], polygone2.vertices[(j + 1) % polygone2.vertices.Length], out Vector2 intersec))
                    {
                        cache0.Add(intersec);
                    }
                }
            }

            if (cache0.Count > 0)
            {
                collisionPoint = Vector2.zero;
                foreach (Vector2 pos in cache0)
                {
                    collisionPoint += pos;
                }
                collisionPoint /= cache0.Count;
                cache0.Clear();
                return true;
            }

            bool contain = polygone1.inclusiveCircle.radius >= polygone2.inclusiveCircle.radius ? polygone1.Contains(polygone2.center) : polygone2.Contains(polygone1.center);
            if (contain)
            {
                collisionPoint = (polygone1.center + polygone2.center) * 0.5f;
                return true;
            }

            collisionPoint = Vector2.zero;
            return false;
        }
        public static bool CollidePolygones(Polygone polygone1, Polygone polygone2, out Vector2 collisionPoint, out Vector2 normal1, out Vector2 normal2)
        {
            if(polygone1.Contains(polygone2.center) && polygone2.Contains(polygone1.center))
            {
                normal1 = (polygone2.center - polygone1.center).normalized;
                normal2 = -normal1;
                collisionPoint = (polygone1.center + polygone2.center) * 0.5f;
                return true;
            }

            Vector2 n;
            Vector2 side1;
            for (int i = 0; i < polygone1.vertices.Length; i++)
            {
                side1 = polygone1.vertices[(i + 1) % polygone1.vertices.Length];
                for (int j = 0; j < polygone2.vertices.Length; j++)
                {
                    collisionPoint = polygone2.vertices[(j + 1) % polygone2.vertices.Length];
                    if (CollideLines(polygone1.vertices[i], side1, polygone2.vertices[j], collisionPoint, out Vector2 intersec))
                    {
                        cache0.Add(intersec);
                        n = (side1 - polygone1.vertices[i]).NormalVector();
                        if(!polygone1.IsNormalOnRightDirection(intersec, n, i))
                        {
                            n *= -1f;
                        }
                        cache1.Add(n);

                        n = (collisionPoint - polygone2.vertices[j]).NormalVector();
                        if (!polygone2.IsNormalOnRightDirection(intersec, n, j))
                        {
                            n *= -1f;
                        }
                        cache1.Add(n);
                    }
                }
            }

            if(cache0.Count <= 0)
            {
                bool contain = polygone1.inclusiveCircle.radius >= polygone2.inclusiveCircle.radius ? polygone1.Contains(polygone2.center) : polygone2.Contains(polygone1.center);
                if (contain)
                {
                    collisionPoint = (polygone1.center + polygone2.center) * 0.5f;
                    normal1 = (polygone2.center - polygone1.center).normalized;
                    normal2 = -normal1;
                    return true;
                }

                collisionPoint = normal1 = normal2 = Vector2.zero;
                return false;
            }

            collisionPoint = normal1 = Vector2.zero;
            foreach (Vector2 point in cache0)
            {
                collisionPoint += point;
            }
            collisionPoint /= cache0.Count;
            cache0.Clear();

            foreach (Vector2 point in cache1)
            {
                normal1 += point;
            }

            normal1.Normalize();
            if(normal1.Dot(collisionPoint - polygone1.center) < 0f)
            {
                normal1 *= -1f;
            }
            normal2 = -normal1;
            cache1.Clear();
            return true;
        }
        public static bool CollidePolygoneHitbox(Polygone polygone, Hitbox hitbox) => CollidePolygones(hitbox.ToPolygone(), polygone);
        public static bool CollidePolygoneHitbox(Polygone polygone, Hitbox hitbox, out Vector2 collisionPoint) => CollidePolygones(polygone, hitbox.ToPolygone(), out collisionPoint);
        public static bool CollidePolygoneHitbox(Polygone polygone, Hitbox hitbox, out Vector2 collisionPoint, out Vector2 normal1, out Vector2 normal2) => CollidePolygones(polygone, hitbox.ToPolygone(), out collisionPoint, out normal1, out normal2);
        public static bool CollidePolygoneLine(Polygone polygone, in Vector2 A, in Vector2 B)
        {
            for (int i = 0; i < polygone.vertices.Length; i++)
            {
                if (CollideLines(A, B, polygone.vertices[i], polygone.vertices[(i + 1) % polygone.vertices.Length]))
                {
                    return true;
                }
            }
            return polygone.Contains(A);
        }
        public static bool CollidePolygoneLine(Polygone polygone, Line2D line) => polygone.CollideLine(line);
        public static bool CollidePolygoneLine(Polygone polygone, in Vector2 A, in Vector2 B, out Vector2 collisionPoint)
        {
            int ip1;
            for (int i = 0; i < polygone.vertices.Length; i++)
            {
                ip1 = (i + 1) % polygone.vertices.Length;
                if (CollideLines(polygone.vertices[i], polygone.vertices[ip1], A, B, out Vector2 intersec))
                {
                    cache0.Add(intersec);
                }
            }

            collisionPoint = Vector2.zero;
            StraightLine2D straightLine;
            float minSqrDist, d;

            if (cache0.Count <= 0)
            {
                if (polygone.Contains(A))
                {
                    Vector2 cp = Line2D.ClosestPoint(A, B, polygone.center);
                    straightLine = new StraightLine2D(cp, cp + (A - B).NormalVector());
                    minSqrDist = float.MaxValue;

                    for (int i = 0; i < polygone.vertices.Length; i++)
                    {
                        if (CollideLineStraightLine(polygone.vertices[i], polygone.vertices[(i + 1) % polygone.vertices.Length], straightLine.A, straightLine.B, out Vector2 intersec))
                        {
                            d = intersec.SqrDistance(cp);
                            if (d < minSqrDist)
                            {
                                minSqrDist = d;
                                collisionPoint = intersec;
                            }
                        }
                    }
                    return true;
                }
                return false;
            }

            if(cache0.Count == 1)
            {
                collisionPoint = cache0[0];
                cache0.Clear();
                return true;
            }

            for (int i = 0; i < cache0.Count; i++)
            {
                collisionPoint += cache0[i];
            }
            Vector2 avgInter = collisionPoint / cache0.Count;

            straightLine = new StraightLine2D(avgInter, avgInter + (B - A).NormalVector());
            minSqrDist = float.MaxValue;

            for (int i = 0; i < polygone.vertices.Length; i++)
            {
                if (CollideLineStraightLine(polygone.vertices[i], polygone.vertices[(i + 1) % polygone.vertices.Length], straightLine.A, straightLine.B, out Vector2 intersec))
                {
                    d = intersec.SqrDistance(avgInter);
                    if (d < minSqrDist)
                    {
                        minSqrDist = d;
                        collisionPoint = intersec;
                    }
                }
            }

            cache0.Clear();
            return true;
        }
        public static bool CollidePolygoneLine(Polygone polygone, Line2D line, out Vector2 collisionPoint) => CollidePolygoneLine(polygone, line.A, line.B, out collisionPoint);
        public static bool CollidePolygoneLine(Polygone polygone, Line2D line, out Vector2 collisionPoint, out Vector2 normal) => CollidePolygoneLine(polygone, line.A, line.B, out collisionPoint, out normal);
        public static bool CollidePolygoneLine(Polygone polygone, in Vector2 A, in Vector2 B, out Vector2 collisionPoint, out Vector2 normal)
        {
            List<Vector2Int> sideIntersec = new List<Vector2Int>();

            int ip1;
            int indexFirstInter = -1;
            for (int i = 0; i < polygone.vertices.Length; i++)
            {
                ip1 = (i + 1) % polygone.vertices.Length;
                if (CollideLines(polygone.vertices[i], polygone.vertices[ip1], A, B, out Vector2 intersec))
                {
                    if(indexFirstInter == -1)
                    {
                        indexFirstInter = i;
                    }
                    cache0.Add(intersec);
                    sideIntersec.Add(new Vector2Int(i, ip1));
                }
            }

            collisionPoint = Vector2.zero;
            Line2D line;
            float minSqrDist, d;

            if (cache0.Count <= 0)
            {
                if (polygone.Contains(A))
                {
                    Vector2 cp = Line2D.ClosestPoint(A, B, polygone.center);
                    normal = (A - B).NormalVector();
                    line = new Line2D(cp, cp + normal * (polygone.inclusiveCircle.radius * 2f));
                    minSqrDist = float.MaxValue;
                    int minIndex = -1;

                    for (int i = 0; i < polygone.vertices.Length; i++)
                    {
                        if (CollideLineStraightLine(polygone.vertices[i], polygone.vertices[(i + 1) % polygone.vertices.Length], line.A, line.B, out Vector2 intersec))
                        {
                            d = intersec.SqrDistance(cp);
                            if (d < minSqrDist)
                            {
                                minSqrDist = d;
                                collisionPoint = intersec;
                                minIndex = i;
                            }
                        }
                    }

                    normal = (polygone.vertices[(minIndex + 1) % polygone.vertices.Length] - polygone.vertices[minIndex]).NormalVector();
                    if (!polygone.IsNormalOnRightDirection(collisionPoint, normal, minIndex))
                        normal *= -1f;
    
                    return true;
                }

                normal = Vector2.zero;
                return false;
            }

            if (cache0.Count == 1)
            {
                collisionPoint = cache0[0];
                normal = (polygone.vertices[(indexFirstInter + 1) % polygone.vertices.Length] - polygone.vertices[indexFirstInter]).NormalVector();

                if(!polygone.IsNormalOnRightDirection(collisionPoint, normal, indexFirstInter))
                    normal *= -1f;

                cache0.Clear();
                return true;
            }

            for (int i = 0; i < cache0.Count; i++)
            {
                collisionPoint += cache0[i];
            }

            Vector2 avgInter = collisionPoint / cache0.Count;
            normal = (B - A).NormalVector();
            Vector2 linePoint = avgInter + normal;
            minSqrDist = float.MaxValue;
            int minSideIndex = -1;
            for (int i = 0; i < polygone.vertices.Length; i++)
            {
                if (CollideLineStraightLine(polygone.vertices[i], polygone.vertices[(i + 1) % polygone.vertices.Length], avgInter, linePoint, out Vector2 intersec))
                {
                    d = intersec.SqrDistance(avgInter);
                    if (d < minSqrDist)
                    {
                        minSqrDist = d;
                        collisionPoint = intersec;
                        minSideIndex = i;
                    }
                }
            }

            polygone.Normal(collisionPoint, out normal);
            cache0.Clear();
            return true;
        }
        public static bool CollidePolygoneStaightLine(Polygone polygone, in Vector2 A, in Vector2 B)
        {
            for (int i = 0; i < polygone.vertices.Length; i++)
            {
                if (CollideLineStraightLine(polygone.vertices[i], polygone.vertices[(i + 1) % polygone.vertices.Length], A, B))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool CollidePolygoneStaightLine(Polygone polygone, StraightLine2D straightLine) => polygone.CollideStraightLine(straightLine);
        public static bool CollidePolygoneStaightLine(Polygone polygone, in Vector2 A, in Vector2 B, out Vector2 collisionPoint)
        {
            for (int i = 0; i < polygone.vertices.Length; i++)
            {
                if (CollideLineStraightLine(polygone.vertices[i], polygone.vertices[(i + 1) % polygone.vertices.Length], A, B, out Vector2 intersec))
                {
                    cache0.Add(intersec);
                }
            }

            collisionPoint = Vector2.zero;
            if (cache0.Count <= 0)
                return false;

            Vector2 avgInter = Vector2.zero;
            foreach (Vector2 pos in cache0)
            {
                avgInter += pos;
            }
            avgInter /= cache0.Count;
            Vector2 normal = (B - A).NormalVector();
            Vector2 otherPoint = avgInter + normal;

            float  minSqrDist = float.MaxValue;
            float d;

            for (int i = 0; i < polygone.vertices.Length; i++)
            {
                if (CollideLineStraightLine(polygone.vertices[i], polygone.vertices[(i + 1) % polygone.vertices.Length], avgInter, otherPoint, out Vector2 intersec))
                {
                    d = intersec.SqrDistance(avgInter);
                    if (d < minSqrDist)
                    {
                        minSqrDist = d;
                        collisionPoint = intersec;
                    }
                }
            }

            cache0.Clear();
            return true;
        }
        public static bool CollidePolygoneStaightLine(Polygone polygone, StraightLine2D straightLine, out Vector2 collisionPoint) => CollidePolygoneStaightLine(polygone, straightLine.A, straightLine.B, out collisionPoint);
        public static bool CollidePolygoneStaightLine(Polygone polygone, in Vector2 A, in Vector2 B, out Vector2 collisionPoint, out Vector2 normal)
        {
            for (int i = 0; i < polygone.vertices.Length; i++)
            {
                if (CollideLineStraightLine(polygone.vertices[i], polygone.vertices[(i + 1) % polygone.vertices.Length], A, B, out Vector2 intersec))
                {
                    cache0.Add(intersec);
                }
            }

            collisionPoint = Vector2.zero;
            if (cache0.Count <= 0)
            {
                normal = Vector2.zero;
                return false;
            }

            Vector2 avgInter = Vector2.zero;
            foreach (Vector2 pos in cache0)
            {
                avgInter += pos;
            }
            avgInter /= cache0.Count;
            normal = (B - A).NormalVector();
            Vector2 otherPoint = avgInter + normal;

            float minSqrDist = float.MaxValue;
            float d;
            int minSideIndex = -1;

            for (int i = 0; i < polygone.vertices.Length; i++)
            {
                if (CollideLineStraightLine(polygone.vertices[i], polygone.vertices[(i + 1) % polygone.vertices.Length], avgInter, otherPoint, out Vector2 intersec))
                {
                    d = intersec.SqrDistance(avgInter);
                    if (d < minSqrDist)
                    {
                        minSqrDist = d;
                        collisionPoint = intersec;
                        minSideIndex = i;
                    }
                }
            }

            normal = (polygone.vertices[(minSideIndex + 1) % polygone.vertices.Length] - polygone.vertices[minSideIndex]).NormalVector();
            if (!polygone.IsNormalOnRightDirection(collisionPoint, normal, minSideIndex))
                normal *= -1f;

            cache0.Clear();
            return true;
        }
        public static bool CollidePolygoneStaightLine(Polygone polygone, StraightLine2D straightLine, out Vector2 collisionPoint, out Vector2 normal) => CollidePolygoneStaightLine(polygone, straightLine.A, straightLine.B, out collisionPoint, out normal);
        public static bool CollidePolygoneRay(Polygone polygone, Ray2D ray) => CollidePolygoneLine(polygone, ray.start, ray.end);
        public static bool CollidePolygoneRay(Polygone polygone, in Vector2 start, in Vector2 end) => CollidePolygoneLine(polygone, start, end);
        public static bool CollidePolygoneRay(Polygone polygone, Ray2D ray, out Vector2 collisionPoint) => CollidePolygoneRay(polygone, ray.start, ray.end, out collisionPoint);
        public static bool CollidePolygoneRay(Polygone polygone, in Vector2 start, in Vector2 end, out Vector2 collisionPoint)
        {
            if (polygone.Contains(start))
            {
                collisionPoint = start;
                return true;
            }

            float minX, maxX, minY, maxY;
            if (start.x >= end.x)
            {
                minX = end.x - 1e-4f;
                maxX = start.x + 1e-4f;
            }
            else
            {
                maxX = end.x + 1e-4f;
                minX = start.x - 1e-4f;
            }
            if (start.y >= end.y)
            {
                minY = end.y - 1e-4f;
                maxY = start.y + 1e-4f;
            }
            else
            {
                maxY = end.y + 1e-4f;
                minY = start.y - 1e-4f;
            }
            
            for (int i = 0; i < polygone.vertices.Length; i++)
            {
                if(CollideLineStraightLine(polygone.vertices[i], polygone.vertices[(i + 1) % polygone.vertices.Length], start, end, out Vector2 inter))
                {
                    if(minX <= inter.x && inter.x <= maxX && minY <= inter.y && inter.y <= maxY)
                    {
                        cache0.Add(inter);
                    }
                }
            }

            if (cache0.Count <= 1)
            {
                if (cache0.Count <= 0)
                {
                    collisionPoint = Vector2.zero;
                    return false;
                }

                collisionPoint = cache0[0];
                cache0.Clear();
                return true;
            }

            collisionPoint = cache0[0];
            float minSqrDist = start.SqrDistance(collisionPoint);
            float currentSqrDistance;

            for (int i = 0; i < cache0.Count; i++)
            {
                currentSqrDistance = start.SqrDistance(cache0[i]);
                if(currentSqrDistance < minSqrDist)
                {
                    minSqrDist = currentSqrDistance;
                    collisionPoint = cache0[i];
                }
            }

            cache0.Clear();
            return true;
        }
        public static bool CollidePolygoneRay(Polygone polygone, Ray2D ray, out Vector2 collisionPoint, out Vector2 normal) => CollidePolygoneRay(polygone, ray.start, ray.end, out collisionPoint, out normal);
        public static bool CollidePolygoneRay(Polygone polygone, in Vector2 start, in Vector2 end, out Vector2 collisionPoint, out Vector2 normal)
        {
            if (polygone.Contains(start))
            {
                collisionPoint = start;
                normal = (start - polygone.center).normalized;
                return true;
            }

            float minX, maxX, minY, maxY;
            if (start.x >= end.x)
            {
                minX = end.x - 1e-4f;
                maxX = start.x + 1e-4f;
            }
            else
            {
                maxX = end.x + 1e-4f;
                minX = start.x - 1e-4f;
            }
            if (start.y >= end.y)
            {
                minY = end.y - 1e-4f;
                maxY = start.y + 1e-4f;
            }
            else
            {
                maxY = end.y + 1e-4f;
                minY = start.y - 1e-4f;
            }


            List<int> intersSideIndex = new List<int>();
            for (int i = 0; i < polygone.vertices.Length; i++)
            {
                if (CollideLineStraightLine(polygone.vertices[i], polygone.vertices[(i + 1) % polygone.vertices.Length], start, end, out Vector2 inter))
                {
                    if (minX <= inter.x && inter.x <= maxX && minY <= inter.y && inter.y <= maxY)
                    {
                        cache0.Add(inter);
                        intersSideIndex.Add(i);
                    }
                }
            }

            if (cache0.Count <= 1)
            {
                if (cache0.Count <= 0)
                {
                    collisionPoint = normal = Vector2.zero;
                    return false;
                }

                collisionPoint = cache0[0];
                normal = (polygone.vertices[(intersSideIndex[0] + 1) % polygone.vertices.Length] - polygone.vertices[intersSideIndex[0]]).NormalVector();
                if(!polygone.IsNormalOnRightDirection(collisionPoint, normal, intersSideIndex[0]))
                {
                    normal *= -1f;
                }

                cache0.Clear();
                return true;
            }

            collisionPoint = cache0[0];
            float minSqrDist = start.SqrDistance(collisionPoint);
            float currentSqrDistance;
            int indexMinDist = 0;

            for (int i = 1; i < cache0.Count; i++)
            {
                currentSqrDistance = start.SqrDistance(cache0[i]);
                if (currentSqrDistance < minSqrDist)
                {
                    minSqrDist = currentSqrDistance;
                    collisionPoint = cache0[i];
                    indexMinDist = i;
                }
            }

            normal = (polygone.vertices[(intersSideIndex[indexMinDist] + 1) % polygone.vertices.Length] - polygone.vertices[intersSideIndex[indexMinDist]]).NormalVector();
            if (!polygone.IsNormalOnRightDirection(collisionPoint, normal, intersSideIndex[indexMinDist]))
            {
                normal *= -1f;
            }

            cache0.Clear();
            return true;
        }
        public static bool CollidePolygoneCapsule(Polygone polygone, Capsule capsule) => CollideCirclePolygone(capsule.circle1, polygone) || CollideCirclePolygone(capsule.circle2, polygone) || CollidePolygoneHitbox(polygone, capsule.hitbox);
        public static bool CollidePolygoneCapsule(Polygone polygone, Capsule capsule, out Vector2 collisionPoint)
        {
            float distance = capsule.circle1.center.Distance(capsule.circle2.center);
            Vector2 dirHori = (capsule.circle1.center - capsule.circle2.center) / distance;
            Vector2 dirVerti = dirHori.NormalVector();
            distance *= 0.5f;
            Line2D line1 = new Line2D(capsule.hitbox.center + (dirVerti * capsule.circle1.radius) - (dirHori * distance), capsule.hitbox.center + (dirVerti * capsule.circle1.radius) + (dirHori * distance));
            Line2D line2 = new Line2D(capsule.hitbox.center - (dirVerti * capsule.circle1.radius) - (dirHori * distance), capsule.hitbox.center - (dirVerti * capsule.circle1.radius) + (dirHori * distance));
            Vector2 inter;
            Vector2 vertices1, vertices2;
            Vector2[] circleInter;

            for (int i = 0; i < polygone.vertices.Length; i++)
            {
                vertices1 = polygone.vertices[i];
                vertices2 = polygone.vertices[(i + 1) % polygone.vertices.Length];
                if (CollideLines(vertices1, vertices2, line1.A, line1.B, out inter))
                {
                    cache1.Add(inter);
                }
                if (CollideLines(vertices1, vertices2, line2.A, line2.B, out inter))
                {
                    cache1.Add(inter);
                }

                circleInter = ComputeCircleLineIntersections(capsule.circle1, vertices1, vertices2);
                for (int j = 0; j < circleInter.Length; j++)
                {
                    if (dirHori.Dot(circleInter[j] - capsule.circle1.center) > 0f)
                        cache1.Add(circleInter[j]);
                }
                circleInter = ComputeCircleLineIntersections(capsule.circle2, vertices1, vertices2);
                for (int j = 0; j < circleInter.Length; j++)
                {
                    if (dirHori.Dot(circleInter[j] - capsule.circle2.center) < 0f)
                        cache1.Add(circleInter[j]);
                }
            }

            if (cache1.Count > 0)
            {
                collisionPoint = Vector2.zero;
                foreach (Vector2 pos in cache1)
                {
                    collisionPoint += pos;
                }
                collisionPoint /= cache1.Count;
                cache1.Clear();
                return true;
            }

            bool contain = capsule.inclusiveCircle.radius >= polygone.inclusiveCircle.radius ? capsule.Contains(polygone.center) : polygone.Contains(capsule.center);
            if (contain)
            {
                collisionPoint = (capsule.center + polygone.center) * 0.5f;
                return true;
            }

            collisionPoint = Vector2.zero;
            return false;
        }
        public static bool CollidePolygoneCapsule(Polygone polygone, Capsule capsule, out Vector2 collisionPoint, out Vector2 normal1, out Vector2 normal2)
        {
            float distance = capsule.circle1.center.Distance(capsule.circle2.center);
            Vector2 dirHori = (capsule.circle1.center - capsule.circle2.center) / distance;
            Vector2 dirVerti = dirHori.NormalVector();
            distance *= 0.5f;
            Line2D line1 = new Line2D(capsule.hitbox.center + (dirVerti * capsule.circle1.radius) - (dirHori * distance), capsule.hitbox.center + (dirVerti * capsule.circle1.radius) + (dirHori * distance));
            Line2D line2 = new Line2D(capsule.hitbox.center - (dirVerti * capsule.circle1.radius) - (dirHori * distance), capsule.hitbox.center - (dirVerti * capsule.circle1.radius) + (dirHori * distance));
            Vector2 vertices1, vertices2, inter, n = Vector2.zero;
            Vector2[] circleInter;

            for (int i = 0; i < polygone.vertices.Length; i++)
            {
                vertices1 = polygone.vertices[i];
                vertices2 = polygone.vertices[(i + 1) % polygone.vertices.Length];
                bool nCompute = false;
                if (CollideLines(vertices1, vertices2, line1.A, line1.B, out inter))
                {
                    cache1.Add(inter);
                    n = (vertices2 - vertices1).NormalVector();
                    if(!polygone.IsNormalOnRightDirection(inter, n, i))
                    {
                        n *= -1f;
                    }
                    cache2.Add(n);
                    nCompute = true;
                }

                if (CollideLines(vertices1, vertices2, line2.A, line2.B, out inter))
                {
                    cache1.Add(inter);

                    if(!nCompute)
                    {
                        n = (vertices2 - vertices1).NormalVector();
                        if (!polygone.IsNormalOnRightDirection(inter, n, i))
                        {
                            n *= -1f;
                        }
                    }
                    cache2.Add(n);
                }

                circleInter = ComputeCircleLineIntersections(capsule.circle1, vertices1, vertices2);
                for (int j = 0; j < circleInter.Length; j++)
                {
                    n = capsule.circle1.center - circleInter[j];
                    if (dirHori.Dot(n) < 0f)
                    {
                        cache1.Add(circleInter[j]);
                        cache2.Add(n * (1f / capsule.circle1.radius));
                    }
                }

                circleInter = ComputeCircleLineIntersections(capsule.circle2, vertices1, vertices2);
                for (int j = 0; j < circleInter.Length; j++)
                {
                    n = capsule.circle2.center - circleInter[j];
                    if (dirHori.Dot(n) > 0f)
                    {
                        cache1.Add(circleInter[j]);
                        cache2.Add(n * (1f / capsule.circle2.radius));
                    }
                }
            }

            if (cache1.Count > 0)
            {
                collisionPoint = Vector2.zero;
                foreach (Vector2 pos in cache1)
                {
                    collisionPoint += pos;
                }
                collisionPoint /= cache1.Count;

                normal1 = Vector2.zero;
                foreach (Vector2 point in cache2)
                {
                    normal1 += point;
                }
                normal1.Normalize();
                normal2 = -normal1;

                cache1.Clear();
                cache2.Clear();
                return true;
            }

            bool contain = capsule.inclusiveCircle.radius >= polygone.inclusiveCircle.radius ? capsule.Contains(polygone.center) : polygone.Contains(capsule.center);
            if (contain)
            {
                collisionPoint = (capsule.center + polygone.center) * 0.5f;
                normal1 = capsule.center - polygone.center;
                normal1.Normalize();
                normal2 = -normal1;
                return true;
            }

            collisionPoint = normal1 = normal2 = Vector2.zero;
            return false;
        }

        #endregion

        #region Collide(Hitbox, other)

        public static bool CollideHitboxes(Hitbox hitbox1, Hitbox hitbox2) => CollidePolygones(hitbox1.ToPolygone(), hitbox2.ToPolygone());
        public static bool CollideHitboxes(Hitbox hitbox1, Hitbox hitbox2, out Vector2 collisionPoint) => CollidePolygones(hitbox1.ToPolygone(), hitbox2.ToPolygone(), out collisionPoint);
        public static bool CollideHitboxes(Hitbox hitbox1, Hitbox hitbox2, out Vector2 collisionPoint, out Vector2 normal1, out Vector2 normal2) => CollidePolygones(hitbox1.ToPolygone(), hitbox2.ToPolygone(), out collisionPoint, out normal1, out normal2);
        public static bool CollideHitboxLine(Hitbox hitbox, in Vector2 A, in Vector2 B) => CollidePolygoneLine(hitbox.ToPolygone(), A, B);
        public static bool CollideHitboxLine(Hitbox hitbox, Line2D line) => CollidePolygoneLine(hitbox.ToPolygone(), line.A, line.B);
        public static bool CollideHitboxLine(Hitbox hitbox, in Vector2 A, in Vector2 B, out Vector2 collisionPoint) => CollidePolygoneLine(hitbox.ToPolygone(), A, B, out collisionPoint);
        public static bool CollideHitboxLine(Hitbox hitbox, Line2D line, out Vector2 collisionPoint) => CollidePolygoneLine(hitbox.ToPolygone(), line.A, line.B, out collisionPoint);
        public static bool CollideHitboxLine(Hitbox hitbox, in Vector2 A, in Vector2 B, out Vector2 collisionPoint, out Vector2 normal) => CollidePolygoneLine(hitbox.ToPolygone(), A, B, out collisionPoint, out normal);
        public static bool CollideHitboxLine(Hitbox hitbox, Line2D line, out Vector2 collisionPoint, out Vector2 normal) => CollidePolygoneLine(hitbox.ToPolygone(), line.A, line.B, out collisionPoint, out normal);
        public static bool CollideHitboxStraigthLine(Hitbox hitbox, StraightLine2D straigthLine) => CollidePolygoneStaightLine(hitbox.ToPolygone(), straigthLine.A, straigthLine.B);
        public static bool CollideHitboxStraigthLine(Hitbox hitbox, in Vector2 A, in Vector2 B) => CollidePolygoneStaightLine(hitbox.ToPolygone(), A, B);
        public static bool CollideHitboxStraigthLine(Hitbox hitbox, in Vector2 A, in Vector2 B, out Vector2 collisionPoint) => CollidePolygoneStaightLine(hitbox.ToPolygone(), A, B, out collisionPoint);
        public static bool CollideHitboxStraigthLine(Hitbox hitbox, StraightLine2D straigthLine, out Vector2 collisionPoint) => CollidePolygoneStaightLine(hitbox.ToPolygone(), straigthLine.A, straigthLine.B, out collisionPoint);
        public static bool CollideHitboxStraigthLine(Hitbox hitbox, in Vector2 A, in Vector2 B, out Vector2 collisionPoint, out Vector2 normal) => CollidePolygoneStaightLine(hitbox.ToPolygone(), A, B, out collisionPoint, out normal);
        public static bool CollideHitboxStraigthLine(Hitbox hitbox, StraightLine2D straigthLine, out Vector2 collisionPoint, out Vector2 normal) => CollidePolygoneStaightLine(hitbox.ToPolygone(), straigthLine.A, straigthLine.B, out collisionPoint, out normal);
        public static bool CollideHitboxRay(Hitbox hitbox, Ray2D ray) => CollidePolygoneRay(hitbox.ToPolygone(), ray);
        public static bool CollideHitboxRay(Hitbox hitbox, in Vector2 start, in Vector2 end) => CollidePolygoneRay(hitbox.ToPolygone(), start, end);
        public static bool CollideHitboxRay(Hitbox hitbox, Ray2D ray, out Vector2 collisionPoint) => CollidePolygoneRay(hitbox.ToPolygone(), ray, out collisionPoint);
        public static bool CollideHitboxRay(Hitbox hitbox, in Vector2 start, in Vector2 end, out Vector2 collisionPoint) => CollidePolygoneRay(hitbox.ToPolygone(), start, end, out collisionPoint);
        public static bool CollideHitboxRay(Hitbox hitbox, Ray2D ray, out Vector2 collisionPoint, out Vector2 normal) => CollidePolygoneRay(hitbox.ToPolygone(), ray, out collisionPoint, out normal);
        public static bool CollideHitboxRay(Hitbox hitbox, in Vector2 start, in Vector2 end, out Vector2 collisionPoint, out Vector2 normal) => CollidePolygoneRay(hitbox.ToPolygone(), start, end, out collisionPoint, out normal);
        public static bool CollideHitboxCapsule(Hitbox hitbox, Capsule capule) => CollideCircleHitbox(capule.circle1, hitbox) || CollideCircleHitbox(capule.circle2, hitbox) || CollideHitboxes(hitbox, capule.hitbox);
        public static bool CollideHitboxCapsule(Hitbox hitbox, Capsule capsule, out Vector2 collisionPoint) => CollidePolygoneCapsule(hitbox.ToPolygone(), capsule, out collisionPoint);
        public static bool CollideHitboxCapsule(Hitbox hitbox, Capsule capsule, out Vector2 collisionPoint, out Vector2 normal1, out Vector2 normal2) => CollidePolygoneCapsule(hitbox.ToPolygone(), capsule, out collisionPoint, out normal1, out normal2);

        #endregion

        #region Collide(Capsule, other)

        public static bool CollideCapsules(Capsule capsule1, Capsule capsule2) =>  CollideCircleCapsule(capsule1.circle1, capsule2) || CollideCircleCapsule(capsule1.circle2, capsule2) || CollideHitboxCapsule(capsule1.hitbox, capsule2);
        public static bool CollideCapsules(Capsule capsule1, Capsule capsule2, out Vector2 collisionPoint)
        {
            if (CollideHitboxes(capsule1.hitbox, capsule2.hitbox, out collisionPoint))
            {
                cache0.Add(collisionPoint);
            }
            if (CollideCircleHitbox(capsule2.circle1, capsule1.hitbox, out collisionPoint))
            {
                cache0.Add(collisionPoint);
            }
            if (CollideCircleHitbox(capsule2.circle2, capsule1.hitbox, out collisionPoint))
            {
                cache0.Add(collisionPoint);
            }
            if (CollideCircleHitbox(capsule1.circle1, capsule2.hitbox, out collisionPoint))
            {
                cache0.Add(collisionPoint);
            }
            if (CollideCircles(capsule1.circle1, capsule2.circle1, out collisionPoint))
            {
                cache0.Add(collisionPoint);
            }
            if (CollideCircles(capsule1.circle1, capsule2.circle2, out collisionPoint))
            {
                cache0.Add(collisionPoint);
            }
            if (CollideCircleHitbox(capsule1.circle2, capsule2.hitbox, out collisionPoint))
            {
                cache0.Add(collisionPoint);
            }
            if (CollideCircles(capsule1.circle2, capsule2.circle1, out collisionPoint))
            {
                cache0.Add(collisionPoint);
            }
            if (CollideCircles(capsule1.circle2, capsule2.circle2, out collisionPoint))
            {
                cache0.Add(collisionPoint);
            }
            collisionPoint = Vector2.zero;
            if (cache0.Count > 0)
            {
                foreach (Vector2 pos in cache0)
                {
                    collisionPoint += pos;
                }
                collisionPoint /= cache0.Count;
                cache0.Clear();
                return true;
            }
            return false;
        }
        public static bool CollideCapsules(Capsule capsule1, Capsule capsule2, out Vector2 collisionPoint, out Vector2 normal1, out Vector2 normal2)
        {
            if (CollideCapsules(capsule1, capsule2, out collisionPoint))
            {
                normal1 = collisionPoint - capsule1.center;
                normal1.Normalize();
                normal2 = collisionPoint - capsule2.center;
                normal2.Normalize();
                return true;
            }
            normal1 = normal2 = Vector2.zero;
            return false;
        }
        public static bool CollideCapsuleLine(Capsule capsule, Line2D line) => CollideCapsuleLine(capsule, line.A, line.B);
        public static bool CollideCapsuleLine(Capsule capsule, in Vector2 A, in Vector2 B)
        {
            return CollideCircleLine(capsule.circle1, A, B) || CollideCircleLine(capsule.circle2, A, B) || CollideHitboxLine(capsule.hitbox, A, B);
        }
        public static bool CollideCapsuleLine(Capsule capsule, in Vector2 A, in Vector2 B, out Vector2 collisionPoint)
        {
            float distance = capsule.circle1.center.Distance(capsule.circle2.center);
            Vector2 dirHori = (capsule.circle1.center - capsule.circle2.center) / distance;
            Vector2 dirVerti = dirHori.NormalVector();
            distance *= 0.5f;
            Line2D line1 = new Line2D(capsule.hitbox.center + (dirVerti * capsule.circle1.radius) - (dirHori * distance), capsule.hitbox.center + (dirVerti * capsule.circle1.radius) + (dirHori * distance));
            Line2D line2 = new Line2D(capsule.hitbox.center - (dirVerti * capsule.circle1.radius) - (dirHori * distance), capsule.hitbox.center - (dirVerti * capsule.circle1.radius) + (dirHori * distance));
            Vector2 inter, inter2;
            Vector2[] circleInters;

            if(CollideLines(line1.A, line1.B, A, B, out inter))
            {
                cache0.Add(inter);
            }
            if (CollideLines(line2.A, line2.B, A, B, out inter))
            {
                cache0.Add(inter);
            }

            circleInters = ComputeCircleLineIntersections(capsule.circle1, A, B);
            for (int i = 0; i < circleInters.Length; i++)
            {
                if (dirHori.Dot(circleInters[i] - capsule.circle1.center) > 0f)
                {
                    cache0.Add(circleInters[i]);
                }
            }
            circleInters = ComputeCircleLineIntersections(capsule.circle2, A, B);
            for (int i = 0; i < circleInters.Length; i++)
            {
                if (dirHori.Dot(circleInters[i] - capsule.circle2.center) < 0f)
                {
                    cache0.Add(circleInters[i]);
                }
            }

            if(cache0.Count <= 1)
            {
                if (cache0.Count == 1)
                {
                    collisionPoint = cache0[0];
                    cache0.Clear();
                    return true;
                }

                if (capsule.Contains(A))
                {
                    if (CollideLineStraightLine(line1.A, line1.B, A, B, out inter))
                    {
                        cache0.Add(inter);
                    }
                    if (CollideLineStraightLine(line2.A, line2.B, A, B, out inter))
                    {
                        cache0.Add(inter);
                    }

                    if(ComputeCircleStraightLineIntersections(capsule.circle1, A, B, out inter, out inter2))
                    {
                        if (dirHori.Dot(inter - capsule.circle1.center) > 0f)
                        {
                            cache0.Add(inter);
                        }
                        if (dirHori.Dot(inter2 - capsule.circle1.center) > 0f)
                        {
                            cache0.Add(inter2);
                        }
                    }
                    if (ComputeCircleStraightLineIntersections(capsule.circle2, A, B, out inter, out inter2))
                    {
                        if (dirHori.Dot(inter - capsule.circle2.center) < 0f)
                        {
                            cache0.Add(inter);
                        }
                        if (dirHori.Dot(inter2 - capsule.circle2.center) < 0f)
                        {
                            cache0.Add(inter2);
                        }
                    }
                }
                else
                {
                    collisionPoint = Vector2.zero;
                    return false;
                }
            }

            collisionPoint = cache0[0];
            for (int i = 1; i < cache0.Count; i++)
            {
                collisionPoint += cache0[i];
            }
            collisionPoint /= cache0.Count;

            cache0.Clear();
            Vector2 otherPoint = collisionPoint + (B - A).NormalVector();

            if (CollideLineStraightLine(line1.A, line1.B, collisionPoint, otherPoint, out inter))
            {
                cache0.Add(inter);
            }
            if (CollideLineStraightLine(line2.A, line2.B, collisionPoint, otherPoint, out inter))
            {
                cache0.Add(inter);
            }

            if(ComputeCircleStraightLineIntersections(capsule.circle1, collisionPoint, otherPoint, out inter, out inter2))
            {
                if (dirHori.Dot(inter - capsule.circle1.center) > 0f)
                {
                    cache0.Add(inter);
                }
                if (dirHori.Dot(inter2 - capsule.circle1.center) > 0f)
                {
                    cache0.Add(inter2);
                }
            }
            if (ComputeCircleStraightLineIntersections(capsule.circle2, collisionPoint, otherPoint, out inter, out inter2))
            {
                if (dirHori.Dot(inter - capsule.circle2.center) < 0f)
                {
                    cache0.Add(inter);
                }
                if (dirHori.Dot(inter2 - capsule.circle2.center) < 0f)
                {
                    cache0.Add(inter2);
                }
            }

            if(cache0.Count <= 0)
            {
                collisionPoint = Vector2.zero;
                return false;
            }

            inter = cache0[0];
            float minSqrDistance = collisionPoint.SqrDistance(inter);
            float currentSqrDistance;

            for (int i = 1; i < cache0.Count; i++)
            {
                currentSqrDistance = collisionPoint.SqrDistance(cache0[i]);
                if(currentSqrDistance < minSqrDistance)
                {
                    minSqrDistance = currentSqrDistance;
                    inter = cache0[i];
                }
            }

            collisionPoint = inter;
            cache0.Clear();
            return true;
        }
        public static bool CollideCapsuleLine(Capsule capsule, Line2D line, out Vector2 collisionPoint) => CollideCapsuleLine(capsule, line.A, line.B, out collisionPoint);
        public static bool CollideCapsuleLine(Capsule capsule, in Vector2 A, in Vector2 B, out Vector2 collisionPoint, out Vector2 normal)
        {
            float distance = capsule.circle1.center.Distance(capsule.circle2.center);
            Vector2 dirHori = (capsule.circle1.center - capsule.circle2.center) / distance;
            Vector2 dirVerti = dirHori.NormalVector();
            distance *= 0.5f;
            Line2D line1 = new Line2D(capsule.hitbox.center + (dirVerti * capsule.circle1.radius) - (dirHori * distance), capsule.hitbox.center + (dirVerti * capsule.circle1.radius) + (dirHori * distance));
            Line2D line2 = new Line2D(capsule.hitbox.center - (dirVerti * capsule.circle1.radius) - (dirHori * distance), capsule.hitbox.center - (dirVerti * capsule.circle1.radius) + (dirHori * distance));
            Vector2 inter, inter2;
            Vector2[] circleInters;
            normal = Vector2.zero;

            if (CollideLines(line1.A, line1.B, A, B, out inter))
            {
                cache0.Add(inter);
                normal = (line1.B - line1.A).NormalVector();
                if(normal.Dot(inter - capsule.center) < 0f)
                {
                    normal *= -1f;
                }
            }
            if (CollideLines(line2.A, line2.B, A, B, out inter))
            {
                cache0.Add(inter);
                normal = (line1.B - line1.A).NormalVector();
                if (normal.Dot(inter - capsule.center) < 0f)
                {
                    normal *= -1f;
                }
            }

            circleInters = ComputeCircleLineIntersections(capsule.circle1, A, B);
            for (int i = 0; i < circleInters.Length; i++)
            {
                if (dirHori.Dot(circleInters[i] - capsule.circle1.center) > 0f)
                {
                    cache0.Add(circleInters[i]);
                    normal = (circleInters[i] - capsule.circle1.center).normalized;
                }
            }
            circleInters = ComputeCircleLineIntersections(capsule.circle2, A, B);
            for (int i = 0; i < circleInters.Length; i++)
            {
                if (dirHori.Dot(circleInters[i] - capsule.circle2.center) < 0f)
                {
                    cache0.Add(circleInters[i]);
                    normal = (circleInters[i] - capsule.circle2.center).normalized;
                }
            }

            if (cache0.Count <= 1)
            {
                if (cache0.Count == 1)
                {
                    collisionPoint = cache0[0];
                    cache0.Clear();
                    return true;
                }

                if (capsule.Contains(A))
                {
                    if (CollideLineStraightLine(line1.A, line1.B, A, B, out inter))
                    {
                        cache0.Add(inter);
                    }
                    if (CollideLineStraightLine(line2.A, line2.B, A, B, out inter))
                    {
                        cache0.Add(inter);
                    }

                    if (ComputeCircleStraightLineIntersections(capsule.circle1, A, B, out inter, out inter2))
                    {
                        if (dirHori.Dot(inter - capsule.circle1.center) > 0f)
                        {
                            cache0.Add(inter);
                        }
                        if (dirHori.Dot(inter2 - capsule.circle1.center) > 0f)
                        {
                            cache0.Add(inter2);
                        }
                    }
                    if (ComputeCircleStraightLineIntersections(capsule.circle2, A, B, out inter, out inter2))
                    {
                        if (dirHori.Dot(inter - capsule.circle2.center) < 0f)
                        {
                            cache0.Add(inter);
                        }
                        if (dirHori.Dot(inter2 - capsule.circle2.center) < 0f)
                        {
                            cache0.Add(inter2);
                        }
                    }
                }
                else
                {
                    collisionPoint = Vector2.zero;
                    return false;
                }
            }

            if (cache0.Count <= 0)
            {
                collisionPoint = Vector2.zero;
                return false;
            }

            collisionPoint = cache0[0];
            for (int i = 1; i < cache0.Count; i++)
            {
                collisionPoint += cache0[i];
            }
            collisionPoint /= cache0.Count;

            cache0.Clear();
            Vector2 otherPoint = collisionPoint + (B - A).NormalVector();

            if (CollideLineStraightLine(line1.A, line1.B, collisionPoint, otherPoint, out inter))
            {
                cache0.Add(inter);
                normal = (line1.B - line1.A).NormalVector();
                if (normal.Dot(inter - capsule.center) < 0f)
                {
                    normal *= -1f;
                }
                cache1.Add(normal);
            }
            if (CollideLineStraightLine(line2.A, line2.B, collisionPoint, otherPoint, out inter))
            {
                cache0.Add(inter);
                normal = (line2.B - line2.A).NormalVector();
                if (normal.Dot(inter - capsule.center) < 0f)
                {
                    normal *= -1f;
                }
                cache1.Add(normal);
            }

            if (ComputeCircleStraightLineIntersections(capsule.circle1, collisionPoint, otherPoint, out inter, out inter2))
            {
                if (dirHori.Dot(inter - capsule.circle1.center) > 0f)
                {
                    cache0.Add(inter);
                    cache1.Add((inter - capsule.circle1.center).normalized);
                }
                if (dirHori.Dot(inter2 - capsule.circle1.center) > 0f)
                {
                    cache0.Add(inter2);
                    cache1.Add((inter2 - capsule.circle1.center).normalized);
                }
            }
            if (ComputeCircleStraightLineIntersections(capsule.circle2, collisionPoint, otherPoint, out inter, out inter2))
            {
                if (dirHori.Dot(inter - capsule.circle2.center) < 0f)
                {
                    cache0.Add(inter);
                    cache1.Add((inter - capsule.circle2.center).normalized);
                }
                if (dirHori.Dot(inter2 - capsule.circle2.center) < 0f)
                {
                    cache0.Add(inter2);
                    cache1.Add((inter2 - capsule.circle2.center).normalized);
                }
            }

            inter = cache0[0];
            float minSqrDistance = collisionPoint.SqrDistance(inter);
            float currentSqrDistance;
            int minIndex = 0;

            for (int i = 1; i < cache0.Count; i++)
            {
                currentSqrDistance = collisionPoint.SqrDistance(cache0[i]);
                if (currentSqrDistance < minSqrDistance)
                {
                    minSqrDistance = currentSqrDistance;
                    inter = cache0[i];
                    minIndex = 1;
                }
            }

            collisionPoint = inter;
            normal = cache1[minIndex];
            cache0.Clear();
            cache1.Clear();
            return true;
        }
        public static bool CollideCapsuleLine(Capsule capsule, Line2D line, out Vector2 collisionPoint, out Vector2 normal) => CollideCapsuleLine(capsule, line.A, line.B, out collisionPoint, out normal);
        public static bool CollideCapsuleStraightLine(Capsule capsule, StraightLine2D straightLine) => CollideCapsuleStraightLine(capsule, straightLine.A, straightLine.B);
        public static bool CollideCapsuleStraightLine(Capsule capsule, in Vector2 A, in Vector2 B)
        {
            return  CollideCircleStraightLine(capsule.circle1, A, B) || CollideCircleStraightLine(capsule.circle2, A, B) || CollideHitboxStraigthLine(capsule.hitbox, A, B);
        }
        public static bool CollideCapsuleStraightLine(Capsule capsule, in Vector2 A, in Vector2 B, out Vector2 collisionPoint)
        {
            float distance = capsule.circle1.center.Distance(capsule.circle2.center);
            Vector2 dirHori = (capsule.circle1.center - capsule.circle2.center) / distance;
            Vector2 dirVerti = dirHori.NormalVector();
            distance *= 0.5f;
            Line2D line1 = new Line2D(capsule.hitbox.center + (dirVerti * capsule.circle1.radius) - (dirHori * distance), capsule.hitbox.center + (dirVerti * capsule.circle1.radius) + (dirHori * distance));
            Line2D line2 = new Line2D(capsule.hitbox.center - (dirVerti * capsule.circle1.radius) - (dirHori * distance), capsule.hitbox.center - (dirVerti * capsule.circle1.radius) + (dirHori * distance));
            Vector2 inter, inter2;

            if (CollideLineStraightLine(line1.A, line1.B, A, B, out inter))
            {
                cache0.Add(inter);
            }
            if (CollideLineStraightLine(line2.A, line2.B, A, B, out inter))
            {
                cache0.Add(inter);
            }

            if(ComputeCircleStraightLineIntersections(capsule.circle1, A, B, out inter, out inter2))
            {
                if (dirHori.Dot(inter - capsule.circle1.center) > 0f)
                {
                    cache0.Add(inter);
                }
                if (dirHori.Dot(inter2 - capsule.circle1.center) > 0f)
                {
                    cache0.Add(inter2);
                }
            }
            if (ComputeCircleStraightLineIntersections(capsule.circle2, A, B, out inter, out inter2))
            {
                if (dirHori.Dot(inter - capsule.circle2.center) < 0f)
                {
                    cache0.Add(inter);
                }
                if (dirHori.Dot(inter2 - capsule.circle2.center) < 0f)
                {
                    cache0.Add(inter2);
                }
            }

            if (cache0.Count <= 0)
            {
                collisionPoint = Vector2.zero;
                return false;
            }

            collisionPoint = cache0[0];
            for (int i = 1; i < cache0.Count; i++)
            {
                collisionPoint += cache0[i];
            }
            collisionPoint /= cache0.Count;

            cache0.Clear();
            Vector2 otherPoint = collisionPoint + (B - A).NormalVector();

            if (CollideLineStraightLine(line1.A, line1.B, collisionPoint, otherPoint, out inter))
            {
                cache0.Add(inter);
            }
            if (CollideLineStraightLine(line2.A, line2.B, collisionPoint, otherPoint, out inter))
            {
                cache0.Add(inter);
            }

            if (ComputeCircleStraightLineIntersections(capsule.circle1, collisionPoint, otherPoint, out inter, out inter2))
            {
                if (dirHori.Dot(inter - capsule.circle1.center) > 0f)
                {
                    cache0.Add(inter);
                }
                if (dirHori.Dot(inter2 - capsule.circle1.center) > 0f)
                {
                    cache0.Add(inter2);
                }
            }
            if (ComputeCircleStraightLineIntersections(capsule.circle2, collisionPoint, otherPoint, out inter, out inter2))
            {
                if (dirHori.Dot(inter - capsule.circle2.center) < 0f)
                {
                    cache0.Add(inter);
                }
                if (dirHori.Dot(inter2 - capsule.circle2.center) < 0f)
                {
                    cache0.Add(inter2);
                }
            }

            if (cache0.Count <= 0)
            {
                collisionPoint = Vector2.zero;
                return false;
            }

            inter = cache0[0];
            float minSqrDistance = collisionPoint.SqrDistance(inter);
            float currentSqrDistance;

            for (int i = 1; i < cache0.Count; i++)
            {
                currentSqrDistance = collisionPoint.SqrDistance(cache0[i]);
                if (currentSqrDistance < minSqrDistance)
                {
                    minSqrDistance = currentSqrDistance;
                    inter = cache0[i];
                }
            }

            collisionPoint = inter;
            cache0.Clear();
            return true;
        }
        public static bool CollideCapsuleStraightLine(Capsule capsule, StraightLine2D straightLine, out Vector2 collisionPoint) => CollideCapsuleStraightLine(capsule, straightLine.A, straightLine.B, out collisionPoint);
        public static bool CollideCapsuleStraightLine(Capsule capsule, in Vector2 A, in Vector2 B, out Vector2 collisionPoint, out Vector2 normal)
        {
            float distance = capsule.circle1.center.Distance(capsule.circle2.center);
            Vector2 dirHori = (capsule.circle1.center - capsule.circle2.center) / distance;
            Vector2 dirVerti = dirHori.NormalVector();
            distance *= 0.5f;
            Line2D line1 = new Line2D(capsule.hitbox.center + (dirVerti * capsule.circle1.radius) - (dirHori * distance), capsule.hitbox.center + (dirVerti * capsule.circle1.radius) + (dirHori * distance));
            Line2D line2 = new Line2D(capsule.hitbox.center - (dirVerti * capsule.circle1.radius) - (dirHori * distance), capsule.hitbox.center - (dirVerti * capsule.circle1.radius) + (dirHori * distance));
            Vector2 inter, inter2;

            if (CollideLines(line1.A, line1.B, A, B, out inter))
            {
                cache0.Add(inter);
            }
            if (CollideLines(line2.A, line2.B, A, B, out inter))
            {
                cache0.Add(inter);
            }

            if (ComputeCircleStraightLineIntersections(capsule.circle1, A, B, out inter, out inter2))
            {
                if (dirHori.Dot(inter - capsule.circle1.center) > 0f)
                {
                    cache0.Add(inter);
                }
                if (dirHori.Dot(inter2 - capsule.circle1.center) > 0f)
                {
                    cache0.Add(inter2);
                }
            }
            if (ComputeCircleStraightLineIntersections(capsule.circle2, A, B, out inter, out inter2))
            {
                if (dirHori.Dot(inter - capsule.circle2.center) < 0f)
                {
                    cache0.Add(inter);
                }
                if (dirHori.Dot(inter2 - capsule.circle2.center) < 0f)
                {
                    cache0.Add(inter2);
                }
            }

            if (cache0.Count <= 0)
            {
                collisionPoint = normal = Vector2.zero;
                return false;
            }

            collisionPoint = cache0[0];
            for (int i = 1; i < cache0.Count; i++)
            {
                collisionPoint += cache0[i];
            }
            collisionPoint /= cache0.Count;

            cache0.Clear();
            Vector2 otherPoint = collisionPoint + (B - A).NormalVector();

            if (CollideLineStraightLine(line1.A, line1.B, collisionPoint, otherPoint, out inter))
            {
                cache0.Add(inter);
                normal = (line1.B - line1.A).NormalVector();
                if (normal.Dot(inter - capsule.center) < 0f)
                {
                    normal *= -1f;
                }
                cache1.Add(normal);
            }
            if (CollideLineStraightLine(line2.A, line2.B, collisionPoint, otherPoint, out inter))
            {
                cache0.Add(inter);
                normal = (line2.B - line2.A).NormalVector();
                if (normal.Dot(inter - capsule.center) < 0f)
                {
                    normal *= -1f;
                }
                cache1.Add(normal);
            }

            if (ComputeCircleStraightLineIntersections(capsule.circle1, collisionPoint, otherPoint, out inter, out inter2))
            {
                if (dirHori.Dot(inter - capsule.circle1.center) > 0f)
                {
                    cache0.Add(inter);
                    cache1.Add((inter - capsule.circle1.center).normalized);
                }
                if (dirHori.Dot(inter2 - capsule.circle1.center) > 0f)
                {
                    cache0.Add(inter2);
                    cache1.Add((inter2 - capsule.circle1.center).normalized);
                }
            }
            if (ComputeCircleStraightLineIntersections(capsule.circle2, collisionPoint, otherPoint, out inter, out inter2))
            {
                if (dirHori.Dot(inter - capsule.circle2.center) < 0f)
                {
                    cache0.Add(inter);
                    cache1.Add((inter - capsule.circle2.center).normalized);
                }
                if (dirHori.Dot(inter2 - capsule.circle2.center) < 0f)
                {
                    cache0.Add(inter2);
                    cache1.Add((inter2 - capsule.circle2.center).normalized);
                }
            }

            if (cache0.Count <= 0)
            {
                collisionPoint = normal = Vector2.zero;
                return false;
            }

            inter = cache0[0];
            float minSqrDistance = collisionPoint.SqrDistance(inter);
            float currentSqrDistance;
            int minIndex = 0;

            for (int i = 1; i < cache0.Count; i++)
            {
                currentSqrDistance = collisionPoint.SqrDistance(cache0[i]);
                if (currentSqrDistance < minSqrDistance)
                {
                    minSqrDistance = currentSqrDistance;
                    inter = cache0[i];
                    minIndex = 1;
                }
            }

            collisionPoint = inter;
            normal = cache1[minIndex];
            cache0.Clear();
            cache1.Clear();
            return true;
        }
        public static bool CollideCapsuleStraightLine(Capsule capsule, StraightLine2D straightLine, out Vector2 collisionPoint, out Vector2 normal) => CollideCapsuleStraightLine(capsule, straightLine.A, straightLine.B, out collisionPoint, out normal);
        public static bool CollideCapsuleRay(Capsule capsule, Ray2D ray) => CollideCapsuleRay(capsule, ray.start, ray.end);
        public static bool CollideCapsuleRay(Capsule capsule, in Vector2 start, in Vector2 end) => CollideCapsuleLine(capsule, start, end);
        public static bool CollideCapsuleRay(Capsule capsule, Ray2D ray, out Vector2 collisionPoint) => CollideCapsuleRay(capsule, ray.start, ray.end, out collisionPoint);
        public static bool CollideCapsuleRay(Capsule capsule, in Vector2 start, in Vector2 end, out Vector2 collisionPoint)
        {
            Vector2 beg = start + (start - end).normalized * (2f * capsule.inclusiveCircle.radius);
            Vector2 cp1, cp2;
            if(CollideCircleRay(capsule.circle1, start, end, out Vector2 cp0))
            {
                if (CollideCircleRay(capsule.circle2, start, end, out cp1))
                {
                    float d0 = beg.SqrDistance(cp0);
                    float d1 = beg.SqrDistance(cp1);
                    if (CollideHitboxRay(capsule.hitbox, start, end, out cp2))
                    {
                        float d2 = beg.SqrDistance(cp2);
                        collisionPoint = d0 <= d1 ? (d0 <= d2 ? cp0 : cp2) : (d1 <= d2 ? cp1 : cp2);
                        return true;
                    }

                    collisionPoint = d0 <= d1 ? cp0 : cp1;
                    return true;
                }

                if (CollideHitboxRay(capsule.hitbox, start, end, out cp2))
                {
                    float d0 = beg.SqrDistance(cp0);
                    float d2 = beg.SqrDistance(cp2);
                    collisionPoint = d0 <= d2 ? cp0 : cp2;
                    return true;
                }

                collisionPoint = cp0;
                return true;
            }

            if (CollideCircleRay(capsule.circle2, start, end, out cp1))
            {
                if (CollideHitboxRay(capsule.hitbox, start, end, out cp2))
                {
                    float d1 = beg.SqrDistance(cp1);
                    float d2 = beg.SqrDistance(cp2);
                    collisionPoint = d1 <= d2 ? cp1 : cp2;  
                    return true;
                }

                collisionPoint = cp1;
                return true;
            }

            if (CollideHitboxRay(capsule.hitbox, start, end, out cp2))
            {
                collisionPoint = cp2;
                return true;
            }

            collisionPoint = Vector2.zero;
            return false;
        }
        public static bool CollideCapsuleRay(Capsule capsule, Ray2D ray, out Vector2 collisionPoint, out Vector2 normal) => CollideCapsuleRay(capsule, ray.start, ray.end, out collisionPoint, out normal);
        public static bool CollideCapsuleRay(Capsule capsule, in Vector2 start, in Vector2 end, out Vector2 collisionPoint, out Vector2 normal)
        {
            Vector2 beg = start + (start - end).normalized * (2f * capsule.inclusiveCircle.radius);
            Vector2 cp1, cp2;
            Vector2 n1, n2;
            if (CollideCircleRay(capsule.circle1, start, end, out Vector2 cp0, out Vector2 n0))
            {
                if (CollideCircleRay(capsule.circle2, start, end, out cp1, out n1))
                {
                    float d0 = beg.SqrDistance(cp0);
                    float d1 = beg.SqrDistance(cp1);
                    if (CollideHitboxRay(capsule.hitbox, start, end, out cp2, out n2))
                    {
                        float d2 = beg.SqrDistance(cp2);
                        if(d0 <= d1)
                        {
                            if(d0 <= d2)
                            {
                                collisionPoint = cp0;
                                normal = n0;
                            }
                            else
                            {
                                collisionPoint = cp2;
                                normal = n2;
                            }
                        }
                        else
                        {
                            if(d1 <= d2)
                            {
                                collisionPoint = cp1;
                                normal = n1;
                            }
                            else
                            {
                                collisionPoint = cp2;
                                normal = n2;
                            }
                        }
                        return true;
                    }

                    if(d0 <= d1)
                    {
                        collisionPoint = cp0;
                        normal = n0;
                    }
                    else
                    {
                        collisionPoint = cp1;
                        normal = n1;
                    }

                    return true;
                }

                if (CollideHitboxRay(capsule.hitbox, start, end, out cp2, out n2))
                {
                    float d0 = beg.SqrDistance(cp0);
                    float d2 = beg.SqrDistance(cp2);

                    if(d0 <= d2)
                    {
                        collisionPoint = cp0;
                        normal = n0;
                    }
                    else
                    {
                        collisionPoint = cp2;
                        normal = n2;
                    }
                    return true;
                }

                collisionPoint = cp0;
                normal = n0;
                return true;
            }

            if (CollideCircleRay(capsule.circle2, start, end, out cp1, out n1))
            {
                if (CollideHitboxRay(capsule.hitbox, start, end, out cp2, out n2))
                {
                    float d1 = beg.SqrDistance(cp1);
                    float d2 = beg.SqrDistance(cp2);

                    if(d1 <= d2)
                    {
                        collisionPoint = cp1;
                        normal = n1;
                    }
                    else
                    {
                        collisionPoint = cp2;
                        normal = n2;
                    }
                    return true;
                }

                collisionPoint = cp1;
                normal = n1;
                return true;
            }

            if (CollideHitboxRay(capsule.hitbox, start, end, out cp2, out n2))
            {
                collisionPoint = cp2;
                normal = n2;
                return true;
            }

            collisionPoint = normal = Vector2.zero;
            return false;
        }

        #endregion

        #region Collide(Lines, StraightLine, Ray)

        public static bool CollideStraightLines(StraightLine2D straightLine1, StraightLine2D straightLine2) => CollideStraightLines(straightLine1.A, straightLine1.B, straightLine2.A, straightLine2.B);
        public static bool CollideStraightLines(in Vector2 A, in Vector2 B, in Vector2 O, in Vector2 P)
        {
            float num = B.x - A.x;
            float num2 = B.y - A.y;
            float num3 = O.x - P.x;
            float num4 = O.y - P.y;
            float num5 = num * num4 - num2 * num3;
            return Mathf.Abs(num5) > 1e-3f;
        }
        public static bool CollideStraightLines(StraightLine2D straightLine1, StraightLine2D straightLine2, out Vector2 collisionPoint) => CollideStraightLines(straightLine1.A, straightLine1.B, straightLine2.A, straightLine2.B, out collisionPoint);
        public static bool CollideStraightLines(in Vector2 A, in Vector2 B, in Vector2 O, in Vector2 P, out Vector2 collisionPoint)
        {
            float num = B.x - A.x;
            float num2 = B.y - A.y;
            float num3 = O.x - P.x;
            float num4 = O.y - P.y;
            float num5 = num * num4 - num2 * num3;

            if (Mathf.Abs(num5) <= 1e-3f)
            {
                collisionPoint = Vector2.zero;
                return false;
            }

            float num6 = O.x - A.x;
            float num7 = O.y - A.y;
            float num8 = (num6 * num4 - num7 * num3) / num5;
            collisionPoint.x = A.x + num8 * num;
            collisionPoint.y = A.y + num8 * num2;
            return true;
        }
        public static bool CollideLines(Line2D line1, Line2D line2) => CollideLines(line1.A, line1.B, line2.A, line2.B);
        public static bool CollideLines(in Vector2 A, in Vector2 B, in Vector2 O, in Vector2 P)
        {
            return CollideLineStraightLine(A, B, O, P) && CollideLineStraightLine(O, P, A, B);
        }
        public static bool CollideLines(Line2D line1, Line2D line2, out Vector2 collisionPoint) => CollideLines(line1.A, line1.B, line2.A, line2.B, out collisionPoint);
        public static bool CollideLines(in Vector2 A, in Vector2 B, in Vector2 O, in Vector2 P, out Vector2 collisionPoint)
        {
            if (!CollideLineStraightLine(A, B, O, P) || !CollideLineStraightLine(O, P, A, B))
            {
                collisionPoint = Vector2.zero;
                return false;
            }

            return CollideStraightLines(A, B, O, P, out collisionPoint);
        }
        public static bool CollideLineStraightLine(Line2D line, StraightLine2D straightLine) => CollideLineStraightLine(line.A, line.B, straightLine.A, straightLine.B);
        internal static bool CollideLineStraightLine(in Vector2 O, in Vector2 P, in Vector2 A, in Vector2 B)
        {
            Vector2 AB = B - A;
            Vector2 AP = P - A;
            Vector2 AO = O - A;
            return (AB.x * AP.y - AB.y * AP.x) * (AB.x * AO.y - AB.y * AO.x) < 0f;
        }
        internal static bool CollideLineStraightLine(in Vector2 O, in Vector2 P, in Vector2 A, in Vector2 B, out Vector2 collisionPoint)
        {
            if(CollideLineStraightLine(O, P, A, B))
            {
                return CollideStraightLines(O, P, A, B, out collisionPoint);
            }

            collisionPoint = Vector2.zero;
            return false;
        }
        public static bool CollideLineStraightLine(Line2D line, StraightLine2D straightLine, out Vector2 collisionPoint) => CollideLineStraightLine(line.A, line.B, straightLine.A, straightLine.B, out collisionPoint);
        public static bool CollideLineRay(Line2D line, Ray2D ray) => CollideLines(line.A, line.B, ray.start, ray.end);
        public static bool CollideLineRay(Line2D line, Ray2D ray, out Vector2 collisionPoint) => CollideLines(line.A, line.B, ray.start, ray.end, out collisionPoint);
        public static bool CollideStraightLineRay(StraightLine2D straightLine, Ray2D ray) => CollideLineStraightLine(ray.start, ray.end,straightLine.A, straightLine.B);
        public static bool CollideStraightLineRay(StraightLine2D straightLine, Ray2D ray, out Vector2 collisionPoint) => CollideLineStraightLine(ray.start, ray.end, straightLine.A, straightLine.B, out collisionPoint);
        public static bool CollideRays(Ray2D ray1, Ray2D ray2) => CollideLines(ray1.start, ray1.end, ray2.start, ray2.end);
        public static bool CollideRays(in Vector2 start1, in Vector2 end1, in Vector2 start2, in Vector2 end2) => CollideLines(start1, end1, start2, end2);
        public static bool CollideRays(Ray2D ray1, Ray2D ray2, out Vector2 collisionPoint) => CollideLines(ray1.start, ray1.end, ray2.start, ray2.end, out collisionPoint);
        public static bool CollideRays(in Vector2 start1, in Vector2 end1, in Vector2 start2, in Vector2 end2, out Vector2 collisionPoint) => CollideLines(start1, end1, start2, end2, out collisionPoint);

        #endregion

        #endregion

        #region Collider Fields/Methods

        public virtual Vector2 center
        {
            get => new Vector2();
            protected set { }
        }

        protected Circle _inclusiveCircle;
        internal virtual Circle inclusiveCircle
        {
            get => _inclusiveCircle;
            set { _inclusiveCircle = value; }
        }

        protected Collider2D()
        {
            
        }

        protected Collider2D(UnityEngine.Collider2D collider)
        {
            
        }

        public abstract Collider2D Clone();
        public abstract bool Collide(Collider2D collider);
        public abstract bool CollideLine(Line2D line);
        public abstract bool CollideStraightLine(StraightLine2D d);
        public abstract bool Contains(in Vector2 p);
        public abstract float Distance(in Vector2 point);
        public abstract float SignedDistance(in Vector2 point);
        public abstract float Area();
        public abstract Vector2 ClosestPoint(in Vector2 point);
        public abstract void MoveAt(in Vector2 position);
        public abstract void Rotate(float angle);
        public abstract Hitbox ToHitbox();
        public abstract void Scale(in Vector2 scale);
        public virtual bool Normal(in Vector2 point, out Vector2 normal) { normal = Vector2.zero; return false; }

        #endregion
    }

    #endregion

    #region Polygone

    public class Polygone : Collider2D
    {
        public static void GizmosDraw(Polygone p, bool gizmos = true) => GizmosDraw(p.vertices, Color.white, gizmos);
        public static void GizmosDraw(Polygone p, Color color, bool gizmos = true) => GizmosDraw(p.vertices, color, gizmos);
        public static void GizmosDraw(Vector2[] points, bool gizmos = true) => GizmosDraw(points,  Color.white, gizmos);
        public static void GizmosDraw(Vector2[] points, Color color, bool gizmos = true)
        {
            if(gizmos)
            {
                GizmosDraw(points, color, (a, b, c) => { Gizmos.color = c; Gizmos.DrawLine(a, b); });
            }
            else
            {
                GizmosDraw(points, color, Debug.DrawLine);
            }
        }
        public static void GizmosDraw(Vector2[] points, Color color, Action<Vector3, Vector3, Color> drawLineFunction)
        {
            for (int i = 0; i < points.Length; i++)
            {
                drawLineFunction(points[i], points[(i + 1) % points.Length], color);
            }
        }

        public Vector2[] vertices { get; private set; }
        private Vector2 _center;

        public override Vector2 center
        {
            get => _center;
            protected set
            {
                MoveAt(value);
            }
        }

        #region Builder

        public Polygone(Vector2[] vertices) : base()
        {
            Builder(vertices.ToList());
            _center = Vector2.zero;
            foreach (Vector2 pos in this.vertices)
            {
                center += pos;
            }
            _center /= vertices.Length;
            inclusiveCircle = GetInclusiveCircle();
        }

        public Polygone(List<Vector2> vertices) : base()
        {
            Builder(vertices);
            _center = Vector2.zero;
            foreach (Vector2 pos in this.vertices)
            {
                center += pos;
            }
            _center /= vertices.Count;
            inclusiveCircle = GetInclusiveCircle();
        }

        public Polygone(Vector2[] vertices, in Vector2 center) : base()
        {
            Builder(vertices.ToList());
            _center = center;
            inclusiveCircle = GetInclusiveCircle();
        }

        public Polygone(List<Vector2> vertices, in Vector2 center) : base()
        {
            Builder(vertices);
            _center = center;
            inclusiveCircle = GetInclusiveCircle();
        }

        public Polygone(PolygonCollider2D poly) : base(poly)
        {
            List<Vector2> vertices = new List<Vector2>();
            poly.GetPath(0, vertices);
            this.vertices = new Vector2[vertices.Count];
            _center = (Vector2)poly.transform.position;
            for (int i = 0; i < vertices.Count; i++)
            {
                this.vertices[i] = vertices[i] + center + poly.offset;
            }

            Scale(poly.transform.lossyScale);
            Rotate(poly.transform.rotation.eulerAngles.z * Mathf.Deg2Rad);

            _center = Vector2.zero;
            foreach (Vector2 pos in this.vertices)
            {
                _center += pos;
            }
            _center /= vertices.Count;
            inclusiveCircle = GetInclusiveCircle();
        }

        private void Builder(List<Vector2> vertices)
        {
            for (int i = vertices.Count - 1; i >= 0; i--)
            {
                if (vertices[i] == vertices[(i + 1) % vertices.Count])
                {
                    vertices.RemoveAt(i);
                }
            }
            this.vertices = vertices.ToArray();
        }

        public override Collider2D Clone() => new Polygone((Vector2[])vertices.Clone(), center);

        #endregion

        public override bool Collide(Collider2D c) => Collider2D.Collide(c, this);

        public override Vector2 ClosestPoint(in Vector2 point)
        {
            Vector2[] closestPointOnSide = new Vector2[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                closestPointOnSide[i] = Line2D.ClosestPoint(vertices[i], vertices[(i + 1) % vertices.Length], point);
            }

            float minSqrDist = closestPointOnSide[0].SqrDistance(point);
            int closestIndex = 0;
            float currestSqrDistance;

            for (int i = 1; i < vertices.Length; i++)
            {
                currestSqrDistance = closestPointOnSide[i].SqrDistance(point);
                if(currestSqrDistance < minSqrDist)
                {
                    minSqrDist = currestSqrDistance;
                    closestIndex = i;
                }
            }

            return closestPointOnSide[closestIndex];
        }

        #region Contain

        public override bool Contains(in Vector2 point)
        {
            if (vertices == null || vertices.Length < 3)
                return false;

            if (!inclusiveCircle.Contains(point))
                return false;

            Vector2 I = center + Random.Vector2(inclusiveCircle.radius * 2f);
            Vector2 A, B;
            int nbintersections = 0;
            int iseg;
            for (int i = 0; i < vertices.Length; i++)
            {
                A = vertices[i];
                B = vertices[(i + 1) % vertices.Length];
                iseg = IntersectSegment(A, B, I, point);
                if (iseg == -1)
                    return Contains(point);  // cas limite, on relance la fonction.
                nbintersections += iseg;
            }

            return nbintersections.IsOdd();

            int IntersectSegment(in Vector2 A, in Vector2 B, in Vector2 I, in Vector2 P)
            {
                Vector2 D = B - A;
                Vector2 E = P - I;
                float denom = D.x * E.y - D.y * E.x;
                if (Mathf.Approximately(denom, 0f))
                    return -1;//error, limit case
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

        public override float Distance(in Vector2 point) => Contains(point) ? 0f : ClosestPoint(point).Distance(point);

        public override float SignedDistance(in Vector2 point)
        {
            Vector2[] closestPointOnSide = new Vector2[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                closestPointOnSide[i] = Line2D.ClosestPoint(vertices[i], vertices[(i + 1) % vertices.Length], point);
            }

            float minSqrDist = closestPointOnSide[0].SqrDistance(point);
            int closestIndex = 0;
            float currestSqrDistance;

            for (int i = 1; i < vertices.Length; i++)
            {
                currestSqrDistance = closestPointOnSide[i].SqrDistance(point);
                if (currestSqrDistance < minSqrDist)
                {
                    minSqrDist = currestSqrDistance;
                    closestIndex = i;
                }
            }

            minSqrDist = point.Distance(closestPointOnSide[closestIndex]);
            if (Contains(point))
            {
                minSqrDist = -minSqrDist;
            }

            return minSqrDist;
        }

        public override float Area()
        {
            float sum = 0f;
            int end = vertices.Length - 1;
            for (int i = 0; i < end; i++)
            {
                sum += (vertices[i].x * vertices[i + 1].y) - (vertices[i + 1].x * vertices[i].y);
            }
            sum += (vertices[end].x * vertices[0].y) - (vertices[0].x * vertices[end].y);

            return 0.5f * Mathf.Abs(sum);
        }

        public override bool CollideLine(Line2D line) => CollidePolygoneLine(this, line.A, line.B);

        public override bool CollideStraightLine(StraightLine2D straightLine) => CollidePolygoneStaightLine(this, straightLine.A, straightLine.B);

        public override void MoveAt(in Vector2 position)
        {
            Vector2 oldCenter = center;
            _center = position;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = center + (vertices[i] - oldCenter);
            }
            inclusiveCircle.MoveAt(position);
        }

        protected Circle GetInclusiveCircle()
        {
            float maxDist = 0;
            for (int i = 0; i < vertices.Length; i++)
            {
                maxDist = Mathf.Max(center.Distance(vertices[i]), maxDist);
            }
            return new Circle(center, maxDist);
        }

        public override void Scale(in Vector2 scale)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = center - (center - vertices[i]) * scale;
            }
            inclusiveCircle?.Scale(scale);
        }

        public override void Rotate(float angle)
        {
            Vector2 O = center;
            for (int i = 0; i < vertices.Length; i++)
            {
                float distOP = vertices[i].Distance(O);
                float newAngle = Useful.AngleHori(O, vertices[i]) + angle;
                vertices[i] = new Vector2(O.x + distOP * Mathf.Cos(newAngle), O.y + distOP * Mathf.Sin(newAngle));
            }
        }

        internal bool IsNormalOnRightDirection(in Vector2 point, in Vector2 n, int indexSide)
        {
            Vector2 A = point;
            Vector2 B = point + (2f * inclusiveCircle.radius) * n;
            int nbInter = 0;
            for (int i = 0; i < vertices.Length; i++)
            {
                if (i == indexSide)
                {
                    continue;
                }

                if (CollideLines(vertices[i], vertices[(i + 1) % vertices.Length], A, B))
                {
                    nbInter++;
                }
            }

            return nbInter.IsEven();
        }

        public override bool Normal(in Vector2 point, out Vector2 normal)
        {
            int minIndex = 0;
            float minSqrDist = Line2D.SqrDistance(vertices[0], vertices[1], point);
            float currentSqrDistance;
            for (int i = 1; i < vertices.Length; i++)
            {
                currentSqrDistance = Line2D.SqrDistance(vertices[i], vertices[(i + 1) % vertices.Length], point);
                if (currentSqrDistance < minSqrDist)
                {
                    minSqrDist = currentSqrDistance;
                    minIndex = i;
                }
            }

            if(minSqrDist > 1e-2f)
            {
                normal = Vector2.zero;
                return false;
            }

            Vector2 A = vertices[minIndex];
            Vector2 B = vertices[(minIndex + 1) % vertices.Length];
            normal = Line2D.Normal(A, B);

            if (!IsNormalOnRightDirection(point, normal, minIndex))
                normal *= -1f;

            return true;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("Vertices number : ");
            sb.AppendLine(vertices.Length.ToString());
            sb.AppendLine("Vertices : ");
            for (int i = 0; i < vertices.Length - 1; i++)
            {
                sb.Append("    ");
                sb.Append(vertices[i].ToString());
                sb.AppendLine(",");
            }
            sb.Append("    ");
            sb.AppendLine(vertices[vertices.Length - 1].ToString());
            return sb.ToString();
        }

        public override Hitbox ToHitbox() => new Hitbox(center, new Vector2(inclusiveCircle.radius, inclusiveCircle.radius));
    }

    #endregion

    #region Hitbox

    public class Hitbox : Collider2D
    {
        public static void GizmosDraw(Hitbox hitbox, bool gizmos = true) => Polygone.GizmosDraw(hitbox.rec, gizmos);
        public static void GizmosDraw(Hitbox hitbox, Color color, bool gizmos = true) => Polygone.GizmosDraw(hitbox.rec, color, gizmos);
        public static void GizmosDraw(in Vector2 center, in Vector2 size, bool gizmos = true) => Polygone.GizmosDraw(new Hitbox(center, size).rec, gizmos);
        public static void GizmosDraw(in Vector2 center, in Vector2 size, Color color, bool gizmos = true) => Polygone.GizmosDraw(new Hitbox(center, size).rec, color, gizmos);
        public static void GizmosDraw(Hitbox hitbox, Color color, Action<Vector3, Vector3, Color> drawLineFunction) => Polygone.GizmosDraw(hitbox.rec.vertices, color, drawLineFunction);

        protected Polygone rec;
        public Vector2 size;
        public Vector2[] vertices => rec.vertices;

        internal override Circle inclusiveCircle
        {
            get => rec.inclusiveCircle;
        }

        public Hitbox(in Vector2 center, in Vector2 size) : base()
        {
            Builder(center, size);
        }

        public Hitbox(BoxCollider2D hitbox) : base(hitbox)
        {
            Builder((Vector2)hitbox.transform.position + hitbox.offset, hitbox.size);
            Scale(hitbox.transform.lossyScale);
            Rotate(hitbox.transform.rotation.eulerAngles.z * Mathf.Deg2Rad);
        }

        private void Builder(in Vector2 center, in Vector2 size)
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

        public Polygone ToPolygone() => rec;

        public override void Rotate(float angle)
        {
            if (Mathf.Abs(angle) > Mathf.Epsilon)
            {
                rec.Rotate(angle);
            }
        }

        public float AngleHori()
        {
            return Useful.AngleHori(center, (rec.vertices[2] + rec.vertices[1]) * 0.5f);
        }

        public override void MoveAt(in Vector2 position)
        {
            rec.MoveAt(position);
        }

        public override bool Collide(Collider2D c) => Collider2D.Collide(c, this);
        public override bool CollideStraightLine(StraightLine2D d) => CollideHitboxStraigthLine(this, d);
        public override bool CollideLine(Line2D l) => CollideHitboxLine(this, l);

        public override Vector2 ClosestPoint(in Vector2 point) => rec.ClosestPoint(point);
        public override float Distance(in Vector2 point) => rec.Distance(point);
        public override float SignedDistance(in Vector2 point) => rec.SignedDistance(point);

        public override float Area() => size.x * size.y;

        public override void Scale(in Vector2 scale)
        {
            rec.Scale(scale);
            size *= scale;
        }

        public override Vector2 center
        {
            get => rec.center;
            protected set
            {
                MoveAt(value);
            }
        }

        public override bool Contains(in Vector2 point) => rec.Contains(point);

        public override Hitbox ToHitbox() => this;

        public override Collider2D Clone()
        {
            Hitbox res = new Hitbox(center, size);
            res.Rotate(AngleHori());
            return res;
        }


        public override string ToString() => $"Center:{center}, Size:{size}, rotation:{AngleHori()}";
        public override bool Normal(in Vector2 point, out Vector2 normal) => rec.Normal(point, out normal);
    }

    #endregion

    #region Circle

    public class Circle : Collider2D
    {
        public static void GizmosDraw(in Vector2 center, float radius, bool gizmos = true) => GizmosDraw(center, radius, Color.white, gizmos);
        public static void GizmosDraw(Circle circle, bool gizmos = true) => GizmosDraw(circle.center, circle.radius, Color.white, gizmos);
        public static void GizmosDraw(Circle circle, Color color, bool gizmos = true) => GizmosDraw(circle.center, circle.radius, color, gizmos);
        public static void GizmosDraw(in Vector2 center, float radius, Color color, bool gizmos = true)
        {
            void DrawLine(Vector3 a, Vector3 b, Color c)
            {
                Gizmos.color = c;
                Gizmos.DrawLine(a, b);
            }

            if (gizmos)
            {
                GizmosDraw(center, radius, color, DrawLine);
            }
            else
            {
                GizmosDraw(center, radius, color, Debug.DrawLine);
            }
        }
        public static void GizmosDraw(in Vector2 center, float radius, float begAngle, float endAngle, bool gizmos = true) => GizmosDraw(center, radius, begAngle, endAngle, Color.white, gizmos);
        public static void GizmosDraw(in Vector2 center, float radius, float begAngle, float endAngle, Color color, bool gizmos = true)
        {
            void DrawLine(Vector3 a, Vector3 b, Color c)
            {
                Gizmos.color = c;
                Gizmos.DrawLine(a, b);
            }

            if (gizmos)
            {
                GizmosDraw(center, radius, begAngle, endAngle, color, DrawLine);
            }
            else
            {
                GizmosDraw(center, radius, begAngle, endAngle, color, Debug.DrawLine);
            }
        }
        public static void GizmosDraw(in Vector2 center, float radius, Color color, Action<Vector3, Vector3, Color> drawLineFunction)
        {
            int sides = ((radius + 10f) * 7f).Round();
            float angleStep = (2f * Mathf.PI) / sides;
            Vector2 lastPoint = center + radius * Vector2.right;
            Vector2 p;
            for (int i = 1; i <= sides; i++)
            {
                float ang = i * angleStep;
                p = center + new Vector2(radius * Mathf.Cos(ang), radius * Mathf.Sin(ang));
                drawLineFunction(lastPoint, p, color);
                lastPoint = p;
            }
        }
        public static void GizmosDraw(in Vector2 center, float radius, float begAngle, float endAngle, Color color, Action<Vector3, Vector3, Color> drawLineFunction)
        {
            int sides = Math.Max(((radius + 10f) * 7f * Mathf.Abs((endAngle - begAngle) / (2f * Mathf.PI))).Round(), 4);
            float angleStep = Mathf.Abs(endAngle - begAngle) / sides;
            Vector2 lastPoint = center + Useful.Vector2FromAngle(begAngle, radius);
            Vector2 p;
            for (int i = 1; i <= sides; i++)
            {
                float ang = begAngle + i * angleStep;
                p = center + Useful.Vector2FromAngle(ang, radius);
                drawLineFunction(lastPoint, p, color);
                lastPoint = p;
            }
        }

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
        internal override Circle inclusiveCircle => this;

        public Circle(in Vector2 center, float radius) : base()
        {
            this.center = center;
            this.radius = radius;
        }

        public Circle(CircleCollider2D circle) : base(circle)
        {
            radius = circle.radius;
            center = (Vector2)circle.transform.position + circle.offset;
            Scale(circle.transform.lossyScale);
        }

        public override bool CollideLine(Line2D line) => CollideCircleLine(this, line.A, line.B);

        public override bool CollideStraightLine(StraightLine2D straightLine) => CollideCircleStraightLine(this, straightLine.A, straightLine.B);

        public override bool Collide(Collider2D collider) => Collider2D.Collide(collider, this);

        public override Vector2 ClosestPoint(in Vector2 point)
        {
            return center + (point - center).normalized * radius;
        }

        public override float Distance(in Vector2 point) => Mathf.Max(0f, point.Distance(center) - radius);

        public override float SignedDistance(in Vector2 point) => point.Distance(center) - radius;

        public override float Area() => Mathf.PI * radius * radius;

        public override Hitbox ToHitbox() => new Hitbox(center, new Vector2(radius, radius));

        public override void MoveAt(in Vector2 position)
        {
            center = position;
        }

        public override void Rotate(float angle) { }

        public override void Scale(in Vector2 scale)
        {
            radius *= Mathf.Max(scale.x, scale.y);
        }

        public override bool Contains(in Vector2 p) => center.SqrDistance(p) <= radius * radius;
        public override string ToString() => ("center : " + center.ToString() + " Radius : " + radius.ToString());
        public override Collider2D Clone() => new Circle(center, radius);

        public override bool Normal(in Vector2 point, out Vector2 normal)
        {
            if (Mathf.Approximately(center.SqrDistance(point), radius * radius))
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

    public class Capsule : Collider2D
    {
        public static void GizmosDraw(Capsule capsule, bool gizmos = true) => GizmosDraw(capsule, Color.white, gizmos);
        public static void GizmosDraw(Capsule capsule, Color color, bool gizmos = true)
        {
            if(gizmos)
            {
                GizmosDraw(capsule, color, (a,b,c) => { Gizmos.color = c; Gizmos.DrawLine(a, b); });
            }
            else
            {
                GizmosDraw(capsule, color, Debug.DrawLine);
            }
        }

        public static void GizmosDraw(Capsule capsule, Color color, Action<Vector3, Vector3, Color> drawLineFunction)
        {
            Circle.GizmosDraw(capsule.circle1.center, capsule.circle1.radius, color, drawLineFunction);
            Circle.GizmosDraw(capsule.circle2.center, capsule.circle2.radius, color, drawLineFunction);
            Hitbox.GizmosDraw(capsule.hitbox, color, drawLineFunction);
        }

        public Circle circle1 { get; private set; }
        public Circle circle2 { get; private set; }
        public Hitbox hitbox { get; private set; }

        public override Vector2 center
        {
            get => hitbox.center;
            protected set { MoveAt(value); }
        }

        #region Ctor

        private Capsule() : base() { } 

        public Capsule(in Vector2 center, in Vector2 size) : base()
        {
            CapsuleDirection2D direction = size.x >= size.y ? CapsuleDirection2D.Horizontal : CapsuleDirection2D.Vertical;
            Builder(center, size, direction, 0f);
        }

        public Capsule(in Vector2 center, in Vector2 size, float angle) : base()
        {
            CapsuleDirection2D direction = size.x >= size.y ? CapsuleDirection2D.Horizontal : CapsuleDirection2D.Vertical;
            Builder(center, size, direction, angle);
        }

        public Capsule(in Vector2 center, in Vector2 size, CapsuleDirection2D direction) : base()
        {
            Builder(center, size, direction, 0f);
        }

        public Capsule(in Vector2 center, in Vector2 size, CapsuleDirection2D direction, float angle) : base()
        {
            Builder(center, size, direction, angle);
        }

        public Capsule(CapsuleCollider2D capsule) : base(capsule)
        {
            Builder((Vector2)capsule.transform.position + capsule.offset, capsule.size * capsule.transform.localScale, capsule.direction, capsule.transform.rotation.eulerAngles.z * Mathf.Deg2Rad);
        }

        private void Builder(in Vector2 center, in Vector2 size, CapsuleDirection2D direction, float rotation)
        {
            if(direction == CapsuleDirection2D.Horizontal)
            {
                hitbox = new Hitbox(center, new Vector2(Mathf.Max(size.x - size.y, 0f), size.y));
                circle1 = new Circle(new Vector2(center.x - (hitbox.size.x * 0.5f), center.y), size.y * 0.5f);
                circle2 = new Circle(new Vector2(center.x + (hitbox.size.x * 0.5f), center.y), size.y * 0.5f);
            }
            else
            {
                hitbox = new Hitbox(center, new Vector2(size.x, Mathf.Max(size.y - size.x, 0f)));
                circle1 = new Circle(new Vector2(center.x, center.y - (hitbox.size.y * 0.5f)), size.x * 0.5f);
                circle2 = new Circle(new Vector2(center.x, center.y + (hitbox.size.y * 0.5f)), size.x * 0.5f);
            }

            inclusiveCircle = GetInclusiveCircle();
            Rotate(rotation);
        }

        #endregion

        public override Collider2D Clone()
        {
            Capsule clone = new Capsule();
            clone.hitbox = (Hitbox)hitbox.Clone();
            clone.circle1 = (Circle)circle1.Clone();
            clone.circle2 = (Circle)circle2.Clone();
            clone.inclusiveCircle = (Circle)inclusiveCircle.Clone();
            return clone;
        }

        public float AngleHori() => hitbox.AngleHori();

        public override bool CollideLine(Line2D line) => CollideCapsuleLine(this, line.A, line.B);
        public override bool CollideStraightLine(StraightLine2D straightLine) => CollideCapsuleStraightLine(this, straightLine.A, straightLine.B);

        public override bool Collide(Collider2D c) => Collider2D.Collide(c, this);

        public override Vector2 ClosestPoint(in Vector2 point)
        {
            float distance = circle1.center.Distance(circle2.center);
            Vector2 dirHori = (circle1.center - circle2.center) / distance;
            Vector2 dirVerti = dirHori.NormalVector();
            distance *= 0.5f;

            Vector2 A = hitbox.center + (dirVerti * circle1.radius) - (dirHori * distance);
            Vector2 B = hitbox.center + (dirVerti * circle1.radius) + (dirHori * distance);
            Vector2 A2 = hitbox.center - (dirVerti * circle1.radius) - (dirHori * distance);
            Vector2 B2 = hitbox.center - (dirVerti * circle1.radius) + (dirHori * distance);

            Vector2 res;
            Vector2 cp1 = Line2D.ClosestPoint(A, B, point);
            Vector2 cp2 = Line2D.ClosestPoint(A2, B2, point);
            float currentSqrDist;
            float cp1Dist = cp1.SqrDistance(point);
            float cp2Dist = cp2.SqrDistance(point);

            (res, currentSqrDist) = cp1Dist <= cp2Dist ? (cp1, cp1Dist) : (cp2, cp2Dist);

            cp1 = point - circle1.center;
            cp2 = point - circle2.center;

            if(dirHori.Dot(cp1) > 0f)
            {
                cp1 = circle1.center + (cp1.normalized * circle1.radius);
                cp1Dist = cp1.SqrDistance(point);
                if(cp1Dist < currentSqrDist)
                {
                    currentSqrDist = cp1Dist;
                    res = cp1;
                }
            }

            if (dirHori.Dot(cp2) < 0f)
            {
                cp2 = circle2.center + (cp2.normalized * circle2.radius);
                cp2Dist = cp2.SqrDistance(point);
                if (cp2Dist < currentSqrDist)
                {
                    res = cp2;
                }
            }

            return res;
        }

        public override float Distance(in Vector2 point) => Contains(point) ? 0f : ClosestPoint(point).Distance(point);

        public override float SignedDistance(in Vector2 point)
        {
            Vector2 cp = ClosestPoint(point);
            float distance = point.Distance(cp);
            if(Contains(point))
            {
                distance = -distance;
            }

            return distance;
        }

        public override float Area() => circle1.Area() + hitbox.Area();

        public override void MoveAt(in Vector2 pos)
        {
            Vector2 distC1 = circle1.center - center;
            Vector2 distC2 = circle2.center - center;
            hitbox.MoveAt(pos);
            circle1.MoveAt(pos + distC1);
            circle2.MoveAt(pos + distC2);
            inclusiveCircle.MoveAt(pos);
        }

        public override void Rotate(float angle)
        {
            Vector2 offsetC1 = circle1.center - center;
            Vector2 offsetC2 = circle2.center - center;
            float norme = offsetC1.magnitude;
            float angTotal = Useful.Angle(Vector2.zero, offsetC1) + angle;
            offsetC1 = new Vector2(norme * Mathf.Cos(angTotal), norme * Mathf.Sin(angTotal));
            circle1.MoveAt(center + offsetC1);
            norme = offsetC2.magnitude;
            angTotal = Useful.Angle(Vector2.zero, offsetC2) + angle;
            offsetC2 = new Vector2((float)(norme * Mathf.Cos(angTotal)), (float)(norme * Mathf.Sin(angTotal)));
            circle2.MoveAt(center + offsetC2);
            hitbox.Rotate(angle);
        }

        public override bool Contains(in Vector2 point) => inclusiveCircle.Contains(point) && (circle1.Contains(point) || circle2.Contains(point) || hitbox.Contains(point));

        public override Hitbox ToHitbox() => hitbox;

        protected Circle GetInclusiveCircle()
        {
            return new Circle(hitbox.center, (circle1.center.Distance(circle2.center) + circle1.radius + circle2.radius) * 0.5f);
        }

        public override void Scale(in Vector2 scale)
        {
            circle1.Scale(scale);
            circle2.Scale(scale);
            hitbox.Scale(scale);

            circle1.MoveAt(center + (circle1.center - center) * scale);
            circle2.MoveAt(center + (circle2.center - center) * scale);

            inclusiveCircle.Scale(scale);
        }

        public override bool Normal(in Vector2 point, out Vector2 normal)
        {
            if (circle1.Normal(point, out normal))
                return true;
            if (circle2.Normal(point, out normal))
                return true;
            if (hitbox.Normal(point, out normal))
                return true;
            return base.Normal(point, out normal);
        }

        public override string ToString() => $"Center:{center}, size:{hitbox.size}, rotation:{AngleHori()}";
    }

    #endregion
}

#endregion

#if UNITY_EDITOR

#region 3D Collisions

namespace Collision3D
{
    #region CustomCollider3

    public abstract class CustomCollider3D
    {
        #region Collide Line3D/Droite3D

        public static bool CollideLineSphere(Sphere s, Line3D l)
        {
            return l.SqrDistance(s.center) <= s.radius * s.radius;
        }

        public static bool CollideLineHitbox3D(Hitbox3D hitbox, Line3D line)
        {
            Vector3[,] faces = hitbox.GetFaces();

            for (int i = 0; i < 6; i++)
            {
                if (hitbox.CollideLinePlane(faces[i, 0], faces[i, 1], faces[i, 2], line, out Vector3 inter))
                {
                    return true;
                }
            }

            return hitbox.Contains(line.A);
        }

        public static bool CollideDroiteSphere(Sphere s, Droite3D d)
        {
            return d.SqrDistance(s.center) <= s.radius * s.radius;
        }

        public static bool CollideDroiteHitbox3D(Hitbox3D hitbox, Droite3D droite)
        {
            Vector3[,] faces = hitbox.GetFaces();

            for (int i = 0; i < 6; i++)
            {
                if (hitbox.CollideDroitePlane(faces[i, 0], faces[i, 1], faces[i, 2], droite, out Vector3 inter))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Collisions Funtions

        #region Dico

        private static readonly Dictionary<Type, Dictionary<Type, Func<CustomCollider3D, CustomCollider3D, bool>>> collisionFunc1 = new Dictionary<Type, Dictionary<Type, Func<CustomCollider3D, CustomCollider3D, bool>>>()
    {
        {
            typeof(Sphere),
            new Dictionary<Type, Func<CustomCollider3D, CustomCollider3D, bool>>()
            {
                { typeof(Sphere),  (CustomCollider3D c1, CustomCollider3D c2) => CollideSpheres((Sphere)c1, (Sphere)c2) },
                { typeof(Hitbox3D),  (CustomCollider3D c1, CustomCollider3D c2) => CollideSphereHitbox3D((Sphere)c1, (Hitbox3D)c2) }
            }
        },
        {
            typeof(Hitbox3D),
            new Dictionary<Type, Func<CustomCollider3D, CustomCollider3D, bool>>()
            {
                { typeof(Sphere),  (CustomCollider3D c1, CustomCollider3D c2) => CollideSphereHitbox3D((Sphere)c2, (Hitbox3D)c1) },
                { typeof(Hitbox3D),  (CustomCollider3D c1, CustomCollider3D c2) => CollideHitbox3D((Hitbox3D)c1, (Hitbox3D)c2) },
            }
        },
    };

        #endregion

        #region GeneralCollision

        private static bool FirstTestBeforeCollision(CustomCollider3D c1, CustomCollider3D c2) => CollideSpheres(c1.inclusiveSphere, c2.inclusiveSphere);

        public static bool Collide(CustomCollider3D c1, CustomCollider3D c2)
        {
            if (!FirstTestBeforeCollision(c1, c2))
                return false;

            return collisionFunc1[c1.GetType()][c2.GetType()](c1, c2);
        }

        #endregion

        #region Collide(Sphere, other)

        public static bool CollideSpheres(Sphere s1, Sphere s2)
        {
            return s1.center.SqrDistance(s2.center) <= (s1.radius + s2.radius) * (s1.radius + s2.radius);
        }

        public static bool CollideSphereHitbox3D(Sphere s, Hitbox3D h)
        {
            return h.ClosestPoint(s.center).SqrDistance(s.center) <= s.radius * s.radius;
        }

        #endregion

        #region Collide(Hitbox3D, other)

        public static bool CollideHitbox3D(Hitbox3D h1, Hitbox3D h2)
        {
            Vector3[] vertices = h1.GetVertices();

            foreach (Vector3 vertex in vertices)
            {
                if (h2.Contains(vertex))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #endregion

        #region customCollider3D

        public static CustomCollider3D FromUnityCollider2D(Collider collider)
        {
            throw new NotImplementedException();
        }

        public virtual Vector3 center
        {
            get => Vector3.zero;
            protected set { }
        }

        protected Sphere _inclusiveSphere;
        public virtual Sphere inclusiveSphere
        {
            get => _inclusiveSphere;
            protected set { _inclusiveSphere = value; }
        }

        protected CustomCollider3D() { }

        public virtual CustomCollider3D Clone() => null;
        public virtual bool Collide(CustomCollider3D c) => false;
        public virtual bool CollideLine(Line3D l) => false;
        public virtual bool CollideLine(in Vector3 A, in Vector3 B) => false;
        public virtual bool CollideDroite(Droite3D d) => false;
        public virtual bool CollideDroite(in Vector3 A, in Vector3 B) => false;
        public virtual bool Contains(in Vector3 p) => false;
        public virtual void MoveAt(in Vector3 position) { }
        public virtual void Rotate(in Vector3 angle) { }
        public virtual void Rotate(float x, float y, float z) { }
        public virtual Hitbox3D ToHitbox3D() => null;
        public virtual Sphere ToSphere() => null;
        public virtual void SetScale(in Vector3 scale) { }
        public virtual bool Normal(in Vector3 point, out Vector3 normal) { normal = Vector3.zero; return false; }

        #region ApplyRot

        protected Vector3 ApplyRotX(in Vector3 center, in Vector3 p, float angle)
        {
            return ApplyRotX(center, p, Mathf.Cos(angle), Mathf.Sin(angle));
        }

        protected Vector3 ApplyRotX(in Vector3 center, in Vector3 p, float cosAngle, float sinAngle)
        {
            Vector3 A = p - center;
            return center + new Vector3(A.x, cosAngle * A.y - sinAngle * A.z, sinAngle * A.y + cosAngle * A.z);
        }

        protected Vector3 ApplyRotY(in Vector3 center, in Vector3 p, float angle)
        {
            return ApplyRotY(center, p, Mathf.Cos(angle), Mathf.Sin(angle));
        }

        protected Vector3 ApplyRotY(in Vector3 center, in Vector3 p, float cosAngle, float sinAngle)
        {
            Vector3 A = p - center;
            return center + new Vector3(A.x * cosAngle + sinAngle * A.z, A.y, cosAngle * A.z - sinAngle * A.x);
        }

        protected Vector3 ApplyRotZ(in Vector3 center, in Vector3 p, float angle)
        {
            return ApplyRotZ(center, p, Mathf.Cos(angle), Mathf.Sin(angle));
        }

        protected Vector3 ApplyRotZ(in Vector3 center, in Vector3 p, float cosAngle, float sinAngle)
        {
            Vector3 A = p - center;
            return center + new Vector3(cosAngle * A.x - sinAngle * A.y, sinAngle * A.x + cosAngle * A.y, A.z);
        }

        protected Vector3 ApplyRotXYZ(in Vector3 center, in Vector3 p, Vector3 rotation)
        {
            return ApplyRotXYZ(center, p, new Vector3(Mathf.Cos(rotation.x), Mathf.Cos(rotation.y), Mathf.Cos(rotation.z)), new Vector3(Mathf.Sin(rotation.x), Mathf.Sin(rotation.y), Mathf.Sin(rotation.z)));
        }

        protected Vector3 ApplyRotXYZ(in Vector3 center, in Vector3 p, Vector3 cosAngles, Vector3 sinAngles)
        {
            return ApplyRotZ(center, ApplyRotY(center, ApplyRotX(center, p, cosAngles.x, sinAngles.x), cosAngles.y, sinAngles.y), cosAngles.z, sinAngles.z);
        }

        #endregion

        #endregion
    }

    #endregion

    #region Line3D/Droite3D

    public class Line3D
    {
        public Vector3 A, B;

        public Line3D(in Vector3 A, in Vector3 B)
        {
            this.A = A; this.B = B;
        }

        public float SqrDistance(in Vector3 point)
        {
            Vector3 AB = B - A;
            Vector3 AV = point - A;
            if (AV.Dot(AB) <= 0f)
                return AV.sqrMagnitude;

            Vector3 BV = point - B;
            if (BV.Dot(A) >= 0f)
                return BV.sqrMagnitude;

            return AB.Cross(AV).sqrMagnitude / AB.sqrMagnitude;
        }

        public float Distance(in Vector3 point) => Mathf.Sqrt(SqrDistance(point));
    }

    public class Droite3D
    {
        public Vector3 A, B;

        public Droite3D(in Vector3 A, in Vector3 B)
        {
            this.A = A; this.B = B;
        }

        public float SqrDistance(in Vector3 point)
        {
            return (B - A).Cross(A - point).sqrMagnitude / (B - A).sqrMagnitude;
        }

        public float Distance(in Vector3 point) => Mathf.Sqrt(SqrDistance(point));

        public float SqrDistance(Droite3D d)
        {
            Vector3 v1 = B - A, v2 = d.B - d.A;
            float denum = v1.Cross(v2).sqrMagnitude;
            if (Mathf.Approximately(denum, 0f))
                return float.MaxValue;

            Vector3 v = d.A - A;
            float k = Mathf.Abs(v.x * v1.y * v2.z + v.y * v1.z * v2.x + v1.x * v2.y * v.z - v.z * v1.y * v2.x - v.y * v1.x * v2.z - v.x * v1.z * v2.y);

            return k * k / denum;
        }

        public float Distance(Droite3D d)
        {
            Vector3 v1 = B - A, v2 = d.B - d.A;
            float denum = v1.Cross(v2).magnitude;
            if (Mathf.Approximately(denum, 0f))
                return float.MaxValue;

            Vector3 v = d.A - A;
            float k = Mathf.Abs(v.x * v1.y * v2.z + v.y * v1.z * v2.x + v1.x * v2.y * v.z - v.z * v1.y * v2.x - v.y * v1.x * v2.z - v.x * v1.z * v2.y);

            return k / denum;
        }
    }

    #endregion

    #region Sphere

    public class Sphere : CustomCollider3D
    {
        public float radius;
        protected Vector3 _center;
        public override Vector3 center
        {
            get => _center;
            protected set
            {
                _center = value;
            }
        }

        public override Sphere inclusiveSphere
        {
            get => this;
            protected set { }
        }

        public Sphere() : base()
        {
            Builder(Vector2.zero, 0f);
        }

        public Sphere(in Vector3 center, float radius) : base()
        {
            Builder(center, radius);
        }

        public Sphere(SphereCollider sphereCollider) : base()
        {
            Builder(sphereCollider.transform.position + sphereCollider.center, sphereCollider.radius);
        }

        private void Builder(in Vector3 center, float radius)
        {
            this.radius = radius;
            this.center = center;
        }

        public override CustomCollider3D Clone() => new Sphere(center, radius);

        public override bool Collide(CustomCollider3D c)
        {
            throw new NotImplementedException();
        }

        public override bool CollideLine(Line3D l) => CustomCollider3D.CollideLineSphere(this, l);
        public override bool CollideLine(in Vector3 A, in Vector3 B) => CollideLine(new Line3D(A, B));
        public override bool CollideDroite(Droite3D d) => CustomCollider3D.CollideDroiteSphere(this, d);
        public override bool CollideDroite(in Vector3 A, in Vector3 B) => CollideDroite(new Droite3D(A, B));

        public override bool Contains(in Vector3 p) => center.SqrDistance(p) <= radius * radius;

        public override void MoveAt(in Vector3 position)
        {
            center = position;
        }

        public override Hitbox3D ToHitbox3D() => new Hitbox3D(center, Vector3.one * radius);

        public override Sphere ToSphere() => this;

        public override void SetScale(in Vector3 scale)
        {
            radius *= (scale.x + scale.y + scale.z) * 0.333333333f;
        }

        public override bool Normal(in Vector3 point, out Vector3 normal)
        {
            if (Mathf.Approximately(point.SqrDistance(center), radius * radius))
            {
                normal = (point - center).normalized;
                return true;
            }
            return base.Normal(point, out normal);
        }
    }

    #endregion

    #region Hitbox3D

    public class Hitbox3D : CustomCollider3D
    {
        private Vector3 _size;
        public Vector3 size
        {
            get => _size;
            set
            {
                _size = value;
                inclusiveSphere.radius = Mathf.Max(value.x, value.y, value.z);
            }
        }

        public Vector3 rotation;

        private Vector3 _center;
        public override Vector3 center
        {
            get => _center;
            protected set
            {
                _center = value;
                inclusiveSphere.MoveAt(value);
            }
        }

        public Hitbox3D() : base()
        {
            Builder(Vector3.zero, Vector3.zero, Vector3.zero);
        }

        public Hitbox3D(in Vector3 center, in Vector3 size) : base()
        {
            Builder(center, size, Vector3.zero);
        }

        public Hitbox3D(in Vector3 center, in Vector3 size, in Vector3 rotation) : base()
        {
            Builder(center, size, rotation);
        }

        public Hitbox3D(BoxCollider boxCollider)
        {
            Builder(boxCollider.transform.position + boxCollider.center, boxCollider.size, boxCollider.transform.rotation.eulerAngles);
        }

        private void Builder(in Vector3 center, in Vector3 size, in Vector3 rotation)
        {
            inclusiveSphere = new Sphere();
            this.center = center;
            this.size = size;
            this.rotation = rotation;
        }

        public override CustomCollider3D Clone() => new Hitbox3D(center, size, rotation);

        public override bool Collide(CustomCollider3D c)
        {
            throw new NotImplementedException();
        }

        public Vector3[] GetVertices()
        {
            Vector3[] baseVertices = new Vector3[8]
            {
            center + new Vector3(-size.x * 0.5f, size.y * 0.5f, -size.z * 0.5f),
            center + new Vector3(size.x * 0.5f, size.y * 0.5f, -size.z * 0.5f),
            center + new Vector3(-size.x * 0.5f, size.y * 0.5f, size.z * 0.5f),
            center + new Vector3(size.x * 0.5f, size.y * 0.5f, size.z * 0.5f),
            center + new Vector3(-size.x * 0.5f, -size.y * 0.5f, -size.z * 0.5f),
            center + new Vector3(size.x * 0.5f, -size.y * 0.5f, -size.z * 0.5f),
            center + new Vector3(-size.x * 0.5f, -size.y * 0.5f, size.z * 0.5f),
            center + new Vector3(size.x * 0.5f, -size.y * 0.5f, size.z * 0.5f)
            };

            //Apply rotation
            Vector3 c = new Vector3(Mathf.Cos(rotation.x), Mathf.Cos(rotation.y), Mathf.Cos(rotation.z));
            Vector3 s = new Vector3(Mathf.Sin(rotation.x), Mathf.Sin(rotation.y), Mathf.Sin(rotation.z));
            for (int i = 0; i < 8; i++)
            {
                baseVertices[i] = ApplyRotXYZ(center, baseVertices[i], c, s);
            }

            return baseVertices;
        }

        internal Vector3[,] GetFaces()
        {
            Vector3[] v = GetVertices();
            return new Vector3[6, 3]
            {
            { v[0], v[1], v[2] },
            { v[4], v[5], v[6] },
            { v[0], v[1], v[4] },
            { v[2], v[3], v[6] },
            { v[0], v[2], v[4] },
            { v[1], v[3], v[5] }
            };
        }

        //intersection entre le plan def par les points p1,p2 et p3 (non aligné avec p1.p2 = p1.p3 = 0) et la ligne line
        internal bool CollideLinePlane(in Vector3 p1, in Vector3 p2, in Vector3 p3, Line3D line, out Vector3 intersection)
        {
            //calcule de l'intersection
            Vector3 u = line.B - line.A;
            Vector3 n = (p2 - p1).Cross(p3 - p1);

            if (Mathf.Approximately(n.Dot(u), 0f))//droite et plan sont parallèles
            {
                intersection = Vector3.zero;
                return false;
            }

            float d = -n.Dot(p1);
            float k = (-n.Dot(line.A) + d) / (u.x + u.y + u.z);
            intersection = line.A + k * u;

            //verif intersection appartient au segment sachant que intersection € droite(A,B)
            float lineSqrLength = u.sqrMagnitude;
            if (line.A.SqrDistance(intersection) > lineSqrLength || line.B.SqrDistance(intersection) > lineSqrLength)
            {
                intersection = Vector3.zero;
                return false;
            }

            //verif intersection € plan carré p1,p2,p3 sachant que intersection € plan(p1,p2,p3)
            if ((p2 - p1).Dot(intersection - p1) < 0f || (p1 - p2).Dot(intersection - p2) < 0f ||
                (p1 - p3).Dot(intersection - p3) < 0f || (p3 - p1).Dot(intersection - p1) < 0f)
            {
                intersection = Vector3.zero;
                return false;
            }
            return true;
        }

        //intersection entre le plan def par les points p1,p2 et p3 (non aligné avec p1.p2 = p1.p3 = 0) et la droite droite
        internal bool CollideDroitePlane(in Vector3 p1, in Vector3 p2, in Vector3 p3, Droite3D droite, out Vector3 intersection)
        {
            //calcule de l'intersection
            Vector3 u = droite.B - droite.A;
            Vector3 n = (p2 - p1).Cross(p3 - p1);

            if (Mathf.Approximately(n.Dot(u), 0f))//droite et plan sont parallèles
            {
                intersection = Vector3.zero;
                return false;
            }

            float d = -n.Dot(p1);
            float k = (-n.Dot(droite.A) + d) / (u.x + u.y + u.z);
            intersection = droite.A + k * u;

            //verif intersection € plan carré p1,p2,p3 sachant que intersection € plan(p1,p2,p3)
            if ((p2 - p1).Dot(intersection - p1) < 0f || (p1 - p2).Dot(intersection - p2) < 0f ||
                (p1 - p3).Dot(intersection - p3) < 0f || (p3 - p1).Dot(intersection - p1) < 0f)
            {
                intersection = Vector3.zero;
                return false;
            }
            return true;
        }

        public Vector3 ClosestPoint(Vector3 p)
        {
            //rotate this to have 0 angle rot
            Hitbox3D h = (Hitbox3D)Clone();
            Vector3 cRot = new Vector3(Mathf.Cos(-rotation.x), Mathf.Cos(-rotation.y), Mathf.Cos(-rotation.z));
            Vector3 sRot = new Vector3(Mathf.Sin(-rotation.x), Mathf.Sin(-rotation.y), Mathf.Sin(-rotation.z));
            h.MoveAt(ApplyRotXYZ(Vector3.zero, h.center, cRot, sRot));
            p = ApplyRotXYZ(Vector3.zero, p, cRot, sRot);
            Vector3 min = h.center - h.size * 0.5f;
            Vector3 max = h.center + h.size * 0.5f;

            //calculate the point
            float x = Mathf.Min(Mathf.Max(p.x, min.x), max.x);
            float y = Mathf.Min(Mathf.Max(p.y, min.y), max.y);
            float z = Mathf.Min(Mathf.Max(p.z, min.z), max.z);

            return ApplyRotXYZ(Vector3.zero, new Vector3(x, y, z), rotation);
        }

        public override bool CollideLine(Line3D l) => CustomCollider3D.CollideLineHitbox3D(this, l);
        public override bool CollideLine(in Vector3 A, in Vector3 B) => CollideLine(new Line3D(A, B));
        public override bool CollideDroite(Droite3D d) => CustomCollider3D.CollideDroiteHitbox3D(this, d);
        public override bool CollideDroite(in Vector3 A, in Vector3 B) => CollideDroite(new Droite3D(A, B));

        public override bool Contains(in Vector3 p)
        {
            Vector3 c = new Vector3(Mathf.Cos(rotation.x), Mathf.Cos(rotation.y), Mathf.Cos(rotation.z));
            Vector3 s = new Vector3(Mathf.Sin(rotation.x), Mathf.Sin(rotation.y), Mathf.Sin(rotation.z));
            Vector3 newCenter = ApplyRotXYZ(Vector3.zero, center, c, s);
            Vector3 p2 = ApplyRotXYZ(Vector3.zero, p, c, s);

            return p2.x >= newCenter.x - size.x * 0.5f && p2.x <= newCenter.x - size.x * 0.5f &&
                p2.y >= newCenter.y - size.y * 0.5f && p2.y <= newCenter.y - size.y * 0.5f &&
                p2.x >= newCenter.z - size.z * 0.5f && p2.z <= newCenter.z - size.z * 0.5f;
        }

        public override void MoveAt(in Vector3 position)
        {
            center = position;
        }

        public override Hitbox3D ToHitbox3D() => this;

        public override Sphere ToSphere() => new Sphere(center, Mathf.Max(size.x, size.y, size.z));

        public override void SetScale(in Vector3 scale)
        {
            size = new Vector3(size.x * scale.x, size.y * scale.y, size.z * scale.z);
        }

        public override bool Normal(in Vector3 point, out Vector3 normal)
        {
            throw new NotImplementedException();
        }
    }

    #endregion

}

#endregion

#endif