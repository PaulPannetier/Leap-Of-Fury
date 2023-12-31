using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Collision2D;
using Collider2D = Collision2D.Collider2D;

public static class PhysicsToric
{
    #region Camera and general things

    private static Vector2 mapSize;
    private static Hitbox[] mapHitboxArounds;
    private static Hitbox mapHitbox;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Start()
    {
        LevelMapData.onMapChange += OnMapChange;
    }

    private static void OnMapChange(LevelMapData mapData)
    {
        mapSize = mapData.mapSize;
        mapHitboxArounds = new Hitbox[4]
        {
            new Hitbox(new Vector2(0f, mapSize.y), mapSize),//haut
            new Hitbox(new Vector2(0f, -mapSize.y), mapSize),//bas
            new Hitbox(new Vector3(mapSize.x, 0f), mapSize),//droite
            new Hitbox(new Vector2(-mapSize.x, 0f), mapSize) //gauche
        };
        mapHitbox = new Hitbox(Vector2.zero, LevelMapData.currentMap.mapSize);
    }

    /// <param name="point"></param>
    /// <returns>Le point visible au coor donnée en param dans l'espace torique</returns>
    public static Vector2 GetPointInsideBounds(in Vector2 point)
    {
        return new Vector2(Useful.ClampModulo(-mapSize.x * 0.5f, mapSize.x * 0.5f, point.x),
            Useful.ClampModulo(-mapSize.y * 0.5f, mapSize.y * 0.5f, point.y));
    }

    public static Vector2 GetComplementaryPoint(Vector2 point)
    {
        Vector2 step = new Vector2(point.x - mapHitbox.center.x > 0f ? 0.01f : -0.01f, point.y - mapHitbox.center.y > 0f ? 0.01f : -0.01f);
        int i = 0;
        while (mapHitbox.Contains(point))
        {
            point += step;
            i++;
        }
        point += step;
        i++;
        return GetPointInsideBounds(point) + i * step;
    }

    public static float Distance(Vector2 p1, Vector2 p2)
    {
        p1 = GetPointInsideBounds(p1);
        p2 = GetPointInsideBounds(p2);

        float toricX = p1.x + (p2.x - p1.x).Sign() * mapSize.x;
        float x = Mathf.Abs(p1.x - p2.x) < Mathf.Abs(toricX - p2.x) ? p1.x : toricX;
        float toricY = p1.y + (p2.y - p1.y).Sign() * mapSize.y;
        float y = Mathf.Abs(p1.y - p2.y) < Mathf.Abs(toricY - p2.y) ? p1.y : toricY;

        return p2.Distance(new Vector2(x, y));
    }

    public static Vector2 Direction(Vector2 a, Vector2 b)
    {
        Vector2[] possibleA = new Vector2[5]
        {
            a,
            new Vector2(a.x + mapSize.x , a.y),
            new Vector2(a.x - mapSize.x, a.y),
            new Vector2(a.x, a.y + mapSize.y),
            new Vector2(a.x, a.y - mapSize.y)
        };

        Vector2[] possibleB = new Vector2[5]
        {
            b,
            new Vector2(b.x + mapSize.x , b.y),
            new Vector2(b.x - mapSize.x, b.y),
            new Vector2(b.x, b.y + mapSize.y),
            new Vector2(b.x, b.y - mapSize.y)
        };

        Vector2 aKeep = possibleA[0], bKeep = possibleB[0];
        float minSqrMag = float.MaxValue;
        float sqrMag;
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                sqrMag = possibleA[i].SqrDistance(possibleB[j]);
                if (sqrMag < minSqrMag)
                {
                    minSqrMag = sqrMag;
                    aKeep = possibleA[i];
                    bKeep = possibleB[j];
                }
            }
        }
        return (bKeep - aKeep) *  (1f / Mathf.Sqrt(minSqrMag));
    }

    public static bool GetToricIntersection(in Vector2 a, in Vector2 b, out Vector2 toricInter)
    {
        float dist = Distance(a, b);
        Vector2 dir = Direction(a, b);
        Line2D line = new Line2D(a, a + dir * dist);

        for (int i = 0; i < 4; i++)
        {
            if (Collider2D.CollideHitboxLine(mapHitboxArounds[i], line, out toricInter))
            {
                return true;
            }
        }
        toricInter = Vector2.zero;
        return false;
    }

    #endregion

    #region Overlap

    public static UnityEngine.Collider2D OverlapPoint(in Vector2 point, LayerMask layerMask) => Physics2D.OverlapPoint(GetPointInsideBounds(point), layerMask);

    public static UnityEngine.Collider2D[] OverlapPointAll(in Vector2 point, LayerMask layerMask) => Physics2D.OverlapPointAll(GetPointInsideBounds(point), layerMask);

    public static UnityEngine.Collider2D OverlapCircle(Circle circle, in LayerMask layerMask) => OverlapCircle(circle.center, circle.radius, layerMask);
    
    public static UnityEngine.Collider2D OverlapCircle(in Vector2 center, float radius, in LayerMask layerMask)
    {
        Circle circle = new Circle(GetPointInsideBounds(center), radius);

        UnityEngine.Collider2D res = Physics2D.OverlapCircle(center, radius, layerMask);
        if (res != null)
            return res;

        bool[] collideWithCamHitbox = new bool[4];
        for(int i = 0; i < 4; i++)
        {
            Hitbox h = mapHitboxArounds[i];
            if(Collider2D.Collide(h, circle))
            {
                collideWithCamHitbox[i] = true;
            }
        }

        for (int i = 0; i < 4; i++)
        {
            if (collideWithCamHitbox[i])
            {
                circle.MoveAt(circle.center - mapHitboxArounds[i].center);
                res = Physics2D.OverlapCircle(center, radius, layerMask);
                if (res != null)
                    return res;
                circle.MoveAt(circle.center + mapHitboxArounds[i].center);
            }
        }

        return null;
    }

    public static UnityEngine.Collider2D[] OverlapCircleAll(Circle circle, in LayerMask layerMask) => OverlapCircleAll(circle.center, circle.radius, layerMask);

    public static UnityEngine.Collider2D[] OverlapCircleAll(in Vector2 center, float radius, in LayerMask layerMask)
    {
        Circle circle = new Circle(GetPointInsideBounds(center), radius);
        UnityEngine.Collider2D[] res = Physics2D.OverlapCircleAll(center, radius, layerMask);

        bool[] collideWithCamHitbox = new bool[4];
        for (int i = 0; i < 4; i++)
        {
            Hitbox h = mapHitboxArounds[i];
            if (Collider2D.Collide(h, circle))
            {
                collideWithCamHitbox[i] = true;
            }
        }

        for (int i = 0; i < 4; i++)
        {
            if (collideWithCamHitbox[i])
            {
                circle.MoveAt(circle.center - mapHitboxArounds[i].center);
                UnityEngine.Collider2D[] res2 = Physics2D.OverlapCircleAll(center, radius, layerMask);
                if (res2 != null && res2.Length > 0)
                    res = res.Merge(res2);
                circle.MoveAt(circle.center + mapHitboxArounds[i].center);
            }
        }

        return res.Length <= 0 ? res : res.Distinct().ToArray();
    }

    public static UnityEngine.Collider2D OverlapBox(Hitbox hitbox, in LayerMask layerMask) => OverlapBox(hitbox.center, hitbox.size, hitbox.AngleHori(), layerMask);

    public static UnityEngine.Collider2D OverlapBox(in Vector2 point, in Vector2 size, float angle, in LayerMask layerMask)
    {
        Hitbox hitbox = new Hitbox(GetPointInsideBounds(point), size);
        hitbox.Rotate(angle);

        UnityEngine.Collider2D res = Physics2D.OverlapBox(hitbox.center, size, angle * Mathf.Rad2Deg, layerMask);
        if (res != null)
            return res;

        bool[] contains = new bool[4];
        foreach (Vector2 p in hitbox.vertices)
        {
            for (int i = 0; i < 4; i++)
            {
                if (!contains[i] && mapHitboxArounds[i].Contains(p))
                    contains[i] = true;
            }
        }

        for (int i = 0; i < 4; i++)
        {
            if (contains[i])
            {
                hitbox.MoveAt(hitbox.center - mapHitboxArounds[i].center);
                res = Physics2D.OverlapBox(hitbox.center, size, angle * Mathf.Rad2Deg, layerMask);
                if (res != null)
                    return res;
                hitbox.MoveAt(hitbox.center + mapHitboxArounds[i].center);
            }
        }
        return res;
    }

    public static UnityEngine.Collider2D[] OverlapBoxAll(Hitbox hitbox, LayerMask layerMask) => OverlapBoxAll(hitbox.center, hitbox.size, hitbox.AngleHori(), layerMask);

    public static UnityEngine.Collider2D[] OverlapBoxAll(in Vector2 point, in Vector2 size, float angle, LayerMask layerMask)
    {
        Hitbox hitbox = new Hitbox(GetPointInsideBounds(point), size);
        hitbox.Rotate(angle);

        UnityEngine.Collider2D[] res = Physics2D.OverlapBoxAll(hitbox.center, size, angle * Mathf.Rad2Deg, layerMask);
        bool[] contains = new bool[4];
        foreach (Vector2 p in hitbox.vertices)
        {
            for (int i = 0; i < 4; i++)
            {
                if (mapHitboxArounds[i].Contains(p))
                    contains[i] = true;
            }
        }

        for (int i = 0; i < 4; i++)
        {
            if (contains[i])
            {
                hitbox.MoveAt(hitbox.center - mapHitboxArounds[i].center);
                UnityEngine.Collider2D[] res2 = Physics2D.OverlapBoxAll(hitbox.center, size, angle * Mathf.Rad2Deg, layerMask);
                if (res2 != null && res2.Length > 0)
                    res = res.Merge(res2);
                hitbox.MoveAt(hitbox.center + mapHitboxArounds[i].center);
            }
        }
        return res == null ? null : res.Distinct().ToArray();
    }

    public static UnityEngine.Collider2D Physics2DOverlapCapsule(in Vector2 center, in Vector2 size, CapsuleDirection2D dir, float angle, LayerMask layerMask)
    {
        Capsule c = new Capsule(center, size, dir);
        if(Mathf.Abs(angle) > 1e-5f)
            c.Rotate(angle);
        return Physics2DOverlapCapsule(c, layerMask);
    }

    public static UnityEngine.Collider2D Physics2DOverlapCapsule(Capsule capsule, LayerMask layerMask)
    {
        UnityEngine.Collider2D res = Physics2D.OverlapBox(capsule.hitbox.center, capsule.hitbox.size, capsule.AngleHori() * Mathf.Rad2Deg, layerMask);
        if (res != null)
            return res;
        res = Physics2D.OverlapCircle(capsule.circle1.center, capsule.circle1.radius, layerMask);
        if (res != null)
            return res;
        res = Physics2D.OverlapCircle(capsule.circle2.center, capsule.circle2.radius, layerMask);
        return res;
    }

    public static UnityEngine.Collider2D[] Physics2DOverlapCapsuleAll(in Vector2 center, in Vector2 size, CapsuleDirection2D dir, float angle, LayerMask layerMask)
    {
        Capsule c = new Capsule(center, size, dir);
        if(Mathf.Abs(angle) > 1e-5)
            c.Rotate(angle);
        return Physics2DOverlapCapsuleAll(c, layerMask);
    }

    public static UnityEngine.Collider2D[] Physics2DOverlapCapsuleAll(Capsule capsule, LayerMask layerMask)
    {
        UnityEngine.Collider2D[] res = Physics2D.OverlapBoxAll(capsule.hitbox.center, capsule.hitbox.size, capsule.AngleHori() * Mathf.Rad2Deg, layerMask);
        res = res.Merge(Physics2D.OverlapCircleAll(capsule.circle1.center, capsule.circle1.radius, layerMask));
        res = res.Merge(Physics2D.OverlapCircleAll(capsule.circle2.center, capsule.circle2.radius, layerMask));
        return res.Distinct().ToArray();
    }

    public static UnityEngine.Collider2D OverlapCapsule(in Vector2 center, in Vector2 size, float angle, in LayerMask layerMask)
    {
        Capsule c = new Capsule(center, size);
        if (Mathf.Abs(angle) > 1e-5f)
            c.Rotate(angle);
        return OverlapCapsule(c, layerMask);
    }

    public static UnityEngine.Collider2D OverlapCapsule(Capsule capsule, in LayerMask layerMask)
    {
        UnityEngine.Collider2D res = OverlapBox(capsule.hitbox, layerMask);
        if(res != null)
            return res;
        res = OverlapCircle(capsule.circle1, layerMask);
        if (res != null)
            return res;
        return OverlapCircle(capsule.circle2, layerMask);
    }

    public static UnityEngine.Collider2D[] OverlapCapsuleAll(in Vector2 center, in Vector2 size, float angle, in LayerMask layerMask)
    {
        Capsule c = new Capsule(center, size);
        if (Mathf.Abs(angle) > 1e-5f)
            c.Rotate(angle);
        return OverlapCapsuleAll(c, layerMask);
    }

    public static UnityEngine.Collider2D[] OverlapCapsuleAll(Capsule capsule, in LayerMask layerMask)
    {
        UnityEngine.Collider2D[] res = OverlapBoxAll(capsule.hitbox, layerMask);
        res = res.Merge(OverlapCircleAll(capsule.circle1, layerMask));
        return res.Merge(OverlapCircleAll(capsule.circle2, layerMask));
    }

    #endregion

    #region Cast

    #region Raycast

    public static RaycastHit2D Raycast(in Vector2 from, in Vector2 direction, float distance, int layerMask)//OK
    {
        List<Vector2> _ = new List<Vector2>();
        float reachDistance = 0f;
        RaycastHit2D raycast = RaycastRecur(GetPointInsideBounds(from), direction.normalized, distance, layerMask, ref reachDistance, ref _);
        raycast.distance = raycast.collider != null ? reachDistance : 0f;
        return raycast;
    }

    public static RaycastHit2D Raycast(in Vector2 from, in Vector2 direction, float distance, int layerMask, out Vector2[] toricHitboxIntersectionsPoints)//ok
    {
        List<Vector2> points = new List<Vector2>();
        float reachDistance = 0f;
        RaycastHit2D raycast = RaycastRecur(GetPointInsideBounds(from), direction.normalized, distance, layerMask, ref reachDistance, ref points);
        raycast.distance = raycast.collider != null ? reachDistance : 0f;
        toricHitboxIntersectionsPoints = points.ToArray();
        return raycast;
    }

    private static RaycastHit2D RaycastRecur(in Vector2 from, in Vector2 direction, float distance, int layerMask, ref float reachDistance, ref List<Vector2> points)
    {
        RaycastHit2D raycast = Physics2D.Raycast(from, direction, distance, layerMask);
        Line2D ray;

        if (raycast.collider == null)
        {
            Vector2 B = from + direction * distance;
            if (mapHitbox.Contains(B))
            {
                reachDistance += distance;
                return raycast;
            }
            else
            {
                ray = new Line2D(from, B);
                if (Collider2D.CollideHitboxLine(mapHitbox, ray, out Vector2 cp))
                {
                    float tmpDist = from.Distance(cp);
                    reachDistance += tmpDist;
                    Vector2 step = new Vector2(cp.x - mapHitbox.center.x > 0f ? 0.01f : -0.01f, cp.y - mapHitbox.center.y > 0f ? 0.01f : -0.01f);
                    while (mapHitbox.Contains(cp))
                    {
                        cp += step;
                    }
                    points.Add(cp);
                    cp += step;
                    cp = GetPointInsideBounds(cp);
                    cp += step;
                    return RaycastRecur(cp, direction, distance - tmpDist, layerMask, ref reachDistance, ref points);
                }
                else
                {
                    reachDistance += raycast.distance;
                    raycast.point = GetPointInsideBounds(B);
                    return raycast;
                }
            }
        }
        //raycast.collider != null
        if (mapHitbox.Contains(raycast.point))
        {
            reachDistance += raycast.distance;
            return raycast;
        }
        ray = new Line2D(from, from + direction * distance);
        if (Collider2D.CollideHitboxLine(mapHitbox, ray, out Vector2 cp2))
        {
            float tmpDist = from.Distance(cp2);
            reachDistance += tmpDist;
            Vector2 step = new Vector2(cp2.x - mapHitbox.center.x > 0f ? 0.01f : -0.01f, cp2.y - mapHitbox.center.y > 0f ? 0.01f : -0.01f);
            while (mapHitbox.Contains(cp2))
            {
                cp2 += step;
            }
            points.Add(cp2);
            cp2 += step;
            cp2 = GetPointInsideBounds(cp2);
            cp2 += step;
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

    public static RaycastHit2D[] RaycastAll(Vector2 from, in Vector2 dir, float distance, LayerMask layerMask)
    {
        from = GetPointInsideBounds(from);
        List<List<Vector2>> interPoints = new List<List<Vector2>>();
        List<Vector2> globalInterPoints = new List<Vector2>();
        RaycastHit2D[] raycasts = RaycastAllRecur(from, dir.normalized, distance, layerMask, 0f, 0, ref globalInterPoints, ref interPoints);

        return raycasts;
    }

    public static RaycastHit2D[] RaycastAll(Vector2 from, in Vector2 dir, float distance, LayerMask layerMask, out Vector2[][] toricHitboxIntersectionsPoints)
    {
        from = GetPointInsideBounds(from);
        List<List<Vector2>> interPoints = new List<List<Vector2>>();
        List<Vector2> globalInterPoints = new List<Vector2>();
        RaycastHit2D[] raycasts = RaycastAllRecur(from, dir.normalized, distance, layerMask, 0f, 0, ref globalInterPoints, ref interPoints);

        toricHitboxIntersectionsPoints = new Vector2[interPoints.Count][];
        for (int i = 0; i < toricHitboxIntersectionsPoints.Length; i++)
        {
            toricHitboxIntersectionsPoints[i] = new Vector2[interPoints[i].Count];
            for (int j = 0; j < toricHitboxIntersectionsPoints[i].Length; j++)
            {
                toricHitboxIntersectionsPoints[i][j] = interPoints[i][j];
            }
        }

        return raycasts;
    }

    private static RaycastHit2D[] RaycastAllRecur(in Vector2 from, in Vector2 direction, float distance, int layerMask, float reachDistance, int raycastIndex,ref List<Vector2> globalInterPoints, ref List<List<Vector2>> interPoints)
    {
        Line2D line = new Line2D(from, from + direction * distance);
        bool collide = false;
        Vector2 inter = Vector2.zero;
        float minInterSqrDist = float.MaxValue;

        for (int i = 0; i < 4; i++)
        {
            if (Collider2D.CollideHitboxLine(mapHitboxArounds[i], line, out Vector2 tmpInter))
            {
                collide = true;
                float sqrDist = from.SqrDistance(tmpInter);
                if(sqrDist < minInterSqrDist)
                {
                    minInterSqrDist = sqrDist;
                    inter = tmpInter;
                }
            }
        }

        if(collide)
        {
            float lineDist = Mathf.Sqrt(minInterSqrDist);
            RaycastHit2D[] raycasts = Physics2D.RaycastAll(from, direction, lineDist, layerMask);
            globalInterPoints.Add(inter);

            for (int i = 0; i < raycasts.Length; i++)
            {
                interPoints.Add(globalInterPoints.Clone());
            }
            raycastIndex += raycasts.Length;
            reachDistance += lineDist;

            inter = GetComplementaryPoint(inter);
            return raycasts.Merge(RaycastAllRecur(inter, direction, distance - lineDist, layerMask, reachDistance, raycastIndex, ref globalInterPoints, ref interPoints));
        }
        else //ez case
        {
            RaycastHit2D[] raycasts = Physics2D.RaycastAll(from, direction, distance, layerMask);
            for (int i = 0; i < raycasts.Length; i++)
            {
                interPoints.Add(new List<Vector2>());
            }
            return raycasts;
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
            Collider2D collider = Collider2D.FromUnityCollider2D(raycasts[raycasts.Length - 1].collider);
            Circle circle = new Circle(raycasts.Last().point, radius);

            if (Collider2D.Collide(collider, circle, out Vector2 collisionPoint))
            {
                raycasts[raycasts.Length - 1].distance = inter.Count > 0 ? collisionPoint.Distance(inter.Last()) : collisionPoint.Distance(start);
                raycasts[raycasts.Length - 1].point = collisionPoint;
            }
            else
            {
                Debug.LogWarning("Debug pls");
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
        if (mapHitbox.Contains(end))
        {
            Circle c = new Circle(end, radius);
            bool calculateEdge = false;
            if(Collider2D.CollideCircleLine(c, mapHitbox.size * 0.5f, 0.5f * new Vector2(mapHitbox.size.x, -mapHitbox.size.y)) ||
                Collider2D.CollideCircleLine(c, mapHitbox.size * (-0.5f), 0.5f * new Vector2(-mapHitbox.size.x, mapHitbox.size.y)))
            {
                end += Vector2.right * (end.x >= 0f ? -mapSize.x : mapSize.x);
                calculateEdge = true;
            }

            if (Collider2D.CollideCircleLine(c, mapHitbox.size * 0.5f, 0.5f * new Vector2(-mapHitbox.size.x, mapHitbox.size.y)) ||
                Collider2D.CollideCircleLine(c, mapHitbox.size * (-0.5f), 0.5f * new Vector2(mapHitbox.size.x, -mapHitbox.size.y)))
            {
                end += Vector2.up * (end.y >= 0f ? -mapSize.y : mapSize.y);
                calculateEdge = true;
            }
            if (calculateEdge)
                return raycasts.Merge(Physics2D.CircleCastAll(end, radius, dir, 0.01f, layerMask).ToList());
            return raycasts;
        }
        else
        {
            if (Collider2D.CollideHitboxLine(mapHitbox, start, end, out Vector2 col))
            {
                inters.Add(col);
                float newDist = distance - start.Distance(col);
                col = GetComplementaryPoint(col);
                return raycasts.Merge(CircleCastRecur(col, dir, radius, newDist, layerMask, ref inters, onlyOne));
            }
            else
            {
                Debug.LogWarning("must be collision");
                LogManager.instance.WriteLog("must be collision between cameraHitbox and the line in PhysicToric.CircleCastRecur()", mapHitbox, new Line2D(start, end));
                return raycasts;
            }
        }

        void RemoveUnvaillableRaycastHit(ref List<RaycastHit2D> raycasts)
        {
            for (int i = raycasts.Count - 1; i >= 0; i--)
            {
                if (raycasts[i].collider == null || !mapHitbox.Contains(raycasts[i].point))
                {
                    raycasts.RemoveAt(i);
                }
            }
        }
    }

    #endregion

    #endregion

    #region Gizmos

#if UNITY_EDITOR

    public static void GizmosDrawRaycast(Vector2 from, Vector2 to)
    {
        from = GetPointInsideBounds(from);
        to = GetPointInsideBounds(to);
        GizmosDrawRaycast(from, Direction(from, to), Distance(from, to));
    }

    public static void GizmosDrawRaycast(Vector2 from, in Vector2 direction, float distance)
    {
        from = GetPointInsideBounds(from);
        RaycastHit2D _ = Raycast(GetPointInsideBounds(from), direction, distance, 0, out Vector2[] inters);
        if(inters.Length <= 0)
        {
            Gizmos.DrawLine(from, from + direction * distance);
        }
        else
        {
            Vector2 beg = from;
            for (int i = 0; i < inters.Length; i++)
            {
                distance -= Distance(beg, inters[i]);
                Gizmos.DrawLine(beg, inters[i]);
                beg = GetComplementaryPoint(inters[i]);
            }
            Gizmos.DrawLine(beg, beg + distance * direction);
        }
    }

#endif

    #endregion
}
