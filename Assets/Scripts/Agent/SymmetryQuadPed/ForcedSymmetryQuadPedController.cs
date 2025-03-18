using System;
using System.Collections.Generic;
using Controller;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Agent.SymmetryQuadPed
{
public class ForcedSymmetryQuadPedController : LegController
{
    public class Leg
    {
        public DriveController thighY;
        public DriveController thighX;
        public DriveController shine;
        public GameObject foot;
    }

    [Header("LegParts")] public DriveController thighY1;
    public DriveController thighX1;
    public DriveController shine1;

    public DriveController thighY2;
    public DriveController thighX2;
    public DriveController shine2;

    public DriveController thighY3;
    public DriveController thighX3;
    public DriveController shine3;

    public DriveController thighY4;
    public DriveController thighX4;
    public DriveController shine4;

    public GameObject foot1;
    public GameObject foot2;
    public GameObject foot3;
    public GameObject foot4;

    public int FirstLeg { get; private set; } = 0;
    public readonly List<Leg> _legs = new();

    private Vector3 RotationDelta
    {
        get
        {
            var dir = root.transform.InverseTransformDirection(moveDir);
            var lookDir = Quaternion.LookRotation(dir).eulerAngles;

            lookDir.x = lookDir.x < 180 ? lookDir.x : lookDir.x - 360;
            lookDir.y = lookDir.y < 180 ? lookDir.y : lookDir.y - 360;
            lookDir.z = lookDir.z < 180 ? lookDir.z : lookDir.z - 360;

            return lookDir;
        }
    }

    public override float LookDot
    {
        get
        {
            var delta = Mathf.Abs(RotationDelta.y) / 180;
            var lookDot = Mathf.Cos(4 * Mathf.PI * delta) * 0.5f + 0.5f;

            return lookDot;
        }
    }

    public void SortLeg()
    {
        var delta = RotationDelta.y;
        var absDelta = Mathf.Abs(delta);

        if (absDelta < 45)
            FirstLeg = 0;
        else if (absDelta < 135)
            FirstLeg = delta > 0 ? 1 : 3;
        else
            FirstLeg = 2;
    }

    private void Awake()
    {
        controllers.Add(thighY1);
        controllers.Add(thighX1);
        controllers.Add(shine1);

        controllers.Add(thighY2);
        controllers.Add(thighX2);
        controllers.Add(shine2);

        controllers.Add(thighY3);
        controllers.Add(thighX3);
        controllers.Add(shine3);

        controllers.Add(thighY4);
        controllers.Add(thighX4);
        controllers.Add(shine4);

        _legs.Add(new Leg()
        {
            thighY = thighY4,
            thighX = thighX4,
            shine = shine4,
            foot = foot4
        });

        _legs.Add(new Leg()
        {
            thighY = thighY1,
            thighX = thighX1,
            shine = shine1,
            foot = foot1
        });

        _legs.Add(new Leg()
        {
            thighY = thighY2,
            thighX = thighX2,
            shine = shine2,
            foot = foot2
        });

        _legs.Add(new Leg()
        {
            thighY = thighY3,
            thighX = thighX3,
            shine = shine3,
            foot = foot3
        });
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

    private void CollectDriveObservations(DriveController controller, VectorSensor sensor)
    {
        sensor.AddObservation(controller.MaxForce);
        sensor.AddObservation(controller.ForceUseRatio);
        sensor.AddObservation(controller.Target);
        sensor.AddObservation(controller.Target - controller.JointPos);
        sensor.AddObservation(controller.JointPos);
        sensor.AddObservation(controller.Velocity);
        sensor.AddObservation(controller.Acceleration);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        SortLeg();
        supporter.UpdateOrientation(root.transform);
        MoveDir = supporter.transform.forward;

        for (int i = 0; i < _legs.Count; i++)
        {
            var legNum = FirstLeg + i;
            legNum = legNum < _legs.Count ? legNum : legNum - _legs.Count;
            var leg = _legs[legNum];
            CollectDriveObservations(leg.thighY, sensor);
            CollectDriveObservations(leg.thighX, sensor);
            CollectDriveObservations(leg.shine, sensor);
            sensor.AddObservation(
                supporter.transform.InverseTransformDirection(
                    leg.foot.transform.position - supporter.transform.position));
        }

        RaycastHit hit;
        float maxRaycastDist = 1;
        if (Physics.Raycast(root.transform.position, Vector3.down, out hit, maxRaycastDist))
        {
            sensor.AddObservation(hit.distance / maxRaycastDist);
        }
        else
            sensor.AddObservation(1);

        sensor.AddObservation(Vector3.Distance(GetAvgVel(), targetVelocity * MoveDir));
        sensor.AddObservation(supporter.transform.InverseTransformDirection(GetAvgVel()));
        sensor.AddObservation(supporter.transform.InverseTransformDirection(targetVelocity * MoveDir));

        var delta = RotationDelta / 180;
        sensor.AddObservation(delta);

        sensor.AddObservation(supporter.transform.InverseTransformDirection(root.linearVelocity));
        sensor.AddObservation(supporter.transform.InverseTransformDirection(root.angularVelocity));
    }

    public override void SetDrive(float[] forceRatios, float[] targets)
    {
        float usableForce = powerHub.UsableForce;
        float requestedForcesSum = 0f;

        int i = 0;
        controllers.ForEach(controller => requestedForcesSum += controller.MaxForce * forceRatios[i++]);

        float forceToUse = requestedForcesSum < usableForce ? requestedForcesSum : usableForce;
        float usableRatio = requestedForcesSum == 0 ? 1 : forceToUse / requestedForcesSum;

        powerHub.ReleaseForce(forceToUse);
        Efficiency = 1f - forceToUse / usableForce;

        Leg[] legs = new Leg[4];
        for (int l = 0; l < _legs.Count; l++)
        {
            var legNum = FirstLeg + l;
            legNum = legNum < _legs.Count ? legNum : legNum - _legs.Count;
            legs[l] = _legs[legNum];
        }

        var range = targets[0] > 0.5f ? 1 - targets[0] : targets[0];
        var t1 = targets[0] + range * (targets[1] * 2f - 1f);
        var t2 = 1 - targets[0] + range * (targets[1] * 2f - 1f);

        legs[0].thighY.Target = targets[0] + range * (targets[1] * 2f - 1f);
        legs[1].thighY.Target = 1 - targets[0] + range * (targets[1] * 2f - 1f);
        legs[2].thighY.Target = targets[0] - range * (targets[1] * 2f - 1f);
        legs[3].thighY.Target = 1 - targets[0] - range * (targets[1] * 2f - 1f);

        range = targets[2] > 0.5f ? 1 - targets[2] : targets[2];
        t1 = targets[2] + range * (targets[3] * 2f - 1f);
        t2 = targets[2] - range * (targets[3] * 2f - 1f);

        legs[0].thighX.Target = t1;
        legs[2].thighX.Target = t1;

        legs[1].thighX.Target = t2;
        legs[3].thighX.Target = t2;

        range = targets[4] > 0.5f ? 1 - targets[4] : targets[4];
        t1 = targets[4] + range * (targets[5] * 2f - 1f);
        t2 = targets[4] - range * (targets[5] * 2f - 1f);

        legs[0].shine.Target = t1;
        legs[3].shine.Target = t1;

        legs[1].shine.Target = t2;
        legs[2].shine.Target = t2;

        i = 0;
        for (int l = 0; l < _legs.Count; l++)
        {
            var legNum = FirstLeg + l;
            legNum = legNum < _legs.Count ? legNum : legNum - _legs.Count;
            var leg = _legs[legNum];
            leg.thighY.ForceUseRatio = forceRatios[i++] * usableRatio;
            leg.thighX.ForceUseRatio = forceRatios[i++] * usableRatio;
            leg.shine.ForceUseRatio = forceRatios[i++] * usableRatio;
        }
    }
}
}
