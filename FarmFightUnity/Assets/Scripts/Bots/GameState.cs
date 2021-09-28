using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable.Collections;
using MLAPI.Messaging;
using MLAPI.Serialization;

public class GameState : MonoBehaviour
{
    public TileHandler tileHandler;

    // Hex coord, (crop#, time planted/time last clicked, farmer or not)
    public Dictionary<Hex, TileSyncData> cropTiles = new Dictionary<Hex, TileSyncData>();

    public List<Hex> hexCoords;

    private static TileSyncData emptyTileSyncData = new TileSyncData(CropType.blankTile, 0.0f, false);

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
        hexCoords = BoardHelperFns.HexList(3);
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
            cropTiles[coord] = emptyTileSyncData;
        }
    }

    public void updateGameState()
    {
        foreach (var coord in hexCoords)
        {
            var tileData = SerializeTile(tileHandler[coord]);
            //Only updates our game state if something has changed
            if (tileData != cropTiles[coord])
            {
                cropTiles[coord] = tileData;
            }
        }
    }

    // Turns TileTemp into tuple
    public static TileSyncData SerializeTile(TileTemp tile)
    {
        var tileData = emptyTileSyncData;
        // Not blank, so crop
        if (!(tile is BlankTile))
        {
            CropTile tileInfo = (CropTile)tile;
            // TODO sync time and farmer
            tileData = new TileSyncData(tileInfo.cropType, 0.0F, false);
        }
        return tileData;
    }

    // Turns all of the tile strings into actual board tiles
    //void DeserializeBoard()
    //{
    //    foreach (var coord in hexCoords)
    //    {
    //        var tileTuple = DeserializeTileString(cropTiles[coord]);
    //        // Change the game state
    //        if (tileTuple != cropTiles[coord])
    //        {
    //            CropType cropNum = (CropType)tileTuple.Item1;
    //            float lastPlanted = tileTuple.Item2;
    //            bool containsFarmer = tileTuple.Item3;

    //            // Initializes tile
    //            TileTemp tile;
    //            if (cropNum == CropType.potato)
    //                tile = new Potato();
    //            else if (cropNum == CropType.wheat)
    //                tile = new Wheat();
    //            else if (cropNum == CropType.carrot)
    //                tile = new Carrot();
    //            else
    //                tile = new BlankTile();

    //            // Sets tile
    //            tileHandler[coord] = tile;

    //        }

    //    }
    //}

    // Goes from TileSyncData to TileTemp
    public static TileTemp DeserializeTile(TileSyncData tileData)
    {
        CropType cropNum = tileData.cropType;
        float timeLastPlanted = tileData.timeLastPlanted;
        bool containsFarmer = tileData.containsFarmer;

        // Initializes tile
        TileTemp tile = new BlankTile();
        if (cropNum != CropType.blankTile)
        {
            if (cropNum == CropType.potato)
                tile = new Potato();
            else if (cropNum == CropType.wheat)
                tile = new Wheat();
            else if (cropNum == CropType.carrot)
                tile = new Carrot();
        }
        // TODO sync timeLimePlanted and containsFarmer

        return tile;
    }
}


public struct TileSyncData : INetworkSerializable
{
    public CropType cropType;
    public float timeLastPlanted;
    public bool containsFarmer;

    public TileSyncData(CropType cropTypeArg, float timeLastPlantedArg, bool containsFarmerArg) : this()
    {
        cropType = cropTypeArg;
        timeLastPlanted = timeLastPlantedArg;
        containsFarmer = containsFarmerArg;
    }

    public void NetworkSerialize(NetworkSerializer serializer)
    {
        serializer.Serialize(ref cropType);
        serializer.Serialize(ref timeLastPlanted);
        serializer.Serialize(ref containsFarmer);
    }

    // Equality
    public static bool operator ==(TileSyncData a, TileSyncData b) => a.Equals(b);

    public static bool operator !=(TileSyncData a, TileSyncData b) => !(a == b);

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        var b = (TileSyncData)obj;
        return (cropType == b.cropType) && (timeLastPlanted == b.timeLastPlanted) && (containsFarmer == b.containsFarmer);
    }

    public override int GetHashCode()
    {
        return (cropType, timeLastPlanted, containsFarmer).GetHashCode();
    }

    // String representation
    public override string ToString()
    {
        return (cropType, timeLastPlanted, containsFarmer).ToString();
    }
}
