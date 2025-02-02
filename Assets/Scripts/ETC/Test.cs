using UnityEngine;

namespace ETC
{
public class Test : MonoBehaviour
{
    private ArticulationBody body;
    public float i = 0;

    public float targetHeight;
    public Vector3 moveDirection;

    void Start()
    {
        // body = GetComponent<ArticulationBody>();

        targetHeight = Random.Range(1.5f, 4.5f);

        float moveDir = Random.Range(0.0f, 360.0f);
        moveDirection = new Vector3(Mathf.Cos(moveDir), 0f, Mathf.Sin(moveDir));

    }

    void Update()
    {
        // float pelvisY = transform.position.y;
        // var heightDelta = Mathf.Abs(targetHeight - pelvisY);
        // float heightReward = Mathf.Clamp(1 - heightDelta / targetHeight, 0, 1) * 2f;
        // Debug.Log(heightReward);

        Debug.Log($"rotation: {transform.rotation}, " +
                  $"lookDir: {Quaternion.LookRotation(moveDirection)}"  +
                  $"gap: {Quaternion.FromToRotation(transform.forward, moveDirection)}");

        Debug.DrawLine(transform.position, transform.position + moveDirection);
    }
}
}
