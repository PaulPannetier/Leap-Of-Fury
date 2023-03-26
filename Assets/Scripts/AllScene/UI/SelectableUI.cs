using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class SelectableUI : MonoBehaviour
{
    private bool oldIsSelected = false;
    protected TextMeshProUGUI textMeshPro;

    public SelectableUI upSelectableUI;
    public SelectableUI downSelectableUI;
    public SelectableUI rightSelectableUI;
    public SelectableUI leftSelectableUI;

    public bool isSelected = false;
    public UnityEvent onPressed;

    private void Awake()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();
    }

    public void OnPressed()
    {
        if(isSelected)
        {
            onPressed.Invoke();
            isSelected = false;
        }
    }

    protected virtual void OnSelected()
    {
        textMeshPro.color = new Color(textMeshPro.color.r, textMeshPro.color.g, textMeshPro.color.b, 0.5f);
    }

    protected virtual void OnDeselected()
    {
        textMeshPro.color = new Color(textMeshPro.color.r, textMeshPro.color.g, textMeshPro.color.b, 1f);
    }

    private void Update()
    {
        if(isSelected && !oldIsSelected)
        {
            OnSelected();
        }
        if(!isSelected && !oldIsSelected)
        {
            OnDeselected();
        }
        oldIsSelected = isSelected;
    }
}
