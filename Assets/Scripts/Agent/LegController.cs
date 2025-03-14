using System;
using System.Collections.Generic;
using Controller;
using DefaultNamespace;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Serialization;

namespace Agent
{
public abstract class LegController : MonoBehaviour
{
    public ArticulationBody root;

    public List<DriveController> controllers;

    [SerializeField] protected Vector3 moveDir;
    [SerializeField] protected float targetVelocity;

    public PowerHub powerHub;

    [SerializeField] protected PositionSupporter supporter;

    public TargetObject targetObject;

    public float Efficiency { get; private set; }

    protected Vector3 MoveDir
    {
        get => moveDir;
        set => moveDir = value.normalized;
    }

    public float TargetVelocity
    {
        get => targetVelocity;
        set
        {
            if (value < 0)
                throw new Exception("The speed is Out of Range");

            targetVelocity = value;
        }
    }

    public virtual float LookDot
        => Vector3.Dot(root.transform.forward, moveDir);

    public float VelocityAccuracy
    {
        get
        {
            var myAvgVel = GetAvgVel();
            var targetVel = MoveDir * targetVelocity;

            var dirAccuracy = Vector3.Dot(myAvgVel.normalized, targetVel.normalized);
            dirAccuracy = (dirAccuracy + 1f) * 0.5f;

            float mySpeed = myAvgVel.magnitude;
            float targetSpeed = targetVelocity;

            var speedDelta = Mathf.Clamp(Mathf.Abs(mySpeed - targetSpeed), 0, targetSpeed);
            var speedAccuracy = Mathf.Clamp01(1 - speedDelta / targetSpeed);

            return dirAccuracy * speedAccuracy;
        }
    }

    protected abstract Vector3 GetAvgVel();
    public abstract void CollectObservations(VectorSensor sensor);

    public void SetDrive(float[] forceRatios, float[] targets)
    {
        if (forceRatios.Length != controllers.Count)
            throw new Exception("forceRatios.length is Not Match Controllers.Count");

        if (targets.Length != controllers.Count)
            throw new Exception("targets.length is Not Match Controllers.Count");

        float usableForce = powerHub.UsableForce;
        float requestedForcesSum = 0f;

        int i = 0;
        controllers.ForEach(controller => requestedForcesSum += controller.MaxForce * forceRatios[i++]);

        i = 0;
        controllers.ForEach(controller => controller.Target = targets[i++]);

        float forceToUse = requestedForcesSum < usableForce ? requestedForcesSum : usableForce;
        float usableRatio = requestedForcesSum == 0 ? 1 : forceToUse / requestedForcesSum;

        i = 0;
        controllers.ForEach(controller => controller.ForceUseRatio = forceRatios[i++] * usableRatio);

        powerHub.ReleaseForce(forceToUse);
        Efficiency = 1f - forceToUse / usableForce;
    }
}
}
