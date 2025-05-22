using UnityEngine;

// 프리팹 만들때 헷갈려서 안넣을까봐 보험용 어트리뷰트
[RequireComponent(typeof(EnemyPathfinder))]
[RequireComponent (typeof(EnemyHealth))]
public class Enemy : MonoBehaviour
{
    public MonsterData Data;


    public GameObject OverlayPrefab;

    private void Start()
    {
        Vector3 pos = transform.position;

        // 3d환경이다보니 기본값으로하면 캐릭터가 큐브에 가려짐
        pos.y = TileGridManager.Instance.cubeSize; 

        // 위치 갱신
        transform.position = pos;
    }

    private void Update()
    {
        // 수정필요!!!
        InfectTile();
    }

    // TileState : Neutral -> Enemy || Player -> Enemy
    public void InfectTile()
    {
        // Debug.Log("InfectTile!!!!!!!!!!!!");

        Vector3 pos = this.transform.position;
        int centerX = Mathf.FloorToInt(pos.x / TileGridManager.Instance.cubeSize);
        int centerZ = Mathf.FloorToInt(pos.z / TileGridManager.Instance.cubeSize);

        int halfWidth = Data.Width / 2;
        int halfHeight = Data.Height / 2;

        for (int dx = -halfWidth; dx <= halfWidth; dx++)
        {
            for (int dz = -halfHeight; dz <= halfHeight; dz++)
            {
                int x = centerX + dx;
                int z = centerZ + dz;

                var tile = TileGridManager.Instance.GetTile(x, z);
                if (tile != null && tile.ColorState != Data.InfectColor && !tile.IsOccupied) // 점유가 된 블럭은 감염x
                    tile.SetColor(Data.InfectColor);
            }
        }
    }
}
