using System;
using UnityEngine;
using UnityEngine.Serialization;
using Weapons;

namespace Turret
{
public class Turret : MonoBehaviour
{
    public Vector3 TargetPoint;
    public ObjectShooter ObjectShooter;

    protected virtual void RotateTurret()
    {
        throw new System.NotImplementedException();
    }

    void FixedUpdate()
    {
        RotateTurret();
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
            ObjectShooter.Shoot();
    }
}
}
