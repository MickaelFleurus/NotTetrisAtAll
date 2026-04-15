using System.Linq;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField] Camera mainCamera;
    [SerializeField] RenderTexture renderTexture;


    void Start()
    {
        float aspectRatio = (float)GridHandler.Width / GridHandler.Height;
        float verticalSize = (GridHandler.Height / 2.0f) * 1.5f;
        mainCamera.orthographicSize = verticalSize;
        mainCamera.aspect = aspectRatio;
        mainCamera.transform.position = new Vector3(0.0f, 0.0f, -1.0f);

    }

    // Update is called once per frame
    void Update()
    {

    }

}
