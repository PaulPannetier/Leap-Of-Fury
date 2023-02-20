using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BounceShotAttack : StrongAttack
{
    private Movement movement;
    private GameObject goLineRendererToDuplicate;
    private BounceShotMagicAttackData data;
    private int maxBounce;
    private float speed;
    private bool isBouncing = false;

    [SerializeField] private GameObject goLinesRendererParent;
    [SerializeField] private float initSpeed = 7f;
    [SerializeField] private float duration = 2f;
    [SerializeField] private bool useMaxTotalDistance = false;
    [SerializeField] private float maxTotalDistance = 5f;
    [SerializeField] private float minDistBetween2Points = 0.1f;
    [SerializeField] private int initMaxBounce = 1;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask playerMask;
    [SerializeField] [Range(0f, 1f)] private float detectionTolerance = 0.001f;

    protected override void Awake()
    {
        base.Awake();
        movement = GetComponent<Movement>();
        goLineRendererToDuplicate = goLinesRendererParent.transform.GetChild(0).gameObject;
    }

    protected override void Start()
    {
        base.Start();
        maxBounce = initMaxBounce;
        speed = initSpeed;
        data = new BounceShotMagicAttackData(Vector2.zero, null, null, 0f, 0, new List<LineRenderer> { goLinesRendererParent.transform.GetChild(0).GetComponent<LineRenderer>() });
    }

    public override bool Launch(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
        if (isBouncing || !cooldown.isActive)
        {
            callbackEnableOtherAttack.Invoke();
            callbackEnableThisAttack.Invoke();
            return false;
        }
        base.Launch(callbackEnableOtherAttack, callbackEnableThisAttack);
        cooldown.Reset();
        StartCoroutine(DoBounceAttack(callbackEnableOtherAttack, callbackEnableThisAttack));
        return true;
    }

    #region CalculateBounce

    private void CalculateBounce(in float maxDistance)
    {
        Vector2 oldPoint;
        if (data.rayPoints[data.rayPoints.Count - 2] != null)
        {
            data.rayPoints.RemoveAt(data.rayPoints.Count - 1);
            oldPoint = data.rayPoints[data.rayPoints.Count - 1];
        }
        else
        {
            oldPoint = data.rayPoints[data.rayPoints.Count - 1];
        }
        RaycastHit2D raycast;

        while (data.nbBounce <= maxBounce)
        {
            raycast = Physics2D.Raycast(oldPoint, data.lastDirection, maxDistance - data.totalDist, groundMask);

            if (raycast.collider != null)
            {
                if (PhysicsToric.cameraHitbox.Contains(raycast.point))
                {
                    data.totalDist += raycast.distance;
                    if (raycast.normal == Vector2.up || raycast.normal == Vector2.down)
                    {
                        data.lastDirection = new Vector2(data.lastDirection.x, -data.lastDirection.y);
                    }
                    else if (raycast.normal == Vector2.left || raycast.normal == Vector2.right)
                    {
                        data.lastDirection = new Vector2(-data.lastDirection.x, data.lastDirection.y);
                    }
                    else
                    {
                        //Version générale bugger
                        Vector2 sim = Droite.Symetric(oldPoint, new Droite(raycast.point, raycast.point + raycast.normal));
                        data.lastDirection = (sim - raycast.point).normalized;
                    }

                    oldPoint = raycast.point + data.lastDirection * detectionTolerance;
                    data.rayPoints.Add(raycast.point + data.lastDirection * detectionTolerance);
                    data.nbBounce++;
                }
                else
                {
                    Line ray = new Line(oldPoint, raycast.point);
                    if (CustomCollider.CollideHitboxLine(PhysicsToric.cameraHitbox, ray, out Vector2 edgePoint))
                    {
                        data.rayPoints.Add(edgePoint);
                        data.totalDist += oldPoint.Distance(edgePoint);
                        while (PhysicsToric.cameraHitbox.Contains(edgePoint))
                        {
                            float ox = edgePoint.x < 0f ? -detectionTolerance : detectionTolerance;
                            float oy = edgePoint.y < 0f ? -detectionTolerance : detectionTolerance;
                            edgePoint = new Vector2(edgePoint.x + ox, edgePoint.y + oy);
                        }
                        Vector2 edgePoint2 = PhysicsToric.GetPointInsideBounds(edgePoint);
                        data.rayPoints.Add(null);
                        data.rayPoints.Add(edgePoint2);
                        oldPoint = edgePoint2;
                    }
                    else
                    {
                        Debug.LogWarning("Debug svp");
                    }
                }
            }
            else
            {
                Vector2 endPoint = oldPoint + data.lastDirection * (maxDistance - data.totalDist);
                if (PhysicsToric.cameraHitbox.Contains(endPoint))
                {
                    data.rayPoints.Add(endPoint);
                    break;
                }
                else
                {
                    Line ray = new Line(oldPoint, endPoint);
                    if (CustomCollider.CollideHitboxLine(PhysicsToric.cameraHitbox, ray, out Vector2 edgePoint))
                    {
                        data.rayPoints.Add(edgePoint);
                        data.totalDist += oldPoint.Distance(edgePoint);
                        while (PhysicsToric.cameraHitbox.Contains(edgePoint))
                        {
                            float ox = edgePoint.x < 0f ? -detectionTolerance : detectionTolerance;
                            float oy = edgePoint.y < 0f ? -detectionTolerance : detectionTolerance;
                            edgePoint = new Vector2(edgePoint.x + ox, edgePoint.y + oy);
                        }
                        Vector2 edgePoint2 = PhysicsToric.GetPointInsideBounds(edgePoint);
                        data.rayPoints.Add(null);
                        data.rayPoints.Add(edgePoint2);
                        oldPoint = edgePoint2;
                    }
                    else
                    {
                        Debug.LogWarning("Debug pls");
                    }
                }
            }
        }

        data.lrPoints = new List<List<Vector3>> { new List<Vector3>() };
        int lrIndex = 0;
        for (int i = 0; i < data.rayPoints.Count; i++)
        {
            if (data.rayPoints[i] == null)
            {
                lrIndex++;
                data.lrPoints.Add(new List<Vector3>());
            }
            else
            {
                data.lrPoints[lrIndex].Add(data.rayPoints[i]);
            }
        }

        if (data.lineRenders.Count != data.lrPoints.Count)
        {
            if (data.lineRenders.Count < data.lrPoints.Count)
            {
                for (int i = data.lineRenders.Count; i < data.lrPoints.Count; i++)
                {
                    GameObject go = Instantiate(goLineRendererToDuplicate, goLinesRendererParent.transform);
                    go.name = "Line " + i;
                    data.lineRenders.Add(go.GetComponent<LineRenderer>());
                }
            }
            else
            {
                for (int i = data.lineRenders.Count - 1; i >= data.lrPoints.Count; i--)
                {
                    Destroy(data.lineRenders[i].gameObject);
                    data.lineRenders.RemoveAt(data.lineRenders.Count - 1);
                }
            }
        }

        for (int i = 0; i < data.lrPoints.Count; i++)
        {
            data.lineRenders[i].positionCount = data.lrPoints[i].Count;
            data.lineRenders[i].SetPositions(data.lrPoints[i].ToArray());
        }
    }

    private void RemoveBounce()
    {
        for (int i = 0; i < data.lrPoints.Count; i++)
        {
            data.lineRenders[i].positionCount = 0;
            data.lineRenders[i].SetPositions(new Vector3[0] { });
        }
    }

    #endregion

    private bool BounceIsLooping()
    {
        for (int i = 0; i < data.lrPoints.Count; i++)
        {
            Vector3 p1 = data.lrPoints[i][data.lrPoints[i].Count - 1];
            Vector3 p2 = data.lrPoints[(i + 1) % data.lrPoints.Count][0];
            if(p1.SqrDistance(p2) <= minDistBetween2Points * minDistBetween2Points)
                return true;
        }
        return false;
    }

    private IEnumerator DoBounceAttack(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
        yield return Useful.GetWaitForSeconds(castDuration);
        callbackEnableOtherAttack.Invoke();
        callbackEnableThisAttack.Invoke();

        isBouncing = true;
        bool isTouchingAnEnnemy = false;
        float maxDistance = 0f;
        Vector2 startDir = movement.GetCurrentDirection();
        data.Clear();
        data.lastDirection = startDir;
        data.rayPoints.Add(transform.position);
        data.rayPoints.Add(transform.position);
        while ((!useMaxTotalDistance || maxDistance < maxTotalDistance) && data.nbBounce <= maxBounce)
        {
            maxDistance = useMaxTotalDistance ? Mathf.Min(maxTotalDistance, maxDistance + speed * Time.deltaTime) : maxDistance + speed * Time.deltaTime;
            CalculateBounce(maxDistance);
            isTouchingAnEnnemy = TouchOtherEnnemy(out GameObject ennemy);
            if (isTouchingAnEnnemy)
            {
                OnTouchEnemy(ennemy);
                break;
            }
            if(BounceIsLooping())
            {
                break;
            }
            yield return null;
        }

        if (isTouchingAnEnnemy)
        {
            RemoveBounce();
            data.Clear();
            isBouncing = false;
        }
        else
        {
            float time = Time.time;
            while(Time.time - time < duration)
            {
                isTouchingAnEnnemy = TouchOtherEnnemy(out GameObject ennemy);
                if (isTouchingAnEnnemy)
                {
                    OnTouchEnemy(ennemy);
                    break;
                }
                yield return null;
            }
            RemoveBounce();
            data.Clear();
            isBouncing = false;
        }
    }

    #region BounceData

    private struct BounceShotMagicAttackData
    {
        public Vector2 lastDirection;
        public List<LinePoint> rayPoints;
        public float totalDist;
        public int nbBounce;
        public List<LineRenderer> lineRenders;
        public List<List<Vector3>> lrPoints;

        public BounceShotMagicAttackData(in Vector2 lastDirection, List<LinePoint> rayPoints, List<List<Vector3>> lrPoints, in float totalDist, in int nbBounce, List<LineRenderer> lineRenders)
        {
            this.lastDirection = lastDirection;
            this.rayPoints = rayPoints;
            this.lrPoints = lrPoints;
            this.totalDist = totalDist;
            this.nbBounce = nbBounce;
            this.lineRenders = lineRenders;
        }

        public void Clear()
        {
            this.lastDirection = Vector2.zero;
            this.rayPoints = new List<LinePoint>();
            this.lrPoints = new List<List<Vector3>>();
            this.totalDist = 0f;
            this.nbBounce = 0;
        }
    }

    [Serializable]
    private class LinePoint
    {
        public Vector3 point;

        public LinePoint(in Vector2 point)
        {
            this.point = point;
        }
        public LinePoint(in Vector3 point)
        {
            this.point = point;
        }

        public override string ToString() => point.ToString();

        //cast
        public static implicit operator LinePoint(Vector2 v) => new LinePoint(v);
        public static implicit operator Vector2(LinePoint l) => l.point;
        public static implicit operator LinePoint(Vector3 v) => new LinePoint(v);
        public static implicit operator Vector3(LinePoint l) => l.point;
    }

    #endregion

    private bool TouchOtherEnnemy(out GameObject otherPlayer)
    {
        foreach (List<Vector3> points in data.lrPoints)
        {
            RaycastHit2D raycast;
            Vector3 oldPoint = points[0];
            for (int i = 1; i < points.Count; i++)
            {
                Vector3 newPoint = points[i];
                raycast = Physics2D.Raycast(oldPoint, newPoint - oldPoint, oldPoint.Distance(newPoint), playerMask);
                if (raycast.collider != null && raycast.collider.CompareTag("Char"))
                {
                    otherPlayer = raycast.collider.GetComponent<ToricObject>().original;
                    if (otherPlayer.GetComponent<PlayerCommon>().id != playerCommon.id)
                    {
                        return true;
                    }
                }
                oldPoint = newPoint;
            }
        }
        otherPlayer = null;
        return false;
    }

    public void PickUpFiole(Fiole fiole)
    {
        maxBounce++;
    }
}
