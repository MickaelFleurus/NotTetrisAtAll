using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class AnimatedButton : Button, IAnimatedElement
{
    private UIAnimationData data;
    private UIAnimationLogic logic;
    // Animation toggles
    [UxmlAttribute]
    public AnimationActivation rotationActivation { get => data.rotationActivation; set { data.rotationActivation = value; logic.TryPlay(); } }
    // Rotation parameters
    [UxmlAttribute]
    public float minRotation { get => data.minRotation; set => data.minRotation = value; }

    [UxmlAttribute]
    public float maxRotation { get => data.maxRotation; set => data.maxRotation = value; }

    [UxmlAttribute]
    public float rotationDuration
    {
        get => data.rotationDuration;
        set => data.rotationDuration = Mathf.Max(0.01f, value);
    }

    [UxmlAttribute]
    public AnimationMode rotationMode
    {
        get => data.rotationMode;
        set => data.rotationMode = value;
    }

    [UxmlAttribute]
    public AnimationActivation scaleActivation { get => data.scaleActivation; set { data.scaleActivation = value; logic.TryPlay(); } }
    [UxmlAttribute]
    public float minScale { get => data.minScale; set => data.minScale = value; }

    [UxmlAttribute]
    public float maxScale { get => data.maxScale; set => data.maxScale = value; }

    [UxmlAttribute]
    public float scaleDuration
    {
        get => data.scaleDuration;
        set => data.scaleDuration = Mathf.Max(0.01f, value);
    }

    [UxmlAttribute]
    public AnimationMode scaleMode
    {
        get => data.scaleMode;
        set => data.scaleMode = value;
    }

    public void ApplyPreset(UIAnimationData preset)
    {
        if (preset == null)
            return;

        data.Copy(preset);
    }

    public AnimatedButton()
    {
        RegisterCallback<FocusInEvent>(OnFocusIn);
        RegisterCallback<FocusOutEvent>(OnFocusOut);
        data = ScriptableObject.CreateInstance<UIAnimationData>();
        logic = new UIAnimationLogic(ref data, this);
    }

    private void OnFocusIn(FocusInEvent evt)
    {
        logic.OnFocusIn(evt);
    }

    private void OnFocusOut(FocusOutEvent evt)
    {
        logic.OnFocusOut(evt);
    }


    public void Update(float deltaTime)
    {
        logic.Update(deltaTime);
    }

    public AnimatedElementType GetAnimatedType()
    {
        return AnimatedElementType.Button;
    }

    public void Copy(UIAnimationData other)
    {
        data.Copy(other);
    }
}
