using UnityEngine;

public class InputSelectorSelectableUI : SelectableUI
{
    private ControlItem controlItem;

    protected override void Awake()
    {
        base.Awake();
        controlItem = GetComponent<ControlItem>();
    }

    public override void OnPressed()
    {
        if (isSelected && !isDesactivatedThisFrame)
        {
            isActive = true;
            controlItem.StartListening();
        }
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
