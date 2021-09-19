﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class TileHandler : MonoBehaviour
{
    public string Name;

    [SerializeField]
    private Hex selected;

    Dictionary<Hex, TileInterFace> Tiles;

    

    Tilemap tilemap;

    public TileTemp this[Hex hex]
    {
        get
        {
            return Tiles[hex].Tile;
        }

        set
        {
            if (Tiles.ContainsKey(hex))
            {
                
                Tiles[hex].Tile = value;
            }
            
        }
    }

    public void Init()
    {



        TryGetComponent(out tilemap);
        if(tilemap == null)
        {
            gameObject.AddComponent<Tilemap>();
        }

        fillTiles();
    }

    

    private void fillTiles()
    {
        Dictionary<Hex, TileInterFace> temp = BoardHelperFns.BoardFiller(4);
        Tiles = new Dictionary<Hex, TileInterFace>();
        foreach(var coord in temp.Keys)
        {
            
                Tiles[coord] = new TileInterFace(coord,new TileTemp());
        }

        Redraw();
    }

    private void Redraw()
    {
        foreach(var tile in Tiles.Values)
        {
            tile.Draw(tilemap);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Redraw();
    }

    
}
