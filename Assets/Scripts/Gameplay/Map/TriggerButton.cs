using System;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine;

public class TriggerButton : MonoBehaviour
{
    private List<uint> charTouchLastFrame;
    private LayerMask charMask;
    private bool isButtonEnable;

    [SerializeField] private Vector2 colliderOffet;
    [SerializeField] private Vector2 colliderSize;
    //public Action<GameObject, bool> triggeredButtonFunctions;
    
    [Space] public UnityEvent<GameObject, bool> triggeredButtonFunctions;

    private void Awake()
    {
        charMask = LayerMask.GetMask("Char");
    }

    private void Start()
    {
        charTouchLastFrame = new List<uint>();
        isButtonEnable = false;
    }

    private void Update()
    {
        Collider2D[] cols = PhysicsToric.OverlapBoxAll((Vector2)transform.position + colliderOffet, colliderSize, 0f, charMask);
        List<(uint, GameObject)> charTouch = new List<(uint, GameObject)>();

        foreach (Collider2D col in cols)
        {
            if(col.CompareTag("Char"))
            {
                GameObject player = col.GetComponent<ToricObject>().original;
                uint id = player.GetComponent<PlayerCommon>().id;
                charTouch.Add((id, player));
            }
        }

        foreach ((uint, GameObject) tmp in charTouch)
        {
            uint id = tmp.Item1;
            GameObject player = tmp.Item2;
            if(!charTouchLastFrame.Contains(id))
            {
                //id vient de passer en col avec le bouton
                TriggerGravityButton(player);
            }
        }

        charTouchLastFrame.Clear();
        foreach ((uint, GameObject) tmp in charTouch)
        {
            charTouchLastFrame.Add(tmp.Item1);
        }

        void TriggerGravityButton(GameObject player)
        {
            isButtonEnable = !isButtonEnable;
            triggeredButtonFunctions.Invoke(player, isButtonEnable);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Hitbox.GizmosDraw((Vector2)transform.position + colliderOffet, colliderSize);
    }
}
