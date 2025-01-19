using System.Collections.Generic;
using UnityEngine;

namespace Walk.Scripts
{
public class Test : MonoBehaviour
{
    private ArticulationBody body;
    public float i = 0;

    void Start()
    {
        body = GetComponent<ArticulationBody>();
    }

    void Update()
    {
        // var drive = body.xDrive;
        //
        // float jointForce = body.jointForce[0];
        // float driveForce = body.driveForce[0];
        // float forceLimit = drive.forceLimit;
        // float target = drive.target;
        // float curPos = body.jointPosition[0] * Mathf.Rad2Deg;
        // Debug.Log("jointVelocity: " + body.jointVelocity[0] * Mathf.Rad2Deg + ", driveForce: " + driveForce + ", target: " + target + ", curPos: " + curPos);
        // Debug.DrawLine(transform.position, transform.position + transform.forward);
        Debug.Log(body.worldCenterOfMass);
    }
}
}
