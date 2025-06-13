using UnityEngine;

public class MinimapAutoSetup : MonoBehaviour
{
    [SerializeField] private Camera minimapCam;
    [SerializeField] private Transform mapRoot;  // Plane 오브젝트
    [SerializeField] private float heightOffset = 50f;

    private void Start()
    {
        if (minimapCam == null || mapRoot == null)
            return;

        Bounds bounds = CalculateTotalBounds(mapRoot);
        Vector3 center = bounds.center;
        Vector3 size = bounds.size;

        minimapCam.transform.position = new Vector3(center.x, center.y + heightOffset, center.z);
        minimapCam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        minimapCam.orthographic = true;
        minimapCam.orthographicSize = size.z / 2f;

        minimapCam.clearFlags = CameraClearFlags.SolidColor;
        minimapCam.backgroundColor = Color.black;
    }

    private Bounds CalculateTotalBounds(Transform root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
            return new Bounds(root.position, Vector3.zero);

        Bounds totalBounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            totalBounds.Encapsulate(renderers[i].bounds);
        }

        return totalBounds;
    }
}
