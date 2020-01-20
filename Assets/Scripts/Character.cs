using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] private float SingleNodeMoveTime = 0.5f;
    public bool atDestination = false;

    [SerializeField] public int maxHP  {get;set;}
    [SerializeField] public int currHP {get;set;}
    [SerializeField] public int maxMG  {get;set;}
    [SerializeField] public int currMG {get;set;}
    [SerializeField] public int maxST  {get;set;}
    [SerializeField] public int currST { get; set; }
                    
    [SerializeField] public int damage { get; set; }
    [SerializeField] public int dmgThreshold { get; set; }

    [SerializeField] public string characterName { get; set; }

    public bool InCombat { get; set; }

    private void Start()
    {
        InCombat = false;
        Init();
    }

    public void Init()
    {
        maxHP = 100;
        currHP = 100;
        maxMG = 100;
        currMG = 100;
        maxST = 100;
        currST = 100;
        damage = 5;
        dmgThreshold = 0;
    }

    public bool TakeDamage(int dmg)
    {
        currHP -= (dmg - dmgThreshold);

        if(currHP <= 0)
        {
            currHP = 0;
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool Consume(int stat, int amount)
    {
        switch (stat)
        {
            case 1:

                if (currMG - amount >= 0)
                {
                    currMG -= amount;
                    return true;
                }
                break;
            case 2:

                if (currST - amount >= 0)
                {
                    currST -= amount;
                    return true;
                }
                break;
            default:
                break;
        }
        return false;
    }
    public void Restore(int stat, int amount)
    {
        switch(stat)
        {
            case 0:
                currHP += amount;

                if(currHP > maxHP)
                {
                    currHP = maxHP;
                }
                break;
            case 1:
                currMG += amount;

                if (currMG > maxMG)
                {
                    currMG = maxMG;
                }
                break;
            case 2:
                currST += amount;

                if (currST > maxST)
                {
                    currST = maxST;
                }
                break;
            default:
                Debug.LogWarning("Invalid Stat. Unable to restore desired amount");
                break;
        }
    }

    public EnvironmentTile CurrentPosition { get; set; }

    private IEnumerator DoMove(Vector3 position, Vector3 destination)
    {
        // Move between the two specified positions over the specified amount of time
        if (position != destination)
        {
            transform.rotation = Quaternion.LookRotation(destination - position, Vector3.up);

            Vector3 p = transform.position;
            float t = 0.0f;

            while (t < SingleNodeMoveTime)
            {
                t += Time.deltaTime;
                p = Vector3.Lerp(position, destination, t / SingleNodeMoveTime);
                transform.position = p;
                yield return null;
            }
        }
    }

    private IEnumerator DoGoTo(List<EnvironmentTile> route)
    {
        // Move through each tile in the given route
        if (route != null)
        {
            Vector3 position = CurrentPosition.Position;
            for (int count = 0; count < route.Count; ++count)
            {
                Vector3 next = route[count].Position;
                yield return DoMove(position, next);
                CurrentPosition = route[count];
                position = next;
            }
            atDestination = true;
        }

    }

    public void GoTo(List<EnvironmentTile> route)
    {
        atDestination = false;
        // Clear all coroutines before starting the new route so 
        // that clicks can interupt any current route animation
        StopAllCoroutines();
        StartCoroutine(DoGoTo(route));
    }
}
