using System;
using DefaultNamespace;
using Randomables;
using UnityEngine;
using UnityEngine.Serialization;

namespace Agent.SymmetryQuadPed
{
public class SymmetryQuadPedAgent : Unity.MLAgents.Agent
{
    private SymmetryQuadPedController _controller;

    public Arena arena;

    private void Start()
    {
        _controller = GetComponent<SymmetryQuadPedController>();
    }

    public override void OnEpisodeBegin()
    {
        arena.Rand();
    }
}
}
