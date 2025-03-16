using System;
using DefaultNamespace;
using Randomables;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Agent.SymmetryQuadPed
{
public class SymmetryQuadPedAgent : LegAgent
{
    public SymmetryQuadPedUI ui;
    public SymmetryQuadPedController LegController => legController as SymmetryQuadPedController;

    public float SymmetryReward
    {
        get
        {
            var controller = LegController;

            var leg1Forward = controller.thighY1.transform.forward;
            var leg2Forward = controller.thighY2.transform.forward;
            var leg3Forward = controller.thighY3.transform.forward;
            var leg4Forward = controller.thighY4.transform.forward;

            leg1Forward.y = 0;
            leg2Forward.y = 0;
            leg3Forward.y = 0;
            leg4Forward.y = 0;

            leg1Forward.Normalize();
            leg2Forward.Normalize();
            leg3Forward.Normalize();
            leg4Forward.Normalize();

            float r1 = (Vector3.Dot(leg1Forward, -leg3Forward) + 1f) * 0.5f;
            float r2 = (Vector3.Dot(leg2Forward, -leg4Forward) + 1f) * 0.5f;

            return r1 * r2;
        }
    }

    protected override void RandTargetVelocity()
    {
        legController.TargetVelocity = Random.Range(setting.minSpeed, setting.maxSpeed);
    }

    protected override float GetReward()
    {
        float speedReward = legController.VelocityAccuracy;
        float efficiency = legController.Efficiency;
        float difficulty = speedReward * (legController.TargetVelocity / setting.maxSpeed);
        float effortReward = speedReward * (difficulty + efficiency * (1 - difficulty));
        float lookReward = legController.LookDot;
        float symmetryReward = SymmetryReward;
        float reward = speedReward * effortReward * lookReward * symmetryReward;

        ui?.UpdateParameters(legController.TargetVelocity, speedReward, effortReward, reward);

        return reward;
    }
}
}
