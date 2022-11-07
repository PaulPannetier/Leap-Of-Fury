using System;
using UnityEngine;

/// <summary>
/// Hey developer!
/// If you have any questions, come chat with me on my Discord: https://discord.gg/GqeHHnhHpz
/// If you enjoy the controller, make sure you give the video a thumbs up: https://youtu.be/rJECT58CQHs
/// Have fun!
///
/// Love,
/// Tarodev
/// </summary>

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerControllerBonCeleste : MonoBehaviour
{   
    [Serializable]
    private struct FrameInputs
    {
        public float x, y;
        public int rawX, rawY;
    }

    private Rigidbody2D rb;
    //[SerializeField] private Animator anim;
    private FrameInputs inputs;
    private bool isFacingLeft;//on se dirige a gauche
    private bool isFacingRight => !isFacingLeft;
    [Header("Inputs")] [SerializeField] private KeyCode up;
    [SerializeField] private KeyCode down, right, left, jump, grab, dash;
    private bool hitUp, hitDown, hitRight, hitLeft, hitJump, hitGrab, hitDash;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        rb.gravityScale = gravityScale;
        //OnTouchedGround += () => { Debug.Log("On viens de toucher le sol."); };
        //OnStartDashing += () => { Debug.Log("On viens de dasher!"); };
        //OnStopDashing += () => { Debug.Log("On a fini de dasher!"); };
    }

    private void Update()
    {
        GatherInputs();
    }

    private void FixedUpdate()
    {
        HandleGrounding();

        HandleWalking();

        HandleJumping();

        HandleWallSlide();

        HandleWallGrab();

        HandleDashing();
    }

    #region Inputs

    private void GatherInputs()
    {
        inputs.x = Input.GetAxis("Horizontal");
        inputs.y = Input.GetAxis("Vertical");
        inputs.rawX = (int)inputs.x;
        inputs.rawY = (int)inputs.y;
        hitUp = Input.GetKey(up);
        hitDown = Input.GetKey(down);
        hitRight = Input.GetKey(right);
        hitLeft = Input.GetKey(left);
        hitJump = Input.GetKey(jump);
        hitGrab = Input.GetKey(grab);
        hitDash = Input.GetKey(dash);

        isFacingLeft = inputs.rawX != 1 && (inputs.rawX == -1 || isFacingLeft);
        if (!isGrabbing)
            SetFacingDirection(isFacingLeft); // Don't turn while grabbing the wall
    }

    //Turn the character
    private void SetFacingDirection(bool left)
    {
        //_anim.transform.rotation = left ? Quaternion.Euler(0, -90, 0) : Quaternion.Euler(0, 90, 0);
    }

    #endregion

    #region Detection

    [Header("Detection")] [SerializeField] private LayerMask groundMask;
    [SerializeField] private Vector2 grounderOffset = new Vector2(0f, -1f), wallCheckOffset = new Vector2(0.5f, 0f);
    [SerializeField] private float wallCheckRadius = 0.05f, grounderRadius = 0.2f;
    private bool isAgainstLeftWall, isAgainstRightWall, pushingLeftWall, pushingRightWall;
    private bool isGrounded;
    public static event Action OnTouchedGround;

    private readonly Collider2D[] ground = new Collider2D[1];
    private readonly Collider2D[] leftWall = new Collider2D[1];
    private readonly Collider2D[] rightWall = new Collider2D[1];

    private void HandleGrounding()
    {
        bool grounded = Physics2D.OverlapCircleNonAlloc(transform.position + new Vector3(grounderOffset.x, grounderOffset.y), grounderRadius, ground, groundMask) > 0;

        if (!isGrounded && grounded)//je viens de toucher le sol
        {
            isGrounded = true;
            hasDashed = false;
            hasJumped = hasDoubleJumped = isJumping = false;
            rb.gravityScale = gravityScale;
            PlayRandomClip(landClips);
            //anim.SetBool("Grounded", true);
            OnTouchedGround?.Invoke();
        }
        else if (isGrounded && !grounded)//je viens de décoller du sol
        {
            isGrounded = false;
            timeLeftGrounded = Time.time;
            //anim.SetBool("Grounded", false);
        }

        // Wall detection
        isAgainstLeftWall = Physics2D.OverlapCircleNonAlloc(transform.position + new Vector3(-wallCheckOffset.x, wallCheckOffset.y), wallCheckRadius, leftWall, groundMask) > 0f;
        isAgainstRightWall = Physics2D.OverlapCircleNonAlloc(transform.position + new Vector3(wallCheckOffset.x, wallCheckOffset.y), wallCheckRadius, rightWall, groundMask) > 0f;
        pushingLeftWall = isAgainstLeftWall && inputs.x < 0f;
        pushingRightWall = isAgainstRightWall && inputs.x > 0f;
    }

    private void DrawGrounderGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + new Vector3(grounderOffset.x, grounderOffset.y), grounderRadius);
    }

    private void OnDrawGizmosSelected()
    {
        DrawGrounderGizmos();
        DrawWallSlideGizmos();
    }

    #endregion

    #region Walking

    [Header("Walking")] [SerializeField] private float walkSpeed = 10f;//vitesse max de marche
    [SerializeField] private float airWalkSpeed = 5f;
    [SerializeField] private float acceleration = 30f;
    [Tooltip("accel selon le %age de vitesse")] [SerializeField] private AnimationCurve walkAcceleration;
    [SerializeField] private float deceleration = 100f;
    [SerializeField] private float airDeceleration = 5f;
    [SerializeField] [Range(0, 1)] private float airControl = 0.5f;

    private void HandleWalking()
    {
        if (isGrabbing || isDashing)
            return;

        float tmpAccel = Mathf.Abs(inputs.x) * (isGrounded ? acceleration : airControl * acceleration);
        float tmpMaxSpeed = isGrounded ? walkSpeed : airWalkSpeed;
        float speedVar = 0f;

        if (hitLeft)
        {
            if (rb.velocity.x > 0f)
                rb.velocity = new Vector2(0f, rb.velocity.y); // Immediate stop and turn. Just feels better
            if(Mathf.Abs(rb.velocity.x) >= tmpMaxSpeed)//On décélère
            {
                speedVar = Time.fixedDeltaTime * (isGrounded ? deceleration : airDeceleration);
            }
            else
            {
                float coeff = walkAcceleration.Evaluate(Mathf.Abs(rb.velocity.x / tmpMaxSpeed));
                speedVar = -coeff * tmpAccel * Time.fixedDeltaTime;
            }
        }
        else if (hitRight)
        {
            if (rb.velocity.x < 0f)
                rb.velocity = new Vector2(0f, rb.velocity.y); // Immediate stop and turn. Just feels better
            if (Mathf.Abs(rb.velocity.x) >= tmpMaxSpeed)
            {
                speedVar = -Time.fixedDeltaTime * (isGrounded ? deceleration : airDeceleration);
            }
            else
            {
                float coeff = walkAcceleration.Evaluate(Mathf.Abs(rb.velocity.x / tmpMaxSpeed));
                speedVar = coeff * tmpAccel * Time.fixedDeltaTime;
            }
        }
        else
        {
            float tmpDecel = Time.fixedDeltaTime * (isGrounded ? deceleration : airDeceleration);
            if (Mathf.Abs(rb.velocity.x) < 3f * tmpDecel)
            {
                speedVar = 0f;
                rb.velocity = new Vector2(0f, rb.velocity.y);
            }
            else
            {
                speedVar = -rb.velocity.x.Sign() * tmpDecel;
            }
        }
        rb.velocity = new Vector2(rb.velocity.x + speedVar, rb.velocity.y);
    }

    #endregion

    #region Jumping

    [Header("Jumping")] [SerializeField] private float gravityScale = 1f;
    [Tooltip("l'impulsion de départ")][SerializeField] private float jumpImpulsion = 15f;
    [Tooltip("la durée du saut(sec)")] [SerializeField] private float jumpLength = 1f;
    [Tooltip("force tout au ling du saut")] [SerializeField] private float jumpForce = 10f;
    [Tooltip("répartition force du saut au court du temps")] [SerializeField] private AnimationCurve jumpForceOverTime;
    [SerializeField] private float doubleJumpImpulsion = 15f;
    [SerializeField] private float doubleJumpLength = 1f;
    [SerializeField] private float doubleJumpForce = 10f;
    [SerializeField] private AnimationCurve doubleJumpForceOverTime;
    [SerializeField] private float fallSpeed = 7.5f;
    [Tooltip("Accel pour ne pas dépasser la fallSpeed")][SerializeField] private float antiGravity = 2f;
    [Tooltip("Durée ou l'on peut pas grab apres un wall jump")][SerializeField] private float wallJumpLock = 0.25f;
    [Tooltip("Time where we can jump after leave a plateform")][SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private bool enableDoubleJump = true;
    [SerializeField] private ParticleSystem jumpParticles;
    [SerializeField] private ParticleSystem doubleJumpParticles;

    private float timeLeftGrounded = -10f;
    private float timeLastWallJumped;
    private float timeLastJump;
    private bool isJumping, hasJumped, hasDoubleJumped;

    private void HandleJumping()
    {
        if (isDashing)
            return;

        if (hitJump)
        {
            //saut contre un mur
            if (isGrabbing || (!isGrounded && (isAgainstLeftWall || isAgainstRightWall)))
            {
                if(!hasDoubleJumped)
                {
                    timeLastJump = timeLastWallJumped = Time.time;
                    float angle = isAgainstLeftWall ? Mathf.PI * 0.25f : 0.75f * Mathf.PI;
                    float tmpJumpForce = hasJumped ? doubleJumpImpulsion : jumpImpulsion;
                    Vector2 jumpDir = new Vector2(tmpJumpForce * Mathf.Cos(angle), tmpJumpForce);
                    ExecuteJump(jumpDir, hasJumped); // Wall jump
                }
            }
            //Jump normal ou double saut
            else if (isGrounded || (Time.time < timeLeftGrounded + coyoteTime || (enableDoubleJump && !hasDoubleJumped)))
            {
                if (!hasJumped || (!hasDoubleJumped && enableDoubleJump))
                {
                    timeLastJump = Time.time;
                    float tmpJumpForce = hasJumped ? doubleJumpImpulsion : jumpImpulsion;
                    Vector2 jumpDir = new Vector2(rb.velocity.x, Mathf.Max(tmpJumpForce + rb.velocity.y, tmpJumpForce));
                    ExecuteJump(jumpDir, hasJumped);
                }
            }
        }

        void ExecuteJump(Vector3 dir, bool doubleJump = false)
        {
            rb.velocity = dir;
            rb.gravityScale = gravityScale;
            if (doubleJump)
                if (doubleJumpParticles != null)
                    doubleJumpParticles.Play();
            else
                if (jumpParticles != null)
                    jumpParticles.Play();
            //_anim.SetTrigger(doubleJump ? "DoubleJump" : "Jump");
            hasDoubleJumped = doubleJump;
            isJumping = hasJumped = true;
        }

        if(isJumping)
        {
            if (hitJump)
            {
                if (hasDoubleJumped)
                {
                    float percent = (Time.time - timeLastJump) / doubleJumpLength;
                    if (percent >= 1f)
                    {
                        isJumping = false;
                    }
                    else
                    {
                        float speedToAdd = doubleJumpForceOverTime.Evaluate(percent) * doubleJumpForce * Time.fixedDeltaTime;
                        rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y + speedToAdd);
                    }
                }
                else
                {
                    float percent = (Time.time - timeLastJump) / jumpLength;
                    if (percent >= 1f)
                    {
                        isJumping = false;
                    }
                    else
                    {
                        float speedToAdd = jumpForceOverTime.Evaluate(percent) * jumpForce * Time.fixedDeltaTime;
                        rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y + speedToAdd);
                    }
                }
            }
            else
                isJumping = false;
        }
        //fallspeed
        if (Mathf.Abs(rb.velocity.y) > fallSpeed && rb.velocity.y < 0f)
        {
            //add antiGravity
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y + antiGravity * Time.fixedDeltaTime);
            rb.gravityScale = 0f;
        }
    }

    #endregion

    #region Wall Slide

    [Header("Wall Slide")] [SerializeField]
    private ParticleSystem wallSlideParticles;
    [Tooltip("la vistesse de glissement")][SerializeField] private float slideSpeed = 4f;
    [Tooltip("la vistesse de décélération")][SerializeField] private float slideDeceleration = 20f;
    private bool isSliding;

    private void HandleWallSlide()
    {
        bool sliding = (pushingLeftWall || pushingRightWall) && !isGrabbing;

        //Si on commence de glisser sur le mur
        if (sliding && !isSliding)
        {
            //transform.SetParent(_pushingLeftWall ? _leftWall[0].transform : _rightWall[0].transform);//jsp a quoi ca sert?
            isSliding = true;
            isJumping = false;
            rb.gravityScale = 0f;
            if (wallSlideParticles != null)
            {
                wallSlideParticles.transform.position = transform.position + new Vector3(pushingLeftWall ? -wallCheckOffset.x : wallCheckOffset.x, wallCheckOffset.y, 0f);
                wallSlideParticles.Play();
            }
        }
        else if (!sliding && isSliding && !isGrabbing)//on s'arrete de glisser
            StopSliding();
        if(isSliding)
        {
            if(Mathf.Abs(rb.velocity.y + slideSpeed) <= 3f * slideDeceleration * Time.fixedDeltaTime)
                rb.velocity = new Vector2(0f, -slideSpeed);
            else if(rb.velocity.y > -slideSpeed)
                rb.velocity = new Vector2(0f, rb.velocity.y - slideDeceleration * Time.fixedDeltaTime);
            else
                rb.velocity = new Vector2(0f, rb.velocity.y + slideDeceleration * Time.fixedDeltaTime);

            if(!(isAgainstRightWall && hitRight) || !(isAgainstLeftWall && hitLeft))
                StopSliding();
        }

        void StopSliding()
        {
            isSliding = false;
            rb.gravityScale = gravityScale;
            if (wallSlideParticles != null)
                wallSlideParticles.Stop();
        }
    }

    private void DrawWallSlideGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + new Vector3(-wallCheckOffset.x, wallCheckOffset.y, 0f), wallCheckRadius);
        Gizmos.DrawWireSphere(transform.position + new Vector3(wallCheckOffset.x, wallCheckOffset.y, 0f), wallCheckRadius);
    }

    #endregion

    #region Wall Grab

    [Header("Wall Grab")][SerializeField] private ParticleSystem wallGrabParticles;
    [Tooltip("la vistesse max de la monter")][SerializeField] private float climbSpeed = 8f;
    [Tooltip("l'acceleration de la monter")][SerializeField] private float climbAccel = 30f;
    private bool isGrabbing;

    private void HandleWallGrab()
    {
        bool grabbing = (isAgainstLeftWall || isAgainstRightWall) && hitGrab && Time.time > timeLastWallJumped + wallJumpLock;

        if (grabbing && !isGrabbing)//On viens de s'accrocher
        {
            rb.gravityScale = gravityScale;
            isGrabbing = true;
            isJumping = isDashing = false;
            rb.gravityScale = 0f;
            if(wallGrabParticles != null)
            {
                wallGrabParticles.transform.position = transform.position + new Vector3(pushingLeftWall ? -wallCheckOffset.x : wallCheckOffset.x, wallCheckOffset.y);
                wallGrabParticles.Play();
            }
            SetFacingDirection(isAgainstLeftWall);
        }
        else if (!grabbing && isGrabbing)//on se décroche
        {
            rb.gravityScale = gravityScale;
            isGrabbing = false;
            if (wallGrabParticles != null)
                wallGrabParticles.Stop();
        }

        if (isGrabbing)
        {
            if (Mathf.Abs(rb.velocity.y - climbSpeed) < 2f * climbAccel * Time.fixedDeltaTime)
                rb.velocity = new Vector2(0f, climbSpeed);
            else
                rb.velocity += new Vector2(0f, climbAccel * Time.fixedDeltaTime);
        }
        //anim.SetBool("Climbing", wallSliding || isGrabbing);
    }

    #endregion

    #region Dash

    [Header("Dash")] [SerializeField] private float dashSpeed = 30f;
    [Tooltip("durée du dash(sec)")][SerializeField] private float dashLength = 0.2f;
    [SerializeField] private ParticleSystem dashParticles;
    [SerializeField] private ParticleSystem dashVisual;

    public static event Action OnStartDashing, OnStopDashing;

    private bool hasDashed, isDashing;
    private float timeStartedDash;
    private Vector3 dashDir;

    private void HandleDashing()
    {
        if (hitDash && !hasDashed)
        {
            Vector2 dashDir;
            if(Mathf.Abs(inputs.x) < Mathf.Epsilon && Mathf.Abs(inputs.y) < Mathf.Epsilon)
                dashDir = isFacingLeft ? Vector3.left : Vector3.right;
            else
                dashDir = new Vector3(inputs.x, inputs.y).normalized;
                
            //dashRing.up = dashDir; //jsp a quoi ca sert
            if(dashParticles != null)
                dashParticles.Play();
            isDashing = true;
            hasDashed = true;
            isSliding = isGrabbing = isJumping = false;
            timeStartedDash = Time.time;
            rb.gravityScale = 0f;
            if(dashVisual != null)
                dashVisual.Play();
            PlayRandomClip(dashClips);
            OnStartDashing?.Invoke();
        }

        if (isDashing)
        {
            rb.velocity = dashDir * dashSpeed;
            if (Time.time >= timeStartedDash + dashLength)
            {
                if(dashParticles != null)
                    dashParticles.Stop();
                isDashing = false;
                // Clamp the velocity so they don't keep shooting off
                rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y > 3f ? 3f : rb.velocity.y);
                rb.gravityScale = gravityScale;
                if (isGrounded)
                    hasDashed = false;
                if(dashVisual != null)
                    dashVisual.Stop();
                OnStopDashing?.Invoke();
            }
        }
    }

    #endregion

    #region Impacts

    [Header("Collisions")] [SerializeField] private ParticleSystem impactParticles;
    [SerializeField] private float minImpactForce = 2f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (impactParticles != null && collision.relativeVelocity.sqrMagnitude > minImpactForce * minImpactForce && isGrounded)
            impactParticles.Play();
    }

    #endregion

    #region Audio

    [Header("Audio")] [SerializeField] private AudioSource _source;
    [SerializeField] private AudioClip[] landClips;
    [SerializeField] private AudioClip[] dashClips;

    private void PlayRandomClip(AudioClip[] clips)
    {
        if(clips != null && clips.Length >= 1)
            _source.PlayOneShot(clips[Random.RandExclude(0, clips.Length)], 0.2f);
    }

    #endregion

    #region OnValidate

    private void OnValidate()
    {
        airWalkSpeed = Mathf.Max(0f, airWalkSpeed);
        acceleration = Mathf.Max(0f, acceleration);
        deceleration = Mathf.Max(0f, deceleration);
        airDeceleration = Mathf.Max(0f, airDeceleration);
        coyoteTime = Mathf.Max(0f, coyoteTime);
        doubleJumpForce = Mathf.Max(0f, doubleJumpForce);
        gravityScale = Mathf.Max(0f, gravityScale);
        jumpImpulsion = Mathf.Max(0f, jumpImpulsion);
        jumpLength = Mathf.Max(0f, jumpLength);
        jumpForce = Mathf.Max(0f, jumpForce);
        doubleJumpImpulsion = Mathf.Max(0f, doubleJumpImpulsion);
        doubleJumpLength = Mathf.Max(0f, doubleJumpLength);
        doubleJumpForce = Mathf.Max(0f, doubleJumpForce);
        fallSpeed = Mathf.Max(0f, fallSpeed);
        antiGravity = Mathf.Max(0f, antiGravity);
        wallCheckRadius = Mathf.Max(0f, wallCheckRadius);
        grounderRadius = Mathf.Max(0f, grounderRadius);
        slideSpeed = Mathf.Max(0f, slideSpeed);
        climbSpeed = Mathf.Max(0f, climbSpeed);
        dashSpeed = Mathf.Max(0f, dashSpeed);
        dashLength = Mathf.Max(0f, dashLength);
    }

    #endregion
}