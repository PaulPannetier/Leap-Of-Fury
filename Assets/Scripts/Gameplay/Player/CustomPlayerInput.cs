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
            rightPressed = CustomInput.GetKey("MoveRight", playerCommon.playerIndex);
            rightPressedDown = CustomInput.GetKeyDown("MoveRight", playerCommon.playerIndex);
            rightPressedUp = CustomInput.GetKeyUp("MoveRight", playerCommon.playerIndex);
            newX = rightPressed ? 1f : 0f;
            leftPressed = CustomInput.GetKey("MoveLeft", playerCommon.playerIndex);
            leftPressedDown = CustomInput.GetKeyDown("MoveLeft", playerCommon.playerIndex);
            leftPressedUp = CustomInput.GetKeyUp("MoveLeft", playerCommon.playerIndex);
            newX = leftPressed ? (newX > 0.01f ? 0f : -1f) : newX;
            upPressed = CustomInput.GetKey("MoveUp", playerCommon.playerIndex);
            upPressedDown = CustomInput.GetKeyDown("MoveUp", playerCommon.playerIndex);
            upPressedUp = CustomInput.GetKeyUp("MoveUp", playerCommon.playerIndex);
            newY = upPressed ? 1f : 0f;
            downPressed = CustomInput.GetKey("MoveDown", playerCommon.playerIndex);
            downPressedDown = CustomInput.GetKeyDown("MoveDown", playerCommon.playerIndex);
            downPressedUp = CustomInput.GetKeyUp("MoveDown", playerCommon.playerIndex);
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
            Vector2 stickPos = CustomInput.GetGamepadStickPosition(controllerType, GamepadStick.left);
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
            rightPressed = CustomInput.GetKey("MoveRight", playerCommon.playerIndex);
            rightPressedDown = CustomInput.GetKeyDown("MoveRight", playerCommon.playerIndex);
            rightPressedUp = CustomInput.GetKeyUp("MoveRight", playerCommon.playerIndex);
            newX = rightPressed ? 1f : 0f;
            leftPressed = CustomInput.GetKey("MoveLeft", playerCommon.playerIndex);
            leftPressedDown = CustomInput.GetKeyDown("MoveLeft", playerCommon.playerIndex);
            leftPressedUp = CustomInput.GetKeyUp("MoveLeft", playerCommon.playerIndex);
            newX = leftPressed ? (newX > 0.01f ? 0f : -1f) : newX;
            upPressed = CustomInput.GetKey("MoveUp", playerCommon.playerIndex);
            upPressedDown = CustomInput.GetKeyDown("MoveUp", playerCommon.playerIndex);
            upPressedUp = CustomInput.GetKeyUp("MoveUp", playerCommon.playerIndex);
            newY = upPressed ? 1f : 0f;
            downPressed = CustomInput.GetKey("MoveDown", playerCommon.playerIndex);
            downPressedDown = CustomInput.GetKeyDown("MoveDown", playerCommon.playerIndex);
            downPressedUp = CustomInput.GetKeyUp("MoveDown", playerCommon.playerIndex);
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

            Vector2 stickPos = CustomInput.GetGamepadStickPosition(controllerType, GamepadStick.left);
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
        dashPressed = CustomInput.GetKey("Dash", playerCommon.playerIndex);
        grabPressed = CustomInput.GetKey("Grab", playerCommon.playerIndex);
        jumpPressed = CustomInput.GetKey("Jump", playerCommon.playerIndex);
        attackWeakPressed = CustomInput.GetKey("AttackWeak", playerCommon.playerIndex);
        attackStrongPressed = CustomInput.GetKey("AttackStrong", playerCommon.playerIndex);
        shieldPressed = CustomInput.GetKey("Shield", playerCommon.playerIndex);

        //Down
        dashPressedDown = CustomInput.GetKeyDown("Dash", playerCommon.playerIndex);
        grabPressedDown = CustomInput.GetKeyDown("Grab", playerCommon.playerIndex);
        jumpPressedDown = CustomInput.GetKeyDown("Jump", playerCommon.playerIndex);
        attackWeakPressedDown = CustomInput.GetKeyDown("AttackWeak", playerCommon.playerIndex);
        attackStrongPressedDown = CustomInput.GetKeyDown("AttackStrong", playerCommon.playerIndex);
        shieldPressedDown = CustomInput.GetKeyDown("Shield", playerCommon.playerIndex);

        //Up
        dashPressedUp = CustomInput.GetKeyUp("Dash", playerCommon.playerIndex);
        grabPressedUp = CustomInput.GetKeyUp("Grab", playerCommon.playerIndex);
        jumpPressedUp = CustomInput.GetKeyUp("Jump", playerCommon.playerIndex);
        attackWeakPressedUp = CustomInput.GetKeyUp("AttackWeak", playerCommon.playerIndex);
        attackStrongPressedUp = CustomInput.GetKeyUp("AttackStrong", playerCommon.playerIndex);
        shieldPressedUp = CustomInput.GetKeyUp("Shield", playerCommon.playerIndex);
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
