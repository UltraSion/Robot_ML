using DefaultNamespace;
using UnityEngine;

namespace Turret
{
public class TestTurret : Turret
{
    public GameObject turretHead;

    protected override void RotateTurret()
    {
        turretHead.transform.rotation = Quaternion.LookRotation(TargetPoint - Hub.MainCamera.transform.position);
    }
}
}
