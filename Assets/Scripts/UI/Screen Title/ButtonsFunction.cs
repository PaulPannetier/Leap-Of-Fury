using UnityEngine;

public class ButtonsFunction : MonoBehaviour
{
    [SerializeField] private SettingMenu settingMenu;

    public void LoadCharSelectorScene()
    {
        TransitionManager.instance.LoadSceneAsync("Selection Char", new OldSceneData("Screen Title"));
    }

    public void LoadOption()
    {
        settingMenu.gameObject.SetActive(true);
    }

    public void LoadCredit()
    {

    }

    public void Quit()
    {
        Application.Quit();
    }
}
