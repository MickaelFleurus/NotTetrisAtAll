using System;
using System.Collections;
using System.Dynamic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;


class InGameActions
{
    bool enabled = false;
    public Action<float> Move;
    public Action Drop;
    public Action RotateClockwise;
    public Action RotateCounterClockwise;
    public Action Hold;
    public Action Pause;

    public void Enable()
    {
        enabled = true;
    }

    public void Disable()
    {
        enabled = false;
    }

    public void OnMove(float val)
    {
        if (!enabled) return;
        Move?.Invoke(val);
    }

    public void OnDrop()
    {
        if (!enabled) return;

        Drop?.Invoke();
    }

    public void OnRotateClockwise()
    {
        if (!enabled) return;

        RotateClockwise?.Invoke();
    }

    public void OnRotateCounterClockwise()
    {
        if (!enabled) return;

        RotateCounterClockwise?.Invoke();
    }

    public void OnHold()
    {
        if (!enabled) return;

        Hold?.Invoke();
    }

    public void OnPause()
    {
        if (!enabled) return;
        Pause?.Invoke();
    }
}

class UIActions
{
    bool enabled = false;

    public void Enable()
    {
        enabled = true;
    }

    public void Disable()
    {
        enabled = false;
    }

    public Action<Vector2> Navigate;
    public Action Approve;
    public Action Cancel;
    public Action<VisualElement> Pressed;

    public void OnNavigate(Vector2 nav)
    {
        if (!enabled) return;
        Navigate?.Invoke(nav);
    }
    public void OnApprove()
    {
        if (!enabled) return;
        Approve?.Invoke();
    }
    public void OnCancel()
    {
        if (!enabled) return;
        Cancel?.Invoke();
    }

    public void OnPressed(VisualElement elem)
    {
        if (!enabled) return;
        Pressed?.Invoke(elem);
    }
}

class PlayerInputs : MonoBehaviour
{
    private static PlayerInputs instance;
    public static PlayerInputs Instance
    {
        get => instance;
    }

    private CustomControls customControls;
    private InputSystem_Actions defaultInputs;

    public InGameActions inGameActions;
    public UIActions uiActions;

    InputRepeatHandler inGameMoveRepeat;
    InputRepeatHandler uiNavigateRepeat;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize both input systems
        customControls = new CustomControls();
        defaultInputs = new InputSystem_Actions();

        inGameActions = new InGameActions();
        uiActions = new UIActions();

        inGameMoveRepeat = new InputRepeatHandler(this, 0.5f);
        uiNavigateRepeat = new InputRepeatHandler(this, 0.4f);
        customControls.Enable();
        defaultInputs.Enable();

        customControls.Player.Drop.started += ctx => inGameActions.OnDrop();
        customControls.Player.RotateClockwise.started += ctx => inGameActions.OnRotateClockwise();
        customControls.Player.RotateCounterClockwise.started += ctx => inGameActions.OnRotateCounterClockwise();
        customControls.Player.Hold.started += ctx => inGameActions.OnHold();
        customControls.Player.Move.started += ctx => inGameMoveRepeat.Start();
        customControls.Player.Move.canceled += ctx => inGameMoveRepeat.Stop();

        defaultInputs.Player.Drop.started += ctx => inGameActions.OnDrop();
        defaultInputs.Player.RotateClockwise.started += ctx => inGameActions.OnRotateClockwise();
        defaultInputs.Player.RotateCounterClockwise.started += ctx => inGameActions.OnRotateCounterClockwise();
        defaultInputs.Player.Hold.started += ctx => inGameActions.OnHold();
        defaultInputs.Player.Pause.started += ctx => inGameActions.OnPause();
        defaultInputs.Player.Move.started += ctx => inGameMoveRepeat.Start();
        defaultInputs.Player.Move.canceled += ctx => inGameMoveRepeat.Stop();

        inGameMoveRepeat.Repeat += TriggerMove;

        defaultInputs.UI.Submit.started += ctx => uiActions.OnApprove();
        defaultInputs.UI.Cancel.started += ctx => uiActions.OnCancel();
        defaultInputs.UI.Navigate.started += ctx => uiNavigateRepeat.Start();
        defaultInputs.UI.Navigate.canceled += ctx => uiNavigateRepeat.Stop();
        uiNavigateRepeat.Repeat += TriggerNav;


        RegisterMouseClickHandler();
    }


    public CustomControls GetCustomControls()
    {
        return customControls;
    }

    public InputSystem_Actions GetDefaultInputs()
    {
        return defaultInputs;
    }

    private void TriggerMove()
    {
        float moveValue = customControls.Player.Move.IsPressed()
            ? customControls.Player.Move.ReadValue<float>()
            : defaultInputs.Player.Move.ReadValue<float>();


        inGameActions.OnMove(moveValue);
    }

    private void TriggerNav()
    {
        Vector2 moveValue =
            defaultInputs.UI.Navigate.ReadValue<Vector2>();

        uiActions.OnNavigate(moveValue);
    }

    private void RegisterMouseClickHandler()
    {
        UIDocument uiDoc = FindFirstObjectByType<UIDocument>();
        if (uiDoc == null) return;

        var root = uiDoc.rootVisualElement;

        // Listen for pointer clicks
        root.RegisterCallback<PointerDownEvent>(OnPointerClick, TrickleDown.TrickleDown);
    }

    private void OnPointerClick(PointerDownEvent evt)
    {
        // Get the element that was clicked
        VisualElement clickedElement = evt.target as VisualElement;
        if (clickedElement == null) return;

        uiActions.OnPressed(clickedElement);
    }

}
