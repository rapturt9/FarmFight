using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Market : MonoBehaviour
{
    public Repository central;
    Hex selectedHex;

    void Start()
    {
        central = Repository.Central;

        selectedHex = central.selectedHex;
    }

    public TileHandler UIHandler;
    public CropManager crops;
    public PeopleManager people;

    // Update is called once per frame
    void Update()
    {
        if(selectedHex != central.selectedHex)
        {

            UIHandler[selectedHex] = new BlankTile();
            selectedHex = central.selectedHex;
            UIHandler[selectedHex] = new HighLight();

            
        }
           
    }



    public void SetRice()
    {
        crops.addWheat(selectedHex);
        SetCrop(selectedHex);
    }

    public void SetPotato()
    {
        crops.addPotato(selectedHex);
        SetCrop(selectedHex);
    }

    public void SetCarrot()
    {
        crops.addCarrot(selectedHex);
        SetCrop(selectedHex);
    }

    public void SetCrop(Hex coord)
    {
        if (crops.handler[coord] is CropTile)
        {
            ((CropTile)crops.handler[coord]).tileOwner = central.localPlayerId;
            crops.handler.SyncTile(coord);
        }
    }

}
