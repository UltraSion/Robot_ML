using System;
using System.Collections.Generic;
using System.Linq;
using Agent;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Controller
{
public class BipedalController : MonoBehaviour
{
    public List<DriveController> controllers = new List<DriveController>();
    public ArticulationBody pelvis;
    public ArticulationBody rFoot;
    public ArticulationBody lFoot;

    public float maximumForce;
    public float outputForce;
    public float Efficiency => Mathf.Clamp01(1 - outputForce / maximumForce);

    public Vector3 moveDir;
    public float LookDot => Vector3.Dot(moveDir, pelvis.transform.forward);

    public float targetVelocity;

    public float VelocityDeltaMagnitude
        => Mathf.Clamp(Vector3.Distance(GetAvgVel(), moveDir * targetVelocity), 0, targetVelocity);

    public float VelocityAccuracy
        => Mathf.Clamp01(1 - VelocityDeltaMagnitude / targetVelocity);

    public float PelvisUprightDot => Vector3.Dot(pelvis.transform.up, Vector3.up);

    public float FootLookDot
    {
        get
        {
            Vector3 leftFootForward = lFoot.transform.forward;
            leftFootForward.y = 0;
            leftFootForward = leftFootForward.normalized;
            Vector3 rightFootForward = rFoot.transform.forward;
            rightFootForward.y = 0;
            rightFootForward = rightFootForward.normalized;

            float leftDot = Vector3.Dot(moveDir, leftFootForward);
            float rightDot = Vector3.Dot(moveDir, rightFootForward);

            leftDot = (leftDot + 1) * 0.5f;
            rightDot = (rightDot + 1) * 0.5f;

            return (leftDot + rightDot) * 0.5f;
        }
    }

    public float targetHeight;

    public PositionSupporter positionSupporter;

    public void SetSpeed(float speed)
    {
        if (speed < 0)
            throw new Exception("The speed is Out of Range");

        targetVelocity = speed;
    }

    public void SetHeight(float height)
    {
        if (height < 0)
            throw new Exception("The height is Out of Range");

        targetHeight = height;
    }

    public void SetMoveDirection(Vector3 direction)
        => moveDir = direction;

    private float[] GetFloatInfo(int index)
    {
        DriveController toGet = controllers[index];
        return new[]
        {
            toGet.softMaxForce,
            // toGet.ForceUsage,
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
            maximumForce,
            outputForce / maximumForce,
            targetHeight,
            Vector3.Distance(GetAvgVel(), targetVelocity * moveDir),
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
            targetVelocity * moveDir
        };
    }

    public Vector3[][] GetVectorInfos()
    {
        Vector3[][] infos = new Vector3[controllers.Count + 1][];

        int index = 0;
        controllers.ForEach(_=> infos[index] = GetVectorInfo(index++));
        infos[index] = GetBodyVectorInfo();

        foreach (var info in infos)
        {
            positionSupporter.transform.InverseTransformDirections(info);
        }

        return infos;
    }

    public void CollectObservations(VectorSensor sensor)
    {
        positionSupporter.UpdateOrientation(pelvis.transform, moveDir);

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
    }

    private Vector3 GetAvgVel()
    {
        Vector3 velSum = Vector3.zero;
        int bodyCount = 0;
        foreach (var manager in controllers)
        {
            ArticulationBody body = manager.articulationBody;
            if (body.mass <= 0)
                continue;

            velSum += body.linearVelocity;
            bodyCount++;
        }

        return velSum / bodyCount;
    }

    public void SetDrive(float forceUsage, float[] forceRatios, float[] targets)
    {
        if (forceRatios.Length != controllers.Count)
            throw new Exception("forceRatios.length is Not Match Controllers.Count");

        if (targets.Length != controllers.Count)
            throw new Exception("targets.length is Not Match Controllers.Count");

        // List<float> driveForces = new();
        //
        // int i = 0;
        // controllers.ForEach(manager => driveForces.Add(manager.MaxForce * forceRatios[i++]));
        // float forceSum = driveForces.Sum();
        //
        // i = 0;
        // controllers.ForEach(manager => manager.SetTarget(targets[i++]));
        //
        // i = 0;
        // if (forceSum > maximumForce)
        // {
        //     float ratioSum = forceRatios.Sum();
        //     controllers.ForEach(manager => manager.SetForceLimit(forceRatios[i++] / ratioSum * maximumForce));
        //     outputForce = maximumForce;
        // }
        // else
        // {
        //     controllers.ForEach(manager => manager.SetForceLimit(driveForces[i++]));
        //     outputForce = forceSum;
        // }

        outputForce = maximumForce * forceUsage;

        int i = 0;
        controllers.ForEach(manager => manager.SetTarget(targets[i++]));
        float ratioSum = forceRatios.Sum();

        i = 0;
        controllers.ForEach(controller =>
        {
            controller.softMaxForce = forceRatios[i++] / ratioSum;
            controller.SetForceLimit(outputForce * controller.softMaxForce);
        });
    }
}
}
