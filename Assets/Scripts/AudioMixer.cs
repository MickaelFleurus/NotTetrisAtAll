using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using Mono.Cecil.Cil;

public class AudioMixer : MonoBehaviour
{
    private static AudioMixer instance;
    public static AudioMixer Instance
    {
        get { return instance; }
        private set { instance = value; }
    }

    [SerializeField] private AudioMixerGroup masterMixerGroup;
    [SerializeField] private AudioMixerGroup musicMixerGroup;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    [SerializeField] private AudioSource musicAudioSource;
    private Dictionary<string, AudioClip> musicClips = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> sfxClips = new Dictionary<string, AudioClip>();

    private const float MuteDB = -80f;
    private const float MinDB = -40f;
    private const float MaxDB = 1f;

    private const float MaxDBSfx = -10f;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Subscribe to volume change events
        GameSettings.OnMasterVolumeChanged += OnMasterVolumeChanged;
        GameSettings.OnMusicVolumeChanged += OnMusicVolumeChanged;
        GameSettings.OnSoundEffectsVolumeChanged += OnSoundEffectsVolumeChanged;
    }

    private void Start()
    {
        UpdateAllVolumes();
    }

    private void OnDestroy()
    {
        GameSettings.OnMasterVolumeChanged -= OnMasterVolumeChanged;
        GameSettings.OnMusicVolumeChanged -= OnMusicVolumeChanged;
        GameSettings.OnSoundEffectsVolumeChanged -= OnSoundEffectsVolumeChanged;
    }

    public void PlayMusic(string musicName, AudioClip clip, float fadeDuration = 0.5f)
    {
        if (musicAudioSource.isPlaying)
        {
            StartCoroutine(FadeMusicAndSwitch(clip, fadeDuration));
        }
        else
        {
            musicAudioSource.clip = clip;
            musicAudioSource.Play();
        }

        musicClips[musicName] = clip;
    }

    public void PlaySFX(AudioClip clip, float volume = 1f, Vector3 position = default)
    {
        GameObject sfxGO = new GameObject("SFX_Clip");
        sfxGO.transform.position = position;

        AudioSource sfxSource = sfxGO.AddComponent<AudioSource>();
        sfxSource.outputAudioMixerGroup = sfxMixerGroup;
        sfxSource.clip = clip;
        sfxSource.volume = volume;
        sfxSource.spatialBlend = position == default ? 0f : 1f; // 2D if no position, 3D otherwise
        sfxSource.Play();

        // Destroy after clip finishes
        Destroy(sfxGO, clip.length);
    }

    /// <summary>
    /// Stop music with optional fade out.
    /// </summary>
    public void StopMusic(float fadeDuration = 0.5f)
    {
        if (musicAudioSource == null || !musicAudioSource.isPlaying) return;

        if (fadeDuration > 0)
        {
            StartCoroutine(FadeMusicOut(fadeDuration));
        }
        else
        {
            musicAudioSource.Stop();
        }
    }

    public void PauseMusic()
    {
        if (musicAudioSource != null && musicAudioSource.isPlaying)
        {
            musicAudioSource.Pause();
        }
    }

    public void ResumeMusic()
    {
        if (musicAudioSource != null && !musicAudioSource.isPlaying)
        {
            musicAudioSource.Play();
        }
    }

    private void OnMasterVolumeChanged(float volume)
    {
        SetMixerGroupVolume(masterMixerGroup, volume);
    }

    private void OnMusicVolumeChanged(float volume)
    {
        SetMixerGroupVolume(musicMixerGroup, volume);
    }

    private void OnSoundEffectsVolumeChanged(float volume)
    {
        SetMixerGroupVolume(sfxMixerGroup, volume);
    }

    private void UpdateAllVolumes()
    {
        SetMixerGroupVolume(masterMixerGroup, GameSettings.Instance.MasterVolume);
        SetMixerGroupVolume(musicMixerGroup, GameSettings.Instance.MusicVolume);
        SetMixerGroupVolume(sfxMixerGroup, GameSettings.Instance.SoundEffectsVolume);
    }

    private void SetMixerGroupVolume(AudioMixerGroup group, float volume)
    {
        float max = group == sfxMixerGroup ? MaxDBSfx : MaxDB;
        float db = volume > 0.001f ? Mathf.Lerp(MinDB, max, volume) : MuteDB;
        group.audioMixer.SetFloat(GetVolumeParameterName(group), db);
    }

    private string GetVolumeParameterName(AudioMixerGroup group)
    {
        if (group == masterMixerGroup) return "MasterVolume";
        if (group == musicMixerGroup) return "MusicVolume";
        if (group == sfxMixerGroup) return "SFXVolume";
        return "Volume";
    }

    private System.Collections.IEnumerator FadeMusicAndSwitch(AudioClip newClip, float fadeDuration)
    {
        yield return StartCoroutine(FadeMusicOut(fadeDuration));
        musicAudioSource.clip = newClip;
        musicAudioSource.Play();
    }

    private System.Collections.IEnumerator FadeMusicOut(float fadeDuration)
    {
        float elapsedTime = 0f;
        float startVolume = musicAudioSource.volume;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            musicAudioSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / fadeDuration);
            yield return null;
        }

        musicAudioSource.Stop();
        musicAudioSource.volume = startVolume;
    }
}
