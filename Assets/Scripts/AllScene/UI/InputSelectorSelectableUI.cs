using UnityEngine;
using TMPro;

public class InputSelectorSelectableUI : SelectableUI
{
    private bool isActivatedThisFrame = false;
    private BaseController controller;

    [SerializeField] private TMP_Text inputText;
    [SerializeField] private TMP_Text keyText;

    public override SelectableUIGroup selectableUIGroup
    {
        get => base.selectableUIGroup;
        set
        {
            base.selectableUIGroup = value;
            if (value != null)
            {
                controller = value.allowedController;
            }
        }
    }

    public override void OnPressed()
    {
        if (isSelected && !isDesactivatedThisFrame)
        {
            isActive = true;
            isActivatedThisFrame = true;
            keyText.text = string.Empty;
        }
    }

    private void Update()
    {
        isDesactivatedThisFrame = false;

        if (!isActive)
        {
            isActivatedThisFrame = false;
            return;
        }

        if(InputManager.Listen(controller, out InputKey key) && !isActivatedThisFrame)
        {
            print(key);
            isActive = false;
            isDesactivatedThisFrame = true;
        }

        isActivatedThisFrame = false;
    }
}
