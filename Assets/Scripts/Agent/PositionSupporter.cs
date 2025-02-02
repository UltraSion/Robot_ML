using UnityEngine;

namespace Agent
{
public class PositionSupporter : MonoBehaviour
{
    public void UpdateOrientation(Transform rootBP, Vector3 dirVector)
    {
        dirVector.y = 0; //flatten dir on the y. this will only work on level, uneven surfaces
        var lookRot =
            dirVector == Vector3.zero
                ? Quaternion.identity
                : Quaternion.LookRotation(dirVector); //get our look rot to the target

        //UPDATE ORIENTATION CUBE POS & ROT
        transform.SetPositionAndRotation(rootBP.position, lookRot);
    }
}
}
