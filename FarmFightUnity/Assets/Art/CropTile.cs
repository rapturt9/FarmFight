using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CropTile : TileTemp
{

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
        frame = 0; 
    }

    public int frame;


    public override void Behavior()
    {
        frame += 1;
        frame %= tileArts.Count * 60;

        currentArt = tileArts[frame / 60];
    }

}



public class Wheat : CropTile
{
    public override TileArt getCropArt()
    {
        return getTileArt("Wheat");
    }
}

public class Potato : CropTile
{
    public override TileArt getCropArt()
    {
        return getTileArt("Potato");
    }
}

public class Carrot : CropTile
{
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
