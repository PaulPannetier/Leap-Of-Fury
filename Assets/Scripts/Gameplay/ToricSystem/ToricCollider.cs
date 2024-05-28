using Collision2D;
using Collider2D = Collision2D.Collider2D;
using UnityEngine;
using System.Collections.Generic;

namespace ToricCollider2D
{
    public class ToricCollider
    {
        #region Toric Params

        internal static Hitbox mapHitbox;
        internal static Line2D[] mapSides;
        internal static Vector2[] mapOffset;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Start()
        {
            EventManager.instance.callbackOnMapChanged += OnMapChange;
        }

        private static void OnMapChange(LevelMapData mapData)
        {
            Vector2 size = mapData.mapSize * mapData.cellSize;
            Vector2 halfSize = size * 0.5f;
            mapHitbox = new Hitbox(Vector2.zero, size);
            mapSides = new Line2D[4]
            {
            new Line2D(new Vector2(-halfSize.x, halfSize.y), halfSize),
            new Line2D(-halfSize, new Vector2(halfSize.x, -halfSize.y)),
            new Line2D(new Vector2(-halfSize.x, halfSize.y), -halfSize),
            new Line2D(halfSize, new Vector2(halfSize.x, -halfSize.y))
            };
            mapOffset = new Vector2[4]
            {
            new Vector2(0f, size.y),
            new Vector2(0f, -size.y),
            new Vector2(-size.x, 0f),
            new Vector2(size.x, 0f)
            };
        }

        #endregion

        public static ToricCollider FromUnityCollider2D(UnityEngine.Collider2D collider)
        {
            if (collider is BoxCollider2D hitbox)
                return new ToricHitbox(hitbox);
            else if (collider is CircleCollider2D circle)
                return new ToricCircle(circle);
            else if (collider is CapsuleCollider2D capsule)
                return new ToricCapsule(capsule);
            else if (collider is PolygonCollider2D poly)
                return new ToricPolygone(poly);

            Debug.LogWarning($"Cant convert a unity collider2D of type {collider.GetType()}!");
            LogManager.instance.AddLog($"Cant convert a unity collider2D of type {collider.GetType()}!", collider);
            return null;
        }

        public virtual Vector2 center
        {
            get => new Vector2();
            protected set { }
        }

        protected ToricCollider() { }

        protected ToricCollider(UnityEngine.Collider2D collider) { }

        public virtual ToricCollider Clone() => null;
        public virtual bool Collide(Collider2D collider) => false;
        public virtual bool CollideLine(Line2D line) => false;
        public virtual bool CollideStraightLine(StraightLine2D d) => false;
        public virtual bool Contains(in Vector2 p) => false;
        public virtual float Distance(in Vector2 point) => 0f;
        public virtual Vector2 ClosestPoint(in Vector2 point) => default;
        public virtual void MoveAt(in Vector2 position) { }
        public virtual void Rotate(float angle) { }
        public virtual Hitbox ToHitbox() => null;
        public virtual void Scale(in Vector2 scale) { }
        public virtual bool Normal(in Vector2 point, out Vector2 normal) { normal = Vector2.zero; return false; }

        #region Internal

        internal bool CollideInternal(Collider2D fixeCol, Collider2D toricCol)
        {
            if (Collider2D.Collide(fixeCol, toricCol))
                return true;

            bool collide = false;
            for (int i = 0; i < 4; i++)
            {
                if (Collider2D.CollideLine(toricCol, mapSides[i]))
                {
                    toricCol.MoveAt(toricCol.center - mapOffset[i]);
                    collide = Collider2D.Collide(fixeCol, toricCol);
                    if (collide)
                        return true;
                    toricCol.MoveAt(toricCol.center + mapOffset[i]);
                }
            }
            return false;
        }

        internal bool CollideLineInternal(Collider2D collider, Line2D line)
        {
            if (Collider2D.CollideLine(collider, line))
                return true;

            bool collide = false;
            for (int i = 0; i < 4; i++)
            {
                if (Collider2D.CollideLine(collider, mapSides[i]))
                {
                    collider.MoveAt(collider.center - mapOffset[i]);
                    collide = Collider2D.CollideLine(collider, line);
                    if (collide)
                        return true;
                    collider.MoveAt(collider.center + mapOffset[i]);
                }
            }
            return false;
        }

        internal bool CollideStraightLineInternal(Collider2D collider, StraightLine2D straightline)
        {
            if (Collider2D.CollideStraightLine(collider, straightline))
                return true;

            bool collide = false;
            for (int i = 0; i < 4; i++)
            {
                if (Collider2D.CollideLine(collider, mapSides[i]))
                {
                    collider.MoveAt(collider.center - mapOffset[i]);
                    collide = Collider2D.CollideStraightLine(collider, straightline);
                    if (collide)
                        return true;
                    collider.MoveAt(collider.center + mapOffset[i]);
                }
            }
            return false;
        }

        internal bool ContainsInternal(Collider2D collider, in Vector2 point)
        {
            if (collider.Contains(point))
                return true;

            bool contains = false;
            for (int i = 0; i < 4; i++)
            {
                if (Collider2D.CollideLine(collider, mapSides[i]))
                {
                    collider.MoveAt(collider.center - mapOffset[i]);
                    contains = collider.Contains(point);
                    if (contains)
                        return true;
                    collider.MoveAt(collider.center + mapOffset[i]);
                }
            }
            return false;
        }

        internal float DistanceInternal(Collider2D collider, in Vector2 point)
        {
            float bestDistance = collider.Distance(point);
            float currentDist;
            for (int i = 0; i < 4; i++)
            {
                if (Collider2D.CollideLine(collider, mapSides[i]))
                {
                    collider.MoveAt(collider.center - mapOffset[i]);
                    currentDist = collider.Distance(point);
                    if (currentDist < bestDistance)
                    {
                        bestDistance = currentDist;
                    }
                    collider.MoveAt(collider.center + mapOffset[i]);
                }
            }
            return bestDistance;
        }

        internal Vector2 ClosestPointInternall(Collider2D collider, in Vector2 point)
        {
            return PhysicsToric.GetPointInsideBounds(collider.ClosestPoint(point));
        }

        internal bool NormalInternal(Collider2D collider, in Vector2 point, out Vector2 normal)
        {
            if(collider.Normal(point, out normal))
                return true;

            for (int i = 0; i < 4; i++)
            {
                if (Collider2D.CollideLine(collider, mapSides[i]))
                {
                    collider.MoveAt(collider.center - mapOffset[i]);
                    if (collider.Normal(point, out normal))
                        return true;
                    collider.MoveAt(collider.center + mapOffset[i]);
                }
            }

            normal = Vector2.zero;
            return false;
        }

        #endregion
    }

    #region Circle

    public class ToricCircle : ToricCollider
    {
        private Circle circle;
        public override Vector2 center
        {
            get => circle.center;
            protected set
            {
                circle.MoveAt(value);
            }
        }

        public ToricCircle(in Vector2 center, float radius) : base()
        {
            circle = new Circle(center, radius);
        }

        public ToricCircle(CircleCollider2D unityCircle) : base(unityCircle)
        {
            circle = new Circle(unityCircle);
        }

        public override bool Collide(Collider2D collider) => CollideInternal(collider, circle);

        public override bool CollideLine(Line2D line) => CollideLineInternal(circle, line);

        public override bool CollideStraightLine(StraightLine2D straightLine) => CollideStraightLineInternal(circle, straightLine);

        public override bool Contains(in Vector2 p) => ContainsInternal(circle, p);

        public override float Distance(in Vector2 point) => DistanceInternal(circle, point);

        public override Vector2 ClosestPoint(in Vector2 point) => ClosestPointInternall(circle, point);

        public override void MoveAt(in Vector2 position) => circle.MoveAt(position);

        public override void Rotate(float angle) { }

        public override Hitbox ToHitbox() => circle.ToHitbox();

        public override void Scale(in Vector2 scale) => circle.Scale(scale);

        public override bool Normal(in Vector2 point, out Vector2 normal) => NormalInternal(circle, point, out normal);

        public override ToricCollider Clone() => new ToricCircle(circle.center, circle.radius);

        public override string ToString() => circle.ToString();
    }

    #endregion

    #region Polygone

    public class ToricPolygone : ToricCollider
    {
        private Polygone polygone;
        public override Vector2 center
        {
            get => polygone.center;
            protected set
            {
                polygone.MoveAt(value);
            }
        }

        public ToricPolygone(Vector2[] vertices) : base()
        {
            polygone = new Polygone(vertices);
        }

        public ToricPolygone(List<Vector2> vertices) : base()
        {
            polygone = new Polygone(vertices);
        }

        public ToricPolygone(Vector2[] vertices, in Vector2 center) : base()
        {
            polygone = new Polygone(vertices, center);
        }

        public ToricPolygone(List<Vector2> vertices, in Vector2 center) : base()
        {
            polygone = new Polygone(vertices, center);
        }

        public ToricPolygone(PolygonCollider2D polygone) : base(polygone)
        {
            this.polygone = new Polygone(polygone);
        }

        public override bool Collide(Collider2D collider) => CollideInternal(polygone, collider);

        public override bool CollideLine(Line2D line) => CollideLineInternal(polygone, line);

        public override bool CollideStraightLine(StraightLine2D straightLine) => CollideStraightLineInternal(polygone, straightLine);

        public override bool Contains(in Vector2 p) => ContainsInternal(polygone, p);

        public override float Distance(in Vector2 point) => DistanceInternal(polygone, point);

        public override Vector2 ClosestPoint(in Vector2 point) => ClosestPointInternall(polygone, point);

        public override void MoveAt(in Vector2 position) => polygone.MoveAt(position);

        public override void Rotate(float angle) => polygone.Rotate(angle);

        public override Hitbox ToHitbox() => polygone.ToHitbox();

        public override void Scale(in Vector2 scale) => polygone.Scale(scale);

        public override bool Normal(in Vector2 point, out Vector2 normal) => NormalInternal(polygone, point, out normal);

        public override ToricCollider Clone() => new ToricPolygone(polygone.vertices, polygone.center);

        public override string ToString() => polygone.ToString();
    }

    #endregion

    #region Hitbox

    public class ToricHitbox : ToricCollider
    {
        private Hitbox hitbox;
        public override Vector2 center
        {
            get => hitbox.center;
            protected set
            {
                hitbox.MoveAt(value);
            }
        }

        public ToricHitbox(in Vector2 center, in Vector2 size) : base()
        {
            hitbox = new Hitbox(center, size);
        }

        public ToricHitbox(BoxCollider2D hitbox) : base(hitbox)
        {
            this.hitbox = new Hitbox(hitbox);
        }

        public override bool Collide(Collider2D collider) => CollideInternal(hitbox, collider);

        public override bool CollideLine(Line2D line) => CollideLineInternal(hitbox, line);

        public override bool CollideStraightLine(StraightLine2D straightLine) => CollideStraightLineInternal(hitbox, straightLine);

        public override bool Contains(in Vector2 p) => ContainsInternal(hitbox, p);

        public override float Distance(in Vector2 point) => DistanceInternal(hitbox, point);

        public override Vector2 ClosestPoint(in Vector2 point) => ClosestPointInternall(hitbox, point);

        public override void MoveAt(in Vector2 position) => hitbox.MoveAt(position);

        public override void Rotate(float angle) => hitbox.Rotate(angle);

        public override Hitbox ToHitbox() => hitbox.ToHitbox();

        public override void Scale(in Vector2 scale) => hitbox.Scale(scale);

        public override bool Normal(in Vector2 point, out Vector2 normal) => NormalInternal(hitbox, point, out normal);

        public override ToricCollider Clone() => new ToricHitbox(hitbox.center, hitbox.size);

        public override string ToString() => hitbox.ToString();
    }

    #endregion

    #region Capsule

    public class ToricCapsule : ToricCollider
    {
        private Capsule capsule;
        public override Vector2 center
        {
            get => capsule.center;
            protected set
            {
                capsule.MoveAt(value);
            }
        }

        public ToricCapsule(in Vector2 center, in Vector2 size) : base()
        {
            capsule = new Capsule(center, size);
        }

        public ToricCapsule(in Vector2 center, in Vector2 size, float angle) : base()
        {
            capsule = new Capsule(center, size, angle);
        }

        public ToricCapsule(in Vector2 center, in Vector2 size, CapsuleDirection2D direction) : base()
        {
            capsule = new Capsule(center, size, direction);
        }

        public ToricCapsule(in Vector2 center, in Vector2 size, CapsuleDirection2D direction, float angle) : base()
        {
            capsule = new Capsule(center, size, direction, angle);
        }

        public ToricCapsule(CapsuleCollider2D capsule) : base(capsule)
        {
            this.capsule = new Capsule(capsule);
        }

        private ToricCapsule(Capsule capsule)
        {
            this.capsule = capsule;
        }

        public override bool Collide(Collider2D collider) => CollideInternal(capsule, collider);

        public override bool CollideLine(Line2D line) => CollideLineInternal(capsule, line);

        public override bool CollideStraightLine(StraightLine2D straightLine) => CollideStraightLineInternal(capsule, straightLine);

        public override bool Contains(in Vector2 p) => ContainsInternal(capsule, p);

        public override float Distance(in Vector2 point) => DistanceInternal(capsule, point);

        public override Vector2 ClosestPoint(in Vector2 point) => ClosestPointInternall(capsule, point);

        public override void MoveAt(in Vector2 position) => capsule.MoveAt(position);

        public override void Rotate(float angle) => capsule.Rotate(angle);

        public override Hitbox ToHitbox() => capsule.ToHitbox();

        public override void Scale(in Vector2 scale) => capsule.Scale(scale);

        public override bool Normal(in Vector2 point, out Vector2 normal) => NormalInternal(capsule, point, out normal);

        public override ToricCollider Clone() => new ToricCapsule((Capsule)capsule.Clone());

        public override string ToString() => capsule.ToString();
    }

    #endregion
}