// CardData.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Game/CardData")]
public class CardData : ScriptableObject
{
    [Header("카드 기본 정보")]
    public string cardName;            // 카드 이름
    public Sprite cardIcon;            // 카드 아이콘
    public int cost;                   // 카드 사용 비용

    [Header("카드 효과")]
    public GameObject prefabToSpawn;   // 소환할 Prefab (터렛이나 울타리 등)

    [Header("소환에 필요한 Data (TurretData, FenceData 등)")]
    public ScriptableObject scriptable; // TurretData 또는 FenceData
}
