using System;
using DefaultNamespace;
using Randomables;
using UnityEngine;
using UnityEngine.Serialization;

namespace Agent.SymmetryQuadPed
{
public class SymmetryQuadPedAgent : LegAgent
{
    protected override float GetReward()
    {
        float speedReward = legController.VelocityAccuracy;
        float efficiency = legController.Efficiency;
        float difficulty = speedReward * (legController.TargetVelocity / setting.maxSpeed);
        float effortReward = speedReward * (difficulty + efficiency * (1 - difficulty));
        return speedReward * effortReward;
    }
}
}
