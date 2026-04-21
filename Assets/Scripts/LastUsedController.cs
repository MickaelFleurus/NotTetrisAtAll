using System;
using UnityEngine.InputSystem;

public class LastUsedController : IDisposable
{
    public enum ControllerUsed { KeyboardMouse, Gamepad, Other }

    public ControllerUsed Current { get; private set; } = ControllerUsed.KeyboardMouse;

    public event Action<ControllerUsed> OnDeviceChanged;

    private ControllerUsed _lastNotified;

    public LastUsedController()
    {
        InputSystem.onActionChange += OnActionChange;
    }

    public void Dispose()
    {
        InputSystem.onActionChange -= OnActionChange;
    }

    private void OnActionChange(object obj, InputActionChange change)
    {
        if (change != InputActionChange.ActionPerformed)
            return;

        var action = obj as InputAction;
        var control = action?.activeControl;
        if (control == null)
            return;

        var device = control.device;
        var kind = Classify(device);

        // Debounce: only notify on real change
        if (kind != Current)
        {
            Current = kind;
            if (_lastNotified != kind)
            {
                _lastNotified = kind;
                OnDeviceChanged?.Invoke(kind);
            }
        }
    }

    private ControllerUsed Classify(InputDevice device)
    {
        if (device is Gamepad) return ControllerUsed.Gamepad;
        if (device is Keyboard || device is Mouse) return ControllerUsed.KeyboardMouse;
        return ControllerUsed.Other;
    }
}
