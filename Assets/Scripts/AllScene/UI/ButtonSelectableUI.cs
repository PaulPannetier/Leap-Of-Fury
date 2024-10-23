using UnityEngine;
using UnityEngine.Events;

public class ButtonSelectableUI : SelectableUI
{
    [Space]
    public UnityEvent onButtonPressed;


    public override void OnPressedUp()
    {

    }

    public override void OnPressed()
    {
        if (isSelected)
        {
            onButtonPressed?.Invoke();
        }
    }
}
