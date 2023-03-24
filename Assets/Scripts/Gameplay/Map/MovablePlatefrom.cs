using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class MovablePlatefrom : MonoBehaviour
{
    private enum PlateformeSide
    {
        None = 0, Up = 1, Down = 2, Left = 3, Right = 4
    }

    private static Vector2[] convertSideToDir = new Vector2[5]
    {
        Vector2.zero, Vector2.up, Vector2.down, Vector2.left, Vector2.right 
    };

    private BoxCollider2D hitbox;
    private Rigidbody2D rb;
    private List<Movement> charInFrontLastFrame;
    private Movement charWhoActivate = null;
    private bool isMoving = false;
    private Vector2 moveDir;

    public bool enableBehaviour = true;

    [SerializeField] private bool enableLeft = true, enableRight = true, enableUp = true, enableDown = true;
    [SerializeField] private float speed;
    [SerializeField, Tooltip("lerp de la vitesse en  %age vMax / sec")] private float speedLerp = 0.6f;
    [SerializeField] private float detectionCharColliderScale = 1.1f;
    [SerializeField, Range(0f, 1)] private float detectionGroundColliderScale = 0.9f;
    [SerializeField] private float minSpeedToTriggerMovement = 1f;
    [SerializeField] private float rayDistOffset = 0.1f;
    [SerializeField] private uint rayCount = 4;
    [SerializeField] private LayerMask charMask, groundMask, groundAndCharMask;

    private void Awake()
    {
        hitbox = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        charInFrontLastFrame = new List<Movement>();
        PauseManager.instance.callBackOnPauseDisable += Disable;
        PauseManager.instance.callBackOnPauseEnable += Enable;
    }

    #region Update

    private void Update()
    {
        if(!isMoving)
        {
            List<Movement> charInFront = new List<Movement>();
            Collider2D[] cols = PhysicsToric.OverlapBoxAll((Vector2)transform.position + hitbox.offset, hitbox.size * detectionCharColliderScale, 0f, charMask);
            foreach (Collider2D col in cols)
            {
                if (col.CompareTag("Char"))
                {
                    charInFront.Add(col.GetComponent<ToricObject>().original.GetComponent<Movement>());
                }
            }

            foreach (Movement charMov in charInFront)
            {
                if (charMov.isDashing)
                {
                    Rigidbody2D charRb = charMov.GetComponent<Rigidbody2D>();
                    BoxCollider2D charHitbox = charMov.GetComponent<BoxCollider2D>();
                    Vector2 charCenter = (Vector2)charHitbox.transform.position + charHitbox.offset;
                    Vector2 closestPoint = hitbox.ClosestPoint(charCenter);

                    if ((closestPoint - charCenter).Dot(charRb.velocity) > 0f)
                    {
                        //on calcule quelle face de la plateforme est touché
                        if(CanMovePlateform(charCenter, charHitbox.size, charRb.velocity, out PlateformeSide ps))
                        {
                            if((ps == PlateformeSide.Up && enableUp) || (ps == PlateformeSide.Down && enableDown)
                                || (ps == PlateformeSide.Right && enableRight) || (ps == PlateformeSide.Left && enableLeft))
                            {
                                moveDir = convertSideToDir[(int)ps];
                                isMoving = true;
                                charWhoActivate = charMov;
                                break;
                            }
                        }
                    }  
                }
            }
            charInFrontLastFrame.Clear();
            charInFrontLastFrame = charInFront;

            bool CanMovePlateform(in Vector2 center, in Vector2 size, in Vector2 speed, out PlateformeSide plateformeSide)
            {
                void IsPointInCoor(in Vector2 point, out bool xCoord, out bool yCoord)
                {
                    xCoord = point.x >= transform.position.x - hitbox.size.x * 0.5f * detectionCharColliderScale &&
                        point.x <= transform.position.x + hitbox.size.x * 0.5f * detectionCharColliderScale;
                    yCoord = point.y >= transform.position.y - hitbox.size.y * 0.5f * detectionCharColliderScale &&
                        point.y <= transform.position.y + hitbox.size.y * 0.5f * detectionCharColliderScale;
                }

                Vector2 topLeft = new Vector2(center.x - size.x * 0.5f, center.y + size.y * 0.5f);
                Vector2 topRight = new Vector2(center.x + size.x * 0.5f, center.y + size.y * 0.5f);
                Vector2 bottomLeft = new Vector2(center.x - size.x * 0.5f, center.y - size.y * 0.5f);
                Vector2 bottomRight = new Vector2(center.x + size.x * 0.5f, center.y - size.y * 0.5f);
                bool isInXCoord, isInYCoord;
                if (speed.x >= 0f)
                {
                    if (speed.y >= 0f)
                    {
                        //gauche ou bas
                        IsPointInCoor(topRight, out isInXCoord, out isInYCoord);
                        if (isInXCoord && isInYCoord)
                        {
                            plateformeSide = speed.x >= speed.y ? PlateformeSide.Left : PlateformeSide.Down;
                            return Mathf.Max(speed.x, speed.y) >= minSpeedToTriggerMovement;
                        }
                        if (isInXCoord || isInYCoord)
                        {
                            plateformeSide = isInXCoord ? PlateformeSide.Down : PlateformeSide.Left;
                            return isInXCoord ? speed.y >= minSpeedToTriggerMovement : speed.x >= minSpeedToTriggerMovement;
                        }
                        plateformeSide = PlateformeSide.None;
                        return false;
                    }
                    else
                    {
                        IsPointInCoor(bottomRight, out isInXCoord, out isInYCoord);
                        if (isInXCoord && isInYCoord)
                        {
                            plateformeSide = speed.x >= -speed.y ? PlateformeSide.Left : PlateformeSide.Up;
                            return Mathf.Max(speed.x, -speed.y) >= minSpeedToTriggerMovement;
                        }
                        if (isInXCoord || isInYCoord)
                        {
                            plateformeSide = isInXCoord ? PlateformeSide.Up : PlateformeSide.Left;
                            return isInXCoord ? -speed.y >= minSpeedToTriggerMovement : speed.x >= minSpeedToTriggerMovement;
                        }
                        plateformeSide = PlateformeSide.None;
                        return false;
                    }
                }
                else
                {
                    if (speed.y >= 0f)
                    {
                        IsPointInCoor(topLeft, out isInXCoord, out isInYCoord);
                        if (isInXCoord && isInYCoord)
                        {
                            plateformeSide = -speed.x >= speed.y ? PlateformeSide.Right : PlateformeSide.Down;
                            return Mathf.Max(-speed.x, speed.y) >= minSpeedToTriggerMovement;
                        }
                        if (isInXCoord || isInYCoord)
                        {
                            plateformeSide = isInXCoord ? PlateformeSide.Down : PlateformeSide.Right;
                            return isInXCoord ? speed.y >= minSpeedToTriggerMovement : -speed.x >= minSpeedToTriggerMovement;
                        }
                        plateformeSide = PlateformeSide.None;
                        return false;
                    }
                    else
                    {
                        IsPointInCoor(bottomLeft, out isInXCoord, out isInYCoord);
                        if (isInXCoord && isInYCoord)
                        {
                            plateformeSide = -speed.x >= -speed.y ? PlateformeSide.Right : PlateformeSide.Up;
                            return Mathf.Max(-speed.x, -speed.y) >= minSpeedToTriggerMovement;
                        }
                        if (isInXCoord || isInYCoord)
                        {
                            plateformeSide = isInXCoord ? PlateformeSide.Up : PlateformeSide.Left;
                            return isInXCoord ? -speed.y >= minSpeedToTriggerMovement : -speed.x >= minSpeedToTriggerMovement;
                        }
                        plateformeSide = PlateformeSide.None;
                        return false;
                    }
                }
            }
        }
        else
        {
            rb.velocity = Vector2.MoveTowards(rb.velocity, moveDir * speed, speedLerp * speed * Time.deltaTime);

            Collider2D[] cols = PhysicsToric.OverlapBoxAll((Vector2)transform.position + hitbox.offset, hitbox.size * detectionGroundColliderScale, 0f, groundMask);
            foreach (Collider2D col in cols)
            {
                MovablePlatefrom mp = col.GetComponent<MovablePlatefrom>();
                if(mp == null || mp != this)
                {
                    isMoving = false;
                    charWhoActivate = null;
                    float toAddToPosX = Mathf.Abs(rb.velocity.x) > 1e-5f ? (hitbox.size.x * (1f - detectionGroundColliderScale)) * -0.5f * rb.velocity.x.Sign() : 0f;
                    float toAddToPosY = Mathf.Abs(rb.velocity.y) > 1e-5f ? (hitbox.size.y * (1f - detectionGroundColliderScale)) * -0.5f * rb.velocity.y.Sign() : 0f;
                    rb.MovePosition((Vector2)transform.position + new Vector2(toAddToPosX, toAddToPosY));
                    rb.velocity = Vector2.zero;
                    return;
                }
            }

            //Detection char ecrasé
            List<Collider2D> charInFrontOf = new List<Collider2D>();
            Vector2 step = Mathf.Abs(moveDir.x) >= Mathf.Abs(moveDir.y) ? new Vector2(0f, hitbox.size.y / (rayCount - 1)) : new Vector2(hitbox.size.x / (rayCount - 1), 0f);
            Vector2 beg = (Vector2)transform.position + hitbox.offset +
                (Mathf.Abs(moveDir.x) >= Mathf.Abs(moveDir.y) ? new Vector2(hitbox.size.x * 0.5f * moveDir.x.Sign(), hitbox.size.y * -0.5f) :
                 new Vector2(hitbox.size.x * -0.5f, hitbox.size.y * 0.5f * moveDir.y.Sign()));
            
            List<uint> lstCharAlreadyKill = new List<uint>();
            for (int i = 0; i < rayCount; i++)
            {
                Vector2 point = beg + i * step + moveDir * rayDistOffset;
                RaycastHit2D raycast = PhysicsToric.Raycast(point, moveDir, Mathf.Max(PhysicsToric.cameraSize.x, PhysicsToric.cameraSize.y), groundAndCharMask);

                if(raycast.collider != null && raycast.collider.CompareTag("Char"))
                {
                    uint id = raycast.collider.GetComponent<PlayerCommon>().id;
                    if (lstCharAlreadyKill.Contains(id))
                        continue;
                    float dst = 0.5f * detectionCharColliderScale * (Mathf.Abs(moveDir.x) >= Mathf.Abs(moveDir.y) ? hitbox.size.x : hitbox.size.y);
                    if (raycast.point.SqrDistance(point) <= dst * dst)
                    {
                        BoxCollider2D charHitbox = raycast.collider.GetComponent<BoxCollider2D>();
                        Vector2 p1, p2, p3;
                        if(Mathf.Abs(moveDir.x) >= Mathf.Abs(moveDir.y))
                        {
                            Vector2 center = (Vector2)charHitbox.transform.position + charHitbox.offset;
                            p1 = center + new Vector2(charHitbox.size.x * 0.5f * moveDir.x.Sign(), charHitbox.size.y * 0.5f);
                            p2 = center + new Vector2(charHitbox.size.x * 0.5f * moveDir.x.Sign(), 0f);
                            p3 = center + new Vector2(charHitbox.size.x * 0.5f * moveDir.x.Sign(), charHitbox.size.y * -0.5f);
                        }
                        else
                        {
                            Vector2 center = (Vector2)charHitbox.transform.position + charHitbox.offset;
                            p1 = center + new Vector2(charHitbox.size.x * -0.5f, charHitbox.size.y * 0.5f * moveDir.y.Sign());
                            p2 = center + new Vector2(0f, charHitbox.size.y * 0.5f * moveDir.y.Sign());
                            p3 = center + new Vector2(charHitbox.size.x * 0.5f, charHitbox.size.y * 0.5f * moveDir.y.Sign());
                        }

                        if(PhysicsToric.OverlapPoint(p1, groundMask) != null || PhysicsToric.OverlapPoint(p2, groundMask) != null || PhysicsToric.OverlapPoint(p3, groundMask) != null)
                        {
                            //collision!
                            lstCharAlreadyKill.Add(id);
                            if (charHitbox.GetComponent<PlayerCommon>().id == charWhoActivate.GetComponent<PlayerCommon>().id)
                            {
                                charWhoActivate.GetComponent<EventController>().OnBeenKillByEnvironnement(gameObject);
                            }
                            else
                            {
                                charHitbox.GetComponent<EventController>().OnBeenKillInstant(charWhoActivate.gameObject);
                                charWhoActivate.GetComponent<EventController>().OnKill(charHitbox.gameObject);
                            }
                        }
                    }
                }
            }
        }
    }

    #endregion

    #region Gizmos/OnValidate

    private void Disable()
    {
        enableBehaviour = false;
    }

    private void Enable()
    {
        enableBehaviour = true;
    }

    private void OnValidate()
    {
        detectionCharColliderScale = Mathf.Max(1f, detectionCharColliderScale);
        rayCount = (uint)Mathf.Max(2f, rayCount);
        speed = Mathf.Max(0f, speed);
    }

    private void OnDrawGizmosSelected()
    {
        if(hitbox == null)
            hitbox = GetComponent<BoxCollider2D>();
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, hitbox.size * detectionCharColliderScale);
        Gizmos.DrawWireCube(transform.position, hitbox.size * detectionGroundColliderScale);
    }

    #endregion
}
