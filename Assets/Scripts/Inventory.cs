using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] public List<Item> inventory;
    public ItemManager manager;
    [SerializeField] public int capacity;
    public bool isContainer = false;

    public List<GameObject> gridcells = new List<GameObject>();

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
            for (int i = 0; i < Random.Range(1, 5); i++)
            {
                AddItem(Random.Range(0, manager.AllItems.Count));
            }
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

        if(manager.AllItems[id].isPotion)
        {
            potionsCount++;
        }

        if (manager.AllItems[id].isCurrency)
        {
            currency++;
        }
        //manager.GetComponent<Game>().RefreshInventory(!isContainer);
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

                    if(gridcells != null)
                    {
                       foreach(GameObject g in gridcells)
                        {
                            if(g.GetComponent<ItemStore>().ID == id)
                            {
                                Destroy(g);
                                gridcells.Clear();
                                return;
                            }
                        }

                    }
                }

                gridcells.Clear();
                return;
            }
        }
    }
}


