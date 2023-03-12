using UnityEngine.UI;
using UnityEngine;

public class SliderAttackHandler : MonoBehaviour
{
    private Slider weakAttackSlider, strongAttackSlider;
    private Image weakAttackSliderImage, strongAttackSliderImage;

    [SerializeField] private Gradient weakAttackGradient;
    [SerializeField] private Gradient strongAttackGradient;

    private bool _enableWeakAttack, _enableStrongAttack;
    public bool enableWeakAttack
    {
        get => _enableWeakAttack;
        set
        {
            _enableWeakAttack = value;
            weakAttackSlider.gameObject.SetActive(value);
        }
    }

    public bool enableStrongAttack
    {
        get => _enableStrongAttack;
        set
        {
            _enableStrongAttack = value;
            strongAttackSlider.gameObject.SetActive(value);
        }
    }

    private void Awake()
    {
        weakAttackSlider = transform.GetChild(0).GetComponent<Slider>();
        strongAttackSlider = transform.GetChild(1).GetComponent<Slider>();
        weakAttackSliderImage = transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<Image>();
        strongAttackSliderImage = transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<Image>();
    }

    public void SetWeakAttackSliderValue(float value)
    {
        weakAttackSlider.value = value;
        weakAttackSliderImage.color = weakAttackGradient.Evaluate(value);
    }

    public void SetStrongAttackSliderValue(float value)
    {
        strongAttackSlider.value = value;
        strongAttackSliderImage.color = strongAttackGradient.Evaluate(value);
    }
}
