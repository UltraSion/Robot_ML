using System;
using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    public Unity.MLAgents.Agent agent;

    private void OnCollisionEnter(Collision other)
    {
        if (other.transform.CompareTag("Ground"))
        {
            agent.AddReward((-1f));
            agent.EndEpisode();
        }
    }
}
