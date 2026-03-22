using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField] Camera mainCamera;
    [SerializeField] RenderTexture renderTexture;


    void Start()
    {
        float aspectRatio = (float)GridHandler.Width / GridHandler.Height;
        float verticalSize = GridHandler.Height / 2.0f;
        mainCamera.orthographicSize = verticalSize;
        mainCamera.aspect = aspectRatio;
    }

    // Update is called once per frame
    void Update()
    {

    }

}
