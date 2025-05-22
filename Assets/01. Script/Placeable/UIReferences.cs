using UnityEngine;
using UnityEngine.UI;
public interface IPlaceableUIBinder
{
    void BindUI(PlaceableBase placeable);
}




[System.Serializable]
public class UIReferences
{
    public GameObject selectPanel;
    public Text levelText;
    public Text damageText;
    public Text rangeText;
    public Text rateText;
    public Text upgradeText;
    public Text sellCostText;
    public Text description;
}
public class TurretUIBinder : IPlaceableUIBinder
{
    private readonly UIReferences ui;

    public TurretUIBinder(UIReferences ui) => this.ui = ui;

    public void BindUI(PlaceableBase placeable)
    {
        var turret = placeable as TurretBase;

        ui.levelText.text = $"Lv.{turret.CurrentLevel}";
        ui.damageText.text = $"Damage.{turret.GetDamage()}";
        ui.rangeText.text = $"Range.{turret.GetRange()}";
        ui.rateText.text = $"Rate.{turret.GetAttackRate()}";
        ui.upgradeText.text = $"Upgrade Price.{turret.GetUpgradeCost()}";
        ui.sellCostText.text = $"Sell Price.{turret.GetSellPrice()}";
        ui.description.text = $"Description.{turret.GetDescription()}";
    }
}



public class FenceUIBinder : IPlaceableUIBinder
{
    private readonly UIReferences ui;

    public FenceUIBinder(UIReferences ui) => this.ui = ui;

    public void BindUI(PlaceableBase placeable)
    {
        var fence = placeable as Fence;

        ui.levelText.text = "";
        ui.damageText.text = "";
        ui.rangeText.text = "";
        ui.rateText.text = "";
        ui.upgradeText.text = "";
        ui.sellCostText.text = $"Sell Price.{fence.GetSellPrice()}";
        ui.description.text = $"Description.{fence.GetDescription()}";
    }
}
