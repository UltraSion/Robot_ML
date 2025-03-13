using System;
using UnityEngine;

namespace Agent
{
public class AgentSetting : MonoBehaviour
{
    public float minSpeed;
    public float maxSpeed;

    public float forcePower;
    public float forceInterval;

    public float targetRandTimer;

    public static AgentSetting instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }
}
}
