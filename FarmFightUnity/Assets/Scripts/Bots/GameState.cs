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

    private static TileSyncData emptyTileSyncData = new TileSyncData(CropType.blankTile, 0.0f, false, -1);

    enum CropTileSyncTypes
    {
        blankTile = -1,
        cropNum,
        lastPlanted,
        containsFarmer,
        tileOwner
    }

    // Start is called before the first frame update
    void Start()
    {
        hexCoords = BoardHelperFns.HexList(3);
        updateGameStateFirstTime();
    }

    public void Init()
    {
        // Calling this in init wasn't working for me, idk why
        // So i called it in start
        //updateGameStateFirstTime();
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
    public void updateGameStateFirstTime()
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
            //tileHandler[coord] not working
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
            tileData = new TileSyncData(tileInfo.cropType, 0.0F, false, tileInfo.tileOwner);
        }
        return tileData;
    }

    // Goes from TileSyncData to TileTemp
    public static TileTemp DeserializeTile(TileSyncData tileData)
    {
        CropType cropNum = tileData.cropType;
        float timeLastPlanted = tileData.timeLastPlanted;
        bool containsFarmer = tileData.containsFarmer;
        int tileOwner = tileData.tileOwner;

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
            ((CropTile)tile).tileOwner = tileOwner;
        }
        // TODO sync timeLastPlanted and containsFarmer

        return tile;
    }

    // Serializes board into something that can be transported via Rpc
    public TileSyncData[] SerializeBoard()
    {
        int numTiles = hexCoords.Count;
        TileSyncData[] allTiles = new TileSyncData[numTiles];

        for (int i = 0; i < numTiles; i++)
        {
            Hex coord = hexCoords[i];
            // Sets TileSyncData
            allTiles[i] = SerializeTile(tileHandler[coord]);
        }
        return allTiles;
    }

    // Turns all of the tile strings into actual board tiles
    public void DeserializeBoard(TileSyncData[] allTiles)
    {
        int numTiles = hexCoords.Count;
        for (int i = 0; i < numTiles; i++)
        {
            Hex coord = hexCoords[i];
            TileTemp tile = DeserializeTile(allTiles[i]);
            tileHandler.TileDict[coord].Tile = tile;
        }
    }
}


public struct TileSyncData : INetworkSerializable
{
    public CropType cropType;
    public float timeLastPlanted;
    public bool containsFarmer;
    public int tileOwner;

    public TileSyncData(CropType cropTypeArg, float timeLastPlantedArg, bool containsFarmerArg, int tileOwnerArg) : this()
    {
        cropType = cropTypeArg;
        timeLastPlanted = timeLastPlantedArg;
        containsFarmer = containsFarmerArg;
        tileOwner = tileOwnerArg;
    }

    public void NetworkSerialize(NetworkSerializer serializer)
    {
        serializer.Serialize(ref cropType);
        serializer.Serialize(ref timeLastPlanted);
        serializer.Serialize(ref containsFarmer);
        serializer.Serialize(ref tileOwner);
    }

    // Equality
    public static bool operator ==(TileSyncData a, TileSyncData b) => a.Equals(b);

    public static bool operator !=(TileSyncData a, TileSyncData b) => !(a == b);

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        var b = (TileSyncData)obj;
        return (cropType == b.cropType) && (timeLastPlanted == b.timeLastPlanted) && (containsFarmer == b.containsFarmer) && (tileOwner == b.tileOwner);
    }

    public override int GetHashCode()
    {
        return (cropType, timeLastPlanted, containsFarmer, tileOwner).GetHashCode();
    }

    // String representation
    public override string ToString()
    {
        return (cropType, timeLastPlanted, containsFarmer, tileOwner).ToString();
    }
}
