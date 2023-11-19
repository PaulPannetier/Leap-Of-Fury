using UnityEngine;

public class CustomPlayerInput : MonoBehaviour
{
    private PlayerCommon playerCommon;
    private float newX, newY;//cache
    private Vector2 oldStickPos = Vector2.zero;

    [HideInInspector] public float x, y;
    [HideInInspector] public int rawX, rawY;
    [HideInInspector] public bool upPressed, rightPressed, downPressed, leftPressed, dashPressed, grabPressed, jumpPressed, attackWeakPressed, attackStrongPressed, interactPressed;
    [HideInInspector] public bool upPressedDown, rightPressedDown, downPressedDown, leftPressedDown, dashPressedDown, grabPressedDown, jumpPressedDown, attackWeakPressedDown, attackStrongPressedDown, interactPressedDown;
    [HideInInspector] public bool upPressedUp, rightPressedUp, downPressedUp, leftPressedUp, dashPressedUp, grabPressedUp, jumpPressedUp, attackWeakPressedUp, attackStrongPressedUp, interactPressedUp;

    public ControllerType controllerType;
    [SerializeField] private bool useMovementLerpForKeyboard = true;
    [SerializeField] private float movementLerpForKeyboard = 10f;

    private void Awake()
    {
        playerCommon = GetComponent<PlayerCommon>();
    }

    private void Start()
    {
        x = y = 0f;
        rawX = rawY = 0;
    }

    private void Update()
    {
        //Axis
        if(controllerType == ControllerType.Keyboard)
        {
            rightPressed = InputManager.GetKey("MoveRight", playerCommon.playerIndex);
            rightPressedDown = InputManager.GetKeyDown("MoveRight", playerCommon.playerIndex);
            rightPressedUp = InputManager.GetKeyUp("MoveRight", playerCommon.playerIndex);
            newX = rightPressed ? 1f : 0f;
            leftPressed = InputManager.GetKey("MoveLeft", playerCommon.playerIndex);
            leftPressedDown = InputManager.GetKeyDown("MoveLeft", playerCommon.playerIndex);
            leftPressedUp = InputManager.GetKeyUp("MoveLeft", playerCommon.playerIndex);
            newX = leftPressed ? (newX > 1e-6f ? 0f : -1f) : newX;
            upPressed = InputManager.GetKey("MoveUp", playerCommon.playerIndex);
            upPressedDown = InputManager.GetKeyDown("MoveUp", playerCommon.playerIndex);
            upPressedUp = InputManager.GetKeyUp("MoveUp", playerCommon.playerIndex);
            newY = upPressed ? 1f : 0f;
            downPressed = InputManager.GetKey("MoveDown", playerCommon.playerIndex);
            downPressedDown = InputManager.GetKeyDown("MoveDown", playerCommon.playerIndex);
            downPressedUp = InputManager.GetKeyUp("MoveDown", playerCommon.playerIndex);
            newY = downPressed ? (newY > 1e-6f ? 0f : -1f) : newY;
            if(useMovementLerpForKeyboard)
            {
                x = Mathf.MoveTowards(x, newX, movementLerpForKeyboard * Time.deltaTime);
                y = Mathf.MoveTowards(y, newY, movementLerpForKeyboard * Time.deltaTime);
            }
            else
            {
                x = newX;
                y = newY;
            }
        }

        if(InputManager.IsAGamepadController(controllerType))
        {
            Vector2 stickPos = InputManager.GetGamepadStickPosition(controllerType, GamepadStick.left);
            if (stickPos.sqrMagnitude > 1f)
                stickPos = stickPos.normalized;

            upPressed = stickPos.y > 1e-6f;
            upPressedDown = oldStickPos.y <= 1e-6f && stickPos.y > 1e-6f;
            if(upPressedDown)
            {
                upPressedDown = true;
            }
            upPressedUp = oldStickPos.y > 1e-6f && stickPos.y <= 1e-6f;

            downPressed = stickPos.y < -1e-6f;
            downPressedDown = oldStickPos.y >= -1e-6f && stickPos.y < -1e-6f;
            downPressedUp = oldStickPos.y < -1e-6f && stickPos.y >= -1e-6f;

            rightPressed = stickPos.x > 1e-6f;
            rightPressedDown = oldStickPos.x <= 1e-6f && stickPos.x > 1e-6f;
            rightPressedUp = oldStickPos.x > 1e-6f && stickPos.x <= 1e-6f;

            leftPressed = stickPos.x < -1e-6f;
            leftPressedDown = oldStickPos.x >= -1e-6f && stickPos.x < -1e-6f;
            leftPressedUp = oldStickPos.x < -1e-6f && stickPos.x >= -1e-6f;

            x = stickPos.x;
            y = stickPos.y;

            oldStickPos = stickPos;
        }

        if(controllerType == ControllerType.All)
        {
            rightPressed = InputManager.GetKey("MoveRight", playerCommon.playerIndex);
            rightPressedDown = InputManager.GetKeyDown("MoveRight", playerCommon.playerIndex);
            rightPressedUp = InputManager.GetKeyUp("MoveRight", playerCommon.playerIndex);
            newX = rightPressed ? 1f : 0f;
            leftPressed = InputManager.GetKey("MoveLeft", playerCommon.playerIndex);
            leftPressedDown = InputManager.GetKeyDown("MoveLeft", playerCommon.playerIndex);
            leftPressedUp = InputManager.GetKeyUp("MoveLeft", playerCommon.playerIndex);
            newX = leftPressed ? (newX > 1e-6f ? 0f : -1f) : newX;
            upPressed = InputManager.GetKey("MoveUp", playerCommon.playerIndex);
            upPressedDown = InputManager.GetKeyDown("MoveUp", playerCommon.playerIndex);
            upPressedUp = InputManager.GetKeyUp("MoveUp", playerCommon.playerIndex);
            newY = upPressed ? 1f : 0f;
            downPressed = InputManager.GetKey("MoveDown", playerCommon.playerIndex);
            downPressedDown = InputManager.GetKeyDown("MoveDown", playerCommon.playerIndex);
            downPressedUp = InputManager.GetKeyUp("MoveDown", playerCommon.playerIndex);
            newY = downPressed ? (newY > 1e-6f ? 0f : -1f) : newY;
            if (useMovementLerpForKeyboard)
            {
                x = Mathf.MoveTowards(x, newX, movementLerpForKeyboard * Time.deltaTime);
                y = Mathf.MoveTowards(y, newY, movementLerpForKeyboard * Time.deltaTime);
            }
            else
            {
                x = newX;
                y = newY;
            }

            Vector2 stickPos = InputManager.GetGamepadStickPosition(controllerType, GamepadStick.left);
            if (stickPos.sqrMagnitude > 1f)
                stickPos = stickPos.normalized;
            if (Mathf.Abs(newX) <= 1e-6f || Mathf.Abs(newY) <= 1e-6f)
            {
                upPressed = stickPos.y > 1e-6f;
                upPressedDown = oldStickPos.y <= 1e-6f && stickPos.y > 1e-6f;
                upPressedUp = oldStickPos.y > 1e-6f && stickPos.y <= 1e-6f;

                downPressed = stickPos.y < -1e-6f;
                downPressedDown = oldStickPos.y >= -1e-6f && stickPos.y < -1e-6f;
                downPressedUp = oldStickPos.y < -1e-6f && stickPos.y >= -1e-6f;

                rightPressed = stickPos.x > 1e-6f;
                rightPressedDown = oldStickPos.x <= 1e-6f && stickPos.x > 1e-6f;
                rightPressedUp = oldStickPos.x > 1e-6f && stickPos.x <= 1e-6f;

                leftPressed = stickPos.x < -1e-6f;
                leftPressedDown = oldStickPos.x >= -1e-6f && stickPos.x < -1e-6f;
                leftPressedUp = oldStickPos.x < -1e-6f && stickPos.x >= -1e-6f;

                x = stickPos.x;
                y = stickPos.y;
            }
            oldStickPos = stickPos;
        }

        rawX = (x > 1e-6f) ? 1 : (x < -1e-6f ? -1 : 0);
        rawY = (y > 1e-6f) ? 1 : (y < -1e-6f ? -1 : 0);

        //Pressed
        dashPressed = InputManager.GetKey("Dash", playerCommon.playerIndex);
        grabPressed = InputManager.GetKey("Grab", playerCommon.playerIndex);
        jumpPressed = InputManager.GetKey("Jump", playerCommon.playerIndex);
        attackWeakPressed = InputManager.GetKey("AttackWeak", playerCommon.playerIndex);
        attackStrongPressed = InputManager.GetKey("AttackStrong", playerCommon.playerIndex);
        bool oldInteractPressed = interactPressed;
        interactPressed = rawY == 1 && Mathf.Abs(x) < 0.25f && !dashPressed && !grabPressed && !jumpPressed && !attackWeakPressed && !attackStrongPressed;

        //Down
        dashPressedDown = InputManager.GetKeyDown("Dash", playerCommon.playerIndex);
        grabPressedDown = InputManager.GetKeyDown("Grab", playerCommon.playerIndex);
        jumpPressedDown = InputManager.GetKeyDown("Jump", playerCommon.playerIndex);
        attackWeakPressedDown = InputManager.GetKeyDown("AttackWeak", playerCommon.playerIndex);
        attackStrongPressedDown = InputManager.GetKeyDown("AttackStrong", playerCommon.playerIndex);
        interactPressedDown = InputManager.GetKeyDown("Interact", playerCommon.playerIndex);
        interactPressedDown = !oldInteractPressed && interactPressed;

        //Up
        dashPressedUp = InputManager.GetKeyUp("Dash", playerCommon.playerIndex);
        grabPressedUp = InputManager.GetKeyUp("Grab", playerCommon.playerIndex);
        jumpPressedUp = InputManager.GetKeyUp("Jump", playerCommon.playerIndex);
        attackWeakPressedUp = InputManager.GetKeyUp("AttackWeak", playerCommon.playerIndex);
        attackStrongPressedUp = InputManager.GetKeyUp("AttackStrong", playerCommon.playerIndex);
        interactPressedUp = oldInteractPressed && !interactPressed;
    }

    public CustomPlayerInput Clone()
    {
        CustomPlayerInput clone = new CustomPlayerInput();
        clone.upPressed = upPressed; clone.upPressedDown = upPressedDown; clone.upPressedUp = upPressedUp;
        clone.downPressed = downPressed; clone.downPressedDown = downPressedDown; clone.downPressedUp = downPressedUp;
        clone.rightPressed = rightPressed; clone.rightPressedDown = rightPressedDown; clone.rightPressedUp = rightPressedUp;
        clone.leftPressed = leftPressed; clone.leftPressedDown = leftPressedDown; clone.leftPressedUp = leftPressedUp;
        clone.x = x; clone.rawX = rawX; clone.rawY = rawY; clone.newX = newX; clone.newY = newY;
        clone.dashPressed = dashPressed; clone.dashPressedDown = dashPressedDown; clone.dashPressedUp = dashPressedUp;
        clone.grabPressed = grabPressed; clone.grabPressedDown = grabPressedDown; clone.grabPressedUp = grabPressedUp;
        clone.jumpPressed = jumpPressed; clone.jumpPressedDown = jumpPressedDown; clone.jumpPressedUp = jumpPressedUp;
        clone.attackWeakPressed = attackWeakPressed; clone.attackWeakPressedDown = attackWeakPressedDown; clone.attackWeakPressedUp = attackWeakPressedUp;
        clone.attackStrongPressed = attackStrongPressed; clone.attackStrongPressedDown = attackStrongPressedDown; clone.attackStrongPressedUp = attackStrongPressedUp;
        clone.interactPressed = interactPressed; clone.interactPressedDown = interactPressedDown; clone.interactPressedUp = interactPressedUp;

        clone.movementLerpForKeyboard = movementLerpForKeyboard;
        clone.playerCommon = playerCommon;
        clone.controllerType = controllerType;

        return clone;
    }
}
