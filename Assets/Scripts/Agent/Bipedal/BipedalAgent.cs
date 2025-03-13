using System.Collections.Generic;
using Controller;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Agent
{
public class BipedalAgent : Unity.MLAgents.Agent
{
    private BipedalController bipedalController;

    public Vector3 StartPos;

    private List<float> backUpPos = new List<float>();

    private void Start()
    {
        bipedalController = GetComponent<BipedalController>();
        StartPos = transform.position;
        bipedalController.pelvis.GetJointPositions(backUpPos);
        RandTarget();
    }

    private void ResetBody()
    {
        bipedalController.pelvis.TeleportRoot(StartPos, Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0));
        bipedalController.pelvis.linearVelocity = Vector3.zero;
        bipedalController.pelvis.angularVelocity = Vector3.zero;
        bipedalController.controllers.ForEach(m =>
        {
            m.articulationBody.linearVelocity = Vector3.zero;
            m.articulationBody.angularVelocity = Vector3.zero;
        });

        bipedalController.pelvis.SetJointPositions(backUpPos);
    }

    public void RandTarget()
    {
        float moveDir = Random.Range(0.0f, 360.0f);
        bipedalController.SetMoveDirection(new Vector3(Mathf.Cos(moveDir), 0f, Mathf.Sin(moveDir)));

        bipedalController.SetSpeed(Random.Range(0.0001f, 10f));
        // bipedalController.SetHeight(Random.Range(2f, 4.5f));
        // bipedalController.SetSpeed(4f);
        bipedalController.SetHeight(3f);
    }

    public override void OnEpisodeBegin()
    {
        ResetBody();
        RandTarget();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        bipedalController.CollectObservations(sensor);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var actions = actionBuffers.ContinuousActions;

        int abCount = bipedalController.controllers.Count;
        float[] forceRatios = new float[abCount];
        float[] targets = new float[abCount];

        int i = 0;
        float forceUsage = (actions[i++] + 1) * 0.5f;
        for (int index = 0; index < abCount; index++)
        {
            forceRatios[index] = (actions[i++] + 1) * 0.5f;
            targets[index] = (actions[i++] + 1) * 0.5f;
        }

        bipedalController.SetDrive(forceUsage, forceRatios, targets);
    }

    private void FixedUpdate()
    {
        float efficiency = bipedalController.Efficiency;

        float pelvisY = bipedalController.pelvis.transform.position.y;
        var heightDelta = Mathf.Abs(bipedalController.targetHeight - pelvisY);

        float speedReward = bipedalController.VelocityAccuracy;

        float heightReward = Mathf.Clamp01(1 - heightDelta / bipedalController.targetHeight);
        // float pelvisUprightReward = (bipedalController.PelvisUprightDot + 1f) * 0.5f;
        // float reward = pelvisUprightReward + heightReward * 2 + speedReward * 3f;
        float lookReward = bipedalController.FootLookDot;

        // AddReward(pelvisUprightReward);
        AddReward(1f);
        AddReward(speedReward);
    }
}
}
