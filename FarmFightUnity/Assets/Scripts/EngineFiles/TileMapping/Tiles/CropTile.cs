using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TileTemp : TileTempDepr
{
    public string TileName;

    public abstract TileArt getCropArt();

    public bool hasFarmer = false;

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
        frame = 0;
        frameRate = 60;
        frameInternal = 0;
    }

    public int frame;
    public float frameRate;
    private float frameInternal;

    public override void Behavior()
    {
        frameInternal += 1;
        frameInternal %= tileArts.Count * frameRate;

        frame = (int) (frameInternal / frameRate);


        currentArt = tileArts[frame];
    }

}


public class BlankTile: TileTemp
{
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
    public override TileArt getCropArt()
    {
        TileName = "Rice";
        return getTileArt("Wheat");
    }
}

public class Potato : TileTemp
{
    public override TileArt getCropArt()
    {
        TileName = "Potato";
        return getTileArt("Potato");
    }
}

public class Carrot : TileTemp
{
    public override TileArt getCropArt()
    {
        
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
