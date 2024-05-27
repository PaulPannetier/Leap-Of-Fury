using System;
using System.Collections;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager instance;

    private int pauseCounter = 0;
    private bool _isPauseEnableThisFrame, _isPauseDisableThisFrame;

    public bool isPauseEnable => pauseCounter > 0;
    public bool isPauseEnableThisFrame {  get; private set; }
    public bool isPauseDisableThisFrame { get; private set; }
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
        callBackOnPauseEnable = new Action(() => { });
        callBackOnPauseDisable = new Action(() => { });
    }

    private void Start()
    {
        pauseCounter = 0;
        EventManager.instance.callbackPreUpdate += PreUpdate;
    }

    private void PreUpdate()
    {
        isPauseEnableThisFrame = _isPauseEnableThisFrame;
        isPauseDisableThisFrame = _isPauseDisableThisFrame;
        _isPauseEnableThisFrame = _isPauseDisableThisFrame = false; ;
    }

    public void EnablePause()
    {
        pauseCounter++;
        if(pauseCounter > 0)
        {
            callBackOnPauseEnable.Invoke();
            _isPauseEnableThisFrame = true;
        }
    }

    public void DisablePause()
    {
        pauseCounter--;
        if(pauseCounter <= 0)
        {
            callBackOnPauseDisable.Invoke();
            _isPauseDisableThisFrame = true;
        }
    }

    public IEnumerator Wait(float duration)
    {
        float timeCounter = 0f;
        while (timeCounter < duration)
        {
            yield return null;
            if (!isPauseEnable)
            {
                timeCounter += Time.deltaTime;
            }
        }
    }

    private void OnDestroy()
    {
        EventManager.instance.callbackPreUpdate -= PreUpdate;
    }
}
