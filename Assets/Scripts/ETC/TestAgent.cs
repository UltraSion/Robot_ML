using UnityEngine;

namespace ETC
{
public class TestAgent : Unity.MLAgents.Agent
{
    public float reward;

    public void Start()
    {
        reward = 0;
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log(reward);
        reward = 0;
    }

    private void FixedUpdate()
    {
        reward += 1f;
    }
}
}
