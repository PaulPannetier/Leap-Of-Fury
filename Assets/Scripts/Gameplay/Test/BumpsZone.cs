using System.Collections.Generic;
using UnityEngine;

public class BumpsZone : MonoBehaviour
{
    private LayerMask charMask;
    private List<uint> charAlreadyTouch = new List<uint>();

    [SerializeField] private float radius = 3f;
    [SerializeField] private float bumpSpeed = 20f;

    private void Awake()
    {
        charMask = LayerMask.GetMask("Char");
    }

    private void Update()
    {
        Collider2D[] cols = PhysicsToric.OverlapCircleAll(transform.position, radius, charMask);
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
                    Vector2 dir = ((Vector2)(player.transform.position - transform.position)).normalized;
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

    private void OnValidate()
    {
        transform.localScale = Vector3.one * 2f * radius;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Circle.GizmosDraw(transform.position, radius);
    }
}
