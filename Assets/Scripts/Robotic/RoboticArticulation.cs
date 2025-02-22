using UnityEngine;

namespace Robotic
{
public class RoboticArticulation : MonoBehaviour
{
    private ArticulationBody _articulationBody;

    [SerializeField]
    private float speed;

    public float Speed
    {
        get => speed;
        set
        {
            if (value is < 0 or > 1080)
                return;

            speed = value;
        }
    }

    [SerializeField]
    private float target;

    public float Target
    {
        set
        {
            var drive = _articulationBody.xDrive;
            target = Mathf.Clamp(value, drive.lowerLimit, drive.upperLimit);
        }
    }

    public float driveForce => _articulationBody.driveForce[0];

    public float JointPos => _articulationBody.jointPosition[0] * Mathf.Rad2Deg;

    public float Velocity => _articulationBody.jointVelocity[0];

    public float Acceleration => _articulationBody.jointAcceleration[0];

    public ArticulationDrive Drive
    {
        get => _articulationBody.xDrive;
        set => _articulationBody.xDrive = value;
    }

    private void Start()
    {
        _articulationBody = GetComponent<ArticulationBody>();
    }

    private void FixedUpdate()
    {
        var drive = _articulationBody.xDrive;
        var currRot = JointPos;
        var toRotPos = Mathf.MoveTowards(currRot, target, speed * Time.fixedDeltaTime);

        float realForce = driveForce;
        float beforeJointPos = JointPos;
        float beforeTarget = drive.target;
        float before = GetForcePredict(drive.target, JointPos);

        var driveDelta = (drive.target - currRot) * Time.fixedDeltaTime;

        drive.target = Mathf.Clamp(toRotPos, drive.lowerLimit, drive.upperLimit);
        _articulationBody.xDrive = drive;

        var after = GetForcePredict(drive.target, JointPos);
        float afterJointPos = JointPos;
        float afterTarget = drive.target;
        // Debug.Log($"RealForce: {realForce}, Before: {before}, After: {after}, R-B: {realForce - before}, b-a: {before - after}," +
        //           $"beforeJointPos: {beforeTarget - beforeJointPos}, afterJointPos: {(afterTarget - afterJointPos) * Mathf.Deg2Rad}, bj - aj: {(beforeTarget - beforeJointPos) - (afterTarget - afterJointPos)}");
    }

    private float GetForcePredict(float targetPos, float curPos)
    {
        var drive = _articulationBody.xDrive;

        float n1 = drive.stiffness * (targetPos - curPos) * Mathf.Deg2Rad;
        float n2 = drive.damping * (drive.targetVelocity - Velocity);
        float predict = n1 + n2;

        return predict;
    }
}
}
