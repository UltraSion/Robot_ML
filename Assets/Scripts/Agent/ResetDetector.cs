using UnityEngine;

namespace Agent
{
public class ResetDetector : MonoBehaviour
{
    public Unity.MLAgents.Agent agent;

    public float maxHeight;
    public float minHeight;

    private void FixedUpdate()
    {
        float y = transform.position.y;

        if(y > maxHeight || y < minHeight)
            agent.EndEpisode();
    }
}
}
