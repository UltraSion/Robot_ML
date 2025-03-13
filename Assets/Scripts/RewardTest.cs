using System;
using UnityEngine;

namespace DefaultNamespace
{
public class RewardTest : MonoBehaviour
{
    [Range(0, 1)] public float v;
    [Range(0, 1)] public float sr;
    [Range(0, 1)] public float e;

    [SerializeField][Range(0, 1)] private float averageSpeedReward = 0;
    private float curStep = 0;

    private void Update()
    {
        float d = (1 - sr);
        float er = d + e * (1 - d);
        float newR = sr * v + e * (1 - sr * v);

        CalculateAverageSpeedReward(sr);

        if(Input.GetKeyDown(KeyCode.Space))
            OptimizeMaxSpeed();
    }

    private void CalculateAverageSpeedReward(float speedReward)
    {
        float rewardSum = averageSpeedReward * curStep;
        rewardSum += speedReward;
        averageSpeedReward = rewardSum / ++curStep;
    }

    private void OptimizeMaxSpeed()
    {
        Debug.Log(averageSpeedReward);


        averageSpeedReward = 0f;
        curStep = 0;
    }
}
}
