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


    List<List<EnvironmentTile>> map;

    void Start()
    {

        mRaycastHits = new RaycastHit[NumberOfRaycastHits];
        mMap = GetComponentInChildren<Environment>();
       
        mCharacter = Instantiate(Character, transform);
        mCharacter.tag = "Player";
        ShowMenu(true);

        if(isDungeon)
        {
            ShowMenu(false);
        }

        map = mMap.GenerateWorld(0.85f, new Vector2Int(50, 40));
        CellStorage.cells.Add(new MapData(map, null));


        map = mMap.GenerateDungeon(1.0f, new Vector2Int(50, 40));
        CellStorage.cells.Add(new MapData(map, null));





    }

    public GameObject dialogue;
    public int target;
    
    public void ShowDialogueBox(bool show)
    {
        if (target > 0)
        {
            dialogue.SetActive(show);
            //dialogue.text +=  cell[target].name
            if (!show)
            {
                target = 0;
            }
        }
    }

    private void Update()
    {
        // Check to see if the player has clicked a tile and if they have, try to find a path to that 
        // tile. If we find a path then the character will move along it to the clicked tile. 
        if(Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject(-1))
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
                        target = tile.GetTargetID();
                    }

                    Debug.Log("CurrentPos = " + mCharacter.CurrentPosition.GridPos);

                    route = mMap.Solve(mCharacter.CurrentPosition, tile);
                    if (route != null)
                    {
                        mCharacter.GoTo(route);
                    }

                }
            }
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

    public void Go()
    {
        mMap.CleanUpWorld();
        mMap.CreateCell(CellStorage.cells[1].map, true);
       
        mCharacter.transform.position = mMap.Start.Position;
        mCharacter.transform.rotation = Quaternion.identity;
        mCharacter.CurrentPosition = mMap.Start;
               
        ShowDialogueBox(false);
        isDungeon = true;

    }
    bool firsttime = false;
    public void ShowMenu(bool show)
    {
        if (Menu != null && Hud != null)
        {
            Menu.enabled = show;
            Hud.enabled = !show;

            if( show )
            {
                mCharacter.transform.position = CharacterStart.position;
                mCharacter.transform.rotation = CharacterStart.rotation;
                //mMap.CleanUpWorld(new Vector2Int(50,40));


               
            }
            else
            {
                mCharacter.transform.position = mMap.Start.Position;
                mCharacter.transform.rotation = Quaternion.identity;
                if (!firsttime)
                {
                    mCharacter.CurrentPosition = mMap.mmap[0][0];
                    firsttime = true;
                }
                mCharacter.gameObject.GetComponent<Inventory>().manager = GetComponent<ItemManager>();
                container = mCharacter.gameObject;
                container.GetComponent<Inventory>().AddItem(0);
                container.GetComponent<Inventory>().AddItem(1);

                //TogglePlayerInventory();
            }
        }
    }

    public Canvas inv;
    public GameObject container;
    public void TogglePlayerInventory()
    {
        bool show = !inv.gameObject.activeSelf;
        inv.gameObject.SetActive(show);


        if(show && container != null)
        {
            //Image[] cells = inv.gameObject.transform.GetChild(0).GetChild(1).GetComponentsInChildren<Image>();
            //List<Item> items = container.GetComponent<Inventory>().inventory;
            //if (cells.Length > 0)
            //{
            //   for(int i = 0; i < items.Count; i++)
            //    {
            //        //if (i < cells.Length)
            //        //{
            //        //    cells[i + 1].sprite = items[i].icon;

            //        //    if(items[i].quantity > 1)
            //        //    {
            //        //        cells[i + 1].transform.GetChild(0).gameObject.SetActive(true);
            //        //        cells[i + 1].transform.GetChild(0).gameObject.GetComponent<Text>().text = "x" + items[i].quantity;
            //        //    }
            //        //}
            //    }
            //}
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
