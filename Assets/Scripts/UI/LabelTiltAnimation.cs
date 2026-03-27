using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

class SAnimationLoop
{
    public float duration;
    public float elapsedTime;

    public SAnimationLoop(float duration)
    {
        this.duration = duration;
        elapsedTime = 0.0f;
    }
    public bool ShouldTrigger()
    {
        return elapsedTime >= duration;
    }
}

class TiltAnimation
{

    VisualElement mElement;
    Angle mTiltAmount;
    Scale mScaleAmount;

    SAnimationLoop mRotate = null;
    SAnimationLoop mScale = null;

    bool isValid = true;


    public TiltAnimation(VisualElement element)
    {
        mElement = element;
        var animations = mElement.resolvedStyle.transitionProperty;
        var durations = mElement.resolvedStyle.transitionDuration;
        int i = 0;
        foreach (var name in animations)
        {
            if (name == "rotate")
            {
                mRotate = new SAnimationLoop(durations.ElementAt(i).value);
                mTiltAmount = mElement.resolvedStyle.rotate.angle;
            }
            else if (name == "scale")
            {
                mScale = new SAnimationLoop(durations.ElementAt(i).value);
                mScaleAmount = mElement.resolvedStyle.scale.value;
            }
            i++;
        }
    }

    public void Update(float elapsedTime)
    {
        if (mRotate != null)
        {
            mRotate.elapsedTime += elapsedTime;
            if (mRotate.ShouldTrigger())
            {
                mTiltAmount.value *= -1.0f;
                mElement.style.rotate = new StyleRotate(mTiltAmount);

                mRotate.elapsedTime = 0.0f;
            }
        }
        if (mScale != null)
        {
            mScale.elapsedTime += elapsedTime;
            if (mScale.ShouldTrigger())
            {
                if (Equals(mScaleAmount, Vector2.one))
                {

                }

                mElement.style.scale = new StyleScale(mScaleAmount);

                mScale.elapsedTime = 0.0f;
            }
        }
    }
}
