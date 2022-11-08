using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class MovablePlatefrom : MonoBehaviour
{
    private enum PlateformeSide
    {
        Up, Down, Left, Right
    }

    private BoxCollider2D hitbox;
    private List<Movement> charInFrontLastFrame;
    private Movement charWhoActivate = null;
    private bool isMoving = false;

    public bool enableBehaviour = true;

    [SerializeField] private bool enableLeft = true, enableRight = true, enableUp = true, enableDown = true;
    [SerializeField] private float detectionColliderScale = 1.1f;
    [SerializeField] private float minSpeedToTriggerMovement = 1f;
    [SerializeField] private LayerMask charMask;

    private void Awake()
    {
        hitbox = GetComponent<BoxCollider2D>();
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
                    Rigidbody2D rb = charMov.GetComponent<Rigidbody2D>();
                    BoxCollider2D charHitbox = charMov.GetComponent<BoxCollider2D>();
                    Vector2 charCenter = (Vector2)charMov.transform.position + charHitbox.offset;
                    Vector2 closestPoint = charHitbox.ClosestPoint(charCenter);

                    if ((closestPoint - charCenter).Dot(rb.velocity) > 0f)
                    {
                        //on calcule quelle face de la plateforme est touché
                        if(CanMovePlateform(charCenter, charHitbox.size, rb.velocity, out PlateformeSide plateformeSide))
                        {
                            isMoving = true;
                            charWhoActivate = charMov;
                            break;
                        }
                    }  
                }
            }
            charInFrontLastFrame = charInFront;
        }
        
    }

    private bool CanMovePlateform(in Vector2 center, in Vector2 size, in Vector2 speed, out PlateformeSide plateformeSide)
    {
        bool isInXcoord = center.x >= transform.position.x - hitbox.size.x * 0.5f &&
                        center.x <= transform.position.x + hitbox.size.x * 0.5f;
        bool isInYcoord = center.y >= transform.position.y - hitbox.size.y * 0.5f &&
            center.y <= transform.position.y + hitbox.size.y * 0.5f;

        Vector2 topLeft  = new Vector2(center.x - size.x * 0.5f, center.y + size.y * 0.5f);
        Vector2 topRight = new Vector2(center.x + size.x * 0.5f, center.y + size.y * 0.5f);
        Vector2 botLeft  = new Vector2(center.x - size.x * 0.5f, center.y - size.y * 0.5f);
        Vector2 botRight = new Vector2(center.x + size.x * 0.5f, center.y - size.y * 0.5f);

        if (speed.x >= 0f)
        {
            if (speed.y >= 0f)
            {
                //gauche ou bas
                if (isInXcoord)
                {
                    plateformeSide = PlateformeSide.Down;
                    return speed.y >= minSpeedToTriggerMovement;
                }
                else if (isInYcoord)
                {
                    plateformeSide = PlateformeSide.Left;
                    return speed.x >= minSpeedToTriggerMovement;
                }
                else
                {

                }
            }
            else
            {
                //gauche ou haut
                if (isInXcoord)
                {
                    plateformeSide = PlateformeSide.Up;
                    return -speed.y >= minSpeedToTriggerMovement;
                }
                else if (isInYcoord)
                {
                    plateformeSide = PlateformeSide.Left;
                    return speed.x >= minSpeedToTriggerMovement;
                }
            }
        }
        else
        {
            if (speed.y >= 0f)
            {
                //droite ou bas
                if (isInXcoord)
                {
                    plateformeSide = PlateformeSide.Down;
                    return speed.y >= minSpeedToTriggerMovement;
                }
                else if (isInYcoord)
                {
                    plateformeSide = PlateformeSide.Right;
                    return -speed.x >= minSpeedToTriggerMovement;
                }
            }
            else
            {
                //droite ou haut
                if (isInXcoord)
                {
                    plateformeSide = PlateformeSide.Up;
                    return -speed.y >= minSpeedToTriggerMovement;
                }
                else if (isInYcoord)
                {
                    plateformeSide = PlateformeSide.Right;
                    return -speed.x >= minSpeedToTriggerMovement;
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
