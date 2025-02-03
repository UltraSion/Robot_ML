using UnityEngine;

namespace Controller.AimControllers
{
public class MouseAimDirectionGetter : AimDirectionGetter
{
    public override Quaternion GetAimRotation()
    {
        var lookDir = GetMouseLookDirection(Input.mousePosition);

        return Quaternion.LookRotation(lookDir);
    }

    public override Vector3 GetAimDirection()
    {
        return GetMouseLookDirection(Input.mousePosition);
    }

    private Vector3 GetMouseLookDirection(Vector3 mousePos)
    {
        float width = Screen.width;
        float height = Screen.height;
        float pov = cam.fieldOfView;

        var distance = height * 0.5f / Mathf.Tan(pov * 0.5f * Mathf.Deg2Rad);
        mousePos -= new Vector3(width, height, 0f) * 0.5f;
        var lookTarget = new Vector3(mousePos.x, mousePos.y, distance);
        lookTarget = cam.transform.TransformDirection(lookTarget);

        return lookTarget;
    }
}
}
