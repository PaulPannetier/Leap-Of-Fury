using System.Collections.Generic;
using UnityEngine;

public abstract class BumpsZone : MonoBehaviour
{
    protected LayerMask charMask;
    private List<uint> charAlreadyTouch = new List<uint>();

    [SerializeField] protected float collisionDetectionScale = 1.2f;
    [SerializeField] protected float bumpSpeed = 20f;

    protected virtual void Awake()
    {
        charMask = LayerMask.GetMask("Char");
    }

    protected abstract Collider2D[] GetTouchingChar();

    protected abstract Vector2 GetColliderNormal(Collider2D charCollider);

    protected virtual void Update()
    {
        Collider2D[] cols = GetTouchingChar();
        List<uint> newCharTouch = new List<uint>();
        foreach (Collider2D col in cols)
        {
            if(col.CompareTag("Char"))
            {
                GameObject player = col.GetComponent<ToricObject>().original;
                uint id = player.GetComponent<PlayerCommon>().id;
                newCharTouch.Add(id);
                if(!charAlreadyTouch.Contains(id))
                {
                    charAlreadyTouch.Add(id);
                    Vector2 dir = GetColliderNormal(col);
                    player.GetComponent<Movement>().ApplyBump(dir * bumpSpeed);
                    Invoke(nameof(ClearCharAlreadyTouch), 1f);
                }
            }
        }

        for (int i = charAlreadyTouch.Count - 1; i >= 0; i--)
        {
            if (!newCharTouch.Contains(charAlreadyTouch[i]))
            {
                charAlreadyTouch.RemoveAt(i);
            }
        }
    }

    private void ClearCharAlreadyTouch()
    {
        charAlreadyTouch.Clear();
    }

    protected virtual void OnValidate()
    {
        collisionDetectionScale = Mathf.Max(collisionDetectionScale, 0f);
    }

    protected virtual void OnDrawGizmosSelected()
    {

    }
}
