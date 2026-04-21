using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

public class StartScreenUI : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    private List<IAnimatedElement> AnimatedElements = new List<IAnimatedElement>();
    private ScrollView creditsView;
    private VisualElement mainMenuButtons;

    private VisualElement creditsParent;
    private VisualElement settingsParent;

    private Button startButton;
    private Button mSettingsButton;
    private Button creditButton;
    private Button quitButton;
    private Button creditsFocusedButton;

    private SettingsPanel settingsPanel = null;
    private GameModePanel gameModePanel = null;

    NavigationGrid pageNavigation;

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

        creditsView = rootVisualElement.Q<ScrollView>("CreditsView");

        creditsParent = rootVisualElement.Q<VisualElement>("Credits");
        settingsParent = rootVisualElement.Q<VisualElement>("SettingsPanel");
        mainMenuButtons = rootVisualElement.Q<VisualElement>("MainMenuButtons");

        startButton = rootVisualElement.Q<Button>("StartButton");

        quitButton = rootVisualElement.Q<Button>("ExitButton");

        mSettingsButton = rootVisualElement.Q<Button>("SettingsButton");

        creditButton = rootVisualElement.Q<Button>("CreditsButton");

        creditsFocusedButton = rootVisualElement.Q<Button>("CreditsBackButton");
        creditsFocusedButton.clicked += BackToTitle;

        settingsPanel = new SettingsPanel(settingsParent);
        settingsPanel.OnClosed += BackToTitle;

        gameModePanel = new GameModePanel(rootVisualElement.Q<VisualElement>("GameModePanel"));
        gameModePanel.OnClosed += BackToTitle;
        gameModePanel.OnStarted += StartGame;

        Dictionary<VisualElement, Action> submitActions = new Dictionary<VisualElement, Action>
        {
            { startButton, ShowGameModePanel },
            { quitButton, CloseGame },
            { mSettingsButton, ShowOptions },
            { creditButton, ShowCredits }
        };

        LoadCredits();
        SetupNavigation(submitActions);
        PlayerInputs.Instance.uiActions.Enable();
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

    private void SetupNavigation(Dictionary<VisualElement, Action> submitActions)
    {
        List<NavigationRow> rows = new List<NavigationRow>() {
            new NavigationRow(new NavigationCell(startButton)),
            new NavigationRow(new NavigationCell(mSettingsButton)),
            new NavigationRow(new NavigationCell(creditButton)),
            new NavigationRow(new NavigationCell(quitButton)),
        };
        pageNavigation = new NavigationGrid(rows, submitActions);

        pageNavigation.Enable();
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

        mainMenuButtons.style.display = DisplayStyle.None;
        settingsParent.style.display = DisplayStyle.None;
        creditsParent.style.display = DisplayStyle.Flex;
        creditsFocusedButton.Focus();
        pageNavigation.Disable();
    }

    private void ShowOptions()
    {
        AudioMixer.Instance.PlaySFX(AudioData.Instance.MenuApproveSfx);
        mainMenuButtons.style.display = DisplayStyle.None;
        settingsPanel.Show();
        pageNavigation.Disable();
    }

    private void ShowGameModePanel()
    {
        AudioMixer.Instance.PlaySFX(AudioData.Instance.MenuApproveSfx);
        mainMenuButtons.style.display = DisplayStyle.None;
        gameModePanel.Show();
        pageNavigation.Disable();
    }

    private void BackToTitle()
    {
        AudioMixer.Instance.PlaySFX(AudioData.Instance.MenuApproveSfx);
        mainMenuButtons.style.display = DisplayStyle.Flex;
        settingsParent.style.display = DisplayStyle.None;
        creditsParent.style.display = DisplayStyle.None;

        pageNavigation.RestoreFocus();
        pageNavigation.Enable();
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
                    creditsView.Add(titleLabel);
                }
                else if (line.StartsWith("@"))
                {
                    Label sectionLabel = new Label(line.Substring(2));
                    sectionLabel.AddToClassList("credit_subtitle");
                    creditsView.Add(sectionLabel);
                }
                else
                {
                    Label creditLabel = new Label(line);
                    creditLabel.AddToClassList("credit_text");
                    creditsView.Add(creditLabel);
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
        if (settingsParent.style.display == DisplayStyle.Flex)
        {
            BackToTitle();
        }
        else if (creditsParent.style.display == DisplayStyle.Flex)
        {
            BackToTitle();
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from UI panel events to prevent memory leaks
        if (settingsPanel != null)
            settingsPanel.OnClosed -= BackToTitle;

        if (gameModePanel != null)
        {
            gameModePanel.OnClosed -= BackToTitle;
            gameModePanel.OnStarted -= StartGame;
        }
    }



}
