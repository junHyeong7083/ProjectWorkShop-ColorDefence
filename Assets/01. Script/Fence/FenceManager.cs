using UnityEngine;
using System.Collections.Generic;

public class FenceManager : MonoBehaviour
{
    public static FenceManager Instance;
    // ���߿� fence�� �Ǹ��ϴ� ����� ������  public List<Tile> occupiedTiles = new(); turretbase�����ؼ� �������ҵ�

    // �ƴϸ� �ƿ� ��ġ������ ������Ʈ �θ�Ŭ�����ϳ� �����ɵ�
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ��ġ ���� ��û�� �ѱ�
    public void StartPlacement(FenceData data, GameObject prefab)
    {
        PlacementManager.Instance.StartPlacement(data, prefab, PlacementType.Fence);
    }
}
