using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment : MonoBehaviour
{
    //List of prefabs for all tiles both accessible and inaccessible
    [SerializeField] private List<EnvironmentTile> AccessibleTilesWorld;
    [SerializeField] private List<EnvironmentTile> InaccessibleTilesWorld;

    [SerializeField] private List<EnvironmentTile> AccessibleTilesDungeon;
    [SerializeField] private List<EnvironmentTile> InaccessibleTilesDungeon;

    [SerializeField] private List<EnvironmentTile> PropsDungeon;

    [SerializeField] private List<EnvironmentTile> Structures;
    //The width and height of the grid
    //[SerializeField] private Vector2Int Size;
    //The percentage of the grid that is accessible
    //[SerializeField] private float AccessiblePercentage;


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
        if (mmap != null)
        {
            for (int x = 0; x < mmap.Count; ++x)
            {
                for (int y = 0; y < mmap[x].Count; ++y)
                {
                    if (mmap[x][y].Connections != null)
                    {
                        for (int n = 0; n < mmap[x][y].Connections.Count; ++n)
                        {
                            Gizmos.color = Color.blue;
                            Gizmos.DrawLine(mmap[x][y].Position, mmap[x][y].Connections[n].Position);
                        }
                    }

                    // Use different colours to represent the state of the nodes
                    Color c = Color.white;
                    if ( !mmap[x][y].IsAccessible )
                    {
                        c = Color.red;
                    }
                    else
                    {
                        if(mLastSolution != null && mLastSolution.Contains( mmap[x][y] ))
                        {
                            c = Color.green;
                        }
                        else if (mmap[x][y].Visited)
                        {
                            c = Color.yellow;
                        }


                        if(mmap[x][y].IsChest)
                        {
                            c = Color.black;
                        }
                    }

                    if (mmap[x][y].IsEntrance)
                    {
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawWireCube(mmap[x][y].Position + Vector3.up, new Vector3(NodeSize.x, NodeSize.y * 2, NodeSize.z));
                    }
                    else
                    {
                        Gizmos.color = c;
                        Gizmos.DrawWireCube(mmap[x][y].Position, NodeSize);
                    }
                }
            }
        }
    }


    public List<GameObject> structurePrefabs = new List<GameObject>();

    private List<List<EnvironmentTile>> Generate(List<List<EnvironmentTile>> map, bool isDungeon, Vector2Int Size, float AccessiblePercentage)
    {
        // Setup the map of the environment tiles according to the specified width and height
        // Generate tiles from the list of accessible and inaccessible prefabs using a random
        // and the specified accessible percentage

        int halfWidth = Size.x / 2;
        int halfHeight = Size.y / 2;
        Vector3 position = new Vector3(-(halfWidth * TileSize), 0.0f, -(halfHeight * TileSize));
        bool start = true;

        for (int x = 0; x < Size.x; ++x)
        {
            map.Add(new List<EnvironmentTile>());
            for (int y = 0; y < Size.y; ++y)
            {
                bool isAccessible = true;
                if (!isDungeon)
                {
                    isAccessible = start || Random.value < AccessiblePercentage;
                }
                else
                {
                    isAccessible = start || Random.value < 1;
                }
             
                EnvironmentTile tile = new EnvironmentTile();
                tile.Position = new Vector3(position.x + (TileSize / 2), TileHeight, position.z + (TileSize / 2));
                tile.IsAccessible = isAccessible;
                tile.IsStructure = false;
                tile.GridPos = new Vector2Int(x, y);
                map[x].Add(tile);

                if (start)
                {
                    Start = tile;
                }

                position.z += TileSize;
                start = false;
            }

            position.x += TileSize;
            position.z = -(halfHeight * TileSize);
        }

        return map;
    }

    //This function handles the generation of structures on the world map.
    //It chooses several random points on the map and places an entrance to a dungeon there
    private List<List<EnvironmentTile>> GenerateStructures(List<List<EnvironmentTile>> map, Vector2Int Size)
    {
        int structCounter = 1;
        int counter = 0;
        int StructCount = Structures.Count;
        //This will generate 5 structures across the map.
        //The grid positions will be chosen at random and the surrounding tiles will be made inaccessible excluding a path up to the entrance.
        while (StructCount > 0)
        {
            if (counter <= 5)
            {
                int x = Random.Range(0, Size.x);
                int y = Random.Range(0, Size.y);

                //Checks that the chosen spot wont result in structures overlapping
                if (!map[x][y].IsStructure && !IsNear(x, y, 4, Size, map) && y > Structures[0].GetComponent<StructureData>().Length / 2 + 1)
                {
                    int w = Structures[0].GetComponent<StructureData>().Width / 2;
                    int l = Structures[0].GetComponent<StructureData>().Length / 2;

                    EnvironmentTile tile = new EnvironmentTile();//Instantiate(Structures[0], map[x][y].Position - new Vector3((TileSize / 2), TileHeight, (TileSize / 2)), Quaternion.identity, transform);
                    tile.Position = map[x][y].Position;
                    tile.IsAccessible = false;
                    tile.GridPos = new Vector2Int(x, y);

                    map[x][y] = tile;
                    map[x][y].IsStructure = true;
                    map[x][y].IsChest = false;
                    map[x][y].SetTargetID(structCounter);
                    structCounter++;

                    Structures[0].GetComponent<StructureData>().Entrance = map[x][y - l];

                    for (int i = -w; i < w; i++)
                    {
                        for (int j = -l; j < l; j++)
                        {
                            //check if the grid position about to be accessed is within the bounds of the array
                            if (x + i > 0 && x + i < Size.x && y + j > 0 && y + j < Size.y)
                            {
                                if (i == 0 && j == 0)
                                {
                                    map[x + i][y + j].IsAccessible = true;
                                    map[x + i][y + j].IsEntrance = true;

                                }
                                else if (i == 0 && j < 0)
                                {
                                    map[x + i][y + j].IsAccessible = true;
                                    map[x + i][y + j].IsEntrance = false;
                                }
                                else
                                {
                                    map[x + i][y + j].IsAccessible = false;
                                    map[x + i][y + j].IsEntrance = false;
                                }

                                map[x + i][y + j].IsStructure = true;
                                map[x + i][y + j].StructureOriginIndex = new Vector2Int(x, y);
                            }
                            
                        }
                    }

                    if (y - (l + 1) > 0 && x + 1 < Size.x && x - 1 > 0)
                    {
                        map[x][y - (l + 1)].IsAccessible = true;
                        map[x + 1][y - (l + 1)].IsAccessible = true;
                        map[x - 1][y - (l + 1)].IsAccessible = true;
                    }
                    counter = 0;
                    StructureTiles.Add(tile);
                    StructCount--;
                }

            }
            else
            {
                StructCount--;
                Debug.LogWarning("Failed to create structure");
            }
            counter++;
        }

        
        //Clear the starting area to ensure the player can access the map 
        //A safe-area or town could be spawned here for access to a merchant/quest-giver etc.
        for(int i = 0; i < 5; i ++)
        {
            for(int j = 0; j < 5; j++)
            {
                map[i][j].IsAccessible = true;
            }
        }

        //Spawn a tester chest at the starting position of the player 
        //Used for testing the trading system but would later be adapted for spawning throughout the dungeons
        map[3][3].IsChest = true;
        map[3][3].IsAccessible = true;

        return map;
    }

    //This function is used when generating structures to esnure they arent spawned too close to each other or the start tile
    bool IsNear(int x, int y, int radius, Vector2Int Size, List<List<EnvironmentTile>> map)
    {
        //This list will store the tiles surrounding the given point using the radius provided
        List<EnvironmentTile> eT = new List<EnvironmentTile>();

        for (int i = -radius; i < radius; i++)
        {
            for (int j = -radius; j < radius; j++)
            {
                if (x + i > 10 && x + i < Size.x && y + j > 10 && y + j < Size.y)
                {
                    eT.Add(map[x + i][y + j]);
                }
            }
        }


        bool near = true;

        //loop through the surrounding tiles and return true if any of them are the start point or an existing structure
        foreach (EnvironmentTile e in eT)
        {
            if (!e.IsStructure)
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
    public List<List<EnvironmentTile>> GenerateWorld(float AccessiblePercentage, Vector2Int Size)
    {
        List<List<EnvironmentTile>> map = new List<List<EnvironmentTile>>();

        map = Generate(map, false, Size, 0.85f);
        
        map = GenerateStructures(map, Size);

        return map;
    }

    public List<List<EnvironmentTile>> mmap;
    private void SetupConnections()
    {
        // Currently we are only setting up connections between adjacnt nodes
        for (int x = 0; x < mmap.Count; ++x)
        {
            for (int y = 0; y < mmap[x].Count; ++y)
            {

                EnvironmentTile tile = mmap[x][y];
                tile.Connections = new List<EnvironmentTile>();
                if (x > 0)
                {
                    tile.Connections.Add(mmap[x - 1][y]);
                }

                if (x < mmap.Count - 1)
                {
                    tile.Connections.Add(mmap[x + 1][y]);
                }

                if (y > 0)
                {
                    tile.Connections.Add(mmap[x][y - 1]);
                }

                if (y < mmap[x].Count - 1)
                {
                    tile.Connections.Add(mmap[x][y + 1]);
                }
            }
        }
        
    }


    public List<EnvironmentTile> nullTile;
    public EnvironmentTile chest;
    public void CreateCell(List<List<EnvironmentTile>> map, bool isDungeon)
    {

        mAll.Clear();
        mToBeTested.Clear();

        
        int halfWidth = map.Count / 2;
        int halfHeight = map[0].Count / 2;
        Vector3 position = new Vector3(-(halfWidth * TileSize), 0.0f, -(halfHeight * TileSize));

        for (int x = 0; x < map.Count; x++)
        {
            for (int y = 0; y < map[x].Count; y++)
            {

                List<EnvironmentTile> tiles;

                if (!isDungeon)
                {
                    if (map[x][y].IsAccessible && !map[x][y].IsEntrance)
                    {
                        tiles = AccessibleTilesWorld;
                    }
                    else if (map[x][y].IsStructure && !map[x][y].IsEntrance)
                    {
                        tiles = AccessibleTilesWorld;
                    }
                    else
                    {
                        tiles = InaccessibleTilesWorld;
                    }
                }
                else
                {
                    if (map[x][y].IsAccessible && !map[x][y].IsEntrance)
                    {
                        tiles = AccessibleTilesDungeon;
                    }
                    else if (map[x][y].IsAccessible && map[x][y].IsEntrance)
                    {
                        tiles = Structures;
                    }
                    else
                    {
                        tiles = InaccessibleTilesDungeon;
                    }
                }

                

                EnvironmentTile prefab = tiles[Random.Range(0, tiles.Count)];

                if (map[x][y].IsStructure && map[x][y].IsEntrance)
                {
                    if (isDungeon)
                    {
                        prefab = Structures[1];
                    }
                    else
                    {
                        prefab = Structures[0];
                    }
                }

                if (map[x][y].IsChest)
                {
                    prefab = chest;
                }

                EnvironmentTile tile = Instantiate(prefab, position, Quaternion.identity, transform);
                tile.gameObject.name = string.Format("Tile({0},{1})", x, y);

                tile.Connections = map[x][y].Connections;
                tile.GridPos = map[x][y].GridPos;
                tile.Position = map[x][y].Position;
                tile.IsAccessible = map[x][y].IsAccessible;
                tile.IsStructure = map[x][y].IsStructure;

                if (!isDungeon)
                {
                    if (tile.IsStructure)
                    {
                        tile.StructureOriginIndex = map[x][y].StructureOriginIndex;
                    }
                }
                tile.IsEntrance = map[x][y].IsEntrance;
                if (tile.IsEntrance)
                {
                    tile.IsAccessible = true;
                }

                if (map[x][y].GetTargetID() != 0)
                {
                    tile.SetTargetID(map[x][y].GetTargetID());
                }
                tile.TileNumb = map[x][y].TileNumb;

                tile.IsChest = map[x][y].IsChest;


                position.z += TileSize;

                mAll.Add(tile);
                map[x][y] = tile;


            }
            position.x += TileSize;
            position.z = -(halfHeight * TileSize);
        }

        mmap = map;
        SetupConnections();


        if (isDungeon)
        {
            roomTiles.Clear();
            ConnectRooms();

            SetupConnections();

            Start = mmap[Start.GridPos.x][Start.GridPos.y];
        }
        else
        {
            Start = mmap[0][0];
        }
    }


    public List<EnvironmentTile> roomTiles = new List<EnvironmentTile>();

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

    public void CleanUpWorld()
    {
        if (mmap != null)
        {
            for (int x = 0; x < mmap.Count; ++x)
            {
                for (int y = 0; y < mmap[x].Count; ++y)
                {
                    Destroy(mmap[x][y].gameObject);
                }
            }
        }
    }


    
    //Dungeon Generation    
    public List<List<EnvironmentTile>> GenerateDungeon(float AccessiblePercentage, Vector2Int Size)
    {
        List<List<EnvironmentTile>> map = new List<List<EnvironmentTile>>();

        map = Generate(map, true, Size, AccessiblePercentage);
       
        map = GenerateRooms(map, Size);

        return map;
    }


    private void ConnectRooms()
    {
        //Setup the connections between rooms via hallways using the Solve() pathfinding function 
        //to set all tiles along the calculated route as a structure and therefore part of the dungeon
        while (connectors.Count > 1)
        {
            List<EnvironmentTile> route = Solve(mmap[connectors[0].GridPos.x][connectors[0].GridPos.y], mmap[connectors[1].GridPos.x][connectors[1].GridPos.y]);

            if (route != null)
            {
                foreach (EnvironmentTile e in route)
                {
                    e.IsAccessible = true;
                    e.IsStructure = true;
                }
            }
            else
            {
                Debug.LogWarning("Cannot create corridor");
            }
            connectors.RemoveAt(0);
        }

        //Iterate through the map to set all non-structure tiles to be inaccessible
        for (int i = 0; i < mmap.Count; i++)
        {
            for (int j = 0; j < mmap[i].Count; j++)
            {
                if(mmap[i][j].IsEntrance)
                {
                    Start = mmap[i][j];
                    Start.GridPos = new Vector2Int(i, j);
                }

                if(mmap[i][j].IsAccessible && !mmap[i][j].IsEntrance && !mmap[i][j].IsChest)
                {
                    roomTiles.Add(mmap[i][j]);
                }

                if (!mmap[i][j].IsStructure)
                {
                    mmap[i][j].IsAccessible = false;
                }

            }
        }
 
    }
    
    List<EnvironmentTile> connectors = new List<EnvironmentTile>();

    //TODO change random numbers range to scale with world space size
    //This function is used when generating dungeons to create the rooms within the interior space
    //and the connections between these rooms
    private List<List<EnvironmentTile>> GenerateRooms(List<List<EnvironmentTile>> map, Vector2Int Size)
    {

        //Randomise the number of rooms present in the dungeon
        //TODO scale the max number with the size of the world space
        int roomCount = 4;// Random.Range(2, 4);

        bool first = false;

        int prevX = Size.x / 2;
        int prevY = Size.y / 2;

        //This for loop will initialise the rooms within the dungeon
        for (int i = 0; i < roomCount; i++)
        {
            int x = Random.Range(Mathf.Clamp(prevX - 20, 0 , Size.x), Mathf.Clamp(prevX + 20, 0, Size.x));
            int y = Random.Range(Mathf.Clamp(prevY - 20, 0, Size.y), Mathf.Clamp(prevY + 20, 0, Size.y));
            int width = Random.Range(5, 15);
            int length = Random.Range(5, 15);
            prevX = x;
            prevY = y;



            

            connectors.Add(map[x][y]);


            for (int j = x - width / 2; j < x + width / 2; j++)
            {
                for (int k = y - length / 2; k < y + length / 2; k++)
                {
                    if (j > 0 && j < Size.x && k > 0 && k < Size.y)
                    {
                        if(!first)
                        {
                            map[j][k].IsEntrance = true;
                            Start = map[j][k];
                            first = true;
                        }
                        map[j][k].IsStructure = true;
                        map[j][k].IsAccessible = true;
                    }
                }
            }

        }

        

        return map;
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
//                Debug.LogFormat("Direct Connection: {0} <-> {1} {2} long", begin, destination, TileSize);
            }
        }
        else
        {
            Debug.Log("Error finding path. Tiles may be null");
        }

        mLastSolution = result;

        return result;
    }
}
