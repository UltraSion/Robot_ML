using UnityEngine;

namespace DefaultNamespace
{
public class TargetObject : RandomObject
{
    public float minDistance;
    public float maxDistance;

    public override void Rand()
    {
        float targetDir = Random.Range(0.0f, 360.0f);
        float targetDistance = Random.Range(minDistance, maxDistance);

        Vector3 targetPos = new Vector3(
            Mathf.Cos(targetDir) * targetDistance,
            transform.localPosition.y,
            Mathf.Sin(targetDir) * targetDistance
            );

        transform.localPosition = targetPos;
    }
}
}
