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

    public void SetCrop(int cropInt)
    {
        CropType cropType = (CropType)cropInt;
        Hex coord = selectedHex;

        // Subtract money if we can afford it
        int cost = CropValues[cropType];
        if (central.money >= cost && crops.addCrop(coord, cropType))
        {
            central.money -= cost;
            // Set owner
            crops.handler[coord].tileOwner = central.localPlayerId;
            crops.handler.SyncTile(coord);
        }
    }

    public void SetFarmer(bool state = true)
    {
        if (state)
            crops.addFarmer(selectedHex);
        else
            crops.removeFarmer(selectedHex);

        crops.handler.SyncTile(selectedHex);
    }

    public void AddSoldier()
    {
        crops.addSoldier(selectedHex);
        //crops.handler.SyncTile(selectedHex);
    }

    

    public void Set()
    {

    }
}
