using System;
using System.Collections.Generic;
using System.Linq;
using Agent;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Serialization;

namespace Controller
{
public class BipedalController02 : MonoBehaviour
{
    public List<DriveController02> controllers = new();
    public ArticulationBody pelvis;
    public ArticulationBody rFoot;
    public ArticulationBody lFoot;

    public PowerHub powerHub;
    public PositionSupporter positionSupporter;
    [SerializeField] private Vector3 moveDir;
    [SerializeField] private float targetVelocity;
    [SerializeField] private float targetHeight;

    public float[][] LastFloatObservations;
    public Vector3[][] LastVectorObservations;

    public Vector3 MoveDir
    {
        get => moveDir;
        set => moveDir = value;
    }

    public float TargetVelocity
    {
        set
        {
            if (value < 0)
                throw new Exception("The speed is Out of Range");

            targetVelocity = value;
        }
    }

    public float TargetHeight
    {
        get => targetHeight;
        set
        {
            if (value < 0)
                throw new Exception("The height is Out of Range");

            targetHeight = value;
        }
    }

    public float LookDot
        => Vector3.Dot(MoveDir, pelvis.transform.forward);

    public float VelocityDeltaMagnitude
    {
        get
        {
            var velDelta = Vector3.Distance(GetAvgVel(), MoveDir * targetVelocity);
            return Mathf.Clamp(velDelta, 0, targetVelocity);
        }
    }

    public float VelocityAccuracy
        => Mathf.Clamp01(1 - VelocityDeltaMagnitude / targetVelocity);

    public float PelvisUprightDot
        => Vector3.Dot(pelvis.transform.up, Vector3.up);

    public float FootLookDot
    {
        get
        {
            Vector3 leftFootForward = lFoot.transform.forward;
            leftFootForward.y = 0;
            leftFootForward.Normalize();
            Vector3 rightFootForward = rFoot.transform.forward;
            rightFootForward.y = 0;
            rightFootForward.Normalize();

            float leftDot = Vector3.Dot(MoveDir, leftFootForward);
            float rightDot = Vector3.Dot(MoveDir, rightFootForward);

            leftDot = (leftDot + 1) * 0.5f;
            rightDot = (rightDot + 1) * 0.5f;

            return (leftDot + rightDot) * 0.5f;
        }
    }

    private float[] GetFloatInfo(int index)
    {
        DriveController02 toGet = controllers[index];
        return new[]
        {
            toGet.MaxForce,
            toGet.ForceUseRatio,
            toGet.Target - toGet.JointPos,
            toGet.Target,
            toGet.JointPos,
            toGet.Velocity,
            toGet.Acceleration,
        };
    }

    private float[] GetBodyFloatsInfo()
    {
        return new[]
        {
            Vector3.Distance(GetAvgVel(), targetVelocity * MoveDir),
            LookDot,
            PelvisUprightDot
        };
    }

    private float[][] GetFloatInfos()
    {
        float[][] infos = new float[controllers.Count + 1][];

        int index = 0;
        controllers.ForEach(_ => infos[index] = GetFloatInfo(index++));
        infos[index] = GetBodyFloatsInfo();

        return infos;
    }

    private Vector3[] GetVectorInfo(int index)
    {
        ArticulationBody toGet = controllers[index].articulationBody;
        return new[]
        {
            toGet.transform.position - pelvis.transform.position,
            toGet.angularVelocity,
            toGet.linearVelocity,
        };
    }

    private Vector3[] GetBodyVectorInfo()
    {
        return new[]
        {
            pelvis.angularVelocity,
            pelvis.linearVelocity,
            GetAvgVel(),
            targetVelocity * MoveDir
        };
    }

    public Vector3[][] GetVectorInfos()
    {
        Vector3[][] infos = new Vector3[controllers.Count + 1][];

        int index = 0;
        controllers.ForEach(_ => infos[index] = GetVectorInfo(index++));
        infos[index] = GetBodyVectorInfo();

        foreach (var info in infos)
        {
            positionSupporter.transform.InverseTransformDirections(info);
        }

        return infos;
    }

    public void CollectObservations(VectorSensor sensor)
    {
        positionSupporter.UpdateOrientation(pelvis.transform, MoveDir);

        var floatObservations = GetFloatInfos();
        var vectorObservations = GetVectorInfos();

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

        LastFloatObservations = floatObservations;
        LastVectorObservations = vectorObservations;
    }

    private Vector3 GetAvgVel()
    {
        Vector3 velSum = Vector3.zero;
        int bodyCount = 0;
        foreach (var controller in controllers)
        {
            ArticulationBody body = controller.articulationBody;
            if (body.mass <= 0)
                continue;

            velSum += body.linearVelocity;
            bodyCount++;
        }

        return velSum / bodyCount;
    }

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
        float usableRatio = forceToUse / requestedForcesSum;

        i = 0;
        controllers.ForEach(controller => controller.ForceUseRatio = forceRatios[i++] * usableRatio);

        powerHub.ReleaseForce(forceToUse);
    }
}
}
