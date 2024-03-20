using UnityEngine;
using System.Collections;

public class CharAudioPlayer : MonoBehaviour
{
    public void Play(string soundName)
    {
        uint id = AudioManager.instance.PlaySound(soundName, 1f);
        StartCoroutine(HandleMusic(soundName, id));
    }

    private IEnumerator HandleMusic(string soundName, uint id)
    {
        float duration = AudioManager.instance.GetSoundLength(soundName);
        float timer = 0f;

        while(timer < duration)
        {
            yield return null;
            timer += Time.deltaTime;

            if(PauseManager.instance.isPauseEnable)
            {
                AudioManager.instance.PauseSound(id);
                while(PauseManager.instance.isPauseEnable)
                {
                    yield return null;
                }
                AudioManager.instance.ResumeSound(id);
            }
        }

    }
}
