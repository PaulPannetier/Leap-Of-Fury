using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class PhysicsToric
{
    #region Camera and general things

    public static Vector2 cameraSize = new Vector2(32f, 18f);
    private static Hitbox[] cameraHitboxArounds = new Hitbox[4]
    {
        new Hitbox(new Vector2(0f, cameraSize.y), cameraSize),//haut
        new Hitbox(new Vector2(0f, -cameraSize.y), cameraSize),//bas
        new Hitbox(new Vector3(cameraSize.x, 0f), cameraSize),//droite
        new Hitbox(new Vector2(-cameraSize.x, 0f), cameraSize) //gauche
    };
    public static readonly Hitbox cameraHitbox = new Hitbox(Vector2.zero, cameraSize);

    /// <param name="point"></param>
    /// <returns>Le point visible au coor donnée en param dans l'espace torique</returns>
    public static Vector2 GetPointInsideBounds(in Vector2 point)
    {
        return new Vector2(Useful.ClampModulo(-cameraSize.x * 0.5f, cameraSize.x * 0.5f, point.x),
            Useful.ClampModulo(-cameraSize.y * 0.5f, cameraSize.y * 0.5f, point.y));
    }

    public static float Distance(Vector2 p1, Vector2 p2)
    {
        p1 = GetPointInsideBounds(p1);
        p2 = GetPointInsideBounds(p2);

        float toricX = p1.x + (p2.x - p1.x).Sign() * cameraSize.x;
        float x = Mathf.Abs(p1.x - p2.x) < Mathf.Abs(toricX - p2.x) ? p1.x : toricX;
        float toricY = p1.y + (p2.y - p1.y).Sign() * cameraSize.y;
        float y = Mathf.Abs(p1.y - p2.y) < Mathf.Abs(toricY - p2.y) ? p1.y : toricY;

        return p2.Distance(new Vector2(x, y));
    }

    public static Vector2 Direction(Vector2 a, Vector2 b)
    {
        Vector2[] possibleDir = new Vector2[4]
        {
            b - a,
            new Vector2((b.x >= 0f ? -cameraSize.x : cameraSize.x) - a.x, b.y - a.y),
            new Vector2(b.x - a.x, (b.y >= 0f ? -cameraSize.y : cameraSize.y) - a.y),
            new Vector2((b.x >= 0f ? -cameraSize.x : cameraSize.x) - a.x, (b.y >= 0f ? -cameraSize.y : cameraSize.y) - a.y)
        };

        Vector2 minDir = possibleDir[0];
        float minSqrMag = possibleDir[0].sqrMagnitude;
        float mag;

        for (int i = 1; i < 4; i++)
        {
            mag = possibleDir[i].sqrMagnitude;
            if(mag < minSqrMag)
            {
                minSqrMag = mag;
                minDir = possibleDir[i];
            }
        }
        return minDir.normalized;
    }

    #endregion

    #region Overlap

    public static Collider2D OverlapPoint(in Vector2 point, LayerMask layerMask) => Physics2D.OverlapPoint(GetPointInsideBounds(point), layerMask);

    public static Collider2D[] OverlapPointAll(in Vector2 point, LayerMask layerMask) => Physics2D.OverlapPointAll(GetPointInsideBounds(point), layerMask);

    public static Collider2D OverlapCircle(Circle circle, in LayerMask layerMask) => OverlapCircle(circle.center, circle.radius, layerMask);
    
    public static Collider2D OverlapCircle(in Vector2 center, float radius, in LayerMask layerMask)
    {
        Circle circle = new Circle(GetPointInsideBounds(center), radius);

        bool containAll = true;
        bool[] collideWithCamHitbox = new bool[4];
        for(int i = 0; i < 4; i++)
        {
            Hitbox h = cameraHitboxArounds[i];
            if(CustomCollider.Collide(h, circle))
            {
                collideWithCamHitbox[i] = true;
                containAll = false;
            }
        }

        if(containAll)//ez case
        {
            return Physics2D.OverlapCircle(center, radius, layerMask);
        }

        Collider2D res = Physics2D.OverlapCircle(center, radius, layerMask);
        if (res != null)
            return res;

        for (int i = 0; i < 4; i++)
        {
            if (collideWithCamHitbox[i])
            {
                circle.MoveAt(circle.center - cameraHitboxArounds[i].center);
                res = Physics2D.OverlapCircle(center, radius, layerMask);
                if (res != null)
                    return res;
                circle.MoveAt(circle.center + cameraHitboxArounds[i].center);
            }
        }

        return null;
    }

    public static Collider2D[] OverlapCircleAll(Circle circle, in LayerMask layerMask) => OverlapCircleAll(circle.center, circle.radius, layerMask);

    public static Collider2D[] OverlapCircleAll(in Vector2 center, float radius, in LayerMask layerMask)
    {
        Circle circle = new Circle(GetPointInsideBounds(center), radius);

        bool containAll = true;
        bool[] collideWithCamHitbox = new bool[4];
        for (int i = 0; i < 4; i++)
        {
            Hitbox h = cameraHitboxArounds[i];
            if (CustomCollider.Collide(h, circle))
            {
                collideWithCamHitbox[i] = true;
                containAll = false;
            }
        }

        if (containAll)//ez case
        {
            return Physics2D.OverlapCircleAll(center, radius, layerMask);
        }

        Collider2D[] res = Physics2D.OverlapCircleAll(center, radius, layerMask);

        for (int i = 0; i < 4; i++)
        {
            if (collideWithCamHitbox[i])
            {
                circle.MoveAt(circle.center - cameraHitboxArounds[i].center);
                Collider2D[] res2 = Physics2D.OverlapCircleAll(center, radius, layerMask);
                if (res2 != null && res2.Length > 0)
                    res = res.Merge(res2);
                circle.MoveAt(circle.center + cameraHitboxArounds[i].center);
            }
        }

        return res == null ? null : res.Distinct().ToArray();
    }

    public static Collider2D OverlapBox(Hitbox hitbox, in LayerMask layerMask) => OverlapBox(hitbox.center, hitbox.size, hitbox.AngleHori(), layerMask);

    public static Collider2D OverlapBox(in Vector2 point, in Vector2 size, float angle, in LayerMask layerMask)
    {
        Hitbox hitbox = new Hitbox(GetPointInsideBounds(point), size);
        hitbox.Rotate(angle);
        bool containAll = true;
        foreach (Vector2 p in hitbox.rec.vertices)
        {
            if(!cameraHitbox.Contains(p))
            {
                containAll = false;
                break;
            }
        }

        if(containAll)
        {
            //cas simple
            return Physics2D.OverlapBox(hitbox.center, size, angle * Mathf.Rad2Deg, layerMask);
        }

        Debug.Log("!ez case");

        Collider2D res = Physics2D.OverlapBox(hitbox.center, size, angle * Mathf.Rad2Deg, layerMask);
        if (res != null)
            return res;

        bool[] contains = new bool[4];
        foreach (Vector2 p in hitbox.rec.vertices)
        {
            for (int i = 0; i < 4; i++)
            {
                if (!contains[i] && cameraHitboxArounds[i].Contains(p))
                    contains[i] = true;
            }
        }

        for (int i = 0; i < 4; i++)
        {
            if (contains[i])
            {
                hitbox.MoveAt(hitbox.center - cameraHitboxArounds[i].center);
                res = Physics2D.OverlapBox(hitbox.center, size, angle * Mathf.Rad2Deg, layerMask);
                if (res != null)
                    return res;
                hitbox.MoveAt(hitbox.center + cameraHitboxArounds[i].center);
            }
        }
        return res;
    }

    public static Collider2D[] OverlapBoxAll(Hitbox hitbox, in LayerMask layerMask) => OverlapBoxAll(hitbox.center, hitbox.size, hitbox.AngleHori(), layerMask);

    public static Collider2D[] OverlapBoxAll(in Vector2 point, in Vector2 size, in float angle, in LayerMask layerMask)
    {
        Hitbox hitbox = new Hitbox(GetPointInsideBounds(point), size);
        hitbox.Rotate(angle);

        bool containAll = true;
        foreach (Vector2 p in hitbox.rec.vertices)
        {
            if (!cameraHitbox.Contains(p))
            {
                containAll = false;
                break;
            }
        }

        if (containAll)
        {
            //cas simple
            return Physics2D.OverlapBoxAll(hitbox.center, size, angle * Mathf.Rad2Deg, layerMask);
        }

        Collider2D[] res = Physics2D.OverlapBoxAll(hitbox.center, size, angle * Mathf.Rad2Deg, layerMask);
        bool[] contains = new bool[4];
        foreach (Vector2 p in hitbox.rec.vertices)
        {
            for (int i = 0; i < 4; i++)
            {
                if (!contains[i] && cameraHitboxArounds[i].Contains(p))
                    contains[i] = true;
            }
        }

        for (int i = 0; i < 4; i++)
        {
            if (contains[i])
            {
                hitbox.MoveAt(hitbox.center - cameraHitboxArounds[i].center);
                Collider2D[] res2 = Physics2D.OverlapBoxAll(hitbox.center, size, angle * Mathf.Rad2Deg, layerMask);
                if (res2 != null && res2.Length > 0)
                    res = res.Merge(res2);
                hitbox.MoveAt(hitbox.center + cameraHitboxArounds[i].center);
            }
        }
        return res == null ? null : res.Distinct().ToArray();
    }

    private static Collider2D Physics2DOverlapCapsule(in Vector2 center, in Vector2 size, CapsuleDirection2D dir, float angle, LayerMask layerMask)
    {
        Capsule c = new Capsule(center, size, dir);
        if(Mathf.Abs(angle) > 1e-5)
            c.Rotate(angle);
        return Physics2DOverlapCapsule(c, layerMask);
    }

    private static Collider2D Physics2DOverlapCapsule(Capsule capsule, LayerMask layerMask)
    {
        Collider2D res = Physics2D.OverlapBox(capsule.hitbox.center, capsule.hitbox.size, capsule.AngleHori() * Mathf.Rad2Deg, layerMask);
        if (res != null)
            return res;
        res = Physics2D.OverlapCircle(capsule.c1.center, capsule.c1.radius, layerMask);
        if (res != null)
            return res;
        res = Physics2D.OverlapCircle(capsule.c2.center, capsule.c2.radius, layerMask);
        return res;
    }

    private static Collider2D[] Physics2DOverlapCapsuleAll(in Vector2 center, in Vector2 size, CapsuleDirection2D dir, float angle, LayerMask layerMask)
    {
        Capsule c = new Capsule(center, size, dir);
        if(Mathf.Abs(angle) > 1e-5)
            c.Rotate(angle);
        return Physic2DOverlapCapsuleAll(c, layerMask);
    }

    private static Collider2D[] Physic2DOverlapCapsuleAll(Capsule capsule, LayerMask layerMask)
    {
        Collider2D[] res = Physics2D.OverlapBoxAll(capsule.hitbox.center, capsule.hitbox.size, capsule.AngleHori() * Mathf.Rad2Deg, layerMask);
        res.Merge(Physics2D.OverlapCircleAll(capsule.c1.center, capsule.c1.radius, layerMask));
        res.Merge(Physics2D.OverlapCircleAll(capsule.c2.center, capsule.c2.radius, layerMask));
        return res == null ? null : res.Distinct().ToArray();
    }

    public static Collider2D OverlapCapsule(in Vector2 center, in Vector2 size, float angle, in LayerMask layerMask)
    {
        Capsule c = new Capsule(center, size);
        if (Mathf.Abs(angle) > 1e-5f)
            c.Rotate(angle);
        return OverlapCapsule(c, layerMask);
    }

    public static Collider2D OverlapCapsule(Capsule capsule, in LayerMask layerMask)
    {
        Capsule c = (Capsule)capsule.Clone();
        c.MoveAt(GetPointInsideBounds(capsule.center));

        bool containAll = true;
        bool[] collideWithCamHitbox = new bool[4];
        for (int i = 0; i < 4; i++)
        {
            Hitbox h = cameraHitboxArounds[i];
            if (CustomCollider.Collide(h, c))
            {
                collideWithCamHitbox[i] = true;
                containAll = false;
            }
        }

        float angle = c.AngleHori();
        if (containAll)//ez case
        {
            return Physics2DOverlapCapsule(c.center, c.hitbox.size, c.direction, angle, layerMask);
        }

        Collider2D res = Physics2DOverlapCapsule(c.center, c.hitbox.size, c.direction, angle, layerMask);
        if (res != null)
            return res;

        for (int i = 0; i < 4; i++)
        {
            if (collideWithCamHitbox[i])
            {
                c.MoveAt(c.center - cameraHitboxArounds[i].center);
                res = Physics2DOverlapCapsule(c.center, c.hitbox.size, c.direction, angle, layerMask);
                if (res != null)
                    return res;
                c.MoveAt(c.center + cameraHitboxArounds[i].center);
            }
        }

        return null;
    }

    public static Collider2D[] OverlapCapsuleAll(in Vector2 center, in Vector2 size, float angle, in LayerMask layerMask)
    {
        Capsule c = new Capsule(center, size);
        if (Mathf.Abs(angle) > 1e-5f)
            c.Rotate(angle);
        return OverlapCapsuleAll(c, layerMask);
    }

    public static Collider2D[] OverlapCapsuleAll(Capsule capsule, in LayerMask layerMask)
    {
        Capsule c = (Capsule)capsule.Clone();

        c.MoveAt(GetPointInsideBounds(capsule.center));

        bool containAll = true;
        bool[] collideWithCamHitbox = new bool[4];
        for (int i = 0; i < 4; i++)
        {
            Hitbox h = cameraHitboxArounds[i];
            if (CustomCollider.Collide(h, c))
            {
                collideWithCamHitbox[i] = true;
                containAll = false;
            }
        }

        float angle = c.AngleHori();
        if (containAll)//ez case
        {
            return Physics2DOverlapCapsuleAll(c.center, c.hitbox.size, c.direction, angle, layerMask);
        }

        Collider2D[] res = Physics2DOverlapCapsuleAll(c.center, c.hitbox.size, c.direction, angle, layerMask);
        for (int i = 0; i < 4; i++)
        {
            if (collideWithCamHitbox[i])
            {
                c.MoveAt(c.center - cameraHitboxArounds[i].center);
                Collider2D[] res2 =  Physics2DOverlapCapsuleAll(c.center, c.hitbox.size, c.direction, angle, layerMask);
                if (res2 != null && res2.Length > 0)
                    res = res.Merge(res2);
                c.MoveAt(c.center + cameraHitboxArounds[i].center);
            }
        }
        return res == null ? null : res.Distinct().ToArray();
    }

    #endregion

    #region Cast

    #region Raycast

    public static RaycastHit2D Raycast(in Vector2 from, in Vector2 direction, in float distance, in int layerMask)//OK
    {
        List<Vector2> _ = new List<Vector2>();
        float reachDistance = 0f;
        RaycastHit2D raycast = RaycastRecur(GetPointInsideBounds(from), direction.normalized, distance, layerMask, ref reachDistance, ref _);
        raycast.distance = raycast.collider != null ? reachDistance : 0f;
        return raycast;
    }

    public static RaycastHit2D Raycast(in Vector2 from, in Vector2 direction, in float distance, in int layerMask, out Vector2[] toricHitboxIntersectionsPoints)//ok
    {
        List<Vector2> points = new List<Vector2>();
        float reachDistance = 0f;
        RaycastHit2D raycast = RaycastRecur(GetPointInsideBounds(from), direction.normalized, distance, layerMask, ref reachDistance, ref points);
        raycast.distance = raycast.collider != null ? reachDistance : 0f;
        toricHitboxIntersectionsPoints = points.ToArray();
        return raycast;
    }

    private static RaycastHit2D RaycastRecur(in Vector2 from, in Vector2 direction, in float distance, in int layerMask, ref float reachDistance, ref List<Vector2> points)
    {
        RaycastHit2D raycast = Physics2D.Raycast(from, direction, distance, layerMask);
        Line ray;

        if (raycast.collider == null)
        {
            Vector2 B = from + direction * distance;
            if (cameraHitbox.Contains(B))
            {
                reachDistance += distance;
                return raycast;
            }
            else
            {
                ray = new Line(from, B);
                if (CustomCollider.CollideHitboxLine(cameraHitbox, ray, out Vector2 cp))
                {
                    float tmpDist = from.Distance(cp);
                    reachDistance += tmpDist;
                    Vector2 step = new Vector2(cp.x - cameraHitbox.center.x > 0f ? 0.01f : -0.01f, cp.y - cameraHitbox.center.y > 0f ? 0.01f : -0.01f);
                    while (cameraHitbox.Contains(cp))
                    {
                        cp += step;
                    }
                    points.Add(cp);
                    cp = GetPointInsideBounds(cp);
                    return RaycastRecur(cp, direction, distance - tmpDist, layerMask, ref reachDistance, ref points);
                }
                else
                {
                    Debug.LogWarning("Debug pls");
                    reachDistance += raycast.distance;
                    raycast.point = GetPointInsideBounds(B);
                    return raycast;
                }
            }
        }
        //raycast.collider != null
        if (cameraHitbox.Contains(raycast.point))
        {
            reachDistance += raycast.distance;
            return raycast;
        }
        ray = new Line(from, from + direction * distance);
        if (CustomCollider.CollideHitboxLine(cameraHitbox, ray, out Vector2 cp2))
        {
            float tmpDist = from.Distance(cp2);
            reachDistance += tmpDist;
            Vector2 step = new Vector2(cp2.x - cameraHitbox.center.x > 0f ? 0.01f : -0.01f, cp2.y - cameraHitbox.center.y > 0f ? 0.01f : -0.01f);
            while (cameraHitbox.Contains(cp2))
            {
                cp2 += step;
            }
            points.Add(cp2);
            cp2 = GetPointInsideBounds(cp2);
            return RaycastRecur(cp2, direction, distance - tmpDist, layerMask, ref reachDistance, ref points);
        }
        else
        {
            Debug.LogWarning("Debug pls");
            reachDistance += raycast.distance;
            raycast.point = GetPointInsideBounds(raycast.point);
            return raycast;
        }
    }

    #endregion

    #region CircleCast

    public static RaycastHit2D CircleCast(in Vector2 start, in Vector2 dir, float radius, float distance, LayerMask layerMask)
    {
        List<Vector2> inter = new List<Vector2>();
        RaycastHit2D[] raycasts = CircleCastRecur(start, dir.normalized, radius, distance, layerMask, ref inter, true).ToArray();
        FixCircleCastRaycastPoints(start, radius, ref raycasts, ref inter);
        return raycasts.Length >= 1 ? raycasts[0] : default;
    }

    public static RaycastHit2D CircleCast(in Vector2 start, in Vector2 dir, float radius, float distance, LayerMask layerMask, out Vector2[] torIntersections)
    {
        List<Vector2> inter = new List<Vector2>();
        RaycastHit2D[] raycasts = CircleCastRecur(start, dir.normalized, radius, distance, layerMask, ref inter, true).ToArray();
        torIntersections= inter.ToArray();
        FixCircleCastRaycastPoints(start, radius, ref raycasts, ref inter);
        return raycasts.Length >= 1 ? raycasts[0] : default;
    }

    public static RaycastHit2D[] CircleCastAll(in Vector2 start, in Vector2 dir, float radius, float distance, LayerMask layerMask)
    {
        List<Vector2> inter = new List<Vector2>();
        RaycastHit2D[] raycasts = CircleCastRecur(start, dir.normalized, radius, distance, layerMask, ref inter, false).ToArray();
        FixCircleCastRaycastPoints(start, radius, ref raycasts, ref inter);
        return raycasts;
    }

    public static RaycastHit2D[] CircleCastAll(in Vector2 start, in Vector2 dir, float radius, float distance, LayerMask layerMask, out Vector2[] torIntersections)
    {
        List<Vector2> inter = new List<Vector2>();
        RaycastHit2D[] raycasts = CircleCastRecur(start, dir.normalized, radius, distance, layerMask, ref inter, false).ToArray();
        torIntersections = inter.ToArray();
        FixCircleCastRaycastPoints(start, radius, ref raycasts, ref inter);
        return raycasts;
    }

    private static void FixCircleCastRaycastPoints(in Vector2 start, float radius, ref RaycastHit2D[] raycasts, ref List<Vector2> inter)
    {
        if (raycasts.Length > 0)
        {
            CustomCollider collider = CustomCollider.FromUnityCollider2D(raycasts[raycasts.Length - 1].collider);
            Circle circle = new Circle(raycasts.Last().point, radius);

            if (CustomCollider.Collide(collider, circle, out Vector2 collisionPoint))
            {
                raycasts[raycasts.Length - 1].distance = inter.Count > 0 ? collisionPoint.Distance(inter.Last()) : collisionPoint.Distance(start);
                raycasts[raycasts.Length - 1].point = collisionPoint;
            }
            else
            {
                Debug.LogWarning("Denug pls");
            }
        }
    }

    private static List<RaycastHit2D> CircleCastRecur(Vector2 start, in Vector2 dir, float radius, float distance, LayerMask layerMask, ref List<Vector2> inters, bool onlyOne)
    {
        start = GetPointInsideBounds(start);
        List<RaycastHit2D> raycasts = Physics2D.CircleCastAll(start, radius, dir, distance, layerMask).ToList();

        RemoveUnvaillableRaycastHit(ref raycasts);

        if (onlyOne && raycasts.Count > 0)
            return raycasts;

        Vector2 end = start + dir * distance;
        if (cameraHitbox.Contains(end))
        {
            Circle c = new Circle(end, radius);
            bool calculateEdge = false;
            if(CustomCollider.CollideCircleLine(c, cameraHitbox.size * 0.5f, 0.5f * new Vector2(cameraHitbox.size.x, -cameraHitbox.size.y)) ||
                CustomCollider.CollideCircleLine(c, cameraHitbox.size * (-0.5f), 0.5f * new Vector2(-cameraHitbox.size.x, cameraHitbox.size.y)))
            {
                end += Vector2.right * (end.x >= 0f ? -cameraSize.x : cameraSize.x);
                calculateEdge = true;
            }

            if (CustomCollider.CollideCircleLine(c, cameraHitbox.size * 0.5f, 0.5f * new Vector2(-cameraHitbox.size.x, cameraHitbox.size.y)) ||
                CustomCollider.CollideCircleLine(c, cameraHitbox.size * (-0.5f), 0.5f * new Vector2(cameraHitbox.size.x, -cameraHitbox.size.y)))
            {
                end += Vector2.up * (end.y >= 0f ? -cameraSize.y : cameraSize.y);
                calculateEdge = true;
            }
            if (calculateEdge)
                return raycasts.Merge(Physics2D.CircleCastAll(end, radius, dir, 0.01f, layerMask).ToList());
            return raycasts;
        }
        else
        {
            if (CustomCollider.CollideHitboxLine(cameraHitbox, start, end, out Vector2 col))
            {
                inters.Add(col);
                float newDist = distance - start.Distance(col);
                while (cameraHitbox.Contains(col))
                    col += dir * 0.01f;
                col = GetPointInsideBounds(col);
                return raycasts.Merge(CircleCastRecur(col, dir, radius, newDist, layerMask, ref inters, onlyOne));
            }
            else
            {
                Debug.LogWarning("must be collision");
                LogManager.instance.WriteLog("must be collision between cameraHitbox and the line in PhysicToric.CircleCastRecur()", cameraHitbox, new Line(start, end));
                return raycasts;
            }
        }

        void RemoveUnvaillableRaycastHit(ref List<RaycastHit2D> raycasts)
        {
            for (int i = raycasts.Count - 1; i >= 0; i--)
            {
                if (raycasts[i].collider == null || !cameraHitbox.Contains(raycasts[i].point))
                {
                    raycasts.RemoveAt(i);
                }
            }
        }
    }

    #endregion

    #endregion
}
