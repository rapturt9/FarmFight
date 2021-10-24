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
                handler.SyncTile(hex);

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

        // We can't overwrite an opponent's crop
        if (handler[hex].cropType != CropType.blankTile && handler[hex].tileOwner != central.localPlayerId)
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
        handler.SyncTile(hex);
        CheckForWinServerRpc(central.localPlayerId);

        return true;
    }

    public void clearTile(Hex hex)
    {
        handler[hex] = new BlankTile();
    }

    // Add farmer
    public bool addFarmer(Hex hex)
    {
        if (handler[hex].containsFarmer == false && handler[hex].tileOwner == central.localPlayerId)
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
        GameObject farmer = handler[hex].addFarmer();
        farmer.GetComponent<NetworkObject>().Spawn();
        handler.SyncTile(hex);
        Debug.Log("Has Farmer");
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
        handler.SyncTile(hex);
    }

    // Add soldier
    public bool addSoldier(Hex hex)
    {
        int[] hexArray = BoardHelperFns.HexToArray(hex);
        int owner = central.localPlayerId;
        if (handler[hex].tileOwner == owner)
        {
            addSoldierServerRpc(hexArray, owner);
            return true;
        }
        return false;
    }

    [ServerRpc(RequireOwnership = false)]
    void addSoldierServerRpc(int[] hexArray, int owner)
    {
        Hex hex = BoardHelperFns.ArrayToHex(hexArray);
        Soldier soldier = handler[hex].addSoldier(owner);
        soldier.GetComponent<NetworkObject>().Spawn();
        soldier.AddToTile(hex);
    }

    // Send soldier
    [ServerRpc(RequireOwnership = false)]
    public void sendSoldierServerRpc(int[] startArray, int[] endArray, int localPlayerId)
    {
        Hex start = BoardHelperFns.ArrayToHex(startArray);
        Hex end = BoardHelperFns.ArrayToHex(endArray);

        TileTemp startTile = handler[start];
        if (startTile.sendSoldier(end, localPlayerId))
            handler.SyncTile(start);
    }


    public void SendSoldier(Hex start, Hex end, int number = 1)
    {
        sendSoldierServerRpc(BoardHelperFns.HexToArray(start),
                BoardHelperFns.HexToArray(end),
                central.localPlayerId);
    }

    [ServerRpc(RequireOwnership = false)]
    void CheckForWinServerRpc(int playerId)
    {
        BoardChecker.Checker.CheckForWin(playerId);
    }
}
