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

        if (money_text != null)
        {
            Debug.Log(money_text);
            money_text.text = "0.00$";
        }
    }

    public TileHandler UIHandler;
    public CropManager crops;
    public PeopleManager people;

    public double money;

    public Text money_text;

    // Update is called once per frame
    void Update()
    {
        if (selectedHex != null && UIHandler != null && UIHandler[selectedHex] != null && selectedHex != central.selectedHex)
        {

            UIHandler[selectedHex] = new BlankTile();
            selectedHex = central.selectedHex;
            UIHandler[selectedHex] = new HighLight();

        }

        if (selectedHex != null && UIHandler != null && UIHandler[selectedHex] != null)
        {
            double add = 0;
            add += crops.harvest(selectedHex);
            if (add > 0)
            {
                money += add * add / 500;
            }

            string dollars = (((int)(money * 100)) / 100.0).ToString() + "$";
            GameObject.FindWithTag("Market").GetComponent<Text>().text=dollars;
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
