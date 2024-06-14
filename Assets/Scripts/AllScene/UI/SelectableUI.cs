using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class SelectableUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private List<Coroutine> changeColorCorout;
    private bool isSelected = false;
    private bool isMouseOver;

    [SerializeField] private bool mouseInteractable = true;

    [SerializeField] ColorFader[] colors;

    public SelectableUI upSelectableUI;
    public SelectableUI downSelectableUI;
    public SelectableUI rightSelectableUI;
    public SelectableUI leftSelectableUI;

#if UNITY_EDITOR
    [Space, Space]
#endif
    public UnityEvent onPressed;

    [HideInInspector] public SelectableUIGroup selectableUIGroup;

    private void Awake()
    {
        changeColorCorout = new List<Coroutine>();
    }

    public void ResetToDefault()
    {
        foreach (Coroutine coroutine in changeColorCorout)
        {
            StopCoroutine(coroutine);
        }

        foreach (ColorFader fader in colors)
        {
            if(fader.text != null)
                fader.text.color = fader.normalColor;
            if (fader.image != null)
                fader.image.color = fader.normalColor;
        }

        isSelected = isMouseOver = false;
        selectableUIGroup = null;
    }

    public void OnPressed()
    {
        if(isSelected)
        {
            onPressed.Invoke();
            isSelected = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!mouseInteractable)
            return;

        isMouseOver = true;
        selectableUIGroup.RequestSelected(this);
        OnSelected();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!mouseInteractable)
            return;

        isMouseOver = false;
        selectableUIGroup.RequestDeselected(this);
        OnDeselected();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!mouseInteractable || !isMouseOver)
            return;

        foreach(Coroutine coroutine in changeColorCorout)
        {
            StopCoroutine(coroutine);
        }

        foreach (ColorFader fader in colors)
        {
            changeColorCorout.Add(StartCoroutine(ChangeColor(fader, fader.pressedColor)));
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!mouseInteractable || !isMouseOver)
            return;

        foreach (Coroutine coroutine in changeColorCorout)
        {
            StopCoroutine(coroutine);
        }

        foreach (ColorFader fader in colors)
        {
            changeColorCorout.Add(StartCoroutine(ChangeColor(fader, fader.highlightedColor)));
        }

        OnPressed();
    }

    public void OnSelected()
    {
        isSelected = true;
        foreach (Coroutine coroutine in changeColorCorout)
        {
            StopCoroutine(coroutine);
        }

        foreach (ColorFader fader in colors)
        {
            changeColorCorout.Add(StartCoroutine(ChangeColor(fader, fader.selectedColor)));
        }
    }

    public void OnDeselected()
    {
        isSelected = false;
        foreach (Coroutine coroutine in changeColorCorout)
        {
            StopCoroutine(coroutine);
        }

        foreach (ColorFader fader in colors)
        {
            changeColorCorout.Add(StartCoroutine(ChangeColor(fader, fader.normalColor)));
        }
    }

    private IEnumerator ChangeColor(ColorFader colorFader, Color target)
    {
        float timer = 0f;
        Color? textColor = colorFader.text == null ? null : colorFader.text.color;
        Color? imageColor = colorFader.image == null ? null : colorFader.image.color;

        while (timer < colorFader.fadeDuration)
        {
            yield return null;
            timer += Time.deltaTime;
            float t = timer / colorFader.fadeDuration;
            if (textColor != null)
            {
                colorFader.text.color = Color.Lerp(textColor.Value, target, t);
            }
            if(imageColor != null)
            {
                colorFader.image.color = Color.Lerp(imageColor.Value, target, t); ;
            }
        }
    }

    #region OnValidate

#if UNITY_EDITOR

    private void OnValidate()
    {
        if(colors != null)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i].fadeDuration = Mathf.Max(0f, colors[i].fadeDuration);
            }
        }
    }

#endif

    #endregion

    [System.Serializable]
    private struct ColorFader
    {
        public Color normalColor;
        public Color highlightedColor;
        public Color pressedColor;
        public Color selectedColor;
        public TextMeshProUGUI text;
        public Image image;
        public float fadeDuration;
    }
}
