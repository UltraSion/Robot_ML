using UnityEngine;

namespace Turret
{
public abstract class AbstractTurretCameraState
{
    protected Vector3 lookRot;
    protected Camera _camera = Camera.main;

    public AbstractTurretCameraState(Vector3 lookRot)
    {
        this.lookRot = lookRot;
    }

    public virtual Quaternion GetCameraRot()
    {
        var xDelta = Input.GetAxisRaw("Mouse X") * Hub.Sensitivity * Time.deltaTime;

        var yDelta = Input.GetAxisRaw("Mouse Y") * Hub.Sensitivity * Time.deltaTime;

        lookRot += new Vector3(-yDelta, xDelta, 0f) * 90f;
        lookRot.x = Mathf.Clamp(lookRot.x, -90f, 90f);

        return Quaternion.Euler(lookRot);
    }

    public virtual Vector3 GetTurretAimPoint()
        => _camera.transform.forward;

    public abstract AbstractTurretCameraState UpdateState();
}

public class TurretCameraStateNormal : AbstractTurretCameraState
{
    public TurretCameraStateNormal(Vector3 lookRot) : base(lookRot)
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    public override AbstractTurretCameraState UpdateState()
    {
        if (Input.GetKeyDown(KeyCode.LeftAlt))
            return new TurretCameraStateHeadOnly(lookRot);

        if (Input.GetMouseButtonDown(1))
            return new TurretCameraStateCrossHairOnly(lookRot);

        return this;
    }
}

public class TurretCameraStateHeadOnly : AbstractTurretCameraState
{
    private Vector3 _turretAimPoint;

    public TurretCameraStateHeadOnly(Vector3 lookRot) : base(lookRot)
    {
        Cursor.lockState = CursorLockMode.Locked;
        _turretAimPoint = _camera.transform.forward;
    }

    public override Vector3 GetTurretAimPoint()
        => _turretAimPoint;

    public override AbstractTurretCameraState UpdateState()
    {
        if (Input.GetKeyUp(KeyCode.LeftAlt))
            return new TurretCameraStateNormal(lookRot);

        if (Input.GetMouseButtonDown(1))
            return new TurretCameraStateCrossHairOnly(lookRot);

        return this;
    }
}

public class TurretCameraStateCrossHairOnly : AbstractTurretCameraState
{
    public TurretCameraStateCrossHairOnly(Vector3 lookRot) : base(lookRot)
    {
        Cursor.lockState = CursorLockMode.Confined;
    }

    public override Quaternion GetCameraRot()
        => Quaternion.Euler(lookRot);

    public override Vector3 GetTurretAimPoint()
    {
        Vector3 mousePos = Input.mousePosition;

        float width = Screen.width;
        float height = Screen.height;
        float pov = _camera.fieldOfView;

        var distance = height * 0.5f / Mathf.Tan(pov * 0.5f * Mathf.Deg2Rad);
        mousePos -= new Vector3(width, height, 0f) * 0.5f;

        var lookTarget = new Vector3(mousePos.x, mousePos.y, distance);
        lookTarget = _camera.transform.TransformDirection(lookTarget);

        return lookTarget;
    }

    public override AbstractTurretCameraState UpdateState()
    {
        if (Input.GetMouseButtonUp(1))
            return new TurretCameraStateNormal(lookRot);

        if (Input.GetKeyDown(KeyCode.LeftAlt))
            return new TurretCameraStateNormal(lookRot);

        return this;
    }
}
}
