using UnityEngine.Audio;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    private Dictionary<string, AudioSource> currentSounds;
    private Transform audioParent;

    [SerializeField] [Range(0f, 1f)] private float _masterVolume = 1f, _musicVolume = 1f, _soundEffectsVoume = 1f;
    public float masterVolume { get => _masterVolume; set { _masterVolume = Mathf.Clamp01(value); RecaculateSoundVolume(); } }
    public float musicVolume { get => _musicVolume; set { _musicVolume = Mathf.Clamp01(value); RecaculateSoundVolume(); } }
    public float soundEffectsVoume { get => _soundEffectsVoume; set { _soundEffectsVoume = Mathf.Clamp01(value); RecaculateSoundVolume(); } }

    [SerializeField] private Sound[] audioClips;
    [SerializeField] private GameObject musicSourcePrefab, soundEffectSourcePrefab;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(this);
            return;
        }
        instance = this;
        currentSounds = new Dictionary<string, AudioSource>();
        audioParent = transform.GetChild(1);
    }

    private void RecaculateSoundVolume()
    {
        foreach (string key in currentSounds.Keys)
        {
            Sound sound = Array.Find(audioClips, item => item.name == key);
            currentSounds[key].volume = sound.volume * masterVolume * (sound.soundEffect ? soundEffectsVoume : musicVolume);
        }
    }

    public void PlayMusic(string name, in float volume)
    {
        Sound sound = Array.Find(audioClips, item => item.name == name);
        if(sound == null)
        {
            Debug.LogWarning("The audioFile " + name + " wasn't find in the AudioManager's audioClips array.");
            return;
        }
        GameObject audioSourceGO = Instantiate(sound.soundEffect ? soundEffectSourcePrefab : musicSourcePrefab, audioParent);
        AudioSource audioSource = audioSourceGO.GetComponent<AudioSource>();
        audioSource.clip = sound.audioClip;
        audioSource.pitch = sound.pitch;
        audioSource.loop = sound.loop;
        audioSource.volume = sound.volume * masterVolume * (sound.soundEffect ? soundEffectsVoume : musicVolume);
        currentSounds.Add(name, audioSource);
        if (!sound.loop)
            RemoveMusic(name, sound.audioClip.length);
        audioSource.Play();
    }

    public bool IsPlayingMusic(string musicName) => currentSounds.ContainsKey(musicName);

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

    public void RemoveAllMusic(in float delay = 0f)
    {
        List<string> keys = new List<string>(currentSounds.Keys);

        foreach (string key in keys)
        {
            RemoveMusic(key, delay);
        }
    }

    public void RemoveMusic(string name, in float delay = 0f)
    {
        if (Mathf.Abs(delay) < Mathf.Epsilon)
        {
            RmMusic(name);
            return;
        }
        StartCoroutine(RemoveMusicCoroutine(name, delay));
    }

    private void RmMusic(string name)
    {
        if (currentSounds.TryGetValue(name, out AudioSource audioSource))
        {
            audioSource.mute = true;
            Destroy(audioSource.gameObject);
            currentSounds.Remove(name);
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
}
