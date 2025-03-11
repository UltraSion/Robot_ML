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
    public TextMeshProUGUI effortReward;
    public TextMeshProUGUI pelvisReward;
    public TextMeshProUGUI footReward;
    public TextMeshProUGUI episodeLength;

    public BipedalAgent02 bipedalAgent;
    public BipedalController02 bipedalController;
    public BipedalAgentSetting setting;

    public float agentTargetSpeed;
    public float agentSpeedReward;
    public float agentEffortReward;
    public float agentPelvisReward;
    private float agentFootReward;

    void Update()
    {
        maxSpeed.text = setting.maxSpeed.ToString();
        curTargetSpeed.text = agentTargetSpeed.ToString();
        speedReward.text = agentSpeedReward.ToString();
        speedReward_avg.text = bipedalAgent.Avg_speedReward.ToString();
        effortReward.text = agentEffortReward.ToString();
        pelvisReward.text = agentPelvisReward.ToString();
        footReward.text = agentFootReward.ToString();
        episodeLength.text = bipedalAgent.CurStep.ToString();
    }

    public void UpdateParameters(float targetSpeed, float speed, float effort, float pelvis, float foot)
    {
        agentTargetSpeed = targetSpeed;
        agentSpeedReward = speed;
        agentEffortReward = effort;
        agentPelvisReward = pelvis;
        agentFootReward = foot;
    }
}
}
