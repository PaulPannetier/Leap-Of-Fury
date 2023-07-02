using TMPro;
using UnityEngine;

public class ControlItem : MonoBehaviour
{
    private TextMeshProUGUI nameText, keyText;

    private void Awake()
    {
        nameText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        keyText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
    }

    public void SetNameText(string text)
    {
        nameText.text = text;
    }

    public void SetKeyText(string key)
    {
        keyText.text = key;    
    }

    private void Update()
    {
        
    }
}
