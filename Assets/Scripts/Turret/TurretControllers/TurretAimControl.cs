using System;
using DefaultNamespace;
using Unity.VisualScripting;
using UnityEngine;

namespace Turret.TurretControllers
{
public class TurretAimControl : MonoBehaviour
{
    private Turret turret;
    private Camera _camera;
    private AbstractTurretCameraState _turretCameraState;

    private void Awake()
    {
        turret = GetComponent<Turret>();
        _camera = Hub.MainCamera;
        _turretCameraState = new TurretCameraStateNormal(_camera.transform.rotation.eulerAngles);
    }

    private void Update()
    {
        UpdateCameraRot();
        turret.TargetPoint = _turretCameraState.GetTurretAimPoint();

        _turretCameraState = _turretCameraState.UpdateState();
    }

    public void UpdateCameraRot()
    {
        var cameraRot = _camera.transform.localEulerAngles;
        var targetRot = _turretCameraState.GetCameraRot().eulerAngles;

        cameraRot.x = targetRot.x;
        cameraRot.y = targetRot.y;

        _camera.transform.localEulerAngles = cameraRot;
    }
}
}
