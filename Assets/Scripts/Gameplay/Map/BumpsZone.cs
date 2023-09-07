using System.Collections.Generic;
using UnityEngine;

public abstract class BumpsZone : MonoBehaviour
{
    public bool enableBehaviour = true;
    protected LayerMask charMask;
    private List<uint> charAlreadyTouch = new List<uint>();

    [SerializeField] protected Vector2 collisionDetectionScale = Vector2.one;
    [SerializeField] protected float bumpSpeed = 20f;

    protected virtual void Awake()
    {

    }

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
                    Invoke(nameof(ClearCharAlreadyTouch), GetBumpTimeOffet());
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

    protected abstract float GetBumpTimeOffet();

    private void ClearCharAlreadyTouch()
    {
        charAlreadyTouch.Clear();
    }

#if UNITY_EDITOR

    protected virtual void OnValidate()
    {
        collisionDetectionScale = new Vector2(Mathf.Max(collisionDetectionScale.x, 0f), Mathf.Max(collisionDetectionScale.y, 0f));
    }

    protected virtual void OnDrawGizmosSelected()
    {

    }

#endif
}
