using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This class is used on the main menu to initialise the cells in the game and store them in a static class
//It will also handle the entering of the first cell from the menu
public class CreateWorld : MonoBehaviour
{
    //List<List<EnvironmentTile>> map;
    //private Environment mMap;

    //This function will handle the creation of the game world by initialising all cells and storing them in the static class
    //ie New Game button press
    void Start()
    {
        //CleanUp cell store
        //Create world and add to the cell store
        //create the 10 dungeons, adapting the genrate dungeon function to take in a difficulty/level parameter, and adding to the cell store
        //create the town by creating a new generate function and add to cell store

        //mMap = GetComponentInChildren<Environment>();

        //CellStorage.cells.Clear();

        //map = mMap.GenerateWorld(0.85f, new Vector2Int(50, 40));
        //CellStorage.cells.Add(new MapData(map, null));

        //map.Clear();

        //map = mMap.GenerateDungeon(1.0f, new Vector2Int(50, 40));
        //CellStorage.cells.Add(new MapData(map, null));
    }

    //This function will handle the entering of a given cell
    //ie New Game OR Load Game
    public void EnterCell(int cellNumb)
    {
        switch(cellNumb)
        {
            default:
                //Load the world cell
                break;
        }
    }
}
