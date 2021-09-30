using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CropManager : MonoBehaviour
{
    // Start is called before the first frame update

    TileHandler handler;

    void Start()
    {
        handler = GetComponent<TileHandler>();

        

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
        //if crop there
        int hLevel = 0;
        if(handler[hex].TileName == "Potato"){
            hLevel = 1;
        }
        if(handler[hex].TileName == "Carrot"){
            hLevel = 4;
        }
        if(handler[hex].TileName == "Wheat"){
            hLevel = 2;
        }
        if(handler[hex].TileName == "Rice"){
            hLevel = 10;
        }

        if(hLevel > 0){
            return handler[hex].reset() * hLevel;
        }

        return 0;
    }
    


    public void addPotato(Hex hex)
    {
        handler[hex] = new Potato();
    }

    public void addCarrot(Hex hex)
    {
        handler[hex] = new Carrot();
    }

    public void addWheat(Hex hex)
    {
        handler[hex] = new Rice();
    }

    public void clearTile(Hex hex)
    {
        handler[hex] = new BlankTile();
    }

    public bool addFarmer(Hex hex)
    {
        
        if(handler[hex].hasFarmer == false)
        {
            handler[hex].hasFarmer = true;
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
