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


    public void addPotato(Hex hex)
    {
        handler[hex] = new Potato();
        handler.SyncTile(hex);
    }

    public void addCarrot(Hex hex)
    {
        handler[hex] = new Carrot();
        handler.SyncTile(hex);
    }

    public void addWheat(Hex hex)
    {
        handler[hex] = new Wheat();
        handler.SyncTile(hex);
    }

    public void clearTile(Hex hex)
    {
        handler[hex] = new BlankTile();
        handler.SyncTile(hex);
    }

    public bool addFarmer(Hex hex)
    {
        return true;
    }

    public void addSoldier(Hex hex, int number = 1)
    {

    }






}
