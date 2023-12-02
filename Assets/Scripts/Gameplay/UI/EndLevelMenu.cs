using System;
using UnityEngine;

public class EndLevelMenu : MonoBehaviour
{
    public void DisplayEndLevelMenu(Action callbackEnd)
    {
        callbackEnd.Invoke();
    }
}
