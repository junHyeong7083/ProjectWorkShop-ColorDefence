using UnityEngine;

public class MinimapAutoSetup : MonoBehaviour
{
    [SerializeField] private Camera minimapCam;
    [SerializeField] private Terrain terrain;

    [SerializeField] private float heightOffset = 50f; // 위에서 바라보는 높이


    private void Start()
    {
        if (minimapCam == null || terrain == null)
        {
            Debug.LogError("MinimapCamera 또는 Terrain이 비어 있습니다.");
            return;
        }

        Bounds terrainBounds = terrain.terrainData.bounds;
        Vector3 terrainSize = terrain.terrainData.size;

        // Terrain 중심 계산
        Vector3 center = terrain.transform.position + new Vector3(terrainSize.x / 2f, 0f, terrainSize.z / 2f);

        // 카메라 위치 = Terrain 중심 위쪽
        minimapCam.transform.position = center + Vector3.up * heightOffset;
        minimapCam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        // 정사영 크기 설정 (세로 방향 기준)
        minimapCam.orthographic = true;
        minimapCam.orthographicSize = terrainSize.z / 2f;

        // (선택) 클리어 컬러 설정
        minimapCam.clearFlags = CameraClearFlags.SolidColor;
        minimapCam.backgroundColor = Color.black;
    }
}
