using UnityEngine;

[RequireComponent(typeof(Movement), typeof(SpriteRenderer))]
public class TransparencyPassif : PassifAttack
{
    private Movement movement;
    private SpriteRenderer spriteRenderer;

    [SerializeField, Range(0f, 1f)] private float minTransparency = 0.3f;
    [SerializeField] private float transitionTime;

    protected override void Awake()
    {
        base.Awake();
        movement = GetComponent<Movement>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected override void Start()
    {
        base.Start();
        spriteRenderer.color = spriteRenderer.color * 1f;
    }

    protected override void Update()
    {
        if (!enableBehaviour || PauseManager.instance.isPauseEnable)
            return;

        base.Update();
        float target = movement.isGrounded || movement.onWall ? 1f : minTransparency;
        float current = Mathf.MoveTowards(spriteRenderer.color.a, target, ((1f - minTransparency) / transitionTime) * Time.deltaTime);
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, current); ; ;
    }

#if UNITY_EDITOR

    protected override void OnValidate()
    {
        base.OnValidate();
        transitionTime = Mathf.Max(transitionTime, 0f);
    }

#endif
}
