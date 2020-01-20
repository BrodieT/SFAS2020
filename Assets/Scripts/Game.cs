using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Game : MonoBehaviour
{
    [SerializeField] private Camera MainCamera;
    [SerializeField] private Character Character;
    [SerializeField] private Canvas Menu;
    [SerializeField] private Canvas Hud;
    [SerializeField] private Transform CharacterStart;

    private RaycastHit[] mRaycastHits;
    private Character mCharacter;
    private Environment mMap;

    private readonly int NumberOfRaycastHits = 1;
    public LayerMask mask;

    [SerializeField] GameObject SceneTransition;

    List<List<EnvironmentTile>> map;
    public GameObject DialogueBox;

    public int target = -1;
    void Start()
    {

        mRaycastHits = new RaycastHit[NumberOfRaycastHits];
        mMap = GetComponentInChildren<Environment>();

        mCharacter = Instantiate(Character, transform);
        mCharacter.tag = "Player";
        mCharacter.characterName = "Player";
        ShowMenu(true);

        if (isDungeon)
        {
            ShowMenu(false);
        }

        map = mMap.GenerateWorld(0.85f, new Vector2Int(50, 40));
        CellStorage.cells.Add(new MapData(map, false));


        map = mMap.GenerateDungeon(1.0f, new Vector2Int(50, 40));
        CellStorage.cells.Add(new MapData(map, true));


        CellStorage.previousCellX = 0;
        CellStorage.previousCellY = 0;

        plane.SetActive(true);
        blackPlane.SetActive(false);

        TogglePlayerInventory();

    }

   

    public void ShowDialogueBox(bool show)
    {
        DialogueBox.SetActive(show);

        if (!show)
        {
            target = -1;
        }
    }

    public bool PlayerCaught = false;
    private void Update()
    {
        RefreshInventory();
       
        // Check to see if the player has clicked a tile and if they have, try to find a path to that 
        // tile. If we find a path then the character will move along it to the clicked tile. 
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject(-1))
        {
            Ray screenClick = MainCamera.ScreenPointToRay(Input.mousePosition);
            int hits = Physics.RaycastNonAlloc(screenClick, mRaycastHits);
            if (hits > 0)
            {
                EnvironmentTile tile = mRaycastHits[0].transform.GetComponent<EnvironmentTile>();

                if (tile != null)
                {

                    List<EnvironmentTile> route = new List<EnvironmentTile>();

                    if (tile.IsStructure && !isDungeon)
                    {
                        tile = mMap.mmap[tile.StructureOriginIndex.x][tile.StructureOriginIndex.y];
                        target = tile.gameObject.GetComponent<StructureData>().ID;
                    }

                    if (tile.IsStructure && isDungeon && tile.IsEntrance)
                    {
                        target = 0;
                    }


                    if (tile.IsChest)
                    {
                        ToggleOtherInventoryOn(tile.gameObject);
                        TogglePlayerInventory();
                        tile = mMap.mmap[tile.GridPos.x - 1][tile.GridPos.y];
                    }

                    route = mMap.Solve(mCharacter.CurrentPosition, tile);
                    if (route != null)
                    {
                        mCharacter.GoTo(route);
                    }

                }
            }
        }

        

        if(PlayerCaught && !mCharacter.InCombat)
        {
            List<Character> EnemiesInCombat = new List<Character>();
            foreach(Character e in GetComponent<EnemySpawner>().GetActiveEnemies())
            {
                if(e.gameObject.GetComponent<EnemyController>().DistanceToPlayer < 10)
                {
                    Character c = new Character();
                    c = e;

                    EnemiesInCombat.Add(c);
                    GetComponent<EnemySpawner>().GetActiveEnemies().Remove(e);
                    Destroy(e.gameObject);
                }
            }
            GetComponent<CombatManager>().EngageCombat(EnemiesInCombat);
        }

        if (mCharacter.atDestination && mCharacter.CurrentPosition.IsChest)
        {
            TogglePlayerInventory();
        }

        if (mCharacter.atDestination && mCharacter.CurrentPosition.IsStructure && mCharacter.CurrentPosition.IsEntrance)
        {
            ShowDialogueBox(true);
        }
    }

    public GameObject plane;
    public GameObject blackPlane;


    //Performs the crossfade and loads the cell in the background
    IEnumerator LoadCell(int cellID)
    {
        SceneTransition.GetComponent<Animator>().Play("Crossfade", 0, 0);

        yield return new WaitForSeconds(1.5f);

        ShowDialogueBox(false);

        if (!isDungeon)
        {
            CellStorage.previousCellX = mCharacter.CurrentPosition.GridPos.x;
            CellStorage.previousCellY = mCharacter.CurrentPosition.GridPos.y - mCharacter.CurrentPosition.gameObject.GetComponent<StructureData>().Length;
        }

        mMap.CleanUpWorld();
        isDungeon = CellStorage.cells[cellID].isDungeon;
        mMap.CreateCell(CellStorage.cells[cellID].map, CellStorage.cells[cellID].isDungeon);

        //ReLoad dungeon cell to ensure the room connections generate properly
        //Requires the solve function to generate the corridors and therefore needs to be reloaded to ensure the correct tiles are set to inaccessible
        //More elgant fix would be ideal but as a temporary solution a simple reload will suffice
        if (isDungeon)
        {
            mMap.CleanUpWorld();
            mMap.CreateCell(CellStorage.cells[cellID].map, CellStorage.cells[cellID].isDungeon);
        }

        if (cellID == 0)
        {

            mCharacter.transform.position = mMap.mmap[CellStorage.previousCellX][CellStorage.previousCellY].Position;
            mCharacter.transform.rotation = Quaternion.identity;
            mCharacter.CurrentPosition = mMap.mmap[CellStorage.previousCellX][CellStorage.previousCellY];
            GetComponent<EnemySpawner>().CleanupEnemies();
        }
        else
        {
            mCharacter.transform.position = mMap.Start.Position;
            mCharacter.transform.rotation = Quaternion.identity;
            mCharacter.CurrentPosition = mMap.mmap[mMap.Start.GridPos.x][mMap.Start.GridPos.y +1];
            GetComponent<EnemySpawner>().roomT = mMap.roomTiles;
            GetComponent<EnemySpawner>().Init(mMap.mmap, new Vector2Int(mMap.mmap.Count, mMap.mmap[0].Count));
            GetComponent<EnemySpawner>().SpawnEnemies(5);
        }


        plane.SetActive(!isDungeon);
        blackPlane.SetActive(isDungeon);

        
    }

    //Function called from the confirm button on the dialogue box for travelling to an interior
    public void ConfirmTravel()
    {
        if (target >= 0)
        {
            if (isDungeon)
            {
                StartCoroutine(LoadCell(0));
            }
            else
            {
                StartCoroutine(LoadCell(target));
            }
        }
        else
        {
            Debug.Log("Warning! Invalid destination for travel");
            CancelTravel();
        }
    }

    //Called when clicking the cancel button in the dialogue box for returning the player to just outside the entrance
    public void CancelTravel()
    {
        ShowDialogueBox(false);

        List<EnvironmentTile> route = new List<EnvironmentTile>();
        if (!isDungeon)
        {
            route = mMap.Solve(mCharacter.CurrentPosition, mMap.mmap[mCharacter.CurrentPosition.GridPos.x][mCharacter.CurrentPosition.GridPos.y - mCharacter.CurrentPosition.gameObject.GetComponent<StructureData>().Length]);
        }
        else
        {
            route = mMap.Solve(mCharacter.CurrentPosition, mMap.mmap[mCharacter.CurrentPosition.GridPos.x][mCharacter.CurrentPosition.GridPos.y + 1]);
        }

        if (route != null)
        {
            mCharacter.GoTo(route);
        }

    }


    public void ShowMenu(bool show)
    {
        if (Menu != null && Hud != null)
        {
            Menu.enabled = show;
            Hud.enabled = !show;

            if (show)
            {
                mCharacter.transform.position = CharacterStart.position;
                mCharacter.transform.rotation = CharacterStart.rotation;
            }
            else
            {
                mCharacter.transform.position = mMap.Start.Position;
                mCharacter.transform.rotation = Quaternion.identity;

                mCharacter.CurrentPosition = mMap.Start;

                mCharacter.gameObject.GetComponent<Inventory>().manager = GetComponent<ItemManager>();
                mCharacter.gameObject.GetComponent<Inventory>().AddItem(0);
                mCharacter.gameObject.GetComponent<Inventory>().AddItem(1);

                //TogglePlayerInventory();
            }
        }
    }

    public GameObject playerInventory;
    public GameObject otherInventory;
    public GameObject inventoryCell;

    public void ToggleOtherInventoryOn(GameObject container)
    {
        otherInventory.gameObject.SetActive(true);

        List<Item> items = container.GetComponent<Inventory>().inventory;

        foreach (Item i in items)
        {
            GameObject g = Instantiate(inventoryCell, otherInventory.transform.GetChild(0).GetChild(0));
            g.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = " " + i.itemName;
            g.GetComponent<ItemStore>().SetContainer(container);
            g.GetComponent<ItemStore>().playerOwned = false;

            g.transform.GetChild(1).gameObject.SetActive(false);
            container.GetComponent<Inventory>().gridcells.Add(g);

        }
        cont = container;
    }


    GameObject cont;
    public void RefreshInventory()
    {
        if (playerInventory.activeSelf)
        {
            for (int i = playerInventory.transform.GetChild(0).GetChild(0).transform.childCount; i < mCharacter.gameObject.GetComponent<Inventory>().inventory.Count; i++)
            {
                GameObject g = Instantiate(inventoryCell, playerInventory.transform.GetChild(0).GetChild(0));
                g.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = " " + mCharacter.gameObject.GetComponent<Inventory>().inventory[i].itemName;
                g.GetComponent<ItemStore>().playerOwned = true;
                mCharacter.GetComponent<Inventory>().gridcells.Add(g);
            }
        }
        //TODO 
        //get the container obj and repeat above step for other inventory
        //call this function from add item
        if (otherInventory.activeSelf && cont != null)
        {
            Inventory inv = cont.GetComponent<Inventory>();


            for (int i = otherInventory.transform.GetChild(0).GetChild(0).transform.childCount; i < inv.inventory.Count; i++)
            {
                GameObject g = Instantiate(inventoryCell, otherInventory.transform.GetChild(0).GetChild(0));
                g.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = " " + inv.inventory[i].itemName;
                g.GetComponent<ItemStore>().playerOwned = false;
                inv.gridcells.Add(g);
            }
        }
    }

    public void ToggleOtherInventoryOff()
    {
        foreach (Transform g in otherInventory.transform.GetChild(0).GetChild(0).transform)
        {
            Destroy(g.gameObject);
        }

        otherInventory.gameObject.SetActive(false);

    }

    public void TogglePlayerInventory()
    {
        bool show = !playerInventory.gameObject.activeSelf;
        playerInventory.gameObject.SetActive(show);


        if(show)
        {

            List<Item> items = mCharacter.gameObject.GetComponent<Inventory>().inventory;
            
            foreach(Item i in items)
            {
                GameObject g = Instantiate(inventoryCell, playerInventory.transform.GetChild(0).GetChild(0));
                g.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = " " + i.itemName + " x" + i.quantity;
                g.GetComponent<ItemStore>().playerOwned = true;
                mCharacter.GetComponent<Inventory>().gridcells.Add(g);

            }
        }
        else
        {
            foreach(Transform g in playerInventory.transform.GetChild(0).GetChild(0).transform)
            {
                Destroy(g.gameObject);
            }
        }
    }
    public bool isDungeon = false;
    public void Generate()
    {
        if (isDungeon)
        {
            mMap.CreateCell(CellStorage.cells[1].map, true);
        }
        else
        {
            mMap.CreateCell(CellStorage.cells[0].map, false) ;
        }

        ShowMenu(false);
    }
    public void Exit()
    {
#if !UNITY_EDITOR
        Application.Quit();
#endif
    }
}
