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

    //This game object contains the animated fade transition
    [SerializeField] GameObject SceneTransition;

    //This nested list contains the current map of environment tiles
    List<List<EnvironmentTile>> map;

    //This gameobject is the dialogue box that will appear onscreen to ensure the player
    //wishes to travel to a different area (e.g. an interior space like a dungeon).
    public GameObject DialogueBox;

    //This canvas represents the trade window and holds the UI
    //for allowing the player to swap/trade items with a container/merchant
    public Canvas TradeWindow;

    //This canvas holds the player menu which contains the players own inventory, quest log, and pause menu
    public Canvas PlayerMenu;
    //These objects represents the conent object within the inventory UI where the item icons will be
    //Below is the item description window which will provide further information about the items in the players inventory
    public GameObject InventoryContent;
    public GameObject ItemDesc;

    //These objects represent the backdrop to the tile-based world, 
    //to disguise the edges of the world better
    public GameObject plane;
    public GameObject blackPlane;
    
    //This bool tracks if an enemy has caught the 
    //player to determine if combat should be initiated
    public bool PlayerCaught = false;

    public int target = -1;


    public Sprite emptySlot;
    public bool isDungeon = false;



    void Start()
    {

        mRaycastHits = new RaycastHit[NumberOfRaycastHits];
        mMap = GetComponentInChildren<Environment>();

        mCharacter = Instantiate(Character, transform);
        mCharacter.tag = "Player";
        mCharacter.characterName = "Player";
        mCharacter.gameObject.GetComponent<Inventory>().SetCarryCapacity(InventoryContent.transform.childCount);
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

        //TogglePlayerInventory();
        ShowPlayerMenu(false);
    }
    private void Update()
    {
        //Debug.Log(mCharacter.damage + ", " + mCharacter.dmgThreshold);
       
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
                        //ToggleOtherInventoryOn(tile.gameObject);
                        //TogglePlayerInventory();
                        tile = mMap.mmap[tile.GridPos.x][tile.GridPos.y - 1];
                    }

                    route = mMap.Solve(mCharacter.CurrentPosition, tile);
                    if (route != null)
                    {
                        mCharacter.GoTo(route);
                    }

                }
            }
        }

        //DEBUG////////////
        //if(Input.GetKeyDown(KeyCode.A))
        //{
        //    mCharacter.gameObject.GetComponent<Inventory>().AddItem(Random.Range(0, 4), 1);
        //}

        //if (Input.GetKeyDown(KeyCode.D))
        //{
        //    //mCharacter.gameObject.GetComponent<Inventory>().RemoveItem(2);
        //}     
        //////////////////

        if (PlayerCaught && !mCharacter.InCombat)
        {
            List<Character> EnemiesInCombat = new List<Character>();
            foreach(Character e in GetComponent<EnemySpawner>().GetActiveEnemies())
            {
                if(e.gameObject.GetComponent<EnemyController>().myState == EnemyState.CAUGHT)
                {                    
                    EnemiesInCombat.Add(e);
                    e.gameObject.SetActive(false);
                    break;
                }
            }
            if (EnemiesInCombat.Count > 0)
            {


                GetComponent<CombatManager>().EngageCombat(EnemiesInCombat);
                mCharacter.InCombat = true;
                PlayerCaught = false;
            }
        }

        if (mCharacter.atDestination && mMap.mmap[mCharacter.CurrentPosition.GridPos.x][mCharacter.CurrentPosition.GridPos.y + 1].IsChest)
        {
            if (!TradeWindow.gameObject.activeSelf)
            {
                ShowTradeWindow(true, mMap.mmap[mCharacter.CurrentPosition.GridPos.x][mCharacter.CurrentPosition.GridPos.y + 1].gameObject);
            }
        }

        if (mCharacter.atDestination && mCharacter.CurrentPosition.IsStructure && mCharacter.CurrentPosition.IsEntrance)
        {
            ShowDialogueBox(true);
        }
    }



    //UI MANAGER FUNCTIONS
    public void ShowDialogueBox(bool show)
    {
        DialogueBox.SetActive(show);

        if (!show)
        {
            target = -1;
        }
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

    //Coroutine for playing the transition outwith the main game loop
    IEnumerator PlayTransition()
    {
        SceneTransition.GetComponent<Animator>().Play("Crossfade", 0, 0);

        yield return new WaitForSeconds(2);
    }

    //This function toggles the main menu and HUD accordingly
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
                StartCoroutine(PlayTransition());
                mCharacter.transform.position = mMap.Start.Position;
                mCharacter.transform.rotation = Quaternion.identity;

                mCharacter.CurrentPosition = mMap.Start;

                mCharacter.gameObject.GetComponent<Inventory>().manager = GetComponent<ItemManager>();
                mCharacter.gameObject.GetComponent<Inventory>().AddItem(0, 1);
                mCharacter.gameObject.GetComponent<Inventory>().AddItem(1, 1);

                //TogglePlayerInventory();
            }
        }
    }

    //This function toggles the player menu, a tabbed menu featuring the player's inventory, the quest log, and the pause menu.
    public void ShowPlayerMenu(bool show)
    {
        Hud.enabled = !show;
        PlayerMenu.transform.gameObject.SetActive(show);

        if (show)
        {
            List<Item> items = mCharacter.gameObject.GetComponent<Inventory>().inventory;

            if (items.Count <= InventoryContent.transform.childCount)
            {
                for (int i = 0; i < InventoryContent.transform.childCount; i++)
                {
                    if (items.Count > i)
                    {
                        InventoryContent.transform.GetChild(i).GetComponent<Image>().sprite = items[i].icon;
                        InventoryContent.transform.GetChild(i).GetComponent<ItemStore>().setItem(items[i]);
                        if (items[i].quantity > 1)
                        {
                            InventoryContent.transform.GetChild(i).GetChild(0).gameObject.SetActive(true);
                            InventoryContent.transform.GetChild(i).GetChild(0).GetComponent<Text>().text = "x" + items[i].quantity;

                        }
                        ItemStore itemS = new ItemStore();
                        itemS.setItem(items[i]);
                        InventoryContent.transform.GetChild(i).GetComponent<Button>().onClick.AddListener(() => ShowItemDescription(true, itemS));

                    }
                    else
                    {
                        InventoryContent.transform.GetChild(i).GetComponent<Button>().onClick.RemoveAllListeners();
                    }
                }
            }
            else
            {
                Debug.Log("Too many items");
            }
        }
        else
        {
            foreach (Transform g in InventoryContent.transform)
            {
                g.GetComponent<Image>().sprite = emptySlot;
                if(g.GetChild(0).gameObject.activeSelf)
                {
                    g.GetChild(0).gameObject.SetActive(false);
                }
            }


        }
    }

    public void DropItem()
    {
        ShowItemDescription(false);
        ShowPlayerMenu(false);
        mCharacter.GetComponent<Inventory>().RemoveItem(ItemDesc.transform.GetChild(0).GetComponent<ItemStore>().ID);
        ShowPlayerMenu(true);
    }

    public void UseItem()
    {
        ShowItemDescription(false);
        ShowPlayerMenu(false);
        mCharacter.GetComponent<Inventory>().Equip(ItemDesc.transform.GetChild(0).GetComponent<ItemStore>());
        ShowPlayerMenu(true);
    }

    GameObject currentContainer;

    public void CloseTradeWindow()
    {
        GameObject playerContent = TradeWindow.transform.GetChild(1).GetChild(0).GetChild(0).gameObject;
        GameObject otherContent = TradeWindow.transform.GetChild(2).GetChild(0).GetChild(0).gameObject;

        TradeWindow.transform.GetChild(1).GetChild(1).GetComponent<Scrollbar>().value = 1;
        TradeWindow.transform.GetChild(2).GetChild(1).GetComponent<Scrollbar>().value = 1;

        currentContainer = null;

        foreach (Transform g in playerContent.transform)
        {
            g.GetComponent<Image>().sprite = emptySlot;
            if (g.GetChild(0).gameObject.activeSelf)
            {
                g.GetChild(0).gameObject.SetActive(false);
            }
        }

        foreach (Transform g in otherContent.transform)
        {
            g.GetComponent<Image>().sprite = emptySlot;
            if (g.GetChild(0).gameObject.activeSelf)
            {
                g.GetChild(0).gameObject.SetActive(false);
            }
        }

        List<EnvironmentTile> route = new List<EnvironmentTile>();
        if (mCharacter.CurrentPosition.GridPos.y - 1 >= 0)
        {
            route = mMap.Solve(mCharacter.CurrentPosition, mMap.mmap[mCharacter.CurrentPosition.GridPos.x][mCharacter.CurrentPosition.GridPos.y - 1]);

        }
        else if(mCharacter.CurrentPosition.GridPos.x - 1 >= 0)
        {
            route = mMap.Solve(mCharacter.CurrentPosition, mMap.mmap[mCharacter.CurrentPosition.GridPos.x -1][mCharacter.CurrentPosition.GridPos.y]);
        }
        else if(mCharacter.CurrentPosition.GridPos.x + 1 <= mMap.mmap.Count -1)
        {
            route = mMap.Solve(mCharacter.CurrentPosition, mMap.mmap[mCharacter.CurrentPosition.GridPos.x + 1][mCharacter.CurrentPosition.GridPos.y]);
        }
        else
        {
            Debug.LogError("Invalid path. Unable to exit trade");
        }


        if (route != null)
        {
            mCharacter.GoTo(route);
        }

        TradeWindow.gameObject.SetActive(false);

    }

    //This function toggles the trade window on/off when the player interacts with a container/merchant
    //It loops through each of the UI slots and assigns them an item from their inventory which is used to visually represent the inventory
    public void ShowTradeWindow(bool show, GameObject container)
    {
        
        GameObject playerContent = TradeWindow.transform.GetChild(1).GetChild(0).GetChild(0).gameObject;
        GameObject otherContent = TradeWindow.transform.GetChild(2).GetChild(0).GetChild(0).gameObject;
        
        TradeWindow.transform.GetChild(1).GetChild(1).GetComponent<Scrollbar>().value = 1;
        TradeWindow.transform.GetChild(2).GetChild(1).GetComponent<Scrollbar>().value = 1;

        if (show)
        {
            currentContainer = container;

            TradeWindow.gameObject.SetActive(true);

            if (container.GetComponent<Inventory>().isContainer)
            {
                TradeWindow.transform.GetChild(2).GetChild(2).GetChild(0).GetComponent<Text>().text = "Chest";
            }
            else
            {
                TradeWindow.transform.GetChild(2).GetChild(2).GetChild(0).GetComponent<Text>().text = "Merchant";
            }



            List<Item> playerItems = mCharacter.gameObject.GetComponent<Inventory>().inventory;

            if (playerItems.Count <= playerContent.transform.childCount)
            {
                for (int i = 0; i < playerContent.transform.childCount; i++)
                {
                    if (playerItems.Count > i)
                    {
                        playerContent.transform.GetChild(i).GetComponent<Image>().sprite = playerItems[i].icon;
                        playerContent.transform.GetChild(i).GetComponent<ItemStore>().setItem(playerItems[i]);
                        if (playerItems[i].quantity > 1)
                        {
                            playerContent.transform.GetChild(i).GetChild(0).gameObject.SetActive(true);
                            playerContent.transform.GetChild(i).GetChild(0).GetComponent<Text>().text = "x" + playerItems[i].quantity;
                        }
                    }
                    else
                    {
                        playerContent.transform.GetChild(i).GetComponent<ItemStore>().ID = 99999;
                    }
                }

            }

            if (container != null)
            {
                List<Item> otherItems = container.GetComponent<Inventory>().inventory;

                if (otherItems.Count <= otherContent.transform.childCount)
                {
                    for (int i = 0; i < otherContent.transform.childCount; i++)
                    {
                        if (otherItems.Count > i)
                        {
                            otherContent.transform.GetChild(i).GetComponent<Image>().sprite = otherItems[i].icon;
                            otherContent.transform.GetChild(i).GetComponent<ItemStore>().setItem(otherItems[i]);
                            if (otherItems[i].quantity > 1)
                            {
                                otherContent.transform.GetChild(i).GetChild(0).gameObject.SetActive(true);
                                otherContent.transform.GetChild(i).GetChild(0).GetComponent<Text>().text = "x" + otherItems[i].quantity;
                            }
                        }
                        else
                        {
                            otherContent.transform.GetChild(i).GetComponent<ItemStore>().ID = 99999;
                        }
                    }
                }

            }
            else
            {
                Debug.LogError("Container is NULL");

            }


        }
        else
        {

            foreach (Transform g in playerContent.transform)
            {
                g.GetComponent<Image>().sprite = emptySlot;
                if (g.GetChild(0).gameObject.activeSelf)
                {
                    g.GetChild(0).gameObject.SetActive(false);
                }
            }

            foreach (Transform g in otherContent.transform)
            {
                g.GetComponent<Image>().sprite = emptySlot;
                if (g.GetChild(0).gameObject.activeSelf)
                {
                    g.GetChild(0).gameObject.SetActive(false);
                }
            }
            TradeWindow.gameObject.SetActive(false);

        }
    }


    //This function handles an item from the container's inventory UI being transferred into the player's inventory
    public void TransferToPlayer(ItemStore item)
    {
        if (currentContainer != null)
        {
            //If the item is successfully added to the player then it is safe to remove it from the container
            if (mCharacter.GetComponent<Inventory>().AddItem(item.ID, 1))
            {
                currentContainer.GetComponent<Inventory>().RemoveItem(item.ID);

                ShowTradeWindow(false, null);
                ShowTradeWindow(true, currentContainer);
            }
            else
            {
                Debug.Log("Transfer Unsuccessful");
            }
        }
        else
        {
            Debug.LogError("Invalid Container");
        }
    }

    //This function handles an item from the player's inventory UI being transferred into the container's inventory
    public void TransferToContainer(ItemStore item)
    {
        if (currentContainer != null)
        {
            //If the item is successfully added to the container then it is safe to remove it from the player
            if (currentContainer.GetComponent<Inventory>().AddItem(item.ID, 1))
            {
                mCharacter.GetComponent<Inventory>().RemoveItem(item.ID);

                ShowTradeWindow(false, null);
                ShowTradeWindow(true, currentContainer);
            }
            else
            {
                Debug.Log("Transfer Unsuccessful");
            }
        }
        else
        {
            Debug.LogError("Invalid Container");
        }
    }


    //This function is no longer being used but could still prove useful if a better solution to the item trading problem is found
    public void TransferItem(ItemStore i, GameObject a, GameObject b)
    {
        if (a.GetComponent<Inventory>().isContainer || a.GetComponent<Inventory>().isMerchant)
        {
            CloseTradeWindow();
      
            a.GetComponent<Inventory>().inventory.Remove(mCharacter.GetComponent<Inventory>().manager.AllItems[i.ID]);// RemoveItem(i.ID);
            b.GetComponent<Inventory>().inventory.Add(mCharacter.GetComponent<Inventory>().manager.AllItems[i.ID]);// (i.ID, 1);
            //ShowTradeWindow(true, a);
        }
        else
        {
            CloseTradeWindow();
            a.GetComponent<Inventory>().inventory.Remove(mCharacter.GetComponent<Inventory>().manager.AllItems[i.ID]);// RemoveItem(i.ID);
            b.GetComponent<Inventory>().inventory.Add(mCharacter.GetComponent<Inventory>().manager.AllItems[i.ID]);// (i.ID, 1);

            //a.GetComponent<Inventory>().RemoveItem(i.ID);
            //b.GetComponent<Inventory>().AddItem(i.ID, 1);
            //ShowTradeWindow(true, b);
        }
    }

    //This function is not currently being used but its purpose will be to transfer all items in a container's inventory into the player's  
    public void TakeAll()
    {
        if (currentContainer != null)
        {
            foreach (Item i in currentContainer.GetComponent<Inventory>().inventory)
            {
                if (mCharacter.GetComponent<Inventory>().AddItem(i.id, 1))
                {
                    currentContainer.GetComponent<Inventory>().RemoveItem(i.id);
                }
            }
        }
        else
        {
            Debug.LogError("Invalid Container");
        }
    }
 

    //This function toggles the item description window in the player's inventory
    public void ShowItemDescription(bool show, ItemStore i)
    {
        if(show)
        {
            ItemDesc.SetActive(true);
            ItemDesc.transform.GetChild(0).GetComponent<ItemStore>().setItem(i);
            ItemDesc.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = i.itemName;
            ItemDesc.transform.GetChild(0).GetChild(1).GetComponent<Text>().text = i.Description;
            ItemDesc.transform.GetChild(0).GetChild(2).transform.gameObject.SetActive(true);
            ItemDesc.transform.GetChild(0).GetChild(3).transform.gameObject.SetActive(true);

            if (i.type == ItemType.QUESTITEM)
            {
                ItemDesc.transform.GetChild(0).GetChild(3).transform.gameObject.SetActive(false);
            }

            if (i.type == ItemType.WEAPON || i.type == ItemType.ARMOUR)
            {
                if (i.isEquipped)
                {
                    ItemDesc.transform.GetChild(0).GetChild(2).GetChild(0).GetComponent<Text>().text = "Unequip";
                }
                else
                {
                    ItemDesc.transform.GetChild(0).GetChild(2).GetChild(0).GetComponent<Text>().text = "Equip";
                }
            }
            else if(i.type == ItemType.CONSUMABLE)
            {
                ItemDesc.transform.GetChild(0).GetChild(2).GetChild(0).GetComponent<Text>().text = "Drink";
            }
            else
            {
                ItemDesc.transform.GetChild(0).GetChild(2).transform.gameObject.SetActive(false);
            }
        }
        else
        {
            ItemDesc.SetActive(false);
        }
    }

    //Overload function for disabling the item description menu without the need for itemstore data
    public void ShowItemDescription(bool show)
    {
        if (show)
        {
            ItemDesc.SetActive(true);
            ItemDesc.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = "Item Name";
            ItemDesc.transform.GetChild(0).GetChild(1).GetComponent<Text>().text = "Item Description";
        }
        else
        {
            ItemDesc.SetActive(false);
        }
    }

    //Performs the fade and loads the cell in the background
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

    //This function Generates the current cell
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
