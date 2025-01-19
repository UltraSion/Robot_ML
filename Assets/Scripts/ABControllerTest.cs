using System;
using UnityEngine;

namespace Walk.Scripts
{
public class ABControllerTest : MonoBehaviour
{
    private ABController Controller;

    private void Start()
    {
        Controller = GetComponent<ABController>();
    }

    void Update()
    {
        string msg = "";
        var info = Controller.GetInfos();

        for (int i = 0; i < info.Length; i++)
        {
            msg += $"Force: {info[i][0]}, " +
                   $"JointPos: {info[i][1]}, " +
                   $"Velocity: {info[i][2]}, " +
                   $"Acceleration: {info[i][3]}, " +
                   $"Mass: {info[i][4]}";

            msg += "\n";
        }

        Debug.Log(msg);
    }
}
}
