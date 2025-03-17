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

            var legs = new SymmetryQuadPedController.Leg[4];

            for (int l = 0; l < controller._legs.Count; l++)
            {
                var legNum = controller.FirstLeg + l;
                legNum = legNum < controller._legs.Count ? legNum : legNum - controller._legs.Count;
                legs[l] = controller._legs[legNum];
            }

            var r1 = legs[0].thighY.Target + legs[3].thighY.Target;
            var r2 = legs[1].thighY.Target + legs[2].thighY.Target;

            r1 = 1 - Mathf.Abs(r1) / 180;
            r2 = 1 - Mathf.Abs(r2) / 180;

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
