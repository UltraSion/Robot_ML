using Robotic;
using UnityEngine;

namespace Turret
{
public class PrototypeTurret : Turret
{
    public RoboticArticulation XJoint;
    public RoboticArticulation YJoint;

    protected override void RotateTurret()
    {
        var targetDir = transform.InverseTransformDirection(TargetPoint);
        var targetRot = Quaternion.LookRotation(targetDir).eulerAngles;

        targetRot.x = targetRot.x > 180 ? targetRot.x - 360 : targetRot.x;
        targetRot.y = targetRot.y > 180 ? targetRot.y - 360 : targetRot.y;

        XJoint.Target = targetRot.x;
        YJoint.Target = targetRot.y;
    }
}
}
