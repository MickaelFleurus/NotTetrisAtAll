using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField] Camera mainCamera;
    [SerializeField] RenderTexture renderTexture;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        float aspectRatio = (float)GridHandler.Width / GridHandler.Height;
        float verticalSize = GridHandler.Height / 2.0f + 1.0f; // add some padding
        mainCamera.orthographicSize = verticalSize;
        mainCamera.aspect = aspectRatio;
    }

    // Update is called once per frame
    void Update()
    {

    }

}
