using UnityEngine;

public class ButtonsFunction : MonoBehaviour
{
    public void LoadCharSelectorScene()
    {
        //TransitionManager.instance.LoadPreloadedScene("Selection Char", null);
        TransitionManager.instance.LoadScene("Selection Char", null);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
