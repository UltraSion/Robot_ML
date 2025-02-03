using UnityEngine;

namespace Controller.AimControllers
{
public class AimController : MonoBehaviour
{
    public Camera cam;
    public AimDirectionGetter aimDirectionGetter;

    public void Start()
    {
        // aimDirectionGetter = GetComponent<AimDirectionGetter>();
        aimDirectionGetter.SetCamera(cam);
    }

    public void Update()
    {
        transform.rotation = aimDirectionGetter.GetAimRotation();

        Debug.DrawLine(transform.position, transform.position + transform.forward * 1000f);
    }
}
}
