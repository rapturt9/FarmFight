using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using MLAPI;

public class TileHandler : NetworkBehaviour
{
    public string Name;

    [SerializeField]
    private Hex selected;

    public Dictionary<Hex, TileInterFace> TileDict;

    private int size;

    Tilemap tilemap;
    public GameManager gameManager;

    public TileTemp this[Hex hex]
    {
        get
        {
            return TileDict[hex].Tile;
        }

        set
        {
            if (TileDict.ContainsKey(hex))
            {
                
                TileDict[hex].Tile = value;
            }
            
        }
    }

    public void Init(int size)
    {

        this.size = size;

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
        TileDict = new Dictionary<Hex, TileInterFace>();
        foreach(var coord in temp.Keys)
        {
            TileDict[coord] = new TileInterFace(coord,new BlankTile());
        }

        Redraw();
    }

    private void Redraw()
    {
        foreach(var tile in TileDict.Values)
        {
            tile.Draw(tilemap);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.gameIsRunning) { return; }

        foreach(var tile in TileDict.Values)
        {
            tile.Update();
            if(automaticRedraw)
                tile.Draw(tilemap);
        }
        

    }

    public bool automaticRedraw;


    public void clearMap()
    {
        fillTiles(size);
    }

    
}
