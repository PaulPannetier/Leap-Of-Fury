using UnityEngine;

public class AnimationScript : MonoBehaviour
{
    private Animator anim;
    private CharacterController charController;
    private EventController eventController;
    [HideInInspector] public SpriteRenderer sr;

    public bool enableBehaviour = true;

    #region Awake/Start

    private void Awake()
    {
        anim = GetComponent<Animator>();
        charController = GetComponentInParent<CharacterController>();
        sr = GetComponent<SpriteRenderer>();
        eventController = GetComponent<EventController>();
    }

    private void Start()
    {
        PauseManager.instance.callBackOnPauseDisable += Enable;
        PauseManager.instance.callBackOnPauseEnable += Disable;
    }

    #endregion

    private void Update()
    {
        if (!enableBehaviour)
            return;

        anim.SetFloat("absXSpeed", Mathf.Abs(charController.velocity.x));
        eventController.OnTriggerAnimatorSetFloat("absXSpeed", Mathf.Abs(charController.velocity.x));
        anim.SetFloat("absYSpeed", Mathf.Abs(charController.velocity.y));
        eventController.OnTriggerAnimatorSetFloat("absYSpeed", Mathf.Abs(charController.velocity.y));
        anim.SetBool("wallGrab", charController.wallGrab);
        eventController.OnTriggerAnimatorSetBool("wallGrab", charController.wallGrab);
        anim.SetBool("isSliding", charController.isSliding);
        eventController.OnTriggerAnimatorSetBool("isSliding", charController.isSliding);
        anim.SetBool("isFalling", charController.isFalling);
        eventController.OnTriggerAnimatorSetBool("isFalling", charController.isFalling);
        anim.SetBool("isGrounded", charController.isGrounded);
        eventController.OnTriggerAnimatorSetBool("isGrounded", charController.isGrounded);
        anim.SetBool("isJumping", charController.isJumping);
        eventController.OnTriggerAnimatorSetBool("isJumping", charController.isJumping);
        anim.SetBool("isDashing", charController.isDashing);
        eventController.OnTriggerAnimatorSetBool("isDashing", charController.isDashing);
        anim.SetBool("death", false);
        eventController.OnTriggerAnimatorSetBool("death", false);
        sr.flipX = charController.flip;
    }

    #region Enable/Disable/OnDestroy

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

    #endregion
}
