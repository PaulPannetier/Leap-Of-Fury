using UnityEngine;

public class ButtonsFunction : MonoBehaviour
{
    public void LoadCharSelectorScene()
    {
        TransitionManager.instance.LoadPreloadedScene("Selection Char", null);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
