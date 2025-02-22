using UnityEngine;

namespace DefaultNamespace
{
public class ATBDForceTest : MonoBehaviour
{
    public ArticulationBody articulationBody;

    void Start()
    {
        articulationBody = GetComponent<ArticulationBody>();
    }

    void FixedUpdate()
    {
        if (articulationBody == null)
            return;

        // 현재 조인트 상태 가져오기
        var currentPosition = articulationBody.jointPosition;
        var currentVelocity = articulationBody.jointVelocity;

        // 목표 상태 가져오기
        var targetPosition = articulationBody.xDrive.target;
        var targetVelocity = articulationBody.xDrive.targetVelocity;

        // 강성 및 감쇠 계수 가져오기
        float stiffness = articulationBody.xDrive.stiffness;
        float damping = articulationBody.xDrive.damping;

        // Force 계산
        float force = ComputeJointForce(
            stiffness, damping,
            currentPosition[0] * Mathf.Rad2Deg, targetPosition,
            currentVelocity[0], targetVelocity
        );

        // 계산된 힘을 출력하거나 적용 가능
        Debug.Log($"Calculated Force: {force}, RealForce: {articulationBody.driveForce[0]}, um: {force / articulationBody.driveForce[0]}");
    }

    float ComputeJointForce(float stiffness, float damping,
        float currentPos, float targetPos,
        float currentVel, float targetVel)
    {
        // PD 제어기 공식 적용
        float force = stiffness * (targetPos - currentPos) * Mathf.Deg2Rad + damping * (targetVel - currentVel);
        return force;
    }
}
}
