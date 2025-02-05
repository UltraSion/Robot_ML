using System;
using UnityEngine;

namespace Turret.TurretControllers
{
public class TurretAimControl : MonoBehaviour
{
    public enum ViewMode
    {
        HeadOnly,
        CrossHairOnly,
        Normal
    }

    public Vector3 lookRot;
    public Turret turret;

    public ViewMode viewMode;
    public float Sensitivity;
    private Camera _camera;

    private void Start()
    {
        _camera = Camera.main;
        viewMode = ViewMode.Normal;

    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            Cursor.lockState = CursorLockMode.Locked;
            UpdateLookRot();
            _camera.transform.rotation = Quaternion.Euler(lookRot);
        }
        else if (Input.GetMouseButton(1))
        {
            Cursor.lockState = CursorLockMode.Confined;
            var lookDir = GetMouseLookDirection(Input.mousePosition);
            turret.TargetLookDir(lookDir);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            UpdateLookRot();
            _camera.transform.rotation = Quaternion.Euler(lookRot);
            turret.TargetLookDir(GetMouseLookDirection(new Vector3(Screen.width, Screen.height, 0f) * 0.5f));
        }
    }

    private void UpdateLookRot()
    {
        var xDelta = Input.GetAxisRaw("Mouse X") * Sensitivity * Time.deltaTime;

        var yDelta = Input.GetAxisRaw("Mouse Y") * Sensitivity * Time.deltaTime;

        lookRot += new Vector3(-yDelta, xDelta, 0f) * 90f;
        lookRot.x = Mathf.Clamp(lookRot.x, -90f, 90f);
    }

    private Vector3 GetMouseLookDirection(Vector3 mousePos)
    {
        float width = Screen.width;
        float height = Screen.height;
        float pov = _camera.fieldOfView;

        var distance = height * 0.5f / Mathf.Tan(pov * 0.5f * Mathf.Deg2Rad);
        mousePos -= new Vector3(width, height, 0f) * 0.5f;

        var lookTarget = new Vector3(mousePos.x, mousePos.y, distance);
        lookTarget = _camera.transform.TransformDirection(lookTarget);

        return lookTarget;
    }
}
}
