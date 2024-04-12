#if UNITY_EDITOR

using UnityEngine;
using System.Collections.Generic;
using Collision2D;
using Collider2D = Collision2D.Collider2D;
using System;

namespace Test
{
    public class Collision2DTest : MonoBehaviour
    {
        private enum CollisionType
        {
            detection, intersection, intersectionAndNormal
        }

        private Color[] colorsColliders;
        private Color[] colorsLines;
        private Color[] colorsRays;
        private Color[] colorsST;
        private List<Vector2> inters;
        private List<Vector2> intersLine;
        private List<Vector2> normals1;
        private List<Vector2> normals2;
        private List<Vector2> normalsLine;

        [SerializeField] private UnityEngine.Collider2D[] colliders;
        [SerializeField] private GameObject followCollider;
        [SerializeField] private Line[] lines;
        [SerializeField] private Line[] rays;
        [SerializeField] private Line[] straightLines;
        [SerializeField] private Color colorTouch, colorUntouch, colorInter, colorNormal1, colorNormal2;
        [SerializeField] private CollisionType collisionType;
        [SerializeField] private InputKey turnLeft, turnRight;
        [SerializeField] private float rotationSpeed;
        [SerializeField] private bool enableColliderCollision = true;
        [SerializeField] private bool enableStraightLineCollision = true;
        [SerializeField] private bool enableRayCollision = true;
        [SerializeField] private bool enableLineCollision = true;
        [SerializeField] private bool enableFollowColliderCollision = true;

        [Header("Closest Point")]
        [SerializeField] private bool testClosestPointContainNormalAndDistance = false;
        [SerializeField] UnityEngine.Collider2D closestPointCollider;

        private void Update()
        {
            if (testClosestPointContainNormalAndDistance)
            {
                Collider2D collider = Collider2D.FromUnityCollider2D(closestPointCollider);
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(InputManager.mousePosition);
                Vector2 closestPoint = collider.ClosestPoint(mousePos);
                Color color = collider.Contains(mousePos) ? colorTouch : colorUntouch;
                DrawCollider(collider, color);
                Circle.GizmosDraw(closestPoint, 0.08f, DrawLine);
                Circle.GizmosDraw(mousePos, 0.08f, DrawLine);
                Debug.Log("Distance from mouse psition : " + collider.Distance(mousePos));
                if (collider.Normal(closestPoint, out Vector2 n))
                {
                    DrawVector(closestPoint, n, colorNormal1);
                }

                void DrawLine(Vector3 star, Vector3 end)
                {
                    Debug.DrawLine(star, end, color);
                }

                return;
            }


            if (InputManager.GetKey(turnLeft))
            {
                followCollider.transform.Rotate(new Vector3(0f, 0f, -rotationSpeed * Time.deltaTime));
            }
            if (InputManager.GetKey(turnRight))
            {
                followCollider.transform.Rotate(new Vector3(0f, 0f, rotationSpeed * Time.deltaTime));
            }

            followCollider.transform.position = Camera.main.ScreenToWorldPoint(InputManager.mousePosition);
            inters = new List<Vector2>();
            intersLine = new List<Vector2>();
            normals1 = new List<Vector2>();
            normals2 = new List<Vector2>();
            normalsLine = new List<Vector2>();
            colorsColliders = new Color[this.colliders.Length];
            for (int i = 0; i < colorsColliders.Length; i++)
            {
                colorsColliders[i] = colorUntouch;
            }
            colorsLines = new Color[lines.Length];
            for (int i = 0; i < colorsLines.Length; i++)
            {
                colorsLines[i] = colorUntouch;
            }
            colorsRays = new Color[rays.Length];
            for (int i = 0; i < colorsRays.Length; i++)
            {
                colorsRays[i] = colorUntouch;
            }
            colorsST = new Color[straightLines.Length];
            for (int i = 0; i < colorsRays.Length; i++)
            {
                colorsRays[i] = colorUntouch;
            }

            Collider2D[] colliders = new Collider2D[this.colliders.Length];
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i] = Collider2D.FromUnityCollider2D(this.colliders[i]);
            }

            if (enableColliderCollision)
            {
                for (int i = 0; i < colliders.Length; i++)
                {
                    for (int j = i + 1; j < colliders.Length; j++)
                    {
                        if (!enableFollowColliderCollision)
                        {
                            UnityEngine.Collider2D unityCollider = followCollider.GetComponent<UnityEngine.Collider2D>();
                            if (this.colliders[i] == unityCollider || this.colliders[j] == unityCollider)
                            {
                                continue;
                            }
                        }

                        Vector2 inter, normal1, normal2;
                        if (collisionType == CollisionType.detection && Collider2D.Collide(colliders[i], colliders[j]))
                        {
                            colorsColliders[i] = colorsColliders[j] = colorTouch;
                        }
                        if (collisionType == CollisionType.intersection && Collider2D.Collide(colliders[i], colliders[j], out inter))
                        {
                            colorsColliders[i] = colorsColliders[j] = colorTouch;
                            inters.Add(inter);
                        }
                        if (collisionType == CollisionType.intersectionAndNormal && Collider2D.Collide(colliders[i], colliders[j], out inter, out normal1, out normal2))
                        {
                            colorsColliders[i] = colorsColliders[j] = colorTouch;
                            inters.Add(inter);
                            normals1.Add(normal1);
                            normals2.Add(normal2);
                        }
                    }
                }
            }

            if (enableLineCollision)
            {
                for (int i = 0; i < colliders.Length; i++)
                {
                    if (!enableColliderCollision && !enableFollowColliderCollision)
                    {
                        break;
                    }

                    Collider2D collider = colliders[i];
                    if (!enableColliderCollision)
                    {
                        collider = Collider2D.FromUnityCollider2D(followCollider.GetComponent<UnityEngine.Collider2D>());
                    }
                    for (int j = 0; j < lines.Length; j++)
                    {
                        if (!enableFollowColliderCollision)
                        {
                            UnityEngine.Collider2D unityCollider = followCollider.GetComponent<UnityEngine.Collider2D>();
                            if (this.colliders[i] == unityCollider || this.colliders[j] == unityCollider)
                            {
                                continue;
                            }
                        }

                        Vector2 inter, normal;
                        if (collisionType == CollisionType.detection && Collider2D.CollideLine(collider, lines[j].A, lines[j].B))
                        {
                            colorsColliders[i] = colorsLines[j] = colorTouch;
                        }
                        if (collisionType == CollisionType.intersection && Collider2D.CollideLine(collider, lines[j].A, lines[j].B, out inter))
                        {
                            colorsColliders[i] = colorsLines[j] = colorTouch;
                            intersLine.Add(inter);
                        }
                        if (collisionType == CollisionType.intersectionAndNormal && Collider2D.CollideLine(collider, lines[j].A, lines[j].B, out inter, out normal))
                        {
                            colorsColliders[i] = colorsLines[j] = colorTouch;
                            intersLine.Add(inter);
                            normalsLine.Add(normal);
                        }
                    }
                    if (!enableColliderCollision)
                        break;
                }

                for (int i = 0; i < lines.Length; i++)
                {
                    for (int j = i + 1; j < lines.Length; j++)
                    {
                        Vector2 inter;
                        if (collisionType == CollisionType.detection && Collider2D.CollideLines(lines[i].A, lines[i].B, lines[j].A, lines[j].B))
                        {
                            colorsLines[i] = colorsLines[j] = colorTouch;
                        }
                        if ((collisionType == CollisionType.intersection || collisionType == CollisionType.intersectionAndNormal) && Collider2D.CollideLines(lines[i].A, lines[i].B, lines[j].A, lines[j].B, out inter))
                        {
                            colorsLines[i] = colorsLines[j] = colorTouch;
                            intersLine.Add(inter);
                        }
                    }
                }

                if (enableStraightLineCollision)
                {
                    for (int i = 0; i < straightLines.Length; i++)
                    {
                        for (int j = 0; j < lines.Length; j++)
                        {
                            Vector2 inter;
                            if (collisionType == CollisionType.detection && Collider2D.CollideLineStraightLine(new Line2D(lines[j].A, lines[j].B), new StraightLine2D(straightLines[i].A, straightLines[i].B)))
                            {
                                colorsST[i] = colorsLines[j] = colorTouch;
                            }
                            if ((collisionType == CollisionType.intersection || collisionType == CollisionType.intersectionAndNormal) && Collider2D.CollideLineStraightLine(new Line2D(lines[j].A, lines[j].B), new StraightLine2D(straightLines[i].A, straightLines[i].B), out inter))
                            {
                                colorsST[i] = colorsLines[j] = colorTouch;
                                intersLine.Add(inter);
                            }
                        }
                    }
                }

                if (enableRayCollision)
                {
                    for (int i = 0; i < rays.Length; i++)
                    {
                        for (int j = 0; j < lines.Length; j++)
                        {
                            Vector2 inter;
                            if (collisionType == CollisionType.detection && Collider2D.CollideLineRay(new Line2D(lines[j].A, lines[j].B), new Collision2D.Ray2D(rays[i].A, rays[i].B)))
                            {
                                colorsRays[i] = colorsLines[j] = colorTouch;
                            }
                            if ((collisionType == CollisionType.intersection || collisionType == CollisionType.intersectionAndNormal) && Collider2D.CollideLineRay(new Line2D(lines[j].A, lines[j].B), new Collision2D.Ray2D(rays[i].A, rays[i].B), out inter))
                            {
                                colorsRays[i] = colorsLines[j] = colorTouch;
                                intersLine.Add(inter);
                            }
                        }
                    }
                }
            }

            if (enableRayCollision)
            {
                for (int i = 0; i < colliders.Length; i++)
                {
                    if (!enableColliderCollision && !enableFollowColliderCollision)
                    {
                        break;
                    }

                    Collider2D collider = colliders[i];
                    if (!enableColliderCollision)
                    {
                        collider = Collider2D.FromUnityCollider2D(followCollider.GetComponent<UnityEngine.Collider2D>());
                    }
                    for (int j = 0; j < rays.Length; j++)
                    {
                        if (!enableFollowColliderCollision)
                        {
                            UnityEngine.Collider2D unityCollider = followCollider.GetComponent<UnityEngine.Collider2D>();
                            if (this.colliders[i] == unityCollider || this.colliders[j] == unityCollider)
                            {
                                continue;
                            }
                        }

                        Vector2 inter, normal;
                        if (collisionType == CollisionType.detection && Collider2D.CollideRay(collider, rays[j].A, rays[j].B))
                        {
                            colorsColliders[i] = colorsRays[j] = colorTouch;
                        }
                        if (collisionType == CollisionType.intersection && Collider2D.CollideRay(collider, rays[j].A, rays[j].B, out inter))
                        {
                            colorsColliders[i] = colorsRays[j] = colorTouch;
                            intersLine.Add(inter);
                        }
                        if (collisionType == CollisionType.intersectionAndNormal && Collider2D.CollideRay(collider, rays[j].A, rays[j].B, out inter, out normal))
                        {
                            colorsColliders[i] = colorsRays[j] = colorTouch;
                            intersLine.Add(inter);
                            normalsLine.Add(normal);
                        }
                    }
                    if (!enableColliderCollision)
                        break;
                }

                for (int i = 0; i < rays.Length; i++)
                {
                    for (int j = i + 1; j < rays.Length; j++)
                    {
                        Vector2 inter;
                        if (collisionType == CollisionType.detection && Collider2D.CollideRays(rays[i].A, rays[i].B, rays[j].A, rays[j].B))
                        {
                            colorsRays[i] = colorsRays[j] = colorTouch;
                        }
                        if ((collisionType == CollisionType.intersection || collisionType == CollisionType.intersectionAndNormal) && Collider2D.CollideRays(rays[i].A, rays[i].B, rays[j].A, rays[j].B, out inter))
                        {
                            colorsRays[i] = colorsRays[j] = colorTouch;
                            intersLine.Add(inter);
                        }
                    }
                }

                if (enableStraightLineCollision)
                {
                    for (int i = 0; i < straightLines.Length; i++)
                    {
                        for (int j = 0; j < rays.Length; j++)
                        {
                            Vector2 inter;
                            if (collisionType == CollisionType.detection && Collider2D.CollideStraightLineRay(new StraightLine2D(straightLines[i].A, straightLines[i].B), new Collision2D.Ray2D(rays[j].A, rays[j].B)))
                            {
                                colorsST[i] = colorsRays[j] = colorTouch;
                            }
                            if ((collisionType == CollisionType.intersection || collisionType == CollisionType.intersectionAndNormal) &&
                                Collider2D.CollideStraightLineRay(new StraightLine2D(straightLines[i].A, straightLines[i].B), new Collision2D.Ray2D(rays[j].A, rays[j].B), out inter))
                            {
                                colorsST[i] = colorsRays[j] = colorTouch;
                                intersLine.Add(inter);
                            }
                        }
                    }
                }
            }

            if (enableStraightLineCollision)
            {
                for (int i = 0; i < colliders.Length; i++)
                {
                    if (!enableColliderCollision && !enableFollowColliderCollision)
                    {
                        break;
                    }
                    Collider2D collider = colliders[i];
                    if (!enableColliderCollision)
                    {
                        collider = Collider2D.FromUnityCollider2D(followCollider.GetComponent<UnityEngine.Collider2D>());
                    }
                    for (int j = 0; j < straightLines.Length; j++)
                    {
                        if (!enableFollowColliderCollision)
                        {
                            UnityEngine.Collider2D unityCollider = followCollider.GetComponent<UnityEngine.Collider2D>();
                            if (this.colliders[i] == unityCollider || this.colliders[j] == unityCollider)
                            {
                                continue;
                            }
                        }
                        Vector2 inter, normal;
                        if (collisionType == CollisionType.detection && Collider2D.CollideStraightLine(collider, straightLines[j].A, straightLines[j].B))
                        {
                            colorsColliders[i] = colorsST[j] = colorTouch;
                        }
                        if (collisionType == CollisionType.intersection && Collider2D.CollideStraightLine(collider, straightLines[j].A, straightLines[j].B, out inter))
                        {
                            colorsColliders[i] = colorsST[j] = colorTouch;
                            intersLine.Add(inter);
                        }
                        if (collisionType == CollisionType.intersectionAndNormal && Collider2D.CollideStraightLine(collider, straightLines[j].A, straightLines[j].B, out inter, out normal))
                        {
                            colorsColliders[i] = colorsST[j] = colorTouch;
                            intersLine.Add(inter);
                            normalsLine.Add(normal);
                        }
                    }
                    if (!enableColliderCollision)
                        break;
                }

                for (int i = 0; i < straightLines.Length; i++)
                {
                    for (int j = i + 1; j < straightLines.Length; j++)
                    {
                        Vector2 inter;
                        if (collisionType == CollisionType.detection && Collider2D.CollideStraightLines(new StraightLine2D(straightLines[i].A, straightLines[i].B), new StraightLine2D(straightLines[j].A, straightLines[j].B)))
                        {
                            colorsST[i] = colorsST[j] = colorTouch;
                        }
                        if ((collisionType == CollisionType.intersection || collisionType == CollisionType.intersectionAndNormal) &&
                            Collider2D.CollideStraightLines(new StraightLine2D(straightLines[i].A, straightLines[i].B), new StraightLine2D(straightLines[j].A, straightLines[j].B), out inter))
                        {
                            colorsST[i] = colorsST[j] = colorTouch;
                            intersLine.Add(inter);
                        }
                    }
                }
            }

            if (enableColliderCollision)
            {
                for (int i = 0; i < colliders.Length; i++)
                {
                    DrawCollider(colliders[i], colorsColliders[i]);
                }
            }
            else if (enableFollowColliderCollision)
            {
                UnityEngine.Collider2D colUnity = followCollider.GetComponent<UnityEngine.Collider2D>();
                Collider2D collider = Collider2D.FromUnityCollider2D(colUnity);
                DrawCollider(collider, colorsColliders[0]);
            }

            if (enableLineCollision)
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    Debug.DrawLine(lines[i].A, lines[i].B, colorsLines[i]);
                }
            }

            if (enableRayCollision)
            {
                for (int i = 0; i < rays.Length; i++)
                {
                    Debug.DrawLine(rays[i].A, rays[i].B, colorsRays[i]);
                }
            }

            if (enableStraightLineCollision)
            {
                for (int i = 0; i < straightLines.Length; i++)
                {
                    Vector2 dir = (straightLines[i].A - straightLines[i].B).normalized * 10000;
                    Debug.DrawLine(straightLines[i].A - dir, straightLines[i].B + dir, colorsST[i]);
                }
            }

            for (int i = 0; i < inters.Count; i++)
            {
                if (collisionType == CollisionType.detection)
                    break;

                if (collisionType == CollisionType.intersectionAndNormal)
                {
                    DrawVector(inters[i], normals1[i], colorNormal1);
                    DrawVector(inters[i], normals2[i], colorNormal2);
                }

                DrawCollider(new Circle(inters[i], 0.1f), colorTouch);
            }

            for (int i = 0; i < intersLine.Count; i++)
            {
                if (collisionType == CollisionType.detection)
                    break;

                if (collisionType == CollisionType.intersectionAndNormal)
                {
                    DrawVector(intersLine[i], normalsLine[i], colorNormal1);
                }

                DrawCollider(new Circle(intersLine[i], 0.1f), colorTouch);
            }
        }

        void DrawVector(in Vector2 from, in Vector2 dir, Color color)
        {
            Useful.GizmoDrawVector(from, dir, DrawLine);
            void DrawLine(Vector3 start, Vector3 end) => Debug.DrawLine(start, end, color);
        }

        void DrawCollider(Collider2D collider, Color color, bool gizmos = false)
        {
            Gizmos.color = color;
            if (collider is Hitbox hitbox)
            {
                Hitbox.GizmosDraw(hitbox, gizmos ? Gizmos.DrawLine : DrawLine);
            }
            else if (collider is Circle circle)
            {
                Circle.GizmosDraw(circle, gizmos ? Gizmos.DrawLine : DrawLine);
            }
            else if (collider is Capsule capsule)
            {
                Capsule.GizmosDraw(capsule, gizmos ? Gizmos.DrawLine : DrawLine);
            }
            else if (collider is Polygone poly)
            {
                Polygone.GizmosDraw(poly, gizmos ? Gizmos.DrawLine : DrawLine);
            }

            void DrawLine(Vector3 start, Vector3 end) => Debug.DrawLine(start, end, color);
        }

        [Serializable]
        private struct Line
        {
            public Vector2 A, B;
        }

        private void OnDrawGizmos()
        {
            if (enableColliderCollision)
            {
                foreach (UnityEngine.Collider2D collider in colliders)
                {
                    Collider2D col = Collider2D.FromUnityCollider2D(collider);
                    DrawCollider(col, colorUntouch, true);
                }
            }

            Gizmos.color = colorUntouch;

            if (enableLineCollision)
            {
                foreach (Line line in lines)
                {
                    Gizmos.DrawLine(line.A, line.B);
                    Circle.GizmosDraw(line.A, 0.08f);
                    Circle.GizmosDraw(line.B, 0.08f);
                }
            }

            if (enableRayCollision)
            {
                foreach (Line line in rays)
                {
                    Gizmos.DrawLine(line.A, line.B);
                    Circle.GizmosDraw(line.B, 0.08f);
                }
            }

            if (enableStraightLineCollision)
            {
                foreach (Line line in straightLines)
                {
                    StraightLine2D.GizmosDraw(line.A, line.B);
                    Circle.GizmosDraw(line.A, 0.08f);
                    Circle.GizmosDraw(line.B, 0.08f);
                }
            }
        }
    }
}

#endif