using System;
using Controller;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Agent.SymmetryQuadPed
{
public class SymmetryQuadPedController : LegController
{
    [Header("LegParts")]
    public DriveController thighY1;
    public DriveController thighX1;
    public DriveController shine1;
    public DriveController foot1;

    public DriveController thighY2;
    public DriveController thighX2;
    public DriveController shine2;
    public DriveController foot2;

    public DriveController thighY3;
    public DriveController thighX3;
    public DriveController shine3;
    public DriveController foot3;

    public DriveController thighY4;
    public DriveController thighX4;
    public DriveController shine4;
    public DriveController foot4;

    public override float LookDot
    {
        get
        {
            var r1 = Quaternion.LookRotation(root.transform.position + moveDir).eulerAngles;
            var r2 = root.transform.rotation.eulerAngles;

            var delta = Mathf.Abs(r1.y - r2.y);
            delta = delta < 180 ? delta : delta - 360;
            delta = Mathf.Abs(delta) / 180;

            var lookDot = Mathf.Cos(4 * Mathf.PI * delta) * 0.5f + 0.5f;
            return lookDot;
        }
    }

    private void Awake()
    {
        controllers.Add(thighY1);
        controllers.Add(thighX1);
        controllers.Add(shine1);
        controllers.Add(foot1);

        controllers.Add(thighY2);
        controllers.Add(thighX2);
        controllers.Add(shine2);
        controllers.Add(foot2);

        controllers.Add(thighY3);
        controllers.Add(thighX3);
        controllers.Add(shine3);
        controllers.Add(foot3);

        controllers.Add(thighY4);
        controllers.Add(thighX4);
        controllers.Add(shine4);
        controllers.Add(foot4);
    }

    protected override Vector3 GetAvgVel()
    {
        Vector3 velSum = Vector3.zero;
        int bodyCount = 0;
        foreach (var controller in controllers)
        {
            ArticulationBody body = controller.articulationBody;

            velSum += body.linearVelocity;
            bodyCount++;
        }

        velSum += root.linearVelocity;
        bodyCount++;

        return velSum / bodyCount;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        supporter.UpdateOrientation(root.transform);
        MoveDir = supporter.transform.forward;

        foreach (var controller in controllers)
        {
            sensor.AddObservation(controller.MaxForce);
            sensor.AddObservation(controller.ForceUseRatio);
            sensor.AddObservation(controller.Target);
            sensor.AddObservation(controller.Target - controller.JointPos);
            sensor.AddObservation(controller.JointPos);
            sensor.AddObservation(controller.Velocity);
            sensor.AddObservation(controller.Acceleration);
        }

        sensor.AddObservation(Vector3.Distance(GetAvgVel(), targetVelocity * MoveDir));
        sensor.AddObservation(supporter.transform.InverseTransformDirection(GetAvgVel()));
        sensor.AddObservation(supporter.transform.InverseTransformDirection(targetVelocity * MoveDir));

        var relativePos = root.transform.InverseTransformPoint(targetObject.transform.position);
        sensor.AddObservation(relativePos);
        sensor.AddObservation(Quaternion.LookRotation(relativePos));
    }
}
}
