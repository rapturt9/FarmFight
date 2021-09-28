using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CropTile : TileTemp
{
    public CropType cropType = CropType.blankTile;
    public float timeLastPlanted = 0f;
    public float timeBetweenFrames = 0.5f;

    public abstract TileArt getCropArt();
    

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
        timeLastPlanted = Time.time;
        frame = 0;
    }

    public int frame;


    public override void Behavior()
    {
        frame += 1;
        frame %= tileArts.Count * 60;

        int artFrame = (int)((Time.time - timeLastPlanted) / timeBetweenFrames);
        // Rolling over
        if (artFrame == tileArts.Count)
        {
            artFrame = 0;
            timeLastPlanted = Time.time;
        }
        currentArt = tileArts[artFrame];
    }

}


public enum CropType
{
    blankTile = -1,
    potato,
    wheat,
    carrot,
    rice
}

public class Wheat : CropTile
{
    public override void Start()
    {
        base.Start();
        cropType = CropType.wheat;
    }

    public override TileArt getCropArt()
    {
        return getTileArt("Wheat");
    }
}

public class Potato : CropTile
{
    public override void Start()
    {
        base.Start();
        cropType = CropType.potato;
    }

    public override TileArt getCropArt()
    {
        return getTileArt("Potato");
    }
}

public class Carrot : CropTile
{
    public override void Start()
    {
        base.Start();
        cropType = CropType.carrot;
    }

    public override TileArt getCropArt()
    {
        return getTileArt("Carrot");
    }
}


public class BlankTile: TileTemp
{
    public override void LoadArt()
    {
        currentArt = null;
    }
}


public class HighLight: TileTemp
{
    public override void LoadArt()
    {
        currentArt = TileArtRepository.Art["Select"];
    }
}
