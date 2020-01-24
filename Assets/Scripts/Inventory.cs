using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] public List<Item> inventory;
    public ItemManager manager;
    [SerializeField] public int capacity;
    public bool isContainer = false;

    public int potionsCount;
    public int currency;

    private void Start()
    {
        manager = GameObject.FindGameObjectWithTag("GameController").GetComponent<ItemManager>();
        if(!manager)
        {
            Debug.LogError("Item Manager not found! Ensure the game controller has the appropriate tag and component.");
        }

        if (isContainer)
        {
            capacity = 5;
            for (int i = 0; i < Random.Range(1, 5); i++)
            {
                AddItem(Random.Range(0, manager.AllItems.Count), 1);
            }
        }

    }

    public void SetCarryCapacity(int cap)
    {
        capacity = cap;
    }

    public void AddItem(int id, int amount)
    {
        if (inventory.Contains(manager.AllItems[id]))
        {
            if (manager.AllItems[id].type != ItemType.QUESTITEM)
            {
                foreach (Item i in inventory)
                {
                    if (i == manager.AllItems[id])
                    {
                        i.quantity++;
                        return;
                    }
                }
            }
            else
            {
                if (inventory.Count < capacity)
                {
                    inventory.Add(manager.AllItems[id]);
                }
                else
                {
                    Debug.Log("Overencumbered!");
                }
            }
           
        }
        else
        {
            if (inventory.Count < capacity)
            {
                inventory.Add(manager.AllItems[id]);
            }
            else
            {
                Debug.Log("Overencumbered!");
            }


        }

        if(manager.AllItems[id].isPotion)
        {
            potionsCount++;
        }

        if (manager.AllItems[id].isCurrency)
        {
            currency++;
        }
    }

    public void RemoveItem(int id)
    {
        foreach(Item i in inventory)
        {
            if(i.id == id)
            {
                if(i.quantity > 1)
                {
                    i.quantity--;
                }
                else
                {
                    inventory.Remove(i);
                }

                if (i.isPotion)
                {
                    potionsCount--;
                }

                if (i.isCurrency)
                {
                    currency--;
                }
                return;
            }
        }

       
    }


    public void Equip(ItemStore i)
    {
        if (i.type != ItemType.MONEY && i.type != ItemType.QUESTITEM)
        {
            Character character = transform.gameObject.GetComponent<Character>();

            if (character != null)
            {
                if (i.type == ItemType.CONSUMABLE)
                {
                    character.Restore(0, 50);
                    character.Restore(1, 25);
                    character.Restore(2, 25);
                    RemoveItem(i.ID);
                }
                else
                {
                    foreach (Item item in inventory)
                    {
                        if (item.id == i.ID
                            && item.defence == i.armour
                            && item.attack == i.weaponDmg)
                        {
                            if (!item.isEquipped)
                            {
                                item.isEquipped = true;
                                character.damage += i.weaponDmg;
                                character.dmgThreshold += i.armour;
                                Debug.Log("Equipping " + item.itemName);
                                Debug.Log(character.damage + ", " + character.dmgThreshold);
                                return;
                            }
                            else
                            {
                                item.isEquipped = false;
                                character.damage -= i.weaponDmg;
                                character.dmgThreshold -= i.armour;
                                Debug.Log("Unequipping " + item.itemName);
                                Debug.Log(character.damage + ", " + character.dmgThreshold);
                                return;
                            }
                        }
                    }


                }
            }
        }

    }

    public void Unequip(ItemStore i)
    {
        Character character = transform.gameObject.GetComponent<Character>();

        if (character != null)
        {
            if(i.ID == 2 || i.ID == 3)
            { 
                character.damage -= i.weaponDmg;
                character.dmgThreshold -= i.armour;
            }
        }
    }
}


