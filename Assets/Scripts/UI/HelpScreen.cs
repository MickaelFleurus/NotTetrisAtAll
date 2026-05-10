using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

class HelpScreen : IScreen
{
    VisualElement root;
    Button backButton;
    NavigationGrid nav;
    ScreenHandler screenHandler;

    public HelpScreen(VisualElement root, ScreenHandler screenHandler)
    {
        this.root = root;
        this.screenHandler = screenHandler;

        backButton = root.Q<Button>("BackButton");

        Dictionary<VisualElement, Action> submitActions = new Dictionary<VisualElement, Action>
        {{ backButton, screenHandler.RequestPop }};
        List<NavigationRow> rows = new List<NavigationRow>() {
            new NavigationRow(new NavigationCell(backButton))};
        nav = new NavigationGrid(rows, submitActions);
    }

    public NavigationGrid GetNavigationGrid()
    {
        return nav;
    }

    public void Hide()
    {
        root.style.display = DisplayStyle.None;
    }

    public void Show()
    {
        root.style.display = DisplayStyle.Flex;
    }

    public void OnCancel()
    {
        screenHandler.RequestPop();
    }
}
