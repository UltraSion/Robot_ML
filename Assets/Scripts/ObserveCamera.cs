using System;
using UnityEngine;

namespace DefaultNamespace
{
public class ObserveCamera : MonoBehaviour
{
    public GameObject toObserve;

    private float fov;
    private float startDistance;

    private void Awake()
    {
        fov = Camera.main.fieldOfView;
        startDistance = (toObserve.transform.position - transform.position).magnitude;
    }

    void Update()
    {
        float distance = (toObserve.transform.position - transform.position).magnitude;

        var newFov = fov * startDistance / distance;
        Camera.main.fieldOfView = newFov;

        var lookDir = toObserve.transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(lookDir);
    }
}
}
