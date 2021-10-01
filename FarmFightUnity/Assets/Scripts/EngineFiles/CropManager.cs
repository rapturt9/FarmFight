using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CropManager : MonoBehaviour
{
    // Start is called before the first frame update

    TileHandler handler;
    bool plantOne;

    void Start()
    {
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

    public double harvest(Hex hex, bool move)
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
            return handler[hex].reset(move) * hLevel;
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

        for (int i = 0; i < 6; i++)
        {
            int x = hex.x + coords[i, 0];
            int y = hex.y + coords[i, 1];
            if (-3 <= x && x <= 3 &&
               -3 <= y && y <= 3 &&
               -3 <= x + y && x + y <= 3)
            {
                Hex adj = new Hex(hex.x + coords[i, 0], hex.y + coords[i, 1]);
                if (hasCrop(adj))
                {
                    return true;
                }

            }
        }

        return false;
    }

    public bool hasCrop(Hex hex)
    {
        return handler[hex].TileName == "Potato" || handler[hex].TileName == "Carrot" || handler[hex].TileName == "Rice" || handler[hex].TileName == "Wheat";
    }


    public bool addPotato(Hex hex)
    {
        if (!canPlant(hex))
        {
            return false;
        }
        handler[hex] = new Potato();
        if (!plantOne)
        {
            plantOne = true;
        }
        return true;
    }

    public bool addCarrot(Hex hex)
    {
        if (!canPlant(hex))
        {
            return false;
        }
        handler[hex] = new Carrot();
        if (!plantOne)
        {
            plantOne = true;
        }
        return true;
    }

    public bool addWheat(Hex hex)
    {
        if (!canPlant(hex))
        {
            return false;
        }
        handler[hex] = new Rice();
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

        if (handler[hex].hasFarmer == false)
        {
            handler[hex].hasFarmer = true;
            Debug.Log("Has Farmer");
            return true;
        }
        return false;
    }

    public bool removeFarmer(Hex hex)
    {

        if (handler[hex].hasFarmer == true)
        {
            handler[hex].hasFarmer = false;
            return true;
        }
        return false;
    }


    public void addSoldier(Hex hex, int number = 1)
    {
        handler[hex].soldiers += number;
    }






}
