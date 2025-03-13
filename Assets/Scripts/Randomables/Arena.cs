using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace DefaultNamespace
{
public class Arena : RandomObject
{
    public List<RandomObject> randomObjects;

    public override void Rand()
        => randomObjects.ForEach(r => r.Rand());

    private void Start()
    {
        Rand();
    }
}
}
