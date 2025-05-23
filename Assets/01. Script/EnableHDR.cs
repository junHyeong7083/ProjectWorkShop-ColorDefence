using UnityEngine;
using UnityEngine.Rendering.Universal;

public class EnableHDR : MonoBehaviour
{
    void Awake()
    {
        var camera = GetComponent<Camera>();
        var data = camera.GetUniversalAdditionalCameraData();
        data.allowHDROutput = true;
    }
}
