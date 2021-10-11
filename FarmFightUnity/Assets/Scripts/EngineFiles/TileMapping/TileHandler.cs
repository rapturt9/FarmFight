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
                    SyncTile(hex);
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
    public void SyncTile(Hex coord)
    {
        // Gets tile data and soldier data
        TileSyncData tileData = GameState.SerializeTile(this[coord]);
        List<Soldier> soldiersToSync = new List<Soldier>(this[coord].soldiers);
        GameObject farmerToSync = this[coord].farmerObj;

        if (IsClient)
        {
            SyncTileServerRpc(BoardHelperFns.HexToArray(coord), tileData);
        }
        else if (IsServer)
        {
            SyncTileClientRpc(BoardHelperFns.HexToArray(coord), tileData);
        }

        // Re-syncs the soldiers to the new tile
        foreach (var soldier in soldiersToSync)
        {
            soldier.AddToTile(coord);
        }
        // Re-syncs the farmers to the new tile
        if (!(farmerToSync is null))
        {
            farmerToSync.GetComponent<Farmer>().AddToTile(coord);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SyncTileServerRpc(int[] coord, TileSyncData tileData)
    {
        _SyncTile(coord, tileData);
        SyncTileClientRpc(coord, tileData);
    }

    [ClientRpc]
    void SyncTileClientRpc(int[] coord, TileSyncData tileData)
    {
        _SyncTile(coord, tileData);
    }

    // Internal function, actually changes the tile
    void _SyncTile(int[] coordArray, TileSyncData tileData)
    {
        Hex coord = BoardHelperFns.ArrayToHex(coordArray);
        TileTemp tile = GameState.DeserializeTile(tileData);
        if (TileDict.ContainsKey(coord))
        {
            TileDict[coord].Tile = tile;
        }
    }
}
