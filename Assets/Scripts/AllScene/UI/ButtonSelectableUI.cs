using UnityEngine.Events;

public class ButtonSelectableUI : SelectableUI
{
    public UnityEvent onButtonPressed;

    public override void OnPressed()
    {
        if(isSelected)
        {
            onButtonPressed.Invoke();
        }
    }
}
