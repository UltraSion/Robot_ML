using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace Walk.Scripts
{
public class ABController : MonoBehaviour
{
    public List<ABManager> managers = new List<ABManager>();
    public ArticulationBody pelvis;

    public float maximumForce;

    [ReadOnly]
    public float outputForce;

    private float[] GetInfo(int index)
    {
        ABManager toGet = managers[index];
        float[] info = new float[5];
        info[0] = toGet.Force;
        info[1] = toGet.jointPos;
        info[2] = toGet.Velocity;
        info[3] = toGet.Acceleration;
        info[4] = toGet.Mass;

        return info;
    }

    public float[][] GetInfos()
    {
        float[][] infos = new float[managers.Count][];

        for (int i = 0; i < managers.Count; i++)
        {
            infos[i] = GetInfo(i);
        }

        return infos;
    }

    public Vector3 GetAvgVel()
    {
        Vector3 velSum = Vector3.zero;
        int bodyCount = 0;
        foreach (var manager in managers)
        {
            ArticulationBody body = manager.articulationBody;
            if(body.mass <= 0)
                continue;

            velSum += body.linearVelocity;
            bodyCount++;
        }

        Vector3 avgVel = velSum / bodyCount;
        return avgVel;
    }

    public void SetDrive(float outputRatio, float[] forceRatios, float[] targets)
    {
        if (outputRatio is < 0 or > 1)
            throw new Exception("outputRatio is Out of Range");

        if (forceRatios.Length != managers.Count)
            throw new Exception("forceRatios.length is Not Match Controllers.Count");

        if (targets.Length != managers.Count)
            throw new Exception("targets.length is Not Match Controllers.Count");

        outputForce = maximumForce * outputRatio;
        float ratioSum = forceRatios.Sum();

        for (int i = 0; i < managers.Count; i++)
        {
            var force = forceRatios[i] / ratioSum * outputForce;
            var target = targets[i];
            managers[i].SetForceLimit(force);
            managers[i].SetTarget(target);
        }
    }


}
}
