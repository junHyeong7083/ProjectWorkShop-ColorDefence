using UnityEngine;
using UnityEngine.UI;

public class TurretManager : MonoBehaviour
{
    public static TurretManager Instance { get; private set; }

    TurretBase selectTurret;

    [Header("UI")]
    public GameObject selectionPanel;
    // �ش系�뵵 ���߿� ���� ���� �����ɵ�
    public Text levelText, damageText, rangeText, rateText, upgradeCostText, sellCostText;

    private void Awake()
    {
        Instance = this;
    }

    // �ͷ� ��ġ ���� -> PlacementManager�� ����
    public void StartPlacement(TurretData data, GameObject prefab)
    {
        PlacementManager.Instance.StartPlacement(data, prefab, PlacementType.Turret);
    }

    // �ͷ� Ŭ���� ���� �г� ����
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
            SelectTurret(selectTurret); // ����
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
