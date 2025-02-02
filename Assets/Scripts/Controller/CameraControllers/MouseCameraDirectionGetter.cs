using UnityEngine;

namespace Controller.CameraControllers
{
public class MouseCameraDirectionGetter : CameraDirectionGetter
{
    private Vector3 _rot;

    void Start()
    {
        _rot = transform.rotation.eulerAngles;
    }
    public override Quaternion GetLookDirection()
    {
        var xDelta = Input.GetAxisRaw("Mouse X") * Sensitivity * Time.deltaTime;

        var yDelta = Input.GetAxisRaw("Mouse Y") * Sensitivity * Time.deltaTime;

        _rot += new Vector3(-yDelta, xDelta, 0f) * 90f;
        _rot.x = Mathf.Clamp(_rot.x, -90f, 90f);

        return Quaternion.Euler(_rot);
    }
}
}
