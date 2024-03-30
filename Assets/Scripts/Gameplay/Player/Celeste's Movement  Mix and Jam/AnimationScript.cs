using UnityEngine;

public class AnimationScript : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private CharacterController move;
    private EventController eventController;
    [HideInInspector] public SpriteRenderer sr;

    public bool enableBehaviour = true;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        move = GetComponentInParent<CharacterController>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        eventController = GetComponent<EventController>();
    }

    private void Start()
    {
        PauseManager.instance.callBackOnPauseDisable += Enable;
        PauseManager.instance.callBackOnPauseEnable += Disable;
    }

    private void Update()
    {
        if (!enableBehaviour)
            return;

        anim.SetFloat("absXSpeed", Mathf.Abs(rb.velocity.x));
        eventController.OnTriggerAnimatorSetFloat("absXSpeed", Mathf.Abs(rb.velocity.x));
        anim.SetFloat("absYSpeed", Mathf.Abs(rb.velocity.y));
        eventController.OnTriggerAnimatorSetFloat("absYSpeed", Mathf.Abs(rb.velocity.y));
        anim.SetBool("wallGrab", move.wallGrab);
        eventController.OnTriggerAnimatorSetBool("wallGrab", move.wallGrab);
        anim.SetBool("isSliding", move.isSliding);
        eventController.OnTriggerAnimatorSetBool("isSliding", move.isSliding);
        anim.SetBool("isFalling", move.isFalling);
        eventController.OnTriggerAnimatorSetBool("isFalling", move.isFalling);
        anim.SetBool("isGrounded", move.isGrounded);
        eventController.OnTriggerAnimatorSetBool("isGrounded", move.isGrounded);
        anim.SetBool("isJumping", move.isJumping);
        eventController.OnTriggerAnimatorSetBool("isJumping", move.isJumping);
        anim.SetBool("isDashing", move.isDashing);
        eventController.OnTriggerAnimatorSetBool("isDashing", move.isDashing);
        anim.SetBool("death", false);
        eventController.OnTriggerAnimatorSetBool("death", false);
    }

    public void Flip(int side)
    {
        if (move.wallGrab || move.isSliding)
        {
            if (side == -1 && sr.flipX)
                return;

            if (side == 1 && !sr.flipX)
            {
                return;
            }
        }

        sr.flipX = side != 1;
    }

    private void Disable()
    {
        enableBehaviour = false;
        anim.enabled = false;
    }

    private void Enable()
    {
        anim.enabled = true;
        enableBehaviour = true;
    }

    private void OnDestroy()
    {
        PauseManager.instance.callBackOnPauseEnable -= Disable;
        PauseManager.instance.callBackOnPauseDisable -= Enable;
    }
}
