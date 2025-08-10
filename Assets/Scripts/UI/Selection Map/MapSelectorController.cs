using UnityEngine;
using TMPro;

public class MapSelectorController : MonoBehaviour
{
    private TurningSelector mapSelector;
    private ControllerType controllerIndex;
    private bool isControllerIndexInit = false;

    [SerializeField] private ControllerSelector controllerSelector = ControllerSelector.last;
    [SerializeField] private InputManager.GeneralInput nextMapInput;
    [SerializeField] private InputManager.GeneralInput previousMapInput;
    [SerializeField] private InputManager.GeneralInput backInput;
    [SerializeField] private InputManager.GeneralInput applyInput;
	[SerializeField] private TextMeshProUGUI buttonPromptLeft, buttonPromptContinue, buttonPromptRight;
    [SerializeField] private TextMeshProUGUI levelNameText;

    private void Awake()
    {
        mapSelector = GetComponentInChildren<TurningSelector>();
    }

    private void Start()
    {
		if(controllerSelector == ControllerSelector.keyboard || controllerSelector == ControllerSelector.gamepad1 || controllerSelector == ControllerSelector.gamepad2 || controllerSelector == ControllerSelector.gamepad3
            || controllerSelector == ControllerSelector.gamepad4 || controllerSelector == ControllerSelector.gamepadAll || controllerSelector == ControllerSelector.all)
        {
            isControllerIndexInit = true;
            controllerIndex = (ControllerType)controllerSelector;
			InitPrompt();
        }
        else
        {
			isControllerIndexInit = false;
            TextMeshProUGUI text = buttonPromptContinue.GetComponent<TextMeshProUGUI>();
			text.text = LanguageManager.instance.GetText("UI_MapSelector_Join").Resolve();
        }

        RefreshSelectedItemDisplay();
    }

    private void Update()
    {
        if(!isControllerIndexInit)
        {
            if (AnyKeyPressed(out ControllerType controllerType, out InputKey key))
            {
                isControllerIndexInit = true;
                controllerIndex = controllerType;
                InitPrompt();
            }
        }

        if (isControllerIndexInit)
        {
            if (controllerSelector == ControllerSelector.last)
            {
                if (AnyKeyPressed(out ControllerType controllerType, out InputKey key))
                    controllerIndex = controllerType;
            }

            nextMapInput.controllerType = controllerIndex;
            previousMapInput.controllerType = controllerIndex;
            backInput.controllerType = controllerIndex;
            applyInput.controllerType = controllerIndex;

            if (nextMapInput.IsPressedDown())
            {
                mapSelector.SelectedNextItem();
                RefreshSelectedItemDisplay();
            }

            if (previousMapInput.IsPressedDown())
            {
                mapSelector.SelectPreviousItem();
                RefreshSelectedItemDisplay();
            }

            if (applyInput.IsPressedDown())
            {
                TryLoadNextScene();
            }

            if (InputManager.GetGamepadUnPluggedAll(out ControllerType[] controllerTypes))
            {
                for (int i = 0; i < controllerTypes.Length; i++)
                {
                    if (controllerTypes[i] == controllerIndex)
                    {
                        isControllerIndexInit = false;
                    }
                }
            }
        }

        ControllerType escapeController = isControllerIndexInit ? controllerIndex : ControllerType.Any;
        backInput.controllerType = escapeController;
        if (backInput.IsPressedDown())
        {
            TransitionManager.instance.LoadScene("Selection Char");
        }
    }

    private void RefreshSelectedItemDisplay()
    {
        MapSelectorItemData selectedItemData = mapSelector.selectedItem.GetComponent<MapSelectorItemData>();
        levelNameText.text = selectedItemData.sceneName;
    }

	private void InitPrompt()
	{
		ControllerModel model = (controllerIndex == ControllerType.Keyboard) ? ControllerModel.Keyboard : ControllerModel.XBoxSeries;
        buttonPromptContinue.text = LanguageManager.instance.GetText("UI_MapSelector_Continue").Resolve(model);
        buttonPromptLeft.text = LanguageManager.instance.GetText("UI_MapSelector_Left").Resolve(model);
		buttonPromptRight.text = LanguageManager.instance.GetText("UI_MapSelector_Right").Resolve(model);
	}

    private void TryLoadNextScene()
    {
        SelectionMapOldSceneData selectionMapSceneData = null;

        if(TransitionManager.instance.oldSceneName == "Selection Char")
        {
            SelectionCharOldSceneData selectionCharOldSceneData = TransitionManager.instance.GetOldSceneData("Selection Char") as SelectionCharOldSceneData;
            selectionMapSceneData = new SelectionMapOldSceneData(selectionCharOldSceneData.charData);
        }
        else
        {
            LevelOldSceneData oldSceneData = TransitionManager.instance.GetOldSceneData() as LevelOldSceneData;
            selectionMapSceneData = new SelectionMapOldSceneData(oldSceneData.charData);
        }

        MapSelectorItemData mapSelectorItemData = mapSelector.selectedItem.GetComponent<MapSelectorItemData>();
        TransitionManager.instance.LoadSceneAsync(mapSelectorItemData.sceneName, selectionMapSceneData);
    }

    #region ControllerAlreadyInit

    private bool AnyKeyPressed(out ControllerType controllerType, out InputKey key)
    {
        bool TestControllerType(ControllerType controllerType, out InputKey key)
        {
            if (controllerType == ControllerType.Keyboard || InputManager.IsGamePadConnected(controllerType))
            {
                if (InputManager.Listen(controllerType, out key))
                    return true;
            }
            key = InputKey.None;
            return false;
        }

        if (TestControllerType(ControllerType.Keyboard, out key))
        {
            controllerType = ControllerType.Keyboard;
            return true;
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
        key = InputKey.None;
        controllerType = ControllerType.Keyboard;
        return false;
    }

    #endregion
}
