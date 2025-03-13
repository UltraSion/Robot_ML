using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

namespace Randomables
{
public class Arena : RandomObject
{
    [SerializeField] private List<RandomObject> randomObjects;

    public override void Rand()
        => randomObjects.ForEach(r => r.Rand());

    private void Start()
    {
        Rand();
    }
}
}
