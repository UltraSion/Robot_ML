using System;
using UnityEngine;

namespace DefaultNamespace
{
public class RoboticArticulation : MonoBehaviour
{
    public ArticulationBody _articulationBody;

    public float speed;
    public float target;
    public float driveForce => _articulationBody.driveForce[0];

    private void Start()
    {
        _articulationBody = GetComponent<ArticulationBody>();
    }

    public void SetTarget(float value)
    {
        var drive = _articulationBody.xDrive;
        target = Mathf.Clamp(value, drive.lowerLimit, drive.upperLimit);
    }

    private void RotateTo(float value)
    {
        var drive = _articulationBody.xDrive;
        drive.target = Mathf.Clamp(value, drive.lowerLimit, drive.upperLimit);
        _articulationBody.xDrive = drive;
    }

    private void FixedUpdate()
    {
        var currRot = _articulationBody.jointPosition[0] * Mathf.Rad2Deg;
        var toRotPos = Mathf.MoveTowards(currRot, target, speed * Time.fixedDeltaTime);
        Debug.Log($"Target: {target}, CurrPos: {currRot}, ToRotPos: {toRotPos}");
        RotateTo(toRotPos);
    }
}
}
