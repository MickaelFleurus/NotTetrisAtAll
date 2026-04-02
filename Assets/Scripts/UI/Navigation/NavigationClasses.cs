using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;

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
    List<NavigationRow> rows;
    int currentCol = 0;
    int currentRow = 0;

    public NavigationGrid(List<NavigationRow> rows, int startCol = 0, int startRow = 0)
    {
        this.rows = rows;
        currentCol = startCol;
        currentRow = startRow;
        RestoreFocus();
    }


    public bool OnNavigationEvent(NavigationMoveEvent evt)
    {
        bool shouldStopPropagation = true;

        // Vertical move
        if (evt.direction == NavigationMoveEvent.Direction.Up)
        {
            if (currentRow != 0)
            {
                int newRow = rows.FindLastIndex(currentRow - 1, currentRow, (row) => row.GetFirstEnabled() != -1);
                if (newRow != -1)
                {
                    SelectRow(newRow);
                }
            }
        }
        else if (evt.direction == NavigationMoveEvent.Direction.Down)
        {
            if (currentRow < rows.Count - 1)
            {
                int newRow = rows.FindIndex(currentRow + 1, (row) => row.GetFirstEnabled() != -1);
                if (newRow != -1)
                {
                    SelectRow(newRow);
                }
            }
        }

        // Horizontal move

        if (evt.direction == NavigationMoveEvent.Direction.Left)
        {
            if (rows[currentRow].ContainsOnlySlider())
            {
                shouldStopPropagation = false;
            }
            else if (currentCol != 0)
            {
                int newColumn = rows[currentRow].cells.FindLastIndex(currentCol - 1, currentCol, (cell) => cell.enabled);
                if (newColumn != -1)
                {
                    SelectColumn(newColumn);
                }
            }
        }
        else if (evt.direction == NavigationMoveEvent.Direction.Right)
        {
            if (rows[currentRow].ContainsOnlySlider())
            {
                shouldStopPropagation = false;
            }
            else if (currentCol < rows[currentRow].cells.Count - 1)
            {
                int newColumn = rows[currentRow].cells.FindIndex(currentCol + 1, (cell) => cell.enabled);
                if (newColumn != -1)
                {
                    SelectColumn(newColumn);
                }
            }
        }

        if (shouldStopPropagation)
        {
            evt.StopImmediatePropagation();
            evt.StopPropagation();
            return true;
        }
        return false;
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

}
