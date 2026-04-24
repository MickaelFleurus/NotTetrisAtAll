using UnityEngine.Rendering;
using UnityEngine.UIElements;

public interface IScreen
{
    public void OnCancel();
    public void Show();
    public void Hide();
    public NavigationGrid GetNavigationGrid();
}

public interface IModal
{
    public void Show(string firstChoice, string secondChoice);
    public VisualElement GetRoot();
    public void OnCancel();
    public NavigationGrid GetNavigationGrid();

}
