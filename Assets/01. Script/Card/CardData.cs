// CardData.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Game/CardData")]
public class CardData : ScriptableObject
{
    [Header("ī�� �⺻ ����")]
    public string cardName;            // ī�� �̸�
    public Sprite cardIcon;            // ī�� ������
    public int cost;                   // ī�� ��� ���

    [Header("ī�� ȿ��")]
    public GameObject prefabToSpawn;   // ��ȯ�� Prefab (�ͷ��̳� ��Ÿ�� ��)

    [Header("��ȯ�� �ʿ��� Data (TurretData, FenceData ��)")]
    public ScriptableObject scriptable; // TurretData �Ǵ� FenceData
}
