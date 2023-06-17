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
        TransitionManager.instance.LoadSceneAsync("Selection Char", null);
    }

    public void LoadOption()
    {
        selectableUIGroup.enableBehaviour = false;
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
