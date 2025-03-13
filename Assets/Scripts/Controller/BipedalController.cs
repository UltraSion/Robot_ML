using System;
using System.Collections.Generic;
using System.Linq;
using Agent;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Serialization;

namespace Controller
{
public class BipedalController : MonoBehaviour
{
    public ArticulationBody pelvis;

    public DriveController r_ThighX;
    public DriveController r_ThighZ;
    public DriveController r_ThighY;
    public DriveController r_Shine;
    public DriveController r_Foot;
    public DriveController l_ThighX;
    public DriveController l_ThighZ;
    public DriveController l_ThighY;
    public DriveController l_Shine;
    public DriveController l_Foot;
    public DriveController body;

    public List<DriveController> controllers = new();

    public PositionSupporter positionSupporter;
    [SerializeField] private Vector3 moveDir;
    [SerializeField] private float targetVelocity;

    public PowerHub powerHub;

    public float Efficiency { get; private set; }

    private void Awake()
    {
        controllers.Add(r_ThighX);
        controllers.Add(r_ThighZ);
        controllers.Add(r_ThighY);
        controllers.Add(r_Shine);
        controllers.Add(r_Foot);
        controllers.Add(l_ThighX);
        controllers.Add(l_ThighZ);
        controllers.Add(l_ThighY);
        controllers.Add(l_Shine);
        controllers.Add(l_Foot);
        controllers.Add(body);
    }

    public Vector3 MoveDir
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
    {
        get
        {
            var velDelta = Vector3.Distance(GetAvgVel(), MoveDir * targetVelocity);
            var velAccuracy = Mathf.Clamp(velDelta, 0, targetVelocity);
            return Mathf.Clamp01(1 - velAccuracy / targetVelocity);
        }
    }

    public float VelocityAccuracy2
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

    public float PelvisUprightDot
        => Vector3.Dot(pelvis.transform.up, Vector3.up);

    public float FootLookDot
    {
        get
        {
            Vector3 leftFootForward = l_Foot.transform.forward;
            leftFootForward.y = 0;
            Vector3 rightFootForward = r_Foot.transform.forward;
            rightFootForward.y = 0;

            float leftDot = Vector3.Dot(MoveDir, leftFootForward.normalized);
            float rightDot = Vector3.Dot(MoveDir, rightFootForward.normalized);

            leftDot = (leftDot + 1) * 0.5f;
            rightDot = (rightDot + 1) * 0.5f;

            return leftDot * rightDot;
        }
    }

    private float[] GetFloatInfo(int index)
    {
        DriveController toGet = controllers[index];
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
        positionSupporter.UpdateOrientation(pelvis.transform);
        MoveDir = positionSupporter.transform.forward;

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
        float usableRatio = requestedForcesSum == 0 ? 1 : forceToUse / requestedForcesSum;

        i = 0;
        controllers.ForEach(controller => controller.ForceUseRatio = forceRatios[i++] * usableRatio);

        powerHub.ReleaseForce(forceToUse);
        Efficiency = 1f - forceToUse / usableForce;
    }
}
}
