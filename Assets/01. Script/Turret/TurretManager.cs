using UnityEngine;
using UnityEngine.UI;

public class TurretManager : MonoBehaviour
{
    public static TurretManager Instance { get; private set; }

    TurretData currentData;
    GameObject currentPrefab;
    GameObject previewInstance;


    Turret selectTurret;
    [Header("UI")]
    public GameObject selectionPanel;
    public Text levelText, damageText, rangeText, rateText, upgradeCostText, sellCostText;
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

    // 터렛을 놓는 함수 
    void PlaceTurret(Vector3 pos)
    {
        int cost = currentData.placementCost;
        if (!GameManager.instance.SpendGold(cost))
        {
            Debug.Log("골드 부족!");
            // 여기에 카메라 쉐이크 함수 넣으면 ㄱㅊ을듯
            return;
        }

        Destroy(previewInstance);
        var turret = Instantiate(currentPrefab, pos, Quaternion.identity);
        turret.GetComponent<Turret>().SetTurretData(currentData);

        currentData = null;
        currentPrefab = null;
        previewInstance = null;

    }

    public void SelectTurret(Turret turret)
    {
        selectTurret = turret;
        selectionPanel.SetActive(true);

        levelText.text = $"Lv.{turret.CurrentLevel}";
        damageText.text = $"Damage: {turret.GetDamage()}";
        rangeText.text = $"Range: {turret.GetRange():F1}";
        rateText.text = $"Rate: {turret.GetAttackRate():F2}";
        upgradeCostText.text = $"Upgrade Cost: {turret.GetUpgradeCost()}";
    }
    public void HidePanel()
    {
        selectionPanel.SetActive(false);
        selectTurret = null;
    }
    public void OnClickUpgrade()
    {
        if(selectTurret != null)
        {
            selectTurret.Upgrade();
            SelectTurret(selectTurret);
        }
    }

    public void OnClickSell()
    {
        if(selectTurret != null) 
        {
            selectTurret.Sell();

            HidePanel();
        }
    }
}
