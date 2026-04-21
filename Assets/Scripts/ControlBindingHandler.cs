using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControlBindingHandler
{
    private LastUsedController lastUsedController;
    private readonly InputActionAsset _actions;

    public event Action<InputAction, int> OnBindingChanged;
    public event Action<InputAction, int> OnRebindStarted;
    public event Action OnRebindCanceled;

    public bool PreventConflicts = true;
    public bool SwapOnConflict = false;

    private InputActionRebindingExtensions.RebindingOperation _currentOp;

    public ControlBindingHandler()
    {
        _actions = PlayerInputs.Instance.GetCustomControls().asset;
        lastUsedController = new LastUsedController();
    }

    // ---------------------------
    // Public API
    // ---------------------------

    public void StartRebind(InputAction action, string bindingId, string bindingGroup = null)
    {
        if (_currentOp != null)
        {
            Debug.LogWarning("Rebind already in progress");
            return;
        }

        int index = FindBindingIndex(action, bindingId);
        if (index < 0)
        {
            Debug.LogError($"Binding ID not found: {bindingId}");
            return;
        }

        var binding = action.bindings[index];

        if (binding.isComposite)
        {
            Debug.LogWarning("Cannot rebind a composite root. Rebind its parts instead.");
            return;
        }

        action.Disable();

        var op = action.PerformInteractiveRebinding(index)
            .WithCancelingThrough("<Keyboard>/escape")
            .OnCancel(operation =>
            {
                Cleanup(action);
                OnRebindCanceled?.Invoke();
            })
            .OnComplete(operation =>
            {
                HandleCompletion(action, index);
            });

        if (!string.IsNullOrEmpty(bindingGroup))
        {
            op.WithBindingGroup(bindingGroup);
        }

        _currentOp = op;

        OnRebindStarted?.Invoke(action, index);

        op.Start();
    }

    public void Cancel()
    {
        _currentOp?.Cancel();
    }

    public void RemoveOverride(InputAction action, string bindingId)
    {
        int index = FindBindingIndex(action, bindingId);
        if (index >= 0)
        {
            action.RemoveBindingOverride(index);
            OnBindingChanged?.Invoke(action, index);
        }
    }

    public void ResetAll()
    {
        foreach (var map in _actions.actionMaps)
        {
            foreach (var action in map.actions)
            {
                action.RemoveAllBindingOverrides();
            }
        }
    }

    // ---------------------------
    // Persistence
    // ---------------------------

    public string Save()
    {
        return _actions.SaveBindingOverridesAsJson();
    }

    public void Load(string json)
    {
        if (!string.IsNullOrEmpty(json))
        {
            _actions.LoadBindingOverridesFromJson(json);
        }
    }

    // ---------------------------
    // Internals
    // ---------------------------

    private void HandleCompletion(InputAction action, int index)
    {
        var newPath = action.bindings[index].effectivePath;

        if (PreventConflicts)
        {
            var conflict = FindConflict(action, index, newPath);

            if (conflict.HasValue)
            {
                if (SwapOnConflict)
                {
                    SwapBindings(action, index, conflict.Value);
                }
                else
                {
                    // Reject new binding
                    action.RemoveBindingOverride(index);
                    Debug.LogWarning("Conflict detected. Rebind rejected.");
                }
            }
        }

        Cleanup(action);

        OnBindingChanged?.Invoke(action, index);
    }

    private void Cleanup(InputAction action)
    {
        action.Enable();

        _currentOp?.Dispose();
        _currentOp = null;
    }

    private int FindBindingIndex(InputAction action, string bindingId)
    {
        var guid = new Guid(bindingId);

        for (int i = 0; i < action.bindings.Count; i++)
        {
            if (action.bindings[i].id == guid)
                return i;
        }

        return -1;
    }

    private int? FindConflict(InputAction action, int targetIndex, string path)
    {
        var map = action.actionMap;

        for (int i = 0; i < map.bindings.Count; i++)
        {
            if (i == targetIndex)
                continue;

            var b = map.bindings[i];

            if (b.effectivePath == path && !b.isComposite)
            {
                return i;
            }
        }

        return null;
    }

    private void SwapBindings(InputAction action, int indexA, int indexB)
    {
        var map = action.actionMap;

        var pathA = map.bindings[indexA].effectivePath;
        var pathB = map.bindings[indexB].effectivePath;

        action.ApplyBindingOverride(indexA, pathB);
        action.ApplyBindingOverride(indexB, pathA);
    }
}
