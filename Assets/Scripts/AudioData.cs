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
                instance = Resources.Load<AudioData>("Audio/AudioData");
            }
            return instance;
        }
    }

    public AudioClip GameMoveSfx { get => gameMoveSfx; }
    public AudioClip GameTurnSfx { get => gameTurnSfx; }
    public AudioClip GamePlacedSfx { get => gamePlacedSfx; }
    public AudioClip GameDestroySfx { get => gameDestroySfx; }

    [Header("UI SFX")]
    [SerializeField] private AudioClip menuNavigationSfx;
    [SerializeField] private AudioClip menuApproveSfx;
    [SerializeField] private AudioClip menuCancelSfx;
    public AudioClip MenuNavigationSfx { get => menuNavigationSfx; }
    public AudioClip MenuApproveSfx { get => menuApproveSfx; }
    public AudioClip MenuCancelSfx { get => menuCancelSfx; }


    [Header("Main menu music")]
    [SerializeField] private AudioClip mainMenuMusic;
    public AudioClip MainMenuMusic { get => mainMenuMusic; }


    [Header("InGame musics")]
    [SerializeField] private AudioClip inGameMusic;
    public AudioClip InGameMusic { get => inGameMusic; }
    [SerializeField] private AudioClip inGameMusic2;
    public AudioClip InGameMusic2 { get => inGameMusic2; }
    [SerializeField] private AudioClip inGameMusicFaster;
    public AudioClip InGameMusicFaster { get => inGameMusicFaster; }
    [SerializeField] private AudioClip inGameMusicFastest;
    public AudioClip InGameMusicFastest { get => inGameMusicFastest; }

    [Header("InGame SFX")]
    [SerializeField] private AudioClip gameMoveSfx;
    [SerializeField] private AudioClip gameTurnSfx;
    [SerializeField] private AudioClip gamePlacedSfx;
    [SerializeField] private AudioClip gameDestroySfx;


}
