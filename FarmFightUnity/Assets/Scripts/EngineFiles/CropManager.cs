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
    }



    private void Update()
    {
        if (!central.gameIsRunning) { return; }

        // Right click to send a soldier
        if (Input.GetMouseButtonDown(1))
        {
            
        }

        // Soldier on tile debugging
        //List<Soldier> s = handler[central.selectedHex].soldiers;
        //if (s.Count > 0)
        //{
        //    Debug.Log("Length " + s.Count.ToString());
        //    Debug.Log(s[0].owner.Value);
        //}
    }

    public double harvest(Hex hex)
    {
        // Only harvest if owned by the local player
        if (handler[hex].tileOwner == central.localPlayerId)
        {
            //if crop there
            int hLevel = 0;
            if (handler[hex].TileName == "Potato")
            {
                hLevel = 1;
            }
            if (handler[hex].TileName == "Carrot")
            {
                hLevel = 4;
            }
            if (handler[hex].TileName == "Wheat")
            {
                hLevel = 2;
            }
            if (handler[hex].TileName == "Rice")
            {
                hLevel = 10;
            }

            if (hLevel > 0)
            {
                double add = handler[hex].reset() * hLevel;
                handler.SyncTile(hex);
                return add;
            }
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
        // Check if there is an adjacent tile owned by us
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
        // Set owner
        handler[hex].tileOwner = central.localPlayerId;
        handler.SyncTile(hex);
        return true;
    }

    public void clearTile(Hex hex)
    {
        handler[hex] = new BlankTile();
    }

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
        handler[hex].addFarmer();
        handler.SyncTile(hex);
        Debug.Log("Has Farmer");
    }

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

    // We initially spawn a soldier only on the server
    [ServerRpc(RequireOwnership = false)]
    void addSoldierServerRpc(int[] hexArray, int owner)
    {
        Hex hex = BoardHelperFns.ArrayToHex(hexArray);
        handler[hex].addSoldier(owner);
        handler.SyncTile(hex);
    }

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
}
