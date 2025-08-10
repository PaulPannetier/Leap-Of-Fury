using UnityEngine;

public class LevelMusic : MonoBehaviour
{
    [SerializeField] private bool enable = true;
    [SerializeField] private string musicName;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float startVolume = 0.3f;
    [SerializeField] private float volumeFadeDuration = 1f;

    private void Start()
    {
        if(enable)
        {
            if (!AudioManager.instance.IsPlayingSound(musicName))
            {
                AudioManager.instance.StopAllSound();
                AudioManager.instance.PlaySound(musicName, startVolume);
                AudioManager.instance.SetVolumeSmooth(musicName, musicVolume, volumeFadeDuration);
            }

            EventManager.instance.callbackOnLevelFinish += OnLevelFinish;
        }
    }

    private void OnLevelFinish(LevelManager.FinishLevelData finishLevelData)
    {
        AudioManager.instance.StopSmooth(musicName, volumeFadeDuration);
    }

    private void OnDestroy()
    {
        EventManager.instance.callbackOnLevelFinish -= OnLevelFinish;
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        volumeFadeDuration = Mathf.Max(0f, volumeFadeDuration);
    }

#endif
}
