using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Item
{
    [SerializeField] public string itemName;
    [SerializeField] public int id;
    [SerializeField] public Sprite icon;
    [SerializeField] public int value;
    [SerializeField] public int weight;

    [SerializeField] public int quantity;

    [SerializeField] public int attack;
    [SerializeField] public int defence;

}
