using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

#region 2D Collisions

namespace Collision2D
{
    #region Line/Ray

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

        /// <summary>
        /// 
        /// </summary>
        /// <returns>true if point € [A,B], false otherwise</returns>
        public static bool Contain(in Vector2 A, in Vector2 B, in Vector2 point)
        {
            if (Mathf.Approximately(A.x, B.x))
            {
                return Mathf.Approximately(((A.x + B.x) * 0.5f), point.x) && Mathf.Min(A.y, B.y) <= point.y && Mathf.Max(A.y, B.y) >= point.y;
            }
            if (Mathf.Min(A.x, B.x) > point.x || Mathf.Max(A.x, B.x) < point.x || Mathf.Min(A.y, B.y) > point.y || Mathf.Max(A.y, B.y) < point.y)
            {
                return false;
            }
            //equetion de la droite (A,B)
            float a = (B.y - A.y) / (B.x - A.x);
            float b = A.y - a * A.x;
            return Mathf.Approximately(a * point.x + b, point.y);
        }

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
            if (Mathf.Approximately(A.x, B.x))
            {
                return Vector2.right;
            }
            //equetion de la droite (A,B)
            return new Vector2((B.y - A.y) / (B.x - A.x), -1f).normalized;
        }
    }

    public class Ray2D
    {
        public Vector2 A, B;
        public Ray2D(in Vector2 A, in Vector2 B)
        {
            this.A = A;
            this.B = B;
        }

        public static void GizmosDraw(Ray2D d) => GizmosDraw(d.A, d.B);
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
        public static Vector2 Symetric(in Vector2 M, Ray2D D)
        {
            //custom version
            if (Mathf.Approximately(D.A.x, D.B.x))
            {
                return new Vector2(M.x >= (D.A.x - D.B.x) * 0.5f ? M.x - 2f * Distance(D.A, D.B, M) : M.x + 2f * Distance(D.A, D.B, M), M.y);
            }
            return 2f * OrthogonalProjection(M, D) - M;
        }

        public static Vector2 Reflection(in Vector2 normal, in Vector2 interPointInDroite, in Vector2 initDir)
        {
            return initDir - 2f * Vector2.Dot(initDir, normal) * normal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="M"></param>
        /// <param name="D"></param>
        /// <returns>le projeté orthogonal du point M sur la droite D</returns>
        public static Vector2 OrthogonalProjection(in Vector2 M, in Ray2D D)
        {
            if (Mathf.Approximately(D.A.x, D.B.x))
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
            if (Mathf.Approximately(A.x, B.x))
            {
                return Mathf.Approximately(((A.x + B.x) * 0.5f), point.x) && Mathf.Min(A.y, B.y) <= point.y && Mathf.Max(A.y, B.y) >= point.y;
            }
            //equetion de la droite (A,B)
            float a = (B.y - A.y) / (B.x - A.x);
            float b = A.y - a * A.x;
            return Mathf.Approximately(a * point.x + b, point.y);
        }

        /// <summary>
        /// Vérif ok
        /// </summary>
        /// <returns> min(Dist(point, P)), P € (A,B)</returns>
        public static float Distance(in Vector2 A, in Vector2 B, in Vector2 point)
        {
            if (Mathf.Approximately(A.x, B.x))
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
            if (Mathf.Approximately(A.x, B.x))
            {
                return Vector2.right;
            }
            return new Vector2((B.y - A.y) / (B.x - A.x), -1f).normalized;
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
            Debug.LogWarning($"Cant convert a unity collider2D of typer {collider.GetType()}!");
            return null;
        }

        #region Collision Functions

        private static readonly List<Vector2> cache = new List<Vector2>(), cache2 = new List<Vector2>(), cache3 = new List<Vector2>();

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
                    { typeof(Hitbox),  (Collider2D c1, Collider2D c2) => CollideHitboxs((Hitbox)c1, (Hitbox)c2) },
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
                    { typeof(Hitbox),  (Collider2D c1, Collider2D c2) => (CollideHitboxs((Hitbox)c1, (Hitbox)c2, out Vector2 v),v) },
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
                    { typeof(Circle),  (Collider2D c1, Collider2D c2) => (CollideCirclePolygone((Circle)c2, (Polygone)c1, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) },
                    { typeof(Polygone),  (Collider2D c1, Collider2D c2) => (CollidePolygones((Polygone)c1, (Polygone)c2, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) },
                    { typeof(Hitbox),  (Collider2D c1, Collider2D c2) => (CollidePolygoneHitbox((Polygone)c1, (Hitbox)c2, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) },
                    { typeof(Capsule),  (Collider2D c1, Collider2D c2) => (CollidePolygoneCapsule((Polygone)c1, (Capsule)c2, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) },
                }
            },
            {
                typeof(Hitbox),
                new Dictionary<Type, Func<Collider2D, Collider2D, (bool, Vector2, Vector2, Vector2)>>()
                {
                    { typeof(Circle),  (Collider2D c1, Collider2D c2) => (CollideCircleHitbox((Circle)c2, (Hitbox)c1, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) },
                    { typeof(Polygone),  (Collider2D c1, Collider2D c2) => (CollidePolygoneHitbox((Polygone)c2, (Hitbox)c1, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) },
                    { typeof(Hitbox),  (Collider2D c1, Collider2D c2) => (CollideHitboxs((Hitbox)c1, (Hitbox)c2, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) },
                    { typeof(Capsule),  (Collider2D c1, Collider2D c2) => (CollideHitboxCapsule((Hitbox)c1, (Capsule)c2, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) },
                }
            },
            {
                typeof(Capsule),
                new Dictionary<Type, Func<Collider2D, Collider2D, (bool, Vector2, Vector2, Vector2)>>()
                {
                    { typeof(Circle),  (Collider2D c1, Collider2D c2) => (CollideCircleCapsule((Circle)c2, (Capsule)c1, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) },
                    { typeof(Polygone),  (Collider2D c1, Collider2D c2) => (CollidePolygoneCapsule((Polygone)c2, (Capsule)c1, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) },
                    { typeof(Hitbox),  (Collider2D c1, Collider2D c2) => (CollideHitboxCapsule((Hitbox)c2, (Capsule)c1, out Vector2 v, out Vector2 v2, out Vector2 v3),v, v2, v3) },
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

        private static readonly Dictionary<Type, Func<Collider2D, Vector2, Vector2, bool>> collisionDroite1 = new Dictionary<Type, Func<Collider2D, Vector2, Vector2, bool>>()
        {
            { typeof(Circle), (Collider2D c, Vector2 A, Vector2 B) => CollideCircleRay((Circle)c, A, B) },
            { typeof(Polygone), (Collider2D c, Vector2 A, Vector2 B) => CollidePolygoneRay((Polygone)c, A, B) },
            { typeof(Hitbox), (Collider2D c, Vector2 A, Vector2 B) => CollideHitboxRay((Hitbox)c, A, B) },
            { typeof(Capsule), (Collider2D c, Vector2 A, Vector2 B) => CollideCapsuleRay((Capsule)c, A, B) },
        };

        private static readonly Dictionary<Type, Func<Collider2D, Vector2, Vector2, (bool, Vector2)>> collisionDroite2 = new Dictionary<Type, Func<Collider2D, Vector2, Vector2, (bool, Vector2)>>()
        {
            { typeof(Circle), (Collider2D c, Vector2 A, Vector2 B) => (CollideCircleRay((Circle)c, A, B, out Vector2 v), v) },
            { typeof(Polygone), (Collider2D c, Vector2 A, Vector2 B) => (CollidePolygoneRay((Polygone)c, A, B, out Vector2 v), v) },
            { typeof(Hitbox), (Collider2D c, Vector2 A, Vector2 B) => (CollideHitboxRay((Hitbox)c, A, B, out Vector2 v), v) },
            { typeof(Capsule), (Collider2D c, Vector2 A, Vector2 B) => (CollideCapsuleRay((Capsule)c, A, B, out Vector2 v), v) },
        };

        private static readonly Dictionary<Type, Func<Collider2D, Vector2, Vector2, (bool, Vector2, Vector2)>> collisionDroite3 = new Dictionary<Type, Func<Collider2D, Vector2, Vector2, (bool, Vector2, Vector2)>>()
        {
            { typeof(Circle), (Collider2D c, Vector2 A, Vector2 B) => (CollideCircleRay((Circle)c, A, B, out Vector2 v, out Vector2 v2), v, v2) },
            { typeof(Polygone), (Collider2D c, Vector2 A, Vector2 B) => (CollidePolygoneRay((Polygone)c, A, B, out Vector2 v, out Vector2 v2), v, v2) },
            { typeof(Hitbox), (Collider2D c, Vector2 A, Vector2 B) => (CollideHitboxRay((Hitbox)c, A, B, out Vector2 v, out Vector2 v2), v, v2) },
            { typeof(Capsule), (Collider2D c, Vector2 A, Vector2 B) => (CollideCapsuleRay((Capsule)c, A, B, out Vector2 v, out Vector2 v2), v, v2) },
        };

        #endregion

        private static bool FirstTestBeforeCollision(Collider2D c1, Collider2D c2) => CollideCircles(c1.inclusiveCircle, c2.inclusiveCircle);

        /// <returns>true if the both collider collide together, false otherwise</returns>
        public static bool Collide(Collider2D collider1, Collider2D collider2) => FirstTestBeforeCollision(collider1, collider2) && collisionFunc1[collider1.GetType()][collider2.GetType()](collider1, collider2);
        /// <param name="collisionPoint">The average point of collision of the two collider if true is return, (0,0) oterwise</param>
        /// <returns>true if the both collider collide together, false otherwise</returns>
        public static bool Collide(Collider2D collider1, Collider2D collider2, out Vector2 collisionPoint)
        {
            if (!FirstTestBeforeCollision(collider1, collider2))
            {
                collisionPoint = Vector2.zero;
                return false;
            }
            bool b;
            (b, collisionPoint) = collisionFunc2[collider1.GetType()][collider2.GetType()](collider1, collider2);
            return b;
        }
        /// <param name="collisionPoint">The average point of collision of the two collider if true is return, (0,0) oterwise</param>
        /// <param name="normal1">The vector normal at the surface of the first collider where the collission happend</param>
        /// <param name="normal2">The vector normal at the surface of the second collider where the collission happend</param>
        /// <returns>true if the both collider collide together, false otherwise</returns>
        public static bool Collide(Collider2D collider1, Collider2D collider2, out Vector2 collisionPoint, out Vector2 normal1, out Vector2 normal2)
        {
            if (!FirstTestBeforeCollision(collider1, collider2))
            {
                collisionPoint = normal1 = normal2 = Vector2.zero;
                return false;
            }
            bool b;
            (b, collisionPoint, normal1, normal2) = collisionFunc3[collider1.GetType()][collider2.GetType()](collider1, collider2);
            return b;
        }

        /// <returns>true if the collider collide together width the line, false otherwise</returns>
        public static bool CollideLine(Collider2D collider, in Vector2 A, in Vector2 B) => collisionLine1[collider.GetType()](collider, A, B);
        /// <returns>true if the collider collide together width the line, false otherwise</returns>
        public static bool CollideLine(Collider2D collider, Line2D l) => CollideLine(collider, l.A, l.B);
        /// <param name="collisionPoint">The point at the surface of the collider where the collission happend</param>
        /// <returns>true if the collider collide together width the line, false otherwise</returns>
        public static bool CollideLine(Collider2D collider, in Vector2 A, in Vector2 B, out Vector2 collisionPoint)
        {
            bool b;
            (b, collisionPoint) = collisionLine2[collider.GetType()](collider, A, B);
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
            bool b;
            (b, collisionPoint, normal) = collisionLine3[collider.GetType()](collider, A, B);
            return b;
        }
        /// <param name="collisionPoint">The point at the surface of the collider where the collission happend</param>
        /// <param name="normal">The vector normal tothe collider's surface wehere the collision happend</param>
        /// <returns>true if the collider collide together width the line, false otherwise</returns>
        public static bool CollideLine(Collider2D collider, Line2D line, out Vector2 collisionPoint, out Vector2 normal) => CollideLine(collider, line.A, line.B, out collisionPoint, out normal);

        /// <returns>true if the collider collide together width the droite, false otherwise</returns>
        public static bool CollideRay(Collider2D collider, in Vector2 A, in Vector2 B) => collisionDroite1[collider.GetType()](collider, A, B);
        /// <returns>true if the collider collide together width the droite, false otherwise</returns>
        public static bool CollideRay(Collider2D ollider, Ray2D d) => CollideRay(ollider, d.A, d.B);
        /// <param name="collisionPoint">The point at the surface of the collider where the collission happend</param>
        /// <returns>true if the collider collide together width the droite, false otherwise</returns>
        public static bool CollideRay(Collider2D collider, in Vector2 A, in Vector2 B, out Vector2 collisionPoint)
        {
            bool b;
            (b, collisionPoint) = collisionDroite2[collider.GetType()](collider, A, B);
            return b;
        }
        /// <param name="collisionPoint">The point at the surface of the collider where the collission happend</param>
        /// <returns>true if the collider collide together width the droite, false otherwise</returns>
        public static bool CollideRay(Collider2D collider, Ray2D ray, out Vector2 collisionPoint) => CollideRay(collider, ray.A, ray.B, out collisionPoint);
        /// <param name="collisionPoint">The point at the surface of the collider where the collission happend</param>
        /// <param name="normal">The vector normal tothe collider's surface wehere the collision happend</param>
        /// <returns>true if the collider collide together width the droite, false otherwise</returns>
        public static bool CollideRay(Collider2D collider, in Vector2 A, in Vector2 B, out Vector2 collisionPoint, out Vector2 normal)
        {
            bool b;
            (b, collisionPoint, normal) = collisionDroite3[collider.GetType()](collider, A, B);
            return b;
        }
        /// <param name="collisionPoint">The point at the surface of the collider where the collission happend</param>
        /// <param name="normal">The vector normal tothe collider's surface wehere the collision happend</param>
        /// <returns>true if the collider collide together width the droite, false otherwise</returns>
        public static bool CollideRay(Collider2D collider, Ray2D ray, out Vector2 collisionPoint, out Vector2 normal) => CollideRay(collider, ray.A, ray.B, out collisionPoint, out normal);

        #endregion

        #region Collide(Circle, other)

        public static bool CollideCircles(Circle circle1, Circle circle2) => circle1.center.SqrDistance(circle2.center) <= (circle1.radius + circle2.radius) * (circle1.radius + circle2.radius);//OK
        public static bool CollideCircles(Circle circle1, Circle circle2, out Vector2 collisionPoint)//ok
        {
            float sqrDist = circle1.center.SqrDistance(circle2.center);
            float rr = (circle1.radius + circle2.radius) * (circle1.radius + circle2.radius);
            if (sqrDist <= rr)// il y a collision
            {
                if (sqrDist < (circle1.radius - circle2.radius) * (circle1.radius - circle2.radius))//un cercle inclus dans l'autre
                {
                    if (circle1.radius <= circle2.radius)
                    {
                        float angle = Useful.AngleHori(circle2.center, circle1.center);
                        Vector2 collisionPoint2 = new Vector2(circle2.center.x + circle2.radius * Mathf.Cos(angle), circle2.center.y + circle2.radius * Mathf.Sin(angle));
                        Vector2 collisionPoint1 = new Vector2(circle1.center.x + circle1.radius * Mathf.Cos(angle + Mathf.PI), circle1.center.y + circle1.radius * Mathf.Sin(angle + Mathf.PI));
                        collisionPoint = (collisionPoint1 + collisionPoint2) * 0.5f;
                    }
                    else
                    {
                        float angle = Useful.AngleHori(circle1.center, circle2.center);
                        Vector2 collisionPoint2 = new Vector2(circle1.center.x + circle1.radius * Mathf.Cos(angle), circle1.center.y + circle1.radius * Mathf.Sin(angle));
                        Vector2 collisionPoint1 = new Vector2(circle2.center.x + circle2.radius * Mathf.Cos(angle + Mathf.PI), circle2.center.y + circle2.radius * Mathf.Sin(angle + Mathf.PI));
                        collisionPoint = (collisionPoint1 + collisionPoint2) * 0.5f;
                    }
                    return true;
                }
                else//cas d'intersection normale
                {
                    if (Mathf.Approximately(circle1.center.y, circle2.center.y))
                    {
                        float x = ((circle2.radius * circle2.radius) - (circle1.radius * circle1.radius) - (circle2.center.x * circle2.center.x) + (circle1.center.x * circle1.center.x)) / (2f * (circle1.center.x - circle2.center.x));
                        float b = -2f * circle2.center.y;
                        float c = (circle2.center.x * circle2.center.x) + (x * x) - (2f * circle2.center.x * x) + (circle2.center.y * circle2.center.y) - (circle2.radius * circle2.radius);
                        float sqrtDelta = Mathf.Sqrt((b * b) - (4f * c));
                        Vector2 i1 = new Vector2(x, (-b - sqrtDelta) * 0.5f);
                        Vector2 i2 = new Vector2(x, (-b + sqrtDelta) * 0.5f);
                        collisionPoint = (i1 + i2) * 0.5f;
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
        public static bool CollideCircles(Circle circle1, Circle circle2, out Vector2 collisionPoint, out Vector2 normal1, out Vector2 normal2)
        {
            if (CollideCircles(circle1, circle2, out collisionPoint))
            {
                normal1 = (collisionPoint - circle1.center);
                normal1.Normalize();
                normal2 = (collisionPoint - circle2.center);
                normal2.Normalize();
                return true;
            }
            normal1 = normal2 = Vector2.zero;
            return false;
        }

        public static bool CollideCirclePolygone(Circle circle, Polygone polygone)//OK
        {
            for (int i = 0; i < polygone.vertices.Length; i++)
            {
                if (circle.CollideLine(new Line2D(polygone.vertices[i], polygone.vertices[(i + 1) % polygone.vertices.Length])))
                    return true;
            }
            return polygone.Contains(circle.center);
        }
        public static bool CollideCirclePolygone(Circle circle, Polygone polygone, out Vector2 collisionPoint)//OK
        {
            collisionPoint = Vector2.zero;
            for (int i = 0; i < polygone.vertices.Length; i++)
            {
                if (CollideCircleLineBothCol(circle, polygone.vertices[i], polygone.vertices[(i + 1) % polygone.vertices.Length], out Vector2 inter))
                    cache.Add(inter);
                if (circle.Contains(polygone.vertices[i]))
                    cache.Add(polygone.vertices[i]);
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
            if (polygone.Contains(circle.center))
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
                if (Mathf.Approximately(A.x, B.x))
                {
                    float srqtDelta = Mathf.Sqrt((c.radius * c.radius) - (((A.x + B.x) * 0.5f) - c.center.x) * (((A.x + B.x) * 0.5f) - c.center.x));
                    Vector2 i1 = new Vector2((A.x + B.x) * 0.5f, -srqtDelta + c.center.y);
                    Vector2 i2 = new Vector2((A.x + B.x) * 0.5f, srqtDelta + c.center.y);
                    //on verif que i1 et i2 appartienne au seg
                    if (Mathf.Min(A.y, B.y) <= i1.y && Mathf.Max(A.y, B.y) >= i1.y && Mathf.Min(A.y, B.y) <= i2.y && Mathf.Max(A.y, B.y) >= i2.y)
                    {
                        collision = (i1 + i2) * 0.5f;
                        Vector2 dir = collision - c.center;
                        if (Mathf.Approximately(dir.sqrMagnitude, 0f))
                            return true;

                        if (CollideCircleLine(c, collision, collision + c.radius * dir.normalized, out Vector2 col2))
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
                    if (Mathf.Approximately(dir2.sqrMagnitude, 0f))
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
                        if (Mathf.Approximately(dir.sqrMagnitude, 0f))
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
                    if (Mathf.Approximately(dir2.sqrMagnitude, 0f))
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
        public static bool CollideCircleHitbox(Circle circle, Hitbox hitbox) => CollideCirclePolygone(circle, hitbox.ToPolygone());//ok
        public static bool CollideCircleHitbox(Circle circle, Hitbox hitbox, out Vector2 collisionPoint) => CollideCirclePolygone(circle, hitbox.ToPolygone(), out collisionPoint);//OK
        public static bool CollideCircleHitbox(Circle circle, Hitbox hitbox, out Vector2 collisionPoint, out Vector2 normal1, out Vector2 normal2) => CollideCirclePolygone(circle, hitbox.ToPolygone(), out collisionPoint, out normal1, out normal2);
        public static bool CollideCircleLine(Circle circle, in Vector2 A, in Vector2 B) => circle.CollideLine(new Line2D(A, B));//ok
        public static bool CollideCircleLine(Circle circle, Line2D line) => circle.CollideLine(line);//ok
        public static bool CollideCircleLine(Circle circle, in Vector2 A, in Vector2 B, out Vector2 collisionPoint)//ok
        {
            if (!CollideCircleLine(circle, A, B))
            {
                collisionPoint = Vector2.zero;
                return false;
            }
            //on regarde si la droite est verticale
            if (Mathf.Approximately(A.x, B.x))
            {
                float srqtDelta = Mathf.Sqrt((circle.radius * circle.radius) - (((A.x + B.x) * 0.5f) - circle.center.x) * (((A.x + B.x) * 0.5f) - circle.center.x));
                Vector2 i1 = new Vector2((A.x + B.x) * 0.5f, -srqtDelta + circle.center.y);
                Vector2 i2 = new Vector2((A.x + B.x) * 0.5f, +srqtDelta + circle.center.y);
                //on verif que i1 et i2 appartienne au seg
                if (Mathf.Min(A.y, B.y) <= i1.y && Mathf.Max(A.y, B.y) >= i1.y && Mathf.Min(A.y, B.y) <= i2.y && Mathf.Max(A.y, B.y) >= i2.y)
                {
                    collisionPoint = (i1 + i2) * 0.5f;
                    Vector2 dir = collisionPoint - circle.center;
                    if (Mathf.Approximately(dir.sqrMagnitude, 0f))
                        return true;

                    return CollideCircleLine(circle, collisionPoint, collisionPoint + circle.radius * dir.normalized, out collisionPoint);
                }
                if (Mathf.Min(A.y, B.y) <= i1.y && Mathf.Max(A.y, B.y) >= i1.y)
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
                Vector2 dir2 = collisionPoint - circle.center;
                if (Mathf.Approximately(dir2.sqrMagnitude, 0f))
                    return true;

                return CollideCircleLine(circle, collisionPoint, collisionPoint + circle.radius * dir2.normalized, out collisionPoint);
            }
            else
            {
                float m = (B.y - A.y) / (B.x - A.x);
                float p = A.y - m * A.x;
                float a = 1f + (m * m);
                float b = 2f * ((m * p) - circle.center.x - (m * circle.center.y));
                float C = ((circle.center.x * circle.center.x) + (p * p) - (2f * p * circle.center.y) + (circle.center.y * circle.center.y) - (circle.radius * circle.radius));
                float sqrtDelta = Mathf.Sqrt((b * b) - (4f * a * C));
                Vector2 i1 = new Vector2((-b - sqrtDelta) / (2f * a), m * ((-b - sqrtDelta) / (2f * a)) + p);
                Vector2 i2 = new Vector2((-b + sqrtDelta) / (2f * a), m * ((-b + sqrtDelta) / (2f * a)) + p);

                //on verif que i1 et i2 appartienne au seg
                if (Mathf.Min(A.y, B.y) <= i1.y && Mathf.Max(A.y, B.y) >= i1.y && Mathf.Min(A.x, B.x) <= i1.x && Mathf.Max(A.x, B.x) >= i1.x &&
                    Mathf.Min(A.y, B.y) <= i2.y && Mathf.Max(A.y, B.y) >= i2.y && Mathf.Min(A.x, B.x) <= i2.x && Mathf.Max(A.x, B.x) >= i2.x)
                {
                    collisionPoint = (i1 + i2) * 0.5f;
                    Vector2 dir = collisionPoint - circle.center;
                    if (Mathf.Approximately(dir.sqrMagnitude, 0f))
                        return true;

                    return CollideCircleLine(circle, collisionPoint, collisionPoint + circle.radius * dir.normalized, out collisionPoint);
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
                Vector2 dir2 = collisionPoint - circle.center;
                if (Mathf.Approximately(dir2.sqrMagnitude, 0f))
                    return true;

                return CollideCircleLine(circle, collisionPoint, collisionPoint + circle.radius * dir2.normalized, out collisionPoint);
            }
        }
        public static bool CollideCircleLine(Circle circle, Line2D line, out Vector2 collisionPoint) => CollideCircleLine(circle, line.A, line.B, out collisionPoint);//ok
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
        public static bool CollideCircleRay(Circle circle, Ray2D ray) => circle.CollideRay(ray);//ok
        public static bool CollideCircleRay(Circle circle, in Vector2 A, in Vector2 B) => circle.CollideRay(new Ray2D(A, B));//ok
        public static bool CollideCircleRay(Circle circle, Ray2D ray, out Vector2 collisionPoint) => CollideCircleRay(circle, ray, out collisionPoint);//OK
        public static bool CollideCircleRay(Circle circle, in Vector2 A, in Vector2 B, out Vector2 collisionPoint)//ok
        {
            if (!CollideCircleRay(circle, A, B))
            {
                collisionPoint = Vector2.zero;
                return false;
            }
            Vector2 u = new Vector2(B.x - A.x, B.y - A.y);
            Vector2 AC = new Vector2(circle.center.x - A.x, circle.center.y - A.y);
            float ti = (u.x * AC.x + u.y * AC.y) / (u.x * u.x + u.y * u.y);
            collisionPoint = new Vector2(A.x + ti * u.x, A.y + ti * u.y);
            if (collisionPoint.SqrDistance(circle.center) > circle.radius * circle.radius)
                return true;

            return CollideCircleLine(circle, collisionPoint, collisionPoint + circle.radius * (collisionPoint - circle.center).normalized, out collisionPoint);
        }
        public static bool CollideCircleRay(Circle circle, in Vector2 A, in Vector2 B, out Vector2 collisionPoint, out Vector2 normal)
        {
            if (CollideCircleRay(circle, A, B, out collisionPoint))
            {
                normal = (collisionPoint - circle.center);
                normal.Normalize();
                return true;
            }
            normal = Vector2.zero;
            return false;
        }
        public static bool CollideCircleRay(Circle circle, Ray2D ray, out Vector2 collisionPoint, out Vector2 normal) => CollideCircleRay(circle, ray.A, ray.B, out collisionPoint, out normal);
        public static bool CollideCircleCapsule(Circle circle, Capsule capsule)//ok
        {
            return CollideCircleHitbox(circle, capsule.hitbox) || CollideCircles(circle, capsule.circle1) || CollideCircles(circle, capsule.circle2);
        }
        public static bool CollideCircleCapsule(Circle circle, Capsule capsule, out Vector2 collisionPoint)//OK
        {
            if (CollideCircleHitbox(circle, capsule.hitbox, out collisionPoint))
            {
                cache.Add(collisionPoint);
            }
            if (CollideCircles(circle, capsule.circle2, out collisionPoint))
            {
                cache.Add(collisionPoint);
            }
            if (CollideCircles(circle, capsule.circle1, out collisionPoint))
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

        public static bool CollidePolygones(Polygone polygone1, Polygone polygone2)//OK
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
            return polygone1.Contains(polygone2.center) || polygone2.Contains(polygone1.center);
        }
        public static bool CollidePolygones(Polygone polygone1, Polygone polygone2, out Vector2 collisionPoint)
        {
            collisionPoint = Vector2.zero;
            for (int i = 0; i < polygone1.vertices.Length; i++)
            {
                for (int j = 0; j < polygone2.vertices.Length; j++)
                {
                    if (CollideLines(polygone1.vertices[i], polygone1.vertices[(i + 1) % polygone1.vertices.Length], polygone2.vertices[j], polygone2.vertices[(j + 1) % polygone2.vertices.Length], out Vector2 intersec))
                    {
                        cache.Add(intersec);
                    }
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
        public static bool CollidePolygones(Polygone polygone1, Polygone polygone2, out Vector2 collisionPoint, out Vector2 normal1, out Vector2 normal2)
        {
            for (int i = 0; i < polygone1.vertices.Length; i++)
            {
                for (int j = 0; j < polygone2.vertices.Length; j++)
                {
                    if (CollideLines(polygone1.vertices[i], polygone1.vertices[(i + 1) % polygone1.vertices.Length], polygone2.vertices[j], polygone2.vertices[(j + 1) % polygone2.vertices.Length], out Vector2 intersec))
                    {
                        cache.Add(intersec);
                        Vector2 n1 = (polygone1.vertices[(i + 1) % polygone1.vertices.Length] - polygone1.vertices[i]).NormalVector();
                        //on regarde si on est dans le bon sens
                        Vector2 middle = (polygone1.vertices[i] + polygone1.vertices[(i + 1) % polygone1.vertices.Length]) * 0.5f;
                        if (polygone1.Contains(middle + n1))//tromper de sens
                        {
                            n1 *= -1f;
                        }
                        cache2.Add(n1);//Stocker le vecteur normal au coté de p1

                        Vector2 n2 = (polygone2.vertices[(j + 1) % polygone2.vertices.Length] - polygone2.vertices[j]).NormalVector();
                        n2.Normalize();
                        middle = (polygone2.vertices[j] + polygone2.vertices[(j + 1) % polygone2.vertices.Length]) * 0.5f;
                        if (polygone2.Contains(middle + n2))//tromper de sens
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
        public static bool CollidePolygoneHitbox(Polygone polygone, Hitbox hitbox) => CollidePolygones(hitbox.ToPolygone(), polygone);//OK
        public static bool CollidePolygoneHitbox(Polygone polygone, Hitbox hitbox, out Vector2 collisionPoint) => CollidePolygones(polygone, hitbox.ToPolygone(), out collisionPoint);
        public static bool CollidePolygoneHitbox(Polygone polygone, Hitbox hitbox, out Vector2 collisionPoint, out Vector2 normal1, out Vector2 normal2) => CollidePolygones(polygone, hitbox.ToPolygone(), out collisionPoint, out normal1, out normal2);
        public static bool CollidePolygoneLine(Polygone polygone, in Vector2 A, in Vector2 B) => polygone.CollideLine(new Line2D(A, B));//OK
        public static bool CollidePolygoneLine(Polygone polygone, Line2D line) => polygone.CollideLine(line);//OK
        public static bool CollidePolygoneLine(Polygone polygone, in Vector2 A, in Vector2 B, out Vector2 collisionPoint)
        {
            collisionPoint = Vector2.zero;
            for (int i = 0; i < polygone.vertices.Length; i++)
            {
                if (CollideLines(polygone.vertices[i], polygone.vertices[(i + 1) % polygone.vertices.Length], A, B, out Vector2 intersec))
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
        public static bool CollidePolygoneLine(Polygone polygone, Line2D line, out Vector2 collisionPoint) => CollidePolygoneLine(polygone, line.A, line.B, out collisionPoint);
        public static bool CollidePolygoneLine(Polygone polygone, in Vector2 A, in Vector2 B, out Vector2 collisionPoint, out Vector2 normal)
        {
            collisionPoint = Vector2.zero;
            for (int i = 0; i < polygone.vertices.Length; i++)
            {
                if (CollideLines(polygone.vertices[i], polygone.vertices[(i + 1) % polygone.vertices.Length], A, B, out Vector2 intersec))
                {
                    cache.Add(intersec);
                    Vector2 n = (polygone.vertices[(i + 1) % polygone.vertices.Length] - polygone.vertices[i]).NormalVector();
                    n.Normalize();
                    //on regarde si on est dans le bon sens
                    Vector2 middle = (polygone.vertices[i] + polygone.vertices[(i + 1) % polygone.vertices.Length]) * 0.5f;
                    if (polygone.Contains(middle + n))//tromper de sens
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
        public static bool CollidePolygoneLine(Polygone polygone, Line2D line, out Vector2 collisionPoint, out Vector2 normal) => CollidePolygoneLine(polygone, line.A, line.B, out collisionPoint, out normal);
        public static bool CollidePolygoneRay(Polygone polygone, in Vector2 A, in Vector2 B) => polygone.CollideRay(new Ray2D(A, B));//OK
        public static bool CollidePolygoneRay(Polygone polygone, Ray2D ray) => polygone.CollideRay(ray);//OK
        public static bool CollidePolygoneRay(Polygone polygone, in Vector2 A, in Vector2 B, out Vector2 collisionPoint)
        {
            collisionPoint = Vector2.zero;
            for (int i = 0; i < polygone.vertices.Length; i++)
            {
                if (CollideLineRay(polygone.vertices[i], polygone.vertices[(i + 1) % polygone.vertices.Length], A, B, out Vector2 intersec))
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
        public static bool CollidePolygoneRay(Polygone polygone, Ray2D ray, out Vector2 collisionPoint) => CollidePolygoneRay(polygone, ray.A, ray.B, out collisionPoint);
        public static bool CollidePolygoneRay(Polygone polygone, in Vector2 A, in Vector2 B, out Vector2 collisionPoint, out Vector2 normal)
        {
            collisionPoint = Vector2.zero;
            for (int i = 0; i < polygone.vertices.Length; i++)
            {
                if (CollideLineRay(polygone.vertices[i], polygone.vertices[(i + 1) % polygone.vertices.Length], A, B, out Vector2 intersec))
                {
                    cache.Add(intersec);
                    Vector2 n = (polygone.vertices[(i + 1) % polygone.vertices.Length] - polygone.vertices[i]).NormalVector();
                    n.Normalize();
                    //on regarde si on est dans le bon sens
                    Vector2 middle = (polygone.vertices[i] + polygone.vertices[(i + 1) % polygone.vertices.Length]) * 0.5f;
                    if (polygone.Contains(middle + n))//tromper de sens
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
        public static bool CollidePolygoneRay(Polygone polygone, Ray2D ray, out Vector2 collisionPoint, out Vector2 normal) => CollidePolygoneRay(polygone, ray.A, ray.B, out collisionPoint, out normal);
        public static bool CollidePolygoneCapsule(Polygone polygone, Capsule capsule) => CollidePolygoneHitbox(polygone, capsule.hitbox) || CollideCirclePolygone(capsule.circle1, polygone) || CollideCirclePolygone(capsule.circle2, polygone);//OK
        public static bool CollidePolygoneCapsule(Polygone polygone, Capsule capsule, out Vector2 collisionPoint)
        {
            if (CollidePolygoneHitbox(polygone, capsule.hitbox, out collisionPoint))
            {
                cache.Add(collisionPoint);
            }
            if (CollideCirclePolygone(capsule.circle1, polygone, out collisionPoint))
            {
                cache.Add(collisionPoint);
            }
            if (CollideCirclePolygone(capsule.circle2, polygone, out collisionPoint))
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
        public static bool CollidePolygoneCapsule(Polygone polygone, Capsule capsule, out Vector2 collisionPoint, out Vector2 normal1, out Vector2 normal2)
        {
            collisionPoint = normal1 = normal2 = Vector2.zero;
            if (CollideCirclePolygone(capsule.circle1, polygone, out Vector2 col, out Vector2 n1, out Vector2 n2))
            {
                cache.Add(col);
                cache2.Add(n1);
                cache3.Add(n2);
            }
            if (CollideCirclePolygone(capsule.circle2, polygone, out col, out n1, out n2))
            {
                cache.Add(col);
                cache2.Add(n1);
                cache3.Add(n2);
            }
            if (CollidePolygoneHitbox(polygone, capsule.hitbox, out col, out n1, out n2))
            {
                cache.Add(col);
                //cache2.Add(n1);
                //cache3.Add(n2);
                normal1 = n1;
                normal2 = n2;
            }
            if (cache.Count > 0)
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

        #endregion

        #region Collide(Hitbox, other)

        public static bool CollideHitboxs(Hitbox hitbox1, Hitbox hitbox2) => CollidePolygones(hitbox1.ToPolygone(), hitbox2.ToPolygone());
        public static bool CollideHitboxs(Hitbox hitbox1, Hitbox hitbox2, out Vector2 collisionPoint) => CollidePolygones(hitbox1.ToPolygone(), hitbox2.ToPolygone(), out collisionPoint);
        public static bool CollideHitboxs(Hitbox hitbox1, Hitbox hitbox2, out Vector2 collisionPoint, out Vector2 normal1, out Vector2 normal2) => CollidePolygones(hitbox1.ToPolygone(), hitbox2.ToPolygone(), out collisionPoint, out normal1, out normal2);
        public static bool CollideHitboxLine(Hitbox hitbox, in Vector2 A, in Vector2 B) => hitbox.ToPolygone().CollideLine(new Line2D(A, B));
        public static bool CollideHitboxLine(Hitbox hitbox, Line2D line) => hitbox.ToPolygone().CollideLine(line);
        public static bool CollideHitboxLine(Hitbox hitbox, in Vector2 A, in Vector2 B, out Vector2 collisionPoint) => CollidePolygoneLine(hitbox.ToPolygone(), A, B, out collisionPoint);
        public static bool CollideHitboxLine(Hitbox hitbox, Line2D line, out Vector2 collisionPoint) => CollidePolygoneLine(hitbox.ToPolygone(), line.A, line.B, out collisionPoint);
        public static bool CollideHitboxLine(Hitbox hitbox, in Vector2 A, in Vector2 B, out Vector2 collisionPoint, out Vector2 normal) => CollidePolygoneLine(hitbox.ToPolygone(), A, B, out collisionPoint, out normal);
        public static bool CollideHitboxLine(Hitbox hitbox, Line2D line, out Vector2 collisionPoint, out Vector2 normal) => CollidePolygoneLine(hitbox.ToPolygone(), line.A, line.B, out collisionPoint, out normal);
        public static bool CollideHitboxRay(Hitbox hitbox, Ray2D ray) => hitbox.ToPolygone().CollideRay(ray);
        public static bool CollideHitboxRay(Hitbox hitbox, in Vector2 A, in Vector2 B) => hitbox.ToPolygone().CollideRay(new Ray2D(A, B));
        public static bool CollideHitboxRay(Hitbox hitbox, in Vector2 A, in Vector2 B, out Vector2 collisionPoint) => CollidePolygoneRay(hitbox.ToPolygone(), A, B, out collisionPoint);
        public static bool CollideHitboxRay(Hitbox hitbox, Line2D line, out Vector2 collisionPoint) => CollidePolygoneRay(hitbox.ToPolygone(), line.A, line.B, out collisionPoint);
        public static bool CollideHitboxRay(Hitbox hitbox, in Vector2 A, in Vector2 B, out Vector2 collisionPoint, out Vector2 normal) => CollidePolygoneRay(hitbox.ToPolygone(), A, B, out collisionPoint, out normal);
        public static bool CollideHitboxRay(Hitbox hitbox, Line2D lie, out Vector2 collisionPoint, out Vector2 normal) => CollidePolygoneRay(hitbox.ToPolygone(), lie.A, lie.B, out collisionPoint, out normal);
        public static bool CollideHitboxCapsule(Hitbox hitbox, Capsule capule) => CollideHitboxs(hitbox, capule.hitbox) || CollideCircleHitbox(capule.circle1, hitbox) || CollideCircleHitbox(capule.circle2, hitbox);
        public static bool CollideHitboxCapsule(Hitbox hitbox, Capsule capsule, out Vector2 collisionPoint) => CollidePolygoneCapsule(hitbox.ToPolygone(), capsule, out collisionPoint);
        public static bool CollideHitboxCapsule(Hitbox hitbox, Capsule capsule, out Vector2 collisionPoint, out Vector2 normal1, out Vector2 normal2) => CollidePolygoneCapsule(hitbox.ToPolygone(), capsule, out collisionPoint, out normal1, out normal2);

        #endregion

        #region Collide(Capsule, other)

        public static bool CollideCapsules(Capsule capsule1, Capsule capsule2) => CollideHitboxCapsule(capsule1.hitbox, capsule2) || CollideCircleCapsule(capsule1.circle1, capsule2) || CollideCircleCapsule(capsule1.circle2, capsule2);
        public static bool CollideCapsules(Capsule capsule1, Capsule capsule2, out Vector2 collisionPoint)
        {
            if (CollideHitboxs(capsule1.hitbox, capsule2.hitbox, out collisionPoint))
            {
                cache.Add(collisionPoint);
            }
            if (CollideCircleHitbox(capsule2.circle1, capsule1.hitbox, out collisionPoint))
            {
                cache.Add(collisionPoint);
            }
            if (CollideCircleHitbox(capsule2.circle2, capsule1.hitbox, out collisionPoint))
            {
                cache.Add(collisionPoint);
            }
            if (CollideCircleHitbox(capsule1.circle1, capsule2.hitbox, out collisionPoint))
            {
                cache.Add(collisionPoint);
            }
            if (CollideCircles(capsule1.circle1, capsule2.circle1, out collisionPoint))
            {
                cache.Add(collisionPoint);
            }
            if (CollideCircles(capsule1.circle1, capsule2.circle2, out collisionPoint))
            {
                cache.Add(collisionPoint);
            }
            if (CollideCircleHitbox(capsule1.circle2, capsule2.hitbox, out collisionPoint))
            {
                cache.Add(collisionPoint);
            }
            if (CollideCircles(capsule1.circle2, capsule2.circle1, out collisionPoint))
            {
                cache.Add(collisionPoint);
            }
            if (CollideCircles(capsule1.circle2, capsule2.circle2, out collisionPoint))
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
        public static bool CollideCapsuleLine(Capsule capsule, Line2D line) => capsule.CollideLine(line);
        public static bool CollideCapsuleLine(Capsule capsule, in Vector2 A, in Vector2 B) => capsule.CollideLine(new Line2D(A, B));
        public static bool CollideCapsuleLine(Capsule capsule, in Vector2 A, in Vector2 B, out Vector2 collisionPoint)
        {
            if (CollideCircleLine(capsule.circle1, A, B, out collisionPoint))
            {
                cache.Add(collisionPoint);
            }
            if (CollideCircleLine(capsule.circle2, A, B, out collisionPoint))
            {
                cache.Add(collisionPoint);
            }
            if (CollideHitboxLine(capsule.hitbox, A, B, out collisionPoint))
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
        public static bool CollideCapsuleLine(Capsule capsule, Line2D line, out Vector2 collisionPoint) => CollideCapsuleLine(capsule, line.A, line.B, out collisionPoint);
        public static bool CollideCapsuleLine(Capsule capsule, in Vector2 A, in Vector2 B, out Vector2 collisionPoint, out Vector2 normal)
        {
            if (CollideCapsuleLine(capsule, A, B, out collisionPoint))
            {
                normal = collisionPoint - capsule.center;
                normal.Normalize();
                return true;
            }
            normal = Vector2.zero;
            return false;
        }
        public static bool CollideCapsuleLine(Capsule capsule, Line2D line, out Vector2 collisionPoint, out Vector2 normal) => CollideCapsuleLine(capsule, line.A, line.B, out collisionPoint, out normal);
        public static bool CollideCapsuleRay(Capsule capsule, Ray2D ray) => capsule.CollideRay(ray);
        public static bool CollideCapsuleRay(Capsule capsule, in Vector2 A, in Vector2 B) => capsule.CollideRay(new Ray2D(A, B));
        public static bool CollideCapsuleRay(Capsule capsule, in Vector2 A, in Vector2 B, out Vector2 collisionPoint)
        {
            if (CollideCircleRay(capsule.circle1, A, B, out collisionPoint))
            {
                cache.Add(collisionPoint);
            }
            if (CollideCircleRay(capsule.circle2, A, B, out collisionPoint))
            {
                cache.Add(collisionPoint);
            }
            if (CollideHitboxRay(capsule.hitbox, A, B, out collisionPoint))
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
        public static bool CollideCapsuleRay(Capsule capsule, Line2D line, out Vector2 collisionPoint) => CollideCapsuleRay(capsule, line.A, line.B, out collisionPoint);
        public static bool CollideCapsuleRay(Capsule capsule, in Vector2 A, in Vector2 B, out Vector2 collisionPoint, out Vector2 normal)
        {
            if (CollideCapsuleRay(capsule, A, B, out collisionPoint))
            {
                normal = collisionPoint - capsule.center;
                normal.Normalize();
                return true;
            }
            normal = Vector2.zero;
            return false;
        }
        public static bool CollideCapsuleRay(Capsule capsule, Line2D line, out Vector2 collisionPoint, out Vector2 normal) => CollideCapsuleRay(capsule, line.A, line.B, out collisionPoint, out normal);


        #endregion

        #region Collide(Lines, Ray)

        public static bool CollideRays(Ray2D ray1, Ray2D ray2) => CollideRays(ray1.A, ray1.B, ray2.A, ray2.B);//OK
        public static bool CollideRays(Ray2D ray1, Ray2D ray2, out Vector2 collisionPoint) => CollideRays(ray1.A, ray1.B, ray2.A, ray2.B, out collisionPoint);//OK
        private static bool CollideRays(in Vector2 A, in Vector2 B, in Vector2 O, in Vector2 P)
        {
            return !(B - A).IsCollinear(P - O);
        }//OK
        private static bool CollideRays(in Vector2 A, in Vector2 B, in Vector2 O, in Vector2 P, out Vector2 collisionPoint)
        {
            //on regarde si une des droites est verticale
            if (Mathf.Approximately(B.x, A.x) || Mathf.Approximately(P.x, O.x))
            {
                //si les 2 sont verticales
                if (Mathf.Approximately(B.x, A.x) && Mathf.Approximately(P.x, O.x))
                {
                    if (!Mathf.Approximately(((A.x + B.x) * 0.5f), ((O.x + P.x) * 0.5f)))
                    {
                        collisionPoint = Vector2.zero;
                        return false;
                    }
                    collisionPoint = new Vector2((O.x + P.x + A.x + B.x) * 0.25f, (O.y + P.y + A.y + B.y) * 0.25f);
                    return true;
                }
                float a, b, ySol;
                if (Mathf.Approximately(B.x, A.x))//(AB) verticale mais pas (OP)
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
            if (Mathf.Approximately(A.y, B.y) || Mathf.Approximately(O.y, P.y))
            {
                if (Mathf.Approximately(A.y, B.y) && Mathf.Approximately(O.y, P.y))//les 2 droites sont horizontales
                {
                    if (Mathf.Approximately(((A.y + B.y) * 0.5f), ((O.y + P.y) * 0.5f)))
                    {
                        collisionPoint = new Vector2((A.x + B.x + O.x + P.x) * 0.25f, (A.y + B.y + O.y + P.y) * 0.25f);
                        return true;
                    }
                    collisionPoint = Vector2.zero;
                    return false;
                }
                float a, b, xSol;
                if (Mathf.Approximately(A.y, B.y))//droite AB horizontal, seg OP non horizontal
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
            else if (Mathf.Approximately(b2, b1))
            {
                collisionPoint = new Vector2((A.x + B.x + O.x + P.x) / 4f, ((a1 + a2) * 0.5f) * ((A.x + B.x + O.x + P.x) / 4f) + ((b1 + b2) * 0.5f));
                return true;
            }
            collisionPoint = Vector2.zero;
            return false;
        }
        public static bool CollideLines(Line2D line1, Line2D line2) => CollideLines(line1.A, line1.B, line2.A, line2.B);//OK
        private static bool CollideLines(in Vector2 A, in Vector2 B, in Vector2 O, in Vector2 P)//OK
        {
            return CollideLineRay(A, B, O, P) && CollideLineRay(O, P, A, B);
        }
        public static bool CollideLines(Line2D line1, Line2D line2, out Vector2 collisionPoint) => CollideLines(line1.A, line1.B, line2.A, line2.B, out collisionPoint);//OK
        private static bool CollideLines(in Vector2 A, in Vector2 B, in Vector2 O, in Vector2 P, out Vector2 collisionPoint)//OK
        {
            //on regarde si un des 2 segments est vertical
            if (Mathf.Approximately(B.x, A.x) || Mathf.Approximately(P.x, O.x))
            {
                //si les 2 sont verticals
                if (Mathf.Approximately(B.x, A.x) && Mathf.Approximately(P.x, O.x))
                {
                    if (!Mathf.Approximately(A.x, O.x))
                    {
                        collisionPoint = Vector2.zero;
                        return false;
                    }
                    float minDesMax = Mathf.Min(Mathf.Max(A.y, B.y), Mathf.Max(O.y, P.y));
                    float maxDesMin = Mathf.Max(Mathf.Min(A.y, B.y), Mathf.Min(O.y, P.y));
                    if (minDesMax >= maxDesMin)
                    {
                        collisionPoint = new Vector2((A.x + B.x + O.x + P.x) * 0.25f, (maxDesMin + minDesMax) * 0.5f);
                        return true;
                    }
                    collisionPoint = Vector2.zero;
                    return false;
                }
                float a, b, ySol;
                if (Mathf.Approximately(B.x, A.x))//AB vertical mais pas OP
                {
                    if (!(((A.x + B.x) * 0.5f) >= Mathf.Min(O.x, P.x) && ((A.x + B.x) * 0.5f) <= Mathf.Max(O.x, P.x)))
                    {
                        collisionPoint = Vector2.zero;
                        return false;
                    }
                    a = (P.y - O.y) / (P.x - O.x);
                    b = O.y - a * O.x;
                    ySol = a * ((A.x + B.x) * 0.5f) + b;
                    if (Mathf.Min(A.y, B.y) <= ySol && Mathf.Max(A.y, B.y) >= ySol)
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
                if (Mathf.Min(O.y, P.y) <= ySol && Mathf.Max(O.y, P.y) >= ySol)
                {
                    collisionPoint = new Vector2((O.x + P.x) * 0.5f, ySol);
                    return true;
                }
                collisionPoint = Vector2.zero;
                return false;
            }
            //on regarde si un des 2 segment est horizontale
            if (Mathf.Approximately(A.y, B.y) || Mathf.Approximately(O.y, P.y))
            {
                if (Mathf.Abs(A.y - B.y) < 1f && Mathf.Abs(O.y - P.y) < 1f)//les 2 segments sont horizontaux
                {
                    if (Mathf.Approximately((A.y + B.y) * 0.5f, (O.y + P.y) * 0.5f))
                    {
                        float minDesMax = Mathf.Min(Mathf.Max(A.x, B.x), Mathf.Max(O.x, P.x));
                        float maxDesMin = Mathf.Max(Mathf.Min(A.x, B.x), Mathf.Min(O.x, P.x));
                        if (minDesMax >= maxDesMin)//si il y a collision
                        {
                            collisionPoint = new Vector2((maxDesMin + minDesMax) * 0.5f, (A.y + B.y + O.y + P.y) * 0.25f);
                            return true;
                        }
                    }
                    collisionPoint = Vector2.zero;
                    return false;
                }
                float a, b, xSol;
                if (Mathf.Approximately(A.y, B.y))//AB horizontal, OP non horizontal
                {
                    a = (P.y - O.y) / (P.x - O.x);
                    b = O.y - a * O.x;
                    xSol = (((A.y + B.y) * 0.5f) - b) / a;
                    if (Mathf.Min(A.x, B.x) <= xSol && Mathf.Max(A.x, B.x) >= xSol && Mathf.Min(O.x, P.x) <= xSol && Mathf.Max(O.x, P.x) >= xSol)
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
            if (!Mathf.Approximately(a1, a2))
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
            else if (Mathf.Abs(b2 - b1) < 1f)
            {
                collisionPoint = new Vector2((A.x + B.x + O.x + P.x) * 0.25f, ((a1 + a2) * 0.5f) * ((A.x + B.x + O.x + P.x) * 0.25f) + ((b1 + b2) * 0.5f));
                return true;
            }
            collisionPoint = Vector2.zero;
            return false;
        }
        public static bool CollideLineRay(Line2D line, Ray2D ray) => CollideLineRay(line.A, line.B, ray.A, ray.B);//OK
        private static bool CollideLineRay(in Vector2 O, in Vector2 P, in Vector2 A, in Vector2 B)
        {
            Vector2 AB = B - A;
            Vector2 AP = P - A;
            Vector2 AO = O - A;
            return (AB.x * AP.y - AB.y * AP.x) * (AB.x * AO.y - AB.y * AO.x) < 0f;
        }//OK
        private static bool CollideLineRay(in Vector2 O, in Vector2 P, in Vector2 A, in Vector2 B, out Vector2 collisionPoint)//OK
        {
            //on regarde si le segment ou la droite est vertical
            if (Mathf.Approximately(B.x, A.x) || Mathf.Approximately(P.x, O.x))
            {
                //si les 2 sont verticals
                if (Mathf.Approximately(B.x, A.x) && Mathf.Approximately(P.x, O.x))
                {
                    if (!Mathf.Approximately(A.x, O.x))
                    {
                        collisionPoint = Vector2.zero;
                        return false;
                    }
                    collisionPoint = new Vector2((O.x + P.x + A.x + B.x) * 0.25f, Mathf.Min(O.y, P.y) + Mathf.Abs(O.y - P.y) * 0.5f);
                    return true;
                }
                float a, b, ySol;
                if (Mathf.Approximately(B.x, A.x))//AB vertical mais pas OP
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
            if (Mathf.Approximately(A.y, B.y) || Mathf.Approximately(O.y, P.y))
            {
                if (Mathf.Abs(A.y - B.y) < 1f && Mathf.Abs(O.y - P.y) < 1f)//le segment et la droite sont horizontaux
                {
                    if (Mathf.Approximately(((A.y + B.y) * 0.5f), ((O.y + P.y) * 0.5f)))
                    {
                        collisionPoint = new Vector2((O.x + P.x) * 0.5f, (A.y + B.y + O.y + P.y) * 0.25f);
                        return true;
                    }
                    collisionPoint = Vector2.zero;
                    return false;
                }
                float a, b, xSol;
                if (Mathf.Approximately(A.y, B.y))//droite AB horizontal, seg OP non horizontal
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
            else if (Mathf.Approximately(b2, b1))
            {
                collisionPoint = new Vector2((O.x + P.x) * 0.5f, (a1 + a2) * 0.5f * ((O.x + P.x) * 0.5f) + ((b1 + b2) * 0.5f));
                return true;
            }
            collisionPoint = Vector2.zero;
            return false;
        }
        public static bool CollideLineDroite(Line2D line, Ray2D ray, out Vector2 collisionPoint) => CollideLineRay(line.A, line.B, ray.A, ray.B, out collisionPoint);//OK

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

        protected Vector2 scale;

        protected Collider2D()
        {
            scale = Vector2.one;
        }
        protected Collider2D(UnityEngine.Collider2D collider)
        {
            scale = Vector2.one;
        }

        public virtual Collider2D Clone() => null;
        public virtual bool Collide(Collider2D collider) => false;
        public virtual bool CollideLine(Line2D line) => false;
        public virtual bool CollideRay(Ray2D d) => false;
        public virtual bool Contains(in Vector2 p) => false;
        public virtual Vector2 ClosestPoint(in Vector2 point) => default;
        public virtual void MoveAt(in Vector2 position) { }
        public virtual void Rotate(float angle) { }
        public virtual Hitbox ToHitbox() => null;
        public virtual void SetScale(in Vector2 scale) { }
        public virtual bool Normal(in Vector2 point, out Vector2 normal) { normal = Vector2.zero; return false; }
    }

    #endregion

    #region Polygone

    public class Polygone : Collider2D
    {
        public static void GizmosDraw(Vector2[] points)
        {
            for (int i = 0; i < points.Length; i++)
            {
                Gizmos.DrawLine(points[i], points[(i + 1) % points.Length]);
            }
        }

        public static void GizmosDraw(Polygone p) => GizmosDraw(p.vertices);

        public Vector2[] vertices { get; private set; }
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

        public Polygone(Vector2[] vertices) : base()
        {
            Builder(vertices.ToList());
            center = Vector2.zero;
            foreach (Vector2 pos in this.vertices)
            {
                center += pos;
            }
            center /= vertices.Length;
            inclusiveCircle = GetInclusiveCircle();
        }

        public Polygone(List<Vector2> vertices) : base()
        {
            Builder(vertices);
            center = Vector2.zero;
            foreach (Vector2 pos in this.vertices)
            {
                center += pos;
            }
            center /= vertices.Count;
            inclusiveCircle = GetInclusiveCircle();
        }

        public Polygone(Vector2[] vertices, in Vector2 center) : base()
        {
            Builder(vertices.ToList());
            this.center = center;
            inclusiveCircle = GetInclusiveCircle();
        }

        public Polygone(List<Vector2> vertices, in Vector2 center) : base()
        {
            Builder(vertices);
            this.center = center;
            inclusiveCircle = GetInclusiveCircle();
        }

        public Polygone(PolygonCollider2D poly) : base(poly)
        {
            List<Vector2> vertices = new List<Vector2>();
            poly.GetPath(0, vertices);
            this.vertices = new Vector2[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
            {
                this.vertices[i] = vertices[i] + (Vector2)poly.transform.position;
            }

            center = Vector2.zero;
            foreach (Vector2 pos in this.vertices)
            {
                center += pos;
            }
            center /= vertices.Count;
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
            Vector2 A = vertices[0], B, projectOrto;
            Vector2 res = A;
            float minSqrDist = A.SqrDistance(point), d;

            for (int i = 1; i < vertices.Length; i++)
            {
                B = vertices[i];
                d = B.SqrDistance(point);
                if (d < minSqrDist)
                {
                    minSqrDist = d;
                    res = B;
                }

                projectOrto = Ray2D.OrthogonalProjection(point, new Ray2D(A, B));
                if (Line2D.Contain(A, B, projectOrto))
                {
                    d = projectOrto.SqrDistance(point);
                    if (d < minSqrDist)
                    {
                        minSqrDist = d;
                        res = projectOrto;
                    }
                }
            }

            A = vertices[0];
            B = vertices.Last();
            projectOrto = Ray2D.OrthogonalProjection(point, new Ray2D(A, B));
            if (Line2D.Contain(A, B, projectOrto))
            {
                d = projectOrto.SqrDistance(point);
                if (d < minSqrDist)
                {
                    minSqrDist = d;
                    res = projectOrto;
                }
            }
            return res;
        }

        #region Contain

        private static int count = 0;

        public override bool Contains(in Vector2 P)
        {
            if (vertices == null || vertices.Length < 3)
                return false;

            Polygone.count++;

            if (Polygone.count > 5)
            {
                Polygone.count = count + 1;
                Polygone.count = count - 1;
            }


            int i;
            Vector2 I = ExternalPoint();
            int nbintersections = 0;
            for (i = 0; i < vertices.Length; i++)
            {
                Vector2 A = vertices[i];
                Vector2 B = vertices[(i + 1) % vertices.Length];
                int iseg = Intersectsegment(A, B, I, P);
                if (iseg == -1)
                    return Contains(P);  // cas limite, on relance la fonction.
                nbintersections += iseg;
            }

            Polygone.count = 0;
            return Useful.IsOdd(nbintersections);

            Vector2 ExternalPoint()
            {
                float maxDist = 0;
                for (int i = 0; i < vertices.Length; i++)
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

        public override bool CollideLine(Line2D l)
        {
            if (!CollideCircleLine(inclusiveCircle, l.A, l.B))
                return false;

            for (int i = 0; i < vertices.Length; i++)
            {
                if (CollideLines(l, new Line2D(vertices[i], vertices[(i + 1) % vertices.Length])))
                {
                    return true;
                }
            }
            return Contains(l.A) || Contains(l.B);
        }

        public override bool CollideRay(Ray2D ray)
        {
            if (!CollideCircleRay(inclusiveCircle, ray.A, ray.B))
                return false;

            for (int i = 0; i < vertices.Length; i++)
            {
                if (CollideLineRay(new Line2D(vertices[i], vertices[(i + 1) % vertices.Length]), ray))
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

        public override void SetScale(in Vector2 scale)
        {
            Vector2 ratio = scale / this.scale;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = center - (center - vertices[i]) * ratio;
            }
            inclusiveCircle.SetScale(scale);
            this.scale = scale;
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
            base.Rotate(angle);
        }

        public override bool Normal(in Vector2 point, out Vector2 normal)
        {
            int minIndex = 0;
            float minDist = Line2D.Distance(vertices[0], vertices[1], point);
            float tmpDist;
            for (int i = 1; i < vertices.Length; i++)
            {
                tmpDist = Line2D.Distance(vertices[i], vertices[(i + 1) % vertices.Length], point);
                if (tmpDist < minDist)
                {
                    minDist = tmpDist;
                    minIndex = i;
                }
            }

            Vector2 A = vertices[minIndex]; Vector2 B = vertices[(minIndex + 1) % vertices.Length];
            float sqrDist = A.SqrDistance(B);
            if (Mathf.Approximately(sqrDist, 0f))
            {
                return base.Normal(point, out normal);
            }
            normal = Line2D.Normal(A, B);
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
    }

    #endregion

    #region Hitbox

    public class Hitbox : Collider2D
    {
        public static void GizmosDraw(Hitbox hitbox) => Polygone.GizmosDraw(hitbox.rec);
        public static void GizmosDraw(in Vector2 center, in Vector2 size, float angle)
        {
            Hitbox h = new Hitbox(center, size);
            h.Rotate(angle);
            GizmosDraw(h);
        }
        public static void GizmosDraw(in Vector2 center, in Vector2 size)
        {
            GizmosDraw(new Hitbox(center, size));
        }

        protected Polygone rec;
        public Vector2 size;
        public Vector2[] vertices => rec.vertices;

        public override Circle inclusiveCircle
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
        public override bool CollideRay(Ray2D d) => CollideHitboxRay(this, d);
        public override bool CollideLine(Line2D l) => CollideHitboxLine(this, l);

        public override Vector2 ClosestPoint(in Vector2 point)
        {
            return rec.ClosestPoint(point);
        }

        public override void SetScale(in Vector2 scale)
        {
            rec.SetScale(scale);
            size *= scale / this.scale;
            this.scale = scale;
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

        public override Collider2D Clone()
        {
            Hitbox res = new Hitbox(center, size);
            res.Rotate(AngleHori());
            return res;
        }

        public override string ToString() => rec.ToString();
        public override bool Normal(in Vector2 point, out Vector2 normal) => rec.Normal(point, out normal);
    }

    #endregion

    #region Circle

    public class Circle : Collider2D
    {
        public static void GizmosDraw(in Vector2 center, float radius)
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

        public Circle(in Vector2 center, float radius) : base()
        {
            this.center = center;
            this.radius = radius;
        }

        public Circle(CircleCollider2D circle) : base(circle)
        {
            radius = circle.radius;
            center = (Vector2)circle.transform.position + circle.offset;
        }

        #region CollideLine

        public override bool CollideLine(Line2D line)
        {
            Vector2 u = new Vector2(line.B.x - line.A.x, line.B.y - line.A.y);
            Vector2 AC = new Vector2(center.x - line.A.x, center.y - line.A.y);
            float CI = Mathf.Abs(u.x * AC.y - u.y * AC.x) / u.magnitude;
            if (CI > radius)
                return false;
            else
            {
                Vector2 AB = new Vector2(line.B.x - line.A.x, line.B.y - line.A.y);
                AC = new Vector2(center.x - line.A.x, center.y - line.A.y);
                Vector2 BC = new Vector2(center.x - line.B.x, center.y - line.B.y);
                float pscal1 = AB.x * AC.x + AB.y * AC.y;  // produit scalaire
                float pscal2 = (-AB.x) * BC.x + (-AB.y) * BC.y;  // produit scalaire
                if (pscal1 >= 0 && pscal2 >= 0)
                    return true;   // I entre A et B, ok.
                                   // dernière possibilité, A ou B dans le cercle
                return center.SqrDistance(line.A) < radius * radius || center.SqrDistance(line.B) < radius * radius;
            }
        }
        public override bool CollideRay(Ray2D ray)
        {
            Vector2 u = new Vector2(ray.B.x - ray.A.x, ray.B.y - ray.A.y);
            Vector2 AC = new Vector2(center.x - ray.A.x, center.y - ray.A.y);
            float numerateur = Mathf.Abs(u.x * AC.y - u.y * AC.x);// norme du vecteur v
            return numerateur / u.magnitude < radius;
        }

        #endregion

        public override bool Collide(Collider2D collider) => Collider2D.Collide(collider, this);

        public override Vector2 ClosestPoint(in Vector2 point)
        {
            return center + (point - center).normalized * radius;
        }

        public override Hitbox ToHitbox() => new Hitbox(center, new Vector2(radius, radius));

        public override void MoveAt(in Vector2 position)
        {
            center = position;
        }

        public override void Rotate(float angle) { }

        public override void SetScale(in Vector2 scale)
        {
            radius *= (((scale.x + this.scale.y) * 0.5f) / ((scale.x + this.scale.y) * 0.5f));
            this.scale = scale;
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
        public static void GizmosDraw(Capsule capsule)
        {
            Circle.GizmosDraw(capsule.circle1);
            Circle.GizmosDraw(capsule.circle2);
            Hitbox.GizmosDraw(capsule.hitbox);
        }

        public Circle circle1, circle2;
        public Hitbox hitbox;

        public override Vector2 center
        {
            get => hitbox.center;
            protected set { MoveAt(value); }
        }

        public CapsuleDirection2D direction;

        public Capsule(in Vector2 center, in Vector2 size) : base()
        {
            direction = size.x >= size.y ? CapsuleDirection2D.Horizontal : CapsuleDirection2D.Vertical;
            Builder(center, size, direction);
        }

        public Capsule(in Vector2 center, in Vector2 size, CapsuleDirection2D direction) : base()
        {
            Builder(center, size, direction);
        }

        public Capsule(CapsuleCollider2D capsule) : base(capsule)
        {
            Builder((Vector2)capsule.transform.position + capsule.offset, capsule.size, capsule.direction);
        }

        private void Builder(in Vector2 center, in Vector2 size, CapsuleDirection2D direction)
        {
            hitbox = new Hitbox(center, size);
            this.direction = direction;
            if (direction == CapsuleDirection2D.Horizontal)
            {
                circle1 = new Circle(new Vector2(center.x - size.x * 0.5f, center.y), size.y * 0.5f);
                circle2 = new Circle(new Vector2(center.x + size.x * 0.5f, center.y), size.y * 0.5f);
            }
            else
            {
                circle1 = new Circle(new Vector2(center.x, center.y - size.y * 0.5f), size.x * 0.5f);
                circle2 = new Circle(new Vector2(center.x, center.y + size.y * 0.5f), size.x * 0.5f);
            }
            inclusiveCircle = GetInclusiveCircle();
        }

        public override Collider2D Clone()
        {
            Capsule clone = new Capsule(center, hitbox.size, direction);
            clone.hitbox = (Hitbox)hitbox.Clone();
            clone.circle1 = (Circle)circle1.Clone();
            clone.circle2 = (Circle)circle2.Clone();
            return clone;
        }

        public float AngleHori() => hitbox.AngleHori();

        public override bool CollideLine(Line2D line)
        {
            return CollideHitboxLine(hitbox, line.A, line.B) || CollideCircleLine(circle1, line.A, line.B) || CollideCircleLine(circle2, line.A, line.B);
        }
        public override bool CollideRay(Ray2D ray)
        {
            return CollideHitboxRay(hitbox, ray.A, ray.B) || CollideCircleRay(circle1, ray.A, ray.B) || CollideCircleRay(circle2, ray.A, ray.B);
        }

        public override bool Collide(Collider2D c) => Collider2D.Collide(c, this);

        public override Vector2 ClosestPoint(in Vector2 point)
        {
            Vector2 p1 = hitbox.ClosestPoint(point);
            Vector2 p2 = circle1.ClosestPoint(point);
            Vector2 p3 = circle2.ClosestPoint(point);
            float d1 = p1.SqrDistance(point);
            float d2 = p2.SqrDistance(point);
            float d3 = p3.SqrDistance(point);

            if (d1 < d2)
            {
                return d1 < d3 ? p1 : p3;
            }
            else
            {
                return d2 < d3 ? p2 : p3;
            }
        }

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

        public override bool Contains(in Vector2 p) => hitbox.Contains(p) || circle1.Contains(p) || circle2.Contains(p);
        public override Hitbox ToHitbox() => hitbox;

        protected Circle GetInclusiveCircle()
        {
            if (direction == CapsuleDirection2D.Horizontal)
                return new Circle(center, Mathf.Max(hitbox.size.x + circle1.radius + circle2.radius, hitbox.size.y) * 0.5f);
            else
                return new Circle(center, Mathf.Max(hitbox.size.x + circle1.radius + circle2.radius, hitbox.size.y) * 0.5f);
        }

        public override void SetScale(in Vector2 scale)
        {
            circle1.SetScale(scale);
            circle2.SetScale(scale);
            hitbox.SetScale(scale);

            Vector2 ratio = scale / this.scale;
            circle1.MoveAt(center + (circle1.center - center) * ratio);
            circle2.MoveAt(center + (circle2.center - center) * ratio);

            inclusiveCircle.SetScale(scale);
            this.scale = scale;
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
    }

    #endregion
}

#endregion

#region 3D Collisions

namespace Collission3D
{
    #region customCollider3

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

        public const float accuracy = 1e-5f;

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
            if (denum <= CustomCollider3D.accuracy)
                return float.MaxValue;

            Vector3 v = d.A - A;
            float k = Mathf.Abs(v.x * v1.y * v2.z + v.y * v1.z * v2.x + v1.x * v2.y * v.z - v.z * v1.y * v2.x - v.y * v1.x * v2.z - v.x * v1.z * v2.y);

            return k * k / denum;
        }

        public float Distance(Droite3D d)
        {
            Vector3 v1 = B - A, v2 = d.B - d.A;
            float denum = v1.Cross(v2).magnitude;
            if (denum <= CustomCollider3D.accuracy)
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
            radius *= (scale.x + scale.y + scale.z) / 3f;
        }

        public override bool Normal(in Vector3 point, out Vector3 normal)
        {
            if (Mathf.Abs(point.SqrDistance(center) - radius * radius) <= accuracy)
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

            if (Mathf.Abs(n.Dot(u)) <= accuracy)//droite et plan sont parallèles
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

            if (Mathf.Abs(n.Dot(u)) <= accuracy)//droite et plan sont parallèles
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
