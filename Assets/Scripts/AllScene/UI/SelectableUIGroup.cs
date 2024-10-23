using UnityEngine;
using System.Collections.Generic;

public enum PlayerSelector
{
    player1 = 1,
    player2 = 2,
    player3 = 3,
    player4 = 4,
    player5 = 5,
    first = 6,
    last = 7,
    all = 8
}

public enum ControllerSelector
{
    keyboard = 0,
    gamepad1 = 1,
    gamepad2 = 2,
    gamepad3 = 3,
    gamepad4 = 4,
    gamepadAll = 5,
    all = 6,
    first = 7,
    last = 8
}

public class SelectableUIGroup : MonoBehaviour
{
    private bool isCurrentUIPressed = false;
    private byte lastDirectionHit; //0 None, 1 => up, 2 => down, 3 => right, 4 => left
    private float lastTimeHitDirection, timerKeepPress;
    private bool hasStarted;
    private Vector2 oldMousePosition;
    private bool isControlByMouse;

    private ControllerType _controllerType;
    public ControllerType controllerType
    {
        get => _controllerType;
        private set
        {
            _controllerType = value;
            upItemInput.controllerType = value;
            downItemInput.controllerType = value;
            rightItemInput.controllerType = value;
            leftItemInput.controllerType = value;
            applyInput.controllerType = value;
        }
    }

    public bool enableBehaviour = true;

    [SerializeField] private ControllerSelector controllerSelector = ControllerSelector.last;
    [SerializeField] private SelectableUI defaultUISelected;
    [SerializeField] private InputManager.GeneralInput upItemInput;
    [SerializeField] private InputManager.GeneralInput downItemInput;
    [SerializeField] private InputManager.GeneralInput rightItemInput;
    [SerializeField] private InputManager.GeneralInput leftItemInput;
    [SerializeField] private InputManager.GeneralInput applyInput;
    [SerializeField] private bool autoSelectDefaultUIWhenEnable;
    [SerializeField] private bool allowKeepPressed;
    [SerializeField] private float keepPressFirstDelay = 1f;
    [SerializeField] private float keepPressDelay = 0.2f;
    public BaseController allowedController = BaseController.KeyboardAndGamepad;

    public SelectableUI selectedUI { get; private set; } = null;

    private void OnEnable()
    {
        if (!hasStarted)
            return;

        Init();

        if (autoSelectDefaultUIWhenEnable)
        {
            selectedUI = defaultUISelected;
            selectedUI.OnSelected();
        }
    }

    private void Start()
    {
        Init();
        hasStarted = true;

        if (autoSelectDefaultUIWhenEnable)
        {
            selectedUI = defaultUISelected;
            selectedUI.OnSelected();
        }
    }

    public void Init()
    {
        selectedUI = null;
        isCurrentUIPressed = false;
        oldMousePosition = InputManager.mousePosition;

        HashSet<SelectableUI> cache = new HashSet<SelectableUI>();
        InitRecur(defaultUISelected, ref cache);

        void InitRecur(SelectableUI selectableUI, ref HashSet<SelectableUI> cache)
        {
            if (cache.Contains(selectableUI))
                return;

            selectableUI.selectableUIGroup = this;
            selectableUI.isMouseInteractable = allowedController != BaseController.Gamepad;
            selectableUI.OnDeselected();
            cache.Add(selectableUI);
            if (selectableUI.upSelectableUI != null)
                InitRecur(selectableUI.upSelectableUI, ref cache);
            if (selectableUI.downSelectableUI != null)
                InitRecur(selectableUI.downSelectableUI, ref cache);
            if (selectableUI.rightSelectableUI != null)
                InitRecur(selectableUI.rightSelectableUI, ref cache);
            if (selectableUI.leftSelectableUI != null)
                InitRecur(selectableUI.leftSelectableUI, ref cache);
        }
    }

    public void ResetToDefault()
    {
        HashSet<SelectableUI> cache = new HashSet<SelectableUI>();
        ResetSelectableUIRecur(defaultUISelected, ref cache);
        selectedUI = null;

        void ResetSelectableUIRecur(SelectableUI selectableUI, ref HashSet<SelectableUI> cache)
        {
            if (cache.Contains(selectableUI))
                return;

            selectableUI.ResetToDefault();
            cache.Add(selectableUI);
            if (selectableUI.upSelectableUI != null)
                ResetSelectableUIRecur(selectableUI.upSelectableUI, ref cache);
            if (selectableUI.downSelectableUI != null)
                ResetSelectableUIRecur(selectableUI.downSelectableUI, ref cache);
            if (selectableUI.rightSelectableUI != null)
                ResetSelectableUIRecur(selectableUI.rightSelectableUI, ref cache);
            if (selectableUI.leftSelectableUI != null)
                ResetSelectableUIRecur(selectableUI.leftSelectableUI, ref cache);
        }
    }

    public bool RequestSelectedByMouse(SelectableUI selectableUI)
    {
        if (isCurrentUIPressed)
            return false;

        if (selectedUI != null)
        {
            selectedUI.OnDeselected();    
        }
        selectedUI = selectableUI;
        isControlByMouse = true;
        return true;
    }

    public void DeselectSelecteUI()
    {
        if (isCurrentUIPressed)
            return;

        if (selectedUI != null)
        {
            selectedUI.OnDeselected();
        }
        isControlByMouse = false;
        selectedUI = null;
    }

    public bool RequestDeselectedByMouse(SelectableUI selectableUI)
    {
        if (isCurrentUIPressed)
            return false;

        if (selectedUI != selectableUI && selectedUI != null)
        {
            selectedUI.OnDeselected();
        }
        isControlByMouse = false;
        selectedUI = null;
        return true;
    }

    private void Update()
    {
        if (!enableBehaviour)
            return;

        if(!isControlByMouse && oldMousePosition.SqrDistance(InputManager.mousePosition) > 1e-4f)
        {
            DeselectSelecteUI();
        }
        oldMousePosition = InputManager.mousePosition;

        if(selectedUI == null)
        {
            isCurrentUIPressed = false;
            if (controllerSelector == ControllerSelector.keyboard || controllerSelector == ControllerSelector.gamepad1 || controllerSelector == ControllerSelector.gamepad2 || controllerSelector == ControllerSelector.gamepad3
                 || controllerSelector == ControllerSelector.gamepad4 || controllerSelector == ControllerSelector.gamepadAll || controllerSelector == ControllerSelector.all)
            {
                controllerType = (ControllerType)controllerSelector;
                if (ControllerIsPressingAKey(out ControllerType controllerTrigger, out InputKey key))
                {
                    if(controllerTrigger == controllerType)
                    {
                        if (IsControllerTypeAnAllowedController(controllerType))
                        {
                            selectedUI = defaultUISelected;
                            isControlByMouse = false;
                            selectedUI.OnSelected();
                        }
                    }

                }
            }
            else
            {
                if(ControllerIsPressingAKey(out ControllerType controllerType, out InputKey key))
                {
                    if(IsControllerTypeAnAllowedController(controllerType))
                    {
                        selectedUI = defaultUISelected;
                        selectedUI.OnSelected();
                        isControlByMouse = false;
                        this.controllerType = controllerType;
                    }
                }
            }
        }
        else
        {
            if(!selectedUI.isActive)
            {
                bool changeControllerType = false;
                if (controllerSelector == ControllerSelector.last && !isCurrentUIPressed)
                {
                    if (ControllerIsPressingAKey(out ControllerType controllerType, out InputKey key))
                    {
                        if (this.controllerType != controllerType && IsControllerTypeAnAllowedController(controllerType))
                        {
                            this.controllerType = controllerType;
                            changeControllerType = true;
                        }
                    }
                }

                if (!changeControllerType)
                {
                    if(!isCurrentUIPressed)
                    {
                        UpdateSelectedUI(rightItemInput.IsPressedDown(), leftItemInput.IsPressedDown(), upItemInput.IsPressedDown(), downItemInput.IsPressedDown());

                        if (applyInput.IsPressedDown())
                        {
                            lastDirectionHit = 0;
                            selectedUI.OnPressed();
                            isCurrentUIPressed = true;
                        }

                        if(allowKeepPressed)
                        {
                            void ChangeSelectableUI()
                            {
                                switch (lastDirectionHit)
                                {
                                    case 1:
                                        UpdateSelectedUI(false, false, true, false, false);
                                        break;
                                    case 2:
                                        UpdateSelectedUI(false, false, false, true, false);
                                        break;
                                    case 3:
                                        UpdateSelectedUI(true, false, false, false, false);
                                        break;
                                    case 4:
                                        UpdateSelectedUI(false, true, false, false, false);
                                        break;
                                    default:
                                        lastTimeHitDirection = Time.time;
                                        break;
                                }
                            }

                            bool IsLastDirectionPress()
                            {
                                switch (lastDirectionHit)
                                {
                                    case 1:
                                        return upItemInput.IsPressed();
                                    case 2:
                                        return downItemInput.IsPressed();
                                    case 3:
                                        return rightItemInput.IsPressed();
                                    case 4:
                                        return leftItemInput.IsPressed();
                                    default:
                                        return false;
                                }
                            }

                            if(IsLastDirectionPress())
                            {
                                if (Time.time - lastTimeHitDirection > keepPressFirstDelay)
                                {
                                    if (timerKeepPress <= 1e-5f)
                                    {
                                        ChangeSelectableUI();
                                    }

                                    if (timerKeepPress > keepPressDelay)
                                    {
                                        timerKeepPress -= keepPressDelay;
                                        ChangeSelectableUI();
                                    }
                                    timerKeepPress += Time.deltaTime;
                                }
                                else
                                {
                                    timerKeepPress = 0f;
                                }
                            }
                            else
                            {
                                timerKeepPress = 0f;
                            }
                        }
                    }

                    if (applyInput.IsPressedUp())
                    {
                        selectedUI.OnPressedUp();
                        isCurrentUIPressed = false;
                    }
                }
            }
            else
            {
                isCurrentUIPressed = false;
            }

            void UpdateSelectedUI(bool moveRight, bool moveLeft, bool moveUp, bool moveDown, bool hitDirectionOverride = true)
            {
                if (selectedUI.upSelectableUI != null && moveUp)
                {
                    if(hitDirectionOverride)
                    {
                        lastDirectionHit = 1;
                        lastTimeHitDirection = Time.time;
                        timerKeepPress = 0f;
                    }
                    isControlByMouse = false;
                    selectedUI.OnDeselected();
                    selectedUI = selectedUI.upSelectableUI;
                    selectedUI.OnSelected();
                }

                if (selectedUI.downSelectableUI != null && moveDown)
                {
                    if (hitDirectionOverride)
                    {
                        lastDirectionHit = 2;
                        lastTimeHitDirection = Time.time;
                        timerKeepPress = 0f;
                    }
                    isControlByMouse = false;
                    selectedUI.OnDeselected();
                    selectedUI = selectedUI.downSelectableUI;
                    selectedUI.OnSelected();
                }

                if (selectedUI.rightSelectableUI != null && moveRight)
                {
                    if (hitDirectionOverride)
                    {
                        lastDirectionHit = 3;
                        lastTimeHitDirection = Time.time;
                        timerKeepPress = 0f;
                    }
                    isControlByMouse = false;
                    selectedUI.OnDeselected();
                    selectedUI = selectedUI.rightSelectableUI;
                    selectedUI.OnSelected();
                }

                if (selectedUI.leftSelectableUI != null && moveLeft)
                {
                    if (hitDirectionOverride)
                    {
                        lastDirectionHit = 4;
                        lastTimeHitDirection = Time.time;
                        timerKeepPress = 0f;
                    }
                    isControlByMouse = false;
                    selectedUI.OnDeselected();
                    selectedUI = selectedUI.leftSelectableUI;
                    selectedUI.OnSelected();
                }
            }
        }
    }

    private bool IsControllerTypeAnAllowedController(ControllerType controllerType)
    {
        if (allowedController == BaseController.KeyboardAndGamepad || controllerType == ControllerType.Any)
            return true;

        if (controllerType == ControllerType.Keyboard && allowedController == BaseController.Keyboard)
            return true;

        return allowedController == BaseController.Gamepad && controllerType != ControllerType.Keyboard;
    }

    private bool ControllerIsPressingAKey(out ControllerType controllerType, out InputKey key)
    {
        bool TestControllerType(ControllerType controllerType, out InputKey key)
        {
            if ((controllerType == ControllerType.Keyboard || InputManager.IsGamePadConnected(controllerType)))
            {
                if (InputManager.Listen(controllerType, out key))
                    return true;
            }
            key = InputKey.None;
            return false;
        }

        if (TestControllerType(ControllerType.Keyboard, out key))
        {
            if(!InputManager.IsMouseKey(key))
            {
                controllerType = ControllerType.Keyboard;
                return true;
            }
        }
        if (TestControllerType(ControllerType.Gamepad1, out key))
        {
            controllerType = ControllerType.Gamepad1;
            return true;
        }
        if (TestControllerType(ControllerType.Gamepad2, out key))
        {
            controllerType = ControllerType.Gamepad2;
            return true;
        }
        if (TestControllerType(ControllerType.Gamepad3, out key))
        {
            controllerType = ControllerType.Gamepad3;
            return true;
        }
        if (TestControllerType(ControllerType.Gamepad4, out key))
        {
            controllerType = ControllerType.Gamepad4;
            return true;
        }
        key = (int)KeyCode.None;
        controllerType = ControllerType.Keyboard;
        return false;
    }

    #region OnValidate

#if UNITY_EDITOR

    private void OnValidate()
    {
        keepPressFirstDelay = Mathf.Max(0f, keepPressFirstDelay);
        keepPressDelay = Mathf.Max(0f, keepPressDelay);
    }

#endif

    #endregion
}
