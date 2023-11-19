using UnityEngine;

public class LevelMusic : MonoBehaviour
{
    [SerializeField] private bool enable = true;
    [SerializeField] private string musicName;
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 1f;

    private void Start()
    {
        if(enable && !AudioManager.instance.IsPlayingSound(musicName))
        {
            AudioManager.instance.StopAllSound();
            AudioManager.instance.PlaySound(musicName, musicVolume);
        }
    }
}
