using Controller;
using TMPro;
using UnityEngine;

namespace Agent
{
public class BipedalAgentUI : MonoBehaviour
{
    public TextMeshProUGUI maxSpeed;
    public TextMeshProUGUI speedReward;
    public TextMeshProUGUI speedReward_avg;
    public TextMeshProUGUI episodeLength;

    public BipedalAgent02 bipedalAgent;
    public BipedalController02 bipedalController;

    void Update()
    {
        maxSpeed.text = bipedalAgent.maxSpeed.ToString();
        speedReward.text = bipedalController.VelocityAccuracy2.ToString();
        speedReward_avg.text = bipedalAgent.Avg_speedReward.ToString();
        episodeLength.text = bipedalAgent.CurStep.ToString();
    }
}
}
