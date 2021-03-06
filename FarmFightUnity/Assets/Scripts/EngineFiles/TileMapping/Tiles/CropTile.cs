using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public abstract class TileTemp : TileTempDepr
{
    public CropType cropType = CropType.blankTile;
    public float timeLastPlanted = 0f;
    
    public bool containsFarmer
    {
        get { return farmerObj != null; }
    }
    public bool battleOccurring = false;

    public float timeStartedCapturing = -1f;

    private float maxTimeToCapture = 2f;

    public bool hostileOccupation
    {
        get { return timeStartedCapturing != -1f; }
    }

    public CropEffect effect;

    public float tileDamage = 0.0f;

    public override void Start()
    {
        if (effect == null)
        {
            effect = SpriteRepo.Sprites["CropEffect"].GetComponent<CropEffect>();
            effect.GetComponent<CropEffect>().init(this);
        }
        

        SortedSoldiers = new Dictionary<int, List<Soldier>>();
        for (int i = 0; i < Repository.maxPlayers; i++)
            SortedSoldiers[i] = new List<Soldier>();

        if (timeLastPlanted == 0f)
            timeLastPlanted = NetworkManager.Singleton.NetworkTime;
        frame = 0;
        mid = 0;
        frameRate = 60;
        frameInternal = 0;
    }

    /// <summary>
    /// love this!!!
    /// </summary>
    public override void Behavior()
    {
        UpdateFrame();
        UpdateEffect();
        TryFarmerAutoharvest();
        UpdateTileArtAndCracks();
        UpdateSelectedTileInfo();
        BattleFunctionality();
        TryCapture();
    }

    // Changes the displayed crop frame
    void UpdateFrame()
    {
        // Update frames
        frameInternal = Mathf.Min(
            (int)((NetworkManager.Singleton.NetworkTime - timeLastPlanted) * frameRate),
            tileArts.Count * frameRate);

        frame = (int)(frameInternal / frameRate);
    }

    // For crop effects
    void UpdateEffect()
    {
        mid = tileArts.Count / 2; //optimal harvest level
        if (cropType == CropType.eggplant)
        {
            mid = 4.5;
        }
        else
        {
            mid = 5.5;
        }
        double diff = frameInternal / frameRate - mid; //for sparkle
        double diff2 = tileArts.Count - frameInternal / frameRate; //for rot

        // This is bugged in android so it's disabled
#if UNITY_EDITOR || UNITY_STANDALONE
        if (-.3 < diff && diff < .3)
        {
            effect.GetComponent<CropEffect>().Sparkle();
        }
        else if (diff2 < .3)
        {
            effect.GetComponent<CropEffect>().Rotting();
        }
        else
        {
            effect.GetComponent<CropEffect>().Stop();
        }
#endif
    }
    
    // Sees if the farmer should harvest its crop
    void TryFarmerAutoharvest()
    {
        if (containsFarmer && frameInternal / frameRate >= mid)
        {
            int hLevel = 0;
            if (cropType == CropType.potato)
            {
                hLevel = 1;
            }
            else if (cropType == CropType.carrot)
            {
                hLevel = 3;
            }
            else if (cropType == CropType.rice)
            {
                hLevel = 6;
            }

            else if (cropType == CropType.eggplant)
            {
                hLevel = 10;
            }

            double moneyToAdd = reset() * hLevel;

            // Is the player
            if (tileOwner == Repository.Central.localPlayerId)
                Repository.Central.money += moneyToAdd;

            // Is a bot or other player
            else
            {
                GameManager.GM.gameState.TryAddBotMoney(tileOwner, moneyToAdd);
            }
        }
    }
    

    // Change the ripeness and cracks
    void UpdateTileArtAndCracks()
    {
        // Update art if no battle is happening, so growing
        if (!battleOccurring)
        {
            //effect.crackAmount = tileDamage / 10.0f;
            if (0 <= frame && frame < tileArts.Count)
            {
                currentArt = tileArts[frame];
            }
            else
            {
                currentArt = tileArts[tileArts.Count - 1];
            }

            if (tileDamage > 0.0f)
            {
                tileDamage -= Time.deltaTime / 3;

            }
        }
        // If we're battling, the tile gets more cracked
        else
        {
            currentArt = null;

            //effect.crackAmount = (tileDamage / 10.0f);

            if (tileDamage < 10.0f)
            {
                tileDamage += Time.deltaTime;
            }
        }

        // Sync the damage
        if (NetworkManager.Singleton.IsServer)
        {
            if (tileDamage % 1 > .9f)
            {
                TileSyncer.Syncer.SyncTileUpdate(hexCoord, new[] { CropTileSyncTypes.tileDamage });
            }
        }
    }


    // Update the top right tile info with whichever tile we have selected
    void UpdateSelectedTileInfo()
    {
        if (Repository.Central.selectedHex == hexCoord)
        {
            Dictionary<int, Dictionary<string, int>> dict = Repository.Central.tileinfo.soldierInfo;
            Repository.Central.tileinfo.homePlayer = tileOwner;
            for (int playerId = 0; playerId < Repository.maxPlayers; playerId++)
            {
                dict[playerId]["num"] = SortedSoldiers[playerId].Count;
                float totalHealth = 0;
                foreach (var soldier in SortedSoldiers[playerId])
                {
                    totalHealth += soldier.Health.Value;
                }
                dict[playerId]["health"] = (int)totalHealth;
            }
        }
    }


    // Should we capture a tile?
    void TryCapture()
    {
        if (NetworkManager.Singleton.IsServer && // Only server can change capture status
            hostileOccupation && // We must be occupied by a hostile soldier
            (NetworkManager.Singleton.NetworkTime - timeStartedCapturing) >= maxTimeToCapture) // The soldier must have been sitting there long enuogh
        {
            CaptureThis();
        }
    }

    //destroys all associated gameobjects
    public override void End()
    {
        if(effect != null)
            GameObject.Destroy(effect.gameObject);
    }


    public Farmer addFarmer()
    {
        if (!NetworkManager.Singleton.IsServer) { Debug.LogWarning("Do not add farmers from the client! Wrap your method in a ServerRpc."); }

        farmerObj = SpriteRepo.Sprites["Farmer"].GetComponent<Farmer>();
        Farmer farmer = farmerObj.GetComponent<Farmer>();
        farmer.Owner.Value = tileOwner;
        farmerObj.transform.position = (Vector2)TileManager.TM.HexToWorld(hexCoord) + .25f * Vector2.right;
        farmerObj.GetComponent<NetworkObject>().Spawn();
        farmer.AddToTile(hexCoord);
        
        return farmerObj;
    }

    public void removeFarmer()
    {
        if (!NetworkManager.Singleton.IsServer) { Debug.LogWarning("Do not remove farmers from the client! Wrap your method in a ServerRpc."); }

        if(farmerObj != null)
            farmerObj.kill();

        farmerObj = null;
    }

    public Soldier addSoldier(int owner)
    {
        if (!NetworkManager.Singleton.IsServer) { Debug.LogWarning("Do not add new soldiers from the client! Wrap your method in a ServerRpc."); }

        Soldier soldier = SpriteRepo.Sprites["Soldier"].GetComponent<Soldier>();
        soldier.transform.position = hexCoord.world() + Vector2.left * .25f;
        soldier.Position = hexCoord;
        soldier.owner.Value = owner;
        SortedSoldiers[owner].Add(soldier);
        BattleFunctionality();
        soldier.GetComponent<NetworkObject>().Spawn();
        soldier.AddToTile(hexCoord);
        return soldier;
    }

    public void addSoldier(Soldier soldier, bool wasMoving = false)
    {
        soldier.AddToTile(hexCoord, wasMoving);

        soldier.transform.position = hexCoord.world() + .25f * Vector2.left;

        soldier.Position = hexCoord;

        BattleFunctionality();

        // Check for capturing
        if (tileOwner != -1 &&
            soldier.owner.Value != tileOwner &&
            SortedSoldiers[tileOwner].Count == 0 &&
            soldierCount == 0
            )
        {
            StartCapturing(soldier.owner.Value);
        }
    }

    public int otherPeoplesSoldiers(int me)
    {
        return soldierCount - SortedSoldiers[me].Count;
    }

    public bool sendSoldier(Hex end, int owner)
    {
        // Can we send?
        if (end != hexCoord && // We aren't sending to the same tile we're already on
            SortedSoldiers[owner].Count != 0 && // There is one of our soldiers
            TileManager.TM.isValidHex(end) // The tile is a valid one on the board
            )
        {
            Soldier soldier = FindFirstSoldierWithID(owner);

            if (soldier == null) // This shouldn't ever happen but it's here as a safeguard
                return false;

            SoldierTrip trip;

            if (!soldier.TryGetComponent(out trip))
            {
                trip = soldier.gameObject.AddComponent<SoldierTrip>();
            }

            bool pathPossible = trip.init(hexCoord,end);

            if (pathPossible)
            {
                soldier._RemoveFromTile(BoardHelperFns.HexToArray(hexCoord));
                soldier.RemoveFromTile(hexCoord);
                soldier.FadeIn();

                // Soldier does stuff to remove itself and moove smoothly
                BoardChecker.Checker.movingSoldierCount[soldier.owner.Value]++;
                soldier.StartTripAsClientRpc(BoardHelperFns.HexToArray(hexCoord), BoardHelperFns.HexToArray(end));

                // Stop capturing if there are no occupying soldiers
                if (soldierCount == 0)
                {
                    StopCapturing();
                }
            }
            else
            {
                Debug.Log("cannot Send");
            }

            return pathPossible;

        }
        else Debug.Log("cannot Send");

        return false;
    }

    /// <summary>
    /// the Gameobject representing a farmer
    /// </summary>
    public Farmer farmerObj = null;

    /// <summary>
    /// the tiles owner
    /// </summary>
    public int tileOwner = -1;

    /// <summary>
    /// name
    /// </summary>
    public string TileName;

    /// <summary>
    /// self explamnatory
    /// </summary>
    public float timeBetweenFrames = 0.5f;

    /// <summary>
    /// implement to set a crop art
    /// </summary>
    /// <returns></returns>
    public abstract List<TileArt> getCropArt();

    public Dictionary<int, List<Soldier>> SortedSoldiers;

    /// <summary>
    /// number of soldiers
    /// </summary>
    public int soldierCount {get { return new List<Soldier>(getSoldierEnumerator()).Count; } }

    // Iterate over all soldiers the tile has
    public IEnumerable<Soldier> getSoldierEnumerator()
    {
        foreach (var soldiersOfPlayer in SortedSoldiers.Values)
        {
            foreach (var soldier in soldiersOfPlayer)
            {
                yield return soldier;
            }
        }
    }

    public List<TileArt> tileArts;

    public override void LoadArt()
    {
        tileArts = new List<TileArt>();
        for(int i = 0; i < 5; i++)
        {
            tileArts.Add(getTileArt("Plant" + i.ToString()));
        }

        tileArts.AddRange(getCropArt());
    }

    /// <summary>
    /// the frame the tile is displaying
    /// </summary>
    public int frame;

    private double mid;

    /// <summary>
    /// the the framerate
    /// </summary>
    public float frameRate;


    private float frameInternal;


    void CaptureThis()
    {
        StopCapturing();
        int newTileOwner = GetCapturingPlayer();
        tileOwner = newTileOwner;
        TileSyncer.Syncer.SyncTileUpdate(hexCoord, new[] { CropTileSyncTypes.tileOwner });
        // Capture farmer as well
        if (containsFarmer)
        {
            removeFarmer();
            addFarmer();
            
            TileSyncer.Syncer.SyncTileUpdate(hexCoord, new[] { CropTileSyncTypes.containsFarmer });
        }
    }

    public double botReset () {
        double mid = tileArts.Count / 2; //optimal harvest level
        if(cropType == CropType.eggplant){
            mid = 4.5;
        } else {
            mid = 5.5;
        }
        int resistance = 1;
        if(cropType == CropType.eggplant) resistance = 10;
        if(cropType == CropType.rice) resistance = 5;

        float calc;
        double stage = frameInternal / frameRate;      

        calc = Mathf.Abs((float)(stage - mid));
        calc = Mathf.Pow(0.25f,calc);
        if (stage > mid){
            calc = 1.1f;
        }        
        if (calc < 0.75f){
            return 0.0f;
        }
        return Mathf.Max(0,calc-tileDamage/resistance);
    }

    public double hReset () {
        double mid = tileArts.Count / 2; //optimal harvest level
        if(cropType == CropType.eggplant){
            mid = 4.5;
        } else {
            mid = 5.5;
        }
        int resistance = 1;
        if(cropType == CropType.eggplant) resistance = 10;
        if(cropType == CropType.rice) resistance = 5;

        float calc;
        double stage = frameInternal / frameRate;      

        //Debug.Log(stage);

        calc = Mathf.Abs((float)(stage - mid));
        calc = Mathf.Pow(0.25f,calc);

        return Mathf.Max(0,calc-tileDamage/resistance);
    }

    //return crop level and reset crop growth
    public double reset () {
        double hr = hReset();
        timeLastPlanted = NetworkManager.Singleton.NetworkTime;
        frameInternal = 0;
        frame = 0;
        return hr;
    }

    public Soldier FindFirstSoldierWithID(int id)
    {
        return SortedSoldiers[id].Count == 0 ? null : SortedSoldiers[id][0];
    }

    /// BattleStuff
    /// 

    public GameObject battleCloud;

    public void BattleFunctionality()
    {
        PruneSoldiers();
        if (NetworkManager.Singleton.IsServer)
        {
            if (CheckForTileFight())
            {
                // Battling for the first time
                if (!battleOccurring)
                {
                    removeFarmer();
                    battleOccurring = true;
                    StartBattle();
                    TileSyncer.Syncer.SyncTileUpdate(hexCoord, new[] { CropTileSyncTypes.battleOccurring });
                    SoldierManager.SM.StartBattleClientRpc(BoardHelperFns.HexToArray(hexCoord));
                }
                Battle.BattleFunction(SortedSoldiers, soldierCount, tileOwner);
            }
            else if (battleOccurring)
            {
                battleOccurring = false;
                StopBattle();
                TileSyncer.Syncer.SyncTileUpdate(hexCoord, new[] { CropTileSyncTypes.battleOccurring });
                SoldierManager.SM.StopBattleClientRpc(BoardHelperFns.HexToArray(hexCoord));
                
            }
        }
    }


    public void StartBattle()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            //Control Display
            if (battleCloud == null)
            {
                battleCloud = SpriteRepo.Sprites["Cloud"];
                battleCloud.transform.position = TileManager.TM.HexToWorld(hexCoord);
                battleCloud.GetComponent<NetworkObject>().Spawn();
                battleCloud.GetComponent<BattleCloud>().AddToTile(hexCoord);
            }

            if (farmerObj != null)
                removeFarmer();
        }
        StopCapturing();
        FadeOutSoldiers();
        effect.Pause();
    }

    public void StopBattle()
    {
        if (battleCloud != null && NetworkManager.Singleton.IsServer)
        {
            GameObject.Destroy(battleCloud);
            battleCloud = null;
        }
        PruneSoldiers();
        FadeInSoldiers();
        OwnershipSwitch();
        effect.Resume();
    }

    private int GetCapturingPlayer()
    {
        int newOwner = -1;
        int currentCount = SortedSoldiers[tileOwner].Count;
        // Only switch if all of the previous soldiers are dead
        if (currentCount == 0)
        {
            int maxCount = 0;
            foreach (var soldiersOfPlayer in SortedSoldiers)
            {
                if (soldiersOfPlayer.Value.Count > maxCount)
                {
                    newOwner = soldiersOfPlayer.Key;
                }
            }
        }
        return newOwner;
    }

    private void OwnershipSwitch()
    {
        // We don't switch ownership of empty tiles
        if (tileOwner == -1)
        {
            return;
        }

        int newOwner = GetCapturingPlayer();

        if (newOwner != -1 && newOwner != tileOwner && NetworkManager.Singleton.IsServer)
        {
            StartCapturing(newOwner);
        }
    }

    // Removes all killed soldiers, which now show up as null
    void PruneSoldiers()
    {
        for (int playerId = 0; playerId < Repository.maxPlayers; playerId++)
        {
            int i = 0;
            while (i < SortedSoldiers[playerId].Count)
            {
                if (SortedSoldiers[playerId][i] == null)
                {
                    var fallenSoldier = SortedSoldiers[playerId][i];
                    SortedSoldiers[playerId].RemoveAt(i);
                    //fallenSoldier.Kill(); // This gave me an error
                }
                else
                {
                    i++;
                }
            }
        }
    }

    // Fades out whatever soldiers are likely to be visible
    void FadeOutSoldiers()
    {
        foreach (var soldier in getSoldierEnumerator())
        {
            soldier.FadeOut();
        }
    }

    // Fades in the soldiers
    void FadeInSoldiers()
    {
        foreach (var soldier in getSoldierEnumerator())
        {
            soldier.FadeIn();
        }
    }

    bool CheckForTileFight()
    {
        // Empty tile
        if (tileOwner == -1)
        {
            bool foundSoldier = false;
            foreach (var soldiersOfPlayer in SortedSoldiers.Values)
            {
                if (soldiersOfPlayer.Count > 0)
                {
                    // If we already have soldiers from another team, fight
                    if (foundSoldier)
                    {
                        return true;
                    }
                    // These are the first soldiers from any team
                    foundSoldier = true;
                }
            }
            return false;
        }
        // Owned tile
        else if (SortedSoldiers[tileOwner].Count != soldierCount)
        {
            // We have some of our soldiers and some other soldiers
            if (SortedSoldiers[tileOwner].Count > 0)
            {
                return true;
            }
            // All of our soldiers are dead, so we can't fight
            else
            {
                return false;
            }
        }
        return false;
    }

    void StartCapturing(int newowner)
    {
        timeStartedCapturing = NetworkManager.Singleton.NetworkTime;
        CropTileRpcHelper.CTRPC.StartCapturingClientRpc(BoardHelperFns.HexToArray(hexCoord), newowner);
        //effect.StartCapture(maxTimeToCapture, Repository.Central.TeamColors[newowner]);
    }

    // Called from CTRPC
    public void StartCapturingClientRpc(int newowner)
    {
        effect.StartCapture(maxTimeToCapture, Repository.Central.TeamColors[newowner]);
    }

    void StopCapturing()
    {
        timeStartedCapturing = -1f;
        CropTileRpcHelper.CTRPC.StopCapturingClientRpc(BoardHelperFns.HexToArray(hexCoord));
        //effect.StopCapture();
    }

    public void StopCapturingClientRpc()
    {
        timeStartedCapturing = -1f;
        effect.StopCapture();
    }
}




public enum CropType
{
    blankTile = -1,
    potato,
    carrot,
    rice,
    eggplant
}

public class BlankTile: TileTemp
{
    public override void Start()
    {
        base.Start();
        cropType = CropType.blankTile;

    }
    public override void LoadArt()
    {
        
    }

    public override List<TileArt> getCropArt()
    {
        return null;
    }


    public override void Behavior()
    {
        currentArt = null;
        base.BattleFunctionality();
    }
}


public class Rice : TileTemp
{
    public override void Start()
    {
        base.Start();
        cropType = CropType.rice;
    }

    public override List<TileArt> getCropArt()
    {
        TileName = "Rice";
        return new List<TileArt>
            {
                getTileArt("Wheat0"),
                getTileArt("Wheat1"),
                getTileArt("Wheat2")
            };
    }
}

public class Potato : TileTemp
{
    public override void Start()
    {
        base.Start();
        cropType = CropType.potato;
    }

    public override List<TileArt> getCropArt()
    {
        TileName = "Potato";
        return new List<TileArt>
            {
                getTileArt("Potato0"),
                getTileArt("Potato1"),
                getTileArt("Potato2")
            };
    }
}

public class Carrot : TileTemp
{
    public override void Start()
    {
        base.Start();
        cropType = CropType.carrot;
    }

    public override List<TileArt> getCropArt()
    {
        TileName = "Carrot";
        return new List<TileArt>
            {
                getTileArt("Carrot0"),
                getTileArt("Carrot1"),
                getTileArt("Carrot2")
            };
    }
}

public class Eggplant : TileTemp
{
    public override void Start()
    {
        base.Start();
        cropType = CropType.eggplant;
    }

    public override List<TileArt> getCropArt()
    {
        
        TileName = "Eggplant";
        return new List<TileArt>
            {
                getTileArt("Golden0"),
                getTileArt("Golden1"),
                getTileArt("Golden2"),
                getTileArt("Golden3"),
                getTileArt("Golden4"),
                getTileArt("Golden5"),
                getTileArt("Golden6"),
                getTileArt("Golden7"),
                getTileArt("Golden8")
            };
    }

    public override void LoadArt()
    {
        tileArts = getCropArt();
    }
}

public class HighLight: TileTemp
{
    public override void LoadArt()
    {
        currentArt = TileArtRepository.Art["Select"];
    }
    public override List<TileArt> getCropArt()
    {
        return null;
    }

    public override void Start()
    {

    }

    public override void Behavior()
    {

    }
    
}


public class SoldierDestination: TileTemp
{
    public override void LoadArt()
    {
        currentArt = TileArtRepository.Art["SoldierDestination"];
    }
    public override List<TileArt> getCropArt()
    {
        return null;
    }

    public override void Start()
    {

    }

    public override void Behavior()
    {

    }
}
