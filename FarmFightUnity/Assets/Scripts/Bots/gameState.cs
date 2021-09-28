using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable.Collections;
using MLAPI.Messaging;

public class GameState : NetworkBehaviour
{
    public TileHandler tileHandler;

    public NetworkDictionary<Hex, string> cropTilesSync = new NetworkDictionary<Hex, string>();
    // Hex coord, (crop#, time planted/time last clicked, farmer or not)
    public Dictionary<Hex, (int, float, bool)> cropTiles = new Dictionary<Hex, (int, float, bool)>();

    public List<Hex> hexCoords;

    private static ValueTuple<int, float, bool> emptyTileTuple = (-1, 0.0f, false);
    private static string emptyTileString;

    enum CropTileSyncTypes
    {
        blankTile = -1,
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
        while (true)
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
        if (IsClient)
        {
            DeserializeBoard();
        }
        foreach (var coord in hexCoords){
            var tileTuple = SerializeTile(tileHandler[coord]);
            // Only updates our game state if something has changed
            if (tileTuple != cropTiles[coord])
            {
                cropTiles[coord] = tileTuple;
                if (IsServer)
                {
                    ChangeTileSync(coord, SerializeTileTuple(tileTuple));
                }
                else if (IsClient)
                {
                    ChangeTileSyncServerRpc(new int[2] { coord.x, coord.y }, SerializeTileTuple(tileTuple));
                }
            }
        }
        print(cropTilesSync[new Hex(0, 0)]);
    }

    // Turns TileTemp into tuple
    ValueTuple<int, float, bool> SerializeTile(TileTemp tile)
    {
        var tileTuple = emptyTileTuple;
        // Crop tile
        if (!(tile is BlankTile))
        {
            CropTile tileInfo = (CropTile)tile;
            int cropNum = tileInfo.cropType;

            tileTuple = (cropNum, 0.0F, false);
        }
        return tileTuple;
    }

    // Turns tile tuple into string, for network syncing
    string SerializeTileTuple(ValueTuple<int, float, bool> tileData)
    {
        return tileData.Item1.ToString() + "," + tileData.Item2.ToString() + "," + tileData.Item3.ToString();
    }

    // Turns string into tile tuple
    ValueTuple<int, float, bool> DeserializeTileString(string tileString)
    {
        string[] split = tileString.Split(',');
        int cropNum = int.Parse(split[(int)CropTileSyncTypes.cropNum]);
        float lastPlanted = float.Parse(split[(int)CropTileSyncTypes.lastPlanted]);
        bool containsFarmer = bool.Parse(split[(int)CropTileSyncTypes.containsFarmer]);
        return (cropNum, lastPlanted, containsFarmer);
    }

    // Turns all of the tile strings into actual board tiles
    void DeserializeBoard()
    {
        foreach (var coord in hexCoords)
        {
            var tileTuple = DeserializeTileString(cropTilesSync[coord]);
            // Change the game state
            if (tileTuple != cropTiles[coord])
            {
                CropTypes cropNum = (CropTypes)tileTuple.Item1;
                float lastPlanted = tileTuple.Item2;
                bool containsFarmer = tileTuple.Item3;

                // Initializes tile
                TileTemp tile;
                if (cropNum == CropTypes.potato)
                    tile = new Potato();
                else if (cropNum == CropTypes.wheat)
                    tile = new Wheat();
                else if (cropNum == CropTypes.carrot)
                    tile = new Carrot();
                else
                    tile = new BlankTile();

                // Sets tile
                tileHandler[coord] = tile;

            }

        }
    }

    // Functions for Server Rpcs
    void ChangeTileSync(Hex coord, string serializedString)
    {
        cropTilesSync[coord] = serializedString;
    }

    // Server Rpcs
    [ServerRpc]
    void ChangeTileSyncServerRpc(int[] coords, string serializedString)
    {
        ChangeTileSync(new Hex(coords[0], coords[1]), serializedString);
    }
}
