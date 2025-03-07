using UnityEngine;

public class InputSelectorSelectableUI : SelectableUI
{
    private ControlItem controlItem;

    public override bool interactable
    {
        get => base.interactable;
        set
        {
            base.interactable = value;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        controlItem = GetComponent<ControlItem>();
    }

    public override void OnPressedUp()
    {
        if (isSelected && !isDesactivatedThisFrame)
        {
            controlItem.OnKeyButtonDown();
            if(controlItem.isListening)
            {
                isActive = true;
            }
        }
    }

    public override void OnPressed()
    {

    }

    private void Update()
    {
        isDesactivatedThisFrame = false;
        if (!isActive)
            return;

        if(!controlItem.isListening)
        {
            isActive = false;
            isDesactivatedThisFrame = true;
        }
    }
}
