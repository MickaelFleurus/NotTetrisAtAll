
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ScreenHandler
{
    public enum EScreens { None, StartMenu, GameModeMenu, PauseMenu, CreditsMenu, SettingsMenu, ConfirmationMenu, GameOverMenu };
    private Stack<IScreen> openedScreens = new Stack<IScreen>();

    UIDocument uiDocument;
    private PauseMenu pauseMenu = null;
    private SettingsMenu settingsMenu = null;
    private ConfirmationModal confirmationModal = null;
    private GameModePanel gameModeMenu = null;
    private StartScreen startMenu = null;
    private CreditsScreen creditsMenu = null;
    private GameOverScreen gameOverMenu = null;

    private IModal topModal = null;

    public ScreenHandler(UIDocument uiDocument)
    {
        this.uiDocument = uiDocument;
        uiDocument.rootVisualElement.RegisterCallback<NavigationMoveEvent>(evt =>
               {
                   uiDocument.rootVisualElement.focusController.IgnoreEvent(evt);
                   evt.StopPropagation();
               }, TrickleDown.TrickleDown);

        uiDocument.rootVisualElement.RegisterCallback<NavigationSubmitEvent>(evt =>
        {
            uiDocument.rootVisualElement.focusController.IgnoreEvent(evt);
            evt.StopPropagation();
        }, TrickleDown.TrickleDown);


        uiDocument.rootVisualElement.RegisterCallback<NavigationCancelEvent>(evt =>
       {
           uiDocument.rootVisualElement.focusController.IgnoreEvent(evt);
           evt.StopPropagation();
       }, TrickleDown.TrickleDown);

        PlayerInputs.Instance.uiActions.Approve += OnApprove;
        PlayerInputs.Instance.uiActions.Cancel += OnCancel;
        PlayerInputs.Instance.uiActions.Navigate += OnNavigate;
        PlayerInputs.Instance.uiActions.Pressed += OnPressed;

        var pauseMenuRoot = uiDocument.rootVisualElement.Q<VisualElement>("PauseMenu");
        if (pauseMenuRoot != null)
            pauseMenu = new PauseMenu(pauseMenuRoot, this);

        var settingsRoot = uiDocument.rootVisualElement.Q<VisualElement>("SettingsMenu");
        if (settingsRoot != null)
            settingsMenu = new SettingsMenu(settingsRoot, this);

        var gameModeMenuRoot = uiDocument.rootVisualElement.Q<VisualElement>("GameModePanel");
        if (gameModeMenuRoot != null)
            gameModeMenu = new GameModePanel(gameModeMenuRoot, this);

        var startMenuRoot = uiDocument.rootVisualElement.Q<VisualElement>("MainMenuButtons");
        if (startMenuRoot != null)
            startMenu = new StartScreen(startMenuRoot, this);

        var creditsRoot = uiDocument.rootVisualElement.Q<VisualElement>("Credits");
        if (creditsRoot != null)
            creditsMenu = new CreditsScreen(creditsRoot, this);

        var gameOverRoot = uiDocument.rootVisualElement.Q<VisualElement>("GameOverScreen");
        if (gameOverRoot != null)
            gameOverMenu = new GameOverScreen(gameOverRoot, this);


        VisualTreeAsset confirmationModalFile = Resources.Load<VisualTreeAsset>("UI/ConfirmationModal");  // No "Assets/Resources/" prefix
        if (confirmationModalFile != null)
        {
            VisualElement clonedUI = confirmationModalFile.CloneTree();
            confirmationModal = new ConfirmationModal(clonedUI, this);
        }
    }

    ~ScreenHandler()
    {
        OnDestroy();
    }

    public void OnDestroy()
    {

        PlayerInputs.Instance.uiActions.Disable();
        PlayerInputs.Instance.uiActions.Approve -= OnApprove;
        PlayerInputs.Instance.uiActions.Cancel -= OnCancel;
        PlayerInputs.Instance.uiActions.Navigate -= OnNavigate;
        PlayerInputs.Instance.uiActions.Pressed -= OnPressed;
    }


    private IScreen ScreenEnumToInstance(EScreens screen)
    {
        return screen switch
        {
            EScreens.StartMenu => startMenu,
            EScreens.GameModeMenu => gameModeMenu,
            EScreens.PauseMenu => pauseMenu,
            EScreens.CreditsMenu => creditsMenu,
            EScreens.SettingsMenu => settingsMenu,
            EScreens.GameOverMenu => gameOverMenu,
            _ => null
        };
    }

    public void RequestShow(EScreens screen)
    {
        IScreen requestedScreen = ScreenEnumToInstance(screen);
        if (requestedScreen == null)
        {
            Debug.Log($"An issue happened when trying to open scene {screen}");
            return;
        }

        if (openedScreens.Count != 0)
        {
            openedScreens.Peek().Hide();
        }
        else
        {
            PlayerInputs.Instance.EnableUiInputNextFrame();
        }
        openedScreens.Push(requestedScreen);
        requestedScreen.Show();
        requestedScreen.GetNavigationGrid().RestoreFocus();
    }

    public void RequestPop()
    {
        openedScreens.Pop().Hide();
        if (openedScreens.Count != 0)
        {
            openedScreens.Peek().Show();
            openedScreens.Peek().GetNavigationGrid().RestoreFocus();
        }
        else
        {
            PlayerInputs.Instance.uiActions.Disable();
        }

    }

    public void CloseModal()
    {
        uiDocument.rootVisualElement.Remove(topModal.GetRoot());
        confirmationModal.CancelPressed = null;
        confirmationModal.ConfirmPressed = null;
        topModal = null;
        if (openedScreens.Count != 0)
        {
            openedScreens.Peek().GetNavigationGrid().RestoreFocus();
        }
    }

    public void ShowConfirmationMenu(string approveText, string cancelText, Action onApprove, Action onCancel)
    {
        topModal = confirmationModal;
        confirmationModal.CancelPressed += onCancel;
        confirmationModal.ConfirmPressed += onApprove;
        VisualElement modalRoot = confirmationModal.GetRoot();
        modalRoot.AddToClassList("Modal");
        uiDocument.rootVisualElement.Add(modalRoot);
        confirmationModal.Show(approveText, cancelText);
        confirmationModal.GetNavigationGrid().RestoreFocus();

    }

    void OnCancel()
    {
        if (topModal != null)
        {
            topModal.OnCancel();
        }
        else if (openedScreens.Count != 0)
        {
            openedScreens.Peek().OnCancel();
        }
    }

    void OnApprove()
    {
        if (topModal != null)
        {
            topModal.GetNavigationGrid().OnApprove();
        }
        else
        {
            openedScreens.Peek().GetNavigationGrid().OnApprove();
        }
    }

    void OnNavigate(Vector2 move)
    {
        if (topModal != null)
        {
            topModal.GetNavigationGrid().OnNavigate(move);
        }
        else
        {
            openedScreens.Peek().GetNavigationGrid().OnNavigate(move);
        }
    }

    void OnPressed(VisualElement elem)
    {
        if (topModal != null)
        {
            topModal.GetNavigationGrid().OnPressed(elem);
        }
        else
        {
            openedScreens.Peek().GetNavigationGrid().OnPressed(elem);
        }
    }
}
