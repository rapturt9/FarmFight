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
    }

    public void SetPotato()
    {
        crops.addPotato(selectedHex);
    }

    public void SetCarrot()
    {
        crops.addCarrot(selectedHex);
    }

    public void Set()
    {
        
    }


}
