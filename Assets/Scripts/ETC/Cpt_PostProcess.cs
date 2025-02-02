using UnityEngine;

namespace ETC
{
[RequireComponent(typeof(Camera))]
public class DepthRenderer : MonoBehaviour
{
    public Material depthMaterial;

    private void Start()
    {
        // Depth Texture 활성화
        Camera camera = GetComponent<Camera>();
        camera.depthTextureMode = DepthTextureMode.Depth;
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        // Depth Material을 사용하여 화면에 렌더링
        if (depthMaterial != null)
        {
            Graphics.Blit(src, dest, depthMaterial);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }
}
}
