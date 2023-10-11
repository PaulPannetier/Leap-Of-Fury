using UnityEngine;

public class ButtonsFunction : MonoBehaviour
{
    private SelectableUIGroup selectableUIGroup;

    [SerializeField] private SettingMenu settingMenu;

    private void Awake()
    {
        selectableUIGroup = GetComponent<SelectableUIGroup>();
    }

    public void LoadCharSelectorScene()
    {
        TransitionManager.instance.LoadSceneAsync("Selection Char", new OldSceneData("Screen Title"));
    }

    public void LoadOption()
    {
        selectableUIGroup.enableBehaviour = false;
        settingMenu.gameObject.SetActive(true);
        settingMenu.OnEnableOptionMenu();
    }

    public void LoadCredit()
    {

    }

    public void Quit()
    {
        Application.Quit();
    }
}
