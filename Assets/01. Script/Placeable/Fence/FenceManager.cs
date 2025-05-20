using UnityEngine;
using System.Collections.Generic;

public class FenceManager : MonoBehaviour
{
    public static FenceManager Instance;
    // 나중에 fence를 판매하는 기능을 넣을때  public List<Tile> occupiedTiles = new(); turretbase참고해서 만들어야할듯
    Fence selectedFence;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 배치 시작 요청만 넘김
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
