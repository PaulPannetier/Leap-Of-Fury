using UnityEngine;

public class CharSelectorController : MonoBehaviour
{
    private TurningSelector[] turningSelectors;
    private GameObject[] helpCanvas;
    private ControllerType[] controllerIndexs;
    private bool[] isTurningSelectorInit;
    private bool[] isTurningSelectorsFinishSelection;
    private bool[] isHelpCanvasOpen;
    private int indexToInit;
    private bool canLoadNextScene = false;
    private bool nextSceneIsLoading = false;

    [SerializeField] private GameObject[] charHelperCanvasPrefabs;

    [SerializeField] private InputManager.GeneralInput helpButton;
    [SerializeField] private InputManager.GeneralInput escapeButton;
    [SerializeField] private InputManager.GeneralInput nextItemInput;
    [SerializeField] private InputManager.GeneralInput previousItemInput;
    [SerializeField] private InputManager.GeneralInput applyItemInput;

    #if UNITY_EDITOR

    private bool debugMode;
    [SerializeField] private InputKey DEBUG_AddGhostCharKey = InputKey.F2;

    #endif

    private void Awake()
    {
        turningSelectors = new TurningSelector[4];
        for (int i = 0; i < 4; i++)
        {
            turningSelectors[i] = transform.GetChild(i).gameObject.GetComponent<TurningSelector>();
        }
    }

    private void Start()
    {
        isTurningSelectorInit = new bool[4];
        isTurningSelectorsFinishSelection = new bool[4];
        isHelpCanvasOpen = new bool[4];
        helpCanvas = new GameObject[4];
        controllerIndexs = new ControllerType[4];
        indexToInit = 0;
    }

    private void Update()
    {
        for (int i = 0; i < turningSelectors.Length; i++)
        {
            if (!isTurningSelectorInit[i])
                break;

            helpButton.controllerType = controllerIndexs[i];

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

            nextItemInput.controllerType = controllerIndexs[i];
            previousItemInput.controllerType = controllerIndexs[i];
            applyItemInput.controllerType = controllerIndexs[i];
            escapeButton.controllerType = controllerIndexs[i];

            if (nextItemInput.IsPressedDown())
            {
                turningSelectors[i].SelectedNextItem();
            }
            else if (previousItemInput.IsPressedDown())
            {
                turningSelectors[i].SelectPreviousItem();
            }

            if(applyItemInput.IsPressedDown())
            {
                isTurningSelectorsFinishSelection[i] = true;
            }
            else if(isTurningSelectorsFinishSelection[i] && escapeButton.IsPressedDown())
            {
                isTurningSelectorsFinishSelection[i] = false;
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

        //si il reste un char a init
        if (!isTurningSelectorInit[turningSelectors.Length - 1])
        {
            if (NewControllerIsPressingAKey(out ControllerType controllerType, out InputKey key))
            {
                isTurningSelectorInit[indexToInit] = true;
                controllerIndexs[indexToInit] = controllerType;
                indexToInit++;
            }

            #if UNITY_EDITOR

            if(InputManager.GetKeyDown(DEBUG_AddGhostCharKey))
            {
                isTurningSelectorInit[indexToInit] = true;
                controllerIndexs[indexToInit] = ControllerType.Gamepad1;
                indexToInit++;
                debugMode = true;
            }

            #endif
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

        canLoadNextScene = allIsSelected && indexToInit >= 2;

        #if UNITY_EDITOR

        if(debugMode)
        {
            allIsSelected = isTurningSelectorInit[0] && !isTurningSelectorsFinishSelection[0];
            canLoadNextScene = allIsSelected;
        }

        #endif

        if(canLoadNextScene && !nextSceneIsLoading)
        {
            nextSceneIsLoading = true;
            LoadSelectionMapScene();
        }

        for (int i = 0; i < indexToInit; i++)
        {
            if (isHelpCanvasOpen[i])
                continue;

            if (escapeButton.IsPressedDown())
            {
                TransitionManager.instance.LoadSceneAsync("Screen Title", null);
            }
        }

        //DebugText.instance.text += turningSelectors[0].selectedItem.GetComponent<CharSelectorItemData>().gameObject.name;
    }

    private void OpenHelpCanvas(int indexTuringSelector)
    {
        isHelpCanvasOpen[indexTuringSelector] = true;
        GameObject selectedChar = turningSelectors[indexTuringSelector].selectedItem;
        int charNumber = selectedChar.GetComponent<CharSelectorItemData>().charNumber;
        helpCanvas[indexTuringSelector] = Instantiate(charHelperCanvasPrefabs[charNumber], transform);
        CharHelpCanvas charHelpCanvas = helpCanvas[indexTuringSelector].GetComponent<CharHelpCanvas>();
        charHelpCanvas.Lauch(turningSelectors[indexTuringSelector], controllerIndexs[indexTuringSelector], indexTuringSelector, OnCloseHelpCanvas);
    }

    private void OnCloseHelpCanvas(int indexHelpCanvas)
    {
        isHelpCanvasOpen[indexHelpCanvas] = false;
    }

    private void LoadSelectionMapScene()
    {
        object[] data = new object[indexToInit];
        for (int i = 0; i < data.Length; i++)
        {
            CharSelectorItemData tmp = turningSelectors[i].selectedItem.GetComponent<CharSelectorItemData>();
            tmp.playerIndex = (PlayerIndex)(i + 1);
            tmp.controllerType = controllerIndexs[i];
            data[i] = tmp;
        }
        TransitionManager.instance.LoadSceneAsync("Selection Map", data);
    }

    private void RemoveSettingsIndex(int index)
    {
        if (index >= turningSelectors.Length)
            return;
        if(index == 3)
        {
            isTurningSelectorInit[index] = isTurningSelectorsFinishSelection[index] = false;
            controllerIndexs[indexToInit] = ControllerType.Keyboard;
            indexToInit--;
            return;
        }
        for (int i = index; i < turningSelectors.Length - 1; i++)
        {
            isTurningSelectorInit[i] = isTurningSelectorInit[i + 1];
            controllerIndexs[i] = controllerIndexs[i + 1];
            isTurningSelectorsFinishSelection[i] = isTurningSelectorsFinishSelection[i + 1];
        }
        isTurningSelectorInit[turningSelectors.Length - 1] = isTurningSelectorsFinishSelection[turningSelectors.Length - 1] = false;
        controllerIndexs[turningSelectors.Length - 1] = ControllerType.Keyboard;
        indexToInit = Mathf.Max(0, indexToInit - 1);
    }

    #region ControllerAlreadyInit / NewControllerIsPressingAKey

    private bool ControllerIsAlreadyInit(ControllerType controllerType, out int index)
    {
        for (int i = 0; i < 4; i++)
        {
            if (isTurningSelectorInit[i] && controllerIndexs[i] == controllerType)
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
}