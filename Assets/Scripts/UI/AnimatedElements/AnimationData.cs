using UnityEngine;


[CreateAssetMenu(fileName = "UIAnimationData", menuName = "UI/Animation Data")]
public class UIAnimationData : ScriptableObject
{

    [Header("Animation Toggles")]
    public AnimationActivation rotationActivation = AnimationActivation.Off;
    public AnimationActivation scaleActivation = AnimationActivation.Off;

    [Header("Rotation Settings")]
    public float minRotation = -15f;
    public float maxRotation = 15f;
    public float rotationDuration = 1f;
    public AnimationMode rotationMode = AnimationMode.Oscillate;

    [Header("Scale Settings")]
    public float minScale = 0.8f;
    public float maxScale = 1.2f;
    public float scaleDuration = 3f;
    public AnimationMode scaleMode = AnimationMode.Loop;

    public bool AnyAnimationOnFocus()
    {
        return rotationActivation == AnimationActivation.OnFocusIn || scaleActivation == AnimationActivation.OnFocusIn;
    }

    public bool AnyAnimationAlways()
    {
        return rotationActivation == AnimationActivation.Always || scaleActivation == AnimationActivation.Always;
    }

    public void Copy(UIAnimationData other)
    {
        rotationActivation = other.rotationActivation;
        rotationDuration = other.rotationDuration;
        rotationMode = other.rotationMode;
        minRotation = other.minRotation;
        maxRotation = other.maxRotation;

        scaleActivation = other.scaleActivation;
        scaleDuration = other.scaleDuration;
        scaleMode = other.scaleMode;
        minScale = other.minScale;
        maxScale = other.maxScale;

    }
}
