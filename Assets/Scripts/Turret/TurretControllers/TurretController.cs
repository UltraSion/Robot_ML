using System;
using Controller.AimControllers;
using Controller.CameraControllers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Turret.TurretControllers
{
public abstract class TurretController : MonoBehaviour
{
    public AimDirectionGetter aimDirectionGetter;
    public CameraDirectionGetter cameraDirectionGetter;

    public Turret turret;

    public void FixedUpdate()
    {

    }
}
}
