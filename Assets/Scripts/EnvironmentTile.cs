using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentTile : MonoBehaviour
{
    public List<EnvironmentTile> Connections { get; set; }
    public EnvironmentTile Parent { get; set; }
    public Vector3 Position { get; set; }
    public float Global { get; set; }
    public float Local { get; set; }
    public bool Visited { get; set; }

    //Tracks whether the player can step onto the given tile
    public bool IsAccessible { get; set; }
    //Stores the x,y grid position of the given tile
    public Vector2Int GridPos { get; set; }
    //Tracks whether the tile is part of a structure
    public bool IsStructure { get; set; }
    //Tracks whether the given tile is an entrance/exit
    public bool IsEntrance { get; set; }
    //Stores the tile index of the tile, used when instantiating the cell
    public int TileNumb { get; set; }
    //Stores the origin tile for a structure
    public Vector2Int StructureOriginIndex { get; set; }

    public bool isOccupied { get; set; }
    public bool IsChest { get; set; }


    private int TargetID;
    public void SetTargetID(int ID)
    {
        TargetID = ID;
    }
    public int GetTargetID()
    {
        if (!IsStructure)
        {
            return 0;
        }
        else
        {
            return TargetID;
        }
    }
}
