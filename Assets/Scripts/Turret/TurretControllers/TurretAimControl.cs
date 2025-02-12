using System;
using DefaultNamespace;
using Unity.VisualScripting;
using UnityEngine;

namespace Turret.TurretControllers
{
public class TurretAimControl : MonoBehaviour
{
    private Turret turret;
    private AbstractTurretCameraState _turretCameraStateState;
    private Camera _camera;

    private void Awake()
    {
        turret = GetComponent<Turret>();
        _camera = Hub.MainCamera;
        _turretCameraStateState = new TurretCameraStateNormal(_camera.transform.rotation.eulerAngles);
    }

    private void Update()
    {
        _camera.transform.rotation = _turretCameraStateState.GetCameraRot();
        turret.TargetPoint = _turretCameraStateState.GetTurretAimPoint();

        _turretCameraStateState = _turretCameraStateState.UpdateState();
    }
}
}
