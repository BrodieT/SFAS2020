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
    public bool IsAccessible { get; set; }
    public Vector2Int GridPos { get; set; }
    public bool IsStructure;


    public EnvironmentTile StructureOrigin { get; set; }


    private string TargetID;
    public void SetTargetID(string ID)
    {
        TargetID = ID;
    }
    public string GetTargetID()
    {
        if (!IsStructure)
        {
            return "00000";
        }
        else
        {
            return TargetID;
        }
    }
}
