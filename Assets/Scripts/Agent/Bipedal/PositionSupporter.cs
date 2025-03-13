using UnityEngine;

namespace Agent
{
public class PositionSupporter : MonoBehaviour
{
    public GameObject targetObject;

    public void UpdateOrientation(Transform rootBP)
    {
        var dirVector = targetObject.transform.position - transform.position;
        dirVector.y = 0;
        var lookRot =
            dirVector == Vector3.zero
                ? Quaternion.identity
                : Quaternion.LookRotation(dirVector);

        transform.SetPositionAndRotation(rootBP.position, lookRot);
    }
}
}
