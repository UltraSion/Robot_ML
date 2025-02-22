using System;
using UnityEngine;

namespace DefaultNamespace
{
public class Generator : PowerSource
{
    public Battery Battery;

    [SerializeField]
    private float generateAmount;

    public float GenerateAmount
    {
        get => generateAmount;
        set
        {
            if (value < 0)
                return;

            generateAmount = value;
        }
    }


    public override float UsableForce { get; }

    public override float ReleaseForce(float requestedForceAmount)
    {
        throw new NotImplementedException();
    }
}
}
