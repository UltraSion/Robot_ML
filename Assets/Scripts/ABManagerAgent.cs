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
public class ABManagerAgent : Agent
{
    private ABController abController;

    public Vector3 moveDirection;
    public float targetHeight;
    public float targetSpeed;

    public Vector3 StartPos;

    private List<float> backUpPos = new List<float>();

    private void Start()
    {
        abController = GetComponent<ABController>();
        StartPos = transform.position;

        float moveDir = Random.Range(0.0f, 360.0f);
        moveDirection = new Vector3(Mathf.Cos(moveDir), 0f, Mathf.Sin(moveDir));

        targetSpeed = Random.Range(0.0001f, 60f);
        targetHeight = Random.Range(2f, 4.5f);

        abController.pelvis.GetJointPositions(backUpPos);
    }

    private void ResetBody()
    {
        abController.pelvis.TeleportRoot(StartPos, Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0));
        abController.pelvis.linearVelocity = Vector3.zero;
        abController.pelvis.angularVelocity = Vector3.zero;
        abController.managers.ForEach(m =>
        {
            m.articulationBody.linearVelocity = Vector3.zero;
            m.articulationBody.angularVelocity = Vector3.zero;
        });

        abController.pelvis.SetJointPositions(backUpPos);
    }

    public override void OnEpisodeBegin()
    {
        ResetBody();

        float moveDir = Random.Range(0.0f, 360.0f);
        moveDirection = new Vector3(Mathf.Cos(moveDir), 0f, Mathf.Sin(moveDir));

        targetSpeed = Random.Range(0.0001f, 60f);
        targetHeight = Random.Range(2f, 4.5f);

        abController.maximumForce = Random.Range(100000f, 500000f);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(abController.maximumForce);
        foreach (var f in abController.GetInfos())
        {
            sensor.AddObservation(f);
        }

        var pelvis = abController.pelvis;
        var pelvisForward = abController.pelvis.transform.forward;
        var avgVel = abController.GetAvgVel();
        var velGoal = moveDirection * targetSpeed;
        sensor.AddObservation(Vector3.Distance(velGoal, avgVel));
        sensor.AddObservation(avgVel);
        sensor.AddObservation(velGoal);

        sensor.AddObservation(Quaternion.FromToRotation(pelvisForward, moveDirection));
        sensor.AddObservation(pelvisForward);
        sensor.AddObservation(moveDirection);

        sensor.AddObservation(pelvis.transform.position.y - targetHeight);
        sensor.AddObservation(pelvis.transform.position.y);
        sensor.AddObservation(targetHeight);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var actions = actionBuffers.ContinuousActions;

        int i = 0;
        float outputRatio = (actions[i++] + 1) * 0.5f;

        int abCount = abController.managers.Count;
        float[] forceRatios = new float[abCount];
        float[] targets = new float[abCount];
        for (int index = 0; index < abCount; index++)
        {
            forceRatios[index] = (actions[i++] + 1) * 0.5f;
            targets[index] = (actions[i++] + 1) * 0.5f;
        }

        abController.SetDrive(outputRatio, forceRatios, targets);
    }

    private void FixedUpdate()
    {
        try
        {
            float leftForceRatio =
                Mathf.Clamp((abController.maximumForce - abController.outputForce) / abController.maximumForce, 0, 1);
            float lookDot = Vector3.Dot(moveDirection, abController.pelvis.transform.forward);

            var actualVel = abController.GetAvgVel();
            var velGoal = moveDirection * targetSpeed;

            var velDeltaMagnitude = Mathf.Clamp(Vector3.Distance(actualVel, velGoal), 0, targetSpeed);

            float pelvisY = abController.pelvis.transform.position.y;
            var heightDelta = Mathf.Abs(targetHeight - pelvisY);

            float speedReward = Mathf.Clamp(1 - velDeltaMagnitude / targetSpeed, 0, 1);

            float heightReward = Mathf.Clamp(1 - heightDelta / targetHeight, 0, 1);
            float lookReward = Mathf.Abs(lookDot);


            // Debug.Log("leftForceRatio: " + leftForceRatio +
            //           ", speedReward: " + speedReward +
            //           ", heightReward: " + heightReward +
            //           ", lookReward: " + lookReward);

            AddReward(leftForceRatio);
            AddReward(speedReward);
            AddReward(heightReward);
            AddReward(lookReward);
        }
        catch
        {
            EndEpisode();
            Debug.Log("Tlqkf");
        }
    }

    public void Update()
    {
        var pelvis = abController.pelvis.transform;
        Debug.DrawLine(pelvis.position, pelvis.position + pelvis.forward);
        Debug.DrawLine(pelvis.position, pelvis.position + moveDirection);
        Debug.DrawLine(pelvis.position, pelvis.position + abController.GetAvgVel());
        Debug.DrawLine(pelvis.position, pelvis.position + targetSpeed * moveDirection);
    }
}
}
