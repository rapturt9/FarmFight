using System;
using System.Collections.Generic;
using UnityEngine;



public class TileTemp
{
    public TileArt currentArt;


    public Hex hexCoord { get; set; }


    public Vector3Int cellCoord
    {
        get { return (Vector3Int)hexCoord.Cell(); }
        set { hexCoord = Hex.fromCell(value); }

    }


    public virtual void Start()
    {

    }

    public virtual void LoadArt()
    {
        currentArt = TileArtRepository.Art["Test"];

    }

    public void init(Hex hex)
    {
        LoadArt();
        Start();
        hexCoord = hex;
    }

    public virtual void Behavior()
    {

    }

    public virtual void End()
    {

    }


    public TileArt getTileArt(string name)
    {
        return TileArtRepository.Art[name];
    }

}
