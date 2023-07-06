using UnityEngine;

public class CustomPlayerInput : MonoBehaviour
{
    private PlayerCommon playerCommon;
    private float newX, newY;//cache
    private Vector2 oldStickPos = Vector2.zero;

    [HideInInspector] public float x, y;
    [HideInInspector] public int rawX, rawY;
    [HideInInspector] public bool upPressed, rightPressed, downPressed, leftPressed, dashPressed, grabPressed, jumpPressed, attackWeakPressed, attackStrongPressed, shieldPressed;
    [HideInInspector] public bool upPressedDown, rightPressedDown, downPressedDown, leftPressedDown, dashPressedDown, grabPressedDown, jumpPressedDown, attackWeakPressedDown, attackStrongPressedDown, shieldPressedDown;
    [HideInInspector] public bool upPressedUp, rightPressedUp, downPressedUp, leftPressedUp, dashPressedUp, grabPressedUp, jumpPressedUp, attackWeakPressedUp, attackStrongPressedUp, shieldPressedUp;

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
            newX = leftPressed ? (newX > 0.01f ? 0f : -1f) : newX;
            upPressed = InputManager.GetKey("MoveUp", playerCommon.playerIndex);
            upPressedDown = InputManager.GetKeyDown("MoveUp", playerCommon.playerIndex);
            upPressedUp = InputManager.GetKeyUp("MoveUp", playerCommon.playerIndex);
            newY = upPressed ? 1f : 0f;
            downPressed = InputManager.GetKey("MoveDown", playerCommon.playerIndex);
            downPressedDown = InputManager.GetKeyDown("MoveDown", playerCommon.playerIndex);
            downPressedUp = InputManager.GetKeyUp("MoveDown", playerCommon.playerIndex);
            newY = downPressed ? (newY > 0.01f ? 0f : -1f) : newY;
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

        if(controllerType == ControllerType.GamepadAll || controllerType == ControllerType.Gamepad1 || controllerType == ControllerType.Gamepad2 ||
            controllerType == ControllerType.Gamepad3 || controllerType == ControllerType.Gamepad4)
        {
            Vector2 stickPos = InputManager.GetGamepadStickPosition(controllerType, GamepadStick.left);
            if (stickPos.sqrMagnitude > 1f)
                stickPos = stickPos.normalized;

            upPressed = stickPos.y > 0.01f;
            upPressedDown = oldStickPos.y <= 0.01f && stickPos.y > 0.01f;
            if(upPressedDown)
            {
                upPressedDown = true;
            }
            upPressedUp = oldStickPos.y > 0.01f && stickPos.y <= 0.01f;

            downPressed = stickPos.y < -0.01f;
            downPressedDown = oldStickPos.y >= -0.01f && stickPos.y < -0.01f;
            downPressedUp = oldStickPos.y < -0.01f && stickPos.y >= -0.01f;

            rightPressed = stickPos.x > 0.01f;
            rightPressedDown = oldStickPos.x <= 0.01f && stickPos.x > 0.01f;
            rightPressedUp = oldStickPos.x > 0.01f && stickPos.x <= 0.01f;

            leftPressed = stickPos.x < -0.01f;
            leftPressedDown = oldStickPos.x >= -0.01f && stickPos.x < -0.01f;
            leftPressedUp = oldStickPos.x < -0.01f && stickPos.x >= -0.01f;

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
            newX = leftPressed ? (newX > 0.01f ? 0f : -1f) : newX;
            upPressed = InputManager.GetKey("MoveUp", playerCommon.playerIndex);
            upPressedDown = InputManager.GetKeyDown("MoveUp", playerCommon.playerIndex);
            upPressedUp = InputManager.GetKeyUp("MoveUp", playerCommon.playerIndex);
            newY = upPressed ? 1f : 0f;
            downPressed = InputManager.GetKey("MoveDown", playerCommon.playerIndex);
            downPressedDown = InputManager.GetKeyDown("MoveDown", playerCommon.playerIndex);
            downPressedUp = InputManager.GetKeyUp("MoveDown", playerCommon.playerIndex);
            newY = downPressed ? (newY > 0.01f ? 0f : -1f) : newY;
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
            if (Mathf.Abs(newX) <= 0.01f || Mathf.Abs(newY) <= 0.01f)
            {
                upPressed = stickPos.y > 0.01f;
                upPressedDown = oldStickPos.y <= 0.01f && stickPos.y > 0.01f;
                upPressedUp = oldStickPos.y > 0.01f && stickPos.y <= 0.01f;

                downPressed = stickPos.y < -0.01f;
                downPressedDown = oldStickPos.y >= -0.01f && stickPos.y < -0.01f;
                downPressedUp = oldStickPos.y < -0.01f && stickPos.y >= -0.01f;

                rightPressed = stickPos.x > 0.01f;
                rightPressedDown = oldStickPos.x <= 0.01f && stickPos.x > 0.01f;
                rightPressedUp = oldStickPos.x > 0.01f && stickPos.x <= 0.01f;

                leftPressed = stickPos.x < -0.01f;
                leftPressedDown = oldStickPos.x >= -0.01f && stickPos.x < -0.01f;
                leftPressedUp = oldStickPos.x < -0.01f && stickPos.x >= -0.01f;

                x = stickPos.x;
                y = stickPos.y;
            }
            oldStickPos = stickPos;
        }

        rawX = (x > 0.01f) ? 1 : (x < -0.01f ? -1 : 0);
        rawY = (y > 0.01f) ? 1 : (y < -0.01f ? -1 : 0);

        //Pressed
        dashPressed = InputManager.GetKey("Dash", playerCommon.playerIndex);
        grabPressed = InputManager.GetKey("Grab", playerCommon.playerIndex);
        jumpPressed = InputManager.GetKey("Jump", playerCommon.playerIndex);
        attackWeakPressed = InputManager.GetKey("AttackWeak", playerCommon.playerIndex);
        attackStrongPressed = InputManager.GetKey("AttackStrong", playerCommon.playerIndex);
        shieldPressed = InputManager.GetKey("Shield", playerCommon.playerIndex);

        //Down
        dashPressedDown = InputManager.GetKeyDown("Dash", playerCommon.playerIndex);
        grabPressedDown = InputManager.GetKeyDown("Grab", playerCommon.playerIndex);
        jumpPressedDown = InputManager.GetKeyDown("Jump", playerCommon.playerIndex);
        attackWeakPressedDown = InputManager.GetKeyDown("AttackWeak", playerCommon.playerIndex);
        attackStrongPressedDown = InputManager.GetKeyDown("AttackStrong", playerCommon.playerIndex);
        shieldPressedDown = InputManager.GetKeyDown("Shield", playerCommon.playerIndex);

        //Up
        dashPressedUp = InputManager.GetKeyUp("Dash", playerCommon.playerIndex);
        grabPressedUp = InputManager.GetKeyUp("Grab", playerCommon.playerIndex);
        jumpPressedUp = InputManager.GetKeyUp("Jump", playerCommon.playerIndex);
        attackWeakPressedUp = InputManager.GetKeyUp("AttackWeak", playerCommon.playerIndex);
        attackStrongPressedUp = InputManager.GetKeyUp("AttackStrong", playerCommon.playerIndex);
        shieldPressedUp = InputManager.GetKeyUp("Shield", playerCommon.playerIndex);
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
        clone.shieldPressed = shieldPressed; clone.shieldPressedDown = shieldPressedDown; clone.shieldPressedUp = shieldPressedUp;

        clone.movementLerpForKeyboard = movementLerpForKeyboard;
        clone.playerCommon = playerCommon;
        clone.controllerType = controllerType;

        return clone;
    }
}
