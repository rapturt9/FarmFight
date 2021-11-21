using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;

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
    public GameManager gameManager;


    private void Update()
    {
        if (!central.gameIsRunning) { return; }

        
    }

    public GameObject Vegetable;
    

    public double harvest(Hex hex)
    {
        // Only harvest if owned by the local player
        if (handler[hex].tileOwner == central.localPlayerId)
        {
            CropType crop = handler[hex].cropType;

            //if crop there
            int hLevel = 0;
            if (crop == CropType.potato)
            {
                hLevel = 1;
            }
            else if (crop == CropType.carrot)
            {
                hLevel = 4;
            }
            else if (crop == CropType.rice)
            {
                hLevel = 2;
            }
            else if (crop == CropType.eggplant)
            {
                hLevel = 10;
            }

            if (hLevel > 0)
            {
                double add = handler[hex].reset() * hLevel;
                handler.SyncTileUpdate(hex, new[] { CropTileSyncTypes.lastPlanted });

                if (central.flyingVegies)
                {
                    var vegie = GameObject.Instantiate(Vegetable);
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

    

    public bool canPlant(Hex hex)
    {
        if (handler[hex].cropType != CropType.blankTile && 
            handler[hex].tileOwner != central.localPlayerId && // We can't overwrite an opponent's crop
            !handler[hex].hostileOccupation)
        {
            return false;
        }

        foreach (var adj in TileManager.TM.getValidNeighbors(hex))
        {
            if (hasCrop(adj) && (handler[adj].tileOwner == central.localPlayerId))
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


    public bool addCrop(Hex hex, CropType cropType)
    {
        if (!canPlant(hex))
        {
            return false;
        }
        
        if (cropType == CropType.potato)
        {
            handler[hex] = new Potato();
        }
        else if (cropType == CropType.carrot)
        {
            handler[hex] = new Carrot();
        }
        else if (cropType == CropType.rice)
        {
            handler[hex] = new Rice();
        }
        else if (cropType == CropType.eggplant)
        {
            handler[hex] = new Eggplant();
        }

        // Set owner
        handler[hex].tileOwner = central.localPlayerId;
        handler.SyncTileUpdate(hex, new[] { CropTileSyncTypes.cropNum, CropTileSyncTypes.tileOwner });

        return true;
    }

    public void clearTile(Hex hex)
    {
        handler[hex] = new BlankTile();
    }

    // Add farmer
    public bool addFarmer(Hex hex)
    {
        if (handler[hex].containsFarmer == false && 
            handler[hex].tileOwner == central.localPlayerId && 
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
