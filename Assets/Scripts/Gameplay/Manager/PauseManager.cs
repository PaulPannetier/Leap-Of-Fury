using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager instance;

    private int pauseCounter = 0;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(this);
            return;
        }
        instance = this;
    }

    public void EnablePause()
    {
        pauseCounter++;
        if(pauseCounter > 0)
        {
            Time.timeScale = 0f;
        }
    }

    public void DisablePause()
    {
        pauseCounter--;
        if(pauseCounter <= 0)
        {
            Time.timeScale = 1f;
        }
    }
}
