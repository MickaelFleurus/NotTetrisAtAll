using UnityEngine;
using UnityEngine.UIElements;

public class UIAnimationLogic
{

    private bool mIsPlaying = false;
    private UIAnimationData data;
    private VisualElement visualElement;
    private float mRotationElapsedTime = 0f;
    private float mScaleElapsedTime = 0f;
    private Angle mBaseRotation;
    private Scale mBaseScale;

    public UIAnimationLogic(ref UIAnimationData data, VisualElement element)
    {
        this.data = data;
        visualElement = element;

        mBaseRotation = visualElement.style.rotate.value.angle;
        mBaseScale = visualElement.style.scale.value;

    }

    public void TryPlay()
    {
        if (data.AnyAnimationAlways())
        {
            Play();
        }
    }

    public void Update(float deltaTime)
    {
        if (!mIsPlaying)
            return;

        if (data.rotationActivation != AnimationActivation.Off)
        {
            mRotationElapsedTime += deltaTime;
            float rotationProgress = CalculateAnimationProgress(mRotationElapsedTime, data.rotationDuration, data.rotationMode);
            float minRotationOffset = data.minRotation - mBaseRotation.value;
            float maxRotationOffset = data.maxRotation - mBaseRotation.value;
            float rotationOffset = Mathf.Lerp(minRotationOffset, maxRotationOffset, rotationProgress);
            float rotation = mBaseRotation.value + rotationOffset;
            visualElement.style.rotate = new StyleRotate(new Angle(rotation));
        }

        if (data.scaleActivation != AnimationActivation.Off)
        {
            mScaleElapsedTime += deltaTime;
            float scaleProgress = CalculateAnimationProgress(mScaleElapsedTime, data.scaleDuration, data.scaleMode);
            float minScaleOffset = data.minScale - mBaseScale.value.x;
            float maxScaleOffset = data.maxScale - mBaseScale.value.x;
            float scaleOffset = Mathf.Lerp(minScaleOffset, maxScaleOffset, scaleProgress);
            float scale = mBaseScale.value.x + scaleOffset;
            visualElement.style.scale = new StyleScale(new Vector2(scale, scale));
        }
    }
    private float CalculateAnimationProgress(float elapsedTime, float duration, AnimationMode mode)
    {
        if (duration <= 0)
            return 0f;

        float cycleDuration = mode == AnimationMode.Oscillate ? duration * 2f : duration;
        float normalizedTime = (elapsedTime % cycleDuration) / cycleDuration;

        if (mode == AnimationMode.Oscillate)
        {
            // Offset by 0.25 to start from center (progress 0.5)
            normalizedTime = (normalizedTime + 0.25f) % 1f;

            // Smooth oscillation with zero velocity at extremes
            if (normalizedTime < 0.5f)
            {
                float t = normalizedTime * 2f;
                return -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f;
            }
            else
            {
                float t = (normalizedTime - 0.5f) * 2f;
                return 1f - (-(Mathf.Cos(Mathf.PI * t) - 1f) / 2f);
            }
        }
        else
        {
            // Linear loop: 0 → 1 continuously
            return normalizedTime;
        }
    }

    public void OnFocusIn(FocusInEvent evt)
    {
        if (data.AnyAnimationOnFocus())
        {
            Play();
        }
    }

    public void OnFocusOut(FocusOutEvent evt)
    {
        if (data.AnyAnimationOnFocus())
        {
            Stop();
            Reset();
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

        visualElement.style.scale = new StyleScale(mBaseScale);
        visualElement.style.rotate = new StyleRotate(mBaseRotation);

    }
}
