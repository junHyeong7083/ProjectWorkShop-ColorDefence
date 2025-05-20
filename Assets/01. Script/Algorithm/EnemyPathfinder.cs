using System.Collections.Generic;
using UnityEngine;

public class EnemyPathfinder : MonoBehaviour
{
    Tile currentTile;
    Queue<Tile> path = new();
    [SerializeField] MonsterData data;
    float speed;

    void Start()
    {
        speed = data.Speed;
        currentTile = TileGridManager.Instance.GetTile(
            Mathf.FloorToInt(transform.position.x / TileGridManager.Instance.cubeSize),
            Mathf.FloorToInt(transform.position.z / TileGridManager.Instance.cubeSize)
        );
        Debug.Log($"[Start] {gameObject.name} currentTile = {currentTile}");
        RecalculatePath();
    }

    public void RecalculatePath()
    {
        Tile goalTile = TileGridManager.Instance.GetTile(0, 0);
        var newPath = Pathfinding.FindPath(currentTile, goalTile);
        if (newPath != null) path = new Queue<Tile>(newPath);
    }

    void Update()
    {
        if (path.Count > 0)
        {
            Tile next = path.Peek();
            Vector3 target = next.CenterWorldPos;
            target.y = transform.position.y;
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target) < 0.05f)
            {
                currentTile = next;
                path.Dequeue();
            }
        }
    }
}
