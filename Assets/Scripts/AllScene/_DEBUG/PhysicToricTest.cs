#if UNITY_EDITOR

using Collision2D;
using System;
using System.Linq;
using UnityEngine;
using Collider2D = Collision2D.Collider2D;

namespace Test
{
    public class PhysicToricTest : MonoBehaviour
    {
        private enum CastType
        {
            Overlap,
            Raycast,
            CircleCast
        }

        private enum CollisionType { first, all }

        private enum ToricIntersection { None, all }

        private bool[] collideColliders;

        [Header("General Settings")]
        [SerializeField] private LevelMapData levelMapData;
        [SerializeField] private UnityEngine.Collider2D[] colliders;
        [SerializeField] private LayerMask mask;
        [SerializeField] private CastType castType;
        [SerializeField] private CollisionType collisionType;
        [SerializeField] private Color colorTouch, colorUntouch, colorNormal, colorCentroid;

        [Header("Overlap Settings")]
        [SerializeField] private GameObject followCollider;

        [Header("Cast Settings")]
        [SerializeField] private float moveSpeed;
        [SerializeField] private InputKey setStartKey = InputKey.Mouse0;
        [SerializeField] private Vector2 start;
        [SerializeField] private ToricIntersection toricIntersection;
        [SerializeField] private bool useCastDistance;
        [SerializeField] private float castDistance;

        [Header("Circle cast Settings")]
        [SerializeField] private float castRadius;

        private Vector2 GetMousePos() => Useful.mainCamera.ScreenToWorldPoint(InputManager.mousePosition);

        private void Start()
        {
            PhysicsToric.ClearPriorityCollider();
            foreach (UnityEngine.Collider2D collider in colliders)
            {
                PhysicsToric.AddPriorityCollider(collider);
            }

            EventManager.instance.OnMapChanged(levelMapData);
        }

        private void Update()
        {
            collideColliders = new bool[colliders.Length];
            switch (castType)
            {
                case CastType.Overlap:
                    UpdateOverlap();
                    break;
                case CastType.Raycast:
                    UpdateRaycast();
                    break;
                case CastType.CircleCast:
                    UpdateCircleCast();
                    break;
                default:
                    break;
            }

            for (int i = 0; i < colliders.Length; i++)
            {
                DrawCollider(Collider2D.FromUnityCollider2D(colliders[i]), collideColliders[i] ? colorTouch : colorUntouch, false);
            }

            Circle.GizmosDraw(start, 0.1f, Debug.DrawLine);
        }

        private int GetColliderIndex(UnityEngine.Collider2D collider)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] == collider)
                {
                    return i;
                }
            }
            return -1;
        }

        private void UpdateOverlap()
        {
            Vector2 mousePos = GetMousePos();

            DrawCircle(PhysicsToric.GetPointInsideBounds(mousePos), 0.2f, colorUntouch, false);

            if (followCollider == null)
            {
                if (collisionType == CollisionType.first)
                {
                    UnityEngine.Collider2D col = PhysicsToric.OverlapPoint(mousePos, mask);
                    if (col != null)
                    {
                        collideColliders[GetColliderIndex(col)] = true;
                    }
                }
                if (collisionType == CollisionType.all)
                {
                    UnityEngine.Collider2D[] cols = PhysicsToric.OverlapPointAll(mousePos, mask);
                    foreach (UnityEngine.Collider2D col in cols)
                    {
                        collideColliders[GetColliderIndex(col)] = true;
                    }
                }
            }
            else
            {
                followCollider.transform.position = mousePos;
                followCollider.layer = mask + 1;

                Collider2D collider = Collider2D.FromUnityCollider2D(followCollider.GetComponent<UnityEngine.Collider2D>());
                if (collider.GetType() != typeof(Polygone))
                {
                    if (collider is Circle circle)
                    {
                        if (collisionType == CollisionType.first)
                        {
                            UnityEngine.Collider2D col = PhysicsToric.OverlapCircle(circle, mask);
                            if (col != null)
                            {
                                collideColliders[GetColliderIndex(col)] = true;
                            }
                        }
                        if (collisionType == CollisionType.all)
                        {
                            UnityEngine.Collider2D[] cols = PhysicsToric.OverlapCircleAll(circle, mask);
                            foreach (UnityEngine.Collider2D col in cols)
                            {
                                collideColliders[GetColliderIndex(col)] = true;
                            }
                        }
                    }
                    else if (collider is Hitbox hitbox)
                    {
                        if (collisionType == CollisionType.first)
                        {
                            UnityEngine.Collider2D col = PhysicsToric.OverlapBox(hitbox, mask);
                            if (col != null)
                            {
                                collideColliders[GetColliderIndex(col)] = true;
                            }
                        }
                        if (collisionType == CollisionType.all)
                        {
                            UnityEngine.Collider2D[] cols = PhysicsToric.OverlapBoxAll(hitbox, mask);
                            foreach (UnityEngine.Collider2D col in cols)
                            {
                                collideColliders[GetColliderIndex(col)] = true;
                            }
                        }
                    }
                    else if (collider is Capsule capsule)
                    {
                        if (collisionType == CollisionType.first)
                        {
                            UnityEngine.Collider2D col = PhysicsToric.OverlapCapsule(capsule, mask);
                            if (col != null)
                            {
                                collideColliders[GetColliderIndex(col)] = true;
                            }
                        }
                        if (collisionType == CollisionType.all)
                        {
                            UnityEngine.Collider2D[] cols = PhysicsToric.OverlapCapsuleAll(capsule, mask);
                            foreach (UnityEngine.Collider2D col in cols)
                            {
                                collideColliders[GetColliderIndex(col)] = true;
                            }
                        }
                    }
                }
            }
        }

        private void UpdateCast()
        {
            if (followCollider != null)
                followCollider.layer = mask;

            if (InputManager.GetKey(InputKey.W))
            {
                start += Vector2.up * moveSpeed * Time.deltaTime;
            }
            if (InputManager.GetKey(InputKey.S))
            {
                start += Vector2.down * moveSpeed * Time.deltaTime;
            }
            if (InputManager.GetKey(InputKey.D))
            {
                start += Vector2.right * moveSpeed * Time.deltaTime;
            }
            if (InputManager.GetKey(InputKey.A))
            {
                start += Vector2.left * moveSpeed * Time.deltaTime;
            }
            start = PhysicsToric.GetPointInsideBounds(start);

            if (InputManager.GetKeyDown(setStartKey))
            {
                start = PhysicsToric.GetPointInsideBounds(GetMousePos());
            }
        }

        private void UpdateRaycast()
        {
            UpdateCast();

            Vector2 dir = (GetMousePos() - start).normalized;
            float distance = useCastDistance ? castDistance : GetMousePos().Distance(start);

            ToricRaycastHit2D[] hits;
            Vector2[] toricInters;
            if (collisionType == CollisionType.first)
            {
                if (toricIntersection == ToricIntersection.None)
                {
                    ToricRaycastHit2D hit = PhysicsToric.Raycast(start, dir, distance, mask);
                    hits = new ToricRaycastHit2D[1] { hit };
                    toricInters = Array.Empty<Vector2>();
                }
                else
                {
                    ToricRaycastHit2D hit = PhysicsToric.Raycast(start, dir, distance, mask, out toricInters);
                    hits = new ToricRaycastHit2D[1] { hit };
                }
            }
            else
            {
                if (toricIntersection == ToricIntersection.None)
                {
                    hits = PhysicsToric.RaycastAll(start, dir, distance, mask);
                    toricInters = Array.Empty<Vector2>();
                }
                else
                {
                    hits = PhysicsToric.RaycastAll(start, dir, distance, mask, out Vector2[][] inters);
                    if (hits.Length > 0)
                    {
                        toricInters = new Vector2[inters.Last().Length];
                        for (int i = 0; i < toricInters.Length; i++)
                        {
                            toricInters[i] = inters.Last()[i];
                        }
                    }
                    else
                    {
                        toricInters = Array.Empty<Vector2>();
                    }
                }
            }

            foreach (ToricRaycastHit2D hit in hits)
            {
                if (hit.collider != null)
                {
                    Debug.DrawLine(start, hit.point, hit.collider != null ? colorTouch : colorUntouch);
                    collideColliders[GetColliderIndex(hit.collider)] = true;
                    DrawVector(hit.point, hit.normal, colorNormal, false);
                    DrawCircle(hit.point, 0.1f, hit.collider != null ? colorTouch : colorUntouch, false);
                    DrawCircle(hit.centroid, 0.15f, colorCentroid, false);
                }
            }

            foreach (Vector2 inter in toricInters)
            {
                DrawCircle(inter, 0.1f, Color.white, false);
            }
        }

        private void UpdateCircleCast()
        {
            UpdateCast();

            UpdateCast();

            Vector2 dir = (GetMousePos() - start).normalized;
            float distance = useCastDistance ? castDistance : GetMousePos().Distance(start);

            ToricRaycastHit2D[] hits;
            Vector2[] toricInters;
            if (collisionType == CollisionType.first)
            {
                if (toricIntersection == ToricIntersection.None)
                {
                    ToricRaycastHit2D hit = PhysicsToric.CircleCast(start, dir, castRadius, distance, mask);
                    hits = new ToricRaycastHit2D[1] { hit };
                    toricInters = Array.Empty<Vector2>();
                }
                else
                {
                    ToricRaycastHit2D hit = PhysicsToric.CircleCast(start, dir, castRadius, distance, mask, out toricInters);
                    hits = new ToricRaycastHit2D[1] { hit };
                }
            }
            else
            {
                if (toricIntersection == ToricIntersection.None)
                {
                    hits = PhysicsToric.CircleCastAll(start, dir, castRadius, distance, mask);
                    toricInters = Array.Empty<Vector2>();
                }
                else
                {
                    hits = PhysicsToric.CircleCastAll(start, dir, castRadius, distance, mask, out Vector2[][] inters);
                    if (hits.Length > 0)
                    {
                        toricInters = new Vector2[inters.Last().Length];
                        for (int i = 0; i < toricInters.Length; i++)
                        {
                            toricInters[i] = inters.Last()[i];
                        }
                    }
                    else
                    {
                        toricInters = Array.Empty<Vector2>();
                    }
                }
            }

            foreach (ToricRaycastHit2D hit in hits)
            {
                Debug.DrawLine(start, hit.centroid, hit.collider != null ? colorTouch : colorUntouch);
                if (hit.collider != null)
                {
                    collideColliders[GetColliderIndex(hit.collider)] = true;
                    DrawVector(hit.point, hit.normal, colorNormal, false);
                    DrawCircle(hit.point, 0.1f, hit.collider != null ? colorTouch : colorUntouch, false);
                    DrawCircle(hit.centroid, 0.15f, colorCentroid, false);
                    DrawCircle(hit.centroid, castRadius, hit.collider != null ? colorTouch : colorUntouch, false);
                }
            }

            foreach (Vector2 inter in toricInters)
            {
                DrawCircle(inter, 0.1f, colorUntouch, false);
            }
        }

        private void OnValidate()
        {
            castDistance = Mathf.Max(0f, castDistance);
            moveSpeed = Mathf.Max(0f, moveSpeed);
            castRadius = Mathf.Max(0f, castRadius);
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
                PhysicsToric.GizmosDrawHitboxes();

            if (!Application.isPlaying)
            {
                Gizmos.color = colorUntouch;
                foreach (UnityEngine.Collider2D col in colliders)
                {
                    Collider2D collider = Collider2D.FromUnityCollider2D(col);
                    DrawCollider(collider, Color.green, true);
                }
            }

            Gizmos.color = colorUntouch;
            Circle.GizmosDraw(start, 0.1f);
        }

        private void DrawCollider(Collider2D collider, Color color, bool gizmos = false)
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

        private void DrawVector(in Vector2 from, in Vector2 dir, Color color, bool gizmo = false)
        {
            if (gizmo)
            {
                Useful.GizmoDrawVector(from, dir);
            }
            else
            {
                Useful.GizmoDrawVector(from, dir, DrawLine);
            }

            void DrawLine(Vector3 start, Vector3 end) => Debug.DrawLine(start, end, color);
        }

        private void DrawCircle(in Vector2 center, float radius, Color color, bool gizmo)
        {
            if (gizmo)
            {
                Gizmos.color = color;
                Circle.GizmosDraw(center, radius);
            }
            else
            {
                Circle.GizmosDraw(center, radius, DrawLine);
            }

            void DrawLine(Vector3 start, Vector3 end) => Debug.DrawLine(start, end, color);
        }
    }
}

#endif