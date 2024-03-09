using UnityEngine;

public class AudioContent : MonoBehaviour
{
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnDestroy()
    {
        AudioManager.instance.OnMusicDestroy(AudioManager.instance.GetSoundName(audioSource.clip.name));
    }
}
