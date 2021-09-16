using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class TileHandler : MonoBehaviour
{


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
            
                Tiles[coord] = new GameTile(coord, new BasicBehavior());
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

    }
}
