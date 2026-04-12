using System;
using UnityEngine;

class Timer
{
    public enum ETimerCountDirection { Up, Down };
    private float current;
    private ETimerCountDirection direction;
    private bool isRunning = false;
    public Action isDone;


    public Timer(ETimerCountDirection direction, float amountMinute = 0f)
    {
        this.direction = direction;
        current = amountMinute * 60;
    }

    public void Start()
    {
        isRunning = true;
    }

    public void Update(float timeElapsed)
    {
        if (!isRunning)
            return;

        if (direction == ETimerCountDirection.Up)
        {
            current += timeElapsed;
        }
        else
        {
            current -= timeElapsed;
        }
        if (current <= 0)
        {
            isDone?.Invoke();
        }
    }

    public string GetTime()
    {
        int minutes = (int)(current / 60f);
        int seconds = (int)(current % 60f);
        int centiseconds = (int)((current % 1f) * 100f);
        return string.Format("{0:D2}:{1:D2}:{2:D2}", minutes, seconds, centiseconds);
    }

}
