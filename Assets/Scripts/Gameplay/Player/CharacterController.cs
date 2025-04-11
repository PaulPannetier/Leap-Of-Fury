using System;
using System.Collections;
using System.Collections.Generic;
using ToricCollider2D;
using UnityEngine;
using DG.Tweening;
using System.Linq;

public class CharacterController : MonoBehaviour
{
    #region Bonus

    private class BonusHandler
    {
        private float initValue;
        private List<float> bonusPercent;
        private List<float> bonus;
        private float _currentValue;
        public float currentValue
        {
            get => _currentValue;
            private set
            {
                _currentValue = value;
            }
        }

        public BonusHandler(float initValue)
        {
            this.initValue = initValue;
            _currentValue = initValue;
            bonusPercent = new List<float>();
            bonus = new List<float>();
        }

        private void RecalculateValue()
        {
            float newValue = initValue;
            float currentBonus = bonus.Sum();
            float currentBonusPercent = bonusPercent.Sum() * initValue;
            currentValue = initValue + currentBonus + currentBonusPercent;
        }

        public void AddPercentBonus(float bonusValue)
        {
            bonusPercent.Add(bonusValue);
            RecalculateValue();
        }

        public void AddBonus(float bonusValue)
        {
            bonus.Add(bonusValue);
            RecalculateValue();
        }

        public bool RemovePercentBonus(float bonusValue)
        {
            bool remove = bonusPercent.Remove(bonusValue);
            if(remove)
                RecalculateValue();
            return remove;
        }

        public bool RemoveBonus(float bonusValue)
        {
            bool remove = bonus.Remove(bonusValue);
            if(remove)
                RecalculateValue();
            return remove;       
        }

        public void Reset()
        {
            bonus.Clear();
            bonusPercent.Clear();
            currentValue = initValue;
        }
    }

    [Flags]
    public enum BonusType : byte
    {
        None = 0,
        Walk = 1,
        Grab = 1 << 1,
        Jump = 1 << 2, //increase jump init speed and continuous jump acceleration
        WallJump = 1 << 3,
        Fall = 1 << 4,
        Dash = 1 << 5
    }

    #endregion

    #region Fields

    #region private field

    private Camera mainCam;
    private CharacterInputs playerInput;
    private PlayerCommon playerCommon;
    private BoxCollider2D hitbox;
    private ToricRaycastHit2D raycastRight, raycastLeft, groundRay, rightSlopeRay, leftSlopeRay, rightFootRay, leftFootRay, topRightRay, topLeftRay;
    private Collider2D groundCollider, oldGroundCollider, leftWallCollider, rightWallCollider;
    private MapColliderData groundColliderData, lastGroundColliderData, leftWallColliderData, rightWallColliderData, apexJumpColliderData;
    private ToricObject toricObject;
    private short disableMovementCounter, disableDashCounter, disableInstantTurnCounter;
    private bool isMovementDisabled => disableMovementCounter > 0;
    private bool isDashDisabled => disableDashCounter > 0;
    private bool disableInstantTurn => disableInstantTurnCounter > 0;

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
    public bool enableMagneticField = false;
    private BonusHandler walkSpeedHandler;
    private BonusHandler grabSpeedHandler;
    private BonusHandler jumbForceHandler;
    private BonusHandler wallJumpForceHandler;
    private BonusHandler fallSpeedYHandler;
    private BonusHandler dashSpeedHandler;

    [field:SerializeField, ShowOnly] public Vector2 velocity { get; private set; }


#if UNITY_EDITOR
    [SerializeField] private bool drawGizmos = true;
#endif

    #endregion

    #region General/Collision

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

    #endregion

    #region Walk

    [Header("Walk")]
    [Tooltip("La vitesse de marche"), SerializeField] private float walkSpeed = 10f;
    [Tooltip("La vitesse d'interpolation de marche"), SerializeField] private float speedLerp = 50f;
    [Tooltip("La propor de vitesse initiale de marche"), SerializeField, Range(0f, 1f)] private float initSpeed = 0.2f;
    [Tooltip("La durée de conservation de la vitesse de marche apres avoir quitté une plateforme"), SerializeField] private float inertiaDurationWhenQuittingGround = 0.1f;
    private float lastTimeQuitGround = -10f;
    private bool forceHorizontalStick = false, forceDownStick = false, overwriteSpeedOnHorizontalTeleportation = true, overwriteSpeedOnVerticalTeleportation = true;
    private Vector2 teleportationShift;

    [Header("Slope")]
    [Tooltip("The maximum angle in degres we can walk"), SerializeField, Range(0f, 90f)] private float maxSlopeAngle = 22.5f;
    [SerializeField] private float slopeSpeed;
    [SerializeField, Range(0f, 1f)] private float initSlopeSpeed = 0.3f;
    [SerializeField, Tooltip("La vitesse de monter selon l'angle de la pente : 0 => pente nulle, 1 => pente max en %ageVMAX")] private AnimationCurve slopeSpeedCurve;
    [SerializeField, Tooltip("acceleration during slope in %ageVMAX/sec ")] private float slopeSpeedLerp = 1f;
    [HideInInspector] public bool isSlopingRight, isSlopingLeft;
    private float slopeAngleLeft, slopeAngleRight;
    private bool isToSteepSlopeRight, isToSteepSlopeLeft;

    #endregion

    #region Jump

    #region Normal Jump

    [Header("Jumping")]
    [Tooltip("Initial jumps speed")] [SerializeField] private float jumpInitSpeed = 20f;
    [Tooltip("Continuous upward acceleration du to jump.")] [SerializeField] private float jumpForce = 20f;
    [Tooltip("Modifie la gravité lorsqu'on monte en l'air mais sans sauter.")] [SerializeField] private float jumpGravityMultiplier = 1f;
    [Tooltip("The minimum duration of a jump.")] [SerializeField] private float jumpMinDuration = 0.1f;
    [Tooltip("The maximum duration of a jump.")] [SerializeField] private float jumpMaxDuration = 1f;
    [Tooltip("La vitesse maximal de saut (VMAX en l'air).")] [SerializeField] private Vector2 jumpMaxSpeed = new Vector2(4f, 20f);
    [Tooltip("La vitesse init horizontale en saut (%age de la vitesse max)")] [Range(0f, 1f)] [SerializeField] private float jumpInitHorizontaSpeed = 0.4f;
    [Tooltip("La vitesse d'interpolation de la vitesse horizontale de saut")] [SerializeField] private float jumpSpeedLerp = 20f;
    [SerializeField] private bool enableDoubleJump = true;
    [Tooltip("The minimum delay after a normal jump to double jumps"), SerializeField] private float minDelayToDoubleJumpAfterNormalJump = 0.2f;
    [Tooltip("The speed of the second jump(magnitude of his velocity vector)"), SerializeField] private float doubleJumpSpeed = 6f;
    [Tooltip("The angle of the double jump"), SerializeField, Range(0f, 90f)] private float doubleJumpAngle = 45f;
    [Tooltip("Le temps apres avoir quité la plateforme ou le saut est possible")] [SerializeField] private float jumpCoyoteTime = 0.1f;
    private float lastTimeLeavePlateform = -10f, lastTimeJumpCommand = -10f, lastTimeBeginJump = -10f;

    #endregion

    #region WallJump

    [Header("Wall jump Opposite Wall")]
    [Tooltip("la vitesse de début de saut de mur"), SerializeField] private float wallJumpInitSpeed = 10f;
    [Tooltip("The angle in degrees between the wall and the horizontal."), Range(0f, 90f), SerializeField] private float wallJumpAngle = 45f;
    [Tooltip("L'accélération continue du saut depuis le mur."), SerializeField] private float wallJumpForce = 20f;
    [Tooltip("La vitesse maximal de saut depuis le mur (VMAX en l'air)."), SerializeField] private Vector2 wallJumpMaxSpeed = new Vector2(4f, 20f);
    [SerializeField] private float wallJumpSpeedLerp = 15f;
    [Tooltip("Modifie la gravité lorsqu'on monte en l'air mais sans sauter."), SerializeField] private float wallJumpGravityMultiplier = 1f;
    [Tooltip("La durée minimale ou le joueur doit avoir la touche de saut effective"), SerializeField] private float wallJumpMinDuration = 0.1f;
    [Tooltip("La durée maximal ou le joueur peut avoir la touche de saut effective"), SerializeField] private float wallJumpMaxDuration = 1f;
    [Tooltip("The minimum delay after a wall jump to double jumps"), SerializeField] private float minDelayToDoubleJumpAfterWallJump = 0.2f;
    [Tooltip("Allow to use the double jumps after a wall jumps in the opposite direction of the wall jump"), SerializeField] private bool allowDoubleJumpAfterWallJumpInOppositeDirection = false;
    [Tooltip("The duration where the double jumps is desactivated after a wall jumps (if the double jumps is on theopposite direction of the wall jump)"), SerializeField]
    private float delayDesactivateDoubleJumpAfterWallJumpInOppositeDirection = 0.5f;
    private bool wallJumpOnNonGrabableWall;
    private float lastTimeBeginWallJump = -10f, lastTimeBeginWallJumpOnNonGrabableWall = -10f;
    private bool isLastWallJumpsDirIsRight;

    [Header("Wall Jump Along Wall")]
    [SerializeField] private bool enableJumpAlongWall = true;
    [Tooltip("la vitesse de début de saut de mur face au mur"), SerializeField] private float wallJumpAlongSpeed = 20f;
    [Tooltip("la courbe de vitesse saut de mur face au mur"), SerializeField] private AnimationCurve wallJumpAlongCurveSpeed;
    [Tooltip("La durée d'un saut face au mur"), SerializeField] private float jumpAlongWallDuration = 0.3f;
    [Tooltip("Le temps minimal entre 2 saut face au mur (sec)"), SerializeField] private float wallJumpAlongCooldown = 0.1f;
    [Tooltip("The minimum delay after a jump along wall to double jumps"), SerializeField] private float minDelayToDoubleJumpAfterJumpAlongWall = 0.2f;

    private float lastTimeBeginWallJumpAlongWall = -10f;

    #endregion

    #region WallJump on Non Grabbable wall

    [Header("Wall Jump on non Grabbable Wall")]
    [Tooltip("Enable or not the jump on non grabable wall."), SerializeField] private bool enableWallJumpOnNonGrabableWall = false;
    [Tooltip("The angle in degrees between the non grabable wall and the horizontal."), Range(0f, 90f), SerializeField] private float wallJumpAngleOnNonGrabableWall = 45f;
    [Tooltip("la vitesse de début de saut de mur"), SerializeField] private float wallJumpInitSpeedOnNonGrabableWall = 20f;
    [Tooltip(""), SerializeField] private float wallJumpOnNonGrabableWallDuration = 0.2f;

    #endregion

    #endregion

    #region Air/Fall

    [Header("Air")]//In falling state but with velocity.y > 0
    [Tooltip("Gravity multiplier when falling with upward velocity.")] [SerializeField] private float airGravityMultiplier = 1f;
    [Tooltip("Max horizontal speed when falling with upward velocity.")] [SerializeField] private float airHorizontalSpeed = 4f;
    [Tooltip("Init horizontal speed when falling with upward velocity in percentage of \"airHorizontalSpeed\")")] [Range(0f, 1f)] [SerializeField] private float airInitHorizontalSpeed = 0.4f;
    [Tooltip("Interpolation horizontal speed when falling with upward velocity.")] [SerializeField] private float airSpeedLerp = 20f;

    [Header("Fall")]
    [Tooltip("Coeff ajustant l'accélération de chute.")] [SerializeField] private float fallGravityMultiplier = 1.5f;
    [Tooltip("Vitesse initial horizontale en chute (%age de vitesse max)")] [SerializeField] [Range(0f, 1f)] private float fallInitHorizontalSpeed = 0.35f;
    [Tooltip("La vitesse maximal de chute en y")] [SerializeField] private float fallSpeedY = 14f;
    [Tooltip("La vitesse maximal de chute en x en début de chute")] [SerializeField] private float beginFallSpeedX = 8f;
    [Tooltip("La vitesse maximal de chute en x en fin de chute")] [SerializeField] private float endFallSpeedX = 5f;
    [Tooltip("La durée de début de chute (Sert pour beginFallSpeedX et endFallSpeedX)")] [SerializeField] private float beginFallDuration = 1f;
    [Tooltip("La vitesse d'interpolation de la vitesse horizontale de chute")] [SerializeField] private float fallSpeedLerp = 10f;
    [Tooltip("La vitesse d'interpolation de la réduction de vitesse horizontale de chute")] [SerializeField] private float fallDecelerationSpeedLerp = 10f;
    [Tooltip("La gravité lors du début de la chute")] [SerializeField] private float beginFallExtraGravity = 2f;
    [Tooltip("Définie le %age de vitesse on l'on considère un début de chute.")] [SerializeField] [Range(0f, 1f)] private float maxBeginFallSpeed = 0.3f;
    [Tooltip("Change la vitesse maximal de chute lors de l'appuie sur le bouton bas.")] [SerializeField] private float fallClampSpeedMultiplierWhenDownPressed = 1.2f;
    [Tooltip("Change la gravité appliqué lors de l'appuie sur la touche bas.")] [SerializeField] private float fallGravityMultiplierWhenDownPressed = 2f;
    private float lastTimefall;

    #endregion

    #region Grab

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

    #endregion

    #region Dash

    [Header("Dash")]
    [Tooltip("La vitesse maximal du dash")] [SerializeField] private float dashSpeed = 20f;
    [Tooltip("La durée du dash en sec")] [SerializeField] private float dashDuration = 0.4f;
    [Tooltip("Le temps durant lequel un dash est impossible après avoir fini un dash")] [SerializeField] private float dashCooldown = 0.15f;
    [Tooltip("La courbe de vitesse de dash")] [SerializeField] private AnimationCurve dashSpeedCurve;
    [Tooltip("%age de la hitbox qui est ignoré lors d'un dash vers le haut"), SerializeField, Range(0f, 1f)] private float antiKnockHead;
    private Vector2 lastDashDir;
    private bool isLastDashUp = false;
    private float lastTimeDashCommand = -10f, lastTimeDashFinish = -10f, lastTimeDashBegin = -10f;

    #endregion

    #region Slide

    [Header("Slide")]
    [Tooltip("La vitesse de glissement sur les murs")] [SerializeField] private float slideSpeed = 5f;
    [Tooltip("L'interpolation lorsqu'on glisse sur un mur.")] [SerializeField] private float slideSpeedLerp = 10f;
    [Tooltip("L'interpolation lorsqu'on ralentie en glissant sur un mur.")] [SerializeField] private float slideSpeedLerpDeceleration = 55f;
    [Tooltip("La vitesse initiale de glissement en %age de vitesse max lorsqu'on glisse a partir de 0.")] [SerializeField] [Range(0f, 1f)] private float initSlideSpeed = 0.1f;

    #endregion

    #region Bump

    [Header("Bump")]
    [SerializeField] private float minBumpSpeedX = 1f;
    [SerializeField] private float maxFallBumpSpeed = 30f;
    [SerializeField] private float bumpFrictionLerp = 2f;
    [SerializeField] private float bumpGravityScale = 1f;
    [SerializeField] private float maxBumpDuration = 1.5f;
    [SerializeField] private float minBumpDuration = 0.3f;
    private float lastTimeBump = -10f;

    #endregion

    #region Map

    [Header("Map Object")]
    [SerializeField] private float inertiaDurationWhenQuittingConvoyerBelt = 0.08f;
    [SerializeField] private float speedWhenQuittingConvoyerBelt = 1f;
    private float lastTimeQuittingConvoyerBelt = -10f;
    private bool isQuittingConvoyerBelt, isQuittingConvoyerBeltRight, isQuittingConvoyerBeltLeft;
    private bool isTraversingOneWayPlateformUp, isTraversingOneWayPlateformDown;
    private bool isTraversingOneWayPlateform => isTraversingOneWayPlateformUp || isTraversingOneWayPlateformDown;

    #endregion

    #region VisualEffect

    [Header("Polish")]
    [SerializeField] private ParticleSystem dashParticle;
    [SerializeField] private ParticleSystem jumpParticle;
    [SerializeField] private ParticleSystem wallJumpParticle;
    [SerializeField] private ParticleSystem slideParticle;
    [SerializeField] private bool enableCameraShaking = true;
    [SerializeField] private ShakeSetting cameraShakeSetting = new ShakeSetting(0.15f, 0.2f, 14, 90, false, true);

    #endregion

    #region State

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
    public bool isWallJumpingOnNonGrabableWall { get; private set; }
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
    private bool oldWallJump, oldJump, oldSecondJump, oldWallJumpAlongWall, oldWallJumpOnNonGrabableWall, oldDash;//use to set var on top just for only one frame

    private bool doJump, doDash;

    [HideInInspector] public bool flip { get; private set; } = false;
    public Action<Vector2> onDash;

    #endregion

    #endregion

    #region Public Methods

    #region Bonus

    private BonusHandler GetHandler(BonusType bonusType)
    {
        switch (bonusType)
        {
            case BonusType.Walk:
                return walkSpeedHandler;
            case BonusType.Grab:
                return grabSpeedHandler;
            case BonusType.Jump:
                return jumbForceHandler;
            case BonusType.WallJump:
                return wallJumpForceHandler;
            case BonusType.Fall:
                return fallSpeedYHandler;
            case BonusType.Dash:
                return dashSpeedHandler;
            default:
                return null;
        }
    }

    public void AddBonus(BonusType bonusType, float value)
    {
        BonusHandler bonusHandler = GetHandler(bonusType);
        if (bonusHandler != null)
        {
            bonusHandler.AddBonus(value);
        }
    }

    public void AddBonusPercent(BonusType bonusType, float value)
    {
        BonusHandler bonusHandler = GetHandler(bonusType);
        if (bonusHandler != null)
        {
            bonusHandler.AddPercentBonus(value);
        }
    }

    public bool RemoveBonus(BonusType bonusType, float value)
    {
        BonusHandler bonusHandler = GetHandler(bonusType);
        if (bonusHandler != null)
        {
            return bonusHandler.RemoveBonus(value);
        }
        return false;
    }

    public bool RemoveBonusPercent(BonusType bonusType, float value)
    {
        BonusHandler bonusHandler = GetHandler(bonusType);
        if (bonusHandler != null)
        {
            return bonusHandler.RemovePercentBonus(value);
        }
        return false;
    }

    #endregion

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
        enableInput = !isMovementDisabled;

        yield return PauseManager.instance.Wait(duration);

        disableMovementCounter--;
        enableInput = !isMovementDisabled;
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
        isDashing = isSliding = wallGrab = isJumping = isApexJump1 = isApexJump2  = grabApexRight = grabApexLeft = isFalling = wallJump = isWallJumping = isWallJumpingOnNonGrabableWall = wallJumpOnNonGrabableWall = false;
        doubleJump = false;
        lastTimeBump = Time.time;
    }

    public void ForceApplyVelocity(in Vector2 velocity)
    {
        this.velocity = velocity;
    }

    private void DisableInstantTurn(float duration)
    {
        StartCoroutine(DisableInstantTurnCorout(duration));
    }

    private IEnumerator DisableInstantTurnCorout(float duration)
    {
        disableInstantTurnCounter++;
        yield return PauseManager.instance.Wait(duration);
        disableInstantTurnCounter--;
    }

    #endregion

    #region Awake and Start

    private void Awake()
    {
        this.transform = base.transform;
        mainCam = Camera.main;
        playerInput = GetComponent<CharacterInputs>();
        hitbox = GetComponent<BoxCollider2D>();
        playerCommon = GetComponent<PlayerCommon>();
        onDash = (Vector2 dir) => { };
        walkSpeedHandler = new BonusHandler(walkSpeed);
        grabSpeedHandler = new BonusHandler(grabSpeed);
        jumbForceHandler = new BonusHandler(jumpForce);
        wallJumpForceHandler = new BonusHandler(wallJumpForce);
        fallSpeedYHandler = new BonusHandler(fallSpeedY);
        dashSpeedHandler = new BonusHandler(dashSpeed);
}

    private void Start()
    {
        toricObject = GetComponent<ToricObject>();
        toricObject.useCustomUpdate = true;
        oldGroundCollider = null;
        PauseManager.instance.callBackOnPauseDisable += OnPauseDisable;
        PauseManager.instance.callBackOnPauseEnable += OnPauseEnable;
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

        toricObject.CustomUpdate();

        // VIII-Debug
        //DebugText.instance.text += $"ElecField : {enableMagneticField}\n";
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
        //DebugText.instance.text += $"{velocity}\n";
        //DebugText.instance.text += $"shift : {shift / Time.deltaTime}\n";
        //DebugText.instance.text += $"wallJump : {isWallJumping}\n";
        //DebugText.instance.text += $"wallGrab : {wallGrab}\n";
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
            slopeAngleLeft = Useful.WrapAngle((270f - Vector2.Angle(Vector2.left, leftSlopeRay.normal)) * Mathf.Deg2Rad);
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
            if(oldGroundCollider.GetComponent<MapColliderData>().groundType == MapColliderData.GroundType.ice)
            {
                DisableInstantTurn(0.1f);
            }
        }

        //Trigger groundTouch
        if (isGrounded && !oldOnGround && !isJumping && !isTraversingOneWayPlateformUp)
        {
            GroundTouch();
            groundTouch = true;
        }
        //enable dash
        if(hasDashed && isGrounded && !isTraversingOneWayPlateformUp && !isTraversingOneWayPlateformDown && (Time.time - lastTimeDashBegin > dashDuration))
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
        if (enableInput && !wallGrab && onWall && playerInput.grabPressed && (!isSliding || playerInput.rawY >= 0) && !isDashing && !isWallJumping && !isJumpingAlongWall && !isBumping)
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
        if (!isFalling && !isJumping && !isWallJumping && !isJumpingAlongWall && !isWallJumpingOnNonGrabableWall && !wallGrab && !isApexJumping && !isSliding && !isDashing && (!isGrounded || isTraversingOneWayPlateform) && !isBumping)
        {
            isFalling = true;
            lastTimefall = Time.time;
        }
        //release jumping and falling
        if ((isGrounded && !isTraversingOneWayPlateform && (velocity.y - groundColliderData.velocity.y) <= 1e-5f) || wallGrab || dash || (isSliding && !wallJump && !isWallJumpingOnNonGrabableWall))
        {
            isJumping = isWallJumping = isWallJumpingOnNonGrabableWall = isJumpingAlongWall = isFalling = false;
        }
        //Release Falling
        if(jump || wallJump || wallJumpOnNonGrabableWall || doubleJump || isApexJumping)
        {
            isFalling = false;
        }

        //release jumping and trigger falling
        if (isJumping && (velocity.y <= 0f || (!playerInput.jumpPressed && Time.time - lastTimeBeginJump > jumpMinDuration) || (Time.time - lastTimeBeginJump > jumpMaxDuration)))
        {
            isJumping = false;
            //cond  || v.y > 0f pour éviter un bug ou la touche saut est activé une seul frame!, ainsi le saut est tellement cour que isGrounded est tj vrai
            if ((!isGrounded || velocity.y > 0f || isTraversingOneWayPlateform) && !wallGrab && !isApexJumping && !isDashing && !isSliding && !isBumping)
            {
                if (!isFalling)
                {
                    isFalling = true;
                    lastTimefall = Time.time;
                }
            }
        }
        //release Wall jumping and trigger falling
        if (isWallJumping && (velocity.y <= 0f || (!playerInput.jumpPressed && Time.time - lastTimeBeginWallJump > wallJumpMinDuration) || (Time.time - lastTimeBeginWallJump > wallJumpMaxDuration)))
        {
            isWallJumping = false;
            //cond  || v.y > 0f pour éviter un bug ou la touche saut est activé une seul frame!, ainsi le saut est tellement court que isGrounded est tj vrai
            if ((!isGrounded || velocity.y > 0f || isTraversingOneWayPlateformUp || isTraversingOneWayPlateformDown) && !wallGrab && !!isApexJumping && !isDashing && !isSliding && !isBumping)
            {
                if (!isFalling)
                {
                    isFalling = true;
                    lastTimefall = Time.time;
                }
            }
        }
        //release Wall jumping along wall and trigger falling
        if (isJumpingAlongWall && (isDashing || (Time.time - lastTimeBeginWallJumpAlongWall > jumpAlongWallDuration)))
        {
            isJumpingAlongWall = false;
            if (!isGrounded && !wallGrab && !isDashing && !isApexJumping && !isSliding && !isBumping)
            {
                if (!isFalling)
                {
                    isFalling = true;
                    lastTimefall = Time.time;
                }
            }
        }

        //release Wall jumping on non grabable wall and trigger falling
        if (isWallJumpingOnNonGrabableWall && (velocity.y <= 0f || isDashing || Time.time - lastTimeBeginWallJumpOnNonGrabableWall > wallJumpOnNonGrabableWallDuration))
        {
            isWallJumpingOnNonGrabableWall = false;
            if ((!isGrounded || velocity.y > 0f || isTraversingOneWayPlateform) && !wallGrab && !isApexJumping && !isDashing && !isSliding && !isBumping)
            {
                if (!isFalling)
                {
                    isFalling = true;
                    lastTimefall = Time.time;
                }
            }
        }

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
        if(isSliding && (!enableInput || (isGrounded && !isTraversingOneWayPlateform) || !onWall || (!playerInput.grabPressed && ((onRightWall && playerInput.rawX != 1) || (onLeftWall && playerInput.rawX != -1))) || wallGrab || isDashing || jump || wallJump || wallJumpOnNonGrabableWall))
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

            if ((onWall || isGrounded) && Time.time - lastTimeBump >= minBumpDuration)
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
            if(Mathf.Abs(velocity.x) > beginFallSpeedX * fallInitHorizontalSpeed * 0.95f)
            {
                flip = velocity.x > 0f ? false : true;
            }
        }
        else if(isJumping || isWallJumping || isWallJumpingOnNonGrabableWall)
        {
            if (Mathf.Abs(velocity.x) > jumpMaxSpeed.x * jumpInitHorizontaSpeed * 0.95f)
            {
                flip = velocity.x > 0f ? false : true;
            }
        }
        else if(isDashing)
        {
            float dashSpeed = dashSpeedHandler.currentValue;
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
        if (oldWallJumpOnNonGrabableWall)
            wallJumpOnNonGrabableWall = false;
        if (oldWallJumpAlongWall)
            wallJumpAlongWall = false;

        oldDash = dash;
        oldJump = jump;
        oldWallJump = wallJump;
        oldSecondJump = doubleJump;
        oldWallJumpOnNonGrabableWall = wallJumpOnNonGrabableWall;
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

        forceHorizontalStick = (isSliding || wallGrab) && !isWallJumping && !isWallJumpingOnNonGrabableWall;
        forceDownStick = !isJumping && !isWallJumping && !wallGrab && !isDashing && !isApexJumping && !isSliding && !isBumping;

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
        if (!isGrounded || isFalling || isJumping || isWallJumping || isDashing || isSliding || wallGrab || isApexJumping || isBumping || isTraversingOneWayPlateformUp)
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
            float walkSpeed = walkSpeedHandler.currentValue;

            //Avoid stick on side
            if (enableInput && raycastLeft && playerInput.rawX == 1)
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
            if (enableInput && ((playerInput.x >= 0f && localVel.x <= 0f) || (playerInput.x <= 0f && localVel.x >= 0f)) && !disableInstantTurn)
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

            if (Mathf.Abs(localVel.x) < initSpeed * walkSpeed * 0.95f && playerInput.rawX != 0 && enableInput && !disableInstantTurn)
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
            float walkSpeed = walkSpeedHandler.currentValue;

            //Avoid stick on side
            if (enableInput && raycastLeft && playerInput.rawX == 1)
            {
                MapColliderData sideColliderData = raycastLeft.collider.GetComponent<MapColliderData>();
                if ((int)sideColliderData.velocity.x.Sign() == 1 && sideColliderData.velocity.x < walkSpeed)
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

            IceColliderData iceColliderData = groundColliderData as IceColliderData;
            if (iceColliderData == null)
            {
                string logMessage = "Can't have a MapColliderData with groundType == ice which is not of type IceColliderData!";
                print(logMessage);
                LogManager.instance.AddLog(logMessage, groundColliderData, "CharacterController::HandleIceWalk");
            }

            float targetedSpeed, speedLerp;
            if(enableInput && playerInput.rawX != 0)
            {
                if (!onWall || (playerInput.rawX == 1 && !onRightWall) || (playerInput.rawX == -1 && !onLeftWall))
                {
                    targetedSpeed = playerInput.x * walkSpeed;
                    speedLerp = this.speedLerp * iceColliderData.iceSpeedLerpFactor;
                }
                else
                {
                    targetedSpeed = 0f;
                    speedLerp = this.speedLerp * iceColliderData.iceDecelerationSpeedLerpFactor;
                }
            }
            else
            {
                targetedSpeed = 0f;
                speedLerp = this.speedLerp * iceColliderData.iceDecelerationSpeedLerpFactor;
            }

            localVel = new Vector2(Mathf.MoveTowards(localVel.x, targetedSpeed, speedLerp * Time.deltaTime), localVel.y);

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
            float walkSpeed = walkSpeedHandler.currentValue;

            if(!convoyer.isActive)
            {
                HandleNormalWalk();
                return;
            }

            if(!enableInput || playerInput.rawX == 0)
            {
                velocity = new Vector2(convoyer.maxSpeed, velocity.y);
            }
            else
            {
                if((int)convoyer.maxSpeed.Sign() == (int)(playerInput.x * walkSpeed).Sign())
                {
                    float targetSpeed = playerInput.x * walkSpeed + convoyer.maxSpeed;
                    float lerp = Mathf.Max(speedLerp, targetSpeed);
                    float speedX = Mathf.MoveTowards(velocity.x, targetSpeed, lerp * Time.deltaTime);
                    if (Mathf.Abs(speedX) < Mathf.Abs(convoyer.maxSpeed))
                        speedX = convoyer.maxSpeed;
                    velocity = new Vector2(speedX, velocity.y);
                }
                else
                {
                    float targetSpeed = (playerInput.x * walkSpeed) + convoyer.maxSpeed;
                    if((int)targetSpeed.Sign() != (int)velocity.x.Sign())
                    {
                        velocity = new Vector2(0f, velocity.y);
                    }
                    float lerp = Mathf.Abs(playerInput.x * walkSpeed) > Mathf.Abs(convoyer.maxSpeed) ? speedLerp : convoyer.speedLerp;
                    velocity = new Vector2(Mathf.MoveTowards(velocity.x, targetSpeed, lerp * Time.deltaTime), velocity.y);
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
                float grabSpeed = grabSpeedHandler.currentValue;
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
        bool IsDoubleJumpDelay()
        {
            return Time.time - lastTimeApexJump >= minDelayToDoubleJumpAfterNormalJump &&
                Time.time - lastTimeBeginJump >= minDelayToDoubleJumpAfterNormalJump &&
                Time.time - lastTimeBeginWallJump >= minDelayToDoubleJumpAfterWallJump &&
                (!enableJumpAlongWall || Time.time - lastTimeBeginWallJumpAlongWall >= minDelayToDoubleJumpAfterJumpAlongWall) &&
                Time.time - lastTimeBeginWallJumpOnNonGrabableWall >= minDelayToDoubleJumpAfterWallJump;
        }

        bool DoubleJumpAfterWallJumpsConditions()
        {
            if(allowDoubleJumpAfterWallJumpInOppositeDirection)
                return true;

            if (isWallJumping || Time.time - lastTimeBeginWallJump < delayDesactivateDoubleJumpAfterWallJumpInOppositeDirection ||
                Time.time - lastTimeBeginWallJumpOnNonGrabableWall < delayDesactivateDoubleJumpAfterWallJumpInOppositeDirection)
            {
                if (playerInput.rawX == 0)
                    return true;

                bool rigth = playerInput.rawX == 1;
                return isLastWallJumpsDirIsRight != rigth;
            }
            return true;
        }

        if (doJump)
        {
            if (isGrounded && !wallGrab && !isBumping)
            {
                Jump(Vector2.up);
                doJump = false;
            }
            else if (!isGrounded && Time.time - lastTimeLeavePlateform <= jumpCoyoteTime && !wallGrab && !isBumping)
            {
                Jump(Vector2.up);
                doJump = false;
            }
            else if ((wallGrab || onWall || isSliding) && !isApexJumping && !isBumping)
            {
                WallJump();
                doJump = false;
            }
            else if (!isGrounded && !onWall && !hasDoubleJump && !isApexJumping && !isBumping && IsDoubleJumpDelay() && DoubleJumpAfterWallJumpsConditions() && enableDoubleJump)
            {
                DoubleJump();
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

        if (isJumping)
        {
            HandleNormalJump();
        }
        else if(isWallJumping)
        {
            HandleWallJump();
        }
        else if (isJumpingAlongWall)
        {
            HandleJumpAlongWall();
        }
        else if(isWallJumpingOnNonGrabableWall)
        {
            HandleJumpOnNonGrabableWall();
        }

        #region Normal Jump

        void HandleNormalJump()
        {
            //vertical movement
            float jumpForce = wallJumpForceHandler.currentValue;
            velocity += Vector2.up * (Physics2D.gravity.y * jumpGravityMultiplier * Time.deltaTime);
            velocity += Vector2.up * (jumpForce * Time.deltaTime);

            //clamp in the y axis
            if (velocity.y > jumpMaxSpeed.y)
            {
                velocity = new Vector2(velocity.x, jumpMaxSpeed.y);
            }

            bool isAffectedByMagneticField = false;
            Vector2 magneticFieldForce = Vector2.zero;
            if (enableMagneticField)
            {
                List<ElectricFieldPassif.ElectricField> magneticFields = GetMagneticFields();
                isAffectedByMagneticField = magneticFields.Count > 0;
                foreach (ElectricFieldPassif.ElectricField field in magneticFields)
                {
                    float force = field.maxFieldForce * Mathf.Clamp01(field.fieldForceOverDistance.Evaluate(field.center.Distance(transform.position) / field.fieldRadius));
                    Vector2 dir = ((Vector2)transform.position - field.center).normalized;
                    magneticFieldForce += force * dir;
                }
            }

            //Horizontal movement
            if(isAffectedByMagneticField)
            {
                if (enableInput && playerInput.rawX != 0 && ((playerInput.x >= 0f && velocity.x <= 0f) || (playerInput.x <= 0f && velocity.x >= 0f)) && !disableInstantTurn)
                {
                    float targetSpeedX = jumpInitHorizontaSpeed * jumpMaxSpeed.x * playerInput.rawX;
                    velocity = new Vector2(Mathf.MoveTowards(velocity.x, targetSpeedX, jumpSpeedLerp * Time.deltaTime), velocity.y);
                }
            }
            else
            {
                if (enableInput && ((playerInput.x >= 0f && velocity.x <= 0f) || (playerInput.x <= 0f && velocity.x >= 0f)) && !disableInstantTurn)
                {
                    velocity = new Vector2(jumpInitHorizontaSpeed * jumpMaxSpeed.x * playerInput.rawX, velocity.y);
                }
            }


            if (isAffectedByMagneticField)
            {
                if(playerInput.rawX != 0)
                {
                    float targetSpeed = enableInput ? playerInput.rawX * Mathf.Max(Mathf.Abs(playerInput.x * jumpMaxSpeed.x), jumpInitHorizontaSpeed * jumpMaxSpeed.x) : 0f;
                    velocity = new Vector2(Mathf.MoveTowards(velocity.x, targetSpeed, jumpSpeedLerp * Time.deltaTime), velocity.y);
                }
            }
            else
            {
                if (Mathf.Abs(velocity.x) < jumpInitHorizontaSpeed * jumpMaxSpeed.x * 0.95f && playerInput.rawX != 0 && !disableInstantTurn)
                {
                    velocity = new Vector2(jumpInitHorizontaSpeed * jumpMaxSpeed.x * playerInput.rawX, velocity.y);
                }
                else
                {
                    float targetSpeed = enableInput ? playerInput.rawX * Mathf.Max(Mathf.Abs(playerInput.x * jumpMaxSpeed.x), jumpInitHorizontaSpeed * jumpMaxSpeed.x) : 0f;
                    velocity = new Vector2(Mathf.MoveTowards(velocity.x, targetSpeed, jumpSpeedLerp * Time.deltaTime), velocity.y);
                }
            }

            velocity += Time.deltaTime * magneticFieldForce;
        }

        #endregion

        #region Wall Jump

        void HandleWallJump()
        {
            //Vertical movement
            float wallJumpForce = wallJumpForceHandler.currentValue;
            velocity += Vector2.up * (Physics2D.gravity.y * wallJumpGravityMultiplier * Time.deltaTime);
            velocity += Vector2.up * (wallJumpForce * Time.deltaTime);

            //clamp in the y axis
            if (velocity.y > wallJumpMaxSpeed.y)
            {
                velocity = new Vector2(velocity.x, wallJumpMaxSpeed.y);
            }

            bool isAffectedByMagneticField = false;
            Vector2 magneticFieldForce = Vector2.zero;
            if (enableMagneticField)
            {
                List<ElectricFieldPassif.ElectricField> magneticFields = GetMagneticFields();
                isAffectedByMagneticField = magneticFields.Count > 0;
                foreach (ElectricFieldPassif.ElectricField field in magneticFields)
                {
                    float force = field.maxFieldForce * Mathf.Clamp01(field.fieldForceOverDistance.Evaluate(field.center.Distance(transform.position) / field.fieldRadius));
                    Vector2 dir = ((Vector2)transform.position - field.center).normalized;
                    magneticFieldForce += force * dir;
                }
            }

            //Horizontal movement
            if (isAffectedByMagneticField)
            {
                if (Time.time - lastTimeBeginWallJump < wallJumpMinDuration)
                {
                    if(playerInput.rawX != 0)
                    {
                        int velocitySign = (int)velocity.x.Sign();
                        float targetedSpeed = 0f;
                        if (playerInput.rawX == velocitySign)
                        {
                            targetedSpeed = velocitySign * wallJumpMaxSpeed.x;
                        }
                        else
                        {
                            float angle = velocitySign * wallJumpAngle * Mathf.Deg2Rad + Mathf.PI * 0.5f;
                            targetedSpeed = wallJumpInitSpeed * Mathf.Cos(angle);
                        }
                        velocity = new Vector2(Mathf.MoveTowards(velocity.x, targetedSpeed, wallJumpSpeedLerp * Time.deltaTime), velocity.y);
                    }
                }
                else
                {
                    if (enableInput && playerInput.rawX != 0 && ((playerInput.x >= 0f && velocity.x <= 0f) || (playerInput.x <= 0f && velocity.x >= 0f)) && !disableInstantTurn)
                    {
                        velocity = new Vector2(0f, velocity.y);
                    }

                    float targetSpeed = enableInput ? playerInput.x * wallJumpMaxSpeed.x : 0f;
                    velocity = new Vector2(Mathf.MoveTowards(velocity.x, targetSpeed, wallJumpSpeedLerp * Time.deltaTime), velocity.y);
                }
            }
            else
            {
                if (Time.time - lastTimeBeginWallJump < wallJumpMinDuration)
                {
                    int velocitySign = (int)velocity.x.Sign();
                    float targetedSpeed = 0f;
                    if(playerInput.rawX == velocitySign)
                    {
                        targetedSpeed = velocitySign * wallJumpMaxSpeed.x;
                    }
                    else
                    {
                        float angle = velocitySign * wallJumpAngle * Mathf.Deg2Rad + Mathf.PI * 0.5f;
                        targetedSpeed = wallJumpInitSpeed * Mathf.Cos(angle);
                    }
                    velocity = new Vector2(Mathf.MoveTowards(velocity.x, targetedSpeed, wallJumpSpeedLerp * Time.deltaTime), velocity.y);
                }
                else
                {
                    if (enableInput && ((playerInput.x >= 0f && velocity.x <= 0f) || (playerInput.x <= 0f && velocity.x >= 0f)) && !disableInstantTurn)
                    {
                        velocity = new Vector2(0f, velocity.y);
                    }

                    float targetSpeed = enableInput ? playerInput.x * wallJumpMaxSpeed.x : 0f;
                    velocity = new Vector2(Mathf.MoveTowards(velocity.x, targetSpeed, wallJumpSpeedLerp * Time.deltaTime), velocity.y);
                }
            }

            velocity += magneticFieldForce * Time.deltaTime;
        }

        #endregion

        #region Jump Along Wall

        void HandleJumpAlongWall()
        {
            //detect changing direction
            if ((playerInput.rawX == -1 && onRightWall) || (playerInput.rawX == 1 && onLeftWall))
            {
                WallJumpOppositeSide(onRightWall);
            }
            else // continue jumping along wall
            {
                float per100 = (Time.time - lastTimeBeginWallJumpAlongWall) / jumpAlongWallDuration;
                velocity = new Vector2(0f, wallJumpAlongSpeed * wallJumpAlongCurveSpeed.Evaluate(per100));
            }
        }

        #endregion

        #region Jump on non Grabable wall

        void HandleJumpOnNonGrabableWall()
        {
            //Use fall parameters
            //Gravity
            velocity += Vector2.up * Physics2D.gravity.y * (airGravityMultiplier * Time.deltaTime);

            bool isAffectedByMagneticField = false;
            Vector2 magneticFieldForce = Vector2.zero;
            if (enableMagneticField)
            {
                List<ElectricFieldPassif.ElectricField> magneticFields = GetMagneticFields();
                isAffectedByMagneticField = magneticFields.Count > 0;
                foreach (ElectricFieldPassif.ElectricField field in magneticFields)
                {
                    float force = field.maxFieldForce * Mathf.Clamp01(field.fieldForceOverDistance.Evaluate(field.center.Distance(transform.position) / field.fieldRadius));
                    Vector2 dir = ((Vector2)transform.position - field.center).normalized;
                    magneticFieldForce += force * dir;
                }
            }

            //Horizotal movement
            velocity = new Vector2(Mathf.MoveTowards(velocity.x, 0f, airSpeedLerp * Time.deltaTime), velocity.y);
            velocity += Time.deltaTime * magneticFieldForce;
        }

        #endregion
    }

    #region Jump

    private void Jump(in Vector2 dir)
    {
        if (slideParticle != null)
            slideParticle.transform.parent.localScale = new Vector3(flip ? -1 : 1, 1, 1);

        Vector2 newVelocity;
        if(groundColliderData != null || lastGroundColliderData != null)
        {
            MapColliderData colData = groundColliderData != null ? groundColliderData : lastGroundColliderData;
            if (colData.groundType == MapColliderData.GroundType.jumper)
            {
                Jumper jumper = colData.GetComponent<Jumper>();
                Vector2 newDir = new Vector2(Mathf.Cos(jumper.angleDir * Mathf.Deg2Rad), Mathf.Sin(jumper.angleDir * Mathf.Deg2Rad));
                newVelocity = new Vector2(velocity.x + newDir.x * jumper.impulseSpeed, newDir.y * jumper.impulseSpeed);
            }
            else
            {
                newVelocity = new Vector2(velocity.x + dir.x * jumpInitSpeed, dir.y * jumpInitSpeed) + colData.velocity;
            }
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
            if (onRightWall == onLeftWall)
            {
                print("Debug pls1 : " + onRightWall);
                LogManager.instance.AddLog("onRightWall == onLeftWall which is not possible!", onRightWall, onLeftWall, "CharacterControler::WallJump");
                return;
            }

            if(onRightWall)
            {
                right = true;
                if (!rightWallColliderData.grabableLeft)
                {
                    NonGrabableWallJump(right);
                    return;
                }
            }
            else
            {
                right = false;
                if (!leftWallColliderData.grabableRight)
                {
                    NonGrabableWallJump(right);
                    return;
                }
            }
        }
        else //ApexJump ou on est accrocher tout en haut d'un mur
        {
            right = rightFootRay;
            if (right == leftFootRay)//!bug
            {
                print("Debug pls2 right : " + right);
                LogManager.instance.AddLog("rightFootRay and leftFootRay are egal which is not possible!", leftFootRay, rightFootRay, right, "CharacterControler::WallJump");
                //Avoid bigger bugs
                isSliding = wallGrab = wallJump = false;
                isFalling = true;
                return;
            }

            if(right)
            {
                if (!rightFootRay.collider.GetComponent<MapColliderData>().grabableLeft)
                {
                    NonGrabableWallJump(right);
                    return;
                }
            }
            else
            {
                if (!leftFootRay.collider.GetComponent<MapColliderData>().grabableRight)
                {
                    NonGrabableWallJump(right);
                    return;
                }
            }
        }

        WallJump(right);
    }

    void NonGrabableWallJump(bool right)
    {
        if(enableWallJumpOnNonGrabableWall)
        {
            float angle = right ? Mathf.PI - (wallJumpAngleOnNonGrabableWall * Mathf.Deg2Rad) : wallJumpAngleOnNonGrabableWall * Mathf.Deg2Rad;
            velocity = new Vector2(wallJumpInitSpeedOnNonGrabableWall * Mathf.Cos(angle), wallJumpInitSpeedOnNonGrabableWall * Mathf.Sin(angle));
            isWallJumpingOnNonGrabableWall = wallJumpOnNonGrabableWall = true;
            lastTimeBeginWallJumpOnNonGrabableWall = Time.time;
        }
    }

    private void WallJump(bool right)
    {
        if(playerInput.rawX == 0 && wallGrab)
        {
            //first case : jump along the wall
            if(enableJumpAlongWall)
            {
                if(Time.time - lastTimeBeginWallJumpAlongWall > wallJumpAlongCooldown)
                {
                    WallJumpAlongWall(right);
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
        }
        else
        {
            WallJumpOppositeSide(right);
        }
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
        wallJump = isWallJumping = true;
        lastTimeBeginWallJump = Time.time;
        isLastWallJumpsDirIsRight = right;
    }

    void DoubleJump()
    {
        bool right;
        if(playerInput.rawX == 0)
        {
            right = isWallJumping ? isLastWallJumpsDirIsRight : !flip;
        }
        else
        {
            right = playerInput.rawX == 1;
        }
        float angle = right ? doubleJumpAngle * Mathf.Deg2Rad : Mathf.PI - (doubleJumpAngle * Mathf.Deg2Rad);
        velocity = new Vector2(doubleJumpSpeed * Mathf.Cos(angle), doubleJumpSpeed * Mathf.Sin(angle));
        hasDoubleJump = doubleJump = true;
    }

    #endregion

    #endregion

    #region Handle Fall

    private void HandleFall()
    {
        if (!isFalling)
            return;

        float fallSpeedY = fallSpeedYHandler.currentValue;
        bool isAffectedByMagneticField = false;
        Vector2 magneticFieldForce = Vector2.zero;
        if (enableMagneticField)
        {
            List<ElectricFieldPassif.ElectricField> magneticFields = GetMagneticFields();
            isAffectedByMagneticField = magneticFields.Count > 0;
            foreach (ElectricFieldPassif.ElectricField field in magneticFields)
            {
                float force = field.maxFieldForce * Mathf.Clamp01(field.fieldForceOverDistance.Evaluate(field.center.Distance(transform.position) / field.fieldRadius));
                Vector2 dir = ((Vector2)transform.position - field.center).normalized;
                magneticFieldForce += force * dir;
            }
        }

        if(isAffectedByMagneticField)
        {
            if (velocity.y > 1e-5f)
            {
                //Gravity
                float coeff = playerInput.rawY == -1 && enableInput ? fallGravityMultiplierWhenDownPressed * airGravityMultiplier : airGravityMultiplier;
                velocity += Vector2.up * (Physics2D.gravity.y * coeff * Time.deltaTime);

                //Horizontal movement 
                if (enableInput && (playerInput.x >= 0f && velocity.x <= 0f) || (playerInput.x <= 0f && velocity.x >= 0f) && !disableInstantTurn)
                {
                    if(playerInput.rawX != 0)
                    {
                        velocity = new Vector2(airInitHorizontalSpeed * airHorizontalSpeed * playerInput.x.Sign(), velocity.y);
                    }
                    else
                    {
                        velocity = new Vector2(Mathf.MoveTowards(velocity.x, 0f, airSpeedLerp * Time.deltaTime), velocity.y);
                    }
                }
                float targetSpeed = !enableInput ? 0f : playerInput.x * airHorizontalSpeed;
                velocity = new Vector2(Mathf.MoveTowards(velocity.x, targetSpeed, airSpeedLerp * Time.deltaTime), velocity.y);
                lastTimefall = Time.time;
            }
            else
            {
                //Clamp the fall speed
                float targetedSpeed;
                if (playerInput.rawY == -1 && enableInput)
                {
                    targetedSpeed = -fallSpeedY * Mathf.Max(fallClampSpeedMultiplierWhenDownPressed * Mathf.Abs(playerInput.y), 1f);
                }
                else
                {
                    targetedSpeed = -fallSpeedY;
                }

                if (velocity.y < targetedSpeed)//slow
                {
                    velocity = new Vector2(velocity.x, Mathf.MoveTowards(velocity.y, targetedSpeed, fallDecelerationSpeedLerp * Time.deltaTime));
                }
                else
                {
                    float coeff = velocity.y >= -fallSpeedY * maxBeginFallSpeed ? fallGravityMultiplier * beginFallExtraGravity : fallGravityMultiplier;
                    coeff = enableInput && playerInput.rawY < 0 ? coeff * fallGravityMultiplierWhenDownPressed : coeff;
                    velocity = new Vector2(velocity.x, Mathf.MoveTowards(velocity.y, targetedSpeed, -Physics2D.gravity.y * coeff * Time.deltaTime));
                }

                //Horizontal movement
                if (isQuittingConvoyerBelt)
                {
                    float speed = speedWhenQuittingConvoyerBelt * (isQuittingConvoyerBeltRight ? 1f : -1f);
                    velocity = new Vector2(speed, velocity.y);
                }
                else if (playerInput.rawX != 0 || Time.time - lastTimeQuitGround > inertiaDurationWhenQuittingGround)
                {
                    if (enableInput && ((playerInput.x >= 0f && velocity.x <= 0f) || (playerInput.x <= 0f && velocity.x >= 0f)) && !disableInstantTurn)
                        velocity = new Vector2(Mathf.MoveTowards(velocity.x, 0f, fallSpeedLerp * Time.deltaTime), velocity.y);

                    float maxSpeedX = Mathf.Lerp(beginFallSpeedX, endFallSpeedX, (Time.time - lastTimefall) / beginFallDuration);
                    float targetSpeed = !enableInput ? 0f : playerInput.x * maxSpeedX;
                    velocity = new Vector2(Mathf.MoveTowards(velocity.x, targetSpeed, fallSpeedLerp * Time.deltaTime), velocity.y);
                }
            }
        }
        else
        {
            if (velocity.y > 1e-5f)
            {
                //Gravity
                float coeff = playerInput.rawY == -1 && enableInput ? fallGravityMultiplierWhenDownPressed * airGravityMultiplier : airGravityMultiplier;
                velocity += Vector2.up * (Physics2D.gravity.y * coeff * Time.deltaTime);

                //Horizontal movement 
                if (enableInput && (playerInput.x >= 0f && velocity.x <= 0f) || (playerInput.x <= 0f && velocity.x >= 0f) && !disableInstantTurn)
                    velocity = new Vector2(airInitHorizontalSpeed * airHorizontalSpeed * playerInput.x.Sign(), velocity.y);
                if (enableInput && Mathf.Abs(velocity.x) < airInitHorizontalSpeed * airHorizontalSpeed * 0.95f && Mathf.Abs(playerInput.x) > 0.01f && !disableInstantTurn)
                {
                    velocity = new Vector2(airInitHorizontalSpeed * airHorizontalSpeed * playerInput.x.Sign(), velocity.y);
                }
                else
                {
                    float targetSpeed = !enableInput ? 0f : playerInput.x * airHorizontalSpeed;
                    velocity = new Vector2(Mathf.MoveTowards(velocity.x, targetSpeed, airSpeedLerp * Time.deltaTime), velocity.y);
                }
                lastTimefall = Time.time;
            }
            else
            {
                //Clamp the fall speed
                float targetedSpeed;
                if (playerInput.rawY == -1 && enableInput)
                {
                    targetedSpeed = -fallSpeedY * Mathf.Max(fallClampSpeedMultiplierWhenDownPressed * Mathf.Abs(playerInput.y), 1f);
                }
                else
                {
                    targetedSpeed = -fallSpeedY;
                }

                if (velocity.y < targetedSpeed)//slow
                {
                    velocity = new Vector2(velocity.x, Mathf.MoveTowards(velocity.y, targetedSpeed, fallDecelerationSpeedLerp * Time.deltaTime));
                }
                else
                {
                    float coeff = velocity.y >= -fallSpeedY * maxBeginFallSpeed ? fallGravityMultiplier * beginFallExtraGravity : fallGravityMultiplier;
                    coeff = enableInput && playerInput.rawY < 0 ? coeff * fallGravityMultiplierWhenDownPressed : coeff;
                    velocity = new Vector2(velocity.x, Mathf.MoveTowards(velocity.y, targetedSpeed, -Physics2D.gravity.y * coeff * Time.deltaTime));
                }

                //Horizontal movement
                if (isQuittingConvoyerBelt)
                {
                    float speed = speedWhenQuittingConvoyerBelt * (isQuittingConvoyerBeltRight ? 1f : -1f);
                    velocity = new Vector2(speed, velocity.y);
                }
                else if (playerInput.rawX != 0 || Time.time - lastTimeQuitGround > inertiaDurationWhenQuittingGround)
                {
                    float maxSpeedX = Mathf.Lerp(beginFallSpeedX, endFallSpeedX, (Time.time - lastTimefall) / beginFallDuration);
                    if (enableInput && ((playerInput.x >= 0f && velocity.x <= 0f) || (playerInput.x <= 0f && velocity.x >= 0f)) && !disableInstantTurn)
                        velocity = new Vector2(fallInitHorizontalSpeed * maxSpeedX * playerInput.x.Sign(), velocity.y);
                    if (enableInput && Mathf.Abs(velocity.x) < fallInitHorizontalSpeed * maxSpeedX * 0.95f && playerInput.rawX != 0 && !disableInstantTurn)
                    {
                        velocity = new Vector2(fallInitHorizontalSpeed * maxSpeedX * playerInput.x.Sign(), velocity.y);
                    }
                    else
                    {
                        float targetSpeed = !enableInput ? 0f : playerInput.x * maxSpeedX;
                        velocity = new Vector2(Mathf.MoveTowards(velocity.x, targetSpeed, fallSpeedLerp * Time.deltaTime), velocity.y);
                    }
                }
            }
        }

        velocity += Time.deltaTime * magneticFieldForce;
    }

    #endregion

    #region Handle Dash

    private void HandleDash()
    {
        //Dashing
        if (doDash && !isDashing)
        {
            if (!isDashDisabled && !hasDashed && Time.time - lastTimeDashFinish >= dashCooldown)
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

        float dashSpeed = dashSpeedHandler.currentValue;
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
            float fallSpeedY = fallSpeedYHandler.currentValue;
            velocity = new Vector2(velocity.x, Mathf.MoveTowards(velocity.y, -fallSpeedY, -Physics2D.gravity.y * bumpGravityScale * Time.deltaTime));
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

    private List<ElectricFieldPassif.ElectricField> GetMagneticFields()
    {
        List<ElectricFieldPassif.ElectricField> fields = ElectricFieldPassif.electricFields.Clone();
        for (int i = fields.Count - 1; i >= 0; i--)
        {
            ElectricFieldPassif.ElectricField electricField = fields[i];
            if ((electricField.playerId < 0 || electricField.playerId == (int)playerCommon.id) && 
                electricField.center.SqrDistance(transform.position) > electricField.fieldRadius * electricField.fieldRadius)
            {
                fields.RemoveAt(i);
            }
        }

        return fields;
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

    #region Gizmos/OnValidate/Pause

    private IEnumerator PauseCorout()
    {
        Vector2 speed = velocity;

        Freeze();

        while(!enableBehaviour)
        {
            yield return null;
            lastTimeApexJump += Time.deltaTime;
            lastTimeBeginJump += Time.deltaTime;
            lastTimeBeginWallJump += Time.deltaTime;
            lastTimeBeginWallJumpAlongWall += Time.deltaTime;
            lastTimeBeginWallJumpOnNonGrabableWall += Time.deltaTime;
            lastTimeBump += Time.deltaTime;
            lastTimeDashBegin += Time.deltaTime;
            lastTimeDashCommand += Time.deltaTime;
            lastTimeDashFinish += Time.deltaTime;
            lastTimeJumpCommand += Time.deltaTime;
            lastTimeLeavePlateform += Time.deltaTime;
            lastTimeQuitGround += Time.deltaTime;
            lastTimeQuittingConvoyerBelt += Time.deltaTime;
        }

        UnFreeze();

        velocity = speed;
    }

    private void OnPauseEnable()
    {
        StartCoroutine(PauseCorout());
    }

    private void OnPauseDisable()
    {
        enableBehaviour = true;
    }

    private void OnDestroy()
    {
        PauseManager.instance.callBackOnPauseEnable -= OnPauseEnable;
        PauseManager.instance.callBackOnPauseDisable -= OnPauseDisable;
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
        fallSpeedY = Mathf.Max(0f, fallSpeedY);
        beginFallSpeedX = Mathf.Max(0f, beginFallSpeedX);
        endFallSpeedX = Mathf.Max(0f, endFallSpeedX);
        beginFallDuration = Mathf.Max(0f, beginFallDuration);
        airHorizontalSpeed = Mathf.Max(0f, airHorizontalSpeed);
        jumpMaxSpeed = new Vector2(Mathf.Max(0f, jumpMaxSpeed.x), Mathf.Max(0f, jumpMaxSpeed.y));
        minDelayToDoubleJumpAfterNormalJump = Mathf.Max(0f, minDelayToDoubleJumpAfterNormalJump);
        minDelayToDoubleJumpAfterJumpAlongWall = Mathf.Max(0f, minDelayToDoubleJumpAfterJumpAlongWall);
        minDelayToDoubleJumpAfterWallJump = Mathf.Max(0f, minDelayToDoubleJumpAfterWallJump);
        doubleJumpSpeed = Mathf.Max(doubleJumpSpeed, 0f);
        wallJumpMaxDuration = Mathf.Max(wallJumpMaxDuration, 0f);
        wallJumpMinDuration = Mathf.Clamp(wallJumpMinDuration, 0f, wallJumpMaxDuration);
        wallJumpSpeedLerp = Mathf.Max(wallJumpSpeedLerp, 0f);
        wallJumpOnNonGrabableWallDuration = Mathf.Max(0f, wallJumpOnNonGrabableWallDuration);
        apexJumpSpeed = new Vector2(Mathf.Max(0f, apexJumpSpeed.x), apexJumpSpeed.y);
        apexJumpSpeed2 = new Vector2(Mathf.Max(0f, apexJumpSpeed2.x), apexJumpSpeed2.y);
        slideSpeed = Mathf.Max(0f, slideSpeed);
        apexJumpSpeedLerp = Mathf.Max(0f, apexJumpSpeedLerp);
        grabSpeedLerp = Mathf.Max(0f, grabSpeedLerp);
        speedLerp = Mathf.Max(0f, speedLerp);
        grabRayLength = Mathf.Max(0f, grabRayLength);
        airSpeedLerp = Mathf.Max(0f, airSpeedLerp);
        jumpInitSpeed = Mathf.Max(0f, jumpInitSpeed);
        jumpMaxDuration = Mathf.Max(jumpMaxDuration, 0f);
        jumpMinDuration = Mathf.Max(jumpMinDuration, 0f);
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
