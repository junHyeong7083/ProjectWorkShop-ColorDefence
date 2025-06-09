using UnityEngine;

public class MinimapAutoSetup : MonoBehaviour
{
    [SerializeField] private Camera minimapCam;
    [SerializeField] private Terrain terrain;

    [SerializeField] private float heightOffset = 50f; // ������ �ٶ󺸴� ����


    private void Start()
    {
        if (minimapCam == null || terrain == null)
        {
            Debug.LogError("MinimapCamera �Ǵ� Terrain�� ��� �ֽ��ϴ�.");
            return;
        }

        Bounds terrainBounds = terrain.terrainData.bounds;
        Vector3 terrainSize = terrain.terrainData.size;

        // Terrain �߽� ���
        Vector3 center = terrain.transform.position + new Vector3(terrainSize.x / 2f, 0f, terrainSize.z / 2f);

        // ī�޶� ��ġ = Terrain �߽� ����
        minimapCam.transform.position = center + Vector3.up * heightOffset;
        minimapCam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        // ���翵 ũ�� ���� (���� ���� ����)
        minimapCam.orthographic = true;
        minimapCam.orthographicSize = terrainSize.z / 2f;

        // (����) Ŭ���� �÷� ����
        minimapCam.clearFlags = CameraClearFlags.SolidColor;
        minimapCam.backgroundColor = Color.black;
    }
}
