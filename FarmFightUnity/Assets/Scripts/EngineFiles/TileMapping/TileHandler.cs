using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using MLAPI;
using MLAPI.Messaging;

public class TileHandler : NetworkBehaviour
{
    public string Name;
    public bool syncsTiles = false;

    [SerializeField]
    private Hex selected;

    public Dictionary<Hex, TileInterFace> TileDict;

    private int size;

    Tilemap tilemap;
    public GameManager gameManager;
    private TileSyncer syncer;

    private void Awake()
    {
        TileDict = new Dictionary<Hex, TileInterFace>();
    }

    private void Start()
    {
        if (TryGetComponent(out syncer))
        {
            syncer.Init(this);
        }
    }

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
                // Only sync some tiles (crops) and not others (UI)
                if (syncsTiles)
                    SyncTileOverwrite(hex);
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


    // We have changed a tile somehow, so it gets synced to everyone
    // Only works on TileTemp
    public void SyncTileOverwrite(Hex coord)
    {
        syncer.SyncTileOverwrite(coord);
    }

    // Only sync some data, don't overwrite whole tile
    public void SyncTileUpdate(Hex coord, CropTileSyncTypes[] dataToSync)
    {
        syncer.SyncTileUpdate(coord, dataToSync);
    }
}
