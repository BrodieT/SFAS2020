using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script will handle the spawning of enemies around the given map
public class EnemySpawner : MonoBehaviour
{
    //Store the given map and size of the grid
    public Vector2Int Size { get; set; }
    List<List<EnvironmentTile>> map;

    public List<EnvironmentTile> roomT = new List<EnvironmentTile>();

    //Enemy prefab assigned in the editor
    [SerializeField] public GameObject EnemyPrefab;

    //Store all currently active enemies
     List<Character> Enemies = new List<Character>();

    public List<Character> GetActiveEnemies()
    {
        return Enemies;
    }

    public void Init(List<List<EnvironmentTile>> m, Vector2Int s)
    {
        map = m;
        Size = s;
    }

    Vector2Int GetTile()
    {
        //Attempt to spawn the enemy 5 times before giving up
        for (int i = 0; i < 5; i++)
        {
            int x = Random.Range(0, Size.x);
            int y = Random.Range(0, Size.y);

            if (map[x][y].IsAccessible && !map[x][y].isOccupied && !map[x][y].IsEntrance)
            {
                return new Vector2Int(x, y);
            }
        }

        return new Vector2Int(-1, -1);
    }


    public void CleanupEnemies()
    {
        foreach(Character c in Enemies)
        {
            Destroy(c.gameObject);
            Enemies.Remove(c);
        }
        Enemies.Clear();
        map.Clear();
        Size = new Vector2Int();

        Debug.Log("Enemy Cleanup");
    }

    public void SpawnEnemies(int enemyCount)
    {
        for(int i = 0; i < enemyCount; i ++)
        {
            Vector2Int pos = roomT[Random.Range(0, roomT.Count)].GridPos;//GetTile();

            if (pos.x != -1 && pos.y != -1)
            {
                GameObject e = Instantiate(EnemyPrefab);
                e.transform.position =  map[pos.x][pos.y].Position;
                e.GetComponent<Character>().Init();
                e.GetComponent<Character>().characterName = "Enemy" + i;
                e.GetComponent<Character>().CurrentPosition = map[pos.x][pos.y];
                Enemies.Add(e.GetComponent<Character>());
                Debug.Log("Spawned Enemy");
            }
        }
    }
}
