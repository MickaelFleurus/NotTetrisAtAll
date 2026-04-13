using System;
using System.IO;
using UnityEngine;


[CreateAssetMenu(fileName = "AudioData", menuName = "ScriptableObjects/AudioData")]
public class AudioData : ScriptableObject
{

    private static AudioData instance;
    public static AudioData Instance
    {
        get
        {
            if (instance == null)
            {
                instance = CreateInstance<AudioData>();
                instance.Load();
            }
            return instance;
        }
    }

    public AudioClip MenuNavigationSfx { get => menuNavigationSfx; }
    public AudioClip MenuApproveSfx { get => menuApproveSfx; }
    public AudioClip MenuCancelSfx { get => menuCancelSfx; }
    public AudioClip MainMenuMusic { get => mainMenuMusic; }
    private AudioClip InGameMusic { get => inGameMusic; }

    [Header("Audio clips")]
    [SerializeField] private AudioClip menuNavigationSfx;
    [SerializeField] private AudioClip menuApproveSfx;
    [SerializeField] private AudioClip menuCancelSfx;
    [SerializeField] private AudioClip mainMenuMusic;

    [SerializeField] private AudioClip inGameMusic;

    public void Load()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL: load from PlayerPrefs (LocalStorage)
        if (PlayerPrefs.HasKey(WebGLPrefsKey))
        {
            string json = PlayerPrefs.GetString(WebGLPrefsKey);
            JsonUtility.FromJsonOverwrite(json, this);
        }
#else
        // Standalone/Editor: load from JSON file
        string loadPath = Path.Combine(Application.persistentDataPath, "AudioData.json");
        if (File.Exists(loadPath))
        {
            string json = File.ReadAllText(loadPath);
            JsonUtility.FromJsonOverwrite(json, this);
        }
#endif
    }

}
