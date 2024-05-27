using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    public static List<Fire> fires = new List<Fire>();

    public static bool IsAFire(Vector2 position, out Fire fire)
    {
        position = PhysicsToric.GetPointInsideBounds(position);
        foreach (Fire f in fires)
        {
            if (f != null && f.hitbox.Contains(position))
            {
                fire = f;
                return true;
            }
        }
        fire = null;
        return false;
    }

    [HideInInspector] public ToricObject toricObject;
    private FirePassif fireAttack;
    private float strength, lastChancePropagation;
    private List<uint> charsAlreadyTouch;
    private PlayerCommon playerCommon;
    private Bounds hitbox => new Bounds(transform.position, fireAttack.fireSize);

    private void Awake()
    {
        toricObject = GetComponent<ToricObject>();
        charsAlreadyTouch = new List<uint>();
        fires.Add(this);
    }

    private void Start()
    {
        if(toricObject.isAClone)
        {
            fireAttack = toricObject.original.GetComponent<Fire>().fireAttack;
        }
    }

    public void OnCreate(FirePassif fireAttack)
    {
        this.fireAttack = fireAttack;
        strength = lastChancePropagation = 100f;
        playerCommon = this.fireAttack.GetComponent<PlayerCommon>();
    }

    //le joueur aillant crée le feu marche dessus;
    public void OnWalk()
    {
        if(strength < 100f - fireAttack.firePropagationGap)
        {
            strength = lastChancePropagation = 100f;
        }
    }

    private void Update()
    {
        //destruction
        if (!toricObject.isAClone)
        {
            strength -= fireAttack.fireDecreasement * Time.deltaTime;
            if (strength <= 0f)
            {
                Destroy();
                return;
            }

            //Propagation
            if (strength > fireAttack.minStrenghtForPropagation && lastChancePropagation - strength > fireAttack.firePropagationGap)
            {
                lastChancePropagation -= fireAttack.firePropagationGap;
                if (Random.Rand() <= fireAttack.firePropagationProbability)
                {
                    TryPropagation();
                }
            }
        }

        //Char collision
        Collider2D[] cols = PhysicsToric.OverlapBoxAll((Vector2)transform.position, fireAttack.fireSize, 0f, fireAttack.charMask);
        foreach (Collider2D col in cols)
        {
            if(col.CompareTag("Char"))
            {
                GameObject player = col.GetComponent<ToricObject>().original;
                uint id = player.GetComponent<PlayerCommon>().id;

                if(playerCommon == null && toricObject.isAClone)
                {
                    playerCommon = toricObject.original.GetComponent<Fire>().playerCommon;
                }

                if (id != playerCommon.id && !charsAlreadyTouch.Contains(id))
                {
                    charsAlreadyTouch.Add(id);
                    fireAttack.OnFireBurnPlayer(player);
                }
            }
        }
    }

    private void TryPropagation()
    {
        bool right = !IsAFire((Vector2)transform.position + Vector2.right * (fireAttack.fireSize.x * 1.45f), out Fire _);
        right = right && (PhysicsToric.Raycast((Vector2)transform.position + Vector2.right * (fireAttack.fireSize.x * 0.5f), Vector2.right, fireAttack.fireSize.x, fireAttack.groundMask).collider == null);
        right = right && (PhysicsToric.Raycast((Vector2)transform.position + Vector2.right * (fireAttack.fireSize.x * 1.45f), Vector2.down, fireAttack.fireSize.y, fireAttack.groundMask).collider != null);

        bool left = !IsAFire((Vector2)transform.position + Vector2.left * (fireAttack.fireSize.x * 1.45f), out Fire _);
        left = left && (PhysicsToric.Raycast((Vector2)transform.position + Vector2.left * (fireAttack.fireSize.x * 0.5f), Vector2.left, fireAttack.fireSize.x, fireAttack.groundMask).collider == null);
        left = left && (PhysicsToric.Raycast((Vector2)transform.position + Vector2.left * (fireAttack.fireSize.x * 1.45f), Vector2.down, fireAttack.fireSize.y, fireAttack.groundMask).collider != null);

        if (right || left)
        {
            if (right && left)
            {
                CreateNewFire(Random.Rand() < 0.5f);
            }
            else if (right)
            {
                CreateNewFire(true);
            }
            else if (left)
            {
                CreateNewFire(false);
            }

            void CreateNewFire(in bool right)
            {
                Vector2 newPos = (Vector2)transform.position + (right ? new Vector2(fireAttack.fireSize.x, 0f) : new Vector2(-fireAttack.fireSize.x, 0f));
                float newStrenght = Mathf.Max(0f, strength - fireAttack.fireDecreasementAtPropagation);
                fireAttack.CreateFire(newStrenght, newPos);
            }
        }
    }

    private void Destroy()
    {
        fires.Remove(this);
        Destroy(gameObject);
    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if(fireAttack != null && fireAttack.drawFireGizmos)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube((Vector2)transform.position, fireAttack.fireSize);
            Vector2 beg = (Vector2)transform.position + Vector2.right * (fireAttack.fireSize.x * 0.45f);
            Gizmos.DrawLine(beg, beg + Vector2.down * fireAttack.fireSize.y);
            beg = (Vector2)transform.position + Vector2.left * (fireAttack.fireSize.x * 0.45f);
            Gizmos.DrawLine(beg, beg + Vector2.down * fireAttack.fireSize.y);
        }
    }

#endif
}
