using UnityEngine;

namespace Controller.AimControllers
{
public abstract class AimDirectionGetter : MonoBehaviour
{
    public Camera cam;

    public abstract Quaternion GetAimRotation();

    public abstract Vector3 GetAimDirection();

    public void SetCamera(Camera cam)
    {
        this.cam = cam;
    }
}
}
