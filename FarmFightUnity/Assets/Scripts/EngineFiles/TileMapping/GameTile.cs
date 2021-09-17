using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;


/// <summary>
/// its a struct so 
/// </summary>
public struct GameTile
{
    public static GameTile empty = new GameTile(Hex.zero,null,null);

    public TileArt art;

    public TileBehavior behavior;


    // Unity uses different coordinate systems than i do for its Cell system,
    // for your and my sake ive built in an automatic conversion system

    public Hex hexCoord { get; set;}

    public Vector3Int cellCoord
    {
        get { return (Vector3Int)hexCoord.Cell(); }
        set { hexCoord = Hex.fromCell(value); }
        
    }


    public GameTile(Hex coord ,TileBehavior behavior, TileArt art)
    {
        this.hexCoord = coord;
        this.behavior = behavior;
        this.art = art;

    }

    public GameTile(Vector3Int coord, TileBehavior behavior, TileArt art)
    {
        hexCoord = Hex.fromCell(coord);
        this.behavior = behavior;
        this.art = art;
    }

    public GameTile(Vector3Int coord, TileBehavior behavior)
    {
        this = new GameTile(coord, behavior, behavior.art);
    }

    public GameTile(Hex coord, TileBehavior behavior)
    {
        this = new GameTile(coord, behavior, behavior.art);
        
    }

    public void Draw(Tilemap tilemap)
    {
        if(art != null)
            tilemap.SetTile(cellCoord, art);
        
    }

    public void Update()
    {
        behavior.Behavior();
    }

    
    

}




