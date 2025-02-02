using UnityEngine;

namespace Controller.CameraControllers
{
public abstract class CameraDirectionGetter : MonoBehaviour
{
    [SerializeField]
    private float sensitivity = 0.5f;

    public float Sensitivity
    {
        get => sensitivity;
        set
        {
            if (value is <= 0 or > 1)
                return;
            sensitivity = value;
        }
    }

    public abstract Quaternion GetLookDirection();
}
}
