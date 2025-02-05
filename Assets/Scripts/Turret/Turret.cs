using UnityEngine;

namespace Turret
{
public class Turret : MonoBehaviour, ITurret
{
    public virtual void TargetLookDir(Vector3 targetDir)
    {
        throw new System.NotImplementedException();
    }
}
}
