using UnityEngine;
using UnityEngine.UI;

public class CheckboxSelectableUI : SelectableUI
{
    [SerializeField] private Toggle toggle;

    public override void OnPressed()
    {
        if (isSelected)
        {
            toggle.isOn = !toggle.isOn;
        }
    }
}
