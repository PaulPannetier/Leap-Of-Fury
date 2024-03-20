using UnityEngine;

public class AudioContent : MonoBehaviour
{
    [HideInInspector] public uint soundId;

    private void OnDestroy()
    {
        AudioManager.instance.OnMusicDestroy(soundId);
    }
}
