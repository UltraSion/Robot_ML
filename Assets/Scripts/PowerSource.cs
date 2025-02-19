using System;
using UnityEngine;

namespace DefaultNamespace
{
public interface IPowerSource
{
    public float UsableForce { get; }

    public float ReleaseForce(float requestedForceAmount);
}
}
