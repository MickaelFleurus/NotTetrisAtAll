using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

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
        rootVisualElement.RegisterCallback<NavigationMoveEvent>(evt =>
        {
            uiDocument.rootVisualElement.focusController.IgnoreEvent(evt);
            evt.StopPropagation();
        }, TrickleDown.TrickleDown); // TrickleDown captures it before children

        rootVisualElement.RegisterCallback<NavigationSubmitEvent>(evt =>
        {
            uiDocument.rootVisualElement.focusController.IgnoreEvent(evt);
            evt.StopPropagation();
        }, TrickleDown.TrickleDown);


        rootVisualElement.RegisterCallback<NavigationCancelEvent>(evt =>
       {
           uiDocument.rootVisualElement.focusController.IgnoreEvent(evt);
           evt.StopPropagation();
       }, TrickleDown.TrickleDown);

        mCreditsView = rootVisualElement.Q<ScrollView>("CreditsView");

        mCreditsParent = rootVisualElement.Q<VisualElement>("Credits");
        mSettingsParent = rootVisualElement.Q<VisualElement>("SettingsPanel");
        mMainMenuButtons = rootVisualElement.Q<VisualElement>("MainMenuButtons");

        mStartButton = rootVisualElement.Q<Button>("StartButton");

        mQuitButton = rootVisualElement.Q<Button>("ExitButton");

        mSettingsButton = rootVisualElement.Q<Button>("SettingsButton");

        mCreditButton = rootVisualElement.Q<Button>("CreditsButton");

        mCreditsFocusedButton = rootVisualElement.Q<Button>("CreditsBackButton");
        mCreditsFocusedButton.clicked += BackToTitle;

        mSettingsPanel = new SettingsPanel(mSettingsParent, this);
        mSettingsPanel.OnClosed += BackToTitle;

        mGameModePanel = new GameModePanel(rootVisualElement.Q<VisualElement>("GameModePanel"), this);
        mGameModePanel.OnClosed += BackToTitle;
        mGameModePanel.OnStarted += StartGame;

        Dictionary<VisualElement, Action> submitActions = new Dictionary<VisualElement, Action>
        {
            { mStartButton, ShowGameModePanel },
            { mQuitButton, CloseGame },
            { mSettingsButton, ShowOptions },
            { mCreditButton, ShowCredits }
        };

        LoadCredits();
        SetupNavigation(submitActions, this);
    }

    private void Start()
    {
        AudioMixer.Instance.PlayMusic(AudioData.Instance.MainMenuMusic);
    }

    private void Update()
    {
        foreach (var element in AnimatedElements)
        {
            element.Update(Time.deltaTime);
        }
    }

    private void SetupNavigation(Dictionary<VisualElement, Action> submitActions, MonoBehaviour coroutineRunner)
    {
        List<NavigationRow> rows = new List<NavigationRow>() {
            new NavigationRow(new NavigationCell(mStartButton)),
            new NavigationRow(new NavigationCell(mSettingsButton)),
            new NavigationRow(new NavigationCell(mCreditButton)),
            new NavigationRow(new NavigationCell(mQuitButton)),
        };
        mPageNavigation = new NavigationGrid(rows, coroutineRunner);
        mPageNavigation.SetupSubmitEvent(submitActions);
        mPageNavigation.Enable();
    }

    private void StartGame(EGameMode gameMode, int levelStart, int blockSize, EGameTimeLimit timeLimit)
    {
        AudioMixer.Instance.PlaySFX(AudioData.Instance.MainMenuMusic);
        GameData.Instance.OnGameStarted(gameMode, levelStart, blockSize, timeLimit);
        NavigationGrid.ResetInputAction();

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
        mPageNavigation.Disable();
    }

    private void ShowOptions()
    {
        AudioMixer.Instance.PlaySFX(AudioData.Instance.MenuApproveSfx);
        mMainMenuButtons.style.display = DisplayStyle.None;
        mSettingsPanel.Show();
        mPageNavigation.Disable();
    }

    private void ShowGameModePanel()
    {
        AudioMixer.Instance.PlaySFX(AudioData.Instance.MenuApproveSfx);
        mMainMenuButtons.style.display = DisplayStyle.None;
        mGameModePanel.Show();
        mPageNavigation.Disable();
    }

    private void BackToTitle()
    {
        AudioMixer.Instance.PlaySFX(AudioData.Instance.MenuApproveSfx);
        mMainMenuButtons.style.display = DisplayStyle.Flex;
        mSettingsParent.style.display = DisplayStyle.None;
        mCreditsParent.style.display = DisplayStyle.None;

        mPageNavigation.RestoreFocus();
        mPageNavigation.Enable();
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

    void OnDestroy()
    {
        // Unsubscribe from UI panel events to prevent memory leaks
        if (mSettingsPanel != null)
            mSettingsPanel.OnClosed -= BackToTitle;

        if (mGameModePanel != null)
        {
            mGameModePanel.OnClosed -= BackToTitle;
            mGameModePanel.OnStarted -= StartGame;
        }
    }



}
