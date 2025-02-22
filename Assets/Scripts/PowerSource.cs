using UnityEngine;

namespace DefaultNamespace
{
public abstract class PowerSource : MonoBehaviour, IPowerSource
{
    public abstract float UsableForce { get; }
    public abstract float ReleaseForce(float requestedForceAmount);
}
}
