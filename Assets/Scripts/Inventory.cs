using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] public List<Item> inventory;
    public ItemManager manager;
    [SerializeField] public int capacity;

    private void Start()
    {
        manager = GameObject.FindGameObjectWithTag("GameController").GetComponent<ItemManager>();
        if(!manager)
        {
            Debug.LogError("Item Manager not found! Ensure the game controller has the appropriate tag and component.");
        }
    }

    public void AddItem(int id)
    {
        if (inventory.Contains(manager.AllItems[id]))
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
            inventory.Add(manager.AllItems[id]);
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
                return;
            }
        }
    }
}


