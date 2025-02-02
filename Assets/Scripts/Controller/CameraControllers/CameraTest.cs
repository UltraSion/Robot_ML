using UnityEngine;

namespace Controller.CameraControllers
{
public class CameraTest : MonoBehaviour
{
    private CameraController Controller;

    private void Awake()
    {
        Controller = GetComponent<CameraController>();
    }

    private void Update()
    {
        float delta = Input.mouseScrollDelta.y * .1f;
        Controller.Sensitivity += delta;
    }
}
}
