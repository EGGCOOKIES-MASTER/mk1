using UnityEngine;

/// <summary>
/// Keeps the target viewport by adjusting the camera rect.
/// Non-16:9 screens show black bars instead of stretching the UI.
/// </summary>
[RequireComponent(typeof(Camera))]
public class AspectRatioController : MonoBehaviour
{
    [SerializeField] private float targetWidth = 1080f;
    [SerializeField] private float targetHeight = 607.76f;

    private Camera targetCamera;
    private int lastScreenWidth;
    private int lastScreenHeight;

    private void Awake()
    {
        targetCamera = GetComponent<Camera>();
        ApplyAspectRatio();
    }

    private void Update()
    {
        if (Screen.width == lastScreenWidth && Screen.height == lastScreenHeight)
        {
            return;
        }

        ApplyAspectRatio();
    }

    public void ApplyAspectRatio()
    {
        if (targetCamera == null)
        {
            targetCamera = GetComponent<Camera>();
        }

        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        float targetAspect = targetWidth / targetHeight;
        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        Rect rect = targetCamera.rect;
        if (scaleHeight < 1f)
        {
            rect.width = 1f;
            rect.height = scaleHeight;
            rect.x = 0f;
            rect.y = (1f - scaleHeight) * 0.5f;
        }
        else
        {
            float scaleWidth = 1f / scaleHeight;
            rect.width = scaleWidth;
            rect.height = 1f;
            rect.x = (1f - scaleWidth) * 0.5f;
            rect.y = 0f;
        }

        targetCamera.rect = rect;
    }
}
