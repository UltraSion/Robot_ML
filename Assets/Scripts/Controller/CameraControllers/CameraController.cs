using UnityEngine;

namespace Controller.CameraControllers
{
public class CameraController : MonoBehaviour
{
    public CameraDirectionGetter cameraDirectionGetter;

    public float Sensitivity
    {
        get => cameraDirectionGetter.Sensitivity;
        set => cameraDirectionGetter.Sensitivity = value;
    }

    public void Awake()
    {
        cameraDirectionGetter = GetComponent<CameraDirectionGetter>();
    }

    public void Update()
    {
        transform.rotation = cameraDirectionGetter.GetLookDirection();
    }
}
}
