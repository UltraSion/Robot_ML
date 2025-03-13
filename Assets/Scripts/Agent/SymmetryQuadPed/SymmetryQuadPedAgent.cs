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
        float reward = speedReward * effortReward;

        ui?.UpdateParameters(legController.TargetVelocity, speedReward, effortReward, reward);

        return reward;
    }
}
}
