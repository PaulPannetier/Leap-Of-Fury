using PathFinding;
using System.Collections.Generic;
using UnityEngine;

public abstract class BumpsZone : PathFindingBlocker
{
    public bool enableBehaviour = true;
    protected LayerMask charMask;
    private List<uint> charAlreadyTouch = new List<uint>();

    [SerializeField] protected Vector2 collisionDetectionScale = Vector2.one;
    [SerializeField] protected float bumpSpeed = 20f;
    [SerializeField] protected float minDurationBetween2Bumps = 0.1f;

    protected virtual void Start()
    {
        charMask = LayerMask.GetMask("Char");
    }

    protected abstract Collider2D[] GetTouchingChar();

    protected abstract Vector2 GetColliderNormal(Collider2D charCollider);

    protected virtual void Update()
    {
        if(!enableBehaviour)
            return;

        Collider2D[] cols = GetTouchingChar();

        foreach (Collider2D col in cols)
        {
            if(col.CompareTag("Char"))
            {
                GameObject player = col.GetComponent<ToricObject>().original;
                uint id = player.GetComponent<PlayerCommon>().id;
                if(!charAlreadyTouch.Contains(id))
                {
                    charAlreadyTouch.Add(id);
                    Vector2 dir = GetColliderNormal(col);
                    player.GetComponent<CharacterController>().ApplyBump(dir * bumpSpeed);
                    this.Invoke(ClearCharAlreadyTouch, id, minDurationBetween2Bumps);
                }
            }
        }
    }

    private void ClearCharAlreadyTouch(uint id)
    {
        for (int i = charAlreadyTouch.Count - 1; i >= 0; i--)
        {
            if (charAlreadyTouch[i] == id)
                charAlreadyTouch.RemoveAt(i);
        }
    }

#if UNITY_EDITOR

    protected virtual void OnValidate()
    {
        collisionDetectionScale = new Vector2(Mathf.Max(collisionDetectionScale.x, 0f), Mathf.Max(collisionDetectionScale.y, 0f));
        minDurationBetween2Bumps = Mathf.Max(0f, minDurationBetween2Bumps);
    }

    protected virtual void OnDrawGizmosSelected()
    {

    }

#endif
}
