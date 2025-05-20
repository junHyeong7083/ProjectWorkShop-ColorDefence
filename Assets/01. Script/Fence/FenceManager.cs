using UnityEngine;
using System.Collections.Generic;

public class FenceManager : MonoBehaviour
{
    public static FenceManager Instance;
    // 나중에 fence를 판매하는 기능을 넣을때  public List<Tile> occupiedTiles = new(); turretbase참고해서 만들어야할듯

    // 아니면 아에 설치가능한 오브젝트 부모클래스하나 만들어도될듯
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
}
