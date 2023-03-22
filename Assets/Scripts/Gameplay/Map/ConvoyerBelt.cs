using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(MapColliderData))]
public class ConvoyerBelt : MonoBehaviour
{
    private BoxCollider2D hitbox;
    private SpriteRenderer spriteRenderer;

    public bool enableBehaviour = true;

    private void Awake()
    {
        hitbox = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        spriteRenderer.color = enableBehaviour ? Color.green : Color.red;
    }

}
