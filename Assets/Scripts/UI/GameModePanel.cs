
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class GameModePanel
{
    private VisualElement mGameModePanel;

    private AnimatedButton mBackButton;

    // Game Modes
    private List<AnimatedButton> mGameModes = new List<AnimatedButton>();

    // Level Select
    private List<AnimatedButton> mLevels;

    // Block size
    private List<AnimatedButton> mBlockSize;
    private Label mBlockSizeWarning;


    // Time Limit
    private Label mTimeLimitLabel;
    private VisualElement mTimeLimitParent;
    private List<AnimatedButton> mTimeLimit;

    private AnimatedButton mStartButton;

    public event Action OnClosed;
    public event Action<EGameMode, int, int, EGameTimeLimit> OnStarted;

    private EGameMode mSelectedGameMode = EGameMode.Marathon;
    private int mSelectedStartLevel = 1;
    private int mSelectedBlockSize = 4;
    private EGameTimeLimit mSelectedTimeLimit = EGameTimeLimit.One;
    NavigationGrid mPageNavigation;
    int mTimeLimitRowIndex = 6;


    public GameModePanel(VisualElement settingsPanel, MonoBehaviour coroutineRunner)
    {
        mGameModePanel = settingsPanel;
        mBackButton = mGameModePanel.Q<AnimatedButton>("BackButton");

        mGameModes.Add(mGameModePanel.Q<AnimatedButton>("Marathon"));
        mGameModes.Add(mGameModePanel.Q<AnimatedButton>("TimeLimit"));
        mGameModes.Add(mGameModePanel.Q<AnimatedButton>("Infinite"));

        mLevels = mGameModePanel.Q<VisualElement>("LevelStartParent").Children().OfType<AnimatedButton>().ToList();
        mBlockSize = mGameModePanel.Q<VisualElement>("BlockSizeParent").Children().OfType<AnimatedButton>().ToList();
        mBlockSizeWarning = mGameModePanel.Q<Label>("BlockSizeWarning");
        mTimeLimitLabel = mGameModePanel.Q<Label>("TimeLimitLabel");
        mTimeLimitParent = mGameModePanel.Q<VisualElement>("TimeLimitParent");
        mTimeLimit = mTimeLimitParent.Children().OfType<AnimatedButton>().ToList();
        mStartButton = mGameModePanel.Q<AnimatedButton>("StartButton");


        Dictionary<VisualElement, Action> submitActions = new Dictionary<VisualElement, Action>
        {
            { mBackButton, Hide },
            { mStartButton, OnStartPressed }
        };

        foreach (var elem in mGameModes)
        {
            submitActions.Add(elem, () =>
             {
                 mSelectedGameMode = Enum.Parse<EGameMode>(elem.name);
                 SetChoiceAsSelected(mGameModes, elem);
                 if (mSelectedGameMode == EGameMode.TimeLimit)
                 {
                     mTimeLimitLabel.style.display = DisplayStyle.Flex;
                     mTimeLimitParent.style.display = DisplayStyle.Flex;
                     mPageNavigation.EnableRow(mTimeLimitRowIndex);
                 }
                 else
                 {
                     mTimeLimitLabel.style.display = DisplayStyle.None;
                     mTimeLimitParent.style.display = DisplayStyle.None;
                     mPageNavigation.DisableRow(mTimeLimitRowIndex);
                 }
             });
        }

        foreach (var elem in mLevels)
        {
            submitActions.Add(elem, () =>
            {
                mSelectedStartLevel = int.Parse(elem.name);
                SetChoiceAsSelected(mLevels, elem, true);
                mPageNavigation.SelectColumnAsDefault();
            });
        }
        foreach (var elem in mBlockSize)
        {
            submitActions.Add(elem, () =>
            {
                SetChoiceAsSelected(mBlockSize, elem, true);
                mSelectedBlockSize = int.Parse(elem.text);
                if (mSelectedBlockSize < 3 || mSelectedBlockSize > 5)
                {
                    mBlockSizeWarning.style.display = DisplayStyle.Flex;
                }
                else
                {
                    mBlockSizeWarning.style.display = DisplayStyle.None;
                }
                mPageNavigation.SelectColumnAsDefault();
            });
        }
        foreach (var elem in mTimeLimit)
        {
            submitActions.Add(elem, () =>
            {
                int timeLimit = int.Parse(elem.text);
                mSelectedTimeLimit = IntToGameTimeLimit(timeLimit);
                SetChoiceAsSelected(mTimeLimit, elem, true);
                mPageNavigation.SelectColumnAsDefault();
            });
        }

        SetupNavigation(submitActions, coroutineRunner);
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

    private void SetupNavigation(Dictionary<VisualElement, Action> onSubmit, MonoBehaviour coroutineRunner)
    {
        List<NavigationRow> rows = new List<NavigationRow>() {
            new NavigationRow(new NavigationCell(mBackButton)),
            new NavigationRow(new NavigationCell(mGameModes[0])),
            new NavigationRow(new NavigationCell(mGameModes[1])),
            new NavigationRow(new NavigationCell(mGameModes[2])),
            new NavigationRow(mLevels.Cast<VisualElement>().ToList(), 3),
            new NavigationRow(mBlockSize.Cast<VisualElement>().ToList(), 3),
            new NavigationRow(mTimeLimit.Cast<VisualElement>().ToList(), 0, false),
            new NavigationRow(new NavigationCell(mStartButton)),
        };
        mPageNavigation = new NavigationGrid(rows, coroutineRunner, 0, 1);
        mPageNavigation.SetupSubmitEvent(onSubmit);
        mTimeLimitRowIndex = 6;
        mPageNavigation.RestoreFocus();
        mPageNavigation.cancelPressed += Hide;
    }

    public bool IsShown()
    {
        return mGameModePanel.style.display == DisplayStyle.Flex;
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
        mGameModePanel.style.display = DisplayStyle.Flex;
        mPageNavigation.RestoreFocus();
        mPageNavigation.Enable();
    }

    public void Hide()
    {
        mGameModePanel.style.display = DisplayStyle.None;
        OnClosed.Invoke();
        mPageNavigation.Disable();
    }

    private void OnStartPressed()
    {
        OnStarted.Invoke(mSelectedGameMode, mSelectedStartLevel, mSelectedBlockSize, mSelectedTimeLimit);
    }

}
