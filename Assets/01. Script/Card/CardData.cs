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
    [Header("ī�� �⺻ ����")]
    public string cardName;            // ī�� �̸�
    public Sprite cardIcon;            // ī�� ������
    public int cost;                   // ī�� ��� ���

    [Header("ī�� Ÿ��")]
    public CardType cardType;

    public GameObject prefabToSpawn;
    public ScriptableObject scriptable;

}
