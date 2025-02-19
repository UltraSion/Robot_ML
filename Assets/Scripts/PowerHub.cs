using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
public class PowerHub : MonoBehaviour, IPowerSource
{
    public List<IPowerSource> Batteries = new();
    public List<IPowerSource> Generators = new();

    private static float GetUsableForce(List<IPowerSource> sources)
    {
        float force = 0f;
        sources.ForEach(source => force += source.UsableForce);
        return force;
    }

    public float UsableForce
    {
        get
        {
            float usableForce = 0f;
            usableForce += GetUsableForce(Batteries);
            usableForce += GetUsableForce(Generators);
            return usableForce;
        }
    }

    public void AddBattery(Battery battery)
        => Batteries.Add(battery);

    public void AddGenerator(Generator generator)
        => Generators.Add(generator);

    public float ReleaseForce(float requestedForceAmount)
    {
        float lastForce = requestedForceAmount;

        foreach (var generator in Generators)
        {
            lastForce -= generator.ReleaseForce(lastForce);
            if (lastForce <= 0)
                return 0;
        }

        foreach (var battery in Batteries)
        {
            lastForce -= battery.ReleaseForce(lastForce);
            if (lastForce <= 0)
                return 0;
        }

        return lastForce;
    }
}
}
