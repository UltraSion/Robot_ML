using UnityEngine;

namespace Controller.AimControllers
{
public class MouseAimDirectionGetter : AimDirectionGetter
{
    public Camera cam;

    public override Quaternion GetAimDirection()
    {
        var rot = transform.localRotation.eulerAngles;
        var delta = GetMouseLookRotation(Input.mousePosition);

        rot.x += delta.x;
        rot.y += delta.y;
        return Quaternion.Euler(rot);
    }

    private Vector3 GetMouseLookRotation(Vector3 mousePos)
    {
        float width = Screen.width;
        float height = Screen.height;
        float pov = cam.fieldOfView;
        mousePos -= new Vector3(width, height, 0f) * 0.5f;
        var distance = height * 0.5f / Mathf.Tan(pov * 0.5f * Mathf.Deg2Rad);
        var lookTarget = new Vector3(mousePos.x, mousePos.y, distance);

        return Quaternion.LookRotation(lookTarget).eulerAngles;
    }
}
}
