using UnityEngine;
using UnityEngine.UI;
public class PlaceableUIManager : MonoBehaviour
{
    public static PlaceableUIManager Instance;

    PlaceableBase selectedPlaceable;

    [Header("UI")]
    public GameObject selectPanel;
    public Text levelText, damageText, rangeText, rateText, upgradeText, sellCostText, description;

    PlaceableBase selectPlaceable;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this.gameObject);
    }

    // 어떤 오브젝트를 선택했는지
    public void Select(PlaceableBase placeableBase)
    {
        selectPlaceable = placeableBase;
        if (selectPanel != null)
            selectPanel.SetActive(true);

        selectPlaceable.gameObject.SetActive(true);

        if(placeableBase is TurretBase turret)
        {
            TurretManager.Instance.SelectTurret(turret);

            levelText.text = $"Lv.{turret.CurrentLevel}";
            damageText.text = $"Damage.{turret.GetDamage()}";
            rangeText.text = $"Range.{turret.GetRange()}";
            rateText.text = $"Rate.{turret.GetAttackRate()}";
            upgradeText.text = $"Upgrade Price.{turret.GetUpgradeCost()}";
            sellCostText.text = $"Sell Price.{turret.GetSellPrice()}";
            description.text = $"Description.{turret.GetDescription()}";
        }
        else if(placeableBase is Fence fence)
        {
            FenceManager.Instance.SelectFence(fence);

            levelText.text = "";
            damageText.text ="";
            rangeText.text = "";
            rateText.text = "";
            upgradeText.text = "";
            sellCostText.text =$"Sell Price.{fence.GetSellPrice()}";
            description.text = $"Description.{fence.GetDescription()}";
        }

        
    }

    public void OnClickUpgrade()
    {
        switch (selectPlaceable)
        {
            case TurretBase turret:
                TurretManager.Instance.OnClickUpgrade();
                break;

                case Fence fence:
                break;
        }

        // upgrade후 ui 갱신
        Select(selectPlaceable);
    }

    public void OnClickSell()
    {
        switch (selectedPlaceable)
        {
            case TurretBase:
                TurretManager.Instance.OnClickSell();
                break;
            case Fence:
                FenceManager.Instance.OnClickSell();
                break;
        }

        Hide();
    }

    public void Hide()
    {
        selectPanel.SetActive(false);
        selectedPlaceable = null;

        TurretManager.Instance.ClearSelection();
        FenceManager.Instance.ClearSelection(); 
    }


}
