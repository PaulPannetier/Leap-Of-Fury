using UnityEngine.Audio;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;
using UnityEngine.Rendering;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    private Dictionary<string, AudioSource> currentSounds;
    private Transform audioParent;
    private List<CrossFadeData> fadeData;
    private List<SingleCrossFadeData> singleFadeData;
    private Dictionary<string, Coroutine> removeCorout;

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
        fadeData = new List<CrossFadeData>();
        singleFadeData = new List<SingleCrossFadeData>();
        removeCorout = new Dictionary<string, Coroutine>();
        audioParent = transform.GetChild(1);
    }

    private void RecaculateSoundVolume()
    {
        foreach (string key in currentSounds.Keys)
        {
            Sound sound = Array.Find(audioClips, item => item.name == key);
            currentSounds[key].volume = sound.volume * masterVolume * (sound.soundEffect ? soundEffectsVolume : musicVolume);
        }
    }

    public void PlayMusic(string name, float volume)
    {
        Sound sound = Array.Find(audioClips, item => item.name == name);
        if(sound == null)
        {
            Debug.LogWarning("The audioFile " + name + " wasn't find in the AudioManager's audioClips array.");
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
            StopMusic(name, sound.audioClip.length);
        audioSource.Play();
    }

    #region CrossFade

    public void CrossFade(string currentMusicName, string newMusicName, float newMusicVolume, float fadeDuration)
    {
        if(!currentSounds.TryGetValue(newMusicName, out AudioSource currentAudioSource))
        {
            Debug.LogWarning("The music : " + currentMusicName + " is not currently playing, can't crossfade it.");
            return;
        }

        List<CrossFadeData> currentFadeDataInCurrent = new List<CrossFadeData>();
        List<CrossFadeData> currentFadeDataInNew = new List<CrossFadeData>();
        List<CrossFadeData> newFadeDataInCurrent = new List<CrossFadeData>();
        List<CrossFadeData> newFadeDataInNew = new List<CrossFadeData>();
        foreach (CrossFadeData item in fadeData)
        {
            if (item.currentMusicName == currentMusicName && item.newMusicName == newMusicName)
            {
                Debug.Log("Crossfade between " + currentSounds + " and  " + newMusicName + " is already in fading");
                return;
            }
            if (item.currentMusicName == currentMusicName)
                currentFadeDataInCurrent.Add(item);
        }

        foreach (CrossFadeData fade in currentFadeDataInCurrent)
        {
            StopCoroutine(fade.coroutine);
            for (int i = singleFadeData.Count - 1; i >= 0; i--)
            {
                if (singleFadeData[i].musicName == fade.newMusicName)
                {
                    StopCoroutine(singleFadeData[i].coroutine);
                    singleFadeData.RemoveAt(i);
                }
            }

            float duration = Mathf.Max(0f, fade.duration - (Time.time - fade.beginTime));
            Coroutine singleCorout = StartCoroutine(CrossFadeSingleCorout(currentSounds[fade.newMusicName], fade.volumeTarget, duration));
            singleFadeData.Add(new SingleCrossFadeData(fade.newMusicName, singleCorout));
            StopMusic(fade.newMusicName, duration);
            fadeData.Remove(fade);
        }

        foreach (CrossFadeData item in fadeData)
        {
            if(item.newMusicName == currentMusicName)
                currentFadeDataInNew.Add(item);
        }

        foreach (CrossFadeData fade in currentFadeDataInNew)
        {
            StopCoroutine(fade.coroutine);
            for (int i = singleFadeData.Count - 1; i >= 0; i--)
            {
                if (singleFadeData[i].musicName == fade.currentMusicName)
                {
                    StopCoroutine(singleFadeData[i].coroutine);
                    singleFadeData.RemoveAt(i);
                }
            }

            AudioSource source = currentSounds[fade.currentMusicName];
            float duration = Mathf.Max((fade.duration * (1f / (1f + fade.volumeTarget))) - (Time.time - fade.beginTime), 0f);
            StartCoroutine(CrossFadeSingleCorout(source, 0f, duration));
            StopMusic(fade.currentMusicName, duration);
            fadeData.Remove(fade);
        }

        foreach (CrossFadeData item in fadeData)
        {
            if(item.currentMusicName == newMusicName)
                newFadeDataInCurrent.Add(item);

        }

        foreach (CrossFadeData fade in newFadeDataInCurrent)
        {
            StopCoroutine(fade.coroutine);
            for (int i = singleFadeData.Count - 1; i >= 0; i--)
            {
                SingleCrossFadeData singleFade = singleFadeData[i];
                if (singleFade.musicName == fade.currentMusicName || singleFade.musicName == fade.newMusicName)
                {
                    StopCoroutine(singleFadeData[i].coroutine);
                    singleFadeData.RemoveAt(i);
                }
            }

            StopMusic(fade.currentMusicName);
            AudioSource source = currentSounds[fade.newMusicName];
            float duration = fade.duration - (Time.time - fade.beginTime);
            StartCoroutine(CrossFadeSingleCorout(source, fade.volumeTarget, duration));
            fadeData.Remove(fade);
        }

        foreach (CrossFadeData item in fadeData)
        {
            if(item.newMusicName == newMusicName)
                newFadeDataInNew.Add(item);
        }

        foreach (CrossFadeData fade in newFadeDataInNew)
        {
            StopCoroutine(fade.coroutine);
            for (int i = singleFadeData.Count - 1; i >= 0; i--)
            {
                SingleCrossFadeData singleFade = singleFadeData[i];
                if (singleFade.musicName == fade.currentMusicName || singleFade.musicName == fade.newMusicName)
                {
                    StopCoroutine(singleFadeData[i].coroutine);
                    singleFadeData.RemoveAt(i);
                }
            }

            StopMusic(fade.newMusicName);
            AudioSource source = currentSounds[fade.currentMusicName];
            float duration = Mathf.Max((fade.duration * (1f / (1f + fade.volumeTarget))) - (Time.time - fade.beginTime), 0f);
            StartCoroutine(CrossFadeSingleCorout(source, 0f, duration));
            StopMusic(fade.currentMusicName, duration);
            fadeData.Remove(fade);
        }

        fadeDuration = Mathf.Max(fadeDuration, 0f);
        Sound sound = Array.Find(audioClips, item => item.name == newMusicName);
        float targetVoume = Mathf.Clamp01(newMusicVolume) * sound.volume * masterVolume * (sound.soundEffect ? soundEffectsVolume : musicVolume);
        PlayMusic(newMusicName, 0f);
        AudioSource newAudioSource = currentSounds[newMusicName];

        Coroutine corout = StartCoroutine(CrossFadeCorout(currentAudioSource, newAudioSource, targetVoume, fadeDuration));
        fadeData.Add(new CrossFadeData(currentMusicName, newMusicName, targetVoume, fadeDuration, Time.time, corout));
        StopMusic(currentMusicName, fadeDuration * (currentAudioSource.volume / (currentAudioSource.volume + targetVoume)));
    }

    private IEnumerator CrossFadeCorout(AudioSource currentSource, AudioSource newSource, float targetVolume, float duration)
    {
        float time = Time.time;
        float volume = currentSource.volume;
        float firstDuration = (volume / (volume + targetVolume)) * duration;
        while(Time.time - time < firstDuration)
        {
            if(currentSource.gameObject == null)
                break;

            currentSource.volume = Mathf.Lerp(volume, 0f, (Time.time - time) / firstDuration);
            yield return null;
        }

        if (currentSource.gameObject != null)
        {
            currentSource.volume = 0f;
            Destroy(currentSource);
        }

        firstDuration = duration - firstDuration;
        time = Time.time;
        volume = newSource.volume;
        while(Time.time - time < firstDuration)
        {
            if (newSource.gameObject == null)
                break;
            newSource.volume = Mathf.Lerp(volume, targetVolume, (Time.time - time) / firstDuration);
            yield return null;
        }

        if (newSource.gameObject != null)
            newSource.volume = targetVolume;
    }

    private IEnumerator CrossFadeSingleCorout(AudioSource source, float target, float duration)
    {
        float volume = source.volume;
        float time = Time.time;
        while (Time.time - time < duration)
        {
            if(source.gameObject == null)
            {
                break;
            }
            source.volume = Mathf.Lerp(volume, target, (Time.time - time) / duration);
            yield return null;
        }
        if (source.gameObject != null)
            source.volume = target;
    }

    #endregion

    public bool IsPlayingMusic(string musicName) => currentSounds.ContainsKey(musicName);

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

    public void MuteMusic(string name)
    {
        if (currentSounds.TryGetValue(name, out AudioSource audioSource))
        {
            audioSource.mute = true;
            return;
        }
        Debug.LogWarning("The sound " + name + "is not currently playing.");
    }

    public void UnMuteMusic(string name)
    {
        if (currentSounds.TryGetValue(name, out AudioSource audioSource))
        {
            audioSource.mute = false;
            return;
        }
        Debug.LogWarning("The sound " + name + "is not currently playing.");
    }

    public void PauseMusic(string name)
    {
        if (currentSounds.TryGetValue(name, out AudioSource audioSource))
        {
            audioSource.Pause();
            return;
        }
        Debug.LogWarning("The sound " + name + "is not currently playing.");
    }

    public void ResumeMusic(string name)
    {
        if (currentSounds.TryGetValue(name, out AudioSource audioSource))
        {
            audioSource.UnPause();
            return;
        }
        Debug.LogWarning("The sound " + name + "is not currently playing.");
    }

    public void StopAllMusic()
    {
        List<string> keys = new List<string>(currentSounds.Keys);
        foreach (string key in keys)
        {
            StopMusic(key);
        }
    }

    public void StopAllMusic(float delay = 0f)
    {
        List<string> keys = new List<string>(currentSounds.Keys);
        foreach (string key in keys)
        {
            StopMusic(key, delay);
        }
    }

    public void StopMusic(string name)
    {
        RmMusic(name);
    }

    public void StopMusic(string name, float delay = 0f)
    {
        if (Mathf.Approximately(delay, 0f))
        {
            RmMusic(name);
            return;
        }
        removeCorout.Add(name, StartCoroutine(RemoveMusicCoroutine(name, delay)));
    }

    private void StopSmooth(string name, float duration)
    {
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
        if (currentSounds.TryGetValue(name, out AudioSource audioSource))
        {
            audioSource.mute = true;
            Destroy(audioSource.gameObject);
            currentSounds.Remove(name);
            if(removeCorout.TryGetValue(name, out Coroutine coroutine))
            {
                StopCoroutine(coroutine);
                removeCorout.Remove(name);
            }
            return;
        }
        Debug.LogWarning("The sound " + name + "is not currently playing.");
    }

    private IEnumerator RemoveMusicCoroutine(string name, float delay)
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

    private class CrossFadeData
    {
        public string currentMusicName, newMusicName;
        public float volumeTarget, duration, beginTime;
        public Coroutine coroutine;

        public CrossFadeData(string currentMusicName, string newMusicName, float volumeTarget, float duration, float beginTime, Coroutine coroutine)
        {
            this.currentMusicName = currentMusicName;
            this.newMusicName = newMusicName;
            this.volumeTarget = volumeTarget;
            this.duration = duration;
            this.beginTime = beginTime;
            this.coroutine = coroutine;
        }
    }

    private class SingleCrossFadeData
    {
        public string musicName;
        public Coroutine coroutine;

        public SingleCrossFadeData(string musicName, Coroutine coroutine)
        {
            this.musicName = musicName;
            this.coroutine = coroutine;
        }
    }
}
