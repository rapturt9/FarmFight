using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable.Collections;

public class GameState : NetworkBehaviour
{
    private bool gameIsRunning = true;

    public TileHandler tileHandler;

    public NetworkDictionary<Hex, string> cropTilesSync = new NetworkDictionary<Hex, string>();
    // Hex coord, (crop#, time planted/time last clicked, farmer or not)
    public Dictionary<Hex, (int, float, bool)> cropTiles = new Dictionary<Hex, (int, float, bool)>();

    public List<Hex> hexCoords;

    private static ValueTuple<int, float, bool> emptyTileTuple = (-1, 0.0f, false);
    private static string emptyTileString;

    enum CropTileSyncTypes
    {
        cropNum,
        lastPlanted,
        containsFarmer
    }

    // Start is called before the first frame update
    void Start()
    {
        emptyTileString = SerializeTileTuple(emptyTileTuple);
        hexCoords = BoardHelperFns.HexList(3);
    }

    public override void NetworkStart()
    {

    }

    public void Init()
    {
        updateGameStateFirstTime();
        StartCoroutine(collectGameData());
    }

    IEnumerator collectGameData()
    {
        while (gameIsRunning)
        {
            yield return new WaitForSeconds(0.5F);
            updateGameState();
        }

    }

    // Makes every tile empty
    void updateGameStateFirstTime()
    {
        foreach (var coord in hexCoords)
        {
            cropTiles[coord] = emptyTileTuple;
            cropTilesSync[coord] = emptyTileString;
        }
    }

    public void updateGameState () {
        foreach (var coord in hexCoords){
            var serialized = SerializeTile(tileHandler[coord]);
            // Only updates our game state if something has changed
            if (serialized != cropTiles[coord])
            {
                cropTiles[coord] = serialized;
                cropTilesSync[coord] = SerializeTileTuple(serialized);
            }
        }
        print(cropTilesSync[new Hex(0, 1)]);
    }

    // Turns TileTemp into tuple
    ValueTuple<int, float, bool> SerializeTile(TileTemp tile)
    {
        var tileData = emptyTileTuple;
        // Crop tile
        if (!(tile is BlankTile))
        {
            CropTile tileInfo = (CropTile)tile;
            int cropNum = tileInfo.cropType;

            tileData = (cropNum, 0.0F, false);
        }
        return tileData;
    }

    // Turns tile tuple into string, for network syncing
    string SerializeTileTuple(ValueTuple<int, float, bool> tileData)
    {
        return tileData.Item1.ToString() + "," + tileData.Item2.ToString() + "," + tileData.Item3.ToString();
    }

    // Turns string into tile tuple
    ValueTuple<int, float, bool> DeserializeTile(string tileString)
    {
        string[] split = tileString.Split(',');
        int cropNum = int.Parse(split[(int)CropTileSyncTypes.cropNum]);
        float lastPlanted = float.Parse(split[(int)CropTileSyncTypes.lastPlanted]);
        bool containsFarmer = bool.Parse(split[(int)CropTileSyncTypes.containsFarmer]);
        return (cropNum, lastPlanted, containsFarmer);
    }
}
