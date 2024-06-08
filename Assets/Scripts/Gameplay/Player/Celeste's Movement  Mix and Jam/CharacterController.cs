using System;
using System.Collections;
using ToricCollider2D;
using UnityEngine;
using DG.Tweening;

public class CharacterController : MonoBehaviour
{
    #region Fields

    private AnimationScript anim;
    private Camera mainCam;
    private CharacterInputs playerInput;
    private BoxCollider2D hitbox;
    private ToricRaycastHit2D raycastRight, raycastLeft, groundRay, rightSlopeRay, leftSlopeRay, rightFootRay, leftFootRay, topRightRay, topLeftRay;
    private Collider2D groundCollider, oldGroundCollider, leftWallCollider, rightWallCollider;
    private MapColliderData groundColliderData, lastGroundColliderData, leftWallColliderData, rightWallColliderData, apexJumpColliderData;
    private FightController fightController;
    private ToricObject toricObject;
    private int disableMovementCounter, disableDashCounter;
    private bool isMovementDisable => disableMovementCounter > 0;
    private bool isDashDisable => disableDashCounter > 0;
    
    private new Transform transform;
    private float oldDeltaTime;

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

    [field:SerializeField, ShowOnly] public Vector2 velocity { get; private set; }

#if UNITY_EDITOR

    [SerializeField] private bool drawGizmos = true;

    #endif

    [Header("General")]
    [Tooltip("Le temps maximal entre l'appuie du joueur sur la touche est l'action engendré."), SerializeField] private float timeUntilCommandIsInvalid = 0.2f;

    [Header("Collision")]
    [Tooltip("Length of the raycast to detect wall on sides."), SerializeField] private float sideRayLength = 0.2f;
    [Tooltip("Vertical offset of the raycast to detect wall on sides."), SerializeField] private float sideRayOffset;
    [Tooltip("The offset on the vertical axis of the detection grab ray."), SerializeField] private float grabRayOffset = 1f;
    [Tooltip("Length of the detection for the grab"), SerializeField] private float grabRayLength = 1f;
    [Tooltip("Offset for the side ground raycast, also use to compute slopes")] public Vector2 groundRaycastOffset = Vector2.zero;
    [Tooltip("Length for the side and mid ground raycast, also use to compute slopes raycast"), SerializeField] public float groundRaycastLength = 0.5f;
    [Tooltip("Offset for the top raycast")] public Vector2 topRaycastOffset = Vector2.zero;
    [Tooltip("Length for the top raycast"), SerializeField] public float topRaycastLength = 0.5f;
    [SerializeField] private float gapBetweenHitboxAndGround = 0.1f;
    [SerializeField] private float gapBetweenHitboxAndWall = 0.1f;
    private LayerMask groundLayer;

    [Header("Walk")]
    [Tooltip("La vitesse de marche"), SerializeField] private float walkSpeed = 10f;
    [Tooltip("La vitesse d'interpolation de marche"), SerializeField] private float speedLerp = 50f;
    [Tooltip("La propor de vitesse initiale de marche"), SerializeField, Range(0f, 1f)] private float initSpeed = 0.2f;
    [Tooltip("La durée de conservation de la vitesse de marche apres avoir quitté une plateforme"), SerializeField] private float inertiaDurationWhenQuittingGround = 0.1f;
    private float lastTimeQuitGround = -10f;
    private bool forceHorizontalStick = false, forceDownStick = false, overwriteSpeedOnHorizontalTeleportation = true, overwriteSpeedOnVerticalTeleportation = true;
    private Vector2 teleportationShift;

    [Header("Jumping")]
    [Tooltip("Initial jumps speed")] [SerializeField] private float jumpInitSpeed = 20f;
    [Tooltip("Continuous upward acceleration du to jump.")] [SerializeField] private float jumpForce = 20f;
    [Tooltip("Modifie la gravité lorsqu'on monte en l'air mais sans sauter.")] [SerializeField] private float jumpGravityMultiplier = 1f;
    [Tooltip("La durée maximal ou le joueur peut avoir la touche de saut effective")] [SerializeField] private float jumpMaxDuration = 1f;
    [Tooltip("La vitesse maximal de saut (VMAX en l'air).")] [SerializeField] private Vector2 jumpMaxSpeed = new Vector2(4f, 20f);
    [Tooltip("La vitesse init horizontale en saut (%age de la vitesse max)")] [Range(0f, 1f)] [SerializeField] private float jumpInitHorizontaSpeed = 0.4f;
    [Tooltip("La vitesse d'interpolation de la vitesse horizontale de saut")] [SerializeField] private float jumpSpeedLerp = 20f;
    [SerializeField] private bool enableDoubleJump = true;
    [Tooltip("The speed of the second jump(magnitude of his velocity vector)"), SerializeField] private float doubleJumpSpeed = 6f;
    [Tooltip("Le temps apres avoir quité la plateforme ou le saut est possible")] [SerializeField] private float jumpCoyoteTime = 0.1f;
    private float lastTimeLeavePlateform = -10f, lastTimeJumpCommand = -10f, lastTimeBeginJump = -10f;
    private bool isPressingJumpButtonDownForFixedUpdate;

    [Header("Air")]//In falling state but with velocity.y > 0
    [Tooltip("Gravity multiplier when falling with upward velocity.")] [SerializeField] private float airGravityMultiplier = 1f;
    [Tooltip("Max horizontal speed when falling with upward velocity.")] [SerializeField] private float airHorizontalSpeed = 4f;
    [Tooltip("Init horizontal speed when falling with upward velocity in percentage of \"airHorizontalSpeed\")")] [Range(0f, 1f)] [SerializeField] private float airInitHorizontalSpeed = 0.4f;
    [Tooltip("Interpolation horizontal speed when falling with upward velocity.")] [SerializeField] private float airSpeedLerp = 20f;

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
    [Tooltip("la vitesse de début de saut de mur")] [SerializeField] private float wallJumpInitSpeed = 10f;
    [Tooltip("L'angle en degré par rapport à la vertical de la direction du wall jump")] [Range(0f, 90f)] [SerializeField] private float wallJumpAngle = 45f;
    [Tooltip("L'accélération continue du saut depuis le mur.")] [SerializeField] private float wallJumpForce = 20f;
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
    [Tooltip("La vitesse en sortie du wall grab.")] [SerializeField] private Vector2 apexJumpSpeed = new Vector2(3f, 12f);
    [Tooltip("La deuxième vitesse en sortie du wall grab.")] [SerializeField] private Vector2 apexJumpSpeed2 = new Vector2(4f, 0f);
    [Tooltip("La vitesse d'interpolation en entrée du wall grab.")] [SerializeField] private float grabSpeedLerp = 2f;
    [Tooltip("La vitesse d'interpolation en sortie du wall grab.")] [SerializeField] private float apexJumpSpeedLerp = 400f;
    [Tooltip("La deuxième vitesse d'interpolation en sortie du wall grab.")] [SerializeField] private float apexJumpSpeedLerp2 = 50f;
    [Tooltip("La durée du l'aide à la monté")] [SerializeField] private float apexJumpDuration = 0.125f;
    [Tooltip("La deuxième durée du l'aide à la monté")] [SerializeField] private float apexJumpDuration2 = 0.1f;
    [Tooltip("La vitesse initiale lors d'un saut depuis le sommet de la plateforme")] [Range(0f, 1f)] [SerializeField] private float apexJumpInitSpeed = 0.4f;
    [Tooltip("La vitesse initiale en phase 2 lors d'un saut depuis le sommet de la plateforme")] [Range(0f, 1f)] [SerializeField] private float apexJumpInitSpeed2 = 0.4f;
    private bool grabApexRight = false, grabApexLeft = false;
    private bool isGrabApex => grabApexRight || grabApexLeft;
    private bool reachGrabApex => reachGrabApexRight || reachGrabApexLeft;
    private bool reachGrabApexRight = false, reachGrabApexLeft = false, isApexJump1 = false, isApexJump2 = false, isApexJumpRight = false, isApexJumpLeft = false;
    private float lastTimeApexJump = -10;

    [Header("Dash")]
    [Tooltip("La vitesse maximal du dash")] [SerializeField] private float dashSpeed = 20f;
    [Tooltip("La durée du dash en sec")] [SerializeField] private float dashDuration = 0.4f;
    [Tooltip("Le temps durant lequel un dash est impossible après avoir fini un dash")] [SerializeField] private float dashCooldown = 0.15f;
    [Tooltip("La courbe de vitesse de dash")] [SerializeField] private AnimationCurve dashSpeedCurve;
    [Tooltip("%age de la hitbox qui est ignoré lors d'un dash vers le haut"), SerializeField, Range(0f, 1f)] private float antiKnockHead;
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
    private bool isTraversingOneWayPlateformUp, isTraversingOneWayPlateformDown;
    private bool isTraversingOneWayPlateform => isTraversingOneWayPlateformUp || isTraversingOneWayPlateformDown;

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
    public bool isDashing { get; private set; } 
    public bool isSliding { get; private set; }//grab vers le bas ou chute contre un mur en appuyant la direction vers le mur
    public bool isJumping { get; private set; }//dans la phase montante apres un saut
    public bool isJumpingAlongWall { get; private set; } //dans la phase montante d'un saut face au mur
    public bool isWallJumping { get; private set; } //dans la phase montante d'un saut depuis un mur
    public bool isFalling { get; private set; } //est en l'air sans saut ni grab ni rien d'autre.
    public bool isSloping { get; private set; } //on est en pente.
    public bool isBumping { get; private set; } //on est en train d'être bump.
    public bool isApexJumping => isApexJump1 || isApexJump2;

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

    [HideInInspector] public bool flip { get; private set; } = false;
    public Action<Vector2> onDash;

    #endregion

    #region Public Methods

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
                return new Vector2(flip ? -1 : 1, 0f);

            float x = playerInput.rawX == 0 ? 0f : playerInput.x.Sign();
            float y = playerInput.rawY == 0 ? 0f : playerInput.y.Sign();
            return new Vector2(x, y).normalized;
        }
        return ((playerInput.rawX != 0 || playerInput.rawY != 0) ? new Vector2(playerInput.x, playerInput.y).normalized : new Vector2(flip ? -1 : 1, 0f));
    }

    public void Teleport(in Vector2 newPosition)
    {
        transform.position = newPosition;
    }

    public void DisableInputs(float duration)
    {
        StartCoroutine(DisableMovementCorout(duration));
    }

    private IEnumerator DisableMovementCorout(float duration)
    {
        disableMovementCounter++;
        enableInput = !isMovementDisable;

        yield return PauseManager.instance.Wait(duration);

        disableMovementCounter--;
        enableInput = !isMovementDisable;
    }

    public void DisableDash(float duration)
    {
        StartCoroutine(DisableDashCorout(duration));
    }

    private IEnumerator DisableDashCorout(float duration)
    {
        disableMovementCounter++;
        yield return PauseManager.instance.Wait(duration);
        disableMovementCounter--;
    }

    public void Freeze()
    {
        velocity = Vector2.zero;
        enableBehaviour = false;
    }

    public void UnFreeze()
    {
        enableBehaviour = enableInput = true;
    }

    public void AddForce(in Vector2 dir, float value) => AddForce(dir * value);

    public void AddForce(in Vector2 force)
    {
        velocity += force;
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
        velocity = bumpForce;
        isBumping = true;
        isDashing = isSliding = wallGrab = isJumping = isApexJump1 = isApexJump2  = grabApexRight = grabApexLeft = isFalling = wallJump = false;
        doubleJump = false;
        lastTimeBump = Time.time;
    }

    public void ForceApplyVelocity(in Vector2 velocity)
    {
        this.velocity = velocity;
    }

    #endregion

    #region Awake and Start

    private void Awake()
    {
        this.transform = base.transform;
        anim = GetComponentInChildren<AnimationScript>();
        mainCam = Camera.main;
        playerInput = GetComponent<CharacterInputs>();
        fightController = GetComponent<FightController>();
        hitbox = GetComponent<BoxCollider2D>();
        onDash = (Vector2 dir) => { };
    }

    private void Start()
    {
        toricObject = GetComponent<ToricObject>();
        toricObject.useCustomUpdate = true;
        oldGroundCollider = null;
        PauseManager.instance.callBackOnPauseDisable += Enable;
        PauseManager.instance.callBackOnPauseEnable += Disable;
        oldDeltaTime = Time.deltaTime;
        groundLayer = LayerMask.GetMask("Floor", "WallProjectile");
    }

    #endregion

    #region Update

    #region LateUpdate

    private void LateUpdate()
    {
        if (!enableBehaviour)
            return;

        UpdateState();

        UpdateVelocity();

        Vector2 shift = velocity * Time.deltaTime;
        int tpSign = (int)teleportationShift.x.Sign();
        int velSign = (int)shift.x.Sign();
        if (tpSign != 0 && velSign != 0)
        {
            if(tpSign == velSign)
            {
                shift.x = Mathf.Abs(shift.x) <= Mathf.Abs(teleportationShift.x) ? 0f : Mathf.Abs(shift.x - teleportationShift.x) * velSign;
            }
        }
        tpSign = (int)teleportationShift.y.Sign();
        velSign = (int)shift.y.Sign();
        if (tpSign != 0 && velSign != 0)
        {
            if (tpSign == velSign)
            {
                shift.y = Mathf.Abs(shift.y) <= Mathf.Abs(teleportationShift.y) ? 0f : Mathf.Abs(shift.y - teleportationShift.y) * velSign;
            }
        }

        transform.position += (Vector3)shift;

        teleportationShift = Vector2.zero;

        oldDeltaTime = Time.deltaTime;

        toricObject.CustomUpdate();

        // VIII-Debug
        //DebugText.instance.text += $"OneWayPlateform : {isTraversingOneWayPlateform}\n";
        //DebugText.instance.text += $"OnWall : {onWall}\n";
        //DebugText.instance.text += $"wallGrab : {wallGrab}\n";
        //DebugText.instance.text += $"Apex : {isGrabApex}\n";
        //DebugText.instance.text += $"ApexJump : {isApexJumping}\n";
        //DebugText.instance.text += $"ApexRight : {isApexJumpRight}\n";
        //DebugText.instance.text += $"ApexLeft : {isApexJumpLeft}\n";
        //DebugText.instance.text += $"Jump : {isJumping}\n";
        //DebugText.instance.text += $"Fall : {isFalling}\n";
        //DebugText.instance.text += $"Slide : {isSliding}\n";
        //DebugText.instance.text += $"Gounded : {isGrounded}\n";
        //DebugText.instance.text += $"Bump : {isBumping}\n";
        //DebugText.instance.text += $"vel : {velocity}\n";
        DebugText.instance.text += $"shift : {shift / Time.deltaTime}\n";
    }

    #endregion

    #region UpdateState

    private void UpdateState()
    {
        // I-Collision/Detection
        topRightRay = PhysicsToric.Raycast(new Vector2(transform.position.x + topRaycastOffset.x, transform.position.y + topRaycastOffset.y), Vector2.up, topRaycastLength, groundLayer);
        topLeftRay = PhysicsToric.Raycast(new Vector2(transform.position.x - topRaycastOffset.x, transform.position.y + topRaycastOffset.y), Vector2.up, topRaycastLength, groundLayer);

        //enable traversing one way platefromUp
        if(velocity.y > 0)
        {
            bool isUpColliderIsOneWayPlateform = true;
            bool isoOneWayPlateform = false;
            if (topRightRay)
            {
                MapColliderData topRightColliderData = topRightRay.collider.GetComponent<MapColliderData>();
                if(topRightColliderData != null)
                {
                    if(topRightColliderData.groundType == MapColliderData.GroundType.oneWayPlateform)
                    {
                        isUpColliderIsOneWayPlateform = isoOneWayPlateform = true;
                    }
                    else
                    {
                        isUpColliderIsOneWayPlateform = false;
                    }
                }
                else
                {
                    print("Debug pls");
                    LogManager.instance.AddLog($"The floor collider at pos {(Vector2)topRightRay.collider.transform.position + topRightRay.collider.offset} don't have a MapColliderData component.", topRightRay, topRightRay.collider, "CharacterController::UpdateState");
                }
            }
            if (isUpColliderIsOneWayPlateform && topLeftRay)
            {
                MapColliderData topLeftColliderData = topLeftRay.collider.GetComponent<MapColliderData>();
                if (topLeftColliderData != null)
                {
                    if (topLeftColliderData.groundType == MapColliderData.GroundType.oneWayPlateform)
                    {
                        isUpColliderIsOneWayPlateform = isoOneWayPlateform = true;
                    }
                    else
                    {
                        isUpColliderIsOneWayPlateform = false;
                    }
                }
                else
                {
                    print("Debug pls");
                    LogManager.instance.AddLog($"The floor collider at pos {(Vector2)topLeftRay.collider.transform.position + topLeftRay.collider.offset} don't have a MapColliderData component.", topLeftRay, topLeftRay.collider, "CharacterController::UpdateState");
                }
            }

            if(isUpColliderIsOneWayPlateform && isoOneWayPlateform && !wallGrab && !isGrounded)
            {
                isTraversingOneWayPlateformUp = true;
            }
        }

        // Disable on way plateformUp
        if(isTraversingOneWayPlateformUp)
        {
            bool isOnWaylateformUp = true;
            MapColliderData rightColliderData = topRightRay ? topRightRay.collider.GetComponent<MapColliderData>() : null;
            MapColliderData leftColliderData = topLeftRay ? topLeftRay.collider.GetComponent<MapColliderData>() : null;
            if(rightColliderData != null || leftColliderData != null)
            {
                if(rightColliderData != null)
                {
                    isOnWaylateformUp = rightColliderData.groundType == MapColliderData.GroundType.oneWayPlateform;
                }
                isOnWaylateformUp = isOnWaylateformUp && (leftColliderData == null || leftColliderData.groundType == MapColliderData.GroundType.oneWayPlateform);
            }
            else
            {
                isOnWaylateformUp = false;
            }

            Collider2D col = PhysicsToric.OverlapBox((Vector2)transform.position + hitbox.offset, hitbox.size, 0f, groundLayer);
            if(isOnWaylateformUp)
            {
                isTraversingOneWayPlateformUp = col != null || velocity.y > 0f;
            }
            else
            {
                isTraversingOneWayPlateformUp = col != null;
            }
        }

        oldGroundCollider = groundCollider;
        groundRay = PhysicsToric.Raycast(new Vector2(transform.position.x, transform.position.y + groundRaycastOffset.y), Vector2.down, groundRaycastLength, groundLayer);
        rightSlopeRay = PhysicsToric.Raycast((Vector2)transform.position + groundRaycastOffset, Vector2.down, groundRaycastLength, groundLayer);
        leftSlopeRay = PhysicsToric.Raycast((Vector2)transform.position + new Vector2(-groundRaycastOffset.x, groundRaycastOffset.y), Vector2.down, groundRaycastLength, groundLayer);

        groundCollider = null;
        if(groundRay)
        {
            groundCollider = groundRay.collider;
            groundColliderData = groundCollider.GetComponent<MapColliderData>();
        }
        else if(rightSlopeRay || leftSlopeRay)
        {
            groundCollider = rightSlopeRay ? rightSlopeRay.collider : leftSlopeRay.collider;
            groundColliderData = groundCollider.GetComponent<MapColliderData>();
            if (rightSlopeRay && leftSlopeRay)
            {
                print("Debug pls");
                LogManager.instance.AddLog("Ground ray collider is null but the left and rigth ray are not null!", groundRay, rightSlopeRay, leftSlopeRay, "CharacterController::UpdateState");
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

        //Traversing one way platefrom down
        if(playerInput.oneWayPlateformPressedDown && enableInput && isGrounded && !isBumping && !isDashing && !wallGrab) 
        {
            bool isOnlyOneWayPlateformBelow = (groundRay ? groundRay.collider.GetComponent<MapColliderData>().groundType == MapColliderData.GroundType.oneWayPlateform : true);
            isOnlyOneWayPlateformBelow = isOnlyOneWayPlateformBelow && rightSlopeRay ? rightSlopeRay.collider.GetComponent<MapColliderData>().groundType == MapColliderData.GroundType.oneWayPlateform : true;
            isOnlyOneWayPlateformBelow = isOnlyOneWayPlateformBelow && (leftSlopeRay ? leftSlopeRay.collider.GetComponent<MapColliderData>().groundType == MapColliderData.GroundType.oneWayPlateform : true);
            isOnlyOneWayPlateformBelow = isOnlyOneWayPlateformBelow && (groundRay || rightSlopeRay || leftSlopeRay);

            if(isOnlyOneWayPlateformBelow)
            {
                isTraversingOneWayPlateformDown = true;
            }
        }

        //Disable one way platefrom down
        if(isTraversingOneWayPlateformDown && !isGrounded)
        {
            Collider2D col = PhysicsToric.OverlapBox((Vector2)transform.position + hitbox.offset, hitbox.size, 0f, groundLayer);
            isTraversingOneWayPlateformDown = col != null;
        }

        Vector2 from = (Vector2)transform.position + Vector2.up * sideRayOffset;

        ToricRaycastHit2D PerformNonOneWayPlateformRaycast(in Vector2 from, in Vector2 dir, float length)
        {
            ToricRaycastHit2D[] raycasts = PhysicsToric.RaycastAll(from, dir, length, groundLayer);
            ToricRaycastHit2D? res = null;
            for (int i = 0; i < raycasts.Length; i++)
            {
                if (raycasts[i].collider.GetComponent<MapColliderData>().groundType != MapColliderData.GroundType.oneWayPlateform)
                {
                    res = raycasts[i];
                    break;
                }
            }
            return res.HasValue ? res.Value : default(ToricRaycastHit2D);
        }

        raycastRight = PerformNonOneWayPlateformRaycast(from, Vector2.right, sideRayLength);
        rightWallCollider = raycastRight.collider;
        onRightWall = rightWallCollider != null;
        raycastLeft = PerformNonOneWayPlateformRaycast(from, Vector2.left, sideRayLength);
        leftWallCollider = raycastLeft.collider;
        onLeftWall = leftWallCollider != null;
        onWall = onRightWall || onLeftWall;
        rightWallColliderData = onRightWall ? rightWallCollider.GetComponent<MapColliderData>() : null;
        leftWallColliderData = onLeftWall ? leftWallCollider.GetComponent<MapColliderData>() : null;

        //Detect quitting convoyerBelt
        if(!isBumping && groundCollider == null && oldGroundCollider != null)
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
        if (rightSlopeRay && !isTraversingOneWayPlateform)
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

        if (leftSlopeRay && !isTraversingOneWayPlateform)
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
        isGrounded = isGrounded || rightSlopeRay || leftSlopeRay;

        //Trigger leave plateform
        if (oldOnGround && !isGrounded)
        {
            lastTimeLeavePlateform = Time.time;
        }

        //Trigger groundTouch
        if (isGrounded && !oldOnGround && !isJumping && !isTraversingOneWayPlateformUp)
        {
            GroundTouch();
            groundTouch = true;
        }
        //enable dash
        if(hasDashed && isGrounded && !isTraversingOneWayPlateformUp && !isTraversingOneWayPlateformDown)
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
        from = new Vector2(transform.position.x, transform.position.y + grabRayOffset);
        rightFootRay = PerformNonOneWayPlateformRaycast(from, Vector2.right, grabRayLength);
        leftFootRay = PerformNonOneWayPlateformRaycast(from, Vector2.left, grabRayLength);

        //Trigger wallGrab
        if (enableInput && !wallGrab && onWall && playerInput.grabPressed && (!isSliding || playerInput.rawY >= 0) && !isDashing && !isJumpingAlongWall && !isBumping)
        {
            if((onRightWall && rightWallColliderData.grabableLeft) || (onLeftWall && leftWallColliderData.grabableRight))
            {
                wallGrab = true;
                isFalling = isJumping = isSliding = false;
            }
        }

        //Trigger reach grab Apex
        if (oldOnWall && !onWall && wallGrab && !reachGrabApex && !isDashing && !isJumping && !isSliding && !isBumping && !isApexJumping)
        {
            reachGrabApexRight = rightFootRay.collider != null;
            reachGrabApexLeft = leftFootRay.collider != null;
            grabApexRight = reachGrabApexRight;
            grabApexLeft = reachGrabApexLeft;
        }

        oldOnWall = onWall;

        //Release wallGrab
        if(wallGrab && (dash || jump || wallJump || wallJumpAlongWall || !enableInput))
        {
            wallGrab = reachGrabApexLeft = reachGrabApexRight = grabApexRight = grabApexLeft = false;
        }

        if (isGrabApex)
        {
            if (playerInput.rawY == 1 && ((grabApexRight && playerInput.rawX == 1) || (grabApexLeft && playerInput.rawX == -1)))
            {
                isApexJump1 = true;
                isApexJump2 = false;
                isApexJumpRight = grabApexRight;
                isApexJumpLeft = grabApexLeft;
                grabApexRight = grabApexLeft = wallGrab = false;

                if(isApexJumpRight)
                {
                    apexJumpColliderData = raycastRight ? raycastRight.collider.GetComponent<MapColliderData>() : rightFootRay.collider.GetComponent<MapColliderData>();
                }
                else
                {
                    apexJumpColliderData = raycastLeft ? raycastLeft.collider.GetComponent<MapColliderData>() : leftFootRay.collider.GetComponent<MapColliderData>();
                }

                lastTimeApexJump = Time.time;
            }

            if(onWall || !wallGrab || isJumping || isWallJumping || isDashing || isBumping || !enableInput)
            {
                grabApexRight = grabApexLeft = false;
            }
        }

        if(isApexJumping)
        {
            if(isApexJump1)
            {
                if (Time.time - lastTimeApexJump > apexJumpDuration)
                {
                    isApexJump1 = false;
                    isApexJump2 = true;
                    lastTimeApexJump = Time.time;
                }
            }
            else
            {
                if (Time.time - lastTimeApexJump > apexJumpDuration2)
                {
                    isApexJump1 = isApexJump2 = false;
                    apexJumpColliderData = null;
                }
            }
        }

        if (wallGrab && playerInput.grabPressedUp)
        {
            wallGrab = reachGrabApexLeft = reachGrabApexRight = grabApexRight = grabApexLeft = false;
        }

        if ((!onRightWall && !rightFootRay && !onLeftWall && !leftFootRay) || dash || jump || wallJump || wallJumpAlongWall)
        {
            wallGrab = reachGrabApexLeft = reachGrabApexRight = grabApexRight = grabApexLeft = false;
        }

        // III-Fall and Jump
        //release walljumpingAlongWall in case of cancelled jump
        if(isJumpingAlongWall && isWallJumping)
        {
            isJumpingAlongWall = false;
        }
        //Trigger falling
        if (!isFalling && !isJumping && !isWallJumping && !isJumpingAlongWall && !wallGrab && !isApexJumping && !isSliding && !isDashing && (!isGrounded || isTraversingOneWayPlateform) && !isBumping)
        {
            isFalling = true;
        }
        //release jumping and falling
        if ((isGrounded && !isTraversingOneWayPlateform && (velocity.y - groundColliderData.velocity.y) <= 1e-5f) || wallGrab || dash || isSliding)
        {
            isJumping = isWallJumping = isJumpingAlongWall = isFalling = false;
        }
        //Release Falling
        if(jump || wallJump || doubleJump || isApexJumping)
        {
            isFalling = false;
        }

        //release jumping and trigger falling
        if (isJumping && (velocity.y <= 0f || !playerInput.jumpPressed || (Time.time - lastTimeBeginJump > jumpMaxDuration)))
        {
            isJumping = false;
            //cond  || v.y > 0f pour éviter un bug ou la touche saut est activé une seul frame!, ainsi le saut est tellement cour que isGrounded est tj vrai
            if ((!isGrounded || velocity.y > 0f || isTraversingOneWayPlateform) && !wallGrab && !isApexJumping && !isDashing && !isSliding && !isBumping)
            {
                isFalling = true;
            }
        }
        //release Wall jumping and trigger falling
        if (isWallJumping && (velocity.y <= 0f || !playerInput.jumpPressed || (Time.time - lastTimeBeginWallJump > wallJumpMaxDuration)))
        {
            isWallJumping = false;
            //cond  || v.y > 0f pour éviter un bug ou la touche saut est activé une seul frame!, ainsi le saut est tellement court que isGrounded est tj vrai
            if ((!isGrounded || velocity.y > 0f || isTraversingOneWayPlateformUp || isTraversingOneWayPlateformDown) && !wallGrab && !!isApexJumping && !isDashing && !isSliding && !isBumping)
            {
                isFalling = true;
            }
        }
        //release Wall jumping along wall and trigger falling
        if (isJumpingAlongWall && (isDashing || (Time.time - lastTimeBeginWallJumpAlongWall > jumpAlongWallDuration)))
        {
            isJumpingAlongWall = false;
            if (!isGrounded && !wallGrab && !isDashing && !isApexJumping && !isSliding && !isBumping)
            {
                isFalling = true;
            }
        }
        //set isPressingJumpButtonDownForFixedUpdate
        isPressingJumpButtonDownForFixedUpdate = isPressingJumpButtonDownForFixedUpdate ? true : playerInput.jumpPressedDown;

        //reset doubleJump
        if (onWall || wallGrab && !isTraversingOneWayPlateform)
        {
            hasDoubleJump = false;
        }

        // IV-Slide

        //Trigger sliding
        //1case, wallGrab => isSliding
        if (!isSliding && wallGrab && !isApexJumping && playerInput.rawY == -1 && enableInput && !reachGrabApex && !isDashing && !isJumping && !isFalling && (!isGrounded || isTraversingOneWayPlateform) && !isBumping)
        {
            isSliding = true;
            wallGrab = reachGrabApexRight = reachGrabApexLeft =  false;
        }

        //2case, isFalling => isSliding
        if (!isSliding && onWall && !isApexJumping && isFalling && velocity.y < 0f && !isDashing && !isJumping && !wallGrab && !isBumping)
        {
            if (enableInput && (playerInput.rawX == 1 && onRightWall) || (playerInput.rawX == -1 && onLeftWall))
            {
                isSliding = true;
                isFalling = false;
            }
        }

        //Release sliding
        //stop slide on the wall
        if(isSliding && (!enableInput || (isGrounded && !isTraversingOneWayPlateform) || !onWall || (!playerInput.grabPressed && ((onRightWall && playerInput.rawX != 1) || (onLeftWall && playerInput.rawX != -1))) || isDashing || jump || wallJump))
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

            if(Time.time - lastTimeBump >= minBumpDuration && Mathf.Abs(velocity.x) < minBumpSpeedX)
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
        if(enableInput)
        {
            doJump = doJump ? true : playerInput.jumpPressedDown && !isTraversingOneWayPlateformDown;
            if (playerInput.jumpPressedDown)
            {
                lastTimeJumpCommand = Time.time;
            }

            doDash = doDash ? true : playerInput.dashPressedDown;
            if (playerInput.dashPressedDown)
            {
                lastTimeDashCommand = Time.time;
            }
        }

        //IX compute Side
        if (wallGrab)
        {
            flip = onLeftWall || leftFootRay.collider != null;
        }
        else if(isSliding)
        {
            flip = onLeftWall || leftFootRay.collider != null;
        }
        else if(isFalling)
        {
            if(Mathf.Abs(velocity.x) > fallSpeed.x * fallInitHorizontalSpeed * 0.95f)
            {
                flip = velocity.x > 0f ? false : true;
            }
        }
        else if(isJumping)
        {
            if (Mathf.Abs(velocity.x) > jumpMaxSpeed.x * jumpInitHorizontaSpeed * 0.95f)
            {
                flip = velocity.x > 0f ? false : true;
            }
        }
        else if(isDashing)
        {
            if (Mathf.Abs(velocity.x) >= dashSpeed * 0.5f)
            {
                flip = velocity.x > 0f ? false : true;
            }
        }
        else if (isApexJumping)
        {
            flip = isApexJumpLeft;
        }
        else if(isGrounded)
        {
            if (playerInput.rawX != 0)
            {
                flip = playerInput.rawX == 1 ? false : true;
            }
        }

        //Polish 
        WallParticle(playerInput.y);

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
    }

    #endregion

    #region UpdateVelocity

    private void UpdateVelocity()
    {
        overwriteSpeedOnHorizontalTeleportation = true;
        overwriteSpeedOnVerticalTeleportation = !isJumping;

        HandleWalk();

        HandleGrab();

        HandleSlide();

        HandleJump();

        HandleFall();

        HandleDash();

        HandleBump();

        forceHorizontalStick = isSliding || wallGrab;
        forceDownStick = !isJumping && !wallGrab && !isDashing && !isApexJumping && !isSliding && !isBumping && !wallJump;

        //DebugText.instance.text += $"forceHorizontalStick : {forceHorizontalStick}\n";
        //DebugText.instance.text += $"forceDownStick : {forceDownStick}\n";

        HandleHorizontalCollision();

        HandleVerticalCollision();

        //Avoid warning
        if(groundTouch)
            groundTouch = false;
    }

    #endregion

    #endregion

    #region Collision

    private void HandleHorizontalCollision()
    {
        Vector2 TransformHitPoint(in Vector2 point, bool leftPoint)
        {
            if (leftPoint)
            {
                if (point.x > transform.position.x + groundRaycastOffset.x)
                {
                    return new Vector2(point.x - LevelMapData.currentMap.mapSize.x * LevelMapData.currentMap.cellSize.x, point.y);
                }
            }
            else
            {
                if (point.x < transform.position.x + topRaycastOffset.x)
                {
                    return new Vector2(point.x + LevelMapData.currentMap.mapSize.x * LevelMapData.currentMap.cellSize.x, point.y);
                }
            }
            return point;
        }

        if (onWall || rightFootRay || raycastRight || leftFootRay || raycastLeft)
        {
            int cpRight = (raycastRight ? 1 : 0) + (rightFootRay ? 1 : 0);
            int cpLeft = (raycastLeft ? 1 : 0) + (leftFootRay ? 1 : 0);
            bool right = cpRight > 0 && cpRight > cpLeft;
            bool left = cpLeft > 0 && cpLeft > cpRight;

            Vector2 hitboxCenter = (Vector2)transform.position + hitbox.offset;
            if (right)
            {
                ToricRaycastHit2D hit = onRightWall ? ref raycastRight : ref rightFootRay;
                Vector2 wallSpeed = hit.collider.GetComponent<MapColliderData>().velocity;
                float deltaX = (velocity.x - wallSpeed.x) * Time.deltaTime;  
                ToricHitbox extendedHitbox = new ToricHitbox(new Vector2(hitboxCenter.x + deltaX, hitboxCenter.y), new Vector2(hitbox.size.x + (2f * gapBetweenHitboxAndWall), hitbox.size.y));
                if (forceHorizontalStick || extendedHitbox.Contains(hit.point))
                {
                    Vector2 hitPoint = TransformHitPoint(hit.point, false);
                    Vector2 target = new Vector2(hitPoint.x - gapBetweenHitboxAndWall - hitbox.size.x * 0.5f + hitbox.offset.x, transform.position.y);
                    teleportationShift += target - (Vector2)transform.position;
                    transform.position = target;
                    if(overwriteSpeedOnHorizontalTeleportation)
                        velocity = new Vector2(wallSpeed.x, velocity.y);
                }
            }
            else if (left)
            {
                ToricRaycastHit2D hit = onLeftWall ? ref raycastLeft : ref leftFootRay;
                Vector2 wallSpeed = hit.collider.GetComponent<MapColliderData>().velocity;
                float deltaX = (velocity.x - wallSpeed.x) * Time.deltaTime;
                ToricHitbox extendedHitbox = new ToricHitbox(new Vector2(hitboxCenter.x + deltaX, hitboxCenter.y), new Vector2(hitbox.size.x + (2f * gapBetweenHitboxAndWall), hitbox.size.y));
                if (forceHorizontalStick || extendedHitbox.Contains(hit.point))
                {
                    Vector2 hitPoint = TransformHitPoint(hit.point, true);
                    Vector2 target = new Vector2(hitPoint.x + gapBetweenHitboxAndWall + hitbox.size.x * 0.5f - hitbox.offset.x, transform.position.y);
                    teleportationShift += target - (Vector2)transform.position;
                    transform.position = target;
                    if (overwriteSpeedOnHorizontalTeleportation)
                        velocity = new Vector2(wallSpeed.x, velocity.y);
                }
            }
        }
    }

    private void HandleVerticalCollision()
    {
        Vector2 TransformHitPoint(in Vector2 point, bool downPoint)
        {
            if (downPoint)
            {
                if (point.y > transform.position.y + groundRaycastOffset.y)
                {
                    return new Vector2(point.x, point.y - LevelMapData.currentMap.mapSize.y * LevelMapData.currentMap.cellSize.y);
                }
            }
            else
            {
                if (point.y < transform.position.y + topRaycastOffset.y)
                {
                    return new Vector2(point.x, point.y + LevelMapData.currentMap.mapSize.y * LevelMapData.currentMap.cellSize.y);
                }
            }
            return point;
        }

        if ((rightSlopeRay || leftSlopeRay || topRightRay || topLeftRay) && !isTraversingOneWayPlateformUp && !isTraversingOneWayPlateformDown)
        {
            int cpUp = (topRightRay ? 1 : 0) + (topLeftRay ? 1 : 0);
            int cpDown = (rightSlopeRay ? 1 : 0) + (leftSlopeRay ? 1 : 0);
            bool up = cpUp > 0 && cpUp > cpDown;
            bool down = cpDown > 0 && cpDown > cpUp;

            Vector2 hitboxCenter = (Vector2)transform.position + hitbox.offset;
            //Down
            if (down)
            {
                ToricRaycastHit2D hit = rightSlopeRay.collider != null ? ref rightSlopeRay : ref leftSlopeRay;
                Vector2 groundVelocity = groundColliderData.velocity;
                float deltaY = (velocity.y - groundVelocity.y) * Time.deltaTime;
                ToricHitbox extendedHitbox = new ToricHitbox(new Vector2(hitboxCenter.x, hitboxCenter.y + deltaY), new Vector2(hitbox.size.x, hitbox.size.y + (2f * gapBetweenHitboxAndGround)));
                if (forceDownStick || extendedHitbox.Contains(hit.point))
                {
                    Vector2 hitPoint = TransformHitPoint(hit.point, true);
                    Vector2 target = new Vector2(transform.position.x, hitPoint.y + gapBetweenHitboxAndGround + hitbox.size.y * 0.5f - hitbox.offset.y);
                    teleportationShift += target - (Vector2)transform.position;
                    transform.position = target;
                    if(overwriteSpeedOnVerticalTeleportation)
                        velocity = new Vector2(velocity.x, groundVelocity.y);
                }
            }
            else if (up)
            {
                ToricRaycastHit2D hit = topRightRay.collider != null ? ref topRightRay : ref topLeftRay;
                Vector2 groundVelocity = hit.collider.GetComponent<MapColliderData>().velocity;
                float deltaY = (velocity.y - groundVelocity.y) * Time.deltaTime;
                ToricHitbox extendedHitbox = new ToricHitbox(new Vector2(hitboxCenter.x, hitboxCenter.y + deltaY), new Vector2(hitbox.size.x, hitbox.size.y + (2f * gapBetweenHitboxAndGround)));
                if (extendedHitbox.Contains(hit.point))
                {
                    Vector2 hitPoint = TransformHitPoint(hit.point, false);
                    Vector2 target = new Vector2(transform.position.x, hitPoint.y - gapBetweenHitboxAndGround - hitbox.size.y * 0.5f - hitbox.offset.y);
                    teleportationShift += target - (Vector2)transform.position;
                    transform.position = target;
                    if (overwriteSpeedOnVerticalTeleportation)
                        velocity = new Vector2(velocity.x, groundVelocity.y);
                }
            }
        }
    }

    #endregion

    #region Handle Walk

    private void HandleWalk()
    {
        if (!isGrounded || isFalling || isJumping || isDashing || isSliding || wallGrab || isApexJumping || isBumping || isTraversingOneWayPlateformUp)
            return;

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
                case MapColliderData.GroundType.jumper:
                    HandleNormalWalk();
                    break;
                case MapColliderData.GroundType.convoyerBelt:
                    HandleConvoyerBeltWalk();
                    break;
                case MapColliderData.GroundType.oneWayPlateform:
                    HandleNormalWalk();
                    break;
                default:
                    HandleNormalWalk();
                    break;
            }
        }

        #region HandleNormalWalk

        void HandleNormalWalk()
        {
            Vector2 groundVel = groundColliderData.velocity;
            Vector2 localVel = velocity - groundVel;

            //Avoid stick on side
            if(enableInput && raycastLeft && playerInput.rawX == 1)
            {
                MapColliderData sideColliderData = raycastLeft.collider.GetComponent<MapColliderData>();
                if((int)sideColliderData.velocity.x.Sign() == 1 && sideColliderData.velocity.x < walkSpeed)
                {
                    localVel.x = Mathf.Min(sideColliderData.velocity.x + initSpeed * walkSpeed, walkSpeed);
                    overwriteSpeedOnHorizontalTeleportation = false;
                }
            }
            if (enableInput && raycastRight && playerInput.rawX == -1)
            {
                MapColliderData sideColliderData = raycastRight.collider.GetComponent<MapColliderData>();
                if ((int)sideColliderData.velocity.x.Sign() == -1 && -sideColliderData.velocity.x < walkSpeed)
                {
                    localVel.x = Mathf.Max(sideColliderData.velocity.x - initSpeed * walkSpeed, -walkSpeed);
                    overwriteSpeedOnHorizontalTeleportation = false;
                }
            }

            //Clamp, on est dans le mauvais sens
            if (enableInput && ((playerInput.x >= 0f && localVel.x <= 0f) || (playerInput.x <= 0f && localVel.x >= 0f)))
            {
                if (playerInput.rawX != 0)
                {
                    if(!onWall || (playerInput.rawX == 1 && !onRightWall) || (playerInput.rawX == -1 && !onLeftWall))
                        localVel = new Vector2(initSpeed * walkSpeed * playerInput.x.Sign(), localVel.y);
                }
                else
                {
                    localVel = new Vector2(0f, localVel.y);
                }
            }

            if (Mathf.Abs(localVel.x) < initSpeed * walkSpeed * 0.95f && playerInput.rawX != 0 && enableInput)
            {
                if (!onWall || (playerInput.rawX == 1 && !onRightWall) || (playerInput.rawX == -1 && !onLeftWall))
                {
                    localVel = new Vector2(initSpeed * walkSpeed * playerInput.x.Sign(), localVel.y);
                }
            }
            else
            {
                localVel = new Vector2(Mathf.MoveTowards(localVel.x, (enableInput ? playerInput.x : 0f) * walkSpeed, speedLerp * Time.deltaTime), localVel.y);
            }

            //Clamp if too steep slope
            float xMin = isToSteepSlopeLeft ? 0f : float.MinValue;
            float xMax = isToSteepSlopeRight ? 0f : float.MaxValue;
            localVel = new Vector2(Mathf.Clamp(localVel.x, xMin, xMax), localVel.y);

            //friction du to ground horizontal axis
            if (groundColliderData != null && groundColliderData.isGripping)
            {
                velocity = new Vector2(localVel.x + groundColliderData.frictionCoefficient * groundColliderData.velocity.x, localVel.y);
            }
            else
            {
                velocity = localVel;
            }

            //friction du to ground vertical axis
            if (groundColliderData != null)
            {
                velocity = new Vector2(velocity.x, velocity.y + groundColliderData.velocity.y);
            }
        }

        #endregion

        #region HandleIceWalk

        void HandleIceWalk()
        {
            Vector2 groundVel = groundColliderData.velocity;
            Vector2 localVel = velocity - groundVel;

            if(playerInput.rawX != 0f & enableInput)
            {
                localVel = new Vector2(Mathf.MoveTowards(localVel.x, playerInput.x * walkSpeed, GroundData.instance.iceSpeedLerpFactor * speedLerp * Time.deltaTime), localVel.y);
            }
            else
            {
                localVel = new Vector2(localVel.x * GroundData.instance.iceFrictionSpeedFactor * (Time.deltaTime / oldDeltaTime), velocity.y);
            }

            //friction du to ground horizontal axis
            if (groundColliderData != null && groundColliderData.isGripping)
            {
                velocity = new Vector2(localVel.x + groundColliderData.frictionCoefficient * groundColliderData.velocity.x, localVel.y);
            }
            else
            {
                velocity = localVel;
            }

            //friction du to ground vertical axis
            if (groundColliderData != null)
            {
                velocity = new Vector2(velocity.x, velocity.y + groundColliderData.velocity.y);
            }
        }

        #endregion

        #region HandleSlope

        void HandleSlope()
        {
            if((isSlopingRight && isSlopingLeft) || (!isSlopingRight && !isSlopingLeft))
            {
                Debug.LogWarning("isSlopingRight == isSlopingLeft == " + isSlopingRight);
                LogManager.instance.AddLog("Function HandleSlope in Movement script, paradox between sloping right and left", isSloping, isSlopingRight, isSlopingLeft, slopeAngleRight, slopeAngleLeft, "CharacterController.HandleSlope");
            }

            if(playerInput.rawX != 0)
            {
                if (!onWall || (playerInput.rawX == 1 && !onRightWall) || (playerInput.rawX == -1 && !onLeftWall))
                {
                    float slopeAngle = isSlopingRight ? slopeAngleRight : slopeAngleLeft;
                    float negativeSlopeAngle = Useful.WrapAngle(slopeAngle + Mathf.PI);
                    float maxSlopeSpeed = slopeSpeed * slopeSpeedCurve.Evaluate(slopeAngle / (maxSlopeAngle * Mathf.Deg2Rad));

                    //Clamp, on est dans le mauvais sens
                    if ((playerInput.x >= 0f && velocity.x <= 0f) || (playerInput.x <= 0f && velocity.x >= 0f))
                    {
                        ApplyMinSpeed(isSlopingRight, playerInput);
                    }

                    if (velocity.sqrMagnitude < (initSlopeSpeed * maxSlopeSpeed * initSlopeSpeed * maxSlopeSpeed * 0.95f * 0.95f))
                    {
                        ApplyMinSpeed(isSlopingRight, playerInput);
                    }
                    else
                    {
                        float angle = GetAngle(isSlopingRight, playerInput);
                        float mag = Mathf.MoveTowards(velocity.magnitude, maxSlopeSpeed, slopeSpeedLerp * maxSlopeSpeed * Time.deltaTime);
                        velocity = new Vector2(mag * Mathf.Cos(angle), mag * Mathf.Sin(angle));
                    }

                    if (velocity.sqrMagnitude >= maxSlopeSpeed * maxSlopeSpeed)
                    {
                        velocity = velocity.normalized * maxSlopeSpeed;
                    }

                    void ApplyMinSpeed(bool isSlopingRight, CharacterInputs playerInput)
                    {
                        float mag = initSlopeSpeed * maxSlopeSpeed;
                        float angle = GetAngle(isSlopingRight, playerInput);
                        velocity = new Vector2(mag * Mathf.Cos(angle), mag * Mathf.Sin(angle));
                    }

                    float GetAngle(bool isSlopingRight, CharacterInputs playerInput)
                    {
                        float angle;
                        if (isSlopingRight)
                            angle = playerInput.rawX == 1 ? slopeAngle : negativeSlopeAngle;
                        else//isSlopingLeft
                            angle = playerInput.rawX == -1 ? slopeAngle : negativeSlopeAngle;
                        return angle;
                    }
                }
            }
            else
            {
                velocity = Vector2.zero;
            }
        }

        #endregion

        #region HandleConvoyerBelt

        void HandleConvoyerBeltWalk()
        {
            ConvoyerBelt convoyer = groundColliderData.GetComponent<ConvoyerBelt>();

            if(!convoyer.isActive)
            {
                HandleNormalWalk();
                return;
            }

            //clamp, mauvais sens
            if(playerInput.rawX != 0 && playerInput.rawX != convoyer.maxSpeed.Sign() && enableInput)
            {
                if (speedLerp > convoyer.speedLerp)
                {
                    if (velocity.x.Sign() != playerInput.rawX)
                    {
                        velocity = new Vector2(0f, velocity.y);
                    }
                }
                else
                {
                    if (velocity.x.Sign() != convoyer.maxSpeed.Sign())
                    {
                        velocity = new Vector2(0f, velocity.y);
                    }
                }
            }

            if(playerInput.rawX == 0 || !enableInput)
            {
                if (!onWall || (convoyer.maxSpeed >= 0f && !onRightWall) || (convoyer.maxSpeed <= 0f && !onLeftWall))
                {
                    velocity = new Vector2(Mathf.MoveTowards(velocity.x, convoyer.maxSpeed, convoyer.speedLerp * Time.deltaTime), velocity.y);
                }
            }
            else if (playerInput.rawX == convoyer.maxSpeed.Sign())
            {
                if (!onWall || (playerInput.rawX == 1 && !onRightWall) || (playerInput.rawX == -1 && !onLeftWall))
                {
                    velocity = new Vector2(Mathf.MoveTowards(velocity.x, walkSpeed * playerInput.x + convoyer.maxSpeed, (convoyer.speedLerp + speedLerp) * Time.deltaTime), velocity.y);
                }
            }
            else //playerInput.rawX != convoyer.maxSpeed.Sign()
            {
                float currentSpeedLerp = speedLerp - convoyer.speedLerp;
                float targetedSpeed = Mathf.Abs(Mathf.Abs(walkSpeed * playerInput.x) - Mathf.Abs(convoyer.maxSpeed));
                float sign = currentSpeedLerp > 0 ? playerInput.rawX : convoyer.maxSpeed.Sign();

                if (!onWall || (sign == 1 && !onRightWall) || (sign == -1 && !onLeftWall))
                {
                    velocity = new Vector2(Mathf.MoveTowards(velocity.x, targetedSpeed * sign, currentSpeedLerp * Time.deltaTime), velocity.y);
                }
            }
        }

        #endregion

    }

    #endregion

    #region Handle Grab

    private void HandleGrab()
    {
        if ((!wallGrab && !isApexJumping && !isGrabApex) || isDashing || isBumping)
            return;

        Vector2 wallSpeed = Vector2.zero;
        if(onRightWall)
        {
            wallSpeed = raycastRight.collider.GetComponent<MapColliderData>().velocity;
        }
        else if(onLeftWall)
        {
            wallSpeed = raycastLeft.collider.GetComponent<MapColliderData>().velocity;
        }
        else if (rightFootRay.collider != null)
        {
            wallSpeed = rightFootRay.collider.GetComponent<MapColliderData>().velocity;
        }
        else if(leftFootRay.collider != null)
        {
            wallSpeed = leftFootRay.collider.GetComponent<MapColliderData>().velocity;
        }

        if (reachGrabApex)
        {
            velocity = new Vector2(wallSpeed.x, 0f);
        }

        //Normal case
        if (wallGrab && !isGrabApex)
        {
            //On veut monter
            if(playerInput.rawY == 1)
            {
                //clamp, on va dans le mauvais sens
                if (velocity.y < grabInitSpeed * grabSpeed * 0.95f)
                {
                    velocity = new Vector2(wallSpeed.x, grabInitSpeed * grabSpeed);
                }
                else
                {
                    float speedModifier = (playerInput.y > maxPreciseGrabValue ? 1f : grabSpeedMultiplierWhenPreciseGrab);
                    velocity = new Vector2(wallSpeed.x, Mathf.MoveTowards(velocity.y, Mathf.Max(grabSpeed * playerInput.y * speedModifier, grabInitSpeed * grabSpeed), grabSpeedLerp * Time.deltaTime));
                }
            }
            else
            {
                velocity = new Vector2(wallSpeed.x, 0f);
            }
        }
        //Grab Apex
        else if (wallGrab && isGrabApex)
        {
            velocity = new Vector2(wallSpeed.x, 0f);
        }

        if(isApexJumping)
        {
            Vector2 localVel = apexJumpColliderData != null ? velocity - apexJumpColliderData.velocity : velocity;

            if(isApexJump1)
            {
                if(Mathf.Abs(localVel.x) <= apexJumpSpeed.x * 0.95f * apexJumpInitSpeed)
                {
                    localVel = new Vector2((isApexJumpRight ? 1f : -1f) * apexJumpSpeed.x * apexJumpInitSpeed, localVel.y);
                }
                if (Mathf.Abs(localVel.y) <= apexJumpSpeed.y * 0.95f * apexJumpInitSpeed)
                {
                    localVel = new Vector2(localVel.x, apexJumpSpeed.y * apexJumpInitSpeed);
                }

                Vector2 targetSpped = new Vector2(isApexJumpRight ? apexJumpSpeed.x : -apexJumpSpeed.x, apexJumpSpeed.y);
                localVel = Vector2.MoveTowards(localVel, targetSpped, apexJumpSpeedLerp * Time.deltaTime);
            }
            else
            //isApexJump2
            {
                if (Mathf.Abs(localVel.x) <= apexJumpSpeed2.x * 0.95f * apexJumpInitSpeed2)
                {
                    localVel = new Vector2(localVel.x.Sign() * apexJumpSpeed2.x * apexJumpInitSpeed2, localVel.y);
                }
                if (Mathf.Abs(localVel.y) <= apexJumpSpeed2.y * 0.95f * apexJumpInitSpeed2)
                {
                    localVel = new Vector2(localVel.x, localVel.y.Sign() * apexJumpSpeed2.y * apexJumpInitSpeed2);
                }

                Vector2 targetSpped = new Vector2(isApexJumpRight ? apexJumpSpeed2.x : -apexJumpSpeed2.x, apexJumpSpeed2.y);
                localVel = Vector2.MoveTowards(localVel, targetSpped, apexJumpSpeedLerp2 * Time.deltaTime);
            }

            velocity = apexJumpColliderData != null ? localVel + apexJumpColliderData.velocity : localVel;
        }

        if(reachGrabApex)
        {
            reachGrabApexRight = reachGrabApexLeft = false;
        }
    }

    #endregion

    #region Handle Jump

    private void HandleJump()
    {
        if (doJump)
        {
            if (isGrounded && !isBumping)
            {
                Jump(Vector2.up);
                doJump = false;
            }
            else if (!isGrounded && Time.time - lastTimeLeavePlateform <= jumpCoyoteTime && !isBumping)
            {
                Jump(Vector2.up);
                doJump = false;
            }
            else if ((wallGrab || onWall || isSliding) && !isGrounded && !isApexJumping && !isBumping)
            {
                WallJump();
                doJump = false;
            }
            else if (!isGrounded && !wallGrab && isPressingJumpButtonDownForFixedUpdate && !hasDoubleJump && !isApexJumping && enableDoubleJump && !isBumping)
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
                speed = jumpMaxSpeed;
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
                velocity = new Vector2(0f, wallJumpAlongSpeed * wallJumpAlongCurveSpeed.Evaluate(per100));
            }
        }

        isPressingJumpButtonDownForFixedUpdate = false;

        void HandleJumpGravity(float GravityMultiplier, float force, in Vector2 speed)
        {
            velocity += Vector2.up * (Physics2D.gravity.y * GravityMultiplier * Time.deltaTime);

            //phase montante du saut
            if (playerInput.jumpPressed && enableInput)
            {
                velocity += Vector2.up * (force * Time.deltaTime);
            }

            //clamp en y
            if (velocity.y > speed.y)
            {
                velocity = new Vector2(velocity.x, speed.y);
            }
        }

        void HandleHorizontalMovement(in Vector2 speed)
        {
            if (Time.time - lastTimeBeginWallJump >= wallJumpMinDuration && enableInput)
            {
                //Clamp, on est dans le mauvais sens
                if ((playerInput.x >= 0f && velocity.x <= 0f) || (playerInput.x <= 0f && velocity.x >= 0f))
                {
                    velocity = new Vector2(jumpInitHorizontaSpeed * jumpMaxSpeed.x * playerInput.x.Sign(), velocity.y);
                }
            }

            if (Mathf.Abs(velocity.x) < jumpInitHorizontaSpeed * speed.x * 0.95f && playerInput.rawX != 0)
            {
                velocity = new Vector2(jumpInitHorizontaSpeed * speed.x * playerInput.x.Sign(), velocity.y);
            }
            else
            {
                float targetSpeed = enableInput ? ((Time.time - lastTimeBeginWallJump >= wallJumpMinDuration) ? playerInput.x * speed.x : speed.x) : 0f;
                velocity = new Vector2(Mathf.MoveTowards(velocity.x, targetSpeed, jumpSpeedLerp * Time.deltaTime), velocity.y);
            }

            //clamp en x
            if (Mathf.Abs(velocity.x) > speed.x)
            {
                velocity = new Vector2(speed.x * velocity.x.Sign(), velocity.y);
            }
        }

        void HandleDoubleJump()
        {
            float right = playerInput.rawX != 0 ? playerInput.rawX : (flip ? -1 : 1);
            Vector2 dir = new Vector2(right, 1f).normalized;
            velocity = dir * doubleJumpSpeed;
            hasDoubleJump = doubleJump = true;
        }
    }

    #region Jump

    private void Jump(in Vector2 dir)
    {
        if (slideParticle != null)
            slideParticle.transform.parent.localScale = new Vector3(flip ? -1 : 1, 1, 1);

        Vector2 newVelocity;
        if(groundColliderData != null)
        {
            if(groundColliderData.groundType == MapColliderData.GroundType.jumper)
            {
                Jumper jumper = groundColliderData.GetComponent<Jumper>();
                Vector2 newDir = new Vector2(Mathf.Cos(jumper.angleDir * Mathf.Deg2Rad), Mathf.Sin(jumper.angleDir * Mathf.Deg2Rad));
                newVelocity = new Vector2(velocity.x + newDir.x * jumper.force, newDir.y * jumper.force);
            }
            else
            {
                newVelocity = new Vector2(velocity.x + dir.x * jumpInitSpeed, dir.y * jumpInitSpeed) + groundColliderData.velocity;
            }
        }
        else if (lastGroundColliderData != null)
        {
            newVelocity = new Vector2(velocity.x + dir.x * jumpInitSpeed, dir.y * jumpInitSpeed) + lastGroundColliderData.velocity;
        }
        else
        {
            newVelocity = new Vector2(velocity.x + dir.x * jumpInitSpeed, dir.y * jumpInitSpeed);
        }

        velocity = newVelocity;
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
        else //ApexJump ou on est accrocher tout en haut d'un mur
        {
            right = rightFootRay.collider != null;
            bool left = leftFootRay.collider != null;

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
        velocity = Vector2.up * (wallJumpAlongSpeed * wallJumpAlongCurveSpeed.Evaluate(0f));
        isJumpingAlongWall = wallJumpAlongWall = true;
        lastTimeBeginWallJumpAlongWall = Time.time;
    }

    private void WallJumpOppositeSide(bool right)
    {
        float angle = (right ? 1f : -1f) * wallJumpAngle * Mathf.Deg2Rad + Mathf.PI * 0.5f;
        Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        velocity = dir * wallJumpInitSpeed;
        isWallJumping = wallJump = true;
        lastTimeBeginWallJump = Time.time;
    }

    #endregion

    #endregion

    #region Handle Fall

    private void HandleFall()
    {
        if (!isFalling)
            return;

        //phase montante en l'air
        if (velocity.y > 0f)
        {
            //Gravity
            float coeff = playerInput.rawY == -1 && enableInput ? fallGravityMultiplierWhenDownPressed * airGravityMultiplier : airGravityMultiplier;
            velocity += Vector2.up * (Physics2D.gravity.y * coeff * Time.deltaTime);

            //Movement horizontal
            //Clamp, on est dans le mauvais sens
            if (enableInput && (playerInput.x >= 0f && velocity.x <= 0f) || (playerInput.x <= 0f && velocity.x >= 0f))
                velocity = new Vector2(airInitHorizontalSpeed * airHorizontalSpeed * playerInput.x.Sign(), velocity.y);
            if (enableInput && Mathf.Abs(velocity.x) < airInitHorizontalSpeed * airHorizontalSpeed * 0.95f && Mathf.Abs(playerInput.x) > 0.01f)
            {
                velocity = new Vector2(airInitHorizontalSpeed * airHorizontalSpeed * playerInput.x.Sign(), velocity.y);
            }
            else
            {
                float targetSpeed = !enableInput ? 0f : playerInput.x * airHorizontalSpeed;
                velocity = new Vector2(Mathf.MoveTowards(velocity.x, targetSpeed, airSpeedLerp * Time.deltaTime), velocity.y);
            }

        }
        else//phase descendante
        {
            //Clamp the fall speed
            float targetedSpeed;
            if(playerInput.rawY == -1 && enableInput)
            {
                targetedSpeed = -fallSpeed.y * Mathf.Max(fallClampSpeedMultiplierWhenDownPressed * Mathf.Abs(playerInput.y), 1f);
            }
            else
            {
                targetedSpeed = -fallSpeed.y;
            }

            if (velocity.y < targetedSpeed)//slow
            {
                velocity = new Vector2(velocity.x, Mathf.MoveTowards(velocity.y, targetedSpeed, fallDecelerationSpeedLerp * Time.deltaTime));
            }
            else
            {
                float coeff = velocity.y >= -fallSpeed.y * maxBeginFallSpeed ? fallGravityMultiplier * beginFallExtraGravity : fallGravityMultiplier;
                coeff = enableInput && playerInput.rawY < 0 ? coeff * fallGravityMultiplierWhenDownPressed : coeff;
                velocity = new Vector2(velocity.x, Mathf.MoveTowards(velocity.y, targetedSpeed, -Physics2D.gravity.y * coeff * Time.deltaTime));
            }

            //Horizontal movement
            //Clamp, on est dans le mauvais sens
            if (isQuittingConvoyerBelt)
            {
                float speed = speedWhenQuittingConvoyerBelt * (isQuittingConvoyerBeltRight ? 1f : -1f);
                velocity = new Vector2(speed, velocity.y);
            }
            else if(playerInput.rawX != 0 || Time.time - lastTimeQuitGround > inertiaDurationWhenQuittingGround)//else just keep our velocity
            {
                if (enableInput && (playerInput.x >= 0f && velocity.x <= 0f) || (playerInput.x <= 0f && velocity.x >= 0f))
                    velocity = new Vector2(fallInitHorizontalSpeed * fallSpeed.x * playerInput.x.Sign(), velocity.y);
                if (enableInput && Mathf.Abs(velocity.x) < fallInitHorizontalSpeed * fallSpeed.x * 0.95f && playerInput.rawX != 0)
                {
                    velocity = new Vector2(fallInitHorizontalSpeed * fallSpeed.x * playerInput.x.Sign(), velocity.y);
                }
                else
                {
                    float targetSpeed = !enableInput ? 0f : playerInput.x * fallSpeed.x;
                    velocity = new Vector2(Mathf.MoveTowards(velocity.x, targetSpeed, fallSpeedLerp * Time.deltaTime), velocity.y);
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
            if (!isDashDisable && !hasDashed && Time.time - lastTimeDashFinish >= dashCooldown)
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

        if (!isDashing)
            return;

        //anti knockhead
        if(isLastDashUp && antiKnockHead > 0f)
        {
            //right
            Vector2 detectSize = new Vector2((hitbox.size.x + 2f * gapBetweenHitboxAndWall) * 0.5f, hitbox.size.y + 2f * gapBetweenHitboxAndGround);
            Vector2 nonDetectSize = new Vector2((hitbox.size.x + 2f * gapBetweenHitboxAndWall) * 0.5f * (1f - antiKnockHead), detectSize.y);
            Vector2 detectOffset = new Vector2(detectSize.x * 0.5f, 0f);
            Vector2 nonDetectOffset = new Vector2(nonDetectSize.x * 0.5f, 0f);

            Collider2D detectCol = PhysicsToric.OverlapBox((Vector2)transform.position + detectOffset, detectSize, 0f, groundLayer);
            Collider2D nonDetectCol = PhysicsToric.OverlapBox((Vector2)transform.position + nonDetectOffset, nonDetectSize, 0f, groundLayer);
            if (detectCol != null && nonDetectCol == null)
            {
                MapColliderData colliderData = detectCol.GetComponent<MapColliderData>();
                if(colliderData != null && !colliderData.disableAntiKnockHead)
                {
                    Teleport((Vector2)transform.position + Vector2.left * (detectSize.x * antiKnockHead));
                }
            }

            detectCol = PhysicsToric.OverlapBox((Vector2)transform.position - detectOffset, detectSize, 0f, groundLayer);
            nonDetectCol = PhysicsToric.OverlapBox((Vector2)transform.position - nonDetectOffset, nonDetectSize, 0f, groundLayer);
            if (detectCol != null && nonDetectCol == null)
            {
                MapColliderData colliderData = detectCol.GetComponent<MapColliderData>();
                if (colliderData != null && !colliderData.disableAntiKnockHead)
                {
                    Teleport((Vector2)transform.position + Vector2.right * (detectSize.x * antiKnockHead));
                }
            }
        }

        lastTimeDashFinish = Time.time;
        float per100 = (Time.time - lastTimeDashBegin) / dashDuration;
        velocity = lastDashDir * (dashSpeedCurve.Evaluate(per100) * dashSpeed);
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
        velocity = dir * dashSpeedCurve.Evaluate(0);
        lastTimeDashBegin = Time.time;
        hasDashed = isDashing = dash = true;
        onDash.Invoke(dir);
    }

    #endregion

    #region Handle Slide

    private void HandleSlide()
    {
        if (!isSliding)
            return;

        //ralentir le glissement
        if(velocity.y <  -slideSpeed)
        {
            velocity = new Vector2(0f, Mathf.MoveTowards(velocity.y, -slideSpeed, slideSpeedLerpDeceleration * Time.deltaTime));
        }
        else if(velocity.y > -slideSpeed * initSlideSpeed * 0.95f)
        {
            velocity = new Vector2(0f, -slideSpeed * initSlideSpeed);
        }
        else
        {
            velocity = new Vector2(0f, Mathf.MoveTowards(velocity.y, -slideSpeed, slideSpeedLerp * Time.deltaTime));
        }
    }

    #endregion

    #region Handle Bump

    private void HandleBump()
    {
        if (!isBumping)
            return;

        //friction
        velocity = new Vector2(Mathf.MoveTowards(velocity.x, 0f, bumpFrictionLerp * Time.deltaTime), velocity.y);

        //gravity
        if (velocity.y < -maxFallBumpSpeed)//slow
        {
            velocity = new Vector2(velocity.x, Mathf.MoveTowards(velocity.y, -maxFallBumpSpeed, fallDecelerationSpeedLerp * Time.deltaTime));
        }
        else
        {
            velocity = new Vector2(velocity.x, Mathf.MoveTowards(velocity.y, -fallSpeed.y, -Physics2D.gravity.y * bumpGravityScale * Time.deltaTime));
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

        lastTimeLeavePlateform = -10f;
        if (jumpParticle != null)
            jumpParticle.Play();
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
            slideParticle.transform.parent.localScale = new Vector3(flip ? -1 : 1, 1, 1);
            main.startColor = Color.white;
        }
        else
        {
            main.startColor = Color.clear;
        }
    }

    #endregion

    #region Gizmos and OnValidate

    private IEnumerator PauseCorout()
    {
        Vector2 speed = velocity;

        Freeze();

        while(!enableBehaviour)
        {
            Freeze();
            yield return null;
        }

        UnFreeze();

        velocity = speed;
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

        //top
        Gizmos.DrawLine((Vector2)transform.position + topRaycastOffset, (Vector2)transform.position + topRaycastOffset + Vector2.up * topRaycastLength);
        Gizmos.DrawLine((Vector2)transform.position + new Vector2(-topRaycastOffset.x, topRaycastOffset.y), (Vector2)transform.position + new Vector2(-topRaycastOffset.x, topRaycastOffset.y) + Vector2.up * topRaycastLength);

        //Side detection
        Gizmos.DrawLine((Vector2)transform.position + Vector2.up * sideRayOffset, (Vector2)transform.position + Vector2.up * sideRayOffset + Vector2.right * sideRayLength);
        Gizmos.DrawLine((Vector2)transform.position + Vector2.up * sideRayOffset, (Vector2)transform.position + Vector2.up * sideRayOffset + Vector2.left * sideRayLength);

        //Grab
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector2(transform.position.x, transform.position.y + grabRayOffset) + (grabRayLength * Vector2.left), new Vector2(transform.position.x, transform.position.y + grabRayOffset) + (grabRayLength * Vector2.right));

        //visual Hitbox
        Gizmos.color = Color.blue;
        Vector2 hitboxCenter = (Vector2)transform.position + hitbox.offset;
        Collision2D.Hitbox extendedHitbox = new Collision2D.Hitbox(hitboxCenter, new Vector2(hitbox.size.x + 2f * gapBetweenHitboxAndWall, hitbox.size.y + 2f * gapBetweenHitboxAndGround));
        Collision2D.Hitbox.GizmosDraw(extendedHitbox, Color.blue);
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
        jumpMaxSpeed = new Vector2(Mathf.Max(0f, jumpMaxSpeed.x), Mathf.Max(0f, jumpMaxSpeed.y));
        doubleJumpSpeed = Mathf.Max(doubleJumpSpeed, 0f);
        wallJumpMaxDuration = Mathf.Max(wallJumpMaxDuration, 0f);
        wallJumpMinDuration = Mathf.Clamp(wallJumpMinDuration, 0f, wallJumpMaxDuration);
        apexJumpSpeed = new Vector2(Mathf.Max(0f, apexJumpSpeed.x), apexJumpSpeed.y);
        apexJumpSpeed2 = new Vector2(Mathf.Max(0f, apexJumpSpeed2.x), apexJumpSpeed2.y);
        slideSpeed = Mathf.Max(0f, slideSpeed);
        apexJumpSpeedLerp = Mathf.Max(0f, apexJumpSpeedLerp);
        grabSpeedLerp = Mathf.Max(0f, grabSpeedLerp);
        speedLerp = Mathf.Max(0f, speedLerp);
        grabRayLength = Mathf.Max(0f, grabRayLength);
        airSpeedLerp = Mathf.Max(0f, airSpeedLerp);
        jumpInitSpeed = Mathf.Max(0f, jumpInitSpeed);
        groundRaycastLength = Mathf.Max(0f, groundRaycastLength);
        minBumpSpeedX = Mathf.Max(0f, minBumpSpeedX);
        bumpFrictionLerp = Mathf.Max(0f, bumpFrictionLerp);
        bumpGravityScale = Mathf.Max(0f, bumpGravityScale);
        maxBumpDuration = Mathf.Max(minBumpDuration, maxBumpDuration);
        minBumpDuration = Mathf.Min(minBumpDuration, maxBumpDuration);
        maxPreciseGrabValue = Mathf.Max(maxPreciseGrabValue, 0f);
        inertiaDurationWhenQuittingGround = Mathf.Max(inertiaDurationWhenQuittingGround, 0f);
        gapBetweenHitboxAndGround = Mathf.Max(gapBetweenHitboxAndGround, 0f);
        gapBetweenHitboxAndWall = Mathf.Max(gapBetweenHitboxAndWall, 0f);
        sideRayLength = Mathf.Max(sideRayLength, 0f);
        topRaycastOffset = new Vector2(Mathf.Max(0f, topRaycastOffset.x), topRaycastOffset.y);
        topRaycastLength = Mathf.Max(topRaycastLength, 0f);
        groundLayer = LayerMask.GetMask("Floor", "WallProjectile");
    }

    #endif

#endregion
}
