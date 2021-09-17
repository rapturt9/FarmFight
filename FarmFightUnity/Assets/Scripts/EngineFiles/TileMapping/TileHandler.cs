using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class TileHandler : MonoBehaviour
{
    public string Name;

    [SerializeField]
    public Hex selected;

    Dictionary<Hex, GameTile> Tiles;

    

    Tilemap tilemap;

   

    public void Init()
    {



        TryGetComponent(out tilemap);
        if(tilemap == null)
        {
            gameObject.AddComponent<Tilemap>();
        }

        fillTiles();
    }

    

    private void fillTiles()
    {
        Dictionary<Hex,GameTile> temp = BoardHelperFns.BoardFiller(4);
        Tiles = new Dictionary<Hex, GameTile>();
        foreach(var coord in temp.Keys)
        {
            
                Tiles[coord] = new GameTile(coord, new SelectBehavior());
        }

        Redraw();
    }

    private void Redraw()
    {
        foreach(var tile in Tiles.Values)
        {
            tile.Draw(tilemap);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Tiles[selected] = new GameTile(selected, new SelectBehavior());

            selected = Hex.fromWorld(Camera.main.ScreenToWorldPoint(Input.mousePosition), tilemap.gameObject.transform.lossyScale.y);

            if (selected != null)
            {

                Tiles[selected] = new GameTile(selected, new BasicBehavior());
                
                
            }
            
            
            
        }

        if (Input.GetKeyUp(KeyCode.R))
            Init();

        Redraw();

    }
}
