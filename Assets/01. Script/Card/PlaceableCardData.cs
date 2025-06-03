using UnityEngine;

[CreateAssetMenu(menuName = "Game/PlaceableCardData")]
public class PlaceableCardData : CardData
{
    public GameObject prefabToSpawn;
    public ScriptableObject scriptable;

    private void OnEnable()
    {
        cardType = CardType.TURRET; // or Fence, ���߿� �ɼǿ� ���� ����
    }
}



