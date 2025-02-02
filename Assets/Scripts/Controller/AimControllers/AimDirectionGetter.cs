using UnityEngine;

namespace Controller.AimControllers
{
public abstract class AimDirectionGetter : MonoBehaviour
{
    public abstract Quaternion GetAimDirection();
}
}
