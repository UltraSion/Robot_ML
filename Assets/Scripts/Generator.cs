using System;
using UnityEngine;

namespace DefaultNamespace
{
public class Generator : MonoBehaviour, IPowerSource
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


    public float UsableForce { get; }

    public float ReleaseForce(float requestedForceAmount)
    {
        throw new NotImplementedException();
    }
}
}
