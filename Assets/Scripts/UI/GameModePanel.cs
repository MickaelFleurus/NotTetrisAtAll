
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

public class GameModePanel
{
    private VisualElement gameModePanel;

    private AnimatedButton backButton;
    private List<AnimatedButton> gameModes = new List<AnimatedButton>();

    private List<AnimatedButton> levels = new List<AnimatedButton>();

    // Block size
    private List<AnimatedButton> blockSize = new List<AnimatedButton>();
    private Label blockSizeWarning;


    // Time Limit
    private Label timeLimitLabel;
    private VisualElement timeLimitParent;
    private List<AnimatedButton> timeLimit;

    private AnimatedButton startButton;

    public event Action OnClosed;
    public event Action<EGameMode, int, int, EGameTimeLimit> OnStarted;

    private EGameMode selectedGameMode = EGameMode.Marathon;
    private int selectedStartLevel = 1;
    private int selectedBlockSize = 4;
    private EGameTimeLimit selectedTimeLimit = EGameTimeLimit.One;
    NavigationGrid pageNavigation;
    int timeLimitRowIndex = 6;


    public GameModePanel(VisualElement settingsPanel)
    {
        gameModePanel = settingsPanel;
        backButton = gameModePanel.Q<AnimatedButton>("BackButton");

        gameModes.Add(gameModePanel.Q<AnimatedButton>("Marathon"));
        gameModes.Add(gameModePanel.Q<AnimatedButton>("TimeLimit"));

        levels = gameModePanel.Q<VisualElement>("LevelStartParent").Children().OfType<AnimatedButton>().ToList();
        blockSize = gameModePanel.Q<VisualElement>("BlockSizeParent").Children().OfType<AnimatedButton>().ToList();
        blockSizeWarning = gameModePanel.Q<Label>("BlockSizeWarning");
        timeLimitLabel = gameModePanel.Q<Label>("TimeLimitLabel");
        timeLimitParent = gameModePanel.Q<VisualElement>("TimeLimitParent");
        timeLimit = timeLimitParent.Children().OfType<AnimatedButton>().ToList();
        startButton = gameModePanel.Q<AnimatedButton>("StartButton");


        Dictionary<VisualElement, Action> submitActions = new Dictionary<VisualElement, Action>
        {
            { backButton, Hide },
            { startButton, OnStartPressed }
        };

        foreach (var elem in gameModes)
        {
            submitActions.Add(elem, () =>
             {
                 selectedGameMode = Enum.Parse<EGameMode>(elem.name);
                 SetChoiceAsSelected(gameModes, elem);
                 if (selectedGameMode == EGameMode.TimeLimit)
                 {
                     timeLimitLabel.style.display = DisplayStyle.Flex;
                     timeLimitParent.style.display = DisplayStyle.Flex;
                     pageNavigation.EnableRow(timeLimitRowIndex);
                 }
                 else
                 {
                     timeLimitLabel.style.display = DisplayStyle.None;
                     timeLimitParent.style.display = DisplayStyle.None;
                     pageNavigation.DisableRow(timeLimitRowIndex);
                 }
             });
        }

        foreach (var elem in levels)
        {
            submitActions.Add(elem, () =>
            {
                selectedStartLevel = int.Parse(elem.name);
                SetChoiceAsSelected(levels, elem, true);
                pageNavigation.SelectColumnAsDefault();
            });
        }
        foreach (var elem in blockSize)
        {
            submitActions.Add(elem, () =>
            {
                SetChoiceAsSelected(blockSize, elem, true);
                selectedBlockSize = int.Parse(elem.text);
                if (selectedBlockSize < 3 || selectedBlockSize > 5)
                {
                    blockSizeWarning.style.display = DisplayStyle.Flex;
                }
                else
                {
                    blockSizeWarning.style.display = DisplayStyle.None;
                }
                pageNavigation.SelectColumnAsDefault();
            });
        }
        foreach (var elem in timeLimit)
        {
            submitActions.Add(elem, () =>
            {
                int timeLimitVal = int.Parse(elem.text);
                selectedTimeLimit = IntToGameTimeLimit(timeLimitVal);
                SetChoiceAsSelected(timeLimit, elem, true);
                pageNavigation.SelectColumnAsDefault();
            });
        }

        SetupNavigation(submitActions);
    }
    private EGameTimeLimit IntToGameTimeLimit(int value)
    {
        return value switch
        {
            1 => EGameTimeLimit.One,
            2 => EGameTimeLimit.Two,
            5 => EGameTimeLimit.Five,
            10 => EGameTimeLimit.Ten,
            20 => EGameTimeLimit.Twenty,
            _ => 0
        };
    }

    private void SetupNavigation(Dictionary<VisualElement, Action> onSubmit)
    {
        List<NavigationRow> rows = new List<NavigationRow>() {
            new NavigationRow(new NavigationCell(backButton)),
            new NavigationRow(new NavigationCell(gameModes[0])),
            new NavigationRow(new NavigationCell(gameModes[1])),
            new NavigationRow(levels.Cast<VisualElement>().ToList(), 3),
            new NavigationRow(blockSize.Cast<VisualElement>().ToList(), 3),
            new NavigationRow(timeLimit.Cast<VisualElement>().ToList(), 0, false),
            new NavigationRow(new NavigationCell(startButton)),
        };
        pageNavigation = new NavigationGrid(rows, onSubmit, 0, 1);
        timeLimitRowIndex = 6;
        pageNavigation.RestoreFocus();
        pageNavigation.cancelPressed += Hide;
    }

    public bool IsShown()
    {
        return gameModePanel.style.display == DisplayStyle.Flex;
    }


    private void SetChoiceAsSelected(List<AnimatedButton> group, VisualElement element, bool isSmall = false)
    {
        string classChange = isSmall ? "GameModeLevelButtonSelected" : "GameModeButtonSelected";
        foreach (var elem in group)
        {
            if (elem == element)
                elem.AddToClassList(classChange);
            else
                elem.RemoveFromClassList(classChange);

        }
    }

    public void Show()
    {
        gameModePanel.style.display = DisplayStyle.Flex;
        pageNavigation.RestoreFocus();
        pageNavigation.Enable();
    }

    public void Hide()
    {
        gameModePanel.style.display = DisplayStyle.None;
        OnClosed.Invoke();
        pageNavigation.Disable();
    }

    private void OnStartPressed()
    {
        OnStarted.Invoke(selectedGameMode, selectedStartLevel, selectedBlockSize, selectedTimeLimit);
    }

}
