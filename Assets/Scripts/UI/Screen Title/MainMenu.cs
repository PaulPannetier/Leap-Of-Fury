using UnityEngine;

public class MainMenu : MonoBehaviour
{
    private SelectableUIGroup selectableUIGroup;

    private void Awake()
    {
        selectableUIGroup = GetComponentInChildren<SelectableUIGroup>();
    }

    private void OnEnableInternal()
    {
        selectableUIGroup.Init();
    }

    private void OnEnable()
    {
        this.InvokeWaitAFrame(nameof(OnEnableInternal));
        InputManager.ShowMouseCursor();
    }

    private void OnDisable()
    {
        selectableUIGroup.ResetToDefault();
    }
}
