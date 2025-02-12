using UnityEngine;
using UnityEngine.Serialization;

namespace Turret
{
public class Turret : MonoBehaviour
{
    public Vector3 TargetPoint;

    protected virtual void RotateTurret()
    {
        throw new System.NotImplementedException();
    }

    void Update()
    {
        RotateTurret();
    }
}
}
