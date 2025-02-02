using UnityEngine;

namespace ETC
{
public class Follower : MonoBehaviour
{
    public GameObject toFollow;

    private void Update()
    {
        try
        {
            var targetPos = toFollow.transform.position;
            if (float.IsNaN(targetPos.x))
                return;

            if (float.IsNaN(targetPos.z))
                return;
            transform.position = new Vector3(targetPos.x, 0f, targetPos.z);
        }
        catch
        {
            Debug.Log("Tlqkf");
        }
    }
}
}
