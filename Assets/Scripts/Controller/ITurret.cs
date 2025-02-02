using UnityEngine;

namespace Controller
{
public interface ITurret
{
    public void RotateY(int dir);
    public void RotateX(int dir);
    public void TargetVector(Vector3 dir);
}
}
