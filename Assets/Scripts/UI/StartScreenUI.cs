using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class StartScreenUI : MonoBehaviour
{

    [SerializeField] private UIDocument uiDocument;
    private ScrollView mCreditsView;
    private VisualElement mMainMenuButtons;
    private VisualElement mCreditsButtons;
    private VisualElement mOptionsButtons;

    private VisualElement mCreditsParent;
    private VisualElement mOptionsParent;

    private Button mMainMenuFocusedButton;
    private Button mCreditsFocusedButton;

    public List<List<VisualElement>> Navigation { get; set; }
    public VisualElement LastSelectedElement { get; set; }

    private void Awake()
    {
        var rootVisualElement = uiDocument.rootVisualElement;

        // Register for navigation events
        rootVisualElement.RegisterCallback<NavigationCancelEvent>(OnCancel);
        rootVisualElement.RegisterCallback<NavigationMoveEvent>(OnMove);
        rootVisualElement.RegisterCallback<NavigationSubmitEvent>(OnSubmit);
        // mCreditsView = rootVisualElement.Q<ScrollView>("CreditsView");

        mCreditsParent = rootVisualElement.Q<VisualElement>("Credits");
        mOptionsParent = rootVisualElement.Q<VisualElement>("Options");
        mMainMenuButtons = rootVisualElement.Q<VisualElement>("MainMenuButtons");
        mCreditsButtons = rootVisualElement.Q<VisualElement>("CreditsButtons");
        mOptionsButtons = rootVisualElement.Q<VisualElement>("OptionsButtons");

        mMainMenuFocusedButton = rootVisualElement.Q<Button>("Start");
        mCreditsFocusedButton = rootVisualElement.Q<Button>("BackCredits");

        mMainMenuFocusedButton.Focus();
        mMainMenuFocusedButton.clicked += StartGame;
        rootVisualElement.Q<Button>("Exit").clicked += CloseGame;
        rootVisualElement.Q<Button>("OptionsButton").clicked += ShowOptions;
        rootVisualElement.Q<Button>("CreditsButton").clicked += ShowCredits;
        mCreditsFocusedButton.clicked += BackToTitle;
        rootVisualElement.RegisterCallback<NavigationMoveEvent>(OnMove);

        LoadCredits();

        Navigation = new List<List<VisualElement>>
        {
            new List<VisualElement> {
            rootVisualElement.Q<Button>("Start"),
            rootVisualElement.Q<Button>("OptionsButton"),
            rootVisualElement.Q<Button>("CreditsButton"),
            rootVisualElement.Q<Button>("Exit") }
        };
    }

    private void Start()
    {
        //SoundManager.Instance.StartMainMenuMusic();
    }

    private void StartGame()
    {
        SceneManager.LoadScene("Game");
        //GameEvents.InvokeInGame();
    }

    private void ShowCredits()
    {
        mMainMenuButtons.style.display = DisplayStyle.None;
        mOptionsButtons.style.display = DisplayStyle.None;
        mCreditsButtons.style.display = DisplayStyle.Flex;

        mOptionsParent.style.display = DisplayStyle.None;
        mCreditsParent.style.display = DisplayStyle.Flex;
        mCreditsFocusedButton.Focus();
    }

    private void ShowOptions()
    {
        mMainMenuButtons.style.display = DisplayStyle.None;
        mOptionsButtons.style.display = DisplayStyle.Flex;
        mCreditsButtons.style.display = DisplayStyle.None;

        mOptionsParent.style.display = DisplayStyle.Flex;
        mCreditsParent.style.display = DisplayStyle.None;
        // mOptionPanel.OnShow();
    }

    private void BackToTitle()
    {
        mMainMenuButtons.style.display = DisplayStyle.Flex;
        mOptionsButtons.style.display = DisplayStyle.None;
        mCreditsButtons.style.display = DisplayStyle.None;

        mOptionsParent.style.display = DisplayStyle.None;
        mCreditsParent.style.display = DisplayStyle.None;
        mMainMenuFocusedButton.Focus();
    }

    private void CloseGame()
    {
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

    }

    void OnSubmit(NavigationSubmitEvent evt)
    {
        if (evt.target is Button button)
        {
            //button.clicked?.Invoke();
        }
    }

    void OnCancel(NavigationCancelEvent evt)
    {
        if (mOptionsParent.style.display == DisplayStyle.Flex)
        {
            BackToTitle();
        }
        else if (mCreditsParent.style.display == DisplayStyle.Flex)
        {
            BackToTitle();
        }
    }

}
