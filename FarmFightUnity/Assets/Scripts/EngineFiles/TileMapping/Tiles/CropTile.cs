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
        set
        {
            if (value)
            {

                farmerObj = SpriteRepo.Sprites["Farmer", hexCoord];
                farmerObj.transform.position = hexCoord.world() + .25f * Vector2.right;
            }
            else
            {
                GameObject.Destroy(farmerObj);
                farmerObj = null;
            }

        }
    }

    public void addSoldier()
    {

        Soldier soldier = SpriteRepo.Sprites["Soldier", hexCoord].GetComponent<Soldier>();

        

        soldier.gameObject.SetActive(true);

        soldier.transform.position = hexCoord.world()+Vector2.left*.25f;
        
        

        soldiers.Add(soldier);

        Debug.Log($"Soldier added to {hexCoord}");
    }


    public void sendSoldier(Hex end)
    {
        if (soldierCount != 0)
        {
            GameObject.Destroy(soldiers[0].GetComponent<SoldierTrip>());

            SoldierTrip temp = soldiers[0].gameObject.AddComponent<SoldierTrip>();

            temp.init(hexCoord, end);


            soldiers.RemoveAt(0);

            if (soldierCount != 0)
            {
                soldiers[0].FadeIn();

                //soldiers[0].transform.position = hexCoord.world() + Vector2.left * .25f;
            }
        }
    }

    

    /// <summary>
    /// list of the soldiers on the tile
    ///
    /// *may need to be split into multiple lists depending on what fights look like*
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
    /// IDK WHAT THIS IS ABOUT BUT I ASSUME IT IS IMPORTANT
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
    public abstract TileArt getCropArt();

    /// <summary>
    /// number of soldiers
    /// </summary>
    public int soldierCount { get { return soldiers.Count; } }

    public List<TileArt> tileArts;

    public override void LoadArt()
    {
        tileArts = new List<TileArt>();
        for(int i = 0; i < 7; i++)
        {
            tileArts.Add(getTileArt("Plant" + i.ToString()));

            if(i == 4)
            {
                tileArts.Add(getCropArt());
            }
        }
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
            frameInternal = tileArts.Count * frameRate - 1;
        } else {
            frameInternal = (int)((NetworkManager.Singleton.NetworkTime - timeLastPlanted) * frameRate);
            //frameInternal += 1;
        }


        frame = (int) (frameInternal / frameRate);


        if(containsFarmer & frame == 6)
        {
            Repository.Central.money += reset();
        }

        if(0 <= frame && frame <= 6){
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
    public override TileArt getCropArt()
    {
        TileName = "Blank";
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

    public override TileArt getCropArt()
    {
        TileName = "Rice";
        return getTileArt("Wheat");
    }
}

public class Potato : TileTemp
{
    public override void Start()
    {
        base.Start();
        cropType = CropType.potato;
    }

    public override TileArt getCropArt()
    {
        TileName = "Potato";
        return getTileArt("Potato");
    }
}

public class Carrot : TileTemp
{
    public override void Start()
    {
        base.Start();
        cropType = CropType.carrot;
    }

    public override TileArt getCropArt()
    {
        TileName = "Carrot";
        return getTileArt("Carrot");
    }
}

public class HighLight: TileTemp
{
    public override void LoadArt()
    {
        currentArt = TileArtRepository.Art["Select"];
    }
    public override TileArt getCropArt()
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
