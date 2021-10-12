using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAPI;

public class Market : NetworkBehaviour
{
    Repository central;
    Hex selectedHex;

    public TileHandler UIHandler;
    public CropManager crops;
    public PeopleManager people;

    public Text moneyText;

    Dictionary<CropType, int> CropValues = new Dictionary<CropType, int>
    {
        {CropType.potato, 1},
        {CropType.carrot, 2},
        {CropType.rice, 10},
    };
    int soldierCost = 10;
    int farmerCost = 5;

    void Start()
    {
        central = Repository.Central;
    }

    public override void NetworkStart()
    {
        selectedHex = central.selectedHex;
        if(UIHandler != null){
            UIHandler[selectedHex] = new HighLight();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsClient && !IsServer) { return; }
        if (!central.gameIsRunning) { return; }

        ChangeSelectedHex();
        TryHarvestCrop();
        TryHotkey();

        // Updates money text
        string dollars = "$" + (((int)(central.money * 100)) / 100.0).ToString();
        moneyText.text = dollars;
    }

    // Change selected hex
    void ChangeSelectedHex()
    {
        if (selectedHex != null && UIHandler != null && UIHandler[selectedHex] != null && selectedHex != central.selectedHex)
        {
            UIHandler[selectedHex] = new BlankTile();
            selectedHex = central.selectedHex;
            UIHandler[selectedHex] = new HighLight();
        }
    }

    // Sees if we clicked on a crop, then tries to harvest it
    void TryHarvestCrop()
    {
        // Clicking a new tile.
        Hex hex = TileManager.TM.getMouseHex();
        if (Input.GetMouseButtonDown(0) & TileManager.TM.isValidHex(hex))
        {
            double add = crops.harvest(selectedHex);
            if (add > 0)
            {
                central.money += add * add / 200;
            }
        }
    }

    // Plants a crop based on keyboard shortcuts
    void TryHotkey()
    {
        if (Input.GetKeyDown("1"))
        {
            SetCrop((int)CropType.potato);
        }
        else if (Input.GetKeyDown("2"))
        {
            SetCrop((int)CropType.carrot);
        }
        else if (Input.GetKeyDown("3"))
        {
            SetCrop((int)CropType.rice);
        }
        else if (Input.GetKeyDown("4"))
        {
            //SetCrop((int)CropType.eggplant);
        }
        else if (Input.GetKeyDown("5"))
        {
            SetFarmer();
        }
        else if (Input.GetKeyDown("6"))
        {
            AddSoldier();
        }
    }

    public void SetCrop(int cropInt)
    {
        CropType cropType = (CropType)cropInt;
        Hex coord = selectedHex;

        // Subtract money if we can afford it
        int cost = CropValues[cropType];
        if (central.money >= cost && crops.addCrop(coord, cropType))
        {
            central.money -= cost;
        }
    }

    public void SetFarmer()
    {
        // Add farmer
        if (crops.handler[selectedHex].containsFarmer == false &&
            central.money >= farmerCost &&
            crops.addFarmer(selectedHex))
        {
            central.money -= farmerCost;
        }
        // Remove farmer
        else
        {
            crops.removeFarmer(selectedHex);
        }
    }

    public void AddSoldier()
    {
        if (central.money >= soldierCost && crops.addSoldier(selectedHex))
            central.money -= soldierCost;
    }
}
