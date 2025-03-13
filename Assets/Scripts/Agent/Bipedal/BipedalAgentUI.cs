using Controller;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

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
    public TextMeshProUGUI totalReward;
    public TextMeshProUGUI episodeLength;

    public Slider curTargetSpeedSlider;
    public Slider speedRewardSlider;
    public Slider speedReward_avgSlider;
    public Slider effortRewardSlider;
    public Slider pelvisRewardSlider;
    public Slider footRewardSlider;
    public Slider totalRewardSlider;
    public Slider episodeLengthSlider;

    public BipedalAgent bipedalAgent;
    public BipedalController bipedalController;
    public AgentSetting setting;

    private float agentTargetSpeed;
    private float agentSpeedReward;
    private float agentEffortReward;
    private float agentPelvisReward;
    private float agentFootReward;
    private float agentTotalReward;

    void Update()
    {
        maxSpeed.text = setting.maxSpeed.ToString();
        curTargetSpeed.text = agentTargetSpeed.ToString();
        speedReward.text = agentSpeedReward.ToString();
        speedReward_avg.text = bipedalAgent.Avg_speedReward.ToString();
        effortReward.text = agentEffortReward.ToString();
        pelvisReward.text = agentPelvisReward.ToString();
        footReward.text = agentFootReward.ToString();
        totalReward.text = agentTotalReward.ToString();
        episodeLength.text = bipedalAgent.CurStep.ToString();

        curTargetSpeedSlider.value = agentTargetSpeed / setting.maxSpeed;
        speedRewardSlider.value = agentSpeedReward;
        speedReward_avgSlider.value = bipedalAgent.Avg_speedReward;
        effortRewardSlider.value = agentEffortReward;
        pelvisRewardSlider.value = agentPelvisReward;
        footRewardSlider.value = agentFootReward;
        totalRewardSlider.value = agentEffortReward;
        episodeLengthSlider.value = bipedalAgent.CurStep / bipedalAgent.MaxStep;
    }

    public void UpdateParameters(float targetSpeed, float speed, float effort, float pelvis, float foot, float total)
    {
        agentTargetSpeed = targetSpeed;
        agentSpeedReward = speed;
        agentEffortReward = effort;
        agentPelvisReward = pelvis;
        agentFootReward = foot;
        agentTotalReward = total;
    }
}
}
