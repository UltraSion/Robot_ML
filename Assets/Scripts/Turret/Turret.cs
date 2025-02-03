using Controller.AimControllers;
using UnityEngine;

namespace Turret
{
public class Turret : MonoBehaviour
{
    public Camera cam;
    public AimDirectionGetter AimDirectionGetter;

    private void Start()
    {
        // AimDirectionGetter = GetComponent<AimDirectionGetter>();
        AimDirectionGetter.SetCamera(cam);
    }

    public void TargetVector(Vector3 dir)
    {

    }

    private void Update()
    {
        UpdateCamDir();
    }

    private void UpdateCamDir()
    {
        cam.transform.rotation = Quaternion.RotateTowards(cam.transform.rotation, AimDirectionGetter.GetAimRotation(),
            30f * Time.deltaTime);
    }
}
}
