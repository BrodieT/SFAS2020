using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment : MonoBehaviour
{
    //List of prefabs for all tiles both accessible and inaccessible
    [SerializeField] private List<EnvironmentTile> AccessibleTiles;
    [SerializeField] private List<EnvironmentTile> InaccessibleTiles;
    [SerializeField] private List<EnvironmentTile> Structures;
    //The width and height of the grid
    [SerializeField] private Vector2Int Size;
    //The percentage of the grid that is accessible
    [SerializeField] private float AccessiblePercentage;


    //TODO
    //Fix the pathfinding to the structure origin nodes 
    // ensure changes made to tiles are updated in mAll?
    //Fix cancel button in dialogue box and ensure the player is returned to the entrance tile

    //Find way to either modify code in environment generation or manually craft the interior cells
    //Make temp models for the different structures that span multiple tiles

    private EnvironmentTile[][] mMap;
    private List<EnvironmentTile> mAll;
    private List<EnvironmentTile> mToBeTested;
    private List<EnvironmentTile> mLastSolution;

    private readonly Vector3 NodeSize = Vector3.one * 9.0f; 
    private const float TileSize = 10.0f;
    private const float TileHeight = 2.5f;

    public EnvironmentTile Start { get; private set; }
    List<EnvironmentTile> StructureTiles = new List<EnvironmentTile>();

    private void Awake()
    {
        mAll = new List<EnvironmentTile>();
        mToBeTested = new List<EnvironmentTile>();
    }

    private void OnDrawGizmos()
    {
        // Draw the environment nodes and connections if we have them
        if (mMap != null)
        {
            for (int x = 0; x < Size.x; ++x)
            {
                for (int y = 0; y < Size.y; ++y)
                {
                    if (mMap[x][y].Connections != null)
                    {
                        for (int n = 0; n < mMap[x][y].Connections.Count; ++n)
                        {
                            Gizmos.color = Color.blue;
                            Gizmos.DrawLine(mMap[x][y].Position, mMap[x][y].Connections[n].Position);
                        }
                    }

                    // Use different colours to represent the state of the nodes
                    Color c = Color.white;
                    if ( !mMap[x][y].IsAccessible )
                    {
                        c = Color.red;
                    }
                    else
                    {
                        if(mLastSolution != null && mLastSolution.Contains( mMap[x][y] ))
                        {
                            c = Color.green;
                        }
                        else if (mMap[x][y].Visited)
                        {
                            c = Color.yellow;
                        }
                    }

                    Gizmos.color = c;
                    Gizmos.DrawWireCube(mMap[x][y].Position, NodeSize);
                }
            }
        }
    }

    //This function is used when generating structures to esnure they arent spawned too close to each other or the start tile
    bool IsNear(int x, int y, int radius)
    {
        //This list will store the tiles surrounding the given point using the radius provided
        List<EnvironmentTile> eT = new List<EnvironmentTile>();

        for(int i = -radius; i< radius; i++)
        {
            for(int j = -radius; j < radius; j++)
            {
                if (x + i > 0 && x + i < Size.x && y + j > 0 && y + j < Size.y)
                {
                    eT.Add(mMap[x + i][y + j]);
                }
            }
        }
        
        
        bool near = true;
             
        //loop through the surrounding tiles and return true if any of them are the start point or an existing structure
        foreach(EnvironmentTile e in eT)
        {
            if(!e.IsStructure && e != Start)
            {
                near = false;
            }
            else
            {
                //return true if any structures or start point is found and stop looping
                return true;
            }
        }


        return near;        
    }


    //This function will randomly generate the structures on the map
    private void GenerateStructures()
    {
        int structCounter = 1;

      while(Structures.Count > 0)
        { 
            int x = Random.Range(0, Size.x);
            int y = Random.Range(0, Size.y);

            if (!mMap[x][y].IsStructure && !IsNear(x, y, 4) && y > Structures[0].GetComponent<StructureData>().Length / 2 + 1)
            {

                int w = Structures[0].GetComponent<StructureData>().Width/2;
                int l = Structures[0].GetComponent<StructureData>().Length/2;

           
                EnvironmentTile tile = Instantiate(Structures[0], mMap[x][y].Position - new Vector3((TileSize / 2), TileHeight, (TileSize / 2)), Quaternion.identity, transform);
                tile.Position = mMap[x][y].Position;
                tile.IsAccessible = false;
                tile.GridPos = new Vector2Int(x, y);
                Destroy(GameObject.Find(mMap[x][y].name).gameObject);

                tile.gameObject.name = mMap[x][y].name;
                
                mMap[x][y] = tile;
                mMap[x][y].IsStructure = true;
                mMap[x][y].SetTargetID("0000" + structCounter.ToString());
                structCounter++;



                Structures[0].GetComponent<StructureData>().Entrance = mMap[x][y];


                for (int i = -w; i < w; i++)
                {
                    for (int j = -l; j < l; j++)
                    {
                        //check if the grid position about to be accessed is within the bounds of the array
                        if (x + i > 0 && x + i < Size.x && y + j > 0 && y + j < Size.y)
                        {
                            mMap[x + i][y + j].IsStructure = true;
                            mMap[x + i][y + j].StructureOrigin = mMap[x][y - l];
                            mMap[x + i][y + j].IsAccessible = true;

                        }
                    }
                }


                StructureTiles.Add(tile);
                Structures.RemoveAt(0);
            }
        }
    }

    private void Generate()
    {
        // Setup the map of the environment tiles according to the specified width and height
        // Generate tiles from the list of accessible and inaccessible prefabs using a random
        // and the specified accessible percentage
        mMap = new EnvironmentTile[Size.x][];

        int halfWidth = Size.x / 2;
        int halfHeight = Size.y / 2;
        Vector3 position = new Vector3( -(halfWidth * TileSize), 0.0f, -(halfHeight * TileSize) );
        bool start = true;

        for ( int x = 0; x < Size.x; ++x)
        {
            mMap[x] = new EnvironmentTile[Size.y];
            for ( int y = 0; y < Size.y; ++y)
            {
                bool isAccessible = start || Random.value < AccessiblePercentage;
                List<EnvironmentTile> tiles = isAccessible ? AccessibleTiles : InaccessibleTiles;
                EnvironmentTile prefab = tiles[Random.Range(0, tiles.Count)];
                EnvironmentTile tile = Instantiate(prefab, position, Quaternion.identity, transform);
                tile.Position = new Vector3( position.x + (TileSize / 2), TileHeight, position.z + (TileSize / 2));
                tile.IsAccessible = isAccessible;
                tile.IsStructure = false;
                tile.gameObject.name = string.Format("Tile({0},{1})", x, y);
                tile.GridPos = new Vector2Int(x, y);
                mMap[x][y] = tile;
                mAll.Add(tile);

                if(start)
                {
                    Start = tile;
                }

                position.z += TileSize;
                start = false;
            }

            position.x += TileSize;
            position.z = -(halfHeight * TileSize);
        }
        GenerateStructures();
    }

    private void SetupConnections()
    {
        // Currently we are only setting up connections between adjacnt nodes
        for (int x = 0; x < Size.x; ++x)
        {
            for (int y = 0; y < Size.y; ++y)
            {
                EnvironmentTile tile = mMap[x][y];
                tile.Connections = new List<EnvironmentTile>();
                if (x > 0)
                {
                    tile.Connections.Add(mMap[x - 1][y]);
                }

                if (x < Size.x - 1)
                {
                    tile.Connections.Add(mMap[x + 1][y]);
                }

                if (y > 0)
                {
                    tile.Connections.Add(mMap[x][y - 1]);
                }

                if (y < Size.y - 1)
                {
                    tile.Connections.Add(mMap[x][y + 1]);
                }
            }
        }
    }

    private float Distance(EnvironmentTile a, EnvironmentTile b)
    {
        // Use the length of the connection between these two nodes to find the distance, this 
        // is used to calculate the local goal during the search for a path to a location
        float result = float.MaxValue;
        EnvironmentTile directConnection = a.Connections.Find(c => c == b);
        if (directConnection != null)
        {
            result = TileSize;
        }
        return result;
    }

    private float Heuristic(EnvironmentTile a, EnvironmentTile b)
    {
        // Use the locations of the node to estimate how close they are by line of sight
        // experiment here with better ways of estimating the distance. This is used  to
        // calculate the global goal and work out the best order to prossess nodes in
        return Vector3.Distance(a.Position, b.Position);
    }

    public void GenerateWorld()
    {
        Generate();
        SetupConnections();
    }

    public void CleanUpWorld()
    {
        if (mMap != null)
        {
            for (int x = 0; x < Size.x; ++x)
            {
                for (int y = 0; y < Size.y; ++y)
                {
                    Destroy(mMap[x][y].gameObject);
                }
            }
        }
    }

    //This function handles the pathfinding through the world
    public List<EnvironmentTile> Solve(EnvironmentTile begin, EnvironmentTile destination)
    {
        List<EnvironmentTile> result = null;
        if (begin != null && destination != null && destination.IsAccessible)
        {
            // Nothing to solve if there is a direct connection between these two locations
            EnvironmentTile directConnection = begin.Connections.Find(c => c == destination);
            if (directConnection == null)
            {
                // Set all the state to its starting values
                mToBeTested.Clear();

                for( int count = 0; count < mAll.Count; ++count )
                {
                    mAll[count].Parent = null;
                    mAll[count].Global = float.MaxValue;
                    mAll[count].Local = float.MaxValue;
                    mAll[count].Visited = false;
                }

                // Setup the start node to be zero away from start and estimate distance to target
                EnvironmentTile currentNode = begin;
                currentNode.Local = 0.0f;
                currentNode.Global = Heuristic(begin, destination);

                // Maintain a list of nodes to be tested and begin with the start node, keep going
                // as long as we still have nodes to test and we haven't reached the destination
                mToBeTested.Add(currentNode);

                while (mToBeTested.Count > 0 && currentNode != destination)
                {
                    // Begin by sorting the list each time by the heuristic
                    mToBeTested.Sort((a, b) => (int)(a.Global - b.Global));

                    // Remove any tiles that have already been visited
                    mToBeTested.RemoveAll(n => n.Visited);

                    // Check that we still have locations to visit
                    if (mToBeTested.Count > 0)
                    {
                        // Mark this note visited and then process it
                        currentNode = mToBeTested[0];
                        currentNode.Visited = true;

                        // Check each neighbour, if it is accessible and hasn't already been 
                        // processed then add it to the list to be tested 
                        for (int count = 0; count < currentNode.Connections.Count; ++count)
                        {
                            EnvironmentTile neighbour = currentNode.Connections[count];

                            if (!neighbour.Visited && neighbour.IsAccessible)
                            {
                                mToBeTested.Add(neighbour);
                            }

                            // Calculate the local goal of this location from our current location and 
                            // test if it is lower than the local goal it currently holds, if so then
                            // we can update it to be owned by the current node instead 
                            float possibleLocalGoal = currentNode.Local + Distance(currentNode, neighbour);
                            if (possibleLocalGoal < neighbour.Local)
                            {
                                neighbour.Parent = currentNode;
                                neighbour.Local = possibleLocalGoal;
                                neighbour.Global = neighbour.Local + Heuristic(neighbour, destination);
                            }
                        }
                    }
                }

                // Build path if we found one, by checking if the destination was visited, if so then 
                // we have a solution, trace it back through the parents and return the reverse route
                if (destination.Visited)
                {
                    result = new List<EnvironmentTile>();
                    EnvironmentTile routeNode = destination;

                    while (routeNode.Parent != null)
                    {
                        result.Add(routeNode);
                        routeNode = routeNode.Parent;
                    }
                    result.Add(routeNode);
                    result.Reverse();
                    //Debug.LogFormat("Path Found: {0} steps {1} long", result.Count, destination.Local);
                }
                else
                {
                    Debug.LogWarning("Path Not Found");
                }
            }
            else
            {
                result = new List<EnvironmentTile>();
                result.Add(begin);
                result.Add(destination);
                Debug.LogFormat("Direct Connection: {0} <-> {1} {2} long", begin, destination, TileSize);
            }
        }
        else
        {
            Debug.LogWarning("Cannot find path for invalid nodes");
        }

        mLastSolution = result;

        return result;
    }
}
