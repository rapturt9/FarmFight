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
            sendSoldierServerRpc(BoardHelperFns.HexToArray(central.selectedHex),
                BoardHelperFns.HexToArray(TileManager.TM.getMouseHex()),
                central.localPlayerId);
        }

        List<Soldier> s = handler[central.selectedHex].soldiers;
        if (s.Count > 0)
        {
            Debug.Log("Length " + s.Count.ToString());
            Debug.Log(s[0].owner.Value);
        }
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
        return true;
    }

    public void clearTile(Hex hex)
    {
        handler[hex] = new BlankTile();
    }

    public bool addFarmer(Hex hex)
    {
        if (handler[hex].containsFarmer == false)
        {
            handler[hex].containsFarmer = true;
            Debug.Log("Has Farmer");
            return true;
        }
        return false;
    }

    public bool removeFarmer(Hex hex)
    {
        if (handler[hex].containsFarmer == true)
        {
            handler[hex].containsFarmer = false;
            return true;
        }
        return false;
    }

    // We initially spawn a soldier only on the server
    [ServerRpc(RequireOwnership = false)]
    public void addSoldierServerRpc(int[] hexArray, int owner)
    {
        Hex hex = BoardHelperFns.ArrayToHex(hexArray);
        TileTemp tile = handler[hex];
        // Spawns soldier
        Soldier soldier = SpriteRepo.Sprites["Soldier", tile.hexCoord].GetComponent<Soldier>();
        soldier.transform.position = tile.hexCoord.world() + Vector2.left * .25f;
        soldier.owner.Value = owner;
        tile.soldiers.Add(soldier);
        handler.SyncTile(hex);
    }

    [ServerRpc(RequireOwnership = false)]
    public void sendSoldierServerRpc(int[] startArray, int[] endArray, int localPlayerId)
    {
        Hex start = BoardHelperFns.ArrayToHex(startArray);
        Hex end = BoardHelperFns.ArrayToHex(endArray);

        TileTemp startTile = handler[start];

        if (startTile.soldierCount != 0 && 
            end != startTile.hexCoord && 
            startTile.soldiers[0].owner.Value == localPlayerId)
        {
            Soldier soldier = startTile.soldiers[0];
            SoldierTrip trip;

            if (!soldier.TryGetComponent(out trip))
            {
                startTile.soldiers[0].gameObject.AddComponent<SoldierTrip>()
                .init(startTile.hexCoord, end);
            }
            else
                trip.init(startTile.hexCoord, end);

            startTile.soldiers.RemoveAt(0);

            if (startTile.soldierCount != 0)
            {
                startTile.soldiers[0].FadeIn();
            }
            handler.SyncTile(start);
        }
        else Debug.Log("cannot Send");
    }

    public void sendSoldier(Hex start, Hex end)
    {
        //handler[start].sendSoldier(end);
    }
}
