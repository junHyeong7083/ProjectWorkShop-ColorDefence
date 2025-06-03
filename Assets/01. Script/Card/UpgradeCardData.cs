using UnityEngine;

[CreateAssetMenu(menuName = "Game/UpgradeCardData")]
public class UpgradeCardData : CardData
{
    public TurretType targetTurretType;
    public int upgradeLevel;

    private void OnEnable()
    {
        cardType = CardType.UPGRADE;
    }
}


