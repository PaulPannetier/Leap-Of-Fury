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
    [SerializeField] private float detectionColliderScale = 1.1f;
    [SerializeField] private float minSpeedToTriggerMovement = 1f;
    [SerializeField] private float rayMultiplier = 1.1f;
    [SerializeField] private LayerMask charMask, groundMask;

    private void Awake()
    {
        hitbox = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        charInFrontLastFrame = new List<Movement>();
    }

    private void Update()
    {
        if(!isMoving)
        {
            List<Movement> charInFront = new List<Movement>();
            Collider2D[] cols = PhysicsToric.OverlapBoxAll(transform.position, hitbox.size * detectionColliderScale, 0f, charMask);
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
                    Vector2 charCenter = (Vector2)charMov.transform.position + charHitbox.offset;
                    Vector2 closestPoint = charHitbox.ClosestPoint(charCenter);

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
                    xCoord = point.x >= transform.position.x - hitbox.size.x * 0.5f * detectionColliderScale &&
                        point.x <= transform.position.x + hitbox.size.x * 0.5f * detectionColliderScale;
                    yCoord = point.y >= transform.position.y - hitbox.size.y * 0.5f * detectionColliderScale &&
                        point.y <= transform.position.y + hitbox.size.y * 0.5f * detectionColliderScale;
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
            Collider2D[] cols = PhysicsToric.OverlapBoxAll(transform.position, hitbox.size * detectionColliderScale, 0f, groundMask);
            foreach (Collider2D col in cols)
            {
                MovablePlatefrom mp = col.GetComponent<MovablePlatefrom>();
                if(mp != null && mp != this)
                {
                    isMoving = false;
                    charWhoActivate = null;
                    rb.velocity = Vector2.zero;
                    return;
                }
            }

            cols = PhysicsToric.OverlapBoxAll(transform.position, hitbox.size * detectionColliderScale, 0f, charMask);
            foreach (Collider2D col in cols)
            {
                if (col.CompareTag("Char"))
                {
                    BoxCollider2D charHitbox = col.GetComponent<BoxCollider2D>();
                    Vector2 point = (Vector2)transform.position + 0.5f * (moveDir.x >= moveDir.y ? new Vector2(hitbox.size.x * moveDir.x.Sign(), 0f) : new Vector2(0f, hitbox.size.y * moveDir.y.Sign()));
                    float dst = rayMultiplier * (moveDir.x >= moveDir.y ? charHitbox.size.x : charHitbox.size.y);
                    RaycastHit2D raycast = PhysicsToric.Raycast(point, moveDir, dst, groundMask);
                    if(raycast.collider != null)
                    {
                        //collision!
                        if(col.GetComponent<PlayerCommon>().id == charWhoActivate.GetComponent<PlayerCommon>().id)
                        {
                            charWhoActivate.GetComponent<EventController>().OnBeenKillByEnvironnement(gameObject);
                        }
                        else
                        {
                            col.GetComponent<EventController>().OnBeenKillInstant(charWhoActivate.gameObject);
                            charWhoActivate.GetComponent<EventController>().OnKill(col.gameObject);
                        }
                    }
                }
            }
        }
    }    

    private void OnValidate()
    {
        detectionColliderScale = Mathf.Max(1f, detectionColliderScale);
    }

    private void OnDrawGizmosSelected()
    {
        if(hitbox == null)
            hitbox = GetComponent<BoxCollider2D>();
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, hitbox.size * detectionColliderScale);
    }
}
