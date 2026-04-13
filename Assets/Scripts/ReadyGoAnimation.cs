using UnityEngine;
using UnityEngine.UIElements;

public class ReadyGoAnimation : MonoBehaviour
{

    [SerializeField] UIDocument inGameUI;
    Label readyLabel;
    Label goLabel;
    readonly string showClassName = "readyGoTextLeft";
    readonly string hideClassName = "readyGoTextDisappear";



    void Awake()
    {
        readyLabel = inGameUI.rootVisualElement.Q<Label>("ReadyLabel");
        goLabel = inGameUI.rootVisualElement.Q<Label>("GoLabel");
    }


    public void OnShowReady()
    {
        readyLabel.RemoveFromClassList(showClassName);
    }

    public void OnDisappearReady()
    {
        readyLabel.AddToClassList(hideClassName);
    }

    public void OnShowGo()
    {
        goLabel.RemoveFromClassList(showClassName);
    }

    public void OnDisappearGo()
    {
        goLabel.AddToClassList(hideClassName);

    }
}
