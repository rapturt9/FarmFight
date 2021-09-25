using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class TileHandler : MonoBehaviour
{
    public string Name;

    [SerializeField]
    private Hex selected;

    Dictionary<Hex, TileInterFace> Tiles;

    

    Tilemap tilemap;

    public TileTemp this[Hex hex]
    {
        get
        {
            return Tiles[hex].Tile;
        }

        set
        {
            if (Tiles.ContainsKey(hex))
            {
                
                Tiles[hex].Tile = value;
            }
            
        }
    }

    public void Init(int size)
    {



        TryGetComponent(out tilemap);
        if(tilemap == null)
        {
            gameObject.AddComponent<Tilemap>();
        }

        fillTiles(size);
    }

    

    private void fillTiles(int size)
    {
        //tilemap.ClearAllTiles();
        Dictionary<Hex, TileInterFace> temp = BoardHelperFns.BoardFiller(size);
        Tiles = new Dictionary<Hex, TileInterFace>();
        foreach(var coord in temp.Keys)
        {
            
<<<<<<< Updated upstream
                Tiles[coord] = new TileInterFace(coord,new BasicTile());
=======
                TileDict[coord] = new TileInterFace(coord,new BlankTile());
>>>>>>> Stashed changes
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

        foreach(var tile in Tiles.Values)
        {
            tile.Update();
            if(automaticRedraw)
                tile.Draw(tilemap);
        }
        

    }

    public bool automaticRedraw;
    

    
}
