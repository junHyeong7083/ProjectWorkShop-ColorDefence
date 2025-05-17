using UnityEngine;
using UnityEngine.SubsystemsImplementation;

public class TurretManager : MonoBehaviour
{
    public static TurretManager Instance { get; private set; }

    TurretData currentData;
    GameObject currentPrefab;
    GameObject previewInstance;


    private void Awake()
    {
        Instance = this;
    }

    public void StartPlacement(TurretData data, GameObject prefab)
    {
        currentData = data;
        currentPrefab = prefab;

        if (previewInstance != null) Destroy(previewInstance);
        previewInstance = Instantiate(prefab);
        previewInstance.GetComponent<Collider>().enabled = false;
        previewInstance.GetComponent<Turret>().enabled = false;
    }

    void Update()
    {
        if (currentData == null || currentPrefab == null || previewInstance == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit))
        {
            if (previewInstance != null)  previewInstance.transform.position = hit.point;

            if (Input.GetMouseButtonDown(0))
            {
                PlaceTurret(hit.point);
            }
        }
    }

    void PlaceTurret(Vector3 pos)
    {
        Destroy(previewInstance);
        var turret = Instantiate(currentPrefab, pos, Quaternion.identity);
        turret.GetComponent<Turret>().SetTurretData(currentData);

        currentData = null;
        currentPrefab = null;
        previewInstance = null;
    }
}
