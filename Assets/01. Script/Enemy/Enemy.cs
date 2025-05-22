using UnityEngine;

// ������ ���鶧 �򰥷��� �ȳ������ ����� ��Ʈ����Ʈ
[RequireComponent(typeof(EnemyPathfinder))]
[RequireComponent (typeof(EnemyHealth))]
public class Enemy : MonoBehaviour
{
    public MonsterData Data;


    public GameObject OverlayPrefab;

    private void Start()
    {
        Vector3 pos = transform.position;

        // 3dȯ���̴ٺ��� �⺻�������ϸ� ĳ���Ͱ� ť�꿡 ������
        pos.y = TileGridManager.Instance.cubeSize; 

        // ��ġ ����
        transform.position = pos;
    }

    private void Update()
    {
        // �����ʿ�!!!
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
                if (tile != null && tile.ColorState != Data.InfectColor && !tile.IsOccupied) // ������ �� ���� ����x
                    tile.SetColor(Data.InfectColor);
            }
        }
    }
}
