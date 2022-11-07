using System.Collections;
using UnityEngine;
using DG.Tweening;

public class Movement : MonoBehaviour
{
    #region Attributs

    private Rigidbody2D rb;
    private AnimationScript anim;
    private Camera mainCam;
    private CustomPlayerInput playerInput;
    private Collider2D groundCollider;
    private MapColliderData groundColliderData;
    private FightController fightController;
    private int disableMovementCounter = 0;

    public bool enableInput = true;
    private bool _enableBehaviour = true;
    public bool enableBehaviour
    {
        get => _enableBehaviour;
        set
        {
            if (!value)
            {
                enableInput = _enableBehaviour = false;
            }
            else
                _enableBehaviour = true;
        }
    }

    [Header("Walk")]
    [Tooltip("La vitesse de marche")] [SerializeField] private float walkSpeed = 10f;
    [Tooltip("La vitesse d'interpolation de marche")] [SerializeField] private float speedLerp = 50f;
    [Tooltip("La propor de vitesse initiale de marche")] [SerializeField] [Range(0f, 1f)] private float initSpeed = 0.2f;
    [Tooltip("Le temps maximal entre l'appuie du joueur sur la touche est l'action engendré.")] [SerializeField] private float timeUntilCommandIsInvalid = 0.2f;

    [Header("Collision")]
    [SerializeField] private LayerMask groundLayer;
    public float groundCollisionRadius = 0.28f;
    public float sideCollisionRadius = 0.28f;
    public Vector2 groundOffset, sideOffset = new Vector2(0.15f, 0f);
    [Tooltip("La longueur de détection de la plateforme lors d'une monté en grab")] [SerializeField] private float grabRayLength = 1f;

    [Header("Jumping")]
    [Tooltip("La hauteur min du saut.")] [SerializeField] private float jumpInitForce = 20f;
    [Tooltip("L'accélération continue dû au saut.")] [SerializeField] private float jumpForce = 20f;
    [Tooltip("Modifie la gravité lorsqu'on monte en l'air mais sans sauter.")] [SerializeField] private float jumpGravityMultiplier = 1f;
    [Tooltip("La durée maximal ou le joueur peut avoir la touche de saut effective")] [SerializeField] private float jumpMaxDuration = 1f;
    [Tooltip("La vitesse maximal de saut (VMAX en l'air).")] [SerializeField] private Vector2 jumpSpeed = new Vector2(4f, 20f);
    [Tooltip("La vitesse init horizontale en saut (%age de la vitesse max)")] [Range(0f, 1f)] [SerializeField] private float jumpInitHorizontaSpeed = 0.4f;
    [Tooltip("La vitesse d'interpolation de la vitesse horizontale de saut")] [SerializeField] private float jumpSpeedLerp = 20f;
    [Tooltip("Le temps apres avoir quité la plateforme ou le saut est possible")] [SerializeField] private float jumpCoyoteTime = 0.1f;
    private float lastTimeLeavePlateform = -10f, lastTimeJumpCommand = -10f, lastTimeBeginJump;

    [Header("Air")]//en chute mais en phase montante
    [Tooltip("L'accélération continue dû au saut.")] [SerializeField] private float airGravityMultiplier = 1f;
    [Tooltip("La vitesse horizontale maximal en l'air (VMAX en l'air).")] [SerializeField] private float airHorizontalSpeed = 4f;
    [Tooltip("La vitesse init horizontale en l'air (%age de la vitesse max)")] [Range(0f, 1f)] [SerializeField] private float airInitHorizontaSpeed = 0.4f;
    [Tooltip("La vitesse d'interpolation de la vitesse horizontale en l'air")] [SerializeField] private float airSpeedLerp = 20f;

    [Header("Fall")]
    [Tooltip("Coeff ajustant l'accélération de chute.")] [SerializeField] private float fallGravityMultiplier = 1.5f;
    [Tooltip("Vitesse initial horizontale en chute (%age de vitesse max)")] [SerializeField] [Range(0f, 1f)] private float fallInitHorizontalSpeed = 0.35f;
    [Tooltip("La vitesse maximal de chute (VMAX en l'air).")] [SerializeField] private Vector2 fallSpeed = new Vector2(4f, 20f);
    [Tooltip("La vitesse d'interpolation de la vitesse horizontale de chute")] [SerializeField] private float fallSpeedLerp = 10f;
    [Tooltip("La vitesse d'interpolation de la réduction de vitesse horizontale de chute")] [SerializeField] private float fallDecelerationSpeedLerp = 10f;
    [Tooltip("La gravité lors du début de la chute")] [SerializeField] private float beginFallExtraGravity = 2f;
    [Tooltip("Définie le %age de vitesse on l'on considère un début de chute.")] [SerializeField] [Range(0f, 1f)] private float maxBeginFallSpeed = 0.3f;
    [Tooltip("Change la vitesse maximal de chute lors de l'appuie sur le bouton bas.")] [SerializeField] private float fallClampSpeedMultiplierWhenDownPressed = 1.2f;
    [Tooltip("Change la gravité appliqué lors de l'appuie sur la touche bas.")] [SerializeField] private float fallGravityMultiplierWhenDownPressed = 2f;

    [Header("Wall jump Opposite Wall")]
    [Tooltip("la vitesse de début de saut de mur")] [SerializeField] private float wallJumpInitForce = 50f;
    [Tooltip("L'angle en degré par rapport à la vertical de la direction du wall jump")] [Range(0f, 90f)] [SerializeField] private float wallJumpAngle = 45f;
    [Tooltip("L'accélération continue du au saut depuis le mur.")] [SerializeField] private float wallJumpForce = 20f;
    [Tooltip("La vitesse maximal de saut depuis le mur (VMAX en l'air).")] [SerializeField] private Vector2 wallJumpSpeed = new Vector2(4f, 20f);
    [Tooltip("Modifie la gravité lorsqu'on monte en l'air mais sans sauter.")] [SerializeField] private float wallJumpGravityMultiplier = 1f;
    [Tooltip("La durée maximal ou le joueur peut avoir la touche de saut effective")] [SerializeField] private float wallJumpMaxDuration = 1f;
    private float lastTimeBeginWallJump = -10f;

    [Header("Wall Jump Along Wall")]
    [Tooltip("la vitesse de début de saut de mur face au mur")] [SerializeField] private float wallJumpAlongSpeed = 20f;
    [Tooltip("la courbe de vitesse saut de mur face au mur")] [SerializeField] private AnimationCurve wallJumpAlongCurveSpeed;
    [Tooltip("La durée d'un saut face au mur")] [SerializeField] private float jumpAlongWallDuration = 0.3f;
    [Tooltip("Le temps minimal entre 2 saut face au mur (sec)")] [SerializeField] private float wallJumpAlongCooldown = 0.1f;
    private float lastTimeBeginWallJumpAlongWall = -10f;

    [Header("Grab")]
    [Tooltip("La vitesse de monter.")] [SerializeField] private float grabSpeed = 6f;
    [Tooltip("La vitesse initial de monter (%age de la vitesse max)")] [SerializeField] [Range(0f, 1f)] private float grabInitSpeed = 0.25f;
    [Tooltip("Réduit la vitesse de monter lorsque l'input est faible pour plus de précisions.")] [SerializeField] [Range(0f, 1f)] private float grabSpeedMultiplierWhenPreciseGrab = 6f;
    [Tooltip("La vitesse en sortie du wall grab.")] [SerializeField] private Vector2 grabApexSpeed = new Vector2(3f, 12f);
    [Tooltip("La deuxième vitesse en sortie du wall grab.")] [SerializeField] private Vector2 grabApexSpeed2 = new Vector2(4f, 0f);
    [Tooltip("La vitesse d'interpolation en entrée du wall grab.")] [SerializeField] private float grabSpeedLerp = 2f;
    [Tooltip("La vitesse d'interpolation en sortie du wall grab.")] [SerializeField] private float grabApexSpeedLerp = 400f;
    [Tooltip("La deuxième vitesse d'interpolation en sortie du wall grab.")] [SerializeField] private float grabApexSpeedLerp2 = 50f;
    [Tooltip("La durée du l'aide à la monté")] [SerializeField] private float grabApexDuration = 0.125f;
    [Tooltip("La deuxième durée du l'aide à la monté")] [SerializeField] private float grabApexDuration2 = 0.1f;
    [Tooltip("La vitesse initiale lors d'un grab apex a vitesse initiale nul")] [Range(0f, 1f)] [SerializeField] private float grabInitSpeedOnApexStay = 0.4f;
    private bool reachGrabApex = false, reachGrabApexRight = false, reachGrabApexLeft = false, isGrabApexEnable = false, isGrabApexRightEnable = false, isGrabApexLeftEnable = false, grabStayAtApex = false, grabStayAtApexRight = false, grabStayAtApexLeft = false;
    private bool finishGrabSpeed1 = false;
    private float timeReachGrabApex = 0f;

    [Header("Dash")]
    [Tooltip("La vitesse maximal du dash")] [SerializeField] private float dashSpeed = 20f;
    [Tooltip("La durée du dash en sec")] [SerializeField] private float dashDuration = 0.4f;
    [Tooltip("Le décalage temporel de l'invincibilité du dash en sec")] [SerializeField] private float invicibilityOffsetTime = 0.1f;
    [Tooltip("Le temps durant lequel un dash est impossible après avoir fini un dash")] [SerializeField] private float dashCooldown = 0.15f;
    [Tooltip("La durée d'invicibilité")] [SerializeField] private float dashInvicibilityDuration = 0.2f;
    [Tooltip("La courbe de vitesse de dash")] [SerializeField] private AnimationCurve dashSpeedCurve;
    private Vector2 lastDashDir;
    private float lastTimeDashCommand = -10f, lastTimeDashFinish = -10f, lastTimeDashBegin = -10f;

    [Header("Slide")]
    [Tooltip("La vitesse de glissement sur les murs")] [SerializeField] private float slideSpeed = 5f;
    [Tooltip("L'interpolation lorsqu'on glisse sur un mur.")] [SerializeField] private float slideSpeedLerp = 10f;
    [Tooltip("L'interpolation lorsqu'on ralentie en glissant sur un mur.")] [SerializeField] private float slideSpeedLerpDeceleration = 55f;
    [Tooltip("La vitesse initiale de glissement en %age de vitesse max lorsqu'on glisse a partir de 0.")] [SerializeField] [Range(0f, 1f)] private float initSlideSpeed = 0.1f;

    [Header("Polish")]
    [SerializeField] private ParticleSystem dashParticle;
    [SerializeField] private ParticleSystem jumpParticle;
    [SerializeField] private ParticleSystem wallJumpParticle;
    [SerializeField] private ParticleSystem slideParticle;

    public bool isGrounded { get; private set; } //le joueur touche le sol
    public bool onWall { get; private set; } //le joueur touche un mur a droite ou a gauche
    public bool onRightWall { get; private set; } // touche le mur droit
    public bool onLeftWall { get; private set; }//touche le mur gauche
    public bool wallGrab { get; private set; } //accroche ou monte le mur droit/gauche, monte au dessus d'une plateforme
    public bool canMove { get; private set; } = true; 
    public bool isDashing { get; private set; } 
    public bool isSliding { get; private set; }//grab vers le bas ou chute contre un mur en appuyant la direction vers le mur
    public bool isJumping { get; private set; }//dans la phase montante apres un saut
    public bool isJumpingAlongWall { get; private set; } //dans la phase montante d'un saut face au mur
    public bool isWallJumping { get; private set; } //dans la phase montante d'un saut depuis un mur
    public bool isFalling { get; private set; } //est en l'air sans saut ni grab ni rien d'autre.

    public int wallSide { get; private set; }

    //old
    private bool oldOnWall, oldOnGround = false;

    private bool groundTouch = false;//vaut true la frame ou l'on touche le sol
    private bool wallJump, jump, wallJumpAlongWall, dash;//vaut true la frame ou l'action est faite;
    private bool hasDashed = false;

    public bool doJump { get; private set; }
    public bool doDash { get; private set; }

    [HideInInspector] public int side = 1;

    #endregion

    #region public methods

    public Vector2 GetCurrentDirection() => ((playerInput.rawX != 0 || playerInput.rawY != 0) ? new Vector2(playerInput.x, playerInput.y).normalized : new Vector2(side, 0f));

    public void Teleport(in Vector2 newPosition)
    {
        transform.position = newPosition;
    }

    public void Freeze()
    {
        rb.velocity = Vector2.zero;
        enableBehaviour = false;
    }

    public void UnFreeze()
    {
        enableBehaviour = enableInput = true;
    }

    public void AddForce(in Vector2 dir, in float value) => AddForce(dir * value);

    public void AddForce(in Vector2 force)
    {
        rb.velocity += force;
    }

    #endregion

    #region Awake

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<AnimationScript>();
        mainCam = Camera.main;
        playerInput = GetComponent<CustomPlayerInput>();
        fightController = GetComponent<FightController>();
    }

    #endregion

    #region Update

    private void Update()
    {
        // I-Collision/detection
        groundCollider = Physics2D.OverlapCircle((Vector2)transform.position + groundOffset, groundCollisionRadius, groundLayer);
        isGrounded = groundCollider != null;
        onRightWall = Physics2D.OverlapCircle((Vector2)transform.position + sideOffset, sideCollisionRadius, groundLayer) != null;
        onLeftWall = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(-sideOffset.x, sideOffset.y), sideCollisionRadius, groundLayer) != null;
        onWall = onRightWall || onLeftWall;
        wallSide = onRightWall ? -1 : 1;

        //Trigger leave plateform
        if (oldOnGround && !isGrounded)
        {
            lastTimeLeavePlateform = Time.time;
        }

        //Trigger groundTouch
        if (isGrounded && !oldOnGround)
        {
            GroundTouch();
            groundTouch = true;
        }
        //enable dash
        if(hasDashed && isGrounded)
        {
            hasDashed = false;
        }

        //Clear collider data
        if (!isGrounded && oldOnGround)
        {
            groundColliderData = null;
        }
        oldOnGround = isGrounded;

        // II-Grab

        //Trigger wallGrab
        if (!wallGrab && onWall && (playerInput.grabPressed && enableInput) && (!isSliding || (playerInput.rawY >= 0 && enableInput)) && !isDashing && !isJumpingAlongWall && canMove)
        {
            if (side != wallSide)
            {
                anim.Flip(side * -1);
            }
            wallGrab = true;
            isFalling = isJumping = isSliding = false;
        }

        //Trigger reach grab Apex
        if (oldOnWall && !onWall && wallGrab && !reachGrabApex && !isDashing && !isJumping && !isSliding)
        {
            reachGrabApex = true;
            timeReachGrabApex = Time.time;
            FloorOnSide(out reachGrabApexRight, out reachGrabApexLeft);
        }
        oldOnWall = onWall;

        //Release wallGrab
        if (((wallGrab || grabStayAtApex) && playerInput.grabPressedUp) || (!onWall && !grabStayAtApex && !reachGrabApex && !isGrabApexEnable) || dash  || jump || wallJump || wallJumpAlongWall || !canMove)
        {
            wallGrab = grabStayAtApex = grabStayAtApexLeft = grabStayAtApexRight = false;
        }

        // III-Fall and Jump

        //Trigger falling
        if (!isFalling && !isJumping && !isWallJumping && !isJumpingAlongWall && !wallGrab && !isSliding && !isDashing && !isGrounded)
        {
            isFalling = true;
        }
        //release jumping and falling
        if ((isGrounded && rb.velocity.y <= Mathf.Epsilon) || wallGrab || dash || isSliding)
        {
            isJumping = isWallJumping = isJumpingAlongWall = isFalling = false;
        }
        //Release Falling
        if(jump || wallJump)
        {
            isFalling = false;
        }

        //release jumping and trigger falling
        if (isJumping && (rb.velocity.y <= 0f || !playerInput.jumpPressed || (Time.time - lastTimeBeginJump > jumpMaxDuration)))
        {
            isJumping = false;
            //cond  || rb.v.y > 0f pour éviter un bug ou la touche saut est activé une seul frame!, ainsi le saut est tellement cour que isGrounded est tj vrai
            if ((!isGrounded || rb.velocity.y > 0f) && !wallGrab && !isDashing && !isSliding)
            {
                isFalling = true;
            }
        }
        //release Wall jumping and trigger falling
        if (isWallJumping && (rb.velocity.y <= 0f || !playerInput.jumpPressed || (Time.time - lastTimeBeginWallJump > wallJumpMaxDuration)))
        {
            isWallJumping = false;
            //cond  || rb.v.y > 0f pour éviter un bug ou la touche saut est activé une seul frame!, ainsi le saut est tellement cour que isGrounded est tj vrai
            if ((!isGrounded || rb.velocity.y > 0f) && !wallGrab && !isDashing && !isSliding)
            {
                isFalling = true;
            }
        }
        //release Wall jumping allog and trigger falling
        if (isJumpingAlongWall && (isDashing || (Time.time - lastTimeBeginWallJumpAlongWall > jumpAlongWallDuration)))
        {
            isJumpingAlongWall = false;
            if (!isGrounded && !wallGrab && !isDashing && !isSliding)
            {
                isFalling = true;
            }
        }

        // IV-Slide

        //Trigger sliding
        //1case, wallGrab => isSliding
        if (!isSliding && wallGrab && playerInput.rawY == -1 && enableInput && !reachGrabApex && !isDashing && !isJumping && !isFalling && !isGrounded)
        {
            isSliding = true;
            wallGrab = grabStayAtApex = grabStayAtApexRight = grabStayAtApexLeft = reachGrabApex = isGrabApexEnable = isGrabApexLeftEnable = isGrabApexRightEnable =  false;
        }
        //2case, isFalling => isSliding
        if (!isSliding && onWall && isFalling && rb.velocity.y < 0f && !isDashing && !isJumping && !wallGrab)
        {
            if (enableInput && (playerInput.rawX == 1 && onRightWall) || (playerInput.rawX == -1 && onLeftWall))
            {
                isSliding = true;
                isFalling = false;
            }
        }

        //Release sliding
        //stop slide on the wall
        if(isSliding && (!enableInput || isGrounded || !onWall || (!playerInput.grabPressed && ((onRightWall && playerInput.rawX != 1) || (onLeftWall && playerInput.rawX != -1))) || isDashing || jump || wallJump))
        {
            isSliding = false;
        }
        if(isSliding && wallGrab)
        {
            isSliding = false;
        }

        // V Dash
        if(isDashing && (Time.time - lastTimeDashBegin) > dashDuration)
        {
            isDashing = false;
            lastTimeDashFinish = Time.time;
        }

        // VI-Inputs
        doJump = doJump ? true : playerInput.jumpPressedDown && enableInput;
        if (playerInput.jumpPressedDown && enableInput)
        {
            lastTimeJumpCommand = Time.time;
        }

        doDash = doDash ? true : playerInput.dashPressedDown && enableInput;
        if (playerInput.dashPressedDown && enableInput)
        {
            lastTimeDashCommand = Time.time;
        }

        //Polish 
        WallParticle(playerInput.y);

        //Animation flip
        if (playerInput.x > 0f && enableInput)
        {
            side = 1;
            anim.Flip(side);
        }
        if (playerInput.x < 0f && enableInput)
        {
            side = -1;
            anim.Flip(side);
        }

        // VII-old / trigger
        wallJump = dash = jump = wallJumpAlongWall = false;

        // VIII-Debug
        
        int count = isJumping ? 1 : 0;
        count = isFalling ? count + 1 : count;
        count = wallGrab ? count + 1 : count;
        count = isSliding ? count + 1 : count;
        if(count > 1)
        {
            print("!bug : " + "isJumping : " + isJumping + " isFalling : " + isFalling + " wallGrab : " + wallGrab + " isSliding : " + isSliding);
        }

        /*
        DebugText.instance.text += rb.velocity + ", " + rb.velocity.magnitude.Round(1) + " m/s\n";
        DebugText.instance.text += "isDashing : " + isDashing + "\n";
        DebugText.instance.text += "isJumping : " + isJumping + "\n";
        DebugText.instance.text += "isWallJumping : " + isWallJumping + "\n";
        DebugText.instance.text += "isWallJumpingAlong : " + isJumpingAlongWall + "\n";
        DebugText.instance.text += "isFalling : " + isFalling+ "\n";
        DebugText.instance.text += "wallGrab : " + wallGrab + "\n";
        DebugText.instance.text += "isSliding : " + isSliding + "\n";
        DebugText.instance.text += "GrabstayAtApex : " + grabStayAtApex + "\n";
        DebugText.instance.text += "isGrabApexEnable : " + isGrabApexEnable + "\n";
        DebugText.instance.text += "reachGrabApex : " + reachGrabApex + "\n";
        */
    }
    
    #endregion

    #region fixedUpdate

    private void FixedUpdate()
    {
        HandleWalk();

        HandleGrab();

        HandleWallSlide();

        HandleJump();

        HandleFall();

        HandleDash();

        //pour éviter les mess ! dans la console
        if(groundTouch)
            groundTouch = false;
        else
            groundTouch = false;
    }

    #endregion

    #region Handle Walk

    private void HandleWalk()
    {
        if (!enableInput || !isGrounded || !canMove || wallGrab || reachGrabApex || grabStayAtApex || isDashing || isJumping || isFalling || isSliding)
            return;

        #region Debug

        bool ok = false;
        if (playerInput.x > 0.01f && playerInput.rawX == 1)
        {
            //print("x OK");
            ok = true;
        }
        if (playerInput.x < -0.01f && playerInput.rawX == -1)
        {
            //print("x OK");
            ok = true;
        }
        if (playerInput.x <= 0.01f && playerInput.x >= -0.01f && playerInput.rawX == 0)
        {
            //print("x OK");
            ok = true;
        }
        if (!ok)
        {
            print("x : " + playerInput.x + " y : " + playerInput.y + " rawX : " + playerInput.rawX + " rawY : " + playerInput.rawY);
        }

        ok = false;
        if (playerInput.y > 0.01f && playerInput.rawY == 1)
        {
            //print("y OK");
            ok = true;
        }
        if (playerInput.y < -0.01f && playerInput.rawY == -1)
        {
            //print("y OK");
            ok = true;
        }
        if (playerInput.y <= 0.01f && playerInput.y >= -0.01f && playerInput.rawY == 0)
        {
            //print("y OK");
            ok = true;
        }
        if (!ok)
        {
            print("x : " + playerInput.x + " y : " + playerInput.y + " rawX : " + playerInput.rawX + " rawY : " + playerInput.rawY);
        }

        #endregion

        switch (groundColliderData.groundType)
        {
            case MapColliderData.GroundType.normal:
                HandleNormalWalk();
                break;
            case MapColliderData.GroundType.ice:
                HandleIceWalk();
                break;
            case MapColliderData.GroundType.trampoline:
                HandleNormalWalk();
                break;
            default:
                break;
        }
        
        void HandleNormalWalk()
        {
            //Clamp, on est dans le mauvais sens
            if ((playerInput.x >= 0f && rb.velocity.x <= 0f) || (playerInput.x <= 0f && rb.velocity.x >= 0f))
            {
                if (playerInput.rawX != 0)
                    rb.velocity = new Vector2(initSpeed * walkSpeed * playerInput.x.Sign(), rb.velocity.y);
                else
                    rb.velocity = new Vector2(0f, rb.velocity.y);
            }

            if (Mathf.Abs(rb.velocity.x) < initSpeed * walkSpeed * 0.95f && playerInput.rawX != 0)
                rb.velocity = new Vector2(initSpeed * walkSpeed * playerInput.x.Sign(), rb.velocity.y);
            else
                rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x, playerInput.x * walkSpeed, speedLerp * Time.fixedDeltaTime), rb.velocity.y);
        }

        void HandleIceWalk()
        {
            if (playerInput.rawX != 0f)
            {
                rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x, playerInput.x * walkSpeed,
                    GroundData.instance.iceSpeedLerpFactor * speedLerp * Time.fixedDeltaTime), rb.velocity.y);
            }
            else
            {
                rb.velocity = new Vector2(rb.velocity.x * 0.985f * (Time.fixedDeltaTime / 0.02f) , rb.velocity.y);
            }
        }
        
    }

    #endregion

    #region Handle Grab

    private void HandleGrab()
    {
        if (reachGrabApex)
        {
            if(!wallGrab || !enableInput || !canMove)
            {
                reachGrabApex = reachGrabApexLeft = reachGrabApexRight = isGrabApexEnable = isGrabApexRightEnable = isGrabApexLeftEnable = false;
            }
            else if (!isGrabApexEnable)
            {
                // si on donne un coup de pouce pour monter sur la plateforme
                if (playerInput.rawY == 1 && ((reachGrabApexRight && playerInput.rawX == 1) || (reachGrabApexLeft && playerInput.rawX == -1)))
                {
                    if (reachGrabApexRight && playerInput.rawX == 1)
                    {
                        isGrabApexRightEnable = true;
                        isGrabApexLeftEnable = false;
                    }
                    else //reachGrabApexLeft
                    {
                        isGrabApexRightEnable = false;
                        isGrabApexLeftEnable = true;
                    }
                    isGrabApexEnable = true;
                }
                else
                {
                    grabStayAtApex = true;
                    grabStayAtApexRight = reachGrabApexRight;
                    grabStayAtApexLeft = reachGrabApexLeft;
                    if(reachGrabApexRight && reachGrabApexLeft)
                    {
                        print("Debug reach grab apex left and right pls");
                    }
                    reachGrabApex = reachGrabApexLeft = reachGrabApexRight = isGrabApexRightEnable = isGrabApexLeftEnable = false;
                    rb.MovePosition((Vector2)transform.position + Vector2.down * (grabSpeed * Time.fixedDeltaTime));
                    rb.velocity = Vector2.zero;
                }
            }
            else
            {
                //on donne le coup de pouce
                Vector2 speedTarget = finishGrabSpeed1 ? grabApexSpeed2 : grabApexSpeed;
                float lerp = finishGrabSpeed1 ? grabApexSpeedLerp2 : grabApexSpeedLerp;
                if (isGrabApexRightEnable)
                {
                    rb.velocity = Vector2.MoveTowards(rb.velocity, speedTarget, lerp * Time.fixedDeltaTime);
                }
                else //isGrabApexLeftEnable
                {
                    rb.velocity = Vector2.MoveTowards(rb.velocity, new Vector2(-speedTarget.x, speedTarget.y), lerp * Time.fixedDeltaTime);
                }

                float duration = finishGrabSpeed1 ? grabApexDuration2 : grabApexDuration;
                if (Time.time - timeReachGrabApex >= duration)
                {
                    if (finishGrabSpeed1)
                    {
                        reachGrabApex = reachGrabApexLeft = reachGrabApexRight = isGrabApexEnable = isGrabApexRightEnable = isGrabApexLeftEnable = false;
                        finishGrabSpeed1 = false;
                    }
                    else
                    {
                        finishGrabSpeed1 = true;
                        timeReachGrabApex = Time.time;
                    }
                }
            }
        }

        //Si on est dans le cas normal de grab
        if (wallGrab && onWall && !grabStayAtApex && !reachGrabApex && !isGrabApexEnable && !isSliding)
        {
            //On veut monter
            if(playerInput.rawY == 1)
            {
                //clamp, on va dans le mauvais sens
                if (rb.velocity.y < grabInitSpeed * grabSpeed * 0.95f && playerInput.y > 0.01f)
                {
                    rb.velocity = new Vector2(0f, grabInitSpeed * grabSpeed * playerInput.y);
                }
                else
                {
                    float speedModifier = (playerInput.y > 0.5f ? 1f : grabSpeedMultiplierWhenPreciseGrab);
                    rb.velocity = new Vector2(0f, Mathf.MoveTowards(rb.velocity.y, grabSpeed * playerInput.y * speedModifier, grabSpeedLerp * Time.fixedDeltaTime));
                }
            }
            else
            {
                rb.velocity = Vector2.zero;
            }
        }
        //on est au sommet du mur
        else if (wallGrab && onWall && grabStayAtApex && !reachGrabApex && !isGrabApexEnable && !isSliding)
        {
            int xVal = grabStayAtApexRight ? 1 : -1;
            //On attend en haut du mur voulant monter au dessus
            if (playerInput.rawY == 1 && playerInput.rawX == xVal)
            {
                rb.velocity = new Vector2(0f, grabInitSpeedOnApexStay * grabSpeed);

                reachGrabApex = true;
                isGrabApexEnable = true;
                reachGrabApexRight = grabStayAtApexRight;
                reachGrabApexLeft = grabStayAtApexLeft;
                isGrabApexRightEnable = grabStayAtApexRight;
                isGrabApexLeftEnable = grabStayAtApexLeft;
                grabStayAtApex = grabStayAtApexRight = grabStayAtApexLeft = finishGrabSpeed1 = false;
                timeReachGrabApex = Time.time;
            }
        }
    }

    #endregion

    #region Handle Jump

    private void HandleJump()
    {
        if (doJump)
        {
            if (isGrounded && canMove)
            {
                Jump(Vector2.up);
                doJump = false;
            }
            else if ((grabStayAtApex || reachGrabApex || wallGrab) && !isGrounded && canMove)
            {
                WallJump();
                doJump = false;
            }
            else if (Time.time - lastTimeLeavePlateform <= jumpCoyoteTime && canMove)
            {
                Jump(Vector2.up);
                doJump = false;
            }
            else if (Time.time - lastTimeJumpCommand <= timeUntilCommandIsInvalid)
            {
                doJump = true;
            }
            else
            {
                doJump = false;
            }
        }

        if (isJumping || isWallJumping)
        {
            float gravityMultiplier, force;
            Vector2 speed;
            if(isJumping)
            {
                gravityMultiplier = jumpGravityMultiplier;
                force = jumpForce;
                speed = jumpSpeed;
            }
            else
            {
                gravityMultiplier = wallJumpGravityMultiplier;
                force = wallJumpForce;
                speed = wallJumpSpeed;
            }

            HandleJumpGravity(gravityMultiplier, force, speed);

            HandleHorizontalMovement(speed);
        }

        if(isJumpingAlongWall)
        {
            float per100 = (Time.time - lastTimeBeginWallJumpAlongWall) / jumpAlongWallDuration;
            rb.velocity = new Vector2(0f, wallJumpAlongSpeed * wallJumpAlongCurveSpeed.Evaluate(per100));
        }

        void HandleJumpGravity(in float GravityMultiplier, in float force, in Vector2 speed)
        {
            rb.velocity += Vector2.up * (Physics2D.gravity.y * GravityMultiplier * Time.fixedDeltaTime);

            //phase montante du saut
            if (playerInput.jumpPressed && enableInput)
            {
                rb.velocity += Vector2.up * (force * Time.fixedDeltaTime);
            }

            //clamp en y
            if (rb.velocity.y > speed.y)
            {
                rb.velocity = new Vector2(rb.velocity.x, speed.y);
            }
            //clamp en x
            if (Mathf.Abs(rb.velocity.x) > speed.x)
            {
                rb.velocity = new Vector2(speed.x * rb.velocity.x.Sign(), rb.velocity.y);
            }
        }

        void HandleHorizontalMovement(in Vector2 speed)
        {
            if (!enableInput)
                return;
            //Movement horizontal
            //Clamp, on est dans le mauvais sens
            if ((playerInput.x >= 0f && rb.velocity.x <= 0f) || (playerInput.x <= 0f && rb.velocity.x >= 0f))
                rb.velocity = new Vector2(jumpInitHorizontaSpeed * jumpSpeed.x * playerInput.x.Sign(), rb.velocity.y);

            if (Mathf.Abs(rb.velocity.x) < jumpInitHorizontaSpeed * speed.x * 0.95f && Mathf.Abs(playerInput.x) > 0.01f)
            {
                rb.velocity = new Vector2(jumpInitHorizontaSpeed * speed.x * playerInput.x.Sign(), rb.velocity.y);
            }
            else
            {
                float targetSpeed = playerInput.x * speed.x;
                rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x, targetSpeed, jumpSpeedLerp * Time.fixedDeltaTime), rb.velocity.y);
            }
        }
    }

    #region Jump

    private void Jump(in Vector2 dir)
    {
        if (slideParticle != null)
            slideParticle.transform.parent.localScale = new Vector3(ParticleSide(), 1, 1);

        Vector2 newVelocity;
        if(groundColliderData != null && groundColliderData.groundType == MapColliderData.GroundType.trampoline)
        {
            Trampoline t = groundColliderData.GetComponent<Trampoline>();
            Vector2 newDir = new Vector2(Mathf.Cos(t.angleDir * Mathf.Deg2Rad), Mathf.Sin(t.angleDir * Mathf.Deg2Rad));
            newVelocity = new Vector2(rb.velocity.x + newDir.x * t.force, newDir.y * t.force);
        }
        else
        {
            newVelocity = new Vector2(rb.velocity.x + dir.x * jumpInitForce, dir.y * jumpInitForce);
        }

        rb.velocity = newVelocity;
        isJumping = jump = true;
        lastTimeBeginJump = Time.time;

        if (jumpParticle != null)
            jumpParticle.Play();
    }

    private void WallJump()
    {
        bool right;
        if (onWall)
        {
            right = onRightWall;
            if (onRightWall == onLeftWall)
            {
                print("Debug pls1 : " + onRightWall);
                return;
            }
        }
        else //ApexJump ou on est accrocher tout en bas d'un mur
        {
            FloorOnSide(out right, out bool left);
            if (right == left)//!bug
            {
                print("Debug pls2 right : " + right);
                //evité de plus gros bug
                isSliding = wallGrab = wallJump = false;
                isFalling = true;
                return;
            }
        }

        //first case : jump along the wall
        if((right && playerInput.rawX >= 0) || (!right && playerInput.rawX <= 0) && wallGrab && playerInput.rawY >= 0)
        {
            if(Time.time - lastTimeBeginWallJumpAlongWall > wallJumpAlongCooldown)
            {
                rb.velocity = Vector2.up * (wallJumpAlongSpeed * wallJumpAlongCurveSpeed.Evaluate(0f));
                isJumpingAlongWall = wallJumpAlongWall = true;
                lastTimeBeginWallJumpAlongWall = Time.time;
            }
        }
        else //2nd case : jump on the oposite of the wall
        {
            float angle = (right ? 1f : -1f) * wallJumpAngle * Mathf.Deg2Rad + Mathf.PI * 0.5f;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.velocity += dir * wallJumpInitForce;
            isWallJumping = wallJump = true;
            lastTimeBeginWallJump = Time.time;

            side *= -1;
        }

        StopCoroutine(DisableMovement(0f));
        StartCoroutine(DisableMovement(0.1f));
    }

    #endregion

    #endregion

    #region Handle Fall

    private void HandleFall()
    {
        if (!isFalling || isJumping  || isWallJumping || isJumpingAlongWall || isSliding || wallGrab || !enableBehaviour)
            return;

        //phase montante en l'air
        if (rb.velocity.y > 0f)
        {
            //Gravity
            float coeff = playerInput.rawY == -1 && enableInput ? fallGravityMultiplierWhenDownPressed * airGravityMultiplier : airGravityMultiplier;
            rb.velocity += Vector2.up * (Physics2D.gravity.y * coeff * Time.fixedDeltaTime);

            //Movement horizontal
            //Clamp, on est dans le mauvais sens
            if (enableInput && (playerInput.x >= 0f && rb.velocity.x <= 0f) || (playerInput.x <= 0f && rb.velocity.x >= 0f))
                rb.velocity = new Vector2(airInitHorizontaSpeed * airHorizontalSpeed * playerInput.x.Sign(), rb.velocity.y);
            if (enableInput && Mathf.Abs(rb.velocity.x) < airInitHorizontaSpeed * airHorizontalSpeed * 0.95f && Mathf.Abs(playerInput.x) > 0.01f)
            {
                rb.velocity = new Vector2(airInitHorizontaSpeed * airHorizontalSpeed * playerInput.x.Sign(), rb.velocity.y);
            }
            else
            {
                float targetSpeed = !enableInput ? 0f : playerInput.x * airHorizontalSpeed;
                rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x, targetSpeed, airSpeedLerp * Time.fixedDeltaTime), rb.velocity.y);
            }

        }
        else//phase descendante
        {
            //Clamp the fall speed
            float targetedSpeed = (playerInput.rawY == -1 && enableInput) ? -fallSpeed.y * Mathf.Max(fallClampSpeedMultiplierWhenDownPressed * Mathf.Abs(playerInput.y), 1f) : -fallSpeed.y;
            if (rb.velocity.y < targetedSpeed)//slow
            {
                rb.velocity = new Vector2(rb.velocity.x, Mathf.MoveTowards(rb.velocity.y, targetedSpeed, fallDecelerationSpeedLerp * Time.fixedDeltaTime));
            }
            else
            {
                float coeff = rb.velocity.y >= -fallSpeed.y * maxBeginFallSpeed ? fallGravityMultiplier * beginFallExtraGravity : fallGravityMultiplier;
                coeff = enableInput && playerInput.rawY < 0 ? coeff * fallGravityMultiplierWhenDownPressed : coeff;
                rb.velocity = new Vector2(rb.velocity.x, Mathf.MoveTowards(rb.velocity.y, targetedSpeed, -Physics2D.gravity.y * coeff * Time.fixedDeltaTime));
                //rb.velocity += Vector2.up * (Physics2D.gravity.y * coeff * Time.fixedDeltaTime);
            }

            //Movement horizontal
            //Clamp, on est dans le mauvais sens
            if (enableInput && (playerInput.x >= 0f && rb.velocity.x <= 0f) || (playerInput.x <= 0f && rb.velocity.x >= 0f))
                rb.velocity = new Vector2(fallInitHorizontalSpeed * fallSpeed.x * playerInput.x.Sign(), rb.velocity.y);
            if (enableInput && Mathf.Abs(rb.velocity.x) < fallInitHorizontalSpeed * fallSpeed.x * 0.95f && Mathf.Abs(playerInput.x) > 0.01f)
            {
                rb.velocity = new Vector2(fallInitHorizontalSpeed * fallSpeed.x * playerInput.x.Sign(), rb.velocity.y);
            }
            else
            {
                float targetSpeed = !enableInput ? 0f : playerInput.x * fallSpeed.x;
                rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x, targetSpeed, fallSpeedLerp * Time.fixedDeltaTime), rb.velocity.y);
            }
        }
    }

    #endregion

    #region Handle Dash

    private void HandleDash()
    {
        //Dashing
        if (doDash && !isDashing)
        {
            if (!hasDashed && canMove && Time.time - lastTimeDashFinish >= dashCooldown)
            {
                Vector2 dir = GetCurrentDirection();
                Dash(dir);
                doDash = false;
            }
            else if (!hasDashed && Time.time - lastTimeDashCommand <= timeUntilCommandIsInvalid)
            {
                doDash = true;
            }
            else
            {
                doDash = false;
            }
        }

        if(isDashing)
        {
            lastTimeDashFinish = Time.time;
            float per100 = (Time.time - lastTimeDashBegin) / dashDuration;
            rb.velocity = lastDashDir * (dashSpeedCurve.Evaluate(per100) * dashSpeed);
        }
    }

    private void Dash(in Vector2 dir)
    {
        mainCam.transform.DOComplete();
        mainCam.transform.DOShakePosition(0.15f, 0.2f, 14, 90, false, true);
        //FindObjectOfType<RippleEffect>().Emit(mainCam.WorldToViewportPoint(transform.position));
        //anim.SetTrigger("dash");
        //FindObjectOfType<GhostTrail>().ShowGhost();

        lastDashDir = dir;
        rb.velocity = dir * dashSpeedCurve.Evaluate(0);
        lastTimeDashBegin = Time.time;
        hasDashed = isDashing = dash = true;
        StartCoroutine(DashInvicibility());
    }

    private IEnumerator DashInvicibility()
    {
        yield return Useful.GetWaitForSeconds(invicibilityOffsetTime);
        fightController.EnableInvicibility();
        yield return Useful.GetWaitForSeconds(dashInvicibilityDuration);
        fightController.DisableInvicibility();
    }

    private IEnumerator DisableMovement(float time)
    {
        disableMovementCounter++;
        canMove = false;
        yield return Useful.GetWaitForSeconds(time);
        disableMovementCounter--;
        canMove = disableMovementCounter <= 0;
    }

    #endregion

    #region Handle Wall Slide

    private void HandleWallSlide()
    {
        if (!isSliding)
            return;

        if (wallSide != side)
        {
            anim.Flip(side);
        }

        //ralentir le glissement
        if(rb.velocity.y <  -slideSpeed)
        {
            rb.velocity = new Vector2(0f, Mathf.MoveTowards(rb.velocity.y, -slideSpeed, slideSpeedLerpDeceleration * Time.fixedDeltaTime));
        }
        else if(rb.velocity.y > -slideSpeed * initSlideSpeed * 0.95f)
        {
            rb.velocity = new Vector2(0f, -slideSpeed * initSlideSpeed);
        }
        else
        {
            rb.velocity = new Vector2(0f, Mathf.MoveTowards(rb.velocity.y, -slideSpeed, slideSpeedLerp * Time.fixedDeltaTime));
        }
    }

    #endregion

    #region Detection

    private void GroundTouch()
    {
        groundColliderData = groundCollider.GetComponent<MapColliderData>();
        hasDashed = false;
        isJumping = false;
        isFalling = false;

        side = anim.sr.flipX ? -1 : 1;
        lastTimeLeavePlateform = -10f;
        if (jumpParticle != null)
            jumpParticle.Play();
    }

    private void FloorOnSide(out bool onRight, out bool onLeft)
    {
        onRight = onLeft = false;
        RaycastHit2D raycast = Physics2D.Raycast((Vector2)transform.position + groundOffset, Vector2.right, grabRayLength, groundLayer);
        if (raycast.collider != null)
            onRight = true;
        raycast = Physics2D.Raycast((Vector2)transform.position + groundOffset, Vector2.left, grabRayLength, groundLayer);
        if (raycast.collider != null)
            onLeft = true;
    }

    #endregion

    #region Particles

    void WallParticle(float vertical)
    {
        if (slideParticle == null)
            return;
        ParticleSystem.MainModule main = slideParticle.main;

        if (isSliding)
        {
            slideParticle.transform.parent.localScale = new Vector3(ParticleSide(), 1, 1);
            main.startColor = Color.white;
        }
        else
        {
            main.startColor = Color.clear;
        }
    }

    private int ParticleSide()
    {
        int particleSide = onRightWall ? 1 : -1;
        return particleSide;
    }

    #endregion

    #region Gizmos and OnValidate

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere((Vector2)transform.position + groundOffset, groundCollisionRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + sideOffset, sideCollisionRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + new Vector2(-sideOffset.x, sideOffset.y), sideCollisionRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawLine((Vector2)transform.position + groundOffset + (grabRayLength * Vector2.left), (Vector2)transform.position + groundOffset + (grabRayLength * Vector2.right));
    }

    private void OnValidate()
    {
        grabSpeed = Mathf.Max(0f, grabSpeed);
        walkSpeed = Mathf.Max(0f, walkSpeed);
        dashSpeed = Mathf.Max(0f, dashSpeed);
        fallSpeed = new Vector2(Mathf.Max(0f, fallSpeed.x), Mathf.Max(0f, fallSpeed.y));
        airHorizontalSpeed = Mathf.Max(0f, airHorizontalSpeed);
        jumpSpeed = new Vector2(Mathf.Max(0f, jumpSpeed.x), Mathf.Max(0f, jumpSpeed.y));
        grabApexSpeed = new Vector2(Mathf.Max(0f, grabApexSpeed.x), Mathf.Max(0f, grabApexSpeed.y));
        grabApexSpeed2 = new Vector2(Mathf.Max(0f, grabApexSpeed2.x), Mathf.Max(0f, grabApexSpeed2.y));
        slideSpeed = Mathf.Max(0f, slideSpeed);
        grabApexSpeedLerp = Mathf.Max(0f, grabApexSpeedLerp);
        grabSpeedLerp = Mathf.Max(0f, grabSpeedLerp);
        speedLerp = Mathf.Max(0f, speedLerp);
        grabRayLength = Mathf.Max(0f, grabRayLength);
        airSpeedLerp = Mathf.Max(0f, airSpeedLerp);
        jumpInitForce = Mathf.Max(0f, jumpInitForce);
    }

    #endregion
}
