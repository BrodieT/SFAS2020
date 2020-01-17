using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

enum combatState { START, PLAYER, ENEMY, WIN, LOSE, IDLE};
public enum attackType { MELEE, MAGIC, HEAL, POTION, BLOCK, COWER};
public class CombatManager : MonoBehaviour
{
    combatState state;
    [SerializeField] Character player;
    List<Character> enemies = new List<Character>();
    Character currentEnemy;

    [SerializeField] Text dialogue;

    [SerializeField] Text playerName;
    [SerializeField] Text enemyName;

    [SerializeField] Slider playerHP;
    [SerializeField] Slider playerMG;
    [SerializeField] Slider playerST;

    [SerializeField] Slider enemyHP;
    [SerializeField] Slider enemyMG;
    [SerializeField] Slider enemyST;


    [SerializeField] Canvas BattleHUD;

    [SerializeField] GameObject PlayerAttacks;

    private void Start()
    {
        state = combatState.IDLE;
        BattleHUD.gameObject.SetActive(false);
    }

    public void EngageCombat(List<Character> e)
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Character>();

        state = combatState.START;
        enemies.Clear();
        enemies.AddRange(e);
        currentEnemy = e[0];

        BattleHUD.gameObject.SetActive(true);
        UpdateHUD();
        state = combatState.PLAYER;
    }

    private void UpdateHUD()
    {
        playerHP.maxValue = player.maxHP;
        playerHP.value = player.currHP;

        playerMG.maxValue = player.maxMG;
        playerMG.value = player.currMG;
        
        playerST.maxValue = player.maxST;
        playerST.value = player.currST;

        playerName.text = player.characterName;

        enemyHP.maxValue = currentEnemy.maxHP;
        enemyHP.value = currentEnemy.currHP;
        
        enemyMG.maxValue = currentEnemy.maxMG;
        enemyMG.value = currentEnemy.currMG;
        
        enemyST.maxValue = currentEnemy.maxST;
        enemyST.value = currentEnemy.currST;
        
        enemyName.text = currentEnemy.characterName;

        if(state == combatState.START)
        {
            dialogue.text = currentEnemy.characterName + " approaches...";
        }
    }


    IEnumerator EnemyTurn()
    {
        bool playerDead = false;

        if (state == combatState.ENEMY)
        {

            //Can be modified to include a priority system here so more likely to heal when low on health etc.
            attackType attack = (attackType)Random.Range(0, 6);

            switch (attack)
            {
                case attackType.MELEE:
                    if (currentEnemy.Consume(2, 15))
                    {
                        playerDead = player.TakeDamage(currentEnemy.damage);
                        dialogue.text = currentEnemy.characterName + " performs a melee attack at you.";
                    }
                    else
                    {
                        dialogue.text = currentEnemy.characterName + " cowers.";
                    }
                    break;
                case attackType.MAGIC:
                    if (currentEnemy.Consume(1, 15))
                    {
                        playerDead = player.TakeDamage(currentEnemy.damage);
                        dialogue.text = currentEnemy.characterName + " performs a magic attack at you.";
                    }
                    break;
                case attackType.HEAL:
                    if (currentEnemy.Consume(1, 15))
                    {
                        currentEnemy.Restore(0, 25);
                        dialogue.text = currentEnemy.characterName + " casts healing on themself.";
                    }
                    break;
                case attackType.POTION:
                    currentEnemy.Restore(1, 25);
                    currentEnemy.Restore(2, 25);
                    dialogue.text = currentEnemy.characterName + " restores their abilities.";
                    break;
                case attackType.BLOCK:
                    if (currentEnemy.Consume(2, 5))
                    {
                        currentEnemy.Restore(0, 5);
                        dialogue.text = currentEnemy.characterName + " prepares to block your next attack.";
                    }
                    break;
                case attackType.COWER:
                    dialogue.text = currentEnemy.characterName + " cowers.";
                    break;
            }
        }

        UpdateHUD();

        yield return new WaitForSeconds(2);

        if (playerDead)
        {
            state = combatState.LOSE;
        }
        else
        {
            dialogue.text = "You prepare your next move...";
            state = combatState.PLAYER;
        }
    }



    IEnumerator PlayerAttack(attackType attack)
    {
        bool enemyDead = false;

        switch (attack)
        {
            case attackType.MELEE:
                enemyDead = currentEnemy.TakeDamage(player.damage);
                break;
            case attackType.MAGIC:
                enemyDead = currentEnemy.TakeDamage(player.damage);
                break;
            case attackType.HEAL:
                player.Restore(0, 25);
                break;
            case attackType.POTION:
                player.Restore(0, 50);
                player.Restore(1, 25);
                player.Restore(2, 25);
                break;
            case attackType.BLOCK:
                player.Restore(0, 5);
                break;
            case attackType.COWER:
                int successChance = Random.Range(0, 4);
                if (successChance == 3)
                {
                    dialogue.text = "You cower successfully and the gods take pity on you. You find your resolve for battle is fortified.";
                    int stat = Random.Range(0, 3);
                    player.Restore(stat, 50);
                }
                else
                {
                    dialogue.text = "You cower unsuccessfully, the gods have forsaken you. You find your resolve for battle falters.";
                    int stat = Random.Range(0, 3);
                    player.Restore(stat, 5);
                }
                break;
        }

        UpdateHUD();

        yield return new WaitForSeconds(2);

        //Check if enemy is dead
        if(enemyDead)
        {
            state = combatState.WIN;
            StartCoroutine(PlayerWin());
        }
        else
        {
            dialogue.text = "Your oponent prepares to attack...";
            state = combatState.ENEMY;
            yield return new WaitForSeconds(1);
            StartCoroutine(EnemyTurn());
        }
    }

    public void PlayerTurnOnCLick(int a)
    {
        attackType attack = (attackType)a;
        Debug.Log(attack);

        if (state == combatState.PLAYER)
        {
            switch (attack)
            {
                case attackType.MELEE:
                    if (player.Consume(2, 15))
                    {
                        dialogue.text = "You perform a melee attack.";
                        StartCoroutine(PlayerAttack(attack));
                    }
                    else
                    {
                        dialogue.text = "You don't have enough stamina to perform a melee attack.";
                    }
                    break;
                case attackType.MAGIC:
                    if (player.Consume(1, 15))
                    {
                        dialogue.text = "You perform a magic attack.";
                        StartCoroutine(PlayerAttack(attack));
                    }
                    else
                    {
                        dialogue.text = "You don't have enough magic to perform a magic attack.";
                    }
                    break;
                case attackType.HEAL:
                    if (player.Consume(1, 15))
                    {
                        dialogue.text = "You use magic to heal yourself.";
                        StartCoroutine(PlayerAttack(attack));
                    }
                    else
                    {
                        dialogue.text = "You don't have enough magic to cast heal.";
                    }
                    break;
                case attackType.POTION:
                    if (player.gameObject.GetComponent<Inventory>().potionsCount > 0)
                    {
                        dialogue.text = "You consume a potion to restore yourself.";
                        StartCoroutine(PlayerAttack(attack));
                    }
                    else
                    {
                        dialogue.text = "You don't have enough potions to perform this action.";
                    }

                    break;
                case attackType.BLOCK:
                    if (player.Consume(2, 5))
                    {
                        dialogue.text = "You attempt to block your opponents next attack, you feel renewed.";
                        StartCoroutine(PlayerAttack(attack));
                    }
                    else
                    {
                        dialogue.text = "You don't have enough stamina to block.";
                    }
                    break;
                case attackType.COWER:
                    StartCoroutine(PlayerAttack(attack));
                    break;
                default:
                    Debug.LogError("Invalid attack type");
                    break;
            }
        }
        else
        {
            Debug.LogError("Player attack buttons should not be visible when not on the players turn.");
        }
    }

    IEnumerator PlayerWin()
    {
        dialogue.text = "You have defeated " + currentEnemy.characterName;

        yield return new WaitForSeconds(2);

        if (enemies.Count > 1)
        {
            enemies.RemoveAt(0);
            currentEnemy = enemies[0];
            state = combatState.PLAYER;
        }
        else
        {
            BattleHUD.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(state != combatState.PLAYER && PlayerAttacks.activeSelf)
        {
            PlayerAttacks.SetActive(false);
        }
        else if(state == combatState.PLAYER && !PlayerAttacks.activeSelf)
        {
            PlayerAttacks.SetActive(true);

        }


    }
}
