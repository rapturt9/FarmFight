using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class TileHandler : MonoBehaviour
{
    [SerializeField]
    public Hex selected;


    Dictionary<Hex, GameTile> Tiles;

    Tilemap tilemap;

    public void Start()
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
        Dictionary<Hex,GameTile> temp = BoardHelperFns.BoardFiller(5);
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

            selected = Hex.fromWorld(Camera.main.ScreenToWorldPoint(Input.mousePosition), tilemap.gameObject.transform.localScale.y);

            if (selected != null)
            {
                Tiles[selected] = new GameTile(selected, new BasicBehavior());
                
                
            }
            
            
            
        }

        if (Input.GetKeyUp(KeyCode.R))
            Start();

        Redraw();

    }
}
