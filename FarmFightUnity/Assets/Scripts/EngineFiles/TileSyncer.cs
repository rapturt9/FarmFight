using MLAPI;
using MLAPI.Messaging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileSyncer : NetworkBehaviour
{
    private TileHandler handler;

    public static TileSyncer Syncer;

    public void Awake()
    {
        if (Syncer == null)
        {
            Syncer = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Init(TileHandler handler)
    {
        this.handler = handler;
    }

    // Updating only some data
    public void SyncTileUpdate(Hex coord, CropTileSyncTypes[] dataToSync)
    {
        TileSyncData tileData = GameState.SerializeTile(handler[coord]);

        if (IsClient)
        {
            SyncTileUpdateServerRpc(BoardHelperFns.HexToArray(coord), tileData, dataToSync);
        }
        else if (IsServer)
        {
            SyncTileUpdateClientRpc(BoardHelperFns.HexToArray(coord), tileData, dataToSync);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SyncTileUpdateServerRpc(int[] coord, TileSyncData tileData, CropTileSyncTypes[] dataToSync)
    {
        _SyncTileUpdate(coord, tileData, dataToSync);
        SyncTileUpdateClientRpc(coord, tileData, dataToSync);
    }

    [ClientRpc]
    void SyncTileUpdateClientRpc(int[] coord, TileSyncData tileData, CropTileSyncTypes[] dataToSync)
    {
        if (!IsServer)
        {
            _SyncTileUpdate(coord, tileData, dataToSync);
        }
    }

    // Internal function, actually changes the tile
    void _SyncTileUpdate(int[] coordArray, TileSyncData tileData, CropTileSyncTypes[] dataToSync)
    {
        Hex coord = BoardHelperFns.ArrayToHex(coordArray);
        TileTemp oldTile = handler[coord];

        // cropNum
        if (Array.Exists(dataToSync, element => element == CropTileSyncTypes.cropNum))
        {
            _SyncTileUpdateCropType(coord, tileData, ref oldTile);
        }
        // lastPlanted
        if (Array.Exists(dataToSync, element => element == CropTileSyncTypes.lastPlanted))
        {
            _SyncTileUpdateLastPlanted(tileData, ref oldTile);
        }
        // containsFarmer
        if (Array.Exists(dataToSync, element => element == CropTileSyncTypes.containsFarmer))
        {
            _SyncTileUpdateFarmer(tileData, ref oldTile);
        }
        // tileOwner
        if (Array.Exists(dataToSync, element => element == CropTileSyncTypes.tileOwner))
        {
            _SyncTileUpdateTileOwner(tileData, ref oldTile);
        }
        // battleOccurring
        if (Array.Exists(dataToSync, element => element == CropTileSyncTypes.battleOccurring))
        {
            _SyncTileUpdateBattle(tileData, ref oldTile);
        }

        // tileDamage
        if (Array.Exists(dataToSync, element => element == CropTileSyncTypes.tileDamage))
        {
            _SyncTileUpdateTileDamage(tileData, ref oldTile);
        }
    }

    // We have changed a tile somehow, so it gets synced to everyone
    // Only works on TileTemp
    public void SyncTileOverwrite(Hex coord)
    {
        // Gets tile data and soldier data
        TileSyncData tileData = GameState.SerializeTile(handler[coord]);
        List<Soldier> soldiersToSync = new List<Soldier>(handler[coord].getSoldierEnumerator());
        GameObject farmerToSync;
        if (handler[coord].farmerObj == null)
            farmerToSync = null;
        else
            farmerToSync = handler[coord].farmerObj.gameObject;
        GameObject battlecloudToSync = handler[coord].battleCloud;

        if (IsClient)
        {
            SyncTileOverwriteServerRpc(BoardHelperFns.HexToArray(coord), tileData);
        }
        else if (IsServer)
        {
            SyncTileOverwriteClientRpc(BoardHelperFns.HexToArray(coord), tileData);
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
        // Re-syncs the battle cloud to the new tile
        if (!(battlecloudToSync == null))
        {
            battlecloudToSync.GetComponent<BattleCloud>().AddToTile(coord);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SyncTileOverwriteServerRpc(int[] coord, TileSyncData tileData)
    {
        _SyncTileOverwrite(coord, tileData);
        SyncTileOverwriteClientRpc(coord, tileData);
    }

    [ClientRpc]
    void SyncTileOverwriteClientRpc(int[] coord, TileSyncData tileData)
    {
        if (!IsServer)
        {
            _SyncTileOverwrite(coord, tileData);
        }
    }

    // Internal function, actually changes the tile
    void _SyncTileOverwrite(int[] coordArray, TileSyncData tileData)
    {
        Hex coord = BoardHelperFns.ArrayToHex(coordArray);
        TileTemp tile = GameState.DeserializeTile(tileData);
        if (handler.TileDict.ContainsKey(coord))
        {
            handler.TileDict[coord].Tile = tile;
        }
    }


    // Syncing individual parts
    public void _SyncTileUpdateCropType(Hex coord, TileSyncData tileData, ref TileTemp oldTile)
    {
        TileTemp newTile = GameState.DeserializeTile(tileData);
        handler.TileDict[coord].Tile = newTile;

        // Rewrite newTile with all other values of oldTile
        newTile.battleOccurring = oldTile.battleOccurring;
        newTile.battleCloud = oldTile.battleCloud;
        newTile.farmerObj = oldTile.farmerObj;
        newTile.tileOwner = oldTile.tileOwner;
        newTile.SortedSoldiers = oldTile.SortedSoldiers;
        newTile.timeStartedCapturing = oldTile.timeStartedCapturing;
        newTile.tileDamage = oldTile.tileDamage;

        oldTile = newTile;
    }

    void _SyncTileUpdateLastPlanted(TileSyncData tileData, ref TileTemp oldTile)
    {
        oldTile.timeLastPlanted = tileData.timeLastPlanted;
    }

    void _SyncTileUpdateFarmer(TileSyncData tileData, ref TileTemp oldTile)
    {
        // Change sprite
        if (oldTile.containsFarmer)
        {
            oldTile.farmerObj.GetComponent<SpriteSwitch>().StartCoroutine("change");
        }
    }

    void _SyncTileUpdateTileOwner(TileSyncData tileData, ref TileTemp oldTile)
    {
        oldTile.tileOwner = tileData.tileOwner;
    }

    void _SyncTileUpdateBattle(TileSyncData tileData, ref TileTemp oldTile)
    {
        oldTile.battleOccurring = tileData.battleOccurring;
        if (!oldTile.battleOccurring)
        {
            oldTile.battleCloud = null;
        }
    }

    void _SyncTileUpdateTileDamage(TileSyncData tileData, ref TileTemp oldTile)
    {
        oldTile.tileDamage = tileData.tileDamage;
    }
}
