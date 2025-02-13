using UnityEngine;

namespace DefaultNamespace
{
public static class Hub
{
    public static float Sensitivity = 0.5f;

    public static Camera MainCamera => Camera.main;

    public static float FOV => MainCamera.fieldOfView;
}
}
