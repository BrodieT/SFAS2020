using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyState { PATROL, CAUGHT, ATTACK}
public class EnemyController : MonoBehaviour
{
    Environment Map;
    Character me;
    Character player;
    EnemyState myState = EnemyState.PATROL;

    Vector2Int[] PatrolPoints = new Vector2Int[5];

    // Start is called before the first frame update
    void Start()
    {
        Map = GameObject.FindGameObjectWithTag("GameController").GetComponentInChildren<Environment>();
        me = GetComponent<Character>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Character>();

        PatrolPoints[0] = new Vector2Int(1, 0);
        PatrolPoints[1] = new Vector2Int(-1, 0);
        PatrolPoints[2] = new Vector2Int(0, 1);
        PatrolPoints[3] = new Vector2Int(0, -1);
        PatrolPoints[4] = new Vector2Int(0, 0);

        if(Map == null)
        {
            Debug.LogError("Enemy cannot find the game controller. Ensure a game controller is present and that it contains the Environment component.");
        }

        if(player == null)
        {
            Debug.LogError("Enemy cannot find the player character, movement AI and combat has been compromised. Ensure the player is properly created.");
        }

        me.atDestination = true;
    }

    int index = 0;

    public int DistanceToPlayer = 0;
    // Update is called once per frame
    void Update()
    {
        List<EnvironmentTile> route2P = Map.Solve(me.CurrentPosition, player.CurrentPosition);
        if(route2P != null)
        {
            DistanceToPlayer = route2P.Count;
        }

        if (DistanceToPlayer < 10 && DistanceToPlayer > 2)
        {
            myState = EnemyState.ATTACK;
        }
        else if(DistanceToPlayer <= 2)
        {
            myState = EnemyState.CAUGHT;
        }
        else
        {
            myState = EnemyState.PATROL;
        }

        switch(myState)
        {
            case EnemyState.PATROL:
                if (me.atDestination)
                {
                    List<EnvironmentTile> route = new List<EnvironmentTile>();
                    index = Random.Range(0, 5);

                    if (me.CurrentPosition.GridPos.x + PatrolPoints[index].x < Map.mmap.Count &&
                        me.CurrentPosition.GridPos.y + PatrolPoints[index].y < Map.mmap[0].Count &&
                        me.CurrentPosition.GridPos.x + PatrolPoints[index].x >= 0 &&
                         me.CurrentPosition.GridPos.y + PatrolPoints[index].y >= 0)
                    {
                        route = Map.Solve(me.CurrentPosition, Map.mmap[me.CurrentPosition.GridPos.x + PatrolPoints[index].x][me.CurrentPosition.GridPos.y + PatrolPoints[index].y]);
                        if (route != null)
                        {
                            me.GoTo(route);
                        }
                    }
                }
                break;
            case EnemyState.ATTACK:
                if (me.atDestination)
                {
                    List<EnvironmentTile> route = Map.Solve(me.CurrentPosition, player.CurrentPosition);
                    if (route != null)
                    {
                        route.RemoveAt(route.Count - 1);
                        me.GoTo(route);
                    }
                }
                break;
            case EnemyState.CAUGHT:
                Debug.Log("I have caught the player");

                GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>().PlayerCaught = true;
                break;
        }
            
        

    }
}
