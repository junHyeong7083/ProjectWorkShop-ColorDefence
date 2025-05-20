using UnityEngine;

public class TestSpawn : MonoBehaviour
{

    [SerializeField]
    GameObject enemyPrefab;

    [SerializeField]
    int spawnCount;
    void Start()
    {
        for(int e = 0; e< spawnCount; e++)
        {
            Instantiate(enemyPrefab);
        }
        

    }

   
}
