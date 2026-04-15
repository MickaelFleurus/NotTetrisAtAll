using UnityEngine;
using UnityEngine.InputSystem;

public class InputRepeatHandler
{
    private float repeatDelay;
    private float timeSinceLastInput = 0f;

    public InputRepeatHandler(float repeatDelay)
    {
        this.repeatDelay = repeatDelay;
        timeSinceLastInput = repeatDelay;
    }

    public bool ShouldRepeat(float value, float deltaTime)
    {
        if (Mathf.Abs(value) > 0.5f)
        {
            timeSinceLastInput += deltaTime;
            if (timeSinceLastInput >= repeatDelay)
            {
                timeSinceLastInput = 0f;
                return true;
            }
            return false;
        }
        else
        {
            timeSinceLastInput = repeatDelay;
            return false;
        }
    }
}
