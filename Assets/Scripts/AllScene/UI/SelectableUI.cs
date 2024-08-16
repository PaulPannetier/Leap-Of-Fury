using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public abstract class SelectableUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private List<Coroutine> changeColorCorout;
    private bool isMouseOver;

    protected bool isSelected = false;

    [SerializeField] protected ColorFader[] colors;

    public SelectableUI upSelectableUI;
    public SelectableUI downSelectableUI;
    public SelectableUI rightSelectableUI;
    public SelectableUI leftSelectableUI;

#if UNITY_EDITOR
    [Space, Space]
#endif
    [HideInInspector] public SelectableUIGroup selectableUIGroup;

    private bool _isMouseInteractable = true;
    public virtual bool isMouseInteractable
    {
        get => _isMouseInteractable;
        set
        {
            _isMouseInteractable = value;
        }
    }

    public abstract void OnPressed();

    private void Awake()
    {
        changeColorCorout = new List<Coroutine>();
    }

    protected virtual void Start()
    {

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

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isMouseInteractable)
            return;

        isMouseOver = true;
        selectableUIGroup.RequestSelected(this);
        OnSelected();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isMouseInteractable)
            return;

        isMouseOver = false;
        selectableUIGroup.RequestDeselected(this);
        OnDeselected();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isMouseInteractable || !isMouseOver)
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
        if (!isMouseInteractable || !isMouseOver)
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

    protected virtual void OnValidate()
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
    protected struct ColorFader
    {
        public Color normalColor;
        public Color highlightedColor;
        public Color pressedColor;
        public Color selectedColor;
        public TextMeshProUGUI text;
        public Image image;
        public float fadeDuration;

        public ColorFader(in Color normalColor, in Color highlightedColor, in Color pressedColor, in Color selectedColor, TextMeshProUGUI text, Image image, float fadeDuration)
        {
            this.normalColor = normalColor;
            this.highlightedColor = highlightedColor;
            this.pressedColor = pressedColor;
            this.selectedColor = selectedColor;
            this.text = text;
            this.image = image;
            this.fadeDuration = fadeDuration;
        }
    }
}
