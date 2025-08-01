using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class CharSelectorController : MonoBehaviour
{
    private TurningSelector[] turningSelectors;
    private GameObject[] helpCanvas;
    private ControllerType[] controllers;
    private GameObject[] statusIndicators;
    private bool[] isTurningSelectorInit;
    private bool[] isTurningSelectorsFinishSelection;
    private bool[] isHelpCanvasOpen;
    private bool canLoadNextScene = false;
    private bool nextSceneIsLoading = false;

	private const int MAX_PLAYERS = 4;

    [SerializeField] private InputManager.GeneralInput helpButton;
    [SerializeField] private InputManager.GeneralInput escapeButton;
    [SerializeField] private InputManager.GeneralInput nextItemInput;
    [SerializeField] private InputManager.GeneralInput previousItemInput;
    [SerializeField] private InputManager.GeneralInput applyItemInput;

    [SerializeField] private Transform turningSelectorsParent;
    [SerializeField] private Transform statusIndicatorsParent;
    [SerializeField] private bool enableHelpCanvas = true;

    [SerializeField] private Color textColorJoin = Color.white;
    [SerializeField] private Color textColorChoose = Color.white;
    [SerializeField] private Color textColorReady = Color.green;

#if UNITY_EDITOR

    [Header("DEBUG")]
    [Space]
    [SerializeField] private bool addSelectedGamepadCharacter;
    [SerializeField] private InputManager.GeneralInput addSelectedGamepadCharacterInput;

#endif

    private void Awake()
    {
	    turningSelectors = new TurningSelector[MAX_PLAYERS];
		statusIndicators = new GameObject[MAX_PLAYERS];

        for (int i = 0; i < turningSelectorsParent.childCount; i++)
        {
            if (i >= MAX_PLAYERS)
                break;
            turningSelectors[i] = turningSelectorsParent.GetChild(i).GetComponent<TurningSelector>();
        }

        for (int i = 0; i < turningSelectorsParent.childCount; i++)
        {
            if (i >= MAX_PLAYERS)
                break;
            statusIndicators[i] = statusIndicatorsParent.GetChild(i).gameObject;
        }

        helpCanvas = new GameObject[MAX_PLAYERS];
        controllers = new ControllerType[MAX_PLAYERS];
		isTurningSelectorInit = new bool[MAX_PLAYERS];
        isTurningSelectorsFinishSelection = new bool[MAX_PLAYERS];
        isHelpCanvasOpen = new bool[MAX_PLAYERS];
    }

    private void Start()
	{
        string statusIndicatorText = LanguageManager.instance.GetText("UI_StatusIndicator_Join").Resolve();
        for (int i = 0; i < MAX_PLAYERS; i++)
        {
			statusIndicators[i].GetComponent<TextMeshProUGUI>().text = statusIndicatorText;
		}
	}

    private void Update()
    {

#if UNITY_EDITOR

        if(addSelectedGamepadCharacterInput.IsPressedDown())
        {
            AddSelectedGamepadCharacter();
        }

#endif

        escapeButton.controllerType = ControllerType.Any;
        List<ControllerType> notInitController = new List<ControllerType>(5)
        {
            ControllerType.Keyboard,
            ControllerType.Gamepad1,
            ControllerType.Gamepad2,
            ControllerType.Gamepad3,
            ControllerType.Gamepad4
        };

        for (int i = 0; i < MAX_PLAYERS; i++)
        {
            if (isTurningSelectorInit[i] || isHelpCanvasOpen[i])
            {
                notInitController.Remove(controllers[i]);
            }
        }

        foreach (ControllerType controllerType in notInitController)
        {
            escapeButton.controllerType = controllerType;
            if (escapeButton.IsPressedDown())
            {
                TransitionManager.instance.LoadSceneAsync("TitleScreen", null);
                return;
            }
        }

        for (int i = 0; i < MAX_PLAYERS; i++)
        {
            if (!isTurningSelectorInit[i])
                continue;

            if(enableHelpCanvas)
            {
                helpButton.controllerType = controllers[i];

                if (isHelpCanvasOpen[i])
                {
                    if (helpButton.IsPressedDown())
                    {
                        OnCloseHelpCanvas(i);
                    }
                    continue;
                }
                else
                {
                    if (helpButton.IsPressedDown())
                    {
                        OpenHelpCanvas(i);
                    }
                }
            }

            nextItemInput.controllerType = controllers[i];
            previousItemInput.controllerType = controllers[i];
            applyItemInput.controllerType = controllers[i];
            escapeButton.controllerType = controllers[i];

			// If we're not ready, we can rotate the wheel
			if (!isTurningSelectorsFinishSelection[i])
			{
				if (nextItemInput.IsPressedDown())
				{
					turningSelectors[i].SelectedNextItem();
				}
				else if (previousItemInput.IsPressedDown())
				{
					turningSelectors[i].SelectPreviousItem();
				}
			}

            if(applyItemInput.IsPressedDown())
            {
                isTurningSelectorsFinishSelection[i] = true;
				TextMeshProUGUI curText = statusIndicators[i].GetComponent<TextMeshProUGUI>();
				curText.text = LanguageManager.instance.GetText("UI_StatusIndicator_Ready").Resolve(InputManager.GetControllerModel(controllers[i]));
				curText.color = textColorReady;
            }
            else if(isTurningSelectorsFinishSelection[i] && escapeButton.IsPressedUp())
            {
                isTurningSelectorsFinishSelection[i] = false;
				TextMeshProUGUI curText = statusIndicators[i].GetComponent<TextMeshProUGUI>();
				curText.text = LanguageManager.instance.GetText("UI_StatusIndicator_Choose").Resolve(InputManager.GetControllerModel(controllers[i]));
				curText.color = textColorChoose;
            }
            else if(isTurningSelectorInit[i] && escapeButton.IsPressedUp())
            {
                isTurningSelectorInit[i] = false;
                TextMeshProUGUI curText = statusIndicators[i].GetComponent<TextMeshProUGUI>();
                curText.text = LanguageManager.instance.GetText("UI_StatusIndicator_Join").Resolve();
                curText.color = textColorJoin;
            }
        }

        if (InputManager.GetGamepadUnPluggedAll(out ControllerType[] controllerTypes))
        {
            for (int i = 0; i < controllerTypes.Length; i++)
            {
                if (ControllerIsAlreadyInit(controllerTypes[i], out int index))
                {
                    RemoveSettingsIndex(index);
                }
            }
        }

        // Init selector
        for (int i = 0; i < MAX_PLAYERS; i++)
        {
            if (isTurningSelectorInit[i])
                continue;

            if (NewControllerIsPressingAKey(out ControllerType controllerType, out InputKey key))
            {
                isTurningSelectorInit[i] = true;
                controllers[i] = controllerType;
                statusIndicators[i].GetComponent<TextMeshProUGUI>().text = LanguageManager.instance.GetText("UI_StatusIndicator_Choose").Resolve(InputManager.GetControllerModel(controllers[i]));
            }
            break;
        }

        bool allIsSelected = true;
        for (int i = 0; i < turningSelectors.Length; i++)
        {
            if(isTurningSelectorInit[i] && !isTurningSelectorsFinishSelection[i])
            {
                allIsSelected = false;
                break;
            }
        }

        canLoadNextScene = allIsSelected && GetNbCharacterInit() >= 2;
        if(canLoadNextScene && !nextSceneIsLoading)
        {
            nextSceneIsLoading = true;
            LoadSelectionMapScene();
        }
	}

    private int GetNbCharacterInit()
    {
        int nbCharInit = 0;
        for (int i = 0; i < MAX_PLAYERS; i++)
        {
            if (isTurningSelectorInit[i])
                nbCharInit++;
        }
        return nbCharInit;
    }

    private void OpenHelpCanvas(int indexTuringSelector)
    {
        // we shouldn't need the check, but better safe than sorry
		if (isHelpCanvasOpen[indexTuringSelector])
			return;

        isHelpCanvasOpen[indexTuringSelector] = true;
        GameObject selectedChar = turningSelectors[indexTuringSelector].selectedItem;
        CharSelectorItemData charSelectorItemData = selectedChar.GetComponent<CharSelectorItemData>();
        helpCanvas[indexTuringSelector] = Instantiate(charSelectorItemData.helpCanvasPrefab, transform);
        CharHelpCanvas charHelpCanvas = helpCanvas[indexTuringSelector].GetComponent<CharHelpCanvas>();
        charHelpCanvas.Launch(turningSelectors[indexTuringSelector], controllers[indexTuringSelector], indexTuringSelector, OnCloseHelpCanvas);
    }

    private void OnCloseHelpCanvas(int indexHelpCanvas)
    {
        isHelpCanvasOpen[indexHelpCanvas] = false;
    }

    private void LoadSelectionMapScene()
    {
        CharData[] data = new CharData[GetNbCharacterInit()];
        int dataIndex = 0;
        for (int i = 0; i < MAX_PLAYERS; i++)
        {
            if(!isTurningSelectorInit[i])
                continue;

            CharSelectorItemData selectorData = turningSelectors[i].selectedItem.GetComponent<CharSelectorItemData>();
            PlayerIndex playerIndex = (PlayerIndex)(dataIndex + 1);
            data[i] = new CharData(playerIndex, controllers[i], selectorData.charPrefabs);
            dataIndex++;
        }
        TransitionManager.instance.LoadSceneAsync("Selection Map", new SelectionCharOldSceneData(data));
    }

    private void RemoveSettingsIndex(int index)
    {
        if (index >= turningSelectors.Length)
            return;

        isTurningSelectorInit[index] = isTurningSelectorsFinishSelection[index] = false;
        controllers[index] = ControllerType.Keyboard;
    }

    #region ControllerAlreadyInit / NewControllerIsPressingAKey

    private bool ControllerIsAlreadyInit(ControllerType controllerType, out int index)
    {
        for (int i = 0; i < MAX_PLAYERS; i++)
        {
            if (isTurningSelectorInit[i] && controllers[i] == controllerType)
            {
                index = i;
                return true;
            }
        }
        index = 0;
        return false;
    }

    private bool NewControllerIsPressingAKey(out ControllerType controllerType, out InputKey key)
    {
        bool TestControllerType(ControllerType controllerType, out InputKey key)
        {
            if (!ControllerIsAlreadyInit(controllerType, out int i) &&
                (controllerType == ControllerType.Keyboard || InputManager.IsGamePadConnected(controllerType)))
            {
                if (InputManager.Listen(controllerType, out key))
                    return true;
            }
            key = (int)KeyCode.None;
            return false;
        }

        if(TestControllerType(ControllerType.Keyboard, out key))
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

    #region OnValidate

#if UNITY_EDITOR

    private void AddSelectedGamepadCharacter()
    {
        bool IsControllerInit(ControllerType controllerType)
        {
            for (int i = 0; i < MAX_PLAYERS; i++)
            {
                if (isTurningSelectorInit[i] && controllers[i] == controllerType)
                    return true;
            }
            return false;
        }

        ControllerType controllerType = ControllerType.Keyboard;
        if(!IsControllerInit(ControllerType.Gamepad1))
            controllerType = ControllerType.Gamepad1;
        else if (!IsControllerInit(ControllerType.Gamepad2))
            controllerType = ControllerType.Gamepad1;
        else if (!IsControllerInit(ControllerType.Gamepad3))
            controllerType = ControllerType.Gamepad3;
        else if (!IsControllerInit(ControllerType.Gamepad4))
            controllerType = ControllerType.Gamepad4;

        for (int i = 0; i < MAX_PLAYERS; i++)
        {
            if (isTurningSelectorInit[i])
                continue;

            isTurningSelectorInit[i] = isTurningSelectorsFinishSelection[i] = true;
            TextMeshProUGUI curText = statusIndicators[i].GetComponent<TextMeshProUGUI>();
            curText.text = LanguageManager.instance.GetText("UI_StatusIndicator_Ready").Resolve();
            curText.color = textColorReady;
            controllers[i] = controllerType;
        }
    }

    private void OnValidate()
    {
        if(addSelectedGamepadCharacter)
        {
            addSelectedGamepadCharacter = false;
            AddSelectedGamepadCharacter();
        }
    }

#endif

    #endregion
}