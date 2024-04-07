using System.Collections;
using UnityEngine;
using DG.Tweening;

public class CharacterController : MonoBehaviour
{
    #region Fields

    private Rigidbody2D rb;
    private AnimationScript anim;
    private Camera mainCam;
    private CustomPlayerInput playerInput;
    private BoxCollider2D hitbox;
    private Collider2D groundCollider, oldGroundCollider, leftWallCollider, rightWallCollider;
    private MapColliderData groundColliderData, lastGroundColliderData, leftWallColliderData, rightWallColliderData;
    private FightController fightController;
    private int disableMovementCounter = 0;
    private new Transform transform;

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

    #if UNITY_EDITOR

    [SerializeField] private bool drawGizmos = true;

    #endif

    [Header("General")]
    [Tooltip("Le temps maximal entre l'appuie du joueur sur la touche est l'action engendré."), SerializeField] private float timeUntilCommandIsInvalid = 0.2f;

    [Header("Collision")]
    public float sideCollisionRadius = 0.28f;
    public Vector2 sideOffset = new Vector2(0.15f, 0f);
    [Tooltip("The offset on the vertical axis of the detection grab ray."),SerializeField] private float grabRayOffset = 1f;
    [Tooltip("Length of the detection for the grab"), SerializeField] private float grabRayLength = 1f;
    [Tooltip("Offset for the side ground raycast, also use to compute slopes")] public Vector2 groundRaycastOffset = Vector2.zero;
    [Tooltip("Length for the side and mid ground raycast, also use to compute slopes raycast"), SerializeField] public float groundRaycastLength = 0.5f;
    [SerializeField] private float gapBetweenHitboxAndGround = 0.1f;
    private LayerMask groundLayer;

    [Header("Walk")]
    [Tooltip("La vitesse de marche")] [SerializeField] private float walkSpeed = 10f;
    [Tooltip("La vitesse d'interpolation de marche")] [SerializeField] private float speedLerp = 50f;
    [Tooltip("La propor de vitesse initiale de marche")] [SerializeField] [Range(0f, 1f)] private float initSpeed = 0.2f;
    [Tooltip("La durée de conservation de la vitesse de marche apres avoir quitté une plateforme"), SerializeField] private float inertiaDurationWhenQuittingGround = 0.1f;
    private Vector2 localVelocityOnGrippingGround;
    private float lastTimeQuitGround = -10f;

    [Header("Jumping")]
    [Tooltip("La hauteur min du saut.")] [SerializeField] private float jumpInitForce = 20f;
    [Tooltip("L'accélération continue dû au saut.")] [SerializeField] private float jumpForce = 20f;
    [Tooltip("Modifie la gravité lorsqu'on monte en l'air mais sans sauter.")] [SerializeField] private float jumpGravityMultiplier = 1f;
    [Tooltip("La durée maximal ou le joueur peut avoir la touche de saut effective")] [SerializeField] private float jumpMaxDuration = 1f;
    [Tooltip("La vitesse maximal de saut (VMAX en l'air).")] [SerializeField] private Vector2 jumpSpeed = new Vector2(4f, 20f);
    [Tooltip("La vitesse init horizontale en saut (%age de la vitesse max)")] [Range(0f, 1f)] [SerializeField] private float jumpInitHorizontaSpeed = 0.4f;
    [Tooltip("La vitesse d'interpolation de la vitesse horizontale de saut")] [SerializeField] private float jumpSpeedLerp = 20f;
    [SerializeField] private bool enableDoubleJump = true;
    [Tooltip("The speed of the second jump(magnitude of his velocity vector)"), SerializeField] private float doubleJumpSpeed = 6f;
    [Tooltip("Le temps apres avoir quité la plateforme ou le saut est possible")] [SerializeField] private float jumpCoyoteTime = 0.1f;
    private float lastTimeLeavePlateform = -10f, lastTimeJumpCommand = -10f, lastTimeBeginJump = -10f;
    private bool isPressingJumpButtonDownForFixedUpdate;

    [Header("Air")]//en chute mais en phase montante
    [Tooltip("L'accélération continue dû au saut.")] [SerializeField] private float airGravityMultiplier = 1f;
    [Tooltip("La vitesse horizontale maximal en l'air (VMAX en l'air).")] [SerializeField] private float airHorizontalSpeed = 4f;
    [Tooltip("La vitesse init horizontale en l'air (%age de la vitesse max)")] [Range(0f, 1f)] [SerializeField] private float airInitHorizontalSpeed = 0.4f;
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
    [Tooltip("La durée minimale ou le joueur peut avoir la touche de saut effective")] [SerializeField] private float wallJumpMinDuration = 0.1f;
    [Tooltip("La durée maximal ou le joueur peut avoir la touche de saut effective")] [SerializeField] private float wallJumpMaxDuration = 1f;
    private float lastTimeBeginWallJump = -10f;

    [Header("Wall Jump Along Wall")]
    [SerializeField] private bool enableJumpAlongWall = true;
    [Tooltip("la vitesse de début de saut de mur face au mur")] [SerializeField] private float wallJumpAlongSpeed = 20f;
    [Tooltip("la courbe de vitesse saut de mur face au mur")] [SerializeField] private AnimationCurve wallJumpAlongCurveSpeed;
    [Tooltip("La durée d'un saut face au mur")] [SerializeField] private float jumpAlongWallDuration = 0.3f;
    [Tooltip("Le temps minimal entre 2 saut face au mur (sec)")] [SerializeField] private float wallJumpAlongCooldown = 0.1f;
    private float lastTimeBeginWallJumpAlongWall = -10f;

    [Header("Grab")]
    [Tooltip("La vitesse de monter.")] [SerializeField] private float grabSpeed = 6f;
    [Tooltip("La vitesse initial de monter (%age de la vitesse max)")] [SerializeField] [Range(0f, 1f)] private float grabInitSpeed = 0.25f;
    [Tooltip("La valeur de l'input en y max ou l'on est en precise grab"), SerializeField, Range(0f, 1f)] private float maxPreciseGrabValue = 0.3f;
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
    [Tooltip("Le temps durant lequel un dash est impossible après avoir fini un dash")] [SerializeField] private float dashCooldown = 0.15f;
    [Tooltip("La courbe de vitesse de dash")] [SerializeField] private AnimationCurve dashSpeedCurve;
    [Tooltip("%age de la hitbox qui est ignoré lors d'un dash vers le haut"), SerializeField, Range(0f, 0.5f)] private float antiKnockHead;
    private Vector2 lastDashDir;
    private bool isLastDashUp = false;
    private float lastTimeDashCommand = -10f, lastTimeDashFinish = -10f, lastTimeDashBegin = -10f;

    [Header("Slide")]
    [Tooltip("La vitesse de glissement sur les murs")] [SerializeField] private float slideSpeed = 5f;
    [Tooltip("L'interpolation lorsqu'on glisse sur un mur.")] [SerializeField] private float slideSpeedLerp = 10f;
    [Tooltip("L'interpolation lorsqu'on ralentie en glissant sur un mur.")] [SerializeField] private float slideSpeedLerpDeceleration = 55f;
    [Tooltip("La vitesse initiale de glissement en %age de vitesse max lorsqu'on glisse a partir de 0.")] [SerializeField] [Range(0f, 1f)] private float initSlideSpeed = 0.1f;

    [Header("Slope")]
    [Tooltip("The maximum angle in degres we can walk"), SerializeField, Range(0f, 90f)] private float maxSlopeAngle = 22.5f;
    [SerializeField] private float slopeSpeed;
    [SerializeField, Range(0f, 1f)] private float initSlopeSpeed = 0.3f;
    [SerializeField, Tooltip("La vitesse de monter selon l'angle de la pente : 0 => pente nulle, 1 => pente max en %ageVMAX")] private AnimationCurve slopeSpeedCurve;
    [SerializeField, Tooltip("acceleration during slope in %ageVMAX/sec ")] private float slopeSpeedLerp = 1f;
    [HideInInspector] public bool isSlopingRight, isSlopingLeft;
    private float slopeAngleLeft, slopeAngleRight;
    private bool isToSteepSlopeRight, isToSteepSlopeLeft;

    [Header("Bump")]
    [SerializeField] private float minBumpSpeedX = 1f;
    [SerializeField] private float maxFallBumpSpeed = 30f;
    [SerializeField] private float bumpFrictionLerp = 2f;
    [SerializeField] private float bumpGravityScale = 1f;
    [SerializeField] private float maxBumpDuration = 1.5f;
    [SerializeField] private float minBumpDuration = 0.3f;
    private float lastTimeBump = -10f;

    [Header("Map Object")]
    [SerializeField] private float inertiaDurationWhenQuittingConvoyerBelt = 0.08f;
    [SerializeField] private float speedWhenQuittingConvoyerBelt = 1f;
    private float lastTimeQuittingConvoyerBelt = -10f;
    private bool isQuittingConvoyerBelt, isQuittingConvoyerBeltRight, isQuittingConvoyerBeltLeft;

    [Header("Polish")]
    [SerializeField] private ParticleSystem dashParticle;
    [SerializeField] private ParticleSystem jumpParticle;
    [SerializeField] private ParticleSystem wallJumpParticle;
    [SerializeField] private ParticleSystem slideParticle;
    [SerializeField] private bool enableCameraShaking = true;
    [SerializeField] private ShakeSetting cameraShakeSetting = new ShakeSetting(0.15f, 0.2f, 14, 90, false, true);

    public bool isGrounded { get; private set; } //le joueur touche le sol
    public bool onWall { get; private set; } //le joueur touche un mur a droite ou a gauche
    public bool onRightWall { get; private set; } // touche le mur droit
    public bool onLeftWall { get; private set; }//touche le mur gauche
    public bool wallGrab { get; private set; } //accroche ou monte le mur droit/gauche, monte au dessus d'une plateforme
    public bool canMove { get; set; } = true; 
    public bool isDashing { get; private set; } 
    public bool isSliding { get; private set; }//grab vers le bas ou chute contre un mur en appuyant la direction vers le mur
    public bool isJumping { get; private set; }//dans la phase montante apres un saut
    public bool isJumpingAlongWall { get; private set; } //dans la phase montante d'un saut face au mur
    public bool isWallJumping { get; private set; } //dans la phase montante d'un saut depuis un mur
    public bool isFalling { get; private set; } //est en l'air sans saut ni grab ni rien d'autre.
    public bool isSloping { get; private set; } //on est en pente.
    public bool isBumping { get; private set; } //on est en train d'être bump.

    public int wallSide { get; private set; }

    //old
    private bool oldOnWall, oldOnGround = false;

    private bool groundTouch = false;//vaut true la frame ou l'on touche le sol
    private bool hasDashed, hasDoubleJump;

    public bool wallJump { get; private set; }//vaut true la frame ou l'action est faite;
    public bool jump{ get; private set; }//vaut true la frame ou l'action est faite;
    public bool doubleJump { get; private set; }//vaut true la frame ou l'action est faite;
    public bool wallJumpAlongWall { get; private set; }//vaut true la frame ou l'action est faite;
    public bool dash { get; private set; }//vaut true la frame ou l'action est faite;
    private bool oldWallJump, oldJump, oldSecondJump, oldWallJumpAlongWall, oldDash;//use to set var on top just for only one frame

    private bool doJump, doDash;

    [HideInInspector] public int side = 1;

    #endregion

    #region public methods

    private static Vector2[] directions8 = new Vector2[8]
    {
        new Vector2(0, 1),
        new Vector2(0.70710678118f, 0.70710678118f),
        new Vector2(1, 0),
        new Vector2(0.70710678118f, -0.70710678118f),
        new Vector2(0, -1),
        new Vector2(-0.70710678118f, -0.70710678118f),
        new Vector2(-1, 0),
        new Vector2(-0.70710678118f, 0.70710678118f)
    };

    public bool GetDashDirection(bool only8Dir, out Vector2 dir)
    {
        if (isDashing)
        {
            if(only8Dir)
            {
                int indexDir = 0;
                float dot = directions8[0].Dot(lastDashDir);
                float tmpDot;

                for (int i = 1; i < 8; i++)
                {
                    tmpDot = directions8[i].Dot(lastDashDir);
                    if (tmpDot < dot)
                    {
                        dot = tmpDot;
                        indexDir = i;
                    }
                }
                dir = directions8[indexDir];
            }
            else
            {
                dir = lastDashDir;
            }
            return true;
        }
        dir = Vector2.zero;
        return false;
    }

    public Vector2 GetCurrentDirection(bool only8Dir = false)
    {
        if(only8Dir)
        {
            if (playerInput.rawX == 0 && playerInput.rawY == 0)
                return new Vector2(side, 0f);

            float x = playerInput.rawX == 0 ? 0f : playerInput.x.Sign();
            float y = playerInput.rawY == 0 ? 0f : playerInput.y.Sign();
            return new Vector2(x, y).normalized;
        }
        return ((playerInput.rawX != 0 || playerInput.rawY != 0) ? new Vector2(playerInput.x, playerInput.y).normalized : new Vector2(side, 0f));
    }

    public void Teleport(in Vector2 newPosition)
    {
        transform.position = newPosition;
    }

    public void DisableMovement(float duration)
    {
        StartCoroutine(DisableMovementCorout(duration));
    }

    public void Freeze()
    {
        rb.velocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        enableBehaviour = false;
    }

    public void UnFreeze()
    {
        enableBehaviour = enableInput = true;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public void AddForce(in Vector2 dir, float value) => AddForce(dir * value);

    public void AddForce(in Vector2 force)
    {
        rb.velocity += force;
    }

    public void RequestWallJump(bool rightWallJump)
    {
        WallJump(rightWallJump);
    }

    public void RequestDash(in Vector2 dir)
    {
        Dash(dir);
    }

    public void ApplyBump(in Vector2 bumpForce)
    {
        rb.velocity = bumpForce;
        isBumping = true;
        isDashing = false;
        lastTimeBump = Time.time;
    }

    #endregion

    #region Awake and Start

    private void Awake()
    {
        this.transform = base.transform;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<AnimationScript>();
        mainCam = Camera.main;
        playerInput = GetComponent<CustomPlayerInput>();
        fightController = GetComponent<FightController>();
        hitbox = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        oldGroundCollider = null;
        PauseManager.instance.callBackOnPauseDisable += Enable;
        PauseManager.instance.callBackOnPauseEnable += Disable;
        groundLayer = LayerMask.GetMask("Floor", "WallProjectile");
    }

    #endregion

    #region Update

    private void Update()
    {
        // I-Collision/detection
        oldGroundCollider = groundCollider;
        ToricRaycastHit2D groundRay = PhysicsToric.Raycast(new Vector2(transform.position.x, transform.position.y + groundRaycastOffset.y), Vector2.down, groundRaycastLength, groundLayer);
        ToricRaycastHit2D rightSlopeRay = PhysicsToric.Raycast((Vector2)transform.position + groundRaycastOffset, Vector2.down, groundRaycastLength, groundLayer);
        ToricRaycastHit2D leftSlopeRay = PhysicsToric.Raycast((Vector2)transform.position + new Vector2(-groundRaycastOffset.x, groundRaycastOffset.y), Vector2.down, groundRaycastLength, groundLayer);

        groundCollider = null;
        if(groundRay.collider != null)
        {
            groundCollider = groundRay.collider;
            groundColliderData = groundCollider.GetComponent<MapColliderData>();
        }
        else if(rightSlopeRay.collider != null || leftSlopeRay.collider != null)
        {
            if(rightSlopeRay.collider == null || leftSlopeRay.collider == null)
            {
                groundCollider = rightSlopeRay.collider == null ? leftSlopeRay.collider : rightSlopeRay.collider;
                groundColliderData = groundCollider.GetComponent<MapColliderData>();
            }
            else
            {
                print("Debug pls");
                LogManager.instance.WriteLog("Groundray collider is null but the left and rigth ray are not null!", groundRay, rightSlopeRay, leftSlopeRay);
                groundCollider = rightSlopeRay.collider;
                groundColliderData = groundCollider.GetComponent<MapColliderData>();
            }
        }

        isGrounded = groundCollider != null;

        if(groundCollider != null && oldGroundCollider != groundCollider)
        {
            groundColliderData = groundCollider.GetComponent<MapColliderData>();
            lastGroundColliderData = groundColliderData;
        }

        rightWallCollider = PhysicsToric.OverlapCircle((Vector2)transform.position + sideOffset, sideCollisionRadius, groundLayer);
        onRightWall = rightWallCollider != null;
        leftWallCollider = PhysicsToric.OverlapCircle((Vector2)transform.position + new Vector2(-sideOffset.x, sideOffset.y), sideCollisionRadius, groundLayer);
        onLeftWall = leftWallCollider != null;
        onWall = onRightWall || onLeftWall;
        rightWallColliderData = onRightWall ? rightWallCollider.GetComponent<MapColliderData>() : null;
        leftWallColliderData = onLeftWall ? leftWallCollider.GetComponent<MapColliderData>() : null;
        wallSide = onRightWall ? -1 : 1;

        //detect quitting convoyerBelt
        if(groundCollider == null && oldGroundCollider != null)
        {
            MapColliderData mapColliderData = oldGroundCollider.GetComponent<MapColliderData>();
            if (mapColliderData.groundType == MapColliderData.GroundType.convoyerBelt)
            {
                isQuittingConvoyerBelt = true;
                isQuittingConvoyerBeltRight = oldGroundCollider.transform.position.x + oldGroundCollider.offset.x < transform.position.x;
                isQuittingConvoyerBeltLeft = !isQuittingConvoyerBeltRight;
                lastTimeQuittingConvoyerBelt = Time.time;
            }
        }

        if(isQuittingConvoyerBelt && Time.time - lastTimeQuittingConvoyerBelt > inertiaDurationWhenQuittingConvoyerBelt)
        {
            isQuittingConvoyerBelt = isQuittingConvoyerBeltLeft = isQuittingConvoyerBelt = false;
        }

        //Slope detecttion
        if (rightSlopeRay.collider != null)
        {
            groundCollider = groundCollider == null ? rightSlopeRay.collider : groundCollider;
            slopeAngleRight = Useful.WrapAngle((Vector2.Angle(Vector2.right, rightSlopeRay.normal) - 90f) * Mathf.Deg2Rad);
            if (slopeAngleRight >= 1f * Mathf.Deg2Rad && slopeAngleRight <= maxSlopeAngle * Mathf.Deg2Rad)
            {
                isSlopingRight = true;
                isToSteepSlopeRight = false;
            }
            else
            {
                isSlopingRight = false;
                isToSteepSlopeRight = slopeAngleRight > maxSlopeAngle * Mathf.Deg2Rad && slopeAngleRight < Mathf.PI * 0.5f;
            }
        }
        else
        {
            slopeAngleRight = 0f;
            isSlopingRight = false;
        }

        if (leftSlopeRay.collider != null)
        {
            groundCollider = groundCollider == null ? leftSlopeRay.collider : groundCollider;
            slopeAngleLeft = Useful.WrapAngle((270f - Vector2.Angle(Vector2.left, rightSlopeRay.normal)) * Mathf.Deg2Rad);
            if (slopeAngleLeft <= 179f * Mathf.Deg2Rad && slopeAngleLeft >= (180f - maxSlopeAngle) * Mathf.Deg2Rad)
            {
                isSlopingLeft = true;
                isToSteepSlopeLeft = false;
            }
            else
            {
                isSlopingLeft = false;
                isToSteepSlopeLeft = slopeAngleLeft < (180f - maxSlopeAngle) * Mathf.Deg2Rad && slopeAngleLeft > Mathf.PI * 0.5f;
            }
        }
        else
        {
            slopeAngleLeft = 0f;
            isSlopingLeft = false;
        }
        isSloping = isSlopingRight || isSlopingLeft;
        isGrounded = isGrounded || (rightSlopeRay.collider != null || leftSlopeRay.collider != null);

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
            lastTimeQuitGround = Time.time;
        }
        oldOnGround = isGrounded;

        // II-Grab

        //Trigger wallGrab
        if (!wallGrab && onWall && (playerInput.grabPressed && enableInput) && (!isSliding || (playerInput.rawY >= 0 && enableInput)) && !isDashing && !isJumpingAlongWall && !isBumping && canMove)
        {
            if((onRightWall && rightWallColliderData.grabable) || (onLeftWall && leftWallColliderData.grabable))
            {
                if (side != wallSide)
                {
                    anim.Flip(side * -1);
                }
                wallGrab = true;
                isFalling = isJumping = isSliding = false;
            }
        }

        //Trigger reach grab Apex
        if (oldOnWall && !onWall && wallGrab && !reachGrabApex && !isDashing && !isJumping && !isSliding && !isBumping)
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

        //release walljumpingAlongWall in case of cancelled jump
        if(isJumpingAlongWall && isWallJumping)
        {
            isJumpingAlongWall = false;
        }
        //Trigger falling
        if (!isFalling && !isJumping && !isWallJumping && !isJumpingAlongWall && !wallGrab && !isSliding && !isDashing && !isGrounded && !isBumping)
        {
            isFalling = true;
        }
        //release jumping and falling
        if ((isGrounded && rb.velocity.y <= Mathf.Epsilon) || wallGrab || dash || isSliding)
        {
            isJumping = isWallJumping = isJumpingAlongWall = isFalling = false;
        }
        //Release Falling
        if(jump || wallJump || doubleJump)
        {
            isFalling = false;
        }

        //release jumping and trigger falling
        if (isJumping && (rb.velocity.y <= 0f || !playerInput.jumpPressed || (Time.time - lastTimeBeginJump > jumpMaxDuration)))
        {
            isJumping = false;
            //cond  || rb.v.y > 0f pour éviter un bug ou la touche saut est activé une seul frame!, ainsi le saut est tellement cour que isGrounded est tj vrai
            if ((!isGrounded || rb.velocity.y > 0f) && !wallGrab && !isDashing && !isSliding && !isBumping)
            {
                isFalling = true;
            }
        }
        //release Wall jumping and trigger falling
        if (isWallJumping && (rb.velocity.y <= 0f || !playerInput.jumpPressed || (Time.time - lastTimeBeginWallJump > wallJumpMaxDuration)))
        {
            isWallJumping = false;
            //cond  || rb.v.y > 0f pour éviter un bug ou la touche saut est activé une seul frame!, ainsi le saut est tellement cour que isGrounded est tj vrai
            if ((!isGrounded || rb.velocity.y > 0f) && !wallGrab && !isDashing && !isSliding && !isBumping)
            {
                isFalling = true;
            }
        }
        //release Wall jumping allog and trigger falling
        if (isJumpingAlongWall && (isDashing || (Time.time - lastTimeBeginWallJumpAlongWall > jumpAlongWallDuration)))
        {
            isJumpingAlongWall = false;
            if (!isGrounded && !wallGrab && !isDashing && !isSliding && !isBumping)
            {
                isFalling = true;
            }
        }
        //set isPressingJumpButtonDownForFixedUpdate
        isPressingJumpButtonDownForFixedUpdate = isPressingJumpButtonDownForFixedUpdate ? true : playerInput.jumpPressedDown;

        //reset doubleJump
        if (onWall || wallGrab)
        {
            hasDoubleJump = false;
        }

        // IV-Slide

        //Trigger sliding
        //1case, wallGrab => isSliding
        if (!isSliding && wallGrab && playerInput.rawY == -1 && enableInput && !reachGrabApex && !isDashing && !isJumping && !isFalling && !isGrounded && !isBumping)
        {
            isSliding = true;
            wallGrab = grabStayAtApex = grabStayAtApexRight = grabStayAtApexLeft = reachGrabApex = isGrabApexEnable = isGrabApexLeftEnable = isGrabApexRightEnable =  false;
        }
        //2case, isFalling => isSliding
        if (!isSliding && onWall && isFalling && rb.velocity.y < 0f && !isDashing && !isJumping && !wallGrab && !isBumping)
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

        //VI Bumps
        if(isBumping)
        {
            if(Time.time - lastTimeBump > maxBumpDuration)
            {
                DisableBump();
            }

            if (onWall && Time.time - lastTimeBump >= minBumpDuration)
            {
                DisableBump();
            }

            if(isGrounded)
            {
                DisableBump();
            }

            if(Time.time - lastTimeBump >= minBumpDuration && Mathf.Abs(rb.velocity.x) < minBumpSpeedX)
            {
                DisableBump();
            }

            void DisableBump()
            {
                isBumping = false;
                lastTimeBump = -10f;
            }
        }

        // VII-Inputs
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
        if (oldDash)
            dash = false;
        if (oldJump)
            jump = false;
        if (oldWallJump)
            wallJump = false;
        if (oldSecondJump)
            doubleJump = false;
        if (oldWallJumpAlongWall)
            wallJumpAlongWall = false;

        oldDash = dash;
        oldJump = jump;
        oldWallJump = wallJump;
        oldSecondJump = doubleJump;
        oldWallJumpAlongWall = wallJumpAlongWall;

        // VIII-Debug
        DebugText.instance.text += $"vel : {rb.velocity.y.Round(2)}\n";
        //DebugText.instance.text += $"Jump : {isJumping}\n";
        DebugText.instance.text += $"Fall : {isFalling}\n";
        DebugText.instance.text += $"Ground : {isGrounded}\n";
    }

    #endregion

    #region FixedUpdate

    private void FixedUpdate()
    {
        HandleWalk();

        HandleGrab();

        HandleWallSlide();

        HandleJump();

        HandleFall();

        HandleDash();

        HandleBump();

        groundTouch = false;
    }

    #endregion

    #region Handle Walk

    private void HandleWalk()
    {
        if (!enableInput || (!isGrounded && !isSloping) || !canMove || wallGrab || reachGrabApex || grabStayAtApex || isDashing || isJumping || isFalling || isSliding || isBumping)
            return;

        //Avoid clip on platefom
        if (groundCollider != null)
        {
            Vector2 hitboxCenter = (Vector2)transform.position + hitbox.offset;
            ToricRaycastHit2D groundRay = PhysicsToric.Raycast(new Vector2(transform.position.x, transform.position.y + groundRaycastOffset.y), Vector2.down, groundRaycastLength, groundLayer);

            if (groundRay.collider != null)
            {
                MoveOnThePlateform(hitboxCenter, groundRay.point);
            }
            else
            {
                ToricRaycastHit2D rightSlopeRay = PhysicsToric.Raycast((Vector2)transform.position + groundRaycastOffset, Vector2.down, groundRaycastLength, groundLayer);
                ToricRaycastHit2D leftSlopeRay = PhysicsToric.Raycast((Vector2)transform.position + new Vector2(-groundRaycastOffset.x, groundRaycastOffset.y), Vector2.down, groundRaycastLength, groundLayer);
                if (rightSlopeRay.collider != null || leftSlopeRay.collider != null)
                {
                    if (rightSlopeRay.collider == null || leftSlopeRay.collider == null)
                    {
                        MoveOnThePlateform(hitboxCenter, (rightSlopeRay.collider == null ? leftSlopeRay : rightSlopeRay).point);
                    }
                    else
                    {
                        print("Debug pls");
                        LogManager.instance.WriteLog("In fixed update, Groundray collider is null but the left and rigth ray are not null!", groundRay, rightSlopeRay, leftSlopeRay);
                        MoveOnThePlateform(hitboxCenter, rightSlopeRay.point);
                    }
                }
            }

            void MoveOnThePlateform(in Vector2 hitboxCenter, Vector2 raycastPoint)
            {
                Collision2D.Hitbox extendedHitbox = new Collision2D.Hitbox(hitboxCenter, new Vector2(hitbox.size.x, hitbox.size.y + (2f * gapBetweenHitboxAndGround)));
                if ((rb.velocity.y <= 0f && groundColliderData.rb.velocity.y >= 0f) || extendedHitbox.Contains(raycastPoint))
                {
                    if (raycastPoint.y > hitboxCenter.y)
                        raycastPoint -= new Vector2(0f, LevelMapData.currentMap.mapSize.y * LevelMapData.currentMap.cellSize.y);
                    rb.position = new Vector2(rb.position.x, raycastPoint.y + hitbox.size.y * 0.5f - hitbox.offset.y + gapBetweenHitboxAndGround);
                }
            }
        }

        if (isSloping && !onWall)
        {
            HandleSlope();
        }
        else
        {
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
                case MapColliderData.GroundType.convoyerBelt:
                    HandleConvoyerBeltWalk();
                    break;
                default:
                    break;
            }
        }

        #region HandleNormalWalk

        void HandleNormalWalk()
        {
            if(groundTouch)
            {
                localVelocityOnGrippingGround = new Vector2(rb.velocity.x, 0f);
            }

            //Clamp, on est dans le mauvais sens
            if ((playerInput.x >= 0f && localVelocityOnGrippingGround.x <= 0f) || (playerInput.x <= 0f && localVelocityOnGrippingGround.x >= 0f))
            {
                if (playerInput.rawX != 0)
                    localVelocityOnGrippingGround = new Vector2(initSpeed * walkSpeed * playerInput.x.Sign(), localVelocityOnGrippingGround.y);
                else
                {
                    localVelocityOnGrippingGround = new Vector2(0f, localVelocityOnGrippingGround.y);
                }
            }

            if (Mathf.Abs(rb.velocity.x) < initSpeed * walkSpeed * 0.95f && playerInput.rawX != 0)
                localVelocityOnGrippingGround = new Vector2(initSpeed * walkSpeed * playerInput.x.Sign(), localVelocityOnGrippingGround.y);
            else
                localVelocityOnGrippingGround = new Vector2(Mathf.MoveTowards(localVelocityOnGrippingGround.x, playerInput.x * walkSpeed, speedLerp * Time.fixedDeltaTime), localVelocityOnGrippingGround.y);

            //Clamp is to steep slope
            float xMin = isToSteepSlopeLeft ? 0f : float.MinValue;
            float xMax = isToSteepSlopeRight ? 0f : float.MaxValue;
            localVelocityOnGrippingGround = new Vector2(Mathf.Clamp(localVelocityOnGrippingGround.x, xMin, xMax), localVelocityOnGrippingGround.y);

            //friction du to ground horizontal axis
            if (groundColliderData != null && groundColliderData.isGripping)
            {
                rb.velocity = localVelocityOnGrippingGround + new Vector2(groundColliderData.frictionCoefficient * groundColliderData.rb.velocity.x, 0f);
            }
            else
            {
                rb.velocity = localVelocityOnGrippingGround;
            }

            //friction du to ground vertical axis
            if (groundColliderData != null)
            {
                rb.velocity += new Vector2(0f, groundColliderData.rb.velocity.y);
            }
        }

        #endregion

        #region HandleIceWalk

        void HandleIceWalk()
        {
            if (playerInput.rawX != 0f)
            {
                rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x, playerInput.x * walkSpeed,
                    GroundData.instance.iceSpeedLerpFactor * speedLerp * Time.fixedDeltaTime), rb.velocity.y);
            }
            else
            {
                rb.velocity = new Vector2(rb.velocity.x * GroundData.instance.iceFrictionSpeedFactor * (Time.fixedDeltaTime / 0.02f) , rb.velocity.y);
            }
        }

        #endregion

        #region HandleSlope

        void HandleSlope()
        {
            if((isSlopingRight && isSlopingLeft) || (!isSlopingRight && !isSlopingLeft))
            {
                Debug.LogWarning("isSlopingRight == isSlopingLeft == " + isSlopingRight);
                LogManager.instance.WriteLog("Function HandleSlope in Movement script, paradox between sloping right and left", isSloping, isSlopingRight, isSlopingLeft, slopeAngleRight, slopeAngleLeft);
            }

            if(playerInput.rawX != 0)
            {
                float slopeAngle = isSlopingRight ? slopeAngleRight : slopeAngleLeft;
                float negativeSlopeAngle = Useful.WrapAngle(slopeAngle + Mathf.PI);
                float maxSlopeSpeed = slopeSpeed * slopeSpeedCurve.Evaluate(slopeAngle / (maxSlopeAngle * Mathf.Deg2Rad));

                //Clamp, on est dans le mauvais sens
                if ((playerInput.x >= 0f && rb.velocity.x <= 0f) || (playerInput.x <= 0f && rb.velocity.x >= 0f))
                {
                    ApplyMinSpeed(isSlopingRight, playerInput, rb);
                }

                if (rb.velocity.sqrMagnitude < (initSlopeSpeed * maxSlopeSpeed * initSlopeSpeed * maxSlopeSpeed * 0.95f * 0.95f))
                {
                    ApplyMinSpeed(isSlopingRight, playerInput, rb);
                }
                else
                {
                    float angle = GetAngle(isSlopingRight, playerInput);
                    float mag = Mathf.MoveTowards(rb.velocity.magnitude, maxSlopeSpeed, slopeSpeedLerp * maxSlopeSpeed * Time.fixedDeltaTime);
                    rb.velocity = new Vector2(mag * Mathf.Cos(angle), mag * Mathf.Sin(angle));
                }

                if (rb.velocity.sqrMagnitude >= maxSlopeSpeed * maxSlopeSpeed)
                {
                    rb.velocity = rb.velocity.normalized * maxSlopeSpeed;
                }

                void ApplyMinSpeed(bool isSlopingRight, CustomPlayerInput playerInput, Rigidbody2D rb)
                {
                    float mag = initSlopeSpeed * maxSlopeSpeed;
                    float angle = GetAngle(isSlopingRight, playerInput);
                    rb.velocity = new Vector2(mag * Mathf.Cos(angle), mag * Mathf.Sin(angle));
                }

                float GetAngle(bool isSlopingRight, CustomPlayerInput playerInput)
                {
                    float angle;
                    if (isSlopingRight)
                        angle = playerInput.rawX == 1 ? slopeAngle : negativeSlopeAngle;
                    else//isSlopingLeft
                        angle = playerInput.rawX == -1 ? slopeAngle : negativeSlopeAngle;
                    return angle;
                }
            }
            else
            {
                rb.velocity = Vector2.zero;
            }
        }

        #endregion

        #region HandleConvoyerBelt

        void HandleConvoyerBeltWalk()
        {
            ConvoyerBelt convoyer = groundColliderData.GetComponent<ConvoyerBelt>();

            //clamp, mauvais sens
            if(playerInput.rawX != 0 && playerInput.rawX != convoyer.maxSpeed.Sign() && enableInput)
            {
                if (speedLerp > convoyer.speedLerp)
                {
                    if (rb.velocity.x.Sign() != playerInput.rawX)
                    {
                        rb.velocity = new Vector2(0f, rb.velocity.y);
                    }
                }
                else
                {
                    if (rb.velocity.x.Sign() != convoyer.maxSpeed.Sign())
                    {
                        rb.velocity = new Vector2(0f, rb.velocity.y);
                    }
                }
            }

            if(playerInput.rawX == 0 || !enableInput)
            {
                rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x, convoyer.maxSpeed, convoyer.speedLerp * Time.fixedDeltaTime), rb.velocity.y);
            }
            else if (playerInput.rawX == convoyer.maxSpeed.Sign())
            {
                rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x, walkSpeed * playerInput.x + convoyer.maxSpeed, (convoyer.speedLerp + speedLerp) * Time.fixedDeltaTime), rb.velocity.y);
            }
            else //playerInput.rawX != convoyer.maxSpeed.Sign()
            {
                float currentSpeedLerp = speedLerp - convoyer.speedLerp;
                float targetedSpeed = Mathf.Abs(Mathf.Abs(walkSpeed * playerInput.x) - Mathf.Abs(convoyer.maxSpeed));
                float sign = currentSpeedLerp > 0 ? playerInput.rawX : convoyer.maxSpeed.Sign();
                rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x, targetedSpeed * sign, currentSpeedLerp * Time.fixedDeltaTime), rb.velocity.y);
            }
        }

        #endregion

    }

    #endregion

    #region Handle Grab

    private void HandleGrab()
    {
        if (isBumping || !canMove)
            return;

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
                if (rb.velocity.y < grabInitSpeed * grabSpeed * 0.98f)
                {
                    rb.velocity = new Vector2(0f, grabInitSpeed * grabSpeed);
                }
                else
                {
                    float speedModifier = (playerInput.y > maxPreciseGrabValue ? 1f : grabSpeedMultiplierWhenPreciseGrab);
                    rb.velocity = new Vector2(0f, Mathf.MoveTowards(rb.velocity.y, Mathf.Max(grabSpeed * playerInput.y * speedModifier, grabInitSpeed * grabSpeed), grabSpeedLerp * Time.fixedDeltaTime));
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
            if (isGrounded && canMove && !isBumping)
            {
                Jump(Vector2.up);
                doJump = false;
            }
            else if (!isGrounded && Time.time - lastTimeLeavePlateform <= jumpCoyoteTime && !isBumping && canMove)
            {
                Jump(Vector2.up);
                doJump = false;
            }
            else if ((grabStayAtApex || reachGrabApex || wallGrab || onWall || isSliding) && !isGrounded && !isBumping && canMove)
            {
                WallJump();
                doJump = false;
            }
            else if (!isGrounded && !(grabStayAtApex || reachGrabApex || wallGrab) && isPressingJumpButtonDownForFixedUpdate && !hasDoubleJump && enableDoubleJump && !isBumping && canMove)
            {
                HandleDoubleJump();
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

        if(enableJumpAlongWall && isJumpingAlongWall)
        {
            //detect changing direction
            if((playerInput.rawX == -1 && onRightWall) || (playerInput.rawX == 1 && onLeftWall))
            {
                WallJumpOppositeSide(onRightWall);
            }
            else // continue jumping along wall
            {
                float per100 = (Time.time - lastTimeBeginWallJumpAlongWall) / jumpAlongWallDuration;
                rb.velocity = new Vector2(0f, wallJumpAlongSpeed * wallJumpAlongCurveSpeed.Evaluate(per100));
            }
        }

        isPressingJumpButtonDownForFixedUpdate = false;

        void HandleJumpGravity(float GravityMultiplier, float force, in Vector2 speed)
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


            if(Time.time - lastTimeBeginWallJump >= wallJumpMinDuration)
            {
                //Clamp, on est dans le mauvais sens
                if ((playerInput.x >= 0f && rb.velocity.x <= 0f) || (playerInput.x <= 0f && rb.velocity.x >= 0f))
                {
                    rb.velocity = new Vector2(jumpInitHorizontaSpeed * jumpSpeed.x * playerInput.x.Sign(), rb.velocity.y);
                }
            }

            if (Mathf.Abs(rb.velocity.x) < jumpInitHorizontaSpeed * speed.x * 0.95f && playerInput.rawX != 0)
            {
                rb.velocity = new Vector2(jumpInitHorizontaSpeed * speed.x * playerInput.x.Sign(), rb.velocity.y);
            }
            else
            {
                float targetSpeed = (Time.time - lastTimeBeginWallJump >= wallJumpMinDuration) ? playerInput.x * speed.x : speed.x;
                rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x, targetSpeed, jumpSpeedLerp * Time.fixedDeltaTime), rb.velocity.y);
            }
        }

        void HandleDoubleJump()
        {
            float right = playerInput.rawX != 0 ? playerInput.rawX : side;
            Vector2 dir = new Vector2(right, 1f).normalized;
            rb.velocity = dir * doubleJumpSpeed;
            hasDoubleJump = doubleJump = true;
        }
    }

    #region Jump

    private void Jump(in Vector2 dir)
    {
        if (slideParticle != null)
            slideParticle.transform.parent.localScale = new Vector3(ParticleSide(), 1, 1);

        Vector2 newVelocity;
        if(groundColliderData != null)
        {
            if(groundColliderData.groundType == MapColliderData.GroundType.trampoline)
            {
                Jumper jumper = groundColliderData.GetComponent<Jumper>();
                Vector2 newDir = new Vector2(Mathf.Cos(jumper.angleDir * Mathf.Deg2Rad), Mathf.Sin(jumper.angleDir * Mathf.Deg2Rad));
                newVelocity = new Vector2(rb.velocity.x + newDir.x * jumper.force, newDir.y * jumper.force);
            }
            else
            {
                newVelocity = new Vector2(rb.velocity.x + dir.x * jumpInitForce, dir.y * jumpInitForce) + groundColliderData.rb.velocity;
            }
        }
        else if (lastGroundColliderData != null)
        {
            newVelocity = new Vector2(rb.velocity.x + dir.x * jumpInitForce, dir.y * jumpInitForce) + lastGroundColliderData.rb.velocity;
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

        WallJump(right);
    }

    private void WallJump(bool right)
    {
        if(enableJumpAlongWall)
        {
            //first case : jump along the wall
            if ((right && playerInput.rawX >= 0) || (!right && playerInput.rawX <= 0) && wallGrab)
            {
                if (Time.time - lastTimeBeginWallJumpAlongWall > wallJumpAlongCooldown)
                {
                    WallJumpAlongWall(right);
                }
            }
            else if ((right && playerInput.rawX == -1) || (!right && playerInput.rawX == 1)) //2nd case : jump on the oposite of the wall
            {
                WallJumpOppositeSide(right);
            }
            else
            {
                return;
            }
        }
        else
        {
            WallJumpOppositeSide(right);
        }

        StopCoroutine(nameof(DisableMovementCorout));
        StartCoroutine(DisableMovementCorout(0.1f));
    }

    private void WallJumpAlongWall(bool right)
    {
        rb.velocity = Vector2.up * (wallJumpAlongSpeed * wallJumpAlongCurveSpeed.Evaluate(0f));
        isJumpingAlongWall = wallJumpAlongWall = true;
        lastTimeBeginWallJumpAlongWall = Time.time;
    }

    private void WallJumpOppositeSide(bool right)
    {
        float angle = (right ? 1f : -1f) * wallJumpAngle * Mathf.Deg2Rad + Mathf.PI * 0.5f;
        Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.velocity += dir * wallJumpInitForce;
        isWallJumping = wallJump = true;
        lastTimeBeginWallJump = Time.time;

        side *= -1;
    }

    private IEnumerator DisableMovementCorout(float duration)
    {
        disableMovementCounter++;
        canMove = false;

        float timer = 0f;
        while (timer < duration)
        {
            yield return null;
            timer += Time.deltaTime;
            while (PauseManager.instance.isPauseEnable)
            {
                yield return null;
            }
        }

        disableMovementCounter--;
        canMove = disableMovementCounter <= 0;
    }

    #endregion

    #endregion

    #region Handle Fall

    private void HandleFall()
    {
        if (!isFalling || isJumping  || isWallJumping || isJumpingAlongWall || isSliding || wallGrab || isBumping || !enableBehaviour)
            return;

        //phase montante en l'air
        if (rb.velocity.y > 0f)
        {
            //Gravity
            float coeff = playerInput.rawY == -1 && (enableInput && canMove) ? fallGravityMultiplierWhenDownPressed * airGravityMultiplier : airGravityMultiplier;
            rb.velocity += Vector2.up * (Physics2D.gravity.y * coeff * Time.fixedDeltaTime);

            //Movement horizontal
            //Clamp, on est dans le mauvais sens
            if (enableInput && canMove && (playerInput.x >= 0f && rb.velocity.x <= 0f) || (playerInput.x <= 0f && rb.velocity.x >= 0f))
                rb.velocity = new Vector2(airInitHorizontalSpeed * airHorizontalSpeed * playerInput.x.Sign(), rb.velocity.y);
            if (enableInput && canMove && Mathf.Abs(rb.velocity.x) < airInitHorizontalSpeed * airHorizontalSpeed * 0.95f && Mathf.Abs(playerInput.x) > 0.01f)
            {
                rb.velocity = new Vector2(airInitHorizontalSpeed * airHorizontalSpeed * playerInput.x.Sign(), rb.velocity.y);
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
            float targetedSpeed;
            if(playerInput.rawY == -1 && enableInput && canMove)
            {
                targetedSpeed = -fallSpeed.y * Mathf.Max(fallClampSpeedMultiplierWhenDownPressed * Mathf.Abs(playerInput.y), 1f);
            }
            else
            {
                targetedSpeed = -fallSpeed.y;
            }

            if (rb.velocity.y < targetedSpeed)//slow
            {
                rb.velocity = new Vector2(rb.velocity.x, Mathf.MoveTowards(rb.velocity.y, targetedSpeed, fallDecelerationSpeedLerp * Time.fixedDeltaTime));
            }
            else
            {
                float coeff = rb.velocity.y >= -fallSpeed.y * maxBeginFallSpeed ? fallGravityMultiplier * beginFallExtraGravity : fallGravityMultiplier;
                coeff = enableInput && playerInput.rawY < 0 ? coeff * fallGravityMultiplierWhenDownPressed : coeff;
                rb.velocity = new Vector2(rb.velocity.x, Mathf.MoveTowards(rb.velocity.y, targetedSpeed, -Physics2D.gravity.y * coeff * Time.fixedDeltaTime));
            }

            //Horizontal movement
            //Clamp, on est dans le mauvais sens
            if (isQuittingConvoyerBelt)
            {
                float speed = speedWhenQuittingConvoyerBelt * (isQuittingConvoyerBeltRight ? 1f : -1f);
                rb.velocity = new Vector2(speed, rb.velocity.y);
            }
            else if(playerInput.rawX != 0 || Time.time - lastTimeQuitGround > inertiaDurationWhenQuittingGround)//else just keep our velocity
            {
                if (canMove && enableInput && (playerInput.x >= 0f && rb.velocity.x <= 0f) || (playerInput.x <= 0f && rb.velocity.x >= 0f))
                    rb.velocity = new Vector2(fallInitHorizontalSpeed * fallSpeed.x * playerInput.x.Sign(), rb.velocity.y);
                if (canMove && enableInput && Mathf.Abs(rb.velocity.x) < fallInitHorizontalSpeed * fallSpeed.x * 0.95f && playerInput.rawX != 0)
                {
                    rb.velocity = new Vector2(fallInitHorizontalSpeed * fallSpeed.x * playerInput.x.Sign(), rb.velocity.y);
                }
                else
                {
                    float targetSpeed = !enableInput || !canMove ? 0f : playerInput.x * fallSpeed.x;
                    rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x, targetSpeed, fallSpeedLerp * Time.fixedDeltaTime), rb.velocity.y);
                }
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
                Vector2 dir = GetCurrentDirection(true);
                isLastDashUp = dir.SqrDistance(Vector2.up) <= 1e-6f;
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
            //anti knockhead
            if(isLastDashUp)
            {
                Vector2 detectSize = new Vector2(hitbox.bounds.size.x * antiKnockHead, hitbox.bounds.size.y);
                Vector2 nonDetectSize = new Vector2(hitbox.bounds.size.x * (0.5f - antiKnockHead), hitbox.bounds.size.y);
                Vector2 detectOffset = new Vector2(0.5f * (detectSize.x - hitbox.bounds.size.x), detectSize.y * 0.1f);//left
                Vector2 nonDetectOffset = new Vector2(-nonDetectSize.x * 0.5f, nonDetectSize.y * 0.1f);

                Collider2D detectCol = PhysicsToric.OverlapBox((Vector2)transform.position + detectOffset, detectSize, 0f, groundLayer);
                Collider2D nonDetectCol = PhysicsToric.OverlapBox((Vector2)transform.position + nonDetectOffset, nonDetectSize, 0f, groundLayer);
                if (detectCol != null && nonDetectCol == null)
                {
                    MapColliderData colliderData = detectCol.GetComponent<MapColliderData>();
                    if(colliderData == null || !colliderData.disableAntiKnockHead)
                    {
                        Teleport((Vector2)transform.position + Vector2.right * detectSize.x);
                    }
                }

                detectOffset = new Vector2(0.5f * (hitbox.bounds.size.x - detectSize.x), detectSize.y * 0.1f);//right
                nonDetectOffset = new Vector2(nonDetectSize.x * 0.5f, nonDetectSize.y * 0.1f);
                detectCol = PhysicsToric.OverlapBox((Vector2)transform.position + detectOffset, detectSize, 0f, groundLayer);
                nonDetectCol = PhysicsToric.OverlapBox((Vector2)transform.position + nonDetectOffset, nonDetectSize, 0f, groundLayer);
                if (detectCol != null && nonDetectCol == null)
                {
                    MapColliderData colliderData = detectCol.GetComponent<MapColliderData>();
                    if (colliderData == null || !colliderData.disableAntiKnockHead)
                    {
                        Teleport((Vector2)transform.position + Vector2.left * detectSize.x);
                    }
                }
            }

            lastTimeDashFinish = Time.time;
            float per100 = (Time.time - lastTimeDashBegin) / dashDuration;
            rb.velocity = lastDashDir * (dashSpeedCurve.Evaluate(per100) * dashSpeed);
        }
    }

    private void Dash(in Vector2 dir)
    {
        if(enableCameraShaking)
        {
            mainCam.transform.DOComplete();
            mainCam.Shake(cameraShakeSetting);
        }
        //FindObjectOfType<RippleEffect>().Emit(mainCam.WorldToViewportPoint(transform.position));
        //anim.SetTrigger("dash");
        //FindObjectOfType<GhostTrail>().ShowGhost();

        lastDashDir = dir;
        rb.velocity = dir * dashSpeedCurve.Evaluate(0);
        lastTimeDashBegin = Time.time;
        hasDashed = isDashing = dash = true;
        fightController.StartDashing();
    }

    #endregion

    #region Handle Wall Slide

    private void HandleWallSlide()
    {
        if (!isSliding || isBumping)
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

    #region Handle Bump

    private void HandleBump()
    {
        if (!isBumping || !enableBehaviour)
            return;

        //friction
        rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x, 0f, bumpFrictionLerp * Time.fixedDeltaTime), rb.velocity.y);

        //gravity
        if (rb.velocity.y < -maxFallBumpSpeed)//slow
        {
            rb.velocity = new Vector2(rb.velocity.x, Mathf.MoveTowards(rb.velocity.y, -maxFallBumpSpeed, fallDecelerationSpeedLerp * Time.fixedDeltaTime));
        }
        else
        {
            rb.velocity = new Vector2(rb.velocity.x, Mathf.MoveTowards(rb.velocity.y, -fallSpeed.y, -Physics2D.gravity.y * bumpGravityScale * Time.fixedDeltaTime));
        }
    }

    #endregion

    #region Detection

    private void GroundTouch()
    {
        groundColliderData = groundCollider.GetComponent<MapColliderData>();
        lastGroundColliderData = groundColliderData;

        hasDashed = false;
        hasDoubleJump = false;
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

        Vector2 start = new Vector2(transform.position.x, transform.position.y + grabRayOffset);
        ToricRaycastHit2D raycast = PhysicsToric.Raycast(start, Vector2.right, grabRayLength, groundLayer);
        if (raycast.collider != null)
            onRight = true;
        raycast = PhysicsToric.Raycast(start, Vector2.left, grabRayLength, groundLayer);
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

    private IEnumerator PauseCorout()
    {
        Vector2 speed = rb.velocity;
        float angularSpeed = rb.angularVelocity;

        Freeze();

        while(!enableBehaviour)
        {
            Freeze();
            yield return null;
        }

        UnFreeze();

        rb.velocity = speed;
        rb.angularVelocity = angularSpeed;
    }

    private void Disable()
    {
        StartCoroutine(PauseCorout());
    }

    private void Enable()
    {
        enableBehaviour = true;
    }

    private void OnDestroy()
    {
        PauseManager.instance.callBackOnPauseEnable -= Disable;
        PauseManager.instance.callBackOnPauseDisable -= Enable;
    }

    #if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        if(!drawGizmos)
            return;

        hitbox = GetComponent<BoxCollider2D>();
        this.transform = base.transform;

        Gizmos.color = Color.red;

        //Ground and slope
        Gizmos.DrawLine((Vector2)transform.position + groundRaycastOffset, (Vector2)transform.position + groundRaycastOffset + Vector2.down * groundRaycastLength);
        Gizmos.DrawLine((Vector2)transform.position + new Vector2(-groundRaycastOffset.x, groundRaycastOffset.y), (Vector2)transform.position + new Vector2(-groundRaycastOffset.x, groundRaycastOffset.y) + Vector2.down * groundRaycastLength);
        Gizmos.DrawLine(new Vector2(transform.position.x, transform.position.y + groundRaycastOffset.y), new Vector2(transform.position.x, transform.position.y + groundRaycastOffset.y) + Vector2.down * groundRaycastLength);

        //Side detection
        Gizmos.DrawWireSphere((Vector2)transform.position + sideOffset, sideCollisionRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + new Vector2(-sideOffset.x, sideOffset.y), sideCollisionRadius);

        //Grab
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector2(transform.position.x, transform.position.y + grabRayOffset) + (grabRayLength * Vector2.left), new Vector2(transform.position.x, transform.position.y + grabRayOffset) + (grabRayLength * Vector2.right));

        //visual Hitbox
        Gizmos.color = Color.blue;
        Vector2 hitboxCenter = (Vector2)transform.position + hitbox.offset;
        Collision2D.Hitbox extendedHitbox = new Collision2D.Hitbox(hitboxCenter, new Vector2(hitbox.size.x, hitbox.size.y + (2f * gapBetweenHitboxAndGround)));
        Collision2D.Hitbox.GizmosDraw(extendedHitbox);
    }

    private void OnValidate()
    {
        this.transform = base.transform;
        grabSpeed = Mathf.Max(0f, grabSpeed);
        walkSpeed = Mathf.Max(0f, walkSpeed);
        dashSpeed = Mathf.Max(0f, dashSpeed);
        slopeSpeed = Mathf.Max(0f, slopeSpeed);
        fallSpeed = new Vector2(Mathf.Max(0f, fallSpeed.x), Mathf.Max(0f, fallSpeed.y));
        airHorizontalSpeed = Mathf.Max(0f, airHorizontalSpeed);
        jumpSpeed = new Vector2(Mathf.Max(0f, jumpSpeed.x), Mathf.Max(0f, jumpSpeed.y));
        doubleJumpSpeed = Mathf.Max(doubleJumpSpeed, 0f);
        wallJumpMaxDuration = Mathf.Max(wallJumpMaxDuration, 0f);
        wallJumpMinDuration = Mathf.Clamp(wallJumpMinDuration, 0f, wallJumpMaxDuration);
        grabApexSpeed = new Vector2(Mathf.Max(0f, grabApexSpeed.x), Mathf.Max(0f, grabApexSpeed.y));
        grabApexSpeed2 = new Vector2(Mathf.Max(0f, grabApexSpeed2.x), Mathf.Max(0f, grabApexSpeed2.y));
        slideSpeed = Mathf.Max(0f, slideSpeed);
        grabApexSpeedLerp = Mathf.Max(0f, grabApexSpeedLerp);
        grabSpeedLerp = Mathf.Max(0f, grabSpeedLerp);
        speedLerp = Mathf.Max(0f, speedLerp);
        grabRayLength = Mathf.Max(0f, grabRayLength);
        airSpeedLerp = Mathf.Max(0f, airSpeedLerp);
        jumpInitForce = Mathf.Max(0f, jumpInitForce);
        groundRaycastLength = Mathf.Max(0f, groundRaycastLength);
        minBumpSpeedX = Mathf.Max(0f, minBumpSpeedX);
        bumpFrictionLerp = Mathf.Max(0f, bumpFrictionLerp);
        bumpGravityScale = Mathf.Max(0f, bumpGravityScale);
        maxBumpDuration = Mathf.Max(minBumpDuration, maxBumpDuration);
        minBumpDuration = Mathf.Min(minBumpDuration, maxBumpDuration);
        maxPreciseGrabValue = Mathf.Max(maxPreciseGrabValue, 0f);
        inertiaDurationWhenQuittingGround = Mathf.Max(inertiaDurationWhenQuittingGround, 0f);
        gapBetweenHitboxAndGround = Mathf.Max(gapBetweenHitboxAndGround, 0f);
        groundLayer = LayerMask.GetMask("Floor", "WallProjectile");
    }

    #endif

#endregion
}
