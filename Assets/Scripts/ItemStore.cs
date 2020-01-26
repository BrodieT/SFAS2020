using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemStore : MonoBehaviour
{
    public int ID { get; set; }
    public string itemName { get; set; }
    public ItemType type { get; set; }
    public string Description { get; set; }
    ItemManager manager;
    GameObject player;
    GameObject container;
    public bool playerOwned;

    public bool isEquipped = false;
    public int armour { get; set; }
    public int weaponDmg { get; set; }
    

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public void setItem(ItemStore i)
    {
        ID = i.ID;
        itemName = i.itemName;
        type = i.type;
        Description = i.Description;
        armour = i.armour;
        weaponDmg = i.weaponDmg;
        isEquipped = i.isEquipped;


    }

    public void setItem(Item i)
    {
        ID = i.id;
        itemName = i.itemName;
        type = i.type;
        Description = "This is a " + itemName;
        armour = i.defence;
        weaponDmg = i.attack;
        isEquipped = i.isEquipped;

        switch (i.type)
        {
            case ItemType.MONEY:
                Description += ". Money can be exchanged for goods and services.";
                break;
            case ItemType.CONSUMABLE:
                Description += ". It can be consumed to restore your cores.";
                break;
            case ItemType.WEAPON:
                Description += ". It can be equipped to increase your damage in combat by " + weaponDmg + " hit points";
                break;
            case ItemType.ARMOUR:
                Description += ". It can be equipped to increase your damage threshold by " + armour + ", decreasing your opponent's effectiveness in combat.";
                break;
            case ItemType.QUESTITEM:
                Description += ". It can be returned to the quest giver for a reward.";
                break;
            default:
                Debug.LogError("Invalid Item ID. Ensure additional cases are added");
                break;
        }

       
    }
  
   
  
}
