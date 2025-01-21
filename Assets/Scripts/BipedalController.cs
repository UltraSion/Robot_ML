using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace Walk.Scripts
{
public class BipedalController : MonoBehaviour
{
    public List<DriveController> controllers = new List<DriveController>();
    public ArticulationBody pelvis;

    public float maximumForce;
    public float outputForce;
    public float Efficiency => Mathf.Clamp01((maximumForce - outputForce) / maximumForce);

    public Vector3 moveDir;
    public float LookDot => Vector3.Dot(moveDir, pelvis.transform.forward);

    public float targetVelocity;

    public float VelocityDeltaMagnitude
        => Mathf.Clamp(Vector3.Distance(GetAvgVel(), moveDir * targetVelocity), 0, targetVelocity);

    public float VelocityAccuracy
        => Mathf.Clamp01(1 - VelocityDeltaMagnitude / targetVelocity);

    public float PelvisUprightDot => Vector3.Dot(pelvis.transform.up, Vector3.up);

    public float targetHeight;


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
        float[] info = new float[]
        {
            toGet.Force,
            toGet.JointPos,
            toGet.Velocity,
            toGet.Acceleration,
            toGet.Mass
        };

        return info;
    }

    public float[][] GetFloatInfos()
    {
        float[][] infos = new float[controllers.Count + 1][];

        int index;
        for (index = 0; index < controllers.Count; index++)
        {
            infos[index] = GetFloatInfo(index);
        }

        infos[index] = new[]
        {
            pelvis.mass,
            targetHeight,
            Vector3.Distance(GetAvgVel(), targetVelocity * moveDir)
        };

        return infos;
    }

    private Vector3[] GetVectorInfo(int index)
    {
        ArticulationBody toGet = controllers[index].articulationBody;

        Vector3[] info =
        {
            pelvis.transform.position - toGet.transform.position,
            toGet.angularVelocity,
            toGet.linearVelocity,
        };

        return info;
    }

    public Vector3[][] GetVectorInfos()
    {
        Vector3[][] infos = new Vector3[controllers.Count + 1][];

        int index;
        for (index = 0; index < controllers.Count; index++)
        {
            infos[index] = GetVectorInfo(index);
        }

        infos[index++] = new[]
        {
            pelvis.angularVelocity,
            pelvis.linearVelocity,
            GetAvgVel(),
            targetVelocity * moveDir
        };

        foreach (var info in infos)
        {
            pelvis.transform.InverseTransformDirections(info);
        }

        return infos;
    }

    public Quaternion[] GetQuaternionInfo()
    {
        Quaternion[] info = new[]
        {
            pelvis.transform.rotation,
            Quaternion.LookRotation(moveDir),
            Quaternion.FromToRotation(pelvis.transform.forward, moveDir)
        };

        return info;
    }

    public Vector3 GetAvgVel()
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

        Vector3 avgVel = velSum / bodyCount;
        return avgVel;
    }

    public void SetDrive(float[] forceRatios, float[] targets)
    {
        if (forceRatios.Length != controllers.Count)
            throw new Exception("forceRatios.length is Not Match Controllers.Count");

        if (targets.Length != controllers.Count)
            throw new Exception("targets.length is Not Match Controllers.Count");

        List<float> driveForces = new();

        int i = 0;
        controllers.ForEach(manager => driveForces.Add(manager.MaxForce * forceRatios[i++]));
        float forceSum = driveForces.Sum();

        i = 0;
        controllers.ForEach(manager => manager.SetTarget(targets[i++]));

        i = 0;
        if (forceSum > maximumForce)
        {
            float ratioSum = forceRatios.Sum();
            controllers.ForEach(manager => manager.SetForceLimit(forceRatios[i++] / ratioSum * maximumForce));
            outputForce = maximumForce;
        }
        else
        {
            controllers.ForEach(manager => manager.SetForceLimit(driveForces[i++]));
            outputForce = forceSum;
        }
    }
}
}
