using UnityEngine;
using UnityEngine.UI;

public class TurretManager : MonoBehaviour
{
    public static TurretManager Instance { get; private set; }

    TurretBase selectTurret;

    [Header("UI")]
    public GameObject selectionPanel;
    // 해당내용도 나중에 따로 빼서 만들면될듯
    public Text levelText, damageText, rangeText, rateText, upgradeCostText, sellCostText;

    private void Awake()
    {
        Instance = this;
    }

    // 터렛 설치 시작 -> PlacementManager에 위임
    public void StartPlacement(TurretData data, GameObject prefab)
    {
        PlacementManager.Instance.StartPlacement(data, prefab, PlacementType.Turret);
    }

    // 터렛 클릭시 정보 패널 열기
    public void SelectTurret(TurretBase turret)
    {
        selectTurret = turret;
        selectionPanel.SetActive(true);

        levelText.text = $"Lv.{turret.CurrentLevel}";
        damageText.text = $"Damage: {turret.GetDamage()}";
        rangeText.text = $"Range: {turret.GetRange():F1}";
        rateText.text = $"Rate: {turret.GetAttackRate():F2}";
        upgradeCostText.text = $"Upgrade Cost: {turret.GetUpgradeCost()}";
        sellCostText.text = $"Sell: {turret.GetSellPrice()}";
    }

    public void HidePanel()
    {
        selectionPanel.SetActive(false);
        selectTurret = null;
    }

    public void OnClickUpgrade()
    {
        if (selectTurret != null)
        {
            selectTurret.Upgrade();
            SelectTurret(selectTurret); // 갱신
        }
    }

    public void OnClickSell()
    {
        if (selectTurret != null)
        {
            selectTurret.Sell();
            HidePanel();
        }
    }
}
