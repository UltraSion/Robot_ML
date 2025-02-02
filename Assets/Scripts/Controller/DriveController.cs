using System;
using UnityEngine;

namespace Controller
{
public class DriveController : MonoBehaviour
{
    public ArticulationBody articulationBody;
    // public float MaxForce = 20000f;
    public float driveForce => articulationBody.driveForce[0];
    public float softMaxForce;

    // public float ForceUsage
    // {
    //     get
    //     {
    //         var forceLimit = articulationBody.xDrive.forceLimit;
    //         return forceLimit / MaxForce;
    //     }
    // }

    public float Target
    {
        get
        {
            var drive = articulationBody.xDrive;
            var target = drive.target;
            return Mathf.InverseLerp(drive.lowerLimit, drive.upperLimit, target);
        }
    }
    public float JointPos
    {
        get
        {
            var drive = articulationBody.xDrive;
            var curPos = articulationBody.jointPosition[0] * Mathf.Rad2Deg;
            return Mathf.InverseLerp(drive.lowerLimit, drive.upperLimit, curPos);
        }
    }
    public float Velocity => articulationBody.jointVelocity[0];
    public float Acceleration => articulationBody.jointAcceleration[0];
    // public float Mass => articulationBody.mass;

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
