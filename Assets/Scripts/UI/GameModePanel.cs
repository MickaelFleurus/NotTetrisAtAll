
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

public class GameModePanel : IScreen
{
    private VisualElement root;

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

    private EGameMode selectedGameMode = EGameMode.Marathon;
    private int selectedStartLevel = 1;
    private int selectedBlockSize = 4;
    private EGameTimeLimit selectedTimeLimit = EGameTimeLimit.One;
    NavigationGrid nav;
    int timeLimitRowIndex = 6;
    ScreenHandler screenHandler;


    public GameModePanel(VisualElement root, ScreenHandler screenHandler)
    {
        this.screenHandler = screenHandler;
        this.root = root;
        backButton = root.Q<AnimatedButton>("BackButton");

        gameModes.Add(root.Q<AnimatedButton>("Marathon"));
        gameModes.Add(root.Q<AnimatedButton>("TimeLimit"));

        levels = root.Q<VisualElement>("LevelStartParent").Children().OfType<AnimatedButton>().ToList();
        blockSize = root.Q<VisualElement>("BlockSizeParent").Children().OfType<AnimatedButton>().ToList();
        blockSizeWarning = root.Q<Label>("BlockSizeWarning");
        timeLimitLabel = root.Q<Label>("TimeLimitLabel");
        timeLimitParent = root.Q<VisualElement>("TimeLimitParent");
        timeLimit = timeLimitParent.Children().OfType<AnimatedButton>().ToList();
        startButton = root.Q<AnimatedButton>("StartButton");


        Dictionary<VisualElement, Action> submitActions = new Dictionary<VisualElement, Action>
        {
            { backButton, screenHandler.RequestPop },
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
                     nav.EnableRow(timeLimitRowIndex);
                 }
                 else
                 {
                     timeLimitLabel.style.display = DisplayStyle.None;
                     timeLimitParent.style.display = DisplayStyle.None;
                     nav.DisableRow(timeLimitRowIndex);
                 }
             });
        }

        foreach (var elem in levels)
        {
            submitActions.Add(elem, () =>
            {
                selectedStartLevel = int.Parse(elem.name);
                SetChoiceAsSelected(levels, elem, true);
                nav.SelectColumnAsDefault();
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
                nav.SelectColumnAsDefault();
            });
        }
        foreach (var elem in timeLimit)
        {
            submitActions.Add(elem, () =>
            {
                int timeLimitVal = int.Parse(elem.text);
                selectedTimeLimit = IntToGameTimeLimit(timeLimitVal);
                SetChoiceAsSelected(timeLimit, elem, true);
                nav.SelectColumnAsDefault();
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
            new NavigationRow(levels.GetRange(0, 5).Cast<VisualElement>().ToList(), 0),
            new NavigationRow(levels.GetRange(5, 5).Cast<VisualElement>().ToList()),
            new NavigationRow(blockSize.GetRange(0, 5).Cast<VisualElement>().ToList(), 0),
            new NavigationRow(blockSize.GetRange(5, 5).Cast<VisualElement>().ToList()),
            new NavigationRow(timeLimit.Cast<VisualElement>().ToList(), 0, false),
            new NavigationRow(new NavigationCell(startButton)),
        };
        nav = new NavigationGrid(rows, onSubmit, 0, 1);
        timeLimitRowIndex = 6;

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
        root.style.display = DisplayStyle.Flex;
    }

    public void Hide()
    {
        root.style.display = DisplayStyle.None;
    }

    private void OnStartPressed()
    {
        GameEvents.StartGame(selectedGameMode, selectedStartLevel, selectedBlockSize, selectedTimeLimit);
    }

    public NavigationGrid GetNavigationGrid()
    {
        return nav;
    }

    public void OnCancel()
    {
        screenHandler.RequestPop();
    }
}
