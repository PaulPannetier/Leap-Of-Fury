using UnityEngine;

public class MaxwellHouseConvoyerBelt : ConvoyerBelt
{
    private SpriteRenderer spriteRenderer;

    [SerializeField] private Color colorActivated = Color.green, colorInacticated = Color.red;

    protected override void Awake()
    {
        base.Awake();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    protected override void Update()
    {
        base.Update();
        spriteRenderer.color = isActive ? colorActivated : colorInacticated;
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.color = base.startActive ? colorActivated : colorInacticated;
    }
}
