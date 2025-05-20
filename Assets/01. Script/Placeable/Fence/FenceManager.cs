using UnityEngine;
using System.Collections.Generic;

public class FenceManager : MonoBehaviour
{
    public static FenceManager Instance;
    // ���߿� fence�� �Ǹ��ϴ� ����� ������  public List<Tile> occupiedTiles = new(); turretbase�����ؼ� �������ҵ�
    Fence selectedFence;
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

    public void SelectFence(Fence fence) => selectedFence = fence;
    public void ClearSelection() => selectedFence = null;

    public void OnClickSell()
    {
        if (selectedFence == null) return;
        selectedFence.Sell();
    }

}
