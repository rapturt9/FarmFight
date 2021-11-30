using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using System;

public class CropManager : NetworkBehaviour
{
    // Start is called before the first frame update

    public TileHandler handler;
    Repository central;

    void Start()
    {
        central = Repository.Central;
        handler = GetComponent<TileHandler>();

        Crops = this;
    }

    public static CropManager Crops;


    private void Update()
    {
        if (!central.gameIsRunning) { return; }

        
    }

    public GameObject Vegetable;
    

    public double harvest(Hex hex, int owner = -1)
    {
        if (owner == -1)
            owner = central.localPlayerId;

        // Only harvest if owned by the local player
        if (handler[hex].tileOwner == owner)
        {
            CropType crop = handler[hex].cropType;

            //if crop there
            int hLevel = 0;
            if (crop == CropType.potato)
            {
                hLevel = Market.market.CropValues[CropType.potato];
            }
            else if (crop == CropType.carrot)
            {
                hLevel = Market.market.CropValues[CropType.carrot];
            }
            else if (crop == CropType.rice)
            {
                hLevel = Market.market.CropValues[CropType.rice];
            }
            else if (crop == CropType.eggplant)
            {
                hLevel = Market.market.CropValues[CropType.eggplant];
            }

            if (hLevel > 0)
            {
                double add = handler[hex].reset() * hLevel;
                handler.SyncTileUpdate(hex, new[] { CropTileSyncTypes.lastPlanted });

                if (central.flyingVegies && owner == central.localPlayerId)
                {
                    var vegie = Instantiate(Vegetable);
                    vegie.GetComponent<Vegetable>().init(hex, crop, add);
                    return 0;
                }
                else
                    return add;
            }

            else return 0;
        }

        return 0;
    }

    

    public bool canPlant(Hex hex, int owner = -1)
    {
        if (owner == -1)
            owner = central.localPlayerId;

        // Disqualifying attributes
        if ((handler[hex].tileOwner != -1 && handler[hex].tileOwner != owner) || // We can't overwrite an opponent's crop
            //handler[hex].hostileOccupation || // Can't change the tile if it's occupied by an enemy
            handler[hex].otherPeoplesSoldiers(owner) != 0 // Can't change the tile if it's occupied by an enemy
            )
        {
            return false;
        }

        foreach (var adj in TileManager.TM.getValidNeighbors(hex))
        {
            if (hasCrop(adj) && (handler[adj].tileOwner == owner))
            {
                return true;
            }
        }

        return false;
    }

    public bool hasCrop(Hex hex)
    {
        return handler[hex].cropType != CropType.blankTile;
    }


    public bool addCrop(Hex hex, CropType cropType, int owner = -1)
    {
        if (owner == -1)
            owner = central.localPlayerId;

        if (!canPlant(hex, owner))
        {
            return false;
        }

        TileTemp oldTile = handler.TileDict[hex].Tile;
        if (cropType == CropType.potato)
        {
            handler.TileDict[hex].Tile = new Potato();
        }
        else if (cropType == CropType.carrot)
        {
            handler.TileDict[hex].Tile = new Carrot();
        }
        else if (cropType == CropType.rice)
        {
            handler.TileDict[hex].Tile = new Rice();
        }
        else if (cropType == CropType.eggplant)
        {
            handler.TileDict[hex].Tile = new Eggplant();
        }
        else
        {
            throw new Exception("Invalid crop type");
        }
        handler.TileDict[hex].Tile.tileOwner = owner;

        handler.TileDict[hex].Tile.battleOccurring = oldTile.battleOccurring;
        handler.TileDict[hex].Tile.battleCloud = oldTile.battleCloud;
        handler.TileDict[hex].Tile.farmerObj = oldTile.farmerObj;
        handler.TileDict[hex].Tile.SortedSoldiers = oldTile.SortedSoldiers;
        handler.TileDict[hex].Tile.timeStartedCapturing = oldTile.timeStartedCapturing;
        handler.TileDict[hex].Tile.tileDamage = oldTile.tileDamage;

        handler.SyncTileUpdate(hex, new[] { CropTileSyncTypes.cropNum, CropTileSyncTypes.tileOwner });

        return true;
    }

    public void clearTile(Hex hex)
    {
        handler[hex] = new BlankTile();
    }

    // Add farmer
    public bool addFarmer(Hex hex, int owner = -1)
    {
        if (owner == -1)
            owner = central.localPlayerId;

        if (handler[hex].containsFarmer == false && 
            handler[hex].tileOwner == owner && 
            !handler[hex].hostileOccupation)
        {
            addFarmerServerRpc(BoardHelperFns.HexToArray(hex));
            return true;
        }
        return false;
    }

    [ServerRpc(RequireOwnership = false)]
    void addFarmerServerRpc(int[] hexArray)
    {
        Hex hex = BoardHelperFns.ArrayToHex(hexArray);
        handler[hex].addFarmer();
    }

    // Remove farmer
    public bool removeFarmer(Hex hex)
    {
        if (handler[hex].containsFarmer == true)
        {
            removeFarmerServerRpc(BoardHelperFns.HexToArray(hex));
            return true;
        }
        return false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void removeFarmerServerRpc(int[] hexArray)
    {
        Hex hex = BoardHelperFns.ArrayToHex(hexArray);
        handler[hex].removeFarmer();
    }
}
