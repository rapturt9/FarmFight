using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAPI;

public class Market : NetworkBehaviour
{
    Repository central;
    Hex selectedHex;

    public Text TileInfoText;

    public static Market market;

    public TileHandler UIHandler;
    public CropManager crops;
    public SoldierManager soldierManager;
    public PeopleManager people;

    public Text moneyText;

    public Dictionary<CropType, int> CropCosts
    {
        get
        {
            return new Dictionary<CropType, int>
            {
                {CropType.potato, potatoCost },
                {CropType.carrot, carrotCost},
                {CropType.rice, riceCost},
                {CropType.eggplant, EggplantCost}
            };
        }
    }

    public Dictionary<CropType, int> CropValues
    {
        get
        {
            return new Dictionary<CropType, int>
            {
                {CropType.potato, potatoCost },
                {CropType.carrot, carrotCost},
                {CropType.rice, riceCost},
                {CropType.eggplant, EggplantCost}
            };
        }
    }

    public int potatoCost, carrotCost, riceCost, EggplantCost, farmerCost;
    public int potatoCVal, carrotVal, riceVal, EggplantVal, farmerVal;



    public float BaseSoldierCost = 5;
    

    public float weightedSoldierCost {
        get
        {
            if ( TileManager.TM.handlers == null || Repository.Central.cropHandler == null)
                return 10;


            float soldierCount = Repository.Central.cropHandler[selectedHex].
                        SortedSoldiers[central.localPlayerId].Count;

            return BaseSoldierCost * (1+ (soldierCount * soldierCount / 16))+
                BoardChecker.Checker.soldierCount[central.localPlayerId]/4;
        } }

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

        if (TileInfoText != null)
        {
            string total = "";
            //for (int i = 0; i < 6; i++)
            //{
            //    total += i + ": " + central.tileinfo.soldierInfo[i]["num"] + " | " + central.tileinfo.soldierInfo[i]["health"] + "h\n";
            //}
            for (int i = 0; i < 6; i++)
            {
                var soldiers = crops.handler[central.selectedHex].SortedSoldiers[i];
                int soldierCount = soldiers.Count;
                float soldierHealth = soldiers.Sum(soldier => soldier.Health.Value);
                total += i + ": " + soldierCount + " | " + (int)soldierHealth + "h\n";
            }
            total+="Dmg: "+(int)(crops.handler[central.selectedHex].tileDamage*10)+"%";
            TileInfoText.text = total;
            return;
        }

        if (central.GamesMode == PlayState.NormalGame)
            MarketUpdateFunctionality();
        else if (central.GamesMode == PlayState.SoldierSend)
            SoldierSendUpdate();

        // Right click soldiers
        if (Input.GetMouseButtonDown(1))
        {
            Hex endHex = TileManager.TM.getMouseHex();
            soldierManager.SendSoldier(selectedHex, endHex);
        }

       
        

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

            //TileTemp tile = crops.handler[selectedHex];
            //if (tile.soldierCount > 0)
            //    Debug.Log(tile.SortedSoldiers[tile.tileOwner][0].Health.Value);
        }
    }

    // Sees if we clicked on a crop, then tries to harvest it
    void TryHarvestCrop()
    {
        // Clicking a new tile.
        Hex hex = TileManager.TM.getMouseHex();
        if (Input.GetMouseButtonDown(0) && TileManager.TM && TileManager.TM.isValidHex(hex) && crops != null && crops.handler != null && crops.handler[hex]!=null && !crops.handler[hex].battleOccurring)
        {
            double add = crops.harvest(selectedHex);
            if (add > 0)
            {
                central.money += add;
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
            SetCrop((int)CropType.eggplant);
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
            //if(crops.handler[selectedHex].tileOwner == Repository.Central.localPlayerId)
            SendSoldier();
        }

        // DEBUG
        if (Input.GetKeyDown("d"))
        {
            var iter = crops.handler[selectedHex].getSoldierEnumerator().GetEnumerator();
            if (iter.MoveNext())
            {
                Debug.Log(iter.Current.Health.Value);
            }
        }
        // Free money DEBUG
        if (Input.GetKeyDown("x") && Input.GetKey(KeyCode.LeftShift))
        {
            central.money += 100;
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
            //central.money += farmerCost/2;
            //crops.removeFarmer(selectedHex);
        }
    }

    public void AddSoldier()
    {
        float SoldierCost = weightedSoldierCost;
        if (central.money >= SoldierCost && soldierManager.addSoldier(selectedHex))
            central.money -= Mathf.Floor(SoldierCost);
    }

    /// <summary>
    /// wanting to Send Soldiers
    /// </summary>
    ///

    public Hex SoldierDestination;


    private void SoldierSendUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            /*if (TileManager.TM.getMouseHex() == SoldierDestination)
            {
                SoldierTrip(selectedHex, SoldierDestination);
            }

            SetSoldierDestination();*/
            if (SoldierDestination != null && UIHandler != null && UIHandler[SoldierDestination] != null){
                SoldierTrip(selectedHex, SoldierDestination);
            }
        }
        if(TileManager.TM.getMouseHex() != selectedHex){
            if(SoldierDestination != null && SoldierDestination != selectedHex){
                UIHandler[SoldierDestination] = new BlankTile();
            }
            SoldierDestination = TileManager.TM.getMouseHex();
            UIHandler[TileManager.TM.getMouseHex()] = new SoldierDestination();
        }
    }

    private void SetSoldierDestination()
    {
        if (SoldierDestination != null && UIHandler != null && UIHandler[SoldierDestination] != null) //&& selectedHex != central.selectedHex)
        {
            if (selectedHex != SoldierDestination){
                UIHandler[SoldierDestination] = new BlankTile();
            }
            SoldierDestination = central.selectedHex;

            if(selectedHex !=  SoldierDestination)
                UIHandler[SoldierDestination] = new SoldierDestination();
        }
    }


    private void SoldierTrip(Hex start, Hex end)
    {
        
        if(SoldierDestination != null && SoldierDestination != selectedHex)
        {
            soldierManager.SendSoldier(start, end);
            SendSoldier();

            UIHandler[SoldierDestination] = new BlankTile();
        }


    }

    public int soldierSendCost;
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
