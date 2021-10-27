using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;

public abstract class TileTemp : TileTempDepr
{
    public CropType cropType = CropType.blankTile;
    public float timeLastPlanted = 0f;
    public bool containsFarmer
    {
        get { return farmerObj != null; }
    }
    public bool battleOccurring = false;

    // Something changed so TileHandler should sync it
    public bool dirty = false;

    public override void Start()
    {
        SortedSoldiers = new Dictionary<int, List<Soldier>>();
        for (int i = 0; i < Repository.maxPlayers; i++)
            SortedSoldiers[i] = new List<Soldier>();

        if (timeLastPlanted == 0f)
            timeLastPlanted = NetworkManager.Singleton.NetworkTime;
        frame = 0;
        frameRate = 60;
        frameInternal = 0;
    }

    public GameObject addFarmer()
    {
        if (!NetworkManager.Singleton.IsServer) { Debug.LogWarning("Do not add farmers from the client! Wrap your method in a ServerRpc."); }
        
        farmerObj = SpriteRepo.Sprites["Farmer"];
        farmerObj.GetComponent<Farmer>().Owner.Value = tileOwner;
        farmerObj.transform.position = (Vector2)TileManager.TM.HexToWorld(hexCoord) + .25f * Vector2.right;
        return farmerObj;
    }

    public void removeFarmer()
    {
        if (!NetworkManager.Singleton.IsServer) { Debug.LogWarning("Do not remove farmers from the client! Wrap your method in a ServerRpc."); }

        GameObject.Destroy(farmerObj);
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
        return soldier;
    }

    public void addSoldier(Soldier soldier)
    {
        soldier.AddToTile(hexCoord);

        soldier.transform.position = hexCoord.world() + .25f * Vector2.left;

        soldier.Position = hexCoord;

        BattleFunctionality();

        //Debug.Log($"Soldier added to {hexCoord}");
    }

    

    public bool sendSoldier(Hex end, int localPlayerId)
    {
        if (soldierCount != 0 &&
            end != hexCoord &&
            TileManager.TM.isValidHex(end) &&
            SortedSoldiers[localPlayerId].Count != 0)
        {
            Soldier soldier = FindFirstSoldierWithID(localPlayerId);

            if (soldier == null)
                return false;

            SoldierTrip trip;

            if (!soldier.TryGetComponent(out trip))
            {
                trip = soldier.gameObject.AddComponent<SoldierTrip>();
            }

            bool pathPossible = trip.init(hexCoord,end);

            if (pathPossible)
            {
                SortedSoldiers[soldier.owner.Value].Remove(soldier);
                // Make trip smooth for clients
                soldier.StartTripAsClientRpc(BoardHelperFns.HexToArray(hexCoord), BoardHelperFns.HexToArray(end));
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

    //destroys all associated gameobjects
    public override void End()
    {
        // Destroying objects messed up things for me with networking - Eli

        //GameObject.Destroy(farmerObj);
        //foreach (var soldier in soldiers)
        //{
        //    //GameObject.Destroy(soldier.gameObject);
        //}
    }


   

    /// <summary>
    /// the Gameobject representing a farmer
    /// </summary>
    public GameObject farmerObj = null;

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

    /// <summary>
    /// the the framerate
    /// </summary>
    public float frameRate;


    private float frameInternal;

    public override void Behavior()
    {
        if(frameInternal >= tileArts.Count * frameRate){
            frameInternal = tileArts.Count * frameRate;
        } 
        else {
            frameInternal = (int)((NetworkManager.Singleton.NetworkTime - timeLastPlanted) * frameRate);
        }

        frame = (int) (frameInternal / frameRate);

        //farmer autoharvest
        if(containsFarmer && frame >= 6)
        {
            double moneyToAdd = reset();
            if (tileOwner == Repository.Central.localPlayerId)
                Repository.Central.money += moneyToAdd;
        }

        if (!battleOccurring)
        {
            if (0 <= frame && frame < tileArts.Count)
            {
                currentArt = tileArts[frame];
            }
            else
            {
                currentArt = tileArts[tileArts.Count - 1];
            }
        }
        else
        {
            currentArt = null;
        }

        BattleFunctionality();
    }


    public double hReset () {
        double mid = tileArts.Count / 2; //optimal harvest level
        if(cropType == CropType.eggplant){
            mid = 4.5;
        } else {
            mid = 5.5;
        }

        float calc;
        double stage = frameInternal / frameRate;

        //Debug.Log(stage);

        calc = Mathf.Abs((float)(stage - mid));
        calc = Mathf.Pow(0.25f,calc);

        return calc;
    }

    //return crop level and reset crop growth
    public double reset () {
        double hr = hReset();
        timeLastPlanted = NetworkManager.Singleton.NetworkTime;
        frameInternal = 0;
        frame = 0;
        return hr;
    }

    void killSoldier(Soldier soldier)
    {
        soldier.Kill();
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
                    battleOccurring = true;
                    StartBattle();
                    SoldierManager.SM.StartBattleClientRpc(BoardHelperFns.HexToArray(hexCoord));
                }
                Battle.BattleFunction(SortedSoldiers, soldierCount, tileOwner);
            }
            else if (battleOccurring)
            {
                battleOccurring = false;
                StopBattle();
                SoldierManager.SM.StopBattleClientRpc(BoardHelperFns.HexToArray(hexCoord));
            }
        }
        if (battleOccurring)
        {
            FadeOutSoldiers();
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
        battleOccurring = true;
        FadeOutSoldiers();
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
        battleOccurring = false;
        Debug.Log("Stopping battle");
    }

    private void OwnershipSwitch()
    {
        // We don't switch ownership of empty tiles
        if (tileOwner == -1)
        {
            return;
        }

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

            if (newOwner != -1 && newOwner != tileOwner)
            {
                tileOwner = newOwner;
                if (NetworkManager.Singleton.IsServer)
                {
                    dirty = true;
                }
            }
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
                    SortedSoldiers[playerId].RemoveAt(i);
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

    // Fades out whatever soldier is likely to be visible
    void FadeInSoldiers()
    {
        //// No soldiers to fade in
        //if (soldierCount < 0)
        //{
        //    return;
        //}
        //// Fade in normally
        //else if (tileOwner != -1 && SortedSoldiers[tileOwner].Count > 0)
        //{
        //    SortedSoldiers[tileOwner][0].FadeIn();
        //    return;
        //}
        //// We are occupied by hostile soldiers, so fade in whichever we find first
        //else if (soldierCount > 0)
        //{
        //    var iter = getSoldierEnumerator().GetEnumerator();
        //    iter.MoveNext();
        //    iter.Current.FadeIn();
        //}
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
