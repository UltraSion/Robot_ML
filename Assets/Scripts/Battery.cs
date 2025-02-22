using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace DefaultNamespace
{
public class Battery : PowerSource
{
    public bool isInfinite;

    [SerializeField]
    private float _maxCapacity;

    public float MaxCapacity
    {
        get => _maxCapacity;
        set
        {
            if (value < 0)
                throw new Exception("Battery's maxCapacity can't be negative");

            _maxCapacity = value;
        }
    }

    [SerializeField]
    private float _capacity;

    public float Capacity
    {
        get => isInfinite ? _maxCapacity : _capacity;
        private set
        {
            if (isInfinite)
                return;

            if (value < 0)
                throw new Exception("Battery's capacity can't be negative");

            _capacity = Mathf.Clamp(value, 0f, _maxCapacity);
        }
    }

    [SerializeField]
    private float outputForce;

    public override float UsableForce
    {
        get
        {
            if (isInfinite)
                return outputForce;

            if (_capacity >= outputForce)
                return outputForce;

            return _capacity;
        }
    }

    public override float ReleaseForce(float requestedForceAmount)
    {
        if (isInfinite)
            return outputForce;

        float usableForce = UsableForce;
        float forceOutput = usableForce <= requestedForceAmount ? usableForce : requestedForceAmount;

        Capacity = _capacity > forceOutput ? _capacity - forceOutput : 0f;

        return forceOutput;
    }

    public float Charge(float forceAmount)
    {
        if (forceAmount < 0)
            throw new Exception("I think you don't know what charge meaning");

        if (isInfinite)
            return 0f;

        float need = _maxCapacity - _capacity;
        float chargeableForceAmount = need < forceAmount ? need : forceAmount;
        Capacity += chargeableForceAmount;

        return chargeableForceAmount;
    }
}
}
