

using System;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "ScriptableObjects/GameSettings")]
public class GameSettings : ScriptableObject
{
    private static GameSettings instance;
    public static GameSettings Instance
    {
        get
        {
            if (instance == null)
            {
                instance = CreateInstance<GameSettings>();
                instance.Load();
            }
            return instance;
        }
    }


    // Events
    public static event Action<float> OnMasterVolumeChanged;
    public static event Action<float> OnMusicVolumeChanged;
    public static event Action<float> OnSoundEffectsVolumeChanged;

    public static void InvokeMasterVolumeChanged(float volume) => OnMasterVolumeChanged?.Invoke(volume);
    public static void InvokeMusicVolumeChanged(float volume) => OnMusicVolumeChanged?.Invoke(volume);
    public static void InvokeSoundEffectsVolumeChanged(float volume) => OnSoundEffectsVolumeChanged?.Invoke(volume);


    [Header("Audio Settings")]
    [SerializeField][Range(0f, 1f)] private float masterVolume = 100f;
    [SerializeField][Range(0f, 1f)] private float musicVolume = 100f;
    [SerializeField][Range(0f, 1f)] private float soundEffectsVolume = 100f;

    public float MasterVolume
    {
        get => masterVolume;
        set
        {
            if (masterVolume != value)
            {
                masterVolume = Mathf.Clamp(value, 0f, 1f);
                OnMasterVolumeChanged?.Invoke(masterVolume);
                Save();
            }
        }
    }

    public float SoundEffectsVolume
    {
        get => soundEffectsVolume;
        set
        {
            if (soundEffectsVolume != value)
            {
                soundEffectsVolume = Mathf.Clamp(value, 0f, 1f);
                OnSoundEffectsVolumeChanged?.Invoke(soundEffectsVolume);
                Save();
            }
        }
    }


    public float MusicVolume
    {
        get => musicVolume;
        set
        {
            if (musicVolume != value)
            {
                musicVolume = Mathf.Clamp(value, 0f, 1f);
                OnMusicVolumeChanged?.Invoke(musicVolume);
                Save();
            }
        }
    }

    private GameSettings()
    {
    }

    public void ResetToDefaults()
    {
        masterVolume = 100f;
        musicVolume = 100f;
        soundEffectsVolume = 100f;
        Save();
    }

    private const string WebGLPrefsKey = "GameSettings";

    public void Save()
    {
        string json = JsonUtility.ToJson(this, true);

#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL: use PlayerPrefs (LocalStorage) - File I/O can cause "Permissions check failed" in iframes
        PlayerPrefs.SetString(WebGLPrefsKey, json);
        PlayerPrefs.Save();
        Debug.Log("Settings saved (WebGL PlayerPrefs)");
#else
        // Standalone/Editor: save to JSON file
        string savePath = Path.Combine(Application.persistentDataPath, "GameSettings.json");
        File.WriteAllText(savePath, json);
        Debug.Log($"Settings saved to {savePath}");
#endif
    }

    public void Load()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL: load from PlayerPrefs (LocalStorage)
        if (PlayerPrefs.HasKey(WebGLPrefsKey))
        {
            string json = PlayerPrefs.GetString(WebGLPrefsKey);
            JsonUtility.FromJsonOverwrite(json, this);
            Debug.Log("Settings loaded (WebGL PlayerPrefs)");
        }
#else
        // Standalone/Editor: load from JSON file
        string loadPath = Path.Combine(Application.persistentDataPath, "GameSettings.json");
        if (File.Exists(loadPath))
        {
            string json = File.ReadAllText(loadPath);
            JsonUtility.FromJsonOverwrite(json, this);
            Debug.Log($"Settings loaded from {loadPath}");
        }
#endif
    }
}
