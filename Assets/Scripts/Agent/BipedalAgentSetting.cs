using System;
using UnityEngine;

namespace Agent
{
public class BipedalAgentSetting : MonoBehaviour
{
    public float minSpeed;
    public float maxSpeed;

    public float forcePower;
    public float forceInterval;

    public float targetRandTimer;

    public static BipedalAgentSetting instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }
}
}
