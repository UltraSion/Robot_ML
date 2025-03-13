using Controller;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Agent.SymmetryQuadPed
{
public class SymmetryQuadPedUI : MonoBehaviour
{
    public TextMeshProUGUI maxSpeed;
    public TextMeshProUGUI curTargetSpeed;
    public TextMeshProUGUI speedReward;
    public TextMeshProUGUI speedReward_avg;
    public TextMeshProUGUI effortReward;
    public TextMeshProUGUI totalReward;
    public TextMeshProUGUI episodeLength;

    public Slider curTargetSpeedSlider;
    public Slider speedRewardSlider;
    public Slider speedReward_avgSlider;
    public Slider effortRewardSlider;
    public Slider totalRewardSlider;
    public Slider episodeLengthSlider;

    public LegAgent LegAgent;
    public AgentSetting setting;

    private float agentTargetSpeed;
    private float agentSpeedReward;
    private float agentEffortReward;
    private float agentPelvisReward;
    private float agentFootReward;
    private float agentTotalReward;

    void Start()
    {
        setting = AgentSetting.instance;
    }

    void Update()
    {
        maxSpeed.text = setting.maxSpeed.ToString();
        curTargetSpeed.text = agentTargetSpeed.ToString();
        speedReward.text = agentSpeedReward.ToString();
        speedReward_avg.text = LegAgent.Avg_speedReward.ToString();
        effortReward.text = agentEffortReward.ToString();
        totalReward.text = agentTotalReward.ToString();
        episodeLength.text = LegAgent.CurStep.ToString();

        curTargetSpeedSlider.value = agentTargetSpeed / setting.maxSpeed;
        speedRewardSlider.value = agentSpeedReward;
        speedReward_avgSlider.value = LegAgent.Avg_speedReward;
        effortRewardSlider.value = agentEffortReward;
        totalRewardSlider.value = agentEffortReward;
        episodeLengthSlider.value = LegAgent.CurStep / LegAgent.MaxStep;
    }

    public void UpdateParameters(float targetSpeed, float speed, float effort, float total)
    {
        agentTargetSpeed = targetSpeed;
        agentSpeedReward = speed;
        agentEffortReward = effort;
        agentTotalReward = total;
    }
}
}
