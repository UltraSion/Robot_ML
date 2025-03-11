using System.Collections.Generic;
using Controller;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Agent
{
public class BipedalAgent02 : Unity.MLAgents.Agent
{
    private BipedalController02 bipedalController;

    private Vector3 startPos;

    private List<float> backUpPos = new List<float>();
    private List<float> backUpVel = new List<float>();
    private List<float> backUpAccel = new List<float>();

    private List<ArticulationDrive> startState = new();

    public GameObject targetObject;

    public BipedalAgentSetting setting;
    public BipedalAgentUI agentUI;

    public float forceTimer = 0;
    public float targetRandTimer = 0;

    [SerializeField] [Range(0, 1)] private float averageSpeedReward = 0;
    public float Avg_speedReward => averageSpeedReward;
    public float CurStep { private set; get; } = 0;

    protected override void Awake()
    {
        base.Awake();
        bipedalController = GetComponent<BipedalController02>();
    }

    private void Start()
    {
        setting = BipedalAgentSetting.instance;

        startPos = transform.position;
        bipedalController.pelvis.GetJointPositions(backUpPos);
        bipedalController.pelvis.GetJointVelocities(backUpVel);
        bipedalController.pelvis.GetJointAccelerations(backUpAccel);

        bipedalController.controllers.ForEach(controller => startState.Add(controller.articulationBody.xDrive));

        RandTarget();
    }

    private void ResetBody()
    {
        bipedalController.pelvis.TeleportRoot(startPos, Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0));

        bipedalController.pelvis.linearVelocity = Vector3.zero;
        bipedalController.pelvis.angularVelocity = Vector3.zero;
        bipedalController.controllers.ForEach(controller =>
        {
            controller.articulationBody.linearVelocity = Vector3.zero;
            controller.articulationBody.angularVelocity = Vector3.zero;
        });

        bipedalController.pelvis.SetJointPositions(backUpPos);
        bipedalController.pelvis.SetJointVelocities(backUpVel);

        for (int i = 0; i < startState.Count; i++)
        {
            bipedalController.controllers[i].articulationBody.xDrive = startState[i];
        }
    }

    private void RandTarget()
    {
        bipedalController.TargetVelocity = Random.Range(setting.minSpeed, setting.maxSpeed);
        RandTargetObject();
    }

    private void RandTargetObject()
    {
        float targetDir = Random.Range(0.0f, 360.0f);
        float targetDistance = Random.Range(3f, 40f);

        Vector3 targetPos = new Vector3(Mathf.Cos(targetDir) * targetDistance, 0.5f,
            Mathf.Sin(targetDir) * targetDistance);
        targetObject.transform.localPosition = targetPos;
    }

    public override void OnEpisodeBegin()
    {
        OptimizeMaxSpeed();


        ResetBody();
        bipedalController.TargetVelocity = Random.Range(setting.minSpeed, setting.maxSpeed);

        RandTarget();

        averageSpeedReward = 0f;
        CurStep = 0f;

        forceTimer = 0f;
        targetRandTimer = 0f;
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

    private bool IsTargetTouched()
    {
        var positionDelta = (transform.position - targetObject.transform.position);
        positionDelta.y = 0f;
        float distance = positionDelta.magnitude;
        return distance < 0.5f;
    }

    private void FixedUpdate()
    {
        // if (IsJumping())
        // {
        //     Checkers.ForEach(checker => checker.Reset());
        //     EndEpisode();
        //     return;
        // }

        // float pelvisY = bipedalController.pelvis.transform.position.y;
        if (IsTargetTouched())
        {
            RandTargetObject();
            AddReward(1f);
            targetRandTimer = 0f;
        }

        if (targetRandTimer > setting.targetRandTimer)
        {
            RandTargetObject();
            targetRandTimer = 0f;
        }
        else
        {
            targetRandTimer += Time.fixedDeltaTime;
        }

        if (forceTimer > setting.forceInterval)
        {
            var n1 = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            var n2 = Random.Range(0f, 360f) * Mathf.Deg2Rad;

            var x = Mathf.Sin(n1) * Mathf.Cos(n2);
            var y = Mathf.Sin(n1) * Mathf.Sin(n2);
            var z = Mathf.Cos(n1);
            var force = setting.forcePower * Random.Range(0.5f, 1f) * new Vector3(x, y, z);
            GetRandomParts().AddForce(force, ForceMode.Impulse);
            forceTimer = 0f;
        }
        else
        {
            forceTimer += Time.fixedDeltaTime;
        }

        float speedReward = bipedalController.VelocityAccuracy2;
        float pelvisUprightReward = (bipedalController.PelvisUprightDot + 1f) * 0.5f;
        float pelvisLookReward = Mathf.Abs(bipedalController.LookDot);
        float footReward = bipedalController.FootLookDot;

        float efficiency = bipedalController.Efficiency;
        float difficulty = speedReward * (bipedalController.TargetVelocity / setting.maxSpeed);
        float effortReward = speedReward * (difficulty + efficiency * (1 - difficulty));

        agentUI?.UpdateParameters(
            bipedalController.TargetVelocity,
            speedReward,
            effortReward,
            pelvisLookReward,
            footReward
            );

        float reward = speedReward * effortReward * pelvisLookReward * footReward;

        CalculateAverageSpeedReward(speedReward);
        AddReward(reward);

        //reward
        //= sr * er
        //= sr * (d + e(1 - d))
        //= sr * (v(1 - sr) + e(1 - v(1 - sr))
        //= sr * (v - v*sr + e - ev + sr*v*e)
        //= sr*v - v*sr^2 + e * sr - ev * sr + sr^2*v*e
        //= sr^2v(e - 1) + sr(v + e - ev)
        //= sr^2v(e - 1) + sr(v(1- e) + e)

        //v(sr - sr^2 - e*sr + sr^2*e) + e*sr
        //=v((e - 1)sr^2 - (e - 1)sr) + e*sr
        //=v((e - 1)(sr^2 - sr) + e*sr
        //=v*sr(e - 1)(sr - 1) + e * sr
        //=sr(v(e - 1)(sr - 1) + e)
        //=sr(e + v(1 - e)(1 - sr))

        //=sr * er
        //=sr(d + e(1 - d))
        //=sr(d + e - ed)
        //=sr((1-sr) + e - e(1-sr))
        //=sr(1 - sr + e - e + esr)
        //=sr(1 - sr + esr)
        //=sr(1 - sr(1 - e))
        //=sr - sr^2 + sre
        //=-sr^2 +sr(1 + e)

        //sr = (1 + e) / 2 일때 최대
        //sr = 0.5f + 0.5e

        // sr * v + e * (1 - sr * v);
        // d = sr * target/max;
        //d + e * (1 - d)

        //reward
        //= sr(d + e(1 - d))
        //= sr(srv + e(1 - srv))
        //= vsr^2 + esr - sr^2ev
        //= sr^2(v - ev) + esr
        //= vSr^2(1 - e) + esr
        //= Sr(2v(1 - e)Sr + e)

        //2V(1 - e)Sr + e
    }

    private ArticulationBody GetRandomParts()
    {
        int randNum = Random.Range(0, bipedalController.controllers.Count);

        return bipedalController.controllers[randNum].articulationBody;
    }

    private void CalculateAverageSpeedReward(float speedReward)
    {
        float rewardSum = averageSpeedReward * CurStep;
        rewardSum += speedReward;
        averageSpeedReward = rewardSum / ++CurStep;
    }

    private void OptimizeMaxSpeed()
    {
        if (bipedalController.TargetVelocity < setting.maxSpeed * 0.9)
            return;

        if (CurStep < MaxStep * 0.9f)
            return;

        if (averageSpeedReward < 0.5f)
            return;


        setting.maxSpeed *= 1.1f;
    }
}
}
