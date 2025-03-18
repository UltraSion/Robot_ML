using System;
using DefaultNamespace;
using Randomables;
using Unity.MLAgents.Actuators;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Agent.SymmetryQuadPed
{
public class ForcedSymmetryQuadPedAgent : LegAgent
{
    public SymmetryQuadPedUI ui;
    public ForcedSymmetryQuadPedController LegController => legController as ForcedSymmetryQuadPedController;

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
        float reward = speedReward * effortReward * lookReward;

        ui?.UpdateParameters(legController.TargetVelocity, speedReward, effortReward, reward);
        return reward;
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var actions = actionBuffers.ContinuousActions;

        int abCount = legController.controllers.Count;
        float[] forceRatios = new float[abCount];
        float[] targets = new float[6];

        int i = 0;
        for (int index = 0; index < abCount; index++)
        {
            forceRatios[index] = (actions[i++] + 1) * 0.5f;
        }

        for (int index = 0; index < 6; index++)
        {
            targets[index] = (actions[i++] + 1) * 0.5f;
        }


        legController.SetDrive(forceRatios, targets);
    }

    private float[] t = new[] { 0f, 0f, 0f, 0f, 0f, 0f };

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        int abCount = legController.controllers.Count;

        int i = 0;
        for (int index = 0; index < abCount; index++)
        {
            continuousActionsOut[i++] = 1f;
        }

        var value = 0f;

        if (Input.GetKey(KeyCode.UpArrow))
            value = 0.1f;
        if (Input.GetKey(KeyCode.DownArrow))
            value = -0.1f;

        if (Input.GetKey(KeyCode.Q))
            t[0] += value;
        if (Input.GetKey(KeyCode.W))
            t[1] += value;
        if (Input.GetKey(KeyCode.E))
            t[2] += value;
        if (Input.GetKey(KeyCode.R))
            t[3] += value;
        if (Input.GetKey(KeyCode.T))
            t[4] += value;
        if (Input.GetKey(KeyCode.Y))
            t[5] += value;

        for (int index = 0; index < t.Length; index++)
        {
            t[index] = Mathf.Clamp(t[index], -1, 1);
            continuousActionsOut[i++] = t[index];
        }
    }
}
}
