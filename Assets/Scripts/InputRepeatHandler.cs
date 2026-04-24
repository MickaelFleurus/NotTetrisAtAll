using System;
using System.Collections;
using UnityEngine;

public class InputRepeatHandler
{
    private float repeatDelay;
    private Coroutine coroutine = null;
    private MonoBehaviour coroutineRunner = null;

    public Action Repeat;

    public InputRepeatHandler(MonoBehaviour coroutineRunner, float repeatDelay)
    {
        this.coroutineRunner = coroutineRunner;
        this.repeatDelay = repeatDelay;
    }


    public void Start()
    {
        if (coroutine != null)
            coroutineRunner.StopCoroutine(coroutine);
        coroutine = coroutineRunner.StartCoroutine(RepeatFunc());
    }

    public void Stop()
    {
        if (coroutine != null)
        {
            coroutineRunner.StopCoroutine(coroutine);
            coroutine = null;
        }
    }

    private IEnumerator RepeatFunc()
    {
        while (true)
        {
            Repeat?.Invoke();
            yield return new WaitForSeconds(repeatDelay);
        }
    }

    public bool IsRunning()
    {
        return coroutine != null;
    }

}
