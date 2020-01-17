using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemStore : MonoBehaviour
{
    public int ID = 0;
    ItemManager manager;
    GameObject player;
    GameObject container;
    public bool playerOwned;


    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public void SetContainer(GameObject c)
    {
        container = c;
    }

  
    public void ButtonClicked()
    {
        if (container != null)
        {
            if (!playerOwned)
            {
                player.GetComponent<Inventory>().AddItem(ID);
                container.GetComponent<Inventory>().RemoveItem(ID);
            }
            else
            {
                player.GetComponent<Inventory>().RemoveItem(ID);
                container.GetComponent<Inventory>().AddItem(ID);
            }
        }
    }
  
}
