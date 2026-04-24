
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class ConfirmationModal : IModal
{
    private VisualElement root;
    private AnimatedButton confirmButton;
    private AnimatedButton cancelButton;
    NavigationGrid nav;
    public Action CancelPressed;
    public Action ConfirmPressed;
    ScreenHandler screenHandler;

    public ConfirmationModal(VisualElement root, ScreenHandler screenHandler)
    {
        this.screenHandler = screenHandler;
        this.root = root;
        confirmButton = root.Q<AnimatedButton>("ExitButton");
        cancelButton = root.Q<AnimatedButton>("BackButton");

        List<NavigationRow> confirmNav = new List<NavigationRow>() { new NavigationRow(new List<VisualElement>() { cancelButton, confirmButton }) };
        Dictionary<VisualElement, Action> submitActionsConfirm = new Dictionary<VisualElement, Action> {
             { cancelButton, ()=> CancelPressed?.Invoke() },
              { confirmButton, ()=> ConfirmPressed?.Invoke()}
               };
        nav = new NavigationGrid(confirmNav, submitActionsConfirm);
    }

    public NavigationGrid GetNavigationGrid()
    {
        return nav;
    }

    public VisualElement GetRoot()
    {
        return root;
    }

    public void OnCancel()
    {
        screenHandler.CloseModal();
    }

    public void Show(string approveText, string cancelText = "Cancel")
    {
        confirmButton.text = approveText;
        cancelButton.text = cancelText;
    }
}
