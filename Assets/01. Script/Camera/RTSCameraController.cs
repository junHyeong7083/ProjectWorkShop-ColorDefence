using UnityEngine;

public class RTSCameraController : MonoBehaviour
{
    [SerializeField] private Camera[] cameras;

    [SerializeField] private float moveSpeed = 10f;

    [Header("Z Movement & X Rotation")]
    [SerializeField] private float minZ = -6.5f;
    [SerializeField] private float maxZ = 90f;
    [SerializeField] private float minXRotZ = 45f;
    [SerializeField] private float maxXRotZ = 60f;

    [Header("X Movement & X Rotation")]
    [SerializeField] private float minX = 30f;
    [SerializeField] private float maxX = 95f;
    [SerializeField] private float minXRotX = 45f;
    [SerializeField] private float maxXRotX = 70f;

    void Start()
    {
        foreach (var cam in cameras)
        {
            cam.orthographic = true;
            cam.orthographicSize = 40;

            cam.transform.position = new Vector3(60f, 40f, 45f);
            float initXRotZ = Mathf.Lerp(minXRotZ, maxXRotZ, Mathf.InverseLerp(minZ, maxZ, 45f));
            float initXRotX = Mathf.Lerp(minXRotX, maxXRotX, Mathf.InverseLerp(minX, maxX, 60f));
            float initXRot = (initXRotZ + initXRotX) * 0.5f;
            cam.transform.rotation = Quaternion.Euler(initXRot, 0f, 0f);
            cam.rect = new Rect(0f, 0.3f, 1f, 0.7f);
        }
    }

    /*void Update()
    {
        float zInput = Input.GetAxis("Vertical");
        float xInput = Input.GetAxis("Horizontal");

        foreach (var cam in cameras)
        {
            Vector3 pos = cam.transform.position;

            // 이동
            pos.z += zInput * moveSpeed * Time.deltaTime;
            pos.z = Mathf.Clamp(pos.z, minZ, maxZ);

            pos.x += xInput * moveSpeed * Time.deltaTime;
            pos.x = Mathf.Clamp(pos.x, minX, maxX);

            cam.transform.position = pos;

            // 각각 기준으로 x 회전값 계산
            float tZ = Mathf.InverseLerp(minZ, maxZ, pos.z);
            float tX = Mathf.InverseLerp(minX, maxX, pos.x);

            float rotFromZ = Mathf.Lerp(minXRotZ, maxXRotZ, tZ);
            float rotFromX = Mathf.Lerp(minXRotX, maxXRotX, tX);

            float finalXRot = (rotFromZ + rotFromX) * 0.5f; // 평균으로 섞기

            cam.transform.rotation = Quaternion.Euler(finalXRot, 45f, 0f);
        }
    }*/
}
