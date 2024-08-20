#if UNITY_EDITOR

using UnityEngine;
using TMPro;

public class TestScript : MonoBehaviour
{
    public TMP_Dropdown dropdown;

    public InputKey showOption = InputKey.S;
    public InputKey hideOption = InputKey.H;

    public void Update()
    {
        if (InputManager.GetKeyDown(showOption))
        {
            print("Show");
            dropdown.Show();
        }

        if (InputManager.GetKeyDown(hideOption))
        {
            print("Hide");
            dropdown.Hide();
        }
    }

    private void OnValidate()
    {
        //if(showOption)
        //{
        //    showOption = false;
        //    dropdown.Select();
        //}

        //if(hideOption)
        //{
        //    hideOption = false;
        //    dropdown.Hide();
        //}
    }
}

#endif
