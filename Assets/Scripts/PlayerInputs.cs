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

    private bool isUpdatingMove = false;
    private bool isDropActive = false;
    private bool isRotateClockwiseActive = false;
    private bool isRotateCounterClockwiseActive = false;
    private bool isHoldActive = false;

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

        inGameMoveRepeat = new InputRepeatHandler(this, 0.2f);
        uiNavigateRepeat = new InputRepeatHandler(this, 0.4f);
        customControls.Enable();
        defaultInputs.Enable();

        customControls.Player.Drop.started += ctx => TriggerIfNotActive(ref isDropActive, inGameActions.OnDrop);
        customControls.Player.Drop.canceled += ctx => isDropActive = false;
        customControls.Player.RotateClockwise.started += ctx => TriggerIfNotActive(ref isRotateClockwiseActive, inGameActions.OnRotateClockwise);
        customControls.Player.RotateClockwise.canceled += ctx => isRotateClockwiseActive = false;
        customControls.Player.RotateCounterClockwise.started += ctx => TriggerIfNotActive(ref isRotateCounterClockwiseActive, inGameActions.OnRotateCounterClockwise);
        customControls.Player.RotateCounterClockwise.canceled += ctx => isRotateCounterClockwiseActive = false;
        customControls.Player.Hold.started += ctx => TriggerIfNotActive(ref isHoldActive, inGameActions.OnHold);
        customControls.Player.Hold.canceled += ctx => isHoldActive = false;
        customControls.Player.Move.performed += ctx => HandleMoveValueChanged();
        customControls.Player.Move.canceled += ctx => inGameMoveRepeat.Stop();

        defaultInputs.Player.Drop.started += ctx => TriggerIfNotActive(ref isDropActive, inGameActions.OnDrop);
        defaultInputs.Player.Drop.canceled += ctx => isDropActive = false;
        defaultInputs.Player.RotateClockwise.started += ctx => TriggerIfNotActive(ref isRotateClockwiseActive, inGameActions.OnRotateClockwise);
        defaultInputs.Player.RotateClockwise.canceled += ctx => isRotateClockwiseActive = false;
        defaultInputs.Player.RotateCounterClockwise.started += ctx => TriggerIfNotActive(ref isRotateCounterClockwiseActive, inGameActions.OnRotateCounterClockwise);
        defaultInputs.Player.RotateCounterClockwise.canceled += ctx => isRotateCounterClockwiseActive = false;
        defaultInputs.Player.Hold.started += ctx => TriggerIfNotActive(ref isHoldActive, inGameActions.OnHold);
        defaultInputs.Player.Hold.canceled += ctx => isHoldActive = false;
        defaultInputs.Player.Pause.started += ctx => inGameActions.OnPause();
        defaultInputs.Player.Move.performed += ctx => HandleMoveValueChanged();
        defaultInputs.Player.Move.canceled += ctx => inGameMoveRepeat.Stop();

        inGameMoveRepeat.Repeat += TriggerMoveAction;

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

    private void TriggerIfNotActive(ref bool isActive, Action action)
    {
        if (isActive) return;
        isActive = true;
        action?.Invoke();
    }

    private void HandleMoveValueChanged()
    {
        if (isUpdatingMove) return;

        isUpdatingMove = true;
        try
        {
            float moveValue = customControls.Player.Move.IsPressed()
                ? customControls.Player.Move.ReadValue<float>()
                : defaultInputs.Player.Move.ReadValue<float>();

            if (Math.Abs(moveValue) >= 0.5f)
            {
                if (!inGameMoveRepeat.IsRunning())
                {
                    inGameMoveRepeat.Start();
                }
            }
            else if (inGameMoveRepeat.IsRunning())
            {
                inGameMoveRepeat.Stop();
            }
        }
        finally
        {
            isUpdatingMove = false;
        }
    }

    private void TriggerMoveAction()
    {
        float moveValue = customControls.Player.Move.IsPressed()
            ? customControls.Player.Move.ReadValue<float>()
            : defaultInputs.Player.Move.ReadValue<float>();

        if (Math.Abs(moveValue) >= 0.5f)
        {
            inGameActions.OnMove(moveValue);
        }
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

    public void EnableUiInputNextFrame()
    {
        StartCoroutine(DelayUiInputEnabling());
    }

    private IEnumerator DelayUiInputEnabling()
    {
        yield return null;
        uiActions.Enable();
    }

}
