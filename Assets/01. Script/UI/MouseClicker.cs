using UnityEngine;

public class MouseClicker : MonoBehaviour
{
    private MeshRenderer mesh;
    private Material mt;
    [SerializeField] float duration = 1f;

    private void Awake()
    {
        mesh = GetComponent<MeshRenderer>();
        mt = mesh.material;
    }

    private float timer = 0f;
    void Update()
    {
        if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                Vector3 spawnPos = new Vector3(hit.point.x, 1, hit.point.z);
                transform.position = spawnPos; // ✅ 이 줄이 중요!
            }
            timer = 0f;
        }

        timer += Time.deltaTime;

        float radius = Mathf.Lerp(0, 1f, timer / duration);
        mt.SetFloat("_Radius", radius);
    }

}
