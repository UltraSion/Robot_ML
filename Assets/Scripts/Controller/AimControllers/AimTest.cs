using UnityEngine;

namespace Controller.AimControllers
{
public class AimTest : MonoBehaviour
{
    public Camera cam;

    void Update()
    {
        float width = Screen.width;
        float height = Screen.height;

        var pov = cam.fieldOfView;
        var pov2 = Mathf.Atan(width / height * Mathf.Tan(pov / 2 * Mathf.Deg2Rad)) * Mathf.Rad2Deg;

        var distance = height / 2 * Mathf.Tan(pov / 2 * Mathf.Deg2Rad);

        // Debug.Log($"width: {width}, height: {height}, pov1: {pov},pov2: {pov2 * 2}, Distance: {distance}");

        var rot = GetMouseLookRotation(Input.mousePosition);

        Debug.Log(rot);
    }

    public Vector3 GetMouseLookRotation(Vector3 mousePos)
    {
        float width = Screen.width;
        float height = Screen.height;

        var pov = cam.fieldOfView;

        var distance = height / 2 * Mathf.Tan(pov / 2 * Mathf.Deg2Rad);
        mousePos -= new Vector3(width, height, 0f) * 0.5f;
        Debug.Log(mousePos);

        float x, y;
        x = Mathf.Atan(mousePos.y / distance) * Mathf.Rad2Deg;
        y = Mathf.Atan(mousePos.x / distance) * Mathf.Rad2Deg;

        return new Vector3(x, y);
    }
}
}
