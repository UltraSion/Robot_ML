using System;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Walk.Scripts
{
public class BipedalAgent : Agent
{
    private BipedalController bipedalController;

    public float minForce = 100000f;
    public float maxForce = 200000f;

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

        bipedalController.SetSpeed(Random.Range(0.0001f, 20f));
        bipedalController.SetHeight(Random.Range(2.5f, 4.5f));
        bipedalController.maximumForce = Random.Range(minForce, maxForce);
    }

    public override void OnEpisodeBegin()
    {
        ResetBody();
        RandTarget();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        var floatObservations = bipedalController.GetFloatInfos();
        var vectorObservations = bipedalController.GetVectorInfos();
        var quaternionObservations = bipedalController.GetQuaternionInfo();

        foreach (var floats in floatObservations)
        {
            foreach (var f in floats)
            {
                sensor.AddObservation(f);
            }
        }

        foreach (var vectors in vectorObservations)
        {
            foreach (var v in vectors)
            {
                sensor.AddObservation(v);
            }
        }

        foreach (var q in quaternionObservations)
        {
            sensor.AddObservation(q);
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var actions = actionBuffers.ContinuousActions;

        int abCount = bipedalController.controllers.Count;
        float[] forceRatios = new float[abCount];
        float[] targets = new float[abCount];

        int i = 0;
        for (int index = 0; index < abCount; index++)
        {
            forceRatios[index] = (actions[i++] + 1) * 0.5f;
            targets[index] = (actions[i++] + 1) * 0.5f;
        }

        bipedalController.SetDrive(forceRatios, targets);
    }

    private void FixedUpdate()
    {
        if (bipedalController.pelvis.transform.position.y  is <= 0.5f or >= 6.0f)
        {
            EndEpisode();
        }
        float efficiency = bipedalController.Efficiency;

        float pelvisY = bipedalController.pelvis.transform.position.y;
        var heightDelta = Mathf.Abs(bipedalController.targetHeight - pelvisY);

        float speedReward = bipedalController.VelocityAccuracy;

        float heightReward = Mathf.Clamp01(1 - heightDelta / bipedalController.targetHeight);
        // float lookReward = Mathf.Abs(bipedalController.LookDot);

        float reward = (heightReward + speedReward * 2f) * efficiency;

        AddReward(reward);
    }
}
}
