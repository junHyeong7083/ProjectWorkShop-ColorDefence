using FischlWorks_FogWar;
using UnityEngine;

public class CameraSettings : MonoBehaviour
{
    [BigHeader("1) RTS 카메라 배열")]
    [SerializeField] private Camera[] cameras;

    [BigHeader("2) Mask용 Orthographic 카메라")]
    [SerializeField] private Camera maskCam;

    [Header("MaskQuad를 위한 머티리얼")]
    [SerializeField] private Material maskMaterial;

    private GameObject maskQuad;
    private Vector3 prevMaskCamPos;
    private float prevMaskCamSize;

    private ComputeBuffer cbCenters;
    private ComputeBuffer cbRadii;
    private Vector2[] revealCenters;
    private float[] revealRadii;

    void Start()
    {
        InitRTSCamera();
        CreateMaskQuad();
        maskMaterial.SetFloat("_FadeRange", 0.05f);
    }

    private void OnDestroy()
    {
        ReleaseBuffers();
    }

    void Update()
    {
        /* if (maskCam != null && maskCam.orthographic)
         {
             if (maskCam.transform.position != prevMaskCamPos
                 || Mathf.Abs(maskCam.orthographicSize - prevMaskCamSize) > Mathf.Epsilon)
             {
                 AdjustMaskQuad();
                 prevMaskCamPos = maskCam.transform.position;
                 prevMaskCamSize = maskCam.orthographicSize;
             }
         }
         SyncMaskCamToMainCamera();
         UpdateRevealBuffers();*/
    }

    private void UpdateRevealBuffers()
    {
        if (csFogWar.Instance == null || maskCam == null || maskMaterial == null)
            return;

        var fogList = csFogWar.Instance._FogRevealers;
        int turretCount = fogList != null ? fogList.Count : 0;
        int dataCount = Mathf.Max(1, turretCount);

        if (revealCenters == null || revealCenters.Length != dataCount)
            AllocateBuffers(dataCount);

        for (int i = 0; i < dataCount; i++)
        {
            if (i >= turretCount || fogList[i] == null)
            {
                revealCenters[i] = new Vector2(-1f, -1f);
                revealRadii[i] = 0f;
                continue;
            }

            var revealer = fogList[i];

            // ✅ 수정된 중심 위치 로직
            Transform tr = revealer._RevealerTransform;
            if (tr == null) tr = revealer._RevealerTransform; // fallback
            Vector3 centerPos = tr.position + Vector3.up * 0.5f; // optional height offset

            int sightUnits = revealer._SightRange;

            // 1) Viewport 기준 좌표
            Vector3 mainVpCenter = Camera.main.WorldToViewportPoint(centerPos);
            float mu = mainVpCenter.x, mv = mainVpCenter.y;

            if (mainVpCenter.z < 0f || mu < 0f || mu > 1f || mv < 0f || mv > 1f)
            {
                revealCenters[i] = new Vector2(-1f, -1f);
                revealRadii[i] = 0f;
                continue;
            }

            float worldRadius = sightUnits * 0.3f * csFogWar.Instance._UnitScale;

            float mainUvRadiusUnclamped;
            if (Camera.main.orthographic)
            {
                mainUvRadiusUnclamped = worldRadius / (Camera.main.orthographicSize * 2f);
            }
            else
            {
                Vector3 worldRight_Main = centerPos + Camera.main.transform.right * worldRadius;
                Vector3 vpRight_Main = Camera.main.WorldToViewportPoint(worldRight_Main);
                if (vpRight_Main.z < 0f)
                    mainUvRadiusUnclamped = 0f;
                else
                    mainUvRadiusUnclamped = Mathf.Abs(vpRight_Main.x - mu);
            }

            float mainUvRadius = Mathf.Min(mainUvRadiusUnclamped, 1f);

            // 3-1) Viewport → World → Ground 교차점
            Ray rayCenter = Camera.main.ViewportPointToRay(new Vector3(mu, mv, 0f));
            RaycastHit hitCenter;
            Vector3 worldCenter = Physics.Raycast(rayCenter, out hitCenter, 1000f, LayerMask.GetMask("Ground"))
                ? hitCenter.point : centerPos;

            float muRight = Mathf.Clamp01(mu + mainUvRadius);
            Ray rayRight = Camera.main.ViewportPointToRay(new Vector3(muRight, mv, 0f));
            RaycastHit hitRight;
            Vector3 worldRightOnGround = Physics.Raycast(rayRight, out hitRight, 1000f, LayerMask.GetMask("Ground"))
                ? hitRight.point : centerPos + Camera.main.transform.right * worldRadius;

            Vector2 maskUvCenter = maskCam.WorldToViewportPoint(worldCenter);
            Vector2 maskUvRight = maskCam.WorldToViewportPoint(worldRightOnGround);

            float maskUvRadius = (maskUvCenter.y < 0f || maskUvRight.y < 0f)
                ? 0f
                : Vector2.Distance(maskUvCenter, maskUvRight);

            maskUvRadius = Mathf.Min(maskUvRadius, 1f);

            revealCenters[i] = new Vector2(maskUvCenter.x, maskUvCenter.y);
            revealRadii[i] = maskUvRadius;

            Debug.Log($"[RevealDebug] i={i}, radius={maskUvRadius:F3}, center={maskUvCenter}");
        }

        cbCenters.SetData(revealCenters);
        cbRadii.SetData(revealRadii);

        maskMaterial.SetInt("_RevealCount", turretCount);
        maskMaterial.SetBuffer("_SB_RevealCenters", cbCenters);
        maskMaterial.SetBuffer("_SB_RevealRadii", cbRadii);
    }


    private void AllocateBuffers(int cnt)
    {
        ReleaseBuffers();
        revealCenters = new Vector2[cnt];
        revealRadii = new float[cnt];
        cbCenters = new ComputeBuffer(cnt, sizeof(float) * 2);
        cbRadii = new ComputeBuffer(cnt, sizeof(float) * 1);
    }

    private void ReleaseBuffers()
    {
        if (cbCenters != null)
        {
            cbCenters.Release();
            cbCenters = null;
        }
        if (cbRadii != null)
        {
            cbRadii.Release();
            cbRadii = null;
        }
        revealCenters = null;
        revealRadii = null;
    }

    private void InitRTSCamera()
    {
        foreach (var cam in cameras)
        {
            cam.transform.position = new Vector3(60f, 40f, 45f);
            cam.transform.rotation = Quaternion.Euler(45f, 45f, 0f);
            cam.rect = new Rect(0f, 0.3f, 1f, 0.7f);
        }
    }
    private void SyncMaskCamToMainCamera()
    {
        if (Camera.main == null || maskCam == null) return;

        // 메인 카메라가 바라보는 중심점 Raycast
        Vector3 mainCamCenterRayPos = new Vector3(0.5f, 0.5f, 0f);
        Ray centerRay = Camera.main.ViewportPointToRay(mainCamCenterRayPos);
        RaycastHit hit;
        Vector3 worldCenter;
        if (Physics.Raycast(centerRay, out hit, 1000f, LayerMask.GetMask("Ground")))
            worldCenter = hit.point;
        else
            worldCenter = Camera.main.transform.position + Camera.main.transform.forward * 50f;

        // maskCam의 위치를 이 지점의 정면 위쪽으로 고정
        maskCam.transform.position = worldCenter + Vector3.up * 50f; // 충분한 높이(예: 50)에서 아래를 내려다보게
        maskCam.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // 정확히 아래쪽 방향(위→아래)

        // maskCam의 orthographicSize를 메인 카메라가 보는 Frustum의 높이에 정확히 맞추기
        Plane groundPlane = new Plane(Vector3.up, worldCenter);
        Ray topRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 1f, 0f));
        Ray bottomRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0f, 0f));

        float enter;
        Vector3 topPoint = groundPlane.Raycast(topRay, out enter) ? topRay.GetPoint(enter) : worldCenter;
        Vector3 bottomPoint = groundPlane.Raycast(bottomRay, out enter) ? bottomRay.GetPoint(enter) : worldCenter;

        float cameraViewHeight = Vector3.Distance(topPoint, bottomPoint);

        maskCam.orthographicSize = cameraViewHeight / 2f;

        // Quad를 maskCam에 정확히 맞추기
        AdjustMaskQuad();
    }

    private void CreateMaskQuad()
    {
        maskQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        maskQuad.name = "[RUNTIME] FogMaskQuad";

        int maskLayer = LayerMask.NameToLayer("FogMask");
        if (maskLayer < 0)
        {
            Debug.LogError("RTSCameraController: 'FogMask' 레이어가 없습니다! Edit → Tags and Layers에서 'FogMask'를 추가하세요.");
            return;
        }
        maskQuad.layer = maskLayer;

        maskQuad.transform.SetParent(maskCam.transform, worldPositionStays: false);
        maskQuad.transform.localRotation = Quaternion.identity;

        if (maskCam != null && maskCam.orthographic)
        {
            float orthoH = maskCam.orthographicSize;
            float orthoW = orthoH * maskCam.aspect;
            maskQuad.transform.localScale = new Vector3(2f * orthoW, 2f * orthoH, 1f);
            float zPos = -(maskCam.nearClipPlane - 0.01f);
            maskQuad.transform.localPosition = new Vector3(0f, 0f, zPos);
        }
        else
        {
            maskQuad.transform.localScale = new Vector3(100f, 100f, 1f);
            maskQuad.transform.localPosition = new Vector3(0f, 0f, -1f);
        }

        if (maskMaterial != null)
        {
            MeshRenderer mr = maskQuad.GetComponent<MeshRenderer>();
            mr.material = maskMaterial;
        }
        else
        {
            Debug.LogError("RTSCameraController: maskMaterial이 할당되지 않았습니다!");
        }

        MeshCollider col = maskQuad.GetComponent<MeshCollider>();
        if (col != null) col.enabled = false;
    }

    private void AdjustMaskQuad()
    {
        if (maskQuad == null || maskCam == null || !maskCam.orthographic)
            return;

        float orthoH = maskCam.orthographicSize;
        float orthoW = orthoH * maskCam.aspect;
        maskQuad.transform.localScale = new Vector3(2f * orthoW, 2f * orthoH, 1f);
        float zPos = -(maskCam.nearClipPlane - 1f);
        maskQuad.transform.localPosition = new Vector3(0f, 0f, zPos);
    }
}


