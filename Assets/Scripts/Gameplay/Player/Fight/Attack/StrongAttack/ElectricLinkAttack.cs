using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Collision2D;
using Collider2D = UnityEngine.Collider2D;

public class ElectricLinkAttack : StrongAttack
{
    private ElectricLink currentLink;
    private LayerMask charMask;
    private bool isLinking;
    private Action callbackEnableOtherAttack, callbackEnableThisAttack;
    private ElectricBallAttack electricBallAttack;
    private float lineVisualToricDistance = 3f;

    [SerializeField] private float arcDuration = 0.5f;
    [SerializeField] private float electricBallExplosionRadius = 1f;
    [SerializeField] private float electricBallExplosionForce = 1f;
    [SerializeField] private bool instanciateOneLineRendererPerLink = true;
    [SerializeField] private LineRenderer linkPrefabs;

    protected override void Awake()
    {
        base.Awake();
        electricBallAttack = GetComponent<ElectricBallAttack>();
        charMask = LayerMask.GetMask("Char");
    }

    protected override void Update()
    {
        base.Update();

        if(isLinking)
        {
            foreach (Line2D link in currentLink.collisionLine)
            {
                ToricRaycastHit2D[] raycasts = PhysicsToric.RaycastAll(link.A, link.B - link.A, link.A.Distance(link.B) * 0.99f, charMask);
                foreach (ToricRaycastHit2D raycast in raycasts)
                {
                    if(raycast.collider == null || !raycast.collider.CompareTag("Char")) 
                        continue;

                    GameObject player = raycast.collider.GetComponent<ToricObject>().original;
                    uint id = player.GetComponent<PlayerCommon>().id;
                    List<uint> charAlreadyTouch = currentLink.charAlreadyTouch[link];
                    if (playerCommon.id != id && !charAlreadyTouch.Contains(id))
                    {
                        charAlreadyTouch.Add(id);
                        base.OnTouchEnemy(player, damageType);
                    }
                }
            }
        }
    }

    public override bool Launch(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
        List<ElectricBall> electricBalls = electricBallAttack.currentBalls;
        if (!cooldown.isActive || electricBalls.Count <= 1)
        {
            callbackEnableOtherAttack.Invoke();
            callbackEnableThisAttack.Invoke();
            return false;
        }

        this.callbackEnableOtherAttack = callbackEnableOtherAttack;
        this.callbackEnableThisAttack = callbackEnableThisAttack;
        StartCoroutine(EndAttackCorout());
        isLinking = true;

        CreateLink();

        return true;
    }

    private void CreateLink()
    {
        List<LineRenderer> CreateRenderer()
        {
            List<ElectricBall> electricBalls = electricBallAttack.currentBalls;
            Vector2 start, end, dir, oldPoint;
            float distance;
            Vector2[] inters;
            Vector3[] lineRendererPoints;
            LineRenderer lineRenderer;
            int i, j;
            List<LineRenderer> res = new List<LineRenderer>();
            if (electricBalls.Count == 2)
            {
                start = electricBalls[0].transform.position;
                end = electricBalls[1].transform.position;
                (dir, distance) = PhysicsToric.DirectionAndDistance(start, end);
                inters = PhysicsToric.RaycastToricIntersections(start, dir, distance);
                if (inters.Length <= 0)
                {
                    lineRenderer = Instantiate(linkPrefabs, Vector3.zero, Quaternion.identity, CloneParent.cloneParent);
                    lineRendererPoints = new Vector3[2]
                    {
                        start, end
                    };
                    lineRenderer.ResetPositions(lineRendererPoints);
                    res.Add(lineRenderer);
                    return res;
                }

                oldPoint = start;
                for (i = 0; i < inters.Length; i++)
                {
                    lineRenderer = Instantiate(linkPrefabs, Vector3.zero, Quaternion.identity, CloneParent.cloneParent);
                    Vector2 cache = (inters[i] - oldPoint).normalized * lineVisualToricDistance;
                    lineRendererPoints = new Vector3[2]
                    {
                        oldPoint, inters[i] + cache
                    };
                    lineRenderer.ResetPositions(lineRendererPoints);
                    res.Add(lineRenderer);
                    oldPoint = PhysicsToric.GetComplementaryPoint(inters[i]) - cache;
                }

                lineRenderer = Instantiate(linkPrefabs, Vector3.zero, Quaternion.identity, CloneParent.cloneParent);
                lineRendererPoints = new Vector3[2]
                {
                    oldPoint, end
                };
                lineRenderer.ResetPositions(lineRendererPoints);
                res.Add(lineRenderer);
                return res;
            }

            if(instanciateOneLineRendererPerLink)
            {
                for (i = 0; i < electricBalls.Count; i++)
                {
                    start = electricBalls[i].transform.position;
                    end = electricBalls[(i + 1) % electricBalls.Count].transform.position;
                    (dir, distance) = PhysicsToric.DirectionAndDistance(start, end);
                    inters = PhysicsToric.RaycastToricIntersections(start, dir, distance);

                    if (inters.Length <= 0)
                    {
                        lineRenderer = Instantiate(linkPrefabs, Vector3.zero, Quaternion.identity, CloneParent.cloneParent);
                        lineRendererPoints = new Vector3[2]
                        {
                            start, end
                        };
                        lineRenderer.ResetPositions(lineRendererPoints);
                        res.Add(lineRenderer);
                    }
                    else
                    {
                        oldPoint = start;
                        for (j = 0; j < inters.Length; j++)
                        {
                            lineRenderer = Instantiate(linkPrefabs, Vector3.zero, Quaternion.identity, CloneParent.cloneParent);
                            Vector2 cache = (inters[j] - oldPoint).normalized * lineVisualToricDistance;
                            lineRendererPoints = new Vector3[2]
                            {
                                oldPoint, inters[j] + cache
                            };
                            lineRenderer.ResetPositions(lineRendererPoints);
                            res.Add(lineRenderer);
                            oldPoint = PhysicsToric.GetComplementaryPoint(inters[j]) - cache;
                        }

                        lineRenderer = Instantiate(linkPrefabs, Vector3.zero, Quaternion.identity, CloneParent.cloneParent);
                        lineRendererPoints = new Vector3[2]
                        {
                            oldPoint, end
                        };
                        lineRenderer.ResetPositions(lineRendererPoints);
                        res.Add(lineRenderer);

                    }
                }

                return res;
            }

            List<Vector2> currentPoints = new List<Vector2>(electricBalls.Count);
            for (i = 0; i < electricBalls.Count; i++)
            {
                start = electricBalls[i].transform.position;
                end = electricBalls[(i + 1) % electricBalls.Count].transform.position;
                (dir, distance) = PhysicsToric.DirectionAndDistance(start, end);
                inters = PhysicsToric.RaycastToricIntersections(start, dir, distance);

                if (inters.Length <= 0)
                {
                    if(currentPoints.Count <= 1)
                    {
                        currentPoints.Add(start);
                        currentPoints.Add(end);
                    }
                    else
                    {
                        currentPoints.Add(end);
                    }
                }
                else
                {
                    if (currentPoints.Count <= 0)
                    {
                        currentPoints.Add(start);
                    }

                    currentPoints.Add(inters[0] + (dir * lineVisualToricDistance));
                    lineRenderer = Instantiate(linkPrefabs, Vector3.zero, Quaternion.identity, CloneParent.cloneParent);
                    lineRendererPoints = new Vector3[currentPoints.Count];
                    for (j = 0; j < currentPoints.Count; j++)
                    {
                        lineRendererPoints[j] = currentPoints[j];
                    }
                    lineRenderer.ResetPositions(lineRendererPoints);
                    res.Add(lineRenderer);
                    currentPoints.Clear();

                    oldPoint = PhysicsToric.GetComplementaryPoint(inters[0]) - (dir * lineVisualToricDistance);

                    if (inters.Length == 1)
                    {
                        currentPoints.Add(oldPoint);
                        continue;
                    }

                    for (j = 0; j < inters.Length - 1; j++)
                    {
                        lineRenderer = Instantiate(linkPrefabs, Vector3.zero, Quaternion.identity, CloneParent.cloneParent);
                        Vector2 cache = (inters[j] - oldPoint).normalized * lineVisualToricDistance;
                        lineRendererPoints = new Vector3[2]
                        {
                            oldPoint, inters[j] + cache
                        };
                        lineRenderer.ResetPositions(lineRendererPoints);
                        res.Add(lineRenderer);
                        oldPoint = PhysicsToric.GetComplementaryPoint(inters[j]) - cache;
                    }

                    currentPoints.Add(oldPoint);
                    currentPoints.Add(end);
                }
            }

            if(currentPoints.Count > 0)
            {
                lineRenderer = Instantiate(linkPrefabs, Vector3.zero, Quaternion.identity, CloneParent.cloneParent);
                lineRendererPoints = new Vector3[currentPoints.Count];
                for (j = 0; j < currentPoints.Count; j++)
                {
                    lineRendererPoints[j] = currentPoints[j];
                }
                lineRenderer.ResetPositions(lineRendererPoints);
                res.Add(lineRenderer);
                currentPoints.Clear();
            }

            return res;
        }

        List<Line2D> CreateLinkCollision()
        {
            List<ElectricBall> electricBalls = electricBallAttack.currentBalls;
            Vector2 start, end, dir;
            float distance;
            Vector2[] inters;
            int i, j;
            List<Line2D> res = new List<Line2D>();
            if (electricBalls.Count == 2)
            {
                start = electricBalls[0].transform.position;
                end = electricBalls[1].transform.position;
                (dir, distance) = PhysicsToric.DirectionAndDistance(start, end);
                inters = PhysicsToric.RaycastToricIntersections(start, dir, distance);
                if (inters.Length <= 0)
                {
                    res.Add(new Line2D(start, end));
                    return res;
                }

                Vector2 oldPoint = start;
                for (i = 0; i < inters.Length; i++)
                {
                    res.Add(new Line2D(oldPoint, inters[i]));
                    oldPoint = PhysicsToric.GetComplementaryPoint(inters[i]);
                }

                res.Add(new Line2D(oldPoint, end));
                return res;
            }

            for (i = 0; i < electricBalls.Count; i++)
            {
                start = electricBalls[i].transform.position;
                end = electricBalls[(i + 1) % electricBalls.Count].transform.position;
                (dir, distance) = PhysicsToric.DirectionAndDistance(start, end);
                inters = PhysicsToric.RaycastToricIntersections(start, dir, distance);

                if(inters.Length <= 0)
                {
                    res.Add(new Line2D(start, end));
                }
                else
                {
                    Vector2 oldPoint = start;
                    for (j = 0; j < inters.Length; j++)
                    {
                        res.Add(new Line2D(oldPoint, inters[j]));
                        oldPoint = PhysicsToric.GetComplementaryPoint(inters[j]);
                    }
                    res.Add(new Line2D(oldPoint, end));
                }
            }

            return res;
        }

        List<ElectricBall> electricBalls = electricBallAttack.currentBalls;
        currentLink = new ElectricLink();

        List<Line2D> colLines = CreateLinkCollision();
        currentLink.collisionLine = colLines;

        Dictionary<Line2D, List<uint>> charAlreadyTouch = new Dictionary<Line2D, List<uint>>(colLines.Count);
        foreach (Line2D line in colLines)
        {
            charAlreadyTouch.Add(line, new List<uint>());
        }
        currentLink.charAlreadyTouch = charAlreadyTouch;

        List<LineRenderer> lineRenderer = CreateRenderer();
        currentLink.lineRenderers = lineRenderer;

        for (int i = 0; i < electricBalls.Count ; i++)
        {
            electricBalls[i].StartLinking();
        }
    }

    private void HandleElectricBallExplosion(ElectricBall electricBall)
    {
        Vector2 explosionPosition = electricBall.transform.position;
        ExplosionManager.instance.CreateExplosion(explosionPosition, electricBallExplosionForce);

        Collider2D[] cols = PhysicsToric.OverlapCircleAll(explosionPosition, electricBallExplosionRadius, charMask);
        foreach (Collider2D col in cols)
        {
            if(col.CompareTag("Char"))
            {
                GameObject player = col.GetComponent<ToricObject>().original;
                uint playerId = player.GetComponent<PlayerCommon>().id;
                if(playerId != playerCommon.id)
                {
                    base.OnTouchEnemy(player, damageType);
                }
            }
        }
    }

    private IEnumerator EndAttackCorout()
    {
        yield return PauseManager.instance.Wait(arcDuration);

        currentLink = null;
        isLinking = false;
        foreach (ElectricBall electricBall in electricBallAttack.currentBalls)
        {
            HandleElectricBallExplosion(electricBall);
            electricBall.EndLinking();
        }

        callbackEnableOtherAttack.Invoke();
        callbackEnableThisAttack.Invoke();
        cooldown.Reset();
    }

    #region OnValidate

#if UNITY_EDITOR

    protected override void OnValidate()
    {
        base.OnValidate();
        arcDuration = Mathf.Max(0f, arcDuration);
        electricBallExplosionForce = Mathf.Max(0f, electricBallExplosionForce);
        electricBallExplosionRadius = Mathf.Max(0f, electricBallExplosionRadius);
    }

#endif

    #endregion

    #region Private Struct

    private class ElectricLink
    {
        public Dictionary<Line2D, List<uint>> charAlreadyTouch;
        public List<Line2D> collisionLine;
        public List<LineRenderer> lineRenderers;

        public ElectricLink()
        {
            charAlreadyTouch = new Dictionary<Line2D, List<uint>>();
            collisionLine = new List<Line2D>();
            lineRenderers = new List<LineRenderer>();
        }
    }

    #endregion

}
