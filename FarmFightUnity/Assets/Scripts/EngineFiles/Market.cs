using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Market : MonoBehaviour
{
    public Repository central;
    Hex selectedHex;

    void Start()
    {
        central = Repository.Central;

        selectedHex = central.selectedHex;
        if(UIHandler != null){
            UIHandler[selectedHex] = new HighLight();
        }

        money = 100;
    }

    public TileHandler UIHandler;
    public CropManager crops;
    public PeopleManager people;

    double money;

    public Text money_text;

    // Update is called once per frame
    void Update()
    {
        bool move = false; //if new til selected
        if (selectedHex != null && UIHandler != null && UIHandler[selectedHex] != null && selectedHex != central.selectedHex)
        {

            UIHandler[selectedHex] = new BlankTile();
            selectedHex = central.selectedHex;
            UIHandler[selectedHex] = new HighLight();

            move = true;

        }

        if (selectedHex != null && UIHandler != null && UIHandler[selectedHex] != null)
        {
            double add = 0;
            add += crops.harvest(selectedHex, move);
            if (add > 0)
            {
                money += add * add / 200; 
            }
            string dollars = (((int)(money * 100)) / 100.0).ToString() + "$";
            GameObject.FindWithTag("Market").GetComponent<Text>().text=dollars;
        }

    }



    public void SetRice()
    {
        if(money >= 10){
            if(crops.addWheat(selectedHex)){
                money -= 10;
            }
        }
    }

    public void SetPotato()
    {
        if(money >= 1){
            if(crops.addPotato(selectedHex)){
                money -= 1;
            }
        }
    }

    public void SetCarrot()
    {
        if(money >= 2){
            if(crops.addCarrot(selectedHex)){
                money -= 2;
            }
        }
    }

    /*public void SetFarmer()
    {
        crops.addFarmer(selectedHex);
    }*/
    public void Set()
    {

    }




}
