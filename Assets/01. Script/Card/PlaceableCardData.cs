using UnityEngine;

[CreateAssetMenu(menuName = "Game/PlaceableCardData")]
public class PlaceableCardData : CardData
{
    public GameObject prefabToSpawn;
    public ScriptableObject scriptable;

    private void OnEnable()
    {
        cardType = CardType.TURRET; // or Fence, 나중에 옵션에 따라 지정
    }
}



