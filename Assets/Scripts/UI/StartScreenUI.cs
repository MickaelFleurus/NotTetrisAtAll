using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class StartScreenUI : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    private List<IAnimatedElement> AnimatedElements = new List<IAnimatedElement>();
    private ScrollView mCreditsView;
    private VisualElement mMainMenuButtons;

    private VisualElement mCreditsParent;
    private VisualElement mSettingsParent;

    private Button mStartButton;
    private Button mSettingsButton;
    private Button mCreditButton;
    private Button mQuitButton;
    private Button mCreditsFocusedButton;

    private SettingsPanel mSettingsPanel = null;
    private GameModePanel mGameModePanel = null;

    NavigationGrid mPageNavigation;

    [SerializeField] private UIAnimationData buttonsAnimationData;
    [SerializeField] private UIAnimationData slidersAnimationData;
    [SerializeField] private UIAnimationData titleAnimationData;

    private void SearchInChildren<T>(VisualElement parent, List<T> container) where T : class
    {
        if (parent is T)
        {
            container.Add(parent as T);
        }
        foreach (var element in parent.Children())
        {
            SearchInChildren<T>(element, container);
        }
    }

    private void Awake()
    {
        var rootVisualElement = uiDocument.rootVisualElement;

        SearchInChildren(rootVisualElement, AnimatedElements);
        foreach (var animated in AnimatedElements)
        {
            switch (animated.GetAnimatedType())
            {
                case AnimatedElementType.Title:
                    animated.Copy(titleAnimationData);
                    break;
                case AnimatedElementType.Slider:
                    animated.Copy(slidersAnimationData);
                    break;
                case AnimatedElementType.Button:
                    animated.Copy(buttonsAnimationData);
                    break;
            }
        }
        rootVisualElement.RegisterCallback<NavigationMoveEvent>(OnMove, TrickleDown.TrickleDown);
        rootVisualElement.RegisterCallback<NavigationCancelEvent>(OnCancel);

        mCreditsView = rootVisualElement.Q<ScrollView>("CreditsView");

        mCreditsParent = rootVisualElement.Q<VisualElement>("Credits");
        mSettingsParent = rootVisualElement.Q<VisualElement>("SettingsPanel");
        mMainMenuButtons = rootVisualElement.Q<VisualElement>("MainMenuButtons");

        mStartButton = rootVisualElement.Q<Button>("StartButton");
        mStartButton.clicked += ShowGameModePanel;

        mQuitButton = rootVisualElement.Q<Button>("ExitButton");
        mQuitButton.clicked += CloseGame;

        mSettingsButton = rootVisualElement.Q<Button>("SettingsButton");
        mSettingsButton.clicked += ShowOptions;

        mCreditButton = rootVisualElement.Q<Button>("CreditsButton");
        mCreditButton.clicked += ShowCredits;

        mCreditsFocusedButton = rootVisualElement.Q<Button>("CreditsBackButton");
        mCreditsFocusedButton.clicked += BackToTitle;

        mSettingsPanel = new SettingsPanel(mSettingsParent);
        mSettingsPanel.OnClosed += BackToTitle;

        mGameModePanel = new GameModePanel(rootVisualElement.Q<VisualElement>("GameModePanel"));
        mGameModePanel.OnClosed += BackToTitle;
        mGameModePanel.OnStarted += StartGame;

        LoadCredits();
        SetupNavigation();
    }

    private void Start()
    {
        AudioMixer.Instance.PlayMusic("mainmenu", AudioData.Instance.MainMenuMusic);
    }

    private void Update()
    {
        foreach (var element in AnimatedElements)
        {
            element.Update(Time.deltaTime);
        }
    }

    private void SetupNavigation()
    {
        List<NavigationRow> rows = new List<NavigationRow>() {
            new NavigationRow(new NavigationCell(mStartButton)),
            new NavigationRow(new NavigationCell(mSettingsButton)),
            new NavigationRow(new NavigationCell(mCreditButton)),
            new NavigationRow(new NavigationCell(mQuitButton)),
        };
        mPageNavigation = new NavigationGrid(rows);

    }

    private void StartGame(EGameMode gameMode, int levelStart, int blockSize, EGameTimeLimit timeLimit)
    {
        AudioMixer.Instance.PlaySFX(AudioData.Instance.MainMenuMusic);
        GameData.Instance.OnGameStarted(gameMode, levelStart, blockSize, timeLimit);

        Invoke("LoadGameScene", 0.5f);
    }

    private void LoadGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }

    private void ShowCredits()
    {
        AudioMixer.Instance.PlaySFX(AudioData.Instance.MenuApproveSfx);

        mMainMenuButtons.style.display = DisplayStyle.None;
        mSettingsParent.style.display = DisplayStyle.None;
        mCreditsParent.style.display = DisplayStyle.Flex;
        mCreditsFocusedButton.Focus();
    }

    private void ShowOptions()
    {
        AudioMixer.Instance.PlaySFX(AudioData.Instance.MenuApproveSfx);
        mMainMenuButtons.style.display = DisplayStyle.None;
        mSettingsPanel.Show();
    }

    private void ShowGameModePanel()
    {
        AudioMixer.Instance.PlaySFX(AudioData.Instance.MenuApproveSfx);
        mMainMenuButtons.style.display = DisplayStyle.None;
        mGameModePanel.Show();
    }

    private void BackToTitle()
    {
        AudioMixer.Instance.PlaySFX(AudioData.Instance.MenuApproveSfx);
        mMainMenuButtons.style.display = DisplayStyle.Flex;
        mSettingsParent.style.display = DisplayStyle.None;
        mCreditsParent.style.display = DisplayStyle.None;

        mPageNavigation.RestoreFocus();
    }

    private void CloseGame()
    {
        AudioMixer.Instance.PlaySFX(AudioData.Instance.MenuApproveSfx);
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
                    sectionLabel.AddToClassList("credit_subtitle");
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
        AudioMixer.Instance.PlaySFX(AudioData.Instance.MenuNavigationSfx);
        bool shouldIgnoreEvent = false;
        if (mSettingsPanel.IsShown())
        {
            shouldIgnoreEvent = mSettingsPanel.OnMove(evt);
        }
        else if (mGameModePanel.IsShown())
        {
            shouldIgnoreEvent = mGameModePanel.OnMove(evt);
        }
        else if (mCreditsParent.style.display != DisplayStyle.Flex)
        {
            shouldIgnoreEvent = mPageNavigation.OnNavigationEvent(evt);
        }
        if (shouldIgnoreEvent)
            uiDocument.rootVisualElement.focusController.IgnoreEvent(evt);
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
