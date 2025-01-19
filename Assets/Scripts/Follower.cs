using System;
using UnityEngine;

namespace Walk.Scripts
{
public class Follower : MonoBehaviour
{
    public GameObject toFollow;

    private void Update()
    {
        try
        {
            var targetPos = toFollow.transform.position;

            transform.position = new Vector3(targetPos.x, 0f, targetPos.z);
        }
        catch
        {
            Debug.Log("Tlqkf");
        }
    }
}
}
