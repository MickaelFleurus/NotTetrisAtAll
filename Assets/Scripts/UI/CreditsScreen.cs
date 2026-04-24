using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

class CreditsScreen : IScreen
{
    VisualElement root;
    ScrollView creditsView;
    Button backButton;
    NavigationGrid nav;
    ScreenHandler screenHandler;

    public CreditsScreen(VisualElement root, ScreenHandler screenHandler)
    {
        this.root = root;
        this.screenHandler = screenHandler;

        backButton = root.Q<Button>("CreditsBackButton");
        creditsView = root.Q<ScrollView>("CreditsView");
        LoadCredits();

        Dictionary<VisualElement, Action> submitActions = new Dictionary<VisualElement, Action>
        {{ backButton, screenHandler.RequestPop }};
        List<NavigationRow> rows = new List<NavigationRow>() {
            new NavigationRow(new NavigationCell(backButton))};
        nav = new NavigationGrid(rows, submitActions);
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
