using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CropManager : MonoBehaviour
{
    // Start is called before the first frame update

    public TileHandler handler;
    Repository central;
    bool plantOne;

    void Start()
    {
        central = Repository.Central;
        handler = GetComponent<TileHandler>();

        coords = new int[6, 2] { { -1, 0 }, { -1, 1 }, { 0, -1 }, { 0, 1 }, { 1, -1 }, { 1, 0 } };
        plantOne = false;

    }

    private void Update()
    {
        /*
        if (Input.GetMouseButtonDown(0) &
                TileManager.TM.isValidHex(TileManager.TM.getMouseHex()))
        {

            addCarrot(TileManager.TM.getMouseHex());
        }

        if (Input.GetMouseButtonDown(1) &
                TileManager.TM.isValidHex(TileManager.TM.getMouseHex()))
        {

            clearTile(TileManager.TM.getMouseHex());
        }
        */
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

    int[,] coords;

    public bool canPlant(Hex hex)
    {
        if (!plantOne)
        {
            return true;
        }

        //for (int i = 0; i < 6; i++)
        //{
        //    int x = hex.x + coords[i, 0];
        //    int y = hex.y + coords[i, 1];
        //    if (-3 <= x && x <= 3 &&
        //       -3 <= y && y <= 3 &&
        //       -3 <= x + y && x + y <= 3)
        //    {
        //        Hex adj = new Hex(hex.x + coords[i, 0], hex.y + coords[i, 1]);
        //        print(hasCrop(adj));
        //        print(handler[hex].tileOwner);
        //        if (hasCrop(adj) && (handler[hex].tileOwner == central.localPlayerId))
        //        {
        //            return true;
        //        }

        //    }
        //}
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

        if (!plantOne)
        {
            plantOne = true;
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


    public void addSoldier(Hex hex, int number = 1)
    {
        handler[hex].soldiers += number;
    }
}
