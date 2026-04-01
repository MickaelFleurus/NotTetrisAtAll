
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
    private EGameTimeLimit mSelectedTimeLimit = EGameTimeLimit.None;

    public GameModePanel(VisualElement settingsPanel)
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
        mStartButton.Focus();

        mBackButton.clicked += Hide;
        mStartButton.clicked += OnStartPressed;
        foreach (var elem in mGameModes)
        {
            elem.clicked += () =>
            {
                mSelectedGameMode = Enum.Parse<EGameMode>(elem.name);
                SetChoiceAsSelected(mGameModes, elem);
                if (mSelectedGameMode == EGameMode.TimeLimit)
                {
                    mTimeLimitLabel.style.display = DisplayStyle.Flex;
                    mTimeLimitParent.style.display = DisplayStyle.Flex;
                }
                else
                {
                    mTimeLimitLabel.style.display = DisplayStyle.None;
                    mTimeLimitParent.style.display = DisplayStyle.None;
                }
            };
        }

        foreach (var elem in mLevels)
        {
            elem.clicked += () =>
            {
                mSelectedStartLevel = int.Parse(elem.name);
                SetChoiceAsSelected(mLevels, elem, true);
            };
        }
        foreach (var elem in mBlockSize)
        {
            elem.clicked += () =>
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
            };
        }
        foreach (var elem in mTimeLimit)
        {
            elem.clicked += () =>
            {
                int indexChoice = int.Parse(elem.name);
                mSelectedTimeLimit = (EGameTimeLimit)indexChoice;
                SetChoiceAsSelected(mTimeLimit, elem, true);
            };
        }

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

    }

    public void Hide()
    {
        mGameModePanel.style.display = DisplayStyle.None;
        OnClosed.Invoke();
    }

    private void OnStartPressed()
    {
        OnStarted.Invoke(mSelectedGameMode, mSelectedStartLevel, mSelectedBlockSize, mSelectedTimeLimit);
    }

}
