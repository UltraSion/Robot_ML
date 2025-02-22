using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Controller
{
public class DriveController02 : MonoBehaviour
{
    public ArticulationBody articulationBody;

    [SerializeField]
    private float maxForce;

    public float MaxForce
    {
        get => maxForce;
        set
        {
            if (value < 0)
                return;

            maxForce = value;
        }
    }

    public float DriveForce => articulationBody.driveForce[0];

    public float ForceUseRatio
    {
        get
        {
            var drive = articulationBody.xDrive;
            return Mathf.InverseLerp(0f, maxForce, drive.forceLimit);
        }
        set
        {
            var drive = articulationBody.xDrive;
            drive.forceLimit =  Mathf.Lerp(0f, maxForce, value);
            articulationBody.xDrive = drive;
        }
    }

    public float Target
    {
        get
        {
            var drive = articulationBody.xDrive;
            return Mathf.InverseLerp(drive.lowerLimit, drive.upperLimit, drive.target);
        }

        set
        {
            var drive = articulationBody.xDrive;
            drive.target =  Mathf.Lerp(drive.lowerLimit, drive.upperLimit, value);
            articulationBody.xDrive = drive;
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

    void Awake()
    {
         articulationBody = GetComponent<ArticulationBody>();
    }
}
}
