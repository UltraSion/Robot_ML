using UnityEngine;

namespace Turret
{
public class TestTurret : Turret
{
    public GameObject turretHead;

    public override void TargetLookDir(Vector3 targetDir)
    {
        turretHead.transform.rotation = Quaternion.LookRotation(targetDir);
    }
}
}
