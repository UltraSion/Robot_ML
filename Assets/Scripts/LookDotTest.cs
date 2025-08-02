using UnityEngine;

namespace DefaultNamespace
{
public class LookDotTest : MonoBehaviour
{
    void Update()
    {
        Debug.DrawLine(transform.position, transform.position + Vector3.forward);

        var r1 = Quaternion.LookRotation(Vector3.forward).eulerAngles.y;
        var r2 = transform.rotation.eulerAngles.y;

        var delta = Mathf.Abs(r1 - r2);
        delta = delta < 180 ? delta : delta - 360;
        delta = Mathf.Abs(delta) / 180;

        // Debug.Log($"r1: {r1}, r2: {r2}, delta: {delta}");

        Debug.Log($"r1: {r1}, r2: {r2}, result: {Mathf.Cos(4 * Mathf.PI * delta) * 0.5f + 0.5f}");
    }
}
}
