using System;
using System.Collections.Generic;
using UnityEngine;

namespace Walk.Scripts
{
public class ABManager : MonoBehaviour
{
    public ArticulationBody articulationBody;


    public float Force
    {
        get
        {
            var forceLimit = articulationBody.xDrive.forceLimit;
            return forceLimit == 0 ? forceLimit : articulationBody.driveForce[0] / forceLimit;
        }
    }

    public float Velocity => articulationBody.jointVelocity[0];
    public float Acceleration => articulationBody.jointAcceleration[0];

    public float Mass => articulationBody.mass;

    public float jointPos
    {
        get
        {
            var drive = articulationBody.xDrive;
            var curPos = articulationBody.jointPosition[0] * Mathf.Rad2Deg;
            return Mathf.InverseLerp(drive.lowerLimit, drive.upperLimit, curPos);
        }
    }

    void Start()
    {
        articulationBody = GetComponent<ArticulationBody>();
    }

    public void SetForceLimit(float force)
    {
        var drive = articulationBody.xDrive;
        drive.forceLimit = force;
        articulationBody.xDrive = drive;
    }

    public void SetTarget(float value)
    {
        if (value is < 0 or > 1)
            throw new Exception("Value is Out of Range");

        var drive = articulationBody.xDrive;
        drive.target =  Mathf.Lerp(drive.lowerLimit, drive.upperLimit, value);
        articulationBody.xDrive = drive;
    }
}
}
