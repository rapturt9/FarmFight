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

    public TileTemp Tile;
    

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
        tilemap.SetTile(cellCoord, Tile.currentArt);
        
    }

    public void Update()
    {
        Tile.Behavior();

    }

    public void changeTile(TileTemp tile)
    {
        End();
        Tile = tile;
        Begin();

    }


    public void Begin()
    {
        Tile.init(hexCoord);
    }

    public void End()
    {
        Tile.End();
    }
    
    

}




