using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAPI;

public class Market : NetworkBehaviour
{
    Repository central;
    Hex selectedHex;


    public static Market market;

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

    private void Awake()
    {
        market = this;
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

        if (central.GamesMode == PlayState.NormalGame)
            MarketUpdateFunctionality();
        else if (central.GamesMode == PlayState.SoldierSend)
            SoldierSendUpdate();
       

        // Updates money text
        string dollars = "$" + (((int)(central.money * 100)) / 100.0).ToString();
        moneyText.text = dollars;
    }



    public void MarketUpdateFunctionality()
    {
        ChangeSelectedHex();
        TryHarvestCrop();
        TryHotkey();
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
        else if (Input.GetKeyDown("7"))
        {
            if(crops.handler[selectedHex].tileOwner == Repository.Central.localPlayerId)
                SendSoldier();
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





    /// <summary>
    /// wanting to Send Soldiers
    /// </summary>
    ///

    public Hex SoldierDestination;

    private void SoldierSendUpdate()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (TileManager.TM.getMouseHex() == SoldierDestination)
            {
                SoldierTrip(selectedHex, SoldierDestination);
            }

            SetSoldierDestination();
        }


        


    }

    private void SetSoldierDestination()
    {
        if (SoldierDestination != null && UIHandler != null && UIHandler[SoldierDestination] != null) //&& selectedHex != central.selectedHex)
        {
            if (selectedHex != SoldierDestination)
                UIHandler[SoldierDestination] = new BlankTile();
            SoldierDestination = central.selectedHex;

            if(selectedHex !=  SoldierDestination)
                UIHandler[SoldierDestination] = new SoldierDestination();
        }
    }


    private void SoldierTrip(Hex start, Hex end)
    {
        
        if(SoldierDestination != null && SoldierDestination != selectedHex)
        {
            crops.SendSoldier(start, end);
            SendSoldier();
        }
        
        

        UIHandler[SoldierDestination] = new BlankTile();

    }


    public void SendSoldier()
    {
        if (central.GamesMode == PlayState.NormalGame)
        {
            central.GamesMode = PlayState.SoldierSend;
        }
        else if (central.GamesMode == PlayState.SoldierSend)
        {
            central.GamesMode = PlayState.NormalGame;
        }
    }
    

    


    
}
