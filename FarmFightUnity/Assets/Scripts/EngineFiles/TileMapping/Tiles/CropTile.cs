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

    public void addFarmer()
    {
        if (!NetworkManager.Singleton.IsServer) { Debug.LogWarning("Do not add farmers from the client! Wrap your method in a ServerRpc."); }

        farmerObj = SpriteRepo.Sprites["Farmer" + Repository.Central.localPlayerId.ToString(), hexCoord];
        farmerObj.transform.position = hexCoord.world() + .25f * Vector2.right;
    }

    public void removeFarmer()
    {
        if (!NetworkManager.Singleton.IsServer) { Debug.LogWarning("Do not remove farmers from the client! Wrap your method in a ServerRpc."); }

        GameObject.Destroy(farmerObj);
        farmerObj = null;
    }

    public void addSoldier(int owner)
    {
        if (!NetworkManager.Singleton.IsServer) { Debug.LogWarning("Do not add new soldiers from the client! Wrap your method in a ServerRpc."); }

        Soldier soldier = SpriteRepo.Sprites["Soldier" + Repository.Central.localPlayerId.ToString(), hexCoord].GetComponent<Soldier>();
        soldier.transform.position = hexCoord.world() + Vector2.left * .25f;
        soldier.owner.Value = owner;
        soldiers.Add(soldier);
    }

    public void addSoldier(Soldier soldier)
    {
        // It's fine to add existing soldiers directly from the client

        soldier.AddToTile(hexCoord);

        soldier.transform.position = hexCoord.world() + .25f * Vector2.left;

        //Debug.Log($"Soldier added to {hexCoord}");

        if(soldierCount > 1)
        {
            soldier.FadeOut();
        }

        
    }



    public bool sendSoldier(Hex end, int localPlayerId)
    {
        // Only send if there is a soldier, the end isn't the start, the hex is on the board, and if the soldier is ours
        if (soldierCount != 0 &&
            end != hexCoord &&
            TileManager.TM.isValidHex(end) &&
            soldiers[0].owner.Value == localPlayerId)
        {
            //GameObject.Destroy(soldiers[0].GetComponent<SoldierTrip>());

            Soldier soldier = soldiers[0];
            SoldierTrip trip;

            if (!soldier.TryGetComponent(out trip))
            {
                soldiers[0].gameObject.AddComponent<SoldierTrip>()
                .init(hexCoord, end);
            }
            else
                trip.init(hexCoord, end);

            soldiers.RemoveAt(0);

            if (soldierCount != 0)
            {
                soldiers[0].FadeIn();
            }
            return true;
        }
        else Debug.Log("cannot Send");
        return false;
    }

    //destroys all associated gameobjects
    public override void End()
    {
        // Destroying objects messed up things for me with networking - Eli

        //GameObject.Destroy(farmerObj);
        foreach (var soldier in soldiers)
        {
            //GameObject.Destroy(soldier.gameObject);
        }
    }


    /// <summary>
    /// list of the soldiers on the tile
    ///
    /// *may need to be split into multiple lists depending on what fights look like*
    /// If you want to add to the list of soldiers, use soldier.AddToTile(hexCoord), NOT soldiers.Add(soldier)
    /// </summary>
    public List<Soldier> soldiers = new List<Soldier>();

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

    /// <summary>
    /// number of soldiers
    /// </summary>
    public int soldierCount { get { return soldiers.Count; } }

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

    public override void Start()
    {
        soldiers = new List<Soldier>();

        if (timeLastPlanted == 0f)
            timeLastPlanted = NetworkManager.Singleton.NetworkTime;
        frame = 0;
        frameRate = 60;
        frameInternal = 0;
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
        } else {
            frameInternal = (int)((NetworkManager.Singleton.NetworkTime - timeLastPlanted) * frameRate);
            //frameInternal += 1;
        }


        frame = (int) (frameInternal / frameRate);


        //farmer autoharvest
        if(containsFarmer && frame >= 6)
        {
            double moneyToAdd = reset();
            if (tileOwner == Repository.Central.localPlayerId)
                Repository.Central.money += moneyToAdd;
        }

        if(0 <= frame && frame <= 7){
            currentArt = tileArts[frame];
        } else {
            currentArt = tileArts[7];
        }
    }

    //return crop level and reset crop growth
    public double reset () {
        double mid = 5.5; //optimal harvest level

        double calc;
        double stage = frameInternal / frameRate;

        calc = Mathf.Abs((float)(stage - mid));
        calc = mid - calc;

        timeLastPlanted = NetworkManager.Singleton.NetworkTime;
        frameInternal = 0;
        frame = 0;
        return calc;
    }
}

public enum CropType
{
    blankTile = -1,
    potato,
    carrot,
    rice
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
