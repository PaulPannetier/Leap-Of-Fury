#region Using

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Collision2D;
using Collider2D = Collision2D.Collider2D;
using Ray2D = Collision2D.Ray2D;
using System;

#endregion

#region Struct

public struct ToricRaycastHit2D
{
    public Vector2 point;
    public Vector2 centroid;
    public UnityEngine.Collider2D collider;
    public Vector2 normal;
    public Rigidbody2D rigidbody;
    public float distance;
    public Transform transform => rigidbody != null ? rigidbody.transform : (collider != null ? collider.transform : null);

    public ToricRaycastHit2D(Vector2 point, Vector2 centroid, UnityEngine.Collider2D collider, Vector2 normal, Rigidbody2D rb, float distance)
    {
        this.point = point;
        this.centroid = centroid;
        this.collider = collider;
        this.normal = normal;
        this.rigidbody = rb;
        this.distance = distance;
    }

    public ToricRaycastHit2D(RaycastHit2D raycast)
    {
        this.point = raycast.point;
        this.centroid = raycast.centroid;
        this.collider = raycast.collider;
        this.normal = raycast.normal;
        this.rigidbody = raycast.rigidbody;
        this.distance = raycast.distance;
    }
}

#endregion

public static class PhysicsToric
{
    #region Camera and General Things

    private static Hitbox mapHitbox;
    private static List<UnityEngine.Collider2D> priorityColliders = new List<UnityEngine.Collider2D>();
    private static Line2D[] mapSides;
    private static Vector2[] mapOffset;

    public static void AddPriorityCollider(UnityEngine.Collider2D collider)
    {
        priorityColliders.Add(collider);
    }

    public static void RemovePriorityCollider(UnityEngine.Collider2D collider)
    {
        priorityColliders.Remove(collider);
    }

    public static void ClearPriorityCollider()
    {
        priorityColliders.Clear();
    }

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

    public static bool IsPointInsideBound(in Vector2 point) => -mapHitbox.size.x * 0.5f <= point.x && mapHitbox.size.x * 0.5f >= point.x && -mapHitbox.size.y * 0.5f <= point.y && mapHitbox.size.y * 0.5f >= point.y;

    /// <param name="point"></param>
    /// <returns>Le point visible au coor donnée en param dans l'espace torique</returns>
    public static Vector2 GetPointInsideBounds(in Vector2 point)
    {
        return new Vector2(Useful.ClampModulo(-mapHitbox.size.x * 0.5f, mapHitbox.size.x * 0.5f, point.x),
            Useful.ClampModulo(-mapHitbox.size.y * 0.5f, mapHitbox.size.y * 0.5f, point.y));
    }

    public static Vector2 GetComplementaryPoint(Vector2 point)
    {
        Vector2 step = new Vector2(point.x - mapHitbox.center.x > 0f ? 0.001f : -0.001f, point.y - mapHitbox.center.y > 0f ? 0.001f : -0.001f);
        while (mapHitbox.Contains(point))
        {
            point += step;
        }
        point += step;
        return GetPointInsideBounds(point);
    }

    public static float Distance(Vector2 p1, Vector2 p2)
    {
        p1 = GetPointInsideBounds(p1);
        p2 = GetPointInsideBounds(p2);

        float toricX = p1.x + (p2.x - p1.x).Sign() * mapHitbox.size.x;
        float x = Mathf.Abs(p1.x - p2.x) < Mathf.Abs(toricX - p2.x) ? p1.x : toricX;
        float toricY = p1.y + (p2.y - p1.y).Sign() * mapHitbox.size.y;
        float y = Mathf.Abs(p1.y - p2.y) < Mathf.Abs(toricY - p2.y) ? p1.y : toricY;

        return p2.Distance(new Vector2(x, y));
    }

    public static Vector2 Direction(Vector2 a, Vector2 b)
    {
        Vector2[] possibleA = new Vector2[5]
        {
            a,
            new Vector2(a.x + mapHitbox.size.x , a.y),
            new Vector2(a.x - mapHitbox.size.x, a.y),
            new Vector2(a.x, a.y + mapHitbox.size.y),
            new Vector2(a.x, a.y - mapHitbox.size.y)
        };

        Vector2[] possibleB = new Vector2[5]
        {
            b,
            new Vector2(b.x + mapHitbox.size.x , b.y),
            new Vector2(b.x - mapHitbox.size.x, b.y),
            new Vector2(b.x, b.y + mapHitbox.size.y),
            new Vector2(b.x, b.y - mapHitbox.size.y)
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
        return (bKeep - aKeep) * (1f / Mathf.Sqrt(minSqrMag));
    }

    public static bool GetToricIntersection(in Vector2 from, in Vector2 end, out Vector2 inter)
    {
        for (short i = 0; i < mapHitbox.vertices.Length; i++)
        {
            if (Collider2D.CollideLines(from, end, mapHitbox.vertices[i], mapHitbox.vertices[(i + 1) % mapHitbox.vertices.Length], out inter))
            {
                return true;
            }
        }
        inter = Vector2.zero;
        return false;
    }

    #endregion

    #region Overlap

    public static UnityEngine.Collider2D OverlapPoint(Vector2 point, LayerMask layerMask)
    {
        point = GetPointInsideBounds(point);
        UnityEngine.Collider2D collider = Physics2D.OverlapPoint(point, layerMask);
        if (collider != null)
            return collider;

        Collider2D col;
        foreach (UnityEngine.Collider2D tmpcol in priorityColliders)
        {
            if (layerMask.Contain(tmpcol.gameObject.layer))
            {
                col = Collider2D.FromUnityCollider2D(tmpcol);
                if (col.Contains(point))
                {
                    return tmpcol;
                }
            }
        }
        return null;
    }

    public static UnityEngine.Collider2D[] OverlapPointAll(Vector2 point, LayerMask layerMask)
    {
        point = GetPointInsideBounds(point);
        UnityEngine.Collider2D[] colliders = Physics2D.OverlapPointAll(point, layerMask);

        Collider2D col;
        List<UnityEngine.Collider2D> resToAdd = new List<UnityEngine.Collider2D>();
        foreach (UnityEngine.Collider2D tmpcol in priorityColliders)
        {
            if (layerMask.Contain(tmpcol.gameObject.layer))
            {
                if (!colliders.Contains(tmpcol))
                {
                    col = Collider2D.FromUnityCollider2D(tmpcol);
                    if (col.Contains(point))
                    {
                        resToAdd.Add(tmpcol);
                    }
                }
            }
        }

        if (resToAdd.Count > 0)
            return colliders.Merge(resToAdd.ToArray());
        return colliders;
    }

    public static UnityEngine.Collider2D OverlapCircle(Circle circle, LayerMask layerMask) => OverlapCircle(circle.center, circle.radius, layerMask);

    public static UnityEngine.Collider2D OverlapCircle(in Vector2 center, float radius, LayerMask layerMask)
    {
        Circle circle = new Circle(GetPointInsideBounds(center), radius);

        UnityEngine.Collider2D res = Physics2D.OverlapCircle(circle.center, radius, layerMask);
        if (res != null)
            return res;

        bool[] collideWithCamSide = new bool[4];
        for (int i = 0; i < 4; i++)
        {
            if(Collider2D.CollideCircleLine(circle, mapSides[i]))
            {
                circle.MoveAt(circle.center - mapOffset[i]);
                res = Physics2D.OverlapCircle(circle.center, radius, layerMask);
                if (res != null)
                    return res;
                circle.MoveAt(circle.center + mapOffset[i]);
                collideWithCamSide[i] = true;
            }
        }

        Collider2D col;
        foreach (UnityEngine.Collider2D tmpcol in priorityColliders)
        {
            if (layerMask.Contain(tmpcol.gameObject.layer))
            {
                col = Collider2D.FromUnityCollider2D(tmpcol);
                if (Collider2D.Collide(col, circle))
                {
                    return tmpcol;
                }

                for (int i = 0; i < 4; i++)
                {
                    if (collideWithCamSide[i])
                    {
                        circle.MoveAt(circle.center - mapOffset[i]);
                        if (Collider2D.Collide(col, circle))
                        {
                            return tmpcol;
                        }
                        circle.MoveAt(circle.center + mapOffset[i]);
                    }
                }
            }
        }

        return null;
    }

    public static UnityEngine.Collider2D[] OverlapCircleAll(Circle circle, LayerMask layerMask) => OverlapCircleAll(circle.center, circle.radius, layerMask);

    public static UnityEngine.Collider2D[] OverlapCircleAll(in Vector2 center, float radius, LayerMask layerMask)
    {
        Circle circle = new Circle(GetPointInsideBounds(center), radius);
        UnityEngine.Collider2D[] res = Physics2D.OverlapCircleAll(circle.center, radius, layerMask);

        bool[] collideWithCamSide = new bool[4];
        for (int i = 0; i < 4; i++)
        {
            if (Collider2D.CollideCircleLine(circle, mapSides[i]))
            {
                circle.MoveAt(circle.center - mapOffset[i]);
                UnityEngine.Collider2D[] res2 = Physics2D.OverlapCircleAll(circle.center, radius, layerMask);
                if (res2 != null && res2.Length > 0)
                    res = res.Merge(res2);
                circle.MoveAt(circle.center + mapOffset[i]);
                collideWithCamSide[i] = true;
            }
        }

        Collider2D col;
        List<UnityEngine.Collider2D> resToAdd = new List<UnityEngine.Collider2D>();
        foreach (UnityEngine.Collider2D tmpcol in priorityColliders)
        {
            if (layerMask.Contain(tmpcol.gameObject.layer))
            {
                col = Collider2D.FromUnityCollider2D(tmpcol);
                if (Collider2D.Collide(col, circle))
                {
                    resToAdd.Add(tmpcol);
                }

                for (int i = 0; i < 4; i++)
                {
                    if (collideWithCamSide[i])
                    {
                        circle.MoveAt(circle.center - mapOffset[i]);
                        if (Collider2D.Collide(col, circle))
                        {
                            resToAdd.Add(tmpcol);
                        }
                        circle.MoveAt(circle.center + mapOffset[i]);
                    }
                }
            }
        }

        if (resToAdd.Count > 0)
            return res.Merge(resToAdd.ToArray()).Distinct().ToArray();
        return res.Length <= 0 ? res : res.Distinct().ToArray();
    }

    public static UnityEngine.Collider2D OverlapBox(Hitbox hitbox, LayerMask layerMask) => OverlapBox(hitbox.center, hitbox.size, hitbox.AngleHori(), layerMask);

    public static UnityEngine.Collider2D OverlapBox(in Vector2 point, in Vector2 size, float angle, LayerMask layerMask)
    {
        Hitbox hitbox = new Hitbox(GetPointInsideBounds(point), size);
        hitbox.Rotate(angle);

        UnityEngine.Collider2D res = Physics2D.OverlapBox(hitbox.center, size, angle * Mathf.Rad2Deg, layerMask);
        if (res != null)
            return res;

        bool[] collideWithCamSide = new bool[4];
        for (int i = 0; i < 4; i++)
        {
            if (Collider2D.CollideHitboxLine(hitbox, mapSides[i]))
            {
                hitbox.MoveAt(hitbox.center - mapOffset[i]);
                res = Physics2D.OverlapBox(hitbox.center, size, angle * Mathf.Rad2Deg, layerMask);
                if (res != null)
                    return res;
                hitbox.MoveAt(hitbox.center + mapOffset[i]);
                collideWithCamSide[i] = true;
            }
        }

        Collider2D col;
        foreach (UnityEngine.Collider2D tmpcol in priorityColliders)
        {
            if (layerMask.Contain(tmpcol.gameObject.layer))
            {
                col = Collider2D.FromUnityCollider2D(tmpcol);
                if (Collider2D.Collide(col, hitbox))
                {
                    return tmpcol;
                }

                for (int i = 0; i < 4; i++)
                {
                    if (collideWithCamSide[i])
                    {
                        hitbox.MoveAt(hitbox.center - mapOffset[i]);
                        if (Collider2D.Collide(col, hitbox))
                            return tmpcol;

                        hitbox.MoveAt(hitbox.center + mapOffset[i]);
                    }
                }
            }
        }

        return null;
    }

    public static UnityEngine.Collider2D[] OverlapBoxAll(Hitbox hitbox, LayerMask layerMask) => OverlapBoxAll(hitbox.center, hitbox.size, hitbox.AngleHori(), layerMask);

    public static UnityEngine.Collider2D[] OverlapBoxAll(in Vector2 point, in Vector2 size, float angle, LayerMask layerMask)
    {
        Hitbox hitbox = new Hitbox(GetPointInsideBounds(point), size);
        hitbox.Rotate(angle);

        UnityEngine.Collider2D[] res = Physics2D.OverlapBoxAll(hitbox.center, size, angle * Mathf.Rad2Deg, layerMask);
        bool[] collideWithCamSide = new bool[4];
        for (int i = 0; i < 4; i++)
        {
            if (Collider2D.CollideHitboxLine(hitbox, mapSides[i]))
            {
                hitbox.MoveAt(hitbox.center - mapOffset[i]);
                UnityEngine.Collider2D[] res2 = Physics2D.OverlapBoxAll(hitbox.center, size, angle * Mathf.Rad2Deg, layerMask);
                if (res2 != null && res2.Length > 0)
                    res = res.Merge(res2);
                hitbox.MoveAt(hitbox.center + mapOffset[i]);
                collideWithCamSide[i] = true;
            }
        }

        Collider2D col;
        List<UnityEngine.Collider2D> resToAdd = new List<UnityEngine.Collider2D>();
        foreach (UnityEngine.Collider2D tmpcol in priorityColliders)
        {
            if (layerMask.Contain(tmpcol.gameObject.layer))
            {
                col = Collider2D.FromUnityCollider2D(tmpcol);
                if (Collider2D.Collide(col, hitbox))
                {
                    resToAdd.Add(tmpcol);
                }

                for (int i = 0; i < 4; i++)
                {
                    if (collideWithCamSide[i])
                    {
                        hitbox.MoveAt(hitbox.center - mapOffset[i]);
                        if (Collider2D.Collide(col, hitbox))
                        {
                            resToAdd.Add(tmpcol);
                        }
                        hitbox.MoveAt(hitbox.center - mapOffset[i]);
                    }
                }
            }
        }

        if (resToAdd.Count > 0)
            return res.Merge(resToAdd.ToArray()).Distinct().ToArray();
        return res.Length <= 0 ? res : res.Distinct().ToArray();
    }

    public static UnityEngine.Collider2D OverlapCapsule(in Vector2 center, in Vector2 size, float angle, LayerMask layerMask)
    {
        Capsule c = new Capsule(center, size);
        if (Mathf.Abs(angle) > 1e-5f)
            c.Rotate(angle);
        return OverlapCapsule(c, layerMask);
    }

    public static UnityEngine.Collider2D OverlapCapsule(Capsule capsule, LayerMask layerMask)
    {
        UnityEngine.Collider2D res = PhysicsToric.OverlapBox(capsule.hitbox, layerMask);
        if (res != null)
            return res;
        res = PhysicsToric.OverlapCircle(capsule.circle1, layerMask);
        if (res != null)
            return res;
        return PhysicsToric.OverlapCircle(capsule.circle2, layerMask);
    }

    public static UnityEngine.Collider2D[] OverlapCapsuleAll(in Vector2 center, in Vector2 size, float angle, LayerMask layerMask)
    {
        Capsule c = new Capsule(center, size);
        if (Mathf.Abs(angle) > 1e-5f)
            c.Rotate(angle);
        return OverlapCapsuleAll(c, layerMask);
    }

    public static UnityEngine.Collider2D[] OverlapCapsuleAll(Capsule capsule, LayerMask layerMask)
    {
        UnityEngine.Collider2D[] res = OverlapBoxAll(capsule.hitbox, layerMask);
        res = res.Merge(OverlapCircleAll(capsule.circle1, layerMask));
        return res.Merge(OverlapCircleAll(capsule.circle2, layerMask)).Distinct().ToArray();
    }

    #endregion

    #region Cast

    #region Raycast

    private static bool ContainRaycast(RaycastHit2D[] raycasts, UnityEngine.Collider2D collider)
    {
        foreach (RaycastHit2D raycast in raycasts)
        {
            if (raycast.collider == collider)
                return true;
        }
        return false;
    }

    public static ToricRaycastHit2D Raycast(in Vector2 from, in Vector2 direction, float distance, LayerMask layerMask)
    {
        List<Vector2> _ = new List<Vector2>();
        ToricRaycastHit2D raycast = RaycastRecur(GetPointInsideBounds(from), direction.normalized, distance, layerMask, 0f, ref _);
        return raycast;
    }

    public static ToricRaycastHit2D Raycast(in Vector2 from, in Vector2 direction, float distance, LayerMask layerMask, out Vector2[] toricHitboxIntersectionsPoints)
    {
        List<Vector2> points = new List<Vector2>();
        ToricRaycastHit2D raycast = RaycastRecur(GetPointInsideBounds(from), direction.normalized, distance, layerMask, 0f, ref points);
        toricHitboxIntersectionsPoints = points.ToArray();
        return raycast;
    }

    private static ToricRaycastHit2D Physics2DRaycastPriority(in Vector2 from, in Vector2 end, LayerMask layerMask)
    {
        float minSqrDist = float.MaxValue;
        Vector2 minCp = Vector2.zero;
        Vector2 minN = Vector2.zero;
        UnityEngine.Collider2D res = null;

        Collider2D col;
        Ray2D ray = new Ray2D(from, end);
        foreach (UnityEngine.Collider2D tmpCol in priorityColliders)
        {
            if (layerMask.Contain(tmpCol.gameObject.layer))
            {
                col = Collider2D.FromUnityCollider2D(tmpCol);
                if (Collider2D.CollideRay(col, ray, out Vector2 cp, out Vector2 n))
                {
                    float sqrDist = from.SqrDistance(cp);
                    if (sqrDist < minSqrDist)
                    {
                        minSqrDist = sqrDist;
                        minCp = cp;
                        res = tmpCol;
                        minN = n;
                    }
                }
            }
        }

        return new ToricRaycastHit2D(minCp, minCp, res, minN, res != null ? res.attachedRigidbody : null, res == null ? -1f : Mathf.Sqrt(minSqrDist));
    }

    private static List<ToricRaycastHit2D> Physics2DRaycastPriorityAll(in Vector2 from, in Vector2 end, LayerMask layerMask)
    {
        List<ToricRaycastHit2D> res = new List<ToricRaycastHit2D>();
        Collider2D col;
        Ray2D ray = new Ray2D(from, end);
        foreach (UnityEngine.Collider2D tmpCol in priorityColliders)
        {
            if (layerMask.Contain(tmpCol.gameObject.layer))
            {
                col = Collider2D.FromUnityCollider2D(tmpCol);
                if (Collider2D.CollideRay(col, ray, out Vector2 cp, out Vector2 n))
                {
                    res.Add(new ToricRaycastHit2D(cp, cp, tmpCol, n, tmpCol.attachedRigidbody, from.Distance(cp)));
                }
            }
        }
        return res;
    }

    private static ToricRaycastHit2D RaycastRecur(in Vector2 from, in Vector2 direction, float distance, LayerMask layerMask, float reachDistance, ref List<Vector2> points)
    {
        RaycastHit2D raycast;
        Vector2 end = from + direction * distance;

        if (GetToricIntersection(from, end, out Vector2 inter))
        {
            float currentDistance = from.Distance(inter);
            raycast = Physics2D.Raycast(from, direction, currentDistance, layerMask);
            ToricRaycastHit2D res = Physics2DRaycastPriority(from, inter, layerMask);

            if (raycast.collider == null)
            {
                if (res.collider == null)
                {
                    reachDistance += currentDistance;
                    points.Add(inter);
                    inter = GetComplementaryPoint(inter);
                    return RaycastRecur(inter, direction, distance - currentDistance, layerMask, reachDistance, ref points);
                }
                else
                {
                    res.distance += reachDistance;
                    return res;
                }
            }
            else
            {
                if (res.collider == null || res.distance + 1e5f > raycast.distance)
                {
                    res = new ToricRaycastHit2D(raycast);
                    res.distance += reachDistance;
                    return res;
                }

                res.distance += reachDistance;
                return res;
            }
        }
        else //ez case
        {
            raycast = Physics2D.Raycast(from, direction, distance, layerMask);
            ToricRaycastHit2D res = Physics2DRaycastPriority(from, end, layerMask);

            if (raycast.collider == null)
            {
                res.distance += reachDistance;
                return res;
            }
            else
            {
                if (res.collider == null || res.distance + 1e5f > raycast.distance)
                {
                    res = new ToricRaycastHit2D(raycast);
                    res.distance += reachDistance;
                    return res;
                }

                res.distance += reachDistance;
                return res;
            }
        }
    }

    public static ToricRaycastHit2D[] RaycastAll(Vector2 from, in Vector2 dir, float distance, LayerMask layerMask)
    {
        return RaycastAllRecur(GetPointInsideBounds(from), dir.normalized, distance, layerMask, 0f);
    }

    public static ToricRaycastHit2D[] RaycastAll(Vector2 from, in Vector2 dir, float distance, LayerMask layerMask, out Vector2[][] toricHitboxIntersectionsPoints)
    {
        from = GetPointInsideBounds(from);
        List<List<Vector2>> interPoints = new List<List<Vector2>>();
        List<Vector2> globalInterPoints = new List<Vector2>();
        ToricRaycastHit2D[] raycasts = RaycastAllRecur(from, dir.normalized, distance, layerMask, 0f, ref globalInterPoints, ref interPoints);

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

    private static ToricRaycastHit2D[] RaycastAllRecur(in Vector2 from, in Vector2 direction, float distance, LayerMask layerMask, float reachDistance)
    {
        Vector2 end = from + (distance * direction);
        if (GetToricIntersection(from, end, out Vector2 inter))
        {
            float currentDist = from.Distance(inter);
            RaycastHit2D[] raycasts = Physics2D.RaycastAll(from, direction, currentDist, layerMask);
            List<ToricRaycastHit2D> priorityRaycasts = Physics2DRaycastPriorityAll(from, inter, layerMask);

            List<ToricRaycastHit2D> resList = new List<ToricRaycastHit2D>();
            for (int i = 0; i < raycasts.Length; i++)
            {
                resList.Add(new ToricRaycastHit2D(raycasts[i]));
            }

            for (int i = 0; i < priorityRaycasts.Count; i++)
            {
                if (!ContainRaycast(raycasts, priorityRaycasts[i].collider))
                {
                    resList.Add(priorityRaycasts[i]);
                }
            }

            ToricRaycastHit2D[] res = new ToricRaycastHit2D[resList.Count];
            for (int i = 0; i < resList.Count; i++)
            {
                res[i] = resList[i];
                res[i].distance += reachDistance;
            }

            reachDistance += currentDist;
            return res.Merge(RaycastAllRecur(GetComplementaryPoint(inter), direction, distance - currentDist, layerMask, reachDistance));

        }
        else
        {
            RaycastHit2D[] raycasts = Physics2D.RaycastAll(from, direction, distance, layerMask);
            List<ToricRaycastHit2D> priorityRaycasts = Physics2DRaycastPriorityAll(from, end, layerMask);

            List<ToricRaycastHit2D> resList = new List<ToricRaycastHit2D>();
            for (int i = 0; i < raycasts.Length; i++)
            {
                resList.Add(new ToricRaycastHit2D(raycasts[i]));
            }

            for (int i = 0; i < priorityRaycasts.Count; i++)
            {
                if (!ContainRaycast(raycasts, priorityRaycasts[i].collider))
                {
                    resList.Add(priorityRaycasts[i]);
                }
            }

            ToricRaycastHit2D[] res = new ToricRaycastHit2D[resList.Count];
            for (int i = 0; i < resList.Count; i++)
            {
                res[i] = resList[i];
                res[i].distance += reachDistance;
            }
            return res;
        }
    }

    private static ToricRaycastHit2D[] RaycastAllRecur(in Vector2 from, in Vector2 direction, float distance, LayerMask layerMask, float reachDistance, ref List<Vector2> globalInterPoints, ref List<List<Vector2>> interPoints)
    {
        Vector2 end = from + (distance * direction);
        if (GetToricIntersection(from, end, out Vector2 inter))
        {
            float currentDist = from.Distance(inter);
            RaycastHit2D[] raycasts = Physics2D.RaycastAll(from, direction, currentDist, layerMask);
            List<ToricRaycastHit2D> priorityRaycasts = Physics2DRaycastPriorityAll(from, inter, layerMask);

            List<ToricRaycastHit2D> resList = new List<ToricRaycastHit2D>();
            for (int i = 0; i < raycasts.Length; i++)
            {
                resList.Add(new ToricRaycastHit2D(raycasts[i]));
            }

            for (int i = 0; i < priorityRaycasts.Count; i++)
            {
                if (!ContainRaycast(raycasts, priorityRaycasts[i].collider))
                {
                    resList.Add(priorityRaycasts[i]);
                }
            }

            ToricRaycastHit2D[] res = new ToricRaycastHit2D[resList.Count];
            for (int i = 0; i < resList.Count; i++)
            {
                res[i] = resList[i];
                res[i].distance += reachDistance;
                interPoints.Add(globalInterPoints.Clone());
            }

            reachDistance += currentDist;
            globalInterPoints.Add(inter);
            return res.Merge(RaycastAllRecur(GetComplementaryPoint(inter), direction, distance - currentDist, layerMask, reachDistance, ref globalInterPoints, ref interPoints));
        }
        else
        {
            RaycastHit2D[] raycasts = Physics2D.RaycastAll(from, direction, distance, layerMask);
            List<ToricRaycastHit2D> priorityRaycasts = Physics2DRaycastPriorityAll(from, end, layerMask);

            List<ToricRaycastHit2D> resList = new List<ToricRaycastHit2D>();
            for (int i = 0; i < raycasts.Length; i++)
            {
                resList.Add(new ToricRaycastHit2D(raycasts[i]));
            }

            for (int i = 0; i < priorityRaycasts.Count; i++)
            {
                if (!ContainRaycast(raycasts, priorityRaycasts[i].collider))
                {
                    resList.Add(priorityRaycasts[i]);
                }
            }

            ToricRaycastHit2D[] res = new ToricRaycastHit2D[resList.Count];
            for (int i = 0; i < resList.Count; i++)
            {
                res[i] = resList[i];
                res[i].distance += reachDistance;
                interPoints.Add(globalInterPoints.Clone());
            }

            return res;
        }
    }

    #endregion

    #region CircleCast

    #region Physics2D

    private static readonly Dictionary<Type, Func<Vector2, Vector2, float, float, Collider2D, ToricRaycastHit2D>> circleCastFunction = new Dictionary<Type, Func<Vector2, Vector2, float, float, Collider2D, ToricRaycastHit2D>>()
    {
        { typeof(Circle), (Vector2 start, Vector2 dir, float radius, float distance, Collider2D circle) => Physics2DCircleCastCircle(start, dir, radius, distance, (Circle)circle) },
        { typeof(Hitbox), (Vector2 start, Vector2 dir, float radius, float distance, Collider2D hitbox) => Physics2DCircleCastHitbox(start, dir, radius, distance, (Hitbox)hitbox) },
        { typeof(Polygone), (Vector2 start, Vector2 dir, float radius, float distance, Collider2D poly) => Physics2DCircleCastPolygone(start, dir, radius, distance, (Polygone)poly) },
        { typeof(Capsule), (Vector2 start, Vector2 dir, float radius, float distance, Collider2D capsule) => Physics2DCircleCastCapsule(start, dir, radius, distance, (Capsule)capsule) }
    };

    #region Circle Cast Single

    private static ToricRaycastHit2D Physics2DCircleCastPriority(in Vector2 start, in Vector2 dir, float radius, float distance, LayerMask layerMask)
    {
        Debug.DrawLine(start, start + dir * distance);

        Collider2D col;
        ToricRaycastHit2D raycast;
        ToricRaycastHit2D res = new ToricRaycastHit2D();
        bool collide = false;

        foreach (UnityEngine.Collider2D unityCol in priorityColliders)
        {
            if(!layerMask.Contain(unityCol.gameObject.layer))
                continue;

            if (!(unityCol is UnityEngine.PolygonCollider2D))
                continue;

            col = Collider2D.FromUnityCollider2D(unityCol);
            raycast = circleCastFunction[col.GetType()](start, dir, radius, distance, col);

            if(raycast.distance >= 0f && (!collide || raycast.distance < res.distance))
            {
                collide = true;
                res = raycast;
                res.collider = unityCol;
                res.rigidbody = unityCol.attachedRigidbody;
            }
        }

        return res;
    }

    #region CircleCast Circle

    private static ToricRaycastHit2D Physics2DCircleCastCircle(in Vector2 start, in Vector2 dir, float radius, float distance, Circle circle)
    {
        Vector2 end = start + dir * distance;
        if (Line2D.Distance(start, end, circle.center) > circle.radius + radius)
            return new ToricRaycastHit2D(Vector2.zero, Vector2.zero, null, Vector2.zero, null, -1f);

        Vector2 maxCenter = StraightLine2D.OrthogonalProjection(circle.center, start, end);
        maxCenter = maxCenter.SqrDistance(start) > distance * distance ? end : maxCenter;

        void FindCenterRecur(Circle circle, in Vector2 minCenter, in Vector2 maxCenter, float sumRadiusSqr, ref Vector2? lastCollideCenter)
        {
            if (minCenter.SqrDistance(maxCenter) < 1e-5f || maxCenter.SqrDistance(circle.center) > sumRadiusSqr)
                return;

            Vector2 avgCenter = (minCenter + maxCenter) * 0.5f;
            if (avgCenter.SqrDistance(circle.center) <= sumRadiusSqr)
            {
                lastCollideCenter = avgCenter;
                FindCenterRecur(circle, minCenter, avgCenter, sumRadiusSqr, ref lastCollideCenter);
                return;
            }
            FindCenterRecur(circle, avgCenter, maxCenter, sumRadiusSqr, ref lastCollideCenter);
        }

        Vector2? bestCenter = null;
        FindCenterRecur(circle, start, maxCenter, (radius + circle.radius) * (radius + circle.radius), ref bestCenter);

        if (bestCenter.HasValue)
        {
            Vector2 n = (bestCenter.Value - circle.center).normalized;
            Vector2 point = bestCenter.Value - n * radius;
            return new ToricRaycastHit2D(point, bestCenter.Value, null, n, null, start.Distance(point));
        }

        return new ToricRaycastHit2D(Vector2.zero, Vector2.zero, null, Vector2.zero, null, -1f);
    }

    #endregion

    #region CircleCast Polygone

    private static ToricRaycastHit2D Physics2DCircleCastPolygone(in Vector2 start, in Vector2 dir, float radius, float distance, Polygone polygone)
    {
        if (polygone.center.SqrDistance(start) > (radius + distance + polygone.inclusiveCircle.radius) * (radius + distance + polygone.inclusiveCircle.radius))
            return new ToricRaycastHit2D(Vector2.zero, Vector2.zero, null, Vector2.zero, null, -1f);

        List<Line2D> sides = new List<Line2D>();
        float cache = (distance + radius) * (distance + radius);
        Vector2 offset = dir.NormalVector() * radius;
        StraightLine2D diameterStraightLine = new StraightLine2D(start + offset, start - offset);

        float minX, maxX, minY, maxY;
        if(diameterStraightLine.A.x <= diameterStraightLine.B.x)
        {
            minX = diameterStraightLine.A.x;
            maxX = diameterStraightLine.B.x;
        }
        else
        {
            minX = diameterStraightLine.B.x;
            maxX = diameterStraightLine.A.x;
        }
        if (diameterStraightLine.A.y <= diameterStraightLine.B.y)
        {
            minY = diameterStraightLine.A.y;
            maxY = diameterStraightLine.B.y;
        }
        else
        {
            minY = diameterStraightLine.B.y;
            maxY = diameterStraightLine.A.y;
        }

        Line2D side;
        Vector2 point1, point2;
        for (int i = 0; i < polygone.vertices.Length; i++)
        {
            side = new Line2D(polygone.vertices[i], polygone.vertices[(i + 1) % polygone.vertices.Length]);
            if (dir.Dot(side.A - start) < 0f && dir.Dot(side.B - start) < 0f)
                continue;

            point1 = diameterStraightLine.OrthogonalProjection(side.A);
            point2 = diameterStraightLine.OrthogonalProjection(side.B);
            if(point1.SqrDistance(side.A) > cache && point2.SqrDistance(side.B) > cache)
                continue;

            bool isCircleCastPossible = minX < point1.x && point1.x <= maxX && minY <= point1.y && point1.y <= maxY;
            isCircleCastPossible = isCircleCastPossible || (minX < point2.x && point2.x <= maxX && minY <= point2.y && point2.y <= maxY);
            isCircleCastPossible = isCircleCastPossible || (offset.Dot(point1 - start).Sign() != offset.Dot(point2 - start).Sign());
            if (isCircleCastPossible)
            {
                sides.Add(side);
                Circle.GizmosDraw(side.A, 0.1f, Debug.DrawLine);
                Circle.GizmosDraw(side.B, 0.1f, Debug.DrawLine);
            }
        }

        if (sides.Count <= 0)
            return new ToricRaycastHit2D(Vector2.zero, Vector2.zero, null, Vector2.zero, null, -1f);

        StraightLine2D straightLine1 = new StraightLine2D(diameterStraightLine.A, diameterStraightLine.A + dir);
        StraightLine2D straightLine2 = new StraightLine2D(diameterStraightLine.B, diameterStraightLine.B + dir);

        Debug.DrawLine(diameterStraightLine.A + dir * 100, diameterStraightLine.A - dir * 100, Color.black);
        Debug.DrawLine(diameterStraightLine.B + dir * 100, diameterStraightLine.B - dir * 100, Color.black);

        List<Line2D> sides2 = new List<Line2D>();
        StraightLine2D sideStraightLine;
        Vector2 a, b, c;
        for (int i = 0; i < sides.Count; i++)
        {
            side = sides[i];
            sideStraightLine = new StraightLine2D(side.A, side.B);
            if(!Collider2D.CollideStraightLines(straightLine1, sideStraightLine, out point1))
                point1 = side.A;
            if (!Collider2D.CollideStraightLines(straightLine2, sideStraightLine, out point2))
                point2 = side.A;

            if (Mathf.Abs(side.A.x - point1.x) < 1e-3f && Mathf.Abs(side.B.x - point2.x) < 1e-3f && Mathf.Abs(side.A.x - side.B.x) < 1e-3f)
            {
                (a, b, c) = RemoveMin(side.A, side.B, point1, point2, false);
                (point1, point2) = RemoveMax(a, b, c, false);
            }
            else
            {
                (a, b, c) = RemoveMin(side.A, side.B, point1, point2, true);
                (point1, point2) = RemoveMax(a, b, c, true);
            }
            (Vector2, Vector2, Vector2) RemoveMin(in Vector2 a, in Vector2 b, in Vector2 c, in Vector2 d, bool x)
            {
                if(x)
                {
                    if (a.x < b.x && a.x < c.x && a.x < d.x)
                        return (b, c, d);
                    if (b.x < c.x && b.x < d.x)
                        return (a, c, d);
                    return c.x < d.x ? (a, b, d) : (a, b, c);
                }
                else
                {
                    if (a.y < b.y && a.y < c.y && a.y < d.y)
                        return (b, c, d);
                    if (b.y < c.y && b.y < d.y)
                        return (a, c, d);
                    return c.y < d.y ? (a, b, d) : (a, b, c);
                }
            }
            (Vector2, Vector2) RemoveMax(in Vector2 a, in Vector2 b, in Vector2 c, bool x)
            {
                if (x)
                {
                    if (a.x > b.x && a.x > c.x)
                        return (b, c);
                    return b.x > c.x ? (a, c) : (a, b);
                }
                else
                {
                    if (a.y > b.y && a.y > c.y)
                        return (b, c);
                    return b.y > c.y ? (a, c) : (a, b);
                }
            }

            if (dir.Dot(point1 - start) > 0f || dir.Dot(point2 - start) > 0f)
            {
                sides2.Add(new Line2D(point1, point2));
                //Circle.GizmosDraw(point1, 0.1f, Debug.DrawLine);
                //Circle.GizmosDraw(point2, 0.1f, Debug.DrawLine);
            }
        }
        sides.Clear();

        Vector2 minCenter, maxCenter;
        straightLine1.A = start;
        straightLine1.B = start + dir;
        ToricRaycastHit2D? res = null;

        for (int i = 0; i < sides2.Count; i++)
        {
            void FindCenterRecur(in Vector2 start, Line2D line, in Vector2 minCenter, in Vector2 maxCenter, float radius, float maxDistanceSqr, ref ToricRaycastHit2D? lastCollideHit)
            {
                if (minCenter.SqrDistance(maxCenter) < 1e-5f || minCenter.SqrDistance(start) > maxDistanceSqr)
                    return;

                Vector2 avgCenter = (minCenter + maxCenter) * 0.5f;
                if(Collider2D.CollideCircleLine(new Circle(avgCenter, radius), line, out Vector2 inter, out Vector2 n))
                {
                    lastCollideHit = new ToricRaycastHit2D(inter, avgCenter, null, -n, null, start.Distance(avgCenter));
                    FindCenterRecur(start, line, minCenter, avgCenter, radius, maxDistanceSqr, ref lastCollideHit);
                    return;
                }
                FindCenterRecur(start, line, avgCenter, maxCenter, radius, maxDistanceSqr, ref lastCollideHit);
            }

            side = sides2[i];
            minCenter = straightLine1.OrthogonalProjection(side.A);
            maxCenter = straightLine1.OrthogonalProjection(side.B);

            if(start.SqrDistance(minCenter) > start.SqrDistance(maxCenter))
            {
                a = minCenter;
                minCenter = maxCenter;
                maxCenter = a;
            }

            minCenter -= radius * dir;

            if(res.HasValue && start.SqrDistance(minCenter) > res.Value.distance)
            {
                continue;
            }

            //Circle.GizmosDraw(minCenter, radius, Debug.DrawLine);
            //Circle.GizmosDraw(maxCenter, radius, Debug.DrawLine);

            ToricRaycastHit2D? bestHit = null;
            FindCenterRecur(start, side, minCenter, maxCenter, radius, cache, ref bestHit);
            if(bestHit.HasValue)
            {
                if(!res.HasValue || bestHit.Value.distance < res.Value.distance)
                {
                    res = bestHit.Value;
                }
            }
        }

        if (!res.HasValue)
            return new ToricRaycastHit2D(Vector2.zero, Vector2.zero, null, Vector2.zero, null, -1f);

        ToricRaycastHit2D toricRaycastHit = res.Value;
        toricRaycastHit.distance = Mathf.Sqrt(toricRaycastHit.distance);
        return toricRaycastHit;
    }

    #endregion

    #region CircleCast Hitbox

    private static ToricRaycastHit2D Physics2DCircleCastHitbox(in Vector2 start, in Vector2 dir, float radius, float distance, Hitbox hitbox)
    {
        return Physics2DCircleCastPolygone(start, dir, radius, distance, hitbox.ToPolygone());
    }

    #endregion

    #region CircleCast Capsule

    private static ToricRaycastHit2D Physics2DCircleCastCapsule(in Vector2 start, in Vector2 dir, float radius, float distance, Capsule capsule)
    {
        if(capsule.inclusiveCircle.center.SqrDistance(start) > (radius + distance + capsule.inclusiveCircle.radius) * (radius + distance + capsule.inclusiveCircle.radius))
            return new ToricRaycastHit2D(Vector2.zero, Vector2.zero, null, Vector2.zero, null, -1f);

        ToricRaycastHit2D rayC1 = Physics2DCircleCastCircle(start, dir, radius, distance, capsule.circle1);
        ToricRaycastHit2D rayC2 = Physics2DCircleCastCircle(start, dir, radius, distance, capsule.circle2);
        ToricRaycastHit2D rayHitbox = Physics2DCircleCastHitbox(start, dir, radius, distance, capsule.hitbox);

        if(rayC1.distance >= 0f || rayC2.distance >= 0f || rayHitbox.distance >= 0f)
        {
            if(rayC1.distance >= 0f && (rayC1.distance <= rayC2.distance || rayC2.distance < 0f) && (rayC1.distance <= rayHitbox.distance || rayHitbox.distance < 0f))
                return rayC1;

            if (rayC2.distance >= 0f && (rayC2.distance <= rayHitbox.distance || rayHitbox.distance < 0f))
                return rayC2;
            return rayHitbox;
        }

        return new ToricRaycastHit2D(Vector2.zero, Vector2.zero, null, Vector2.zero, null, -1f);
    }

    #endregion

    #endregion

    #region Circle Cast All

    private static ToricRaycastHit2D[] Physics2DCircleCastPriorityAll(in Vector2 start, in Vector2 dir, float radius, float distance, LayerMask layerMask)
    {
        Debug.DrawLine(start, start + dir * distance);

        Collider2D col;
        ToricRaycastHit2D raycast;
        List<ToricRaycastHit2D> res = new List<ToricRaycastHit2D>();
        foreach (UnityEngine.Collider2D unityCol in priorityColliders)
        {
            if (!layerMask.Contain(unityCol.gameObject.layer))
                continue;

            col = Collider2D.FromUnityCollider2D(unityCol);
            raycast = circleCastFunction[col.GetType()](start, dir, radius, distance, col);

            if (raycast.distance >= 0f)
            {
                raycast.collider = unityCol;
                raycast.rigidbody = unityCol.attachedRigidbody;
                res.Add(raycast);
            }
        }

        int CompareToricRaycastHit2D(ToricRaycastHit2D hit1, ToricRaycastHit2D hit2)
        {
            return (int)(hit1.distance - hit2.distance).Sign();
        }

        res.Sort(CompareToricRaycastHit2D);
        return res.ToArray();
    }

    #endregion

    #endregion

    private static void RemoveUnvaillableRaycastHit(ref List<RaycastHit2D> raycasts)
    {
        for (int i = raycasts.Count - 1; i >= 0; i--)
        {
            if (raycasts[i].collider == null || !mapHitbox.Contains(raycasts[i].point))
            {
                raycasts.RemoveAt(i);
            }
        }
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

    #region Circle Cast Single

    public static ToricRaycastHit2D CircleCast(in Vector2 from, in Vector2 dir, float radius, float distance, LayerMask layerMask)
    {
        List<Vector2> _ = new List<Vector2>();
        ToricRaycastHit2D raycasts = CircleCastRecur(GetPointInsideBounds(from), dir.normalized, radius, distance, layerMask, 0f, ref _);
        //FixCircleCastRaycastPoints(from, radius, ref raycasts, ref inter);
        return raycasts;
    }

    public static ToricRaycastHit2D CircleCast(in Vector2 from, in Vector2 dir, float radius, float distance, LayerMask layerMask, out Vector2[] toricIntersections)
    {
        List<Vector2> inter = new List<Vector2>();
        ToricRaycastHit2D raycasts = CircleCastRecur(GetPointInsideBounds(from), dir.normalized, radius, distance, layerMask, 0f, ref inter);
        toricIntersections = inter.ToArray();
        //FixCircleCastRaycastPoints(from, radius, ref raycasts, ref inter);
        return raycasts;
    }

    private static ToricRaycastHit2D CircleCastRecur(Vector2 from, in Vector2 direction, float radius, float distance, LayerMask layerMask, float reachDistance, ref List<Vector2> points)
    {
        RaycastHit2D circleCast = default;
        ToricRaycastHit2D circleCastPriority;
        Vector2 end = from + direction * distance;
        if (GetToricIntersection(from, end, out Vector2 inter))
        {
            float currentDistance = from.Distance(inter);
            //circleCast = Physics2D.CircleCast(from, radius, direction, currentDistance, layerMask);
            circleCastPriority = Physics2DCircleCastPriority(from, direction, radius, currentDistance, layerMask);

            if (circleCast.collider == null)
            {
                if (circleCastPriority.collider == null)
                {
                    reachDistance += currentDistance;
                    points.Add(inter);
                    inter = GetComplementaryPoint(inter);
                    return RaycastRecur(inter, direction, distance - currentDistance, layerMask, reachDistance, ref points);
                }
                else
                {
                    if(!mapHitbox.Contains(circleCastPriority.point))
                    {
                        reachDistance += currentDistance;
                        points.Add(inter);
                        inter = GetComplementaryPoint(inter);
                        return RaycastRecur(inter, direction, distance - currentDistance, layerMask, reachDistance, ref points);
                    }

                    circleCastPriority.distance += reachDistance;
                    return circleCastPriority;
                }
            }
            else
            {
                if (circleCastPriority.collider == null || circleCastPriority.distance + 1e5f > circleCast.distance)
                {
                    if (!mapHitbox.Contains(circleCast.point))
                    {
                        reachDistance += currentDistance;
                        points.Add(inter);
                        inter = GetComplementaryPoint(inter);
                        return RaycastRecur(inter, direction, distance - currentDistance, layerMask, reachDistance, ref points);
                    }

                    circleCastPriority = new ToricRaycastHit2D(circleCast);
                    circleCastPriority.distance += reachDistance;
                    return circleCastPriority;
                }

                if (!mapHitbox.Contains(circleCastPriority.point))
                {
                    reachDistance += currentDistance;
                    points.Add(inter);
                    inter = GetComplementaryPoint(inter);
                    return RaycastRecur(inter, direction, distance - currentDistance, layerMask, reachDistance, ref points);
                }

                circleCastPriority.distance += reachDistance;
                return circleCastPriority;
            }
        }
        else //ez case
        {
            //circleCast = Physics2D.CircleCast(from, radius, direction, distance, layerMask);
            circleCastPriority = Physics2DCircleCastPriority(from, direction, radius, distance, layerMask);

            if (circleCast.collider == null)
            {
                if(circleCastPriority.collider != null)
                {
                    if(!mapHitbox.Contains(circleCastPriority.point))
                        return default(ToricRaycastHit2D);

                    circleCastPriority.distance += reachDistance;
                }
                return circleCastPriority;
            }
            else
            {
                if (circleCastPriority.collider == null || circleCastPriority.distance + 1e5f > circleCast.distance)
                {
                    if (!mapHitbox.Contains(circleCast.point))
                        return default(ToricRaycastHit2D);

                    circleCastPriority = new ToricRaycastHit2D(circleCast);
                    circleCastPriority.distance += reachDistance;
                    return circleCastPriority;
                }

                if (!mapHitbox.Contains(circleCastPriority.point))
                    return default(ToricRaycastHit2D);

                circleCastPriority.distance += reachDistance;
                return circleCastPriority;
            }
        }
    }

    #endregion

    #region Circle Cast All

    public static ToricRaycastHit2D[] CircleCastAll(in Vector2 from, in Vector2 dir, float radius, float distance, LayerMask layerMask)
    {
        ToricRaycastHit2D[] raycasts = CircleCastRecurAll(GetPointInsideBounds(from), dir.normalized, radius, distance, layerMask, 0f);
        //FixCircleCastRaycastPoints(from, radius, ref raycasts, ref inter);
        return raycasts;
    }

    public static ToricRaycastHit2D[] CircleCastAll(in Vector2 from, in Vector2 dir, float radius, float distance, LayerMask layerMask, out Vector2[][] toricIntersections)
    {
        List<List<Vector2>> interPoints = new List<List<Vector2>>();
        List<Vector2> globalInterPoints = new List<Vector2>();
        ToricRaycastHit2D[] raycasts = CircleCastRecurAll(GetPointInsideBounds(from), dir.normalized, radius, distance, layerMask, 0f, ref globalInterPoints, ref interPoints);

        toricIntersections = new Vector2[interPoints.Count][];
        for (int i = 0; i < toricIntersections.Length; i++)
        {
            toricIntersections[i] = new Vector2[interPoints[i].Count];
            for (int j = 0; j < toricIntersections[i].Length; j++)
            {
                toricIntersections[i][j] = interPoints[i][j];
            }
        }
        //FixCircleCastRaycastPoints(from, radius, ref raycasts, ref inter);
        return raycasts;
    }

    private static ToricRaycastHit2D[] CircleCastRecurAll(Vector2 from, in Vector2 direction, float radius, float distance, LayerMask layerMask, float reachDistance)
    {
        Vector2 end = from + (distance * direction);
        if (GetToricIntersection(from, end, out Vector2 inter))
        {
            float currentDist = from.Distance(inter);
            //RaycastHit2D[] circleCasts = Physics2D.CircleCastAll(from, radius, direction, distance, layerMask);
            RaycastHit2D[] circleCasts = Array.Empty<RaycastHit2D>();
            ToricRaycastHit2D[] priorityCirclesCasts = Physics2DCircleCastPriorityAll(from, direction, radius, currentDist, layerMask);

            List<ToricRaycastHit2D> resList = new List<ToricRaycastHit2D>();
            for (int i = 0; i < circleCasts.Length; i++)
            {
                if (mapHitbox.Contains(circleCasts[i].point))
                {
                    resList.Add(new ToricRaycastHit2D(circleCasts[i]));
                }
            }

            for (int i = 0; i < priorityCirclesCasts.Length; i++)
            {
                if (!ContainRaycast(circleCasts, priorityCirclesCasts[i].collider) && mapHitbox.Contains(priorityCirclesCasts[i].point))
                {
                    resList.Add(priorityCirclesCasts[i]);
                }
            }

            ToricRaycastHit2D[] res = new ToricRaycastHit2D[resList.Count];
            for (int i = 0; i < resList.Count; i++)
            {
                res[i] = resList[i];
                res[i].distance += reachDistance;
            }

            reachDistance += currentDist;
            return res.Merge(CircleCastRecurAll(GetComplementaryPoint(inter), direction, radius, distance - currentDist, layerMask, reachDistance));
        }
        else
        {
            //RaycastHit2D[] circleCasts = Physics2D.CircleCastAll(from, radius, direction, distance, layerMask);
            RaycastHit2D[] circleCasts = Array.Empty<RaycastHit2D>();

            ToricRaycastHit2D[] priorityCirclesCasts = Physics2DCircleCastPriorityAll(from, direction, radius, distance, layerMask);

            List<ToricRaycastHit2D> resList = new List<ToricRaycastHit2D>();
            for (int i = 0; i < circleCasts.Length; i++)
            {
                if (mapHitbox.Contains(circleCasts[i].point))
                {
                    resList.Add(new ToricRaycastHit2D(circleCasts[i]));
                }
            }

            for (int i = 0; i < priorityCirclesCasts.Length; i++)
            {
                if (!ContainRaycast(circleCasts, priorityCirclesCasts[i].collider) && mapHitbox.Contains(priorityCirclesCasts[i].point))
                {
                    resList.Add(priorityCirclesCasts[i]);
                }
            }

            ToricRaycastHit2D[] res = new ToricRaycastHit2D[resList.Count];
            for (int i = 0; i < resList.Count; i++)
            {
                res[i] = resList[i];
                res[i].distance += reachDistance;
            }
            return res;
        }
    }

    private static ToricRaycastHit2D[] CircleCastRecurAll(Vector2 from, in Vector2 direction, float radius, float distance, LayerMask layerMask, float reachDistance, ref List<Vector2> globalInterPoints, ref List<List<Vector2>> interPoints)
    {
        Vector2 end = from + (distance * direction);
        if (GetToricIntersection(from, end, out Vector2 inter))
        {
            float currentDist = from.Distance(end);
            RaycastHit2D[] circleCasts = Physics2D.CircleCastAll(from, radius, direction, distance, layerMask);
            ToricRaycastHit2D[] priorityCirclesCasts = Physics2DCircleCastPriorityAll(from, direction, radius, currentDist, layerMask);

            List<ToricRaycastHit2D> resList = new List<ToricRaycastHit2D>();
            for (int i = 0; i < circleCasts.Length; i++)
            {
                if (mapHitbox.Contains(circleCasts[i].point))
                {
                    resList.Add(new ToricRaycastHit2D(circleCasts[i]));
                }
            }

            for (int i = 0; i < priorityCirclesCasts.Length; i++)
            {
                if (!ContainRaycast(circleCasts, priorityCirclesCasts[i].collider) && mapHitbox.Contains(priorityCirclesCasts[i].point))
                {
                    resList.Add(priorityCirclesCasts[i]);
                }
            }

            ToricRaycastHit2D[] res = new ToricRaycastHit2D[resList.Count];
            for (int i = 0; i < resList.Count; i++)
            {
                res[i] = resList[i];
                res[i].distance += reachDistance;
                interPoints.Add(globalInterPoints.Clone());
            }

            reachDistance += currentDist;
            globalInterPoints.Add(inter);
            return res.Merge(CircleCastRecurAll(GetComplementaryPoint(inter), direction, radius, distance - currentDist, layerMask, reachDistance, ref globalInterPoints, ref interPoints));
        }
        else
        {
            RaycastHit2D[] circleCasts = Physics2D.CircleCastAll(from, radius, direction, distance, layerMask);
            ToricRaycastHit2D[] priorityCirclesCasts = Physics2DCircleCastPriorityAll(from, direction, radius, distance, layerMask);

            List<ToricRaycastHit2D> resList = new List<ToricRaycastHit2D>();
            for (int i = 0; i < circleCasts.Length; i++)
            {
                if (mapHitbox.Contains(circleCasts[i].point))
                {
                    resList.Add(new ToricRaycastHit2D(circleCasts[i]));
                }
            }

            for (int i = 0; i < priorityCirclesCasts.Length; i++)
            {
                if (!ContainRaycast(circleCasts, priorityCirclesCasts[i].collider) && mapHitbox.Contains(priorityCirclesCasts[i].point))
                {
                    resList.Add(priorityCirclesCasts[i]);
                }
            }

            ToricRaycastHit2D[] res = new ToricRaycastHit2D[resList.Count];
            for (int i = 0; i < resList.Count; i++)
            {
                res[i] = resList[i];
                res[i].distance += reachDistance;
                interPoints.Add(globalInterPoints.Clone());
            }

            return res;
        }
    }

    #endregion

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
        ToricRaycastHit2D _ = Raycast(GetPointInsideBounds(from), direction, distance, 0, out Vector2[] inters);
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

    public static void GizmosDrawHitboxes()
    {
        Gizmos.color = Color.blue;
        Hitbox.GizmosDraw(mapHitbox);
    }

#endif

    #endregion
}
