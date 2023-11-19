using System;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager instance;

    private int pauseCounter = 0;

    public bool isPauseEnable => pauseCounter > 0;
    public Action callBackOnPauseEnable;
    public Action callBackOnPauseDisable;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(this);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        pauseCounter = 0;
    }

    private void Update()
    {
        if(InputManager.GetKeyDown(KeyCode.P))
        {
            if(isPauseEnable)
            {
                DisablePause();
            }
            else
            {
                EnablePause();
            }
        }
    }

    public void EnablePause()
    {
        pauseCounter++;
        if(pauseCounter > 0)
        {
            callBackOnPauseEnable.Invoke();
        }
    }

    public void DisablePause()
    {
        pauseCounter--;
        if(pauseCounter <= 0)
        {
            callBackOnPauseDisable.Invoke();
        }
    }
}
