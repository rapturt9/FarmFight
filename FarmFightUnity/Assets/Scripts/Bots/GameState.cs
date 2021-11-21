using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable.Collections;
using MLAPI.Messaging;
using MLAPI.Serialization;

public enum CropTileSyncTypes
{
    blankTile = -1,
    cropNum,
    lastPlanted,
    containsFarmer,
    tileOwner,
    battleOccurring,
    tileDamage
}

public class GameState : MonoBehaviour
{
    public GameManager gameManager;
    public TileHandler tileHandler;
    public TileManager tileManager;
    public CropManager cropManager;
    public SoldierManager soldierManager;

    // Hex coord, (crop#, time planted/time last clicked, farmer or not)
    public Dictionary<Hex, TileSyncData> cropTiles = new Dictionary<Hex, TileSyncData>();
    private Dictionary<int, makeMove> bots = new Dictionary<int, makeMove>();
    private List<int> botIds = new List<int>();

    public List<Hex> hexCoords;

    private static TileSyncData emptyTileSyncData = new TileSyncData(CropType.blankTile, 0.0f, false, -1, false, 0.0f);

    // Start is called before the first frame update
    void Start()
    {
        hexCoords = BoardHelperFns.HexList(3);
    }

    public void Init(int numBots, int currMaxLocalPlayerId)
    {
        updateGameStateFirstTime();
        StartCoroutine(collectGameData());
        
        for (int playerId = currMaxLocalPlayerId; playerId < numBots + currMaxLocalPlayerId; playerId++)
        {
            gameManager.addNewPlayer(playerId);
            var botObj = new GameObject("Bot"+playerId.ToString());
            botObj.transform.parent = transform;
            makeMove bot = botObj.AddComponent<makeMove>();
            bot.Init(playerId, this, cropManager, tileManager, tileHandler, soldierManager);
            bots.Add(playerId, bot);
        }
        botIds = new List<int>(bots.Keys);
    }

    public void TryAddBotMoney(int playerId, double moneyToAdd)
    {
        if (botIds.Contains(playerId))
            bots[playerId].money += moneyToAdd;
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
            /*if (coord == new Hex(0,0)){
                cropTiles[coord] = new TileSyncData(CropType.carrot, 0.0f, false, 0);
                tileHandler[coord] = DeserializeTile(cropTiles[coord]);
                tileHandler[coord].tileOwner = 0;
                tileHandler.SyncTile(coord);
            }*/
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
        return new TileSyncData(tile.cropType, tile.timeLastPlanted, tile.containsFarmer, tile.tileOwner, tile.battleOccurring, tile.tileDamage);
    }

    // Goes from TileSyncData to TileTemp
    public static TileTemp DeserializeTile(TileSyncData tileData)
    {
        CropType cropType = tileData.cropType;
        float timeLastPlanted = tileData.timeLastPlanted;
        bool containsFarmer = tileData.containsFarmer;
        int tileOwner = tileData.tileOwner;
        bool battleOccurring = tileData.battleOccurring;
        float tileDamage = tileData.tileDamage;

        TileTemp tile;
        if (cropType == CropType.potato)
            tile = new Potato();
        else if (cropType == CropType.carrot)
            tile = new Carrot();
        else if (cropType == CropType.rice)
            tile = new Rice();
        else if (cropType == CropType.eggplant)
            tile = new Eggplant();
        else
            tile = new BlankTile();

        tile.timeLastPlanted = timeLastPlanted;
        //tile.containsFarmer = containsFarmer; // Can't sync now
        tile.tileOwner = tileOwner;
        tile.battleOccurring = battleOccurring;
        tile.tileDamage = tileDamage;
        
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
        // Deserialize tiles
        int numTiles = hexCoords.Count;
        for (int i = 0; i < numTiles; i++)
        {
            Hex coord = hexCoords[i];
            TileTemp tile = DeserializeTile(allTiles[i]);
            tileHandler.TileDict[coord].Tile = tile;
            
        }
        // Add soldiers
        // TODO
    }
}


public struct TileSyncData : INetworkSerializable
{
    public CropType cropType;
    public float timeLastPlanted;
    public bool containsFarmer;
    public int tileOwner;
    public bool battleOccurring;

    public float tileDamage;

    public TileSyncData(CropType cropTypeArg, float timeLastPlantedArg, bool containsFarmerArg, int tileOwnerArg, bool battleOccurringArg, float tileDamageArg) : this()
    {
        cropType = cropTypeArg;
        timeLastPlanted = timeLastPlantedArg;
        containsFarmer = containsFarmerArg;
        tileOwner = tileOwnerArg;
        battleOccurring = battleOccurringArg;
        tileDamage = tileDamageArg;
        //soldiers = soldiersArg;
    }

    public void NetworkSerialize(NetworkSerializer serializer)
    {
        serializer.Serialize(ref cropType);
        serializer.Serialize(ref timeLastPlanted);
        serializer.Serialize(ref containsFarmer);
        serializer.Serialize(ref tileOwner);
        serializer.Serialize(ref battleOccurring);
        serializer.Serialize(ref tileDamage);
    }

    // Equality
    public static bool operator ==(TileSyncData a, TileSyncData b) => a.Equals(b);

    public static bool operator !=(TileSyncData a, TileSyncData b) => !(a == b);

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        var b = (TileSyncData)obj;
        return (cropType == b.cropType) && 
            (timeLastPlanted == b.timeLastPlanted) && 
            (containsFarmer == b.containsFarmer) && 
            (tileOwner == b.tileOwner) &&
            (tileDamage == b.tileDamage) &&
            (battleOccurring == b.battleOccurring);
    }

    public override int GetHashCode()
    {
        return (cropType, timeLastPlanted, containsFarmer, tileOwner, battleOccurring, tileDamage).GetHashCode();
    }

    // String representation
    public override string ToString()
    {
        return (cropType, timeLastPlanted, containsFarmer, tileOwner, battleOccurring, tileDamage).ToString();
    }
}