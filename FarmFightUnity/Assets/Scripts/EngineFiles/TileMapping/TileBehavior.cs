using System;
using System.Collections.Generic;
using UnityEngine;


public abstract class TileTemp
{
    public TileArt currentDraw;


    public Hex hexCoord { get; set; }


    public Vector3Int cellCoord
    {
        get { return (Vector3Int)hexCoord.Cell(); }
        set { hexCoord = Hex.fromCell(value); }

    }


    public virtual void StartBehavior()
    {

    }

    public virtual void Behavior()
    {

    }

    public virtual void EndBehavior()
    {

    }


}
