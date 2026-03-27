using UnityEngine;
using UnityEngine.UIElements;


[UxmlElement]
public partial class AnimatedLabel : Label
{
    public enum AnimationMode
    {
        Oscillate,
        Loop
    }

    private float mRotationElapsedTime = 0f;
    private float mRotationDuration = 1f;
    private AnimationMode mRotationMode = AnimationMode.Oscillate;

    private float mScaleElapsedTime = 0f;
    private float mScaleDuration = 3f;
    private AnimationMode mScaleMode = AnimationMode.Loop;

    private bool mIsPlaying = true;

    [UxmlAttribute]
    public bool animateRotation = false;
    [UxmlAttribute]
    public float minRotation { get; set; } = 0.0f;

    [UxmlAttribute]
    public float maxRotation { get; set; } = 0.0f;

    [UxmlAttribute]
    public float rotationDuration
    {
        get => mRotationDuration;
        set => mRotationDuration = value;
    }

    [UxmlAttribute]
    public AnimationMode rotationMode
    {
        get => mRotationMode;
        set => mRotationMode = value;
    }

    // Scale parameters
    [UxmlAttribute]
    public float minScale { get; set; } = 0.8f;

    [UxmlAttribute]
    public float maxScale { get; set; } = 1.2f;

    [UxmlAttribute]
    public float scaleDuration
    {
        get => mScaleDuration;
        set => mScaleDuration = Mathf.Max(0.01f, value);
    }

    [UxmlAttribute]
    public AnimationMode scaleMode
    {
        get => mScaleMode;
        set => mScaleMode = value;
    }

    public bool IsPlaying
    {
        get => mIsPlaying;
        set => mIsPlaying = value;
    }

    public void Update(float deltaTime)
    {
        if (!mIsPlaying)
            return;

        mRotationElapsedTime += deltaTime;
        mScaleElapsedTime += deltaTime;

        // Calculate progress for each animation independently
        float rotationProgress = CalculateAnimationProgress(mRotationElapsedTime, mRotationDuration, mRotationMode);
        float scaleProgress = CalculateAnimationProgress(mScaleElapsedTime, mScaleDuration, mScaleMode);

        // Calculate rotation and scale values
        float rotation = Mathf.Lerp(minRotation, maxRotation, rotationProgress);
        float scale = Mathf.Lerp(minScale, maxScale, scaleProgress);

        // Apply transforms
        style.rotate = new StyleRotate(new Angle(rotation));
        style.scale = new StyleScale(new Vector2(scale, scale));
    }

    private float CalculateAnimationProgress(float elapsedTime, float duration, AnimationMode mode)
    {
        if (duration <= 0)
            return 0f;

        float cycleDuration = mode == AnimationMode.Oscillate ? duration * 2f : duration;
        float normalizedTime = (elapsedTime % cycleDuration) / cycleDuration;

        if (mode == AnimationMode.Oscillate)
        {
            // Smooth oscillation with zero velocity at extremes
            // Split into two halves with ease-in-out cosine for each
            if (normalizedTime < 0.5f)
            {
                float t = normalizedTime * 2f;  // 0 to 1 for first half
                return -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f;  // Ease 0 to 1
            }
            else
            {
                float t = (normalizedTime - 0.5f) * 2f;  // 0 to 1 for second half
                return 1f - (-(Mathf.Cos(Mathf.PI * t) - 1f) / 2f);  // Ease 1 to 0
            }
        }
        else
        {
            // Linear loop: 0 → 1 continuously
            return normalizedTime;
        }
    }

    public void Play()
    {
        mIsPlaying = true;
        mRotationElapsedTime = 0f;
        mScaleElapsedTime = 0f;
    }

    public void Stop()
    {
        mIsPlaying = false;
    }

    public void Reset()
    {
        mRotationElapsedTime = 0f;
        mScaleElapsedTime = 0f;
    }
}
