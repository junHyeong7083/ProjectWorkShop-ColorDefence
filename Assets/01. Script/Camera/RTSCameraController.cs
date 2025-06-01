using FischlWorks_FogWar;
using UnityEngine;

public class RTSCameraController : MonoBehaviour
{
    [BigHeader("1) RTS 카메라 배열")]
    [SerializeField] private Camera[] cameras;

    [BigHeader("2) Mask용 Orthographic 카메라")]
    [SerializeField] private Camera maskCam;
    [Header("MaskQuad를 위한 머티리얼")]
    [SerializeField] private Material maskMaterial;

    // maskQuad 오브젝트를 런타임에 생성/제어용
    private GameObject maskQuad;

    // 이전 프레임 대비 마스크 카메라 설정 변화를 감지하기 위한 저장 변수
    private Vector3 prevMaskCamPos;
    private float prevMaskCamSize;

    // -- ComputeBuffer 관련 변수 --
    private ComputeBuffer cbCenters;
    private ComputeBuffer cbRadii;
    private Vector2[] revealCenters;
    private float[] revealRadii;

    void Start()
    {
        InitRTSCamera();
        CreateMaskQuad();
    }

    private void OnDestroy()
    {
        ReleaseBuffers();
    }

    void Update()
    {
        // A) MaskCamera가 움직이거나 Orthographic Size가 바뀌면 MaskQuad를 재조정
        if (maskCam != null && maskCam.orthographic)
        {
            if (maskCam.transform.position != prevMaskCamPos
                || Mathf.Abs(maskCam.orthographicSize - prevMaskCamSize) > Mathf.Epsilon)
            {
                AdjustMaskQuad();
                prevMaskCamPos = maskCam.transform.position;
                prevMaskCamSize = maskCam.orthographicSize;
            }
        }

        // B) csFogWar에 등록된 FogRevealer(터렛) 정보 기반으로
        //    ComputeBuffer에 원형 구멍 정보를 매 프레임 넘겨주기
        UpdateRevealBuffers();
    }

    // -----------------------------------------------------
    // 1. RTS 카메라 초기화 (기존 코드 동일)
    // -----------------------------------------------------
    private void InitRTSCamera()
    {
        foreach (var cam in cameras)
        {
            cam.transform.position = new Vector3(60f, 40f, 45f);
            cam.transform.rotation = Quaternion.Euler(45f, 45f, 0f);
            cam.rect = new Rect(0f, 0.3f, 1f, 0.7f);
        }
    }

    // -----------------------------------------------------
    // 2. ComputeBuffer 관련 메서드 (기존 코드 동일)
    // -----------------------------------------------------
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

    // --------------------------------------------------------
    // 3) 매 프레임마다 csFogWar.Instance._FogRevealers 정보를 읽어
    //    ComputeBuffer에 (u,v) 좌표 & 반경(uv 단위) 정보를 업데이트
    // --------------------------------------------------------
    private void UpdateRevealBuffers()
    {
        // csFogWar 인스턴스와 메인 카메라가 있어야 동작
        if (csFogWar.Instance == null || Camera.main == null || maskMaterial == null)
            return;

        var fogList = csFogWar.Instance._FogRevealers;
        int turretCount = fogList != null ? fogList.Count : 0;
        int dataCount = Mathf.Max(1, turretCount);

        // 배열 길이가 바뀌었으면 버퍼 다시 할당
        if (revealCenters == null || revealCenters.Length != dataCount)
        {
            AllocateBuffers(dataCount);
        }

        // 각 FogRevealer로부터 Transform & SightRange 가져오기
        for (int i = 0; i < dataCount; i++)
        {
            if (i >= turretCount || fogList[i] == null)
            {
                revealCenters[i] = new Vector2(-1f, -1f);
                revealRadii[i] = 0f;
            }
            else
            {
                var revealer = fogList[i];
                Transform tr = revealer._RevealerTransform;
                int sightUnits = revealer._SightRange;

                if (tr == null)
                {
                    revealCenters[i] = new Vector2(-1f, -1f);
                    revealRadii[i] = 0f;
                }
                else
                {
                    // 1) 월드 좌표 → 메인 카메라 Viewport 좌표 (0~1)
                    Vector3 vp = Camera.main.WorldToViewportPoint(tr.position);
                    float u = vp.x, v = vp.y;

                    // 뒤쪽이거나 화면 밖이면 무효화
                    if (u < 0f || u > 1f || v < 0f || v > 1f || vp.z < 0f)
                    {
                        revealCenters[i] = new Vector2(-1f, -1f);
                        revealRadii[i] = 0f;
                    }
                    else
                    {
                        revealCenters[i] = new Vector2(u, v);

                        // 2) 타일 단위 시야(sightUnits) → 월드 단위 반경
                        float worldRadius = sightUnits * csFogWar.Instance._UnitScale;

                        // 3) 월드 단위 반경 → Viewport(UV) 반경
                        float uvRadius;
                        if (Camera.main.orthographic)
                        {
                            uvRadius = worldRadius / (Camera.main.orthographicSize * 2f);
                        }
                        else
                        {
                            // Perspective 카메라일 때, “위로 worldRadius 이동 → Viewport 차이” 방식
                            Vector3 worldAbove = tr.position + Vector3.up * worldRadius;
                            Vector3 vpAbove = Camera.main.WorldToViewportPoint(worldAbove);
                            uvRadius = Mathf.Abs(vpAbove.y - v);
                        }
                        revealRadii[i] = uvRadius;
                    }
                }
            }
        }

        // ComputeBuffer에 데이터 업로드
        cbCenters.SetData(revealCenters);
        cbRadii.SetData(revealRadii);

        // 셰이더에 Count와 버퍼 연결
        maskMaterial.SetInt("_RevealCount", turretCount);
        maskMaterial.SetBuffer("_SB_RevealCenters", cbCenters);
        maskMaterial.SetBuffer("_SB_RevealRadii", cbRadii);
    }

    // --------------------------------------------------------
    // 4) MaskQuad 생성 메서드 (여기서 수정)
    // --------------------------------------------------------
    private void CreateMaskQuad()
    {
        // 1) Quad 생성
        maskQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        maskQuad.name = "[RUNTIME] FogMaskQuad";

        // 2) 'FogMask' 레이어가 있는지 확인 후 설정
        int maskLayer = LayerMask.NameToLayer("FogMask");
        if (maskLayer < 0)
        {
            Debug.LogError("RTSCameraController: 'FogMask' 레이어가 없습니다! Edit → Tags and Layers에서 'FogMask'를 추가하세요.");
            return;
        }
        maskQuad.layer = maskLayer;

        // 3) Quad를 FogMaskCam의 자식으로 만들어 로컬 좌표계를 곧바로 쓰도록 설정
        maskQuad.transform.SetParent(maskCam.transform, worldPositionStays: false);

        // 4) 로컬 회전을 (0,0,0)으로 두면 Quad는 FogMaskCam의 회전(기울기)에 딱 평행하게 된다.
        maskQuad.transform.localRotation = Quaternion.identity;

        // 5) FogMaskCam이 Orthographic 모드라면, Quad가 화면 전체를 덮도록 크기(스케일)와 위치를 잡는다.
        if (maskCam != null && maskCam.orthographic)
        {
            // (a) 카메라 로컬 공간: 절반 높이(orthoH), 절반 너비(orthoW) 계산
            float orthoH = maskCam.orthographicSize;           // 카메라 절반 높이(월드 단위)
            float orthoW = orthoH * maskCam.aspect;            // 카메라 절반 너비(월드 단위)

            // (b) Quad 크기를 화면 전체(가로=2×orthoW, 세로=2×orthoH)로 맞춘다.
            maskQuad.transform.localScale = new Vector3(2f * orthoW, 2f * orthoH, 1f);

            // (c) Quad 로컬 위치를 “카메라 앞쪽 (로컬 −Z 방향, nearPlane보다 살짝 앞)”으로 설정
            //     => 카메라는 로컬 −Z 축을 정면으로 보기 때문에, Quad도 반드시 Z를 음수로 줘야 카메라 앞에 온다.
            float zPos = -(maskCam.nearClipPlane - 0.01f);
            maskQuad.transform.localPosition = new Vector3(0f, 0f, zPos);
        }
        else
        {
            // Orthographic이 아니면(권장되지 않음) 그냥 크게 만들어도 동작은 하나,
            // 화면에 보이도록 카메라 앞쪽(로컬 −Z)으로 살짝 배치해야 함.
            maskQuad.transform.localScale = new Vector3(100f, 100f, 1f);
            maskQuad.transform.localPosition = new Vector3(0f, 0f, -1f);
        }

        // 6) MaskQuad에 원형 구멍 셰이더가 담긴 머티리얼 할당
        if (maskMaterial != null)
        {
            MeshRenderer mr = maskQuad.GetComponent<MeshRenderer>();
            mr.material = maskMaterial;
        }
        else
        {
            Debug.LogError("RTSCameraController: maskMaterial이 할당되지 않았습니다!");
        }

        // 7) Collider는 필요 없으므로 비활성화
        MeshCollider col = maskQuad.GetComponent<MeshCollider>();
        if (col != null)
            col.enabled = false;
    }

    // --------------------------------------------------------
    // 5) MaskQuad를 마스크 카메라에 맞춰 재조정하는 메서드 (기존 코드와 동일)
    // --------------------------------------------------------
    private void AdjustMaskQuad()
    {
        if (maskQuad == null || maskCam == null || !maskCam.orthographic)
            return;

        float orthoH = maskCam.orthographicSize;
        float orthoW = orthoH * maskCam.aspect;

        maskQuad.transform.localScale = new Vector3(2f * orthoW, 2f * orthoH, 1f);

        // 위치도 nearClipPlane 쪽(−Z)으로 다시 맞춰 준다.
        float zPos = -(maskCam.nearClipPlane + 0.01f);
        maskQuad.transform.localPosition = new Vector3(0f, 0f, zPos);
    }
}
