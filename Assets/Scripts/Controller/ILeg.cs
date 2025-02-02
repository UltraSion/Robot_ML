using UnityEngine;

namespace Controller
{
public enum LegDrive {Neutral, Move}
public interface ILeg
{
    public void SetState(LegDrive drive);
    public void SetMoveDir(Vector3 dir);
    public void SetSpeed(float speed);
}
}
