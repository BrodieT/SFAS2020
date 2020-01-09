using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MapData
{
    public MapData(List<List<EnvironmentTile>> m, EnvironmentTile t)
    {
        map = m;
        start = t;
    }
    public List<List<EnvironmentTile>> map { get; set; }
    public EnvironmentTile start { get; set; }
    public bool isDungeon = false;
}

//This static class is used to store the map data across scenes 
//to allow the player to return to a previously generated cell
public static class CellStorage
{

  
    //Stores the cells in a list
   public static List<MapData> cells = new List<MapData>();

    //index in the list of the current cell
    public static int CurrentCell;



}
