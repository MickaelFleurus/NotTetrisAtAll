using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

public class StartScreenUI : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private UIAnimationData buttonsAnimationData;
    [SerializeField] private UIAnimationData slidersAnimationData;
    [SerializeField] private UIAnimationData titleAnimationData;
    ScreenHandler screenHandler;

    private List<IAnimatedElement> AnimatedElements = new List<IAnimatedElement>();

    private void SearchInChildren<T>(VisualElement parent, List<T> container) where T : class
    {
        if (parent is T)
        {
            container.Add(parent as T);
        }
        foreach (var element in parent.Children())
        {
            SearchInChildren<T>(element, container);
        }
    }

    void Awake()
    {
        var rootVisualElement = uiDocument.rootVisualElement;

        SearchInChildren(rootVisualElement, AnimatedElements);
        foreach (var animated in AnimatedElements)
        {
            switch (animated.GetAnimatedType())
            {
                case AnimatedElementType.Title:
                    animated.Copy(titleAnimationData);
                    break;
                case AnimatedElementType.Slider:
                    animated.Copy(slidersAnimationData);
                    break;
                case AnimatedElementType.Button:
                    animated.Copy(buttonsAnimationData);
                    break;
            }
        }
        screenHandler = new ScreenHandler(uiDocument);
        screenHandler.RequestShow(ScreenHandler.EScreens.StartMenu);
    }

    void Start()
    {
        AudioMixer.Instance.PlayMusic(AudioData.Instance.MainMenuMusic);
    }

    void Update()
    {
        foreach (var element in AnimatedElements)
        {
            element.Update(Time.deltaTime);
        }
    }

    void OnDestroy()
    {
        screenHandler.OnDestroy();
    }
}
