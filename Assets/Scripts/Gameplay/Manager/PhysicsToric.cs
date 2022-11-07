using UnityEngine;

public static class PhysicsToric
{
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

        return res;
    }

    public static Collider2D OverlapBox(in Vector2 point, in Vector2 size, in float angle, in LayerMask layerMask)
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
            return Physics2D.OverlapBox(hitbox.center, size, angle, layerMask);
        }

        Collider2D res = Physics2D.OverlapBox(hitbox.center, size, angle, layerMask);
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
                res = Physics2D.OverlapBox(hitbox.center, size, angle, layerMask);
                if (res != null)
                    return res;
                hitbox.MoveAt(hitbox.center + cameraHitboxArounds[i].center);
            }
        }
        return res;
    }

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
            return Physics2D.OverlapBoxAll(hitbox.center, size, angle, layerMask);
        }

        Collider2D[] res = Physics2D.OverlapBoxAll(hitbox.center, size, angle, layerMask);
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
                Collider2D[] res2 = Physics2D.OverlapBoxAll(hitbox.center, size, angle, layerMask);
                if (res2 != null && res2.Length > 0)
                    res = res.Merge(res2);
                hitbox.MoveAt(hitbox.center + cameraHitboxArounds[i].center);
            }
        }
        return res;
    }

    public static Collider2D OverlapCapsule(in Vector2 center, in Vector2 size, float angle, in LayerMask layerMask) => OverlapCapsule(new Capsule(center, size), angle, layerMask);

    public static Collider2D OverlapCapsule(Capsule capsule, float angle, in LayerMask layerMask)
    {
        Capsule c = (Capsule)capsule.Clone();
        if (Mathf.Abs(angle) > 1e-5f)
            c.Rotate(angle);

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

        if (containAll)//ez case
        {
            return Physics2D.OverlapCapsule(c.center, new Vector2(c.hitbox.width, c.hitbox.height), c.direction, angle, layerMask);
        }

        Collider2D res = Physics2D.OverlapCapsule(c.center, new Vector2(c.hitbox.width, c.hitbox.height), c.direction, angle, layerMask);
        if (res != null)
            return res;

        for (int i = 0; i < 4; i++)
        {
            if (collideWithCamHitbox[i])
            {
                c.MoveAt(c.center - cameraHitboxArounds[i].center);
                res = Physics2D.OverlapCapsule(c.center, new Vector2(c.hitbox.width, c.hitbox.height), c.direction, angle, layerMask);
                if (res != null)
                    return res;
                c.MoveAt(c.center + cameraHitboxArounds[i].center);
            }
        }

        return null;
    }

    public static Collider2D[] OverlapCapsuleAll(in Vector2 center, in Vector2 size, float angle, in LayerMask layerMask) => OverlapCapsuleAll(new Capsule(center, size), angle, layerMask);

    public static Collider2D[] OverlapCapsuleAll(Capsule capsule, float angle, in LayerMask layerMask)
    {
        Capsule c = (Capsule)capsule.Clone();
        if (Mathf.Abs(angle) > 1e-5f)
            c.Rotate(angle);

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

        if (containAll)//ez case
        {
            return Physics2D.OverlapCapsuleAll(c.center, new Vector2(c.hitbox.width, c.hitbox.height), c.direction, angle, layerMask);
        }

        Collider2D[] res = Physics2D.OverlapCapsuleAll(c.center, new Vector2(c.hitbox.width, c.hitbox.height), c.direction, angle, layerMask);

        for (int i = 0; i < 4; i++)
        {
            if (collideWithCamHitbox[i])
            {
                c.MoveAt(c.center - cameraHitboxArounds[i].center);
                Collider2D[] res2 = Physics2D.OverlapCapsuleAll(c.center, new Vector2(c.hitbox.width, c.hitbox.height), c.direction, angle, layerMask);
                if (res2 != null && res2.Length > 0)
                    res = res.Merge(res2);
                c.MoveAt(c.center + cameraHitboxArounds[i].center);
            }
        }
        return res;
    }

    public static RaycastHit2D Raycast(in Vector2 from, in Vector2 direction, in float distance, in int layerMask)
    {
        float reachDistance = 0f;
        RaycastHit2D raycast = RaycastRecur(GetPointInsideBounds(from), direction.normalized, distance, layerMask, ref reachDistance);
        raycast.distance = raycast.collider != null ? reachDistance : 0f;
        return raycast;
    }

    private static RaycastHit2D RaycastNormalizeDir(in Vector2 from, in Vector2 direction, in float distance, in int layerMask)
    {
        float reachDistance = 0f;
        RaycastHit2D raycast = RaycastRecur(from, direction, distance, layerMask, ref reachDistance);
        raycast.distance = raycast.collider != null ? reachDistance : 0f;
        return raycast;
    }

    private static RaycastHit2D RaycastRecur(Vector2 from, Vector2 direction, in float distance, in int layerMask, ref float reachDistance)
    {
        RaycastHit2D raycast = Physics2D.Raycast(from, direction, distance, layerMask);
        Line ray;

        if(raycast.collider == null)
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
                    cp = GetPointInsideBounds(cp);
                    return RaycastRecur(cp, direction, distance - tmpDist, layerMask, ref reachDistance);
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
        if(cameraHitbox.Contains(raycast.point))
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
            cp2 = GetPointInsideBounds(cp2);
            return RaycastRecur(cp2, direction, distance - tmpDist, layerMask, ref reachDistance);
        }
        else
        {
            Debug.LogWarning("Debug pls");
            reachDistance += raycast.distance;
            raycast.point = GetPointInsideBounds(raycast.point);
            return raycast;
        }
    }
}
