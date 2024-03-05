using UnityEngine;
using System.Collections;

public class TeemoPassif : PassifAttack
{
    private Movement movement;
    private float timerGroundedWithoutMoving;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Coroutine currentChangeColor;
    private bool isTransparent;

    [SerializeField] private float durationToWait;
    [SerializeField, Range(0f, 1f)] private float transparency = 0.1f;
    [SerializeField] private float maxSpeedWhenNotMove = 0.2f;
    [SerializeField] private float colorFadeDuration = 0.2f;

    #region Awake/Start

    protected override void Awake()
    {
        base.Awake();
        timerGroundedWithoutMoving = 0f;
    }

    protected override void Start()
    {
        base.Start();
        movement = GetComponent<Movement>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    #endregion

    protected override void Update()
    {
        base.Update();

        if (PauseManager.instance.isPauseEnable)
            return;

        if (movement.isGrounded && rb.velocity.sqrMagnitude < maxSpeedWhenNotMove * maxSpeedWhenNotMove)
        {
            timerGroundedWithoutMoving += Time.deltaTime;
        }
        else
        {
            if(isTransparent)
            {
                ChangeColor(playerCommon.color);
                timerGroundedWithoutMoving = 0f;
                isTransparent = false;
            }
        }

        if(timerGroundedWithoutMoving > durationToWait)
        {
            if(!isTransparent)
            {
                Color target = new Color(playerCommon.color.r, playerCommon.color.g, playerCommon.color.b, playerCommon.color.a * transparency);
                ChangeColor(target);
                isTransparent = true;
            }
        }
    }

    private void ChangeColor(Color color)
    {
        if (currentChangeColor != null)
            StopCoroutine(currentChangeColor);

        currentChangeColor = StartCoroutine(ChangeColorCorout(color));
    }

    private IEnumerator ChangeColorCorout(Color color)
    {
        Color begColor = spriteRenderer.color;
        float timer = 0f;

        while(timer < colorFadeDuration)
        {
            if(!PauseManager.instance.isPauseEnable)
            {
                yield return null;
                timer += Time.deltaTime;

                spriteRenderer.color = Color.Lerp(begColor, color, timer / colorFadeDuration);
            }
        }

        spriteRenderer.color = color;
        currentChangeColor = null;
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        durationToWait = Mathf.Max(0f, durationToWait);
        maxSpeedWhenNotMove = Mathf.Max(0f, maxSpeedWhenNotMove);
        colorFadeDuration = Mathf.Max(0f, colorFadeDuration);
    }

#endif
}
