using Controller;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Agent
{
public class BipedalAgentUI : MonoBehaviour
{
    public TextMeshProUGUI maxSpeed;
    public TextMeshProUGUI curTargetSpeed;
    public TextMeshProUGUI speedReward;
    public TextMeshProUGUI speedReward_avg;
    public TextMeshProUGUI episodeLength;

    public BipedalAgent02 bipedalAgent;
    public BipedalController02 bipedalController;
    public BipedalAgentSetting setting;

    void Update()
    {
        maxSpeed.text = setting.maxSpeed.ToString();
        curTargetSpeed.text = bipedalController.TargetVelocity.ToString();
        speedReward.text = bipedalController.VelocityAccuracy2.ToString();
        speedReward_avg.text = bipedalAgent.Avg_speedReward.ToString();
        episodeLength.text = bipedalAgent.CurStep.ToString();
    }
}
}
