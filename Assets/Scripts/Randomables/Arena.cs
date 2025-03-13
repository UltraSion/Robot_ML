using System;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

namespace Randomables
{
public class Arena : RandomObject
{
    public TargetObject TargetObject;
    public RandomGroundMesh RandomGroundMesh;

    private List<RandomObject> randomObjects = new();

    public override void Rand()
        => randomObjects.ForEach(r => r.Rand());

    private void Awake()
    {
        randomObjects.Add(TargetObject);
        randomObjects.Add(RandomGroundMesh);
    }

    private void Start()
    {
        Rand();
    }
}
}
