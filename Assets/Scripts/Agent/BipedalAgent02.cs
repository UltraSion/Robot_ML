using System.Collections.Generic;
using Controller;
using NUnit.Framework;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Agent
{
public class BipedalAgent02 : Unity.MLAgents.Agent
{
    private BipedalController02 bipedalController;
    public ArticulationBody body;

    public Vector3 startPos;

    private List<float> backUpPos = new List<float>();
    private List<float> backUpVel = new List<float>();
    private List<float> backUpAccel = new List<float>();

    private List<ArticulationDrive> startState = new();

    public List<WalkChecker> Checkers;

    protected override void Awake()
    {
        base.Awake();
        bipedalController = GetComponent<BipedalController02>();

    }

    private void Start()
    {
        startPos = transform.position;
        bipedalController.pelvis.GetJointPositions(backUpPos);
        bipedalController.pelvis.GetJointVelocities(backUpVel);
        bipedalController.pelvis.GetJointAccelerations(backUpAccel);

        bipedalController.controllers.ForEach(controller => startState.Add(controller.articulationBody.xDrive));

        RandTarget();
    }

    private void ResetBody()
    {
        // bipedalController.pelvis.TeleportRoot(startPos, Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0));
        bipedalController.pelvis.TeleportRoot(startPos, Quaternion.identity);

        bipedalController.pelvis.linearVelocity = Vector3.zero;
        bipedalController.pelvis.angularVelocity = Vector3.zero;
        bipedalController.controllers.ForEach(controller =>
        {
            controller.articulationBody.linearVelocity = Vector3.zero;
            controller.articulationBody.angularVelocity = Vector3.zero;
        });
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;

        bipedalController.pelvis.SetJointPositions(backUpPos);
        bipedalController.pelvis.SetJointVelocities(backUpVel);

        for (int i = 0; i < startState.Count; i++)
        {
            bipedalController.controllers[i].articulationBody.xDrive = startState[i];
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
            ResetBody();
    }

    public void RandTarget()
    {
        float moveDir = Random.Range(0.0f, 360.0f);
        bipedalController.MoveDir = new Vector3(Mathf.Cos(moveDir), 0f, Mathf.Sin(moveDir));

        bipedalController.TargetVelocity = Random.Range(0.0001f, 10f);
        // bipedalController.SetHeight(Random.Range(2f, 4.5f));
        // bipedalController.SetSpeed(4f);
        bipedalController.TargetHeight = 3f;
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
        for (int index = 0; index < abCount; index++)
        {
            forceRatios[index] = (actions[i++] + 1) * 0.5f;
            targets[index] = (actions[i++] + 1) * 0.5f;
        }

        bipedalController.SetDrive(forceRatios, targets);
    }

    private bool IsJumping()
    {
        bool isJumping = true;

        foreach (var checker in Checkers)
        {
            if (checker.IsGround)
                isJumping = false;
        }

        return isJumping;
    }

    private void FixedUpdate()
    {
        if (IsJumping())
        {
            Checkers.ForEach(checker => checker.Reset());
            EndEpisode();
            return;
        }

        float pelvisY = bipedalController.pelvis.transform.position.y;

        float speedReward = bipedalController.VelocityAccuracy;

        float lookReward = bipedalController.FootLookDot;

        AddReward(1f);
        AddReward(speedReward);
    }
}
}
