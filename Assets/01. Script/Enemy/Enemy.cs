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

        // 3d환경이다보니 기본값으로하면 캐릭터가 큐브에 가려짐
        pos.y = TileGridManager.Instance.cubeSize; 

        // 위치 갱신
        transform.position = pos;
    }



}
