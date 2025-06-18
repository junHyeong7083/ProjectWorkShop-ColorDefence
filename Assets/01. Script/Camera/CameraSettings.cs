using UnityEngine;

public class CameraSettings : MonoBehaviour
{
    [SerializeField] private Camera[] cameras;



    [Header("기준 오브젝트")]
    [SerializeField] private Transform queenAnt;


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
        if (queenAnt == null)
        {
            Debug.LogError("CameraSettings: queenant가 할당되지 않았습니다!");
            return;
        }

        foreach (var cam in cameras)
        {
            // 기준점과 거리/높이 설정
            Vector3 offset = new Vector3(-30f, 40f, -30f); // ← 상황에 맞게 조정
            cam.transform.position = queenAnt.position + offset;

            // 항상 여왕개미를 바라보게
            cam.transform.LookAt(queenAnt.position);

            cam.rect = new Rect(0f, 0.2f, 1f, 0.8f);
        }
    }
    

  

   
}


