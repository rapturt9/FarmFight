using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;


/// <summary>
/// REMAKE REMAKE BAD BAD
/// </summary>
public class TileInterFace
{
    // Unity uses different coordinate systems than i do for its Cell system,
    // for your and my sake ive built in an automatic conversion system

    public Hex hexCoord { get; set;}

    public TileTemp tileInternal;

    public TileTemp Tile
    {
        set
        {
            changeTile(value);
        }
        get
        {
            return tileInternal;
        }
    }


    public Vector3Int cellCoord
    {
        get { return (Vector3Int)hexCoord.Cell(); }
        set { hexCoord = Hex.fromCell(value); }

    }


    public TileInterFace(Hex coord, TileTemp tile)
    {
        this.hexCoord = coord;

        this.Tile = tile;

        Begin();
    }

    public void Draw(Tilemap tilemap)
    {
        if(Tile.currentArt != null)
            tilemap.SetTile(cellCoord, tileInternal.currentArt);
        else
            tilemap.SetTile(cellCoord,null);

    }

    public void Update()
    {
        tileInternal.Behavior();

    }

    public void changeTile(TileTemp tile)
    {
        if(tileInternal != null)
            End();
        tileInternal = tile;
        Begin();

    }


    public void Begin()
    {
        tileInternal.init(hexCoord);
    }

    public void End()
    {
        tileInternal.End();
    }



}
