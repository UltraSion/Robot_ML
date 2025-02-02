using UnityEngine;

namespace Controller.AimControllers
{
public class AimController : MonoBehaviour
{
    public AimDirectionGetter aimDirectionGetter;

    // public void Awake()
    // {
    //     aimDirectionGetter = GetComponent<AimDirectionGetter>();
    // }

    public void Update()
    {
        transform.rotation = aimDirectionGetter.GetAimDirection();

        Debug.DrawLine(transform.position, transform.position + transform.forward * 1000f);
    }
}
}
