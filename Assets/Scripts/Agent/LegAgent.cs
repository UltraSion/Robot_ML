using System;
using System.Collections.Generic;
using Randomables;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Agent
{
public abstract class LegAgent : Unity.MLAgents.Agent
{
    public Arena arena;
    private LegController legController;

    private Vector3 startPos;
    private List<float> backUpPos = new List<float>();
    private List<float> backUpVel = new List<float>();
    private List<ArticulationDrive> startState = new();

    public AgentSetting setting;

    private float averageSpeedReward = 0;
    public float Avg_speedReward => averageSpeedReward;
    public float CurStep { private set; get; } = 0;

    protected override void Awake()
    {
        base.Awake();
        legController = GetComponent<LegController>();
    }

    private void Start()
    {
        setting = AgentSetting.instance;

        startPos = transform.position;
        legController.root.GetJointPositions(backUpPos);
        legController.root.GetJointVelocities(backUpVel);

        legController.controllers.ForEach(controller => startState.Add(controller.articulationBody.xDrive));

       arena.Rand();
    }

    private void ResetBody()
    {
        legController.root.TeleportRoot(startPos, Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0));
        legController.root.SetJointPositions(backUpPos);
        legController.root.SetJointVelocities(backUpVel);

        legController.root.linearVelocity = Vector3.zero;
        legController.root.angularVelocity = Vector3.zero;
        foreach (var controller in legController.controllers)
        {
            controller.articulationBody.linearVelocity = Vector3.zero;
            controller.articulationBody.angularVelocity = Vector3.zero;
        }

        int i = 0;
        legController.controllers.ForEach(controller => controller.articulationBody.xDrive = startState[i++]);
    }

    public override void OnEpisodeBegin()
    {
        OptimizeMaxSpeed();
        ResetBody();

        legController.TargetVelocity = Mathf.Pow(Random.Range(0, 1f), 0.5f) * (setting.maxSpeed - setting.minSpeed) + setting.minSpeed;
        arena.TargetObject.Rand();

        averageSpeedReward = 0f;
        CurStep = 0f;
    }

    private void OptimizeMaxSpeed()
    {
        if (legController.TargetVelocity < setting.maxSpeed * 0.9)
            return;

        if (CurStep < MaxStep * 0.9f)
            return;

        if (averageSpeedReward < 0.5f)
            return;


        setting.maxSpeed *= 1.1f;
    }

    public override void CollectObservations(VectorSensor sensor)
        => legController.CollectObservations(sensor);

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var actions = actionBuffers.ContinuousActions;

        int abCount = legController.controllers.Count;
        float[] forceRatios = new float[abCount];
        float[] targets = new float[abCount];

        int i = 0;
        for (int index = 0; index < abCount; index++)
        {
            forceRatios[index] = (actions[i++] + 1) * 0.5f;
            targets[index] = (actions[i++] + 1) * 0.5f;
        }

        legController.SetDrive(forceRatios, targets);
    }

    private bool IsTargetTouched()
    {
        var positionDelta = (transform.position - arena.TargetObject.transform.position);
        positionDelta.y = 0f;
        float distance = positionDelta.magnitude;
        return distance < 0.5f;
    }

    protected abstract float GetReward();

    private void CalculateAverageSpeedReward(float speedReward)
    {
        float rewardSum = averageSpeedReward * CurStep;
        rewardSum += speedReward;
        averageSpeedReward = rewardSum / ++CurStep;
    }

    private void FixedUpdate()
    {
        if (IsTargetTouched())
        {
            arena.TargetObject.Rand();
            AddReward(1f);
        }

        CalculateAverageSpeedReward(legController.VelocityAccuracy);
        AddReward(GetReward());
    }
}
}
