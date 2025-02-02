using Controller;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ETC
{
public class ABControllerTest : MonoBehaviour
{
    private BipedalController Controller;

    public Vector3 moveDirection;


    private void Start()
    {
        Controller = GetComponent<BipedalController>();

        float moveDir = Random.Range(0.0f, 360.0f);

        moveDirection = new Vector3(Mathf.Cos(moveDir), 0f, Mathf.Sin(moveDir));

    }

    void Update()
    {
        // string msg = "";
        // var info = Controller.GetInfos();
        //
        // for (int i = 0; i < info.Length; i++)
        // {
        //     msg += $"Force: {info[i][0]}, " +
        //            $"JointPos: {info[i][1]}, " +
        //            $"Velocity: {info[i][2]}, " +
        //            $"Acceleration: {info[i][3]}, " +
        //            $"Mass: {info[i][4]}";
        //
        //     msg += "\n";
        // }
        //
        // Debug.Log(msg);

        float lookDot = Vector3.Dot(moveDirection, Controller.controllers[0].transform.forward);
        float lookReward = Mathf.Abs(lookDot);
        Debug.Log(lookReward);

        Debug.DrawLine(Controller.pelvis.transform.position, Controller.pelvis.transform.position + moveDirection);
    }
}
}
