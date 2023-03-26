using UnityEngine;

public class DisableComponentAtPause : MonoBehaviour
{
    [SerializeField] private Behaviour[] componentsToDisable;

    private void Start()
    {
        PauseManager.instance.callBackOnPauseDisable += Enable;
        PauseManager.instance.callBackOnPauseEnable += Disable;
    }

    private void Disable()
    {
        foreach (Behaviour comp in componentsToDisable)
        {
            comp.enabled = false;
        }
    }

    private void Enable()
    {
        foreach (Behaviour comp in componentsToDisable)
        {
            comp.enabled = true;
        }
    }

    private void OnDestroy()
    {
        PauseManager.instance.callBackOnPauseEnable -= Disable;
        PauseManager.instance.callBackOnPauseDisable -= Enable;
    }
}
