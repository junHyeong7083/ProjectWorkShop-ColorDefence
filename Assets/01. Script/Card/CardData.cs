// CardData.cs
using UnityEngine;

public enum CardType
{
    TURRET,
    FENCE,
    UPGRADE,
    SELL,
}





[CreateAssetMenu(menuName = "Game/CardData")]
public class CardData : ScriptableObject
{
    [Header("카드 기본 정보")]
    public string cardName;            // 카드 이름
    public Sprite cardIcon;            // 카드 아이콘
    public int cost;                   // 카드 사용 비용

    [Header("카드 타입")]
    public CardType cardType;

    public GameObject prefabToSpawn;
    public ScriptableObject scriptable;

}
