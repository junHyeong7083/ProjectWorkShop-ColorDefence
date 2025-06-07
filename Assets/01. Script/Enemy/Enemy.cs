using UnityEngine;

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



}
