using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TileTemp : TileTempDepr
{
    public CropType cropType = CropType.blankTile;
    public float timeLastPlanted = 0f;
    public bool containsFarmer = false;
    public int tileOwner = -1;
    public string TileName;
    public float timeBetweenFrames = 0.5f;

    public abstract TileArt getCropArt();

    public int soldiers = 0;

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
        if (timeLastPlanted == 0f)
            timeLastPlanted = Time.time;
        frame = 0;
        frameRate = 60;
        frameInternal = 0;
    }

    public int frame;
    public float frameRate;
    private float frameInternal;

    public override void Behavior()
    {
        if(frameInternal >= tileArts.Count * frameRate){
            frameInternal = tileArts.Count * frameRate - 1;
        } else {
            frameInternal = (int)((Time.time - timeLastPlanted) * frameRate);
            //frameInternal += 1;
        }


        frame = (int) (frameInternal / frameRate);

        if(0 <= frame && frame <= 6){
            currentArt = tileArts[frame];
        } else {
            currentArt = tileArts[6];
        }

    }

    //return crop level and reset crop growth
    public double reset () {
        double mid = 5.5; //optimal harvest level

        double calc;
        double stage = frameInternal / frameRate;

        calc = Mathf.Abs((float)(stage - mid));
        calc = mid - calc;

        timeLastPlanted = Time.time;
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
