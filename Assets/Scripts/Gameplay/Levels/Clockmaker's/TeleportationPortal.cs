using UnityEngine;

public class TeleportationPortal : MonoBehaviour
{
    private enum TeleporterState
    {
        Waiting, Entering, Teleporting, Reloading
    }

    private new Transform transform;
    private LayerMask charMask;
    private GameObject currentChar;
    private float lastTimer = -10f, lastTimerExitCharacter = -10f;
    private TeleporterState state;
    private bool isExitingCharacter => exitingCharacter != null;
    private GameObject exitingCharacter;
    private Animator animator;
    private int enableAnim, disableAnim;

    [SerializeField] private TeleportationPortal otherPortal;
    [SerializeField] private float enteringDuration;
    [SerializeField] private float teleportationDuration;
    [SerializeField] private float exitDuration;
    [SerializeField] private float teleportationDelay;
    [SerializeField] private Vector2 hitboxSize, hitboxOffset;
    [SerializeField] private Vector2 teleportationOffset = Vector2.zero;

    private void Awake()
    {
        this.transform = base.transform;
        animator = GetComponent<Animator>();
        enableAnim = Animator.StringToHash("enable");
        disableAnim = Animator.StringToHash("disable");
    }

    private void Start()
    {
        charMask = LayerMask.GetMask("Char");
    }

    private void Update()
    {
        switch (state)
        {
            case TeleporterState.Waiting:
                HandleWaiting();
                break;
            case TeleporterState.Entering:
                HandleEntering();
                break;
            case TeleporterState.Teleporting:
                HandleTeleportation();
                break;
            case TeleporterState.Reloading:
                HandleReloading();
                break;
            default:
                break;
        }

        if(isExitingCharacter)
        {
            HandleExit();
        }
    }

    #region Wait

    private void HandleWaiting()
    {
        if (PauseManager.instance.isPauseEnable)
            return;

        if (otherPortal.state == TeleporterState.Entering || otherPortal.state == TeleporterState.Teleporting)
            return;

        Collider2D[] cols = PhysicsToric.OverlapBoxAll((Vector2)transform.position + hitboxOffset, hitboxSize, 0f, charMask);
        for (int i = 0; i < cols.Length; i++)
        {
            if (cols[i].CompareTag("Char"))
            {
                GameObject player = cols[i].GetComponent<ToricObject>().original;
                CharacterInputs playerInput = player.GetComponent<CharacterInputs>();
                if (playerInput.interactPressedDown)
                {
                    SetupEntering(player);
                    break;
                }
            }
        }
    }

    private void SetupEntering(GameObject player)
    {
        currentChar = player;
        CharacterController characterController = player.GetComponent<CharacterController>();
        characterController.Freeze();
        FightController fightController = player.GetComponent<FightController>();
        fightController.enableBehavior = false;
        animator.CrossFade(disableAnim, 0f, 0);
        lastTimer = Time.time;
        state = TeleporterState.Entering;
    }

    #endregion

    #region Entering

    private void HandleEntering()
    {
        if(PauseManager.instance.isPauseEnable)
        {
            lastTimer += Time.deltaTime;
            return;
        }

        if(Time.time - lastTimer >= enteringDuration)
        {
            FightController fightController = currentChar.GetComponent<FightController>();
            fightController.EnableInvicibility();
            SpriteRenderer spriteRenderer = currentChar.GetComponent<SpriteRenderer>();
            spriteRenderer.enabled = false;
            currentChar.GetComponent<BoxCollider2D>().enabled = false;
            lastTimer = Time.time;
            state = TeleporterState.Teleporting;
        }
    }

    #endregion

    #region Teleporting

    private void HandleTeleportation()
    {
        if (PauseManager.instance.isPauseEnable)
        {
            lastTimer += Time.deltaTime;
            return;
        }

        if(Time.time - lastTimer >= teleportationDuration)
        {
            otherPortal.TeleportChar(currentChar);
            currentChar = null;
            lastTimer = Time.time;
            state = TeleporterState.Reloading;
        }
    }

    private void TeleportChar(GameObject character)
    {
        exitingCharacter = character;
        lastTimerExitCharacter = Time.time;
        CharacterController characterController = character.GetComponent<CharacterController>();
        characterController.Teleport((Vector2)transform.position + teleportationOffset);
        FightController fightController = character.GetComponent<FightController>();
        fightController.enableBehavior = true;
        fightController.DisableInvicibility();
        SpriteRenderer spriteRenderer = character.GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = true;
        character.GetComponent<BoxCollider2D>().enabled = true;
        lastTimer = -10f;
    }

    #endregion

    #region Reload

    private void HandleReloading()
    {
        if (PauseManager.instance.isPauseEnable)
        {
            lastTimer += Time.deltaTime;
            return;
        }

        if (Time.time - lastTimer >= teleportationDelay)
        {
            lastTimer = -10f;
            animator.CrossFade(enableAnim, 0f, 0);
            state = TeleporterState.Waiting;
        }
    }

    #endregion

    #region Exit

    private void HandleExit()
    {
        if (PauseManager.instance.isPauseEnable)
        {
            lastTimer += Time.deltaTime;
            return;
        }

        if(Time.time - lastTimerExitCharacter >= exitDuration)
        {
            ReleaseExitCharacter();
        }
    }

    private void ReleaseExitCharacter()
    {
        CharacterController characterController = exitingCharacter.GetComponent<CharacterController>();
        characterController.UnFreeze();
        lastTimerExitCharacter = -10f;
        exitingCharacter = null;
    }

    #endregion

    #region Gizmos/OnValidate

#if UNITY_EDITOR

    private void OnValidate()
    {
        this.transform = base.transform;
        enteringDuration = Mathf.Max(0f, enteringDuration);
        teleportationDuration = Mathf.Max(0f, teleportationDuration);
        teleportationDelay = Mathf.Max(0f, teleportationDelay);
        hitboxSize = new Vector2(Mathf.Max(0f, hitboxSize.x), Mathf.Max(0f, hitboxSize.y));
    }

    private void OnDrawGizmosSelected()
    {
        Collision2D.Hitbox.GizmosDraw((Vector2)transform.position + hitboxOffset, hitboxSize, Color.green, true);
        Collision2D.Circle.GizmosDraw((Vector2)transform.position + teleportationOffset, 0.1f, Color.green, true);
    }

#endif

    #endregion
}
