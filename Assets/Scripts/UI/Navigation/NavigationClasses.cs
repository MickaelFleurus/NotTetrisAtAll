using System;
using System.Collections.Generic;

using UnityEngine.UIElements;

class NavigationCell
{
    public bool enabled = true;
    public VisualElement element = null;

    public NavigationCell(VisualElement element, bool enabled = true)
    {
        this.element = element;
        this.enabled = enabled;
    }
}

class NavigationRow
{
    public int activeCellColumn = 0;
    public List<NavigationCell> cells;


    public NavigationRow(List<VisualElement> elements, int activeCell = 0, bool enabled = true)
    {
        cells = new List<NavigationCell>();
        activeCellColumn = activeCell;
        foreach (var element in elements)
        {
            cells.Add(new NavigationCell(element, enabled));
        }
    }

    public NavigationRow(NavigationCell cell)
    {
        cells = new List<NavigationCell>() { cell };
    }

    public NavigationRow(List<NavigationCell> cells)
    {
        this.cells = cells;
    }

    public void Enable()
    {
        foreach (var cell in cells)
        {
            cell.enabled = true;
        }
    }

    public void Disable()
    {
        foreach (var cell in cells)
        {
            cell.enabled = false;
        }
    }

    public int GetFirstEnabled()
    {
        return cells.FindIndex((cell) => cell.enabled);
    }

    public bool ContainsOnlySlider()
    {
        return cells.Count == 1 && cells[0].element is Slider;
    }
}

class NavigationGrid
{
    Dictionary<VisualElement, Action> onSubmitFuncs;
    List<NavigationRow> rows;
    int currentCol = 0;
    int currentRow = 0;

    bool enabled = false;
    static bool hasCreatedCallbacks = false;
    static List<NavigationGrid> registeredInstances = new List<NavigationGrid>();

    public Action cancelPressed;

    public NavigationGrid(List<NavigationRow> rows, Dictionary<VisualElement, Action> submitFuncs, int startCol = 0, int startRow = 0)
    {
        if (!hasCreatedCallbacks)
        {
            PlayerInputs.Instance.uiActions.Enable();

            // Register static event handlers once
            PlayerInputs.Instance.uiActions.Approve += OnSubmitStatic;
            PlayerInputs.Instance.uiActions.Cancel += OnCancelStatic;
            PlayerInputs.Instance.uiActions.Navigate += OnNavigateStatic;
            hasCreatedCallbacks = true;
        }

        registeredInstances.Add(this);

        this.rows = rows;
        currentCol = startCol;
        currentRow = startRow;
        onSubmitFuncs = submitFuncs;

        RestoreFocus();
    }

    ~NavigationGrid()
    {
        cancelPressed = null;
        registeredInstances.Remove(this);
    }

    // Static event handlers that dispatch to enabled instances
    private static void OnSubmitStatic()
    {
        foreach (var instance in registeredInstances)
        {
            if (instance.enabled)
            {
                instance.OnSubmit();
                break;
            }
        }
    }

    private static void OnCancelStatic()
    {
        foreach (var instance in registeredInstances)
        {
            if (instance.enabled)
            {
                instance.cancelPressed?.Invoke();
                break;
            }
        }
    }

    private static void OnNavigateStatic(UnityEngine.Vector2 nav)
    {
        foreach (var instance in registeredInstances)
        {
            if (instance.enabled)
            {
                instance.OnNavigate(nav);
                break;
            }
        }
    }

    private void OnSubmit()
    {
        if (onSubmitFuncs.Count == 0) return;
        var element = rows[currentRow].cells[currentCol].element;
        if (onSubmitFuncs.ContainsKey(element))
        {
            AudioMixer.Instance.PlaySFX(AudioData.Instance.MenuNavigationSfx);
            onSubmitFuncs[element]();
        }
    }

    public void Enable()
    {
        enabled = true;
    }

    public void Disable()
    {
        enabled = false;
    }

    private void OnNavigate(UnityEngine.Vector2 movementVal)
    {
        if (!enabled) return;
        bool hasMoved = false;

        NavigationMoveEvent.Direction direction = NavigationMoveEvent.Direction.None;
        if (Math.Abs(movementVal.x) > 0.75f)
        {
            direction = movementVal.x < 0f ? NavigationMoveEvent.Direction.Left : NavigationMoveEvent.Direction.Right;
        }
        else if (Math.Abs(movementVal.y) > 0.75f)
        {
            direction = movementVal.y < 0f ? NavigationMoveEvent.Direction.Down : NavigationMoveEvent.Direction.Up;
        }
        if (direction == NavigationMoveEvent.Direction.None) return;
        // Vertical move
        if (direction == NavigationMoveEvent.Direction.Up)
        {
            if (currentRow != 0)
            {
                int newRow = rows.FindLastIndex(currentRow - 1, currentRow, (row) => row.GetFirstEnabled() != -1);
                if (newRow != -1)
                {
                    hasMoved = true;
                    SelectRow(newRow);
                }
            }
        }
        else if (direction == NavigationMoveEvent.Direction.Down)
        {
            if (currentRow < rows.Count - 1)
            {
                int newRow = rows.FindIndex(currentRow + 1, (row) => row.GetFirstEnabled() != -1);
                if (newRow != -1)
                {
                    hasMoved = true;
                    SelectRow(newRow);
                }
            }
        }

        // Horizontal move

        if (direction == NavigationMoveEvent.Direction.Left)
        {
            if (rows[currentRow].ContainsOnlySlider())
            {
                Slider slider = rows[currentRow].cells[currentCol].element as Slider;
                slider.value = Math.Max(slider.value - 1, slider.lowValue);
            }
            else if (currentCol != 0)
            {
                int newColumn = rows[currentRow].cells.FindLastIndex(currentCol - 1, currentCol, (cell) => cell.enabled);
                if (newColumn != -1)
                {
                    hasMoved = true;
                    SelectColumn(newColumn);
                }
            }
        }
        else if (direction == NavigationMoveEvent.Direction.Right)
        {
            if (rows[currentRow].ContainsOnlySlider())
            {
                Slider slider = rows[currentRow].cells[currentCol].element as Slider;
                slider.value = Math.Min(slider.value + 1, slider.highValue);
            }
            else if (currentCol < rows[currentRow].cells.Count - 1)
            {
                int newColumn = rows[currentRow].cells.FindIndex(currentCol + 1, (cell) => cell.enabled);
                if (newColumn != -1)
                {
                    hasMoved = true;
                    SelectColumn(newColumn);
                }
            }
        }

        if (hasMoved)
        {
            AudioMixer.Instance.PlaySFX(AudioData.Instance.MenuNavigationSfx);
        }
    }

    public void RestoreFocus()
    {
        rows[currentRow].cells[currentCol].element.Focus();
    }

    private void SelectRow(int row)
    {
        currentRow = row;
        var selectedRow = rows[row];
        if (selectedRow.cells[selectedRow.activeCellColumn].enabled)
        {
            currentCol = selectedRow.activeCellColumn;
        }
        else
        {
            currentCol = selectedRow.cells.FindIndex((cell) => cell.enabled);
        }
        RestoreFocus();
    }

    private void SelectColumn(int col)
    {
        currentCol = col;

        RestoreFocus();
    }

    public void SelectColumnAsDefault()
    {
        rows[currentRow].activeCellColumn = currentCol;
    }

    public void EnableRow(int index)
    {
        rows[index].Enable();
    }

    public void DisableRow(int index)
    {
        rows[index].Disable();
    }

    public static void ResetInputAction()
    {
        registeredInstances.Clear();
    }

}
