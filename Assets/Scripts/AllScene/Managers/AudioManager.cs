using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    private Dictionary<SoundID, AudioSource> currentSounds;
    private Dictionary<SoundID, Coroutine> removeCorout;
    private Dictionary<SoundID, Coroutine> changeVolumeCorout;

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
        currentSounds = new Dictionary<SoundID, AudioSource>();
        removeCorout = new Dictionary<SoundID, Coroutine>();
        changeVolumeCorout = new Dictionary<SoundID, Coroutine>();
    }

    private void RecaculateSoundVolume()
    {
        foreach (SoundID key in currentSounds.Keys)
        {
            Sound sound = Array.Find(audioClips, item => item.name == key.name);
            currentSounds[key].volume = sound.volume * masterVolume * (sound.soundEffect ? soundEffectsVolume : musicVolume);
        }
    }

    private SoundID GetNewId(string name)
    {
        SoundID.ids++;
        return new SoundID(name, SoundID.ids);
    }

    private SoundID GetIdFromName(string name)
    {
        foreach (SoundID soundID in currentSounds.Keys)
        {
            if(soundID.name == name)
                return soundID;
        }
        return default(SoundID);
    }

    private SoundID GetSoundID(uint id)
    {
        foreach (SoundID soundID in currentSounds.Keys)
        {
            if (soundID.id == id)
                return soundID;
        }
        return default(SoundID);
    }

    public uint PlaySound(string name, float volume) => PlaySoundInternal(name, volume).id;

    private SoundID PlaySoundInternal(string name, float volume)
    {
        Sound sound = Array.Find(audioClips, item => item.name == name);
        if (sound == null)
        {
            string errorMsg = "The sound " + name + " wasn't find in the AudioManager's audioClips array.";
            Debug.LogWarning(errorMsg);
            LogManager.instance.AddLog(errorMsg, new object[] { name, audioClips.Length });
            return default(SoundID);
        }
        AudioSource audioSource = Instantiate(sound.soundEffect ? soundEffectSourcePrefab : musicSourcePrefab, audioParent);
        audioSource.clip = sound.audioClip;
        audioSource.pitch = sound.pitch;
        audioSource.loop = sound.loop;
        volume = Mathf.Clamp01(volume);
        audioSource.volume = volume * sound.volume * masterVolume * (sound.soundEffect ? soundEffectsVolume : musicVolume);
        SoundID id = GetNewId(name);
        currentSounds.Add(id, audioSource);
        audioSource.GetComponent<AudioContent>().soundId = id.id;
        if (!sound.loop)
            StopSound(id, sound.audioClip.length);
        audioSource.Play();
        return id;
    }

    #region CrossFade

    public void CrossFade(string currentSoundName, string newSoundName, float newSoundVolume, float duration)
    {
        CrossFade(GetIdFromName(currentSoundName), newSoundName, newSoundVolume, duration);
    }

    public void CrossFade(uint currentSoundId, string newSoundName, float newSoundVolume, float duration)
    {
        CrossFade(GetSoundID(currentSoundId), newSoundName, newSoundVolume, duration);
    }

    private void CrossFade(in SoundID currentSoundId, string newSoundName, float newSoundVolume, float duration)
    {
        if(!IsPlayingSound(currentSoundId))
        {
            string errorMsg = "The sound " + currentSoundId.name + " (id:" + currentSoundId.id + ") is not playing, can't crossfade with the sound : " + newSoundName;
            Debug.Log(errorMsg);
            LogManager.instance.AddLog(errorMsg, new object[] { currentSoundId.name, newSoundName });
            PlaySound(newSoundName, 0f);
            SetVolumeSmooth(newSoundName, newSoundVolume, duration);
            return;
        }
        StartCoroutine(CrossFadeCorout(currentSoundId, currentSounds[currentSoundId], newSoundName, newSoundVolume, duration));
    }

    private IEnumerator CrossFadeCorout(SoundID currentSoundId, AudioSource currentSource, string newSoundName, float targetVolume, float duration)
    {
        float duration1 = duration * (currentSource.volume / (currentSource.volume + targetVolume));
        Coroutine changeVolCorout = StartCoroutine(SetVolumeSmoothCorout(currentSoundId, currentSource, 0f, duration1));
        changeVolumeCorout.Add(currentSoundId, changeVolCorout);
        yield return new WaitForSeconds(duration1);
        RmMusic(currentSoundId);

        SoundID newSoundId = PlaySoundInternal(newSoundName, 0f);
        Coroutine changeVolCorout2 = StartCoroutine(SetVolumeSmoothCorout(newSoundId, currentSounds[newSoundId], targetVolume, duration - duration1));
        changeVolumeCorout.Add(newSoundId, changeVolCorout2);
    }

    #endregion

    public bool IsPlayingSound(string name) => IsPlayingSound(GetIdFromName(name));
    public bool IsPlayingSound(uint id) => IsPlayingSound(GetSoundID(id));
    private bool IsPlayingSound(in SoundID id) => currentSounds.ContainsKey(id);

    public void SetVolume(string name, float newVolume) => SetVolume(GetIdFromName(name), newVolume);
    public void SetVolume(uint id, float newVolume) => SetVolume(GetSoundID(id), newVolume);
    private void SetVolume(SoundID id, float newVolume)
    {
        if (currentSounds.TryGetValue(id, out AudioSource audioSource))
        {
            Sound sound = Array.Find(audioClips, item => item.name == id.name);
            audioSource.volume = Mathf.Clamp01(newVolume) * sound.volume * masterVolume * (sound.soundEffect ? soundEffectsVolume : musicVolume);
            return;
        }
        string errorMsg = "The sound " + id + " is not currently playing.";
        Debug.LogWarning(errorMsg);
        LogManager.instance.AddLog(errorMsg, new object[] { id });
    }

    public void SetVolumeSmooth(string name, float newVolume, float duration) => SetVolumeSmooth(GetIdFromName(name), newVolume, duration);
    public void SetVolumeSmooth(uint id, float newVolume, float duration) => SetVolumeSmooth(GetSoundID(id), newVolume, duration);
    private void SetVolumeSmooth(SoundID id, float newVolume, float duration)
    {
        if(!currentSounds.TryGetValue(id, out AudioSource audioSource))
        {
            string errorMsg = $"The sound : {id} is not playing, can't set volume of a non playing sound.";
            Debug.Log(errorMsg);
            LogManager.instance.AddLog(errorMsg, new object[] { id });
            return;
        }
        if(changeVolumeCorout.ContainsKey(id))
        {
            string errorMsg = $"The sound : {id} is already changing volume smoothly.";
            Debug.Log(errorMsg);
            LogManager.instance.AddLog(errorMsg, new object[] { id });
            StopCoroutine(changeVolumeCorout[id]);
            changeVolumeCorout.Remove(id);
        }

        Sound sound = Array.Find(audioClips, item => item.name == id.name);
        newVolume = Mathf.Clamp01(newVolume) * masterVolume * (sound.soundEffect? soundEffectsVolume : musicVolume);
        Coroutine changeVolCorout = StartCoroutine(SetVolumeSmoothCorout(id, audioSource, newVolume, Mathf.Max(duration, 0f)));
        changeVolumeCorout.Add(id, changeVolCorout);
    }

    private IEnumerator SetVolumeSmoothCorout(SoundID id, AudioSource source, float targetVolume, float duration)
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
        changeVolumeCorout.Remove(id);
    }

    public void MuteSound(string name) => MuteSound(GetIdFromName(name));
    public void MuteSound(uint id) => MuteSound(GetSoundID(id));
    private void MuteSound(in SoundID id)
    {
        if (currentSounds.TryGetValue(id, out AudioSource audioSource))
        {
            audioSource.mute = true;
            return;
        }
        string errorMsg = $"The sound {id} is not currently playing, can't mute them.";
        Debug.LogWarning(errorMsg);
        LogManager.instance.AddLog(errorMsg, new object[] { id });
    }

    public void UnMuteSound(string name) => UnMuteSound(GetIdFromName(name));
    public void UnMuteSound(uint id) => UnMuteSound(GetSoundID(id));
    private void UnMuteSound(in SoundID id)
    {
        if (currentSounds.TryGetValue(id, out AudioSource audioSource))
        {
            audioSource.mute = false;
            return;
        }
        string errorMsg = "The sound " + id + "is not currently playing, can't unmute them.";
        Debug.LogWarning(errorMsg);
        LogManager.instance.AddLog(errorMsg, new object[] { id });
    }

    public void PauseSound(string name) => PauseSound(GetIdFromName(name));
    public void PauseSound(uint id) => PauseSound(GetSoundID(id));
    private void PauseSound(in SoundID id)
    {
        if (currentSounds.TryGetValue(id, out AudioSource audioSource))
        {
            audioSource.Pause();
            return;
        }
        string errorMsg = "The sound " + id + "is not currently playing, can't pause them.";
        Debug.LogWarning(errorMsg);
        LogManager.instance.AddLog(errorMsg, new object[] { id });
    }

    public void ResumeSound(string id) => ResumeSound(GetIdFromName(id));
    public void ResumeSound(uint id) => ResumeSound(GetSoundID(id));
    private void ResumeSound(in SoundID id)
    {
        if (currentSounds.TryGetValue(id, out AudioSource audioSource))
        {
            audioSource.UnPause();
            return;
        }
        string errorMsg = "The sound " + id + "is not currently playing, can't resume them.";
        Debug.LogWarning(errorMsg);
        LogManager.instance.AddLog(errorMsg, new object[] { id });
    }

    public void StopAllSound()
    {
        List<SoundID> keys = new List<SoundID>(currentSounds.Keys);
        foreach (SoundID key in keys)
        {
            StopSound(key);
        }
    }

    public void StopAllSound(float delay)
    {
        List<SoundID> keys = new List<SoundID>(currentSounds.Keys);
        foreach (SoundID key in keys)
        {
            StopSound(key, delay);
        }
    }

    public void StopSound(string id) => StopSound(GetIdFromName(id));
    public void StopSound(uint id) => StopSound(GetSoundID(id));
    private void StopSound(in SoundID id)
    {
        RmMusic(id);
    }

    public void StopSound(string id, float delay) => StopSound(GetIdFromName(id), delay);
    public void StopSound(uint id, float delay) => StopSound(GetSoundID(id), delay);
    private void StopSound(in SoundID id, float delay)
    {
        if (Mathf.Approximately(delay, 0f))
        {
            RmMusic(id);
            return;
        }

        if(removeCorout.TryGetValue(id, out Coroutine c))
        {
            StopCoroutine(c);
            removeCorout.Remove(id);
        }
        removeCorout.Add(id, StartCoroutine(RemoveSoundCoroutine(id, delay)));
    }

    public void StopSmooth(string id, float duration) => StopSmooth(GetIdFromName(id), duration);
    public void StopSmooth(uint id, float duration) => StopSmooth(GetSoundID(id), duration);
    private void StopSmooth(in SoundID id, float duration)
    {
        if(removeCorout.TryGetValue(id, out Coroutine c))
        {
            StopCoroutine(c);
            removeCorout.Remove(id);
        }

        if (currentSounds.TryGetValue(id, out AudioSource audioSource))
        {
            removeCorout.Add(id, StartCoroutine(StopSmoothCorout(id, audioSource, duration)));
        }
    }

    private IEnumerator StopSmoothCorout(SoundID id, AudioSource audioSource, float duration)
    {
        float time = Time.time;
        float volume = audioSource.volume;
        while(Time.time - time < duration)
        {
            audioSource.volume = Mathf.Lerp(volume, 0f, (Time.time - time) / duration);
            yield return null;
        }
        RmMusic(id);
    }

    private void RmMusic(in SoundID id)
    {
        if (!currentSounds.TryGetValue(id, out AudioSource audioSource))
            return;
        audioSource.mute = true;
        Destroy(audioSource.gameObject);
        currentSounds.Remove(id);
        if (removeCorout.TryGetValue(id, out Coroutine coroutine))
        {
            StopCoroutine(coroutine);
            removeCorout.Remove(id);
        }
    }

    private IEnumerator RemoveSoundCoroutine(SoundID id, float delay)
    {
        yield return new WaitForSeconds(delay);
        RmMusic(id);
    }

    public float GetSoundLength(string audioClipName)
    {
        Sound sound = Array.Find(audioClips, item => item.audioClip.name == audioClipName);
        return sound == null ? 0f : sound.audioClip.length;
    }

    public void OnMusicDestroy(uint id)
    {
        SoundID soundID = GetSoundID(id);
        Coroutine coroutine;
        if (removeCorout.TryGetValue(soundID, out coroutine))
        {
            StopCoroutine(coroutine);
            removeCorout.Remove(soundID);
        }
        if (changeVolumeCorout.TryGetValue(soundID, out coroutine))
        {
            StopCoroutine(coroutine);
            changeVolumeCorout.Remove(soundID);
        }
        if (changeVolumeCorout.ContainsKey(soundID))
        {
            currentSounds.Remove(soundID);
        }
    }

#if UNITY_EDITOR

    public void OnValidate()
    {
        if(currentSounds != null)
            RecaculateSoundVolume();
    }

#endif

    private struct SoundID
    {
        public static uint ids = 0;

        public string name;
        public uint id;

        public SoundID(uint id)
        {
            name = string.Empty;
            this.id = id;
        }

        public SoundID(string name, uint id)
        {
            this.name = name;
            this.id = id;
        }

        public override bool Equals(object obj)
        {
            return obj is SoundID soundID ? id == soundID.id : false;
        }

        public override string ToString() => string.Concat("{id:", id, ",name:", name, "}");

        public override int GetHashCode() => id.GetHashCode();

        public static bool operator ==(SoundID a, SoundID b) => a.id == b.id;

        public static bool operator !=(SoundID a, SoundID b) => a.id != b.id;
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
