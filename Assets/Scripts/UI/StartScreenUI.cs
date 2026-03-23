using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEditor.SettingsManagement;

public class StartScreenUI : MonoBehaviour
{

    [SerializeField] private UIDocument uiDocument;
    private ScrollView mCreditsView;
    private VisualElement mMainMenuButtons;
    private VisualElement mCreditsButtons;
    private VisualElement mOptionsButtons;

    private VisualElement mCreditsParent;
    private VisualElement mSettingsParent;

    private Button mMainMenuFocusedButton;
    private Button mCreditsFocusedButton;

    public List<List<VisualElement>> Navigation { get; set; }
    public VisualElement LastSelectedElement { get; set; }
    private SettingsPanel mSettingsPanel = null;

    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip navigationSFX;
    [SerializeField] private AudioClip selectSFX;

    private void Awake()
    {
        var rootVisualElement = uiDocument.rootVisualElement;

        rootVisualElement.RegisterCallback<NavigationCancelEvent>(OnCancel);
        rootVisualElement.RegisterCallback<NavigationMoveEvent>(OnMove);
        mCreditsView = rootVisualElement.Q<ScrollView>("CreditsView");

        mCreditsParent = rootVisualElement.Q<VisualElement>("Credits");
        mSettingsParent = rootVisualElement.Q<VisualElement>("SettingsPanel");
        mMainMenuButtons = rootVisualElement.Q<VisualElement>("MainMenuButtons");
        mCreditsButtons = rootVisualElement.Q<VisualElement>("CreditsButtons");
        mOptionsButtons = rootVisualElement.Q<VisualElement>("OptionsButtons");

        mMainMenuFocusedButton = rootVisualElement.Q<Button>("StartButton");
        //mCreditsFocusedButton = rootVisualElement.Q<Button>("BackCredits");

        mMainMenuFocusedButton.Focus();
        mMainMenuFocusedButton.clicked += StartGame;
        rootVisualElement.Q<Button>("ExitButton").clicked += CloseGame;
        rootVisualElement.Q<Button>("SettingsButton").clicked += ShowOptions;
        rootVisualElement.Q<Button>("CreditsButton").clicked += ShowCredits;
        rootVisualElement.Q<Button>("SettingsBackButton").clicked += () =>
        {
            AudioMixer.Instance.PlaySFX(selectSFX, GameSettings.Instance.SoundEffectsVolume);
            mSettingsParent.style.display = DisplayStyle.None;
            mMainMenuButtons.style.display = DisplayStyle.Flex;
            mMainMenuFocusedButton.Focus();
        };

        mSettingsPanel = new SettingsPanel(mSettingsParent);

        //LoadCredits();

        Navigation = new List<List<VisualElement>>
        {
            new List<VisualElement> {
            rootVisualElement.Q<Button>("StartButton"),
            rootVisualElement.Q<Button>("SettingsButton"),
            rootVisualElement.Q<Button>("CreditsButton"),
            rootVisualElement.Q<Button>("ExitButton") }
        };
    }

    private void Start()
    {
        AudioMixer.Instance.PlayMusic("mainmenu", mainMenuMusic);
    }

    private void StartGame()
    {
        AudioMixer.Instance.PlaySFX(selectSFX, GameSettings.Instance.SoundEffectsVolume);
        Invoke("LoadGameScene", 0.5f);
        //GameEvents.InvokeInGame();
    }

    private void LoadGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }

    private void ShowCredits()
    {
        AudioMixer.Instance.PlaySFX(selectSFX, GameSettings.Instance.SoundEffectsVolume);

        // mMainMenuButtons.style.display = DisplayStyle.None;
        // mOptionsButtons.style.display = DisplayStyle.None;
        // mCreditsButtons.style.display = DisplayStyle.Flex;

        // mOptionsParent.style.display = DisplayStyle.None;
        // mCreditsParent.style.display = DisplayStyle.Flex;
        // mCreditsFocusedButton.Focus();
    }

    private void ShowOptions()
    {
        AudioMixer.Instance.PlaySFX(selectSFX, GameSettings.Instance.SoundEffectsVolume);
        mMainMenuButtons.style.display = DisplayStyle.None;
        mSettingsPanel.Show();
    }

    private void BackToTitle()
    {
        mMainMenuButtons.style.display = DisplayStyle.Flex;
        mSettingsParent.style.display = DisplayStyle.None;
        //mCreditsButtons.style.display = DisplayStyle.None;
        mMainMenuFocusedButton.Focus();
    }

    private void CloseGame()
    {
        AudioMixer.Instance.PlaySFX(selectSFX, GameSettings.Instance.SoundEffectsVolume);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBGL
        // In WebGL, just exit fullscreen
        if (Screen.fullScreen)
        {
            Screen.fullScreen = false;
        }
#else
        Application.Quit();
#endif
    }

    private void LoadCredits()
    {
        TextAsset creditsFile = Resources.Load<TextAsset>("Credits");

        if (creditsFile != null)
        {
            string[] lines = creditsFile.text.Split('\n');

            foreach (string line in lines)
            {
                if (line.StartsWith("#"))
                {
                    Label titleLabel = new Label(line.Substring(2));
                    titleLabel.AddToClassList("credit_title");
                    mCreditsView.Add(titleLabel);
                }
                else if (line.StartsWith("@"))
                {
                    Label sectionLabel = new Label(line.Substring(2));
                    sectionLabel.AddToClassList("credit_section");
                    mCreditsView.Add(sectionLabel);
                }
                else
                {
                    Label creditLabel = new Label(line);
                    creditLabel.AddToClassList("credit_text");
                    mCreditsView.Add(creditLabel);
                }
            }
        }
        else
        {
            Debug.LogError("Credits file not found in Resources folder");
        }
    }

    void OnMove(NavigationMoveEvent evt)
    {
        AudioMixer.Instance.PlaySFX(navigationSFX, GameSettings.Instance.SoundEffectsVolume);
    }

    void OnCancel(NavigationCancelEvent evt)
    {
        if (mSettingsParent.style.display == DisplayStyle.Flex)
        {
            BackToTitle();
        }
        else if (mCreditsParent.style.display == DisplayStyle.Flex)
        {
            BackToTitle();
        }
    }

}
