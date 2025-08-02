using System;
using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    public Unity.MLAgents.Agent agent;

    private void OnCollisionEnter(Collision other)
    {
        if (other.transform.CompareTag("Ground"))
        {
            // agent.AddReward((-1f));
            agent.EndEpisode();
        }
    }
    // private void OnCollisionStay(Collision other)
    // {
    //     if (other.transform.CompareTag("Ground"))
    //     {
    //         var uprightDot = Vector3.Dot(agent.transform.up, Vector3.down);
    //         float penalty = -(0.01f + (uprightDot + 1f) * 0.5f * 0.01f);
    //         agent.AddReward(penalty);
    //         // agent.EndEpisode();
    //     }
    // }
}
