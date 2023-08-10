using UnityEngine.Audio;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    private Dictionary<string, AudioSource> currentSounds;
    private Dictionary<string, Coroutine> removeCorout;
    private Dictionary<string, Coroutine> changeVolumeCorout;

    [SerializeField] private Transform audioParent;
    [SerializeField] [Range(0f, 1f)] private float _masterVolume = 1f, _musicVolume = 1f, _soundEffectsVolume = 1f;
    public float masterVolume { get => _masterVolume; set { _masterVolume = Mathf.Clamp01(value); RecaculateSoundVolume(); } }
    public float musicVolume { get => _musicVolume; set { _musicVolume = Mathf.Clamp01(value); RecaculateSoundVolume(); } }
    public float soundEffectsVolume { get => _soundEffectsVolume; set { _soundEffectsVolume = Mathf.Clamp01(value); RecaculateSoundVolume(); } }

    [SerializeField] private Sound[] audioClips;
    [SerializeField] private AudioSource musicSourcePrefab, soundEffectSourcePrefab;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(this);
            return;
        }
        instance = this;
        currentSounds = new Dictionary<string, AudioSource>();
        removeCorout = new Dictionary<string, Coroutine>();
        changeVolumeCorout = new Dictionary<string, Coroutine>();
    }

    private void RecaculateSoundVolume()
    {
        foreach (string key in currentSounds.Keys)
        {
            Sound sound = Array.Find(audioClips, item => item.name == key);
            currentSounds[key].volume = sound.volume * masterVolume * (sound.soundEffect ? soundEffectsVolume : musicVolume);
        }
    }

    public void PlaySound(string name, float volume)
    {
        Sound sound = Array.Find(audioClips, item => item.name == name);
        if(sound == null)
        {
            Debug.LogWarning("The sound " + name + " wasn't find in the AudioManager's audioClips array.");
            return;
        }
        AudioSource audioSource = Instantiate(sound.soundEffect ? soundEffectSourcePrefab : musicSourcePrefab, audioParent);
        audioSource.clip = sound.audioClip;
        audioSource.pitch = sound.pitch;
        audioSource.loop = sound.loop;
        volume = Mathf.Clamp01(volume);
        audioSource.volume = volume * sound.volume * masterVolume * (sound.soundEffect ? soundEffectsVolume : musicVolume);
        currentSounds.Add(name, audioSource);
        if (!sound.loop)
            StopSound(name, sound.audioClip.length);
        audioSource.Play();
    }

    #region CrossFade

    public void CrossFade(string currentSoundName, string newSoundName, float newSoundVolume, float duration)
    {
        if(!IsPlayingSound(currentSoundName))
        {
            Debug.Log("The sound " + currentSoundName + " is not playing, can't crossfade!");
            PlaySound(newSoundName, 0f);
            SetVolumeSmooth(newSoundName, newSoundVolume, duration);
            return;
        }
        StartCoroutine(CrossFadeCorout(currentSoundName, currentSounds[currentSoundName], newSoundName, newSoundVolume, duration));
    }

    private IEnumerator CrossFadeCorout(string currentSoundName, AudioSource currentSource, string newSoundName, float targetVolume, float duration)
    {
        float duration1 = duration * (currentSource.volume / (currentSource.volume + targetVolume));
        Coroutine changeVolCorout = StartCoroutine(SetVolumeSmoothCorout(currentSoundName, currentSource, 0f, duration1));
        changeVolumeCorout.Add(currentSoundName, changeVolCorout);
        yield return Useful.GetWaitForSeconds(duration1);
        RmMusic(currentSoundName);

        PlaySound(newSoundName, 0f);
        Coroutine changeVolCorout2 = StartCoroutine(SetVolumeSmoothCorout(newSoundName, currentSounds[newSoundName], targetVolume, duration - duration1));
        changeVolumeCorout.Add(newSoundName, changeVolCorout2);
    }

    #endregion

    public bool IsPlayingSound(string name) => currentSounds.ContainsKey(name);

    public void SetVolume(string name, float newVolume)
    {
        if (currentSounds.TryGetValue(name, out AudioSource audioSource))
        {
            Sound sound = Array.Find(audioClips, item => item.name == name);
            audioSource.volume = Mathf.Clamp01(newVolume) * sound.volume * masterVolume * (sound.soundEffect ? soundEffectsVolume : musicVolume);
            return;
        }
        Debug.LogWarning("The sound " + name + "is not currently playing.");
    }

    public void SetVolumeSmooth(string name, float newVolume, float duration)
    {
        if(!currentSounds.TryGetValue(name, out AudioSource audioSource))
        {
            Debug.Log("The sound : " + name + " is not playing, can't set volume of a non playing sound.");
            return;
        }
        if(changeVolumeCorout.ContainsKey(name))
        {
            Debug.Log("The sound : " + name + " is already changing volume smootly.");
            StopCoroutine(changeVolumeCorout[name]);
            changeVolumeCorout.Remove(name);
        }
        Coroutine changeVolCorout = StartCoroutine(SetVolumeSmoothCorout(name, audioSource, Mathf.Clamp01(newVolume), Mathf.Max(duration, 0f)));
        changeVolumeCorout.Add(name, changeVolCorout);
    }

    private IEnumerator SetVolumeSmoothCorout(string name, AudioSource source, float targetVolume, float duration)
    {
        float volume = source.volume;
        float time = Time.time;
        while(Time.time - time < duration)
        {
            if(source.gameObject == null)
                break;
            source.volume = Mathf.Lerp(volume, targetVolume, (Time.time - time) / duration);
            yield return null;
        }
        if (source != null)
            source.volume = targetVolume;
        changeVolumeCorout.Remove(name);
    }

    public void MuteSound(string name)
    {
        if (currentSounds.TryGetValue(name, out AudioSource audioSource))
        {
            audioSource.mute = true;
            return;
        }
        Debug.LogWarning("The sound " + name + "is not currently playing, can't mute them.");
    }

    public void UnMuteSound(string name)
    {
        if (currentSounds.TryGetValue(name, out AudioSource audioSource))
        {
            audioSource.mute = false;
            return;
        }
        Debug.LogWarning("The sound " + name + "is not currently playing, can't unmute them.");
    }

    public void PauseSound(string name)
    {
        if (currentSounds.TryGetValue(name, out AudioSource audioSource))
        {
            audioSource.Pause();
            return;
        }
        Debug.LogWarning("The sound " + name + "is not currently playing, can't pause them.");
    }

    public void ResumeSound(string name)
    {
        if (currentSounds.TryGetValue(name, out AudioSource audioSource))
        {
            audioSource.UnPause();
            return;
        }
        Debug.LogWarning("The sound " + name + "is not currently playing, can't resume them.");
    }

    public void StopAllSound()
    {
        List<string> keys = new List<string>(currentSounds.Keys);
        foreach (string key in keys)
        {
            StopSound(key);
        }
    }

    public void StopAllSound(float delay)
    {
        List<string> keys = new List<string>(currentSounds.Keys);
        foreach (string key in keys)
        {
            StopSound(key, delay);
        }
    }

    public void StopSound(string name)
    {
        RmMusic(name);
    }

    public void StopSound(string name, float delay)
    {
        if (Mathf.Approximately(delay, 0f))
        {
            RmMusic(name);
            return;
        }

        if(removeCorout.TryGetValue(name, out Coroutine c))
        {
            StopCoroutine(c);
            removeCorout.Remove(name);
        }
        removeCorout.Add(name, StartCoroutine(RemoveSoundCoroutine(name, delay)));
    }

    public void StopSmooth(string name, float duration)
    {
        if(removeCorout.TryGetValue(name, out Coroutine c))
        {
            StopCoroutine(c);
            removeCorout.Remove(name);
        }

        if (currentSounds.TryGetValue(name, out AudioSource audioSource))
        {
            removeCorout.Add(name, StartCoroutine(StopSmoothCorout(audioSource, duration)));
        }
    }

    private IEnumerator StopSmoothCorout(AudioSource audioSource, float duration)
    {
        float time = Time.time;
        float volume = audioSource.volume;
        while(Time.time - time < duration)
        {
            audioSource.volume = Mathf.Lerp(volume, 0f, (Time.time - time) / duration);
            yield return null;
        }
        RmMusic(name);
    }

    private void RmMusic(string name)
    {
        if (!currentSounds.TryGetValue(name, out AudioSource audioSource))
            return;
        audioSource.mute = true;
        Destroy(audioSource.gameObject);
        currentSounds.Remove(name);
        if (removeCorout.TryGetValue(name, out Coroutine coroutine))
        {
            StopCoroutine(coroutine);
            removeCorout.Remove(name);
        }
    }

    private IEnumerator RemoveSoundCoroutine(string name, float delay)
    {
        yield return Useful.GetWaitForSeconds(delay);
        RmMusic(name);
    }

    public void OnValidate()
    {
        if(currentSounds != null)
            RecaculateSoundVolume();
    }

    [Serializable]
    private class Sound
    {
        public string name;
        public AudioClip audioClip;
        [Range(0f, 1f)] public float volume;
        [Range(0f, 3f)] public float pitch;
        public bool loop;
        public bool soundEffect;
    }
}
