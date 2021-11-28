using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;

public class BoardChecker : NetworkBehaviour
{
    public TileHandler cropTiles;
    public GameEndDisplay gameEndDisplay;

    public static BoardChecker Checker;

    public int[] ownedTileCount;
    public int[] soldierCount;

    public int totalOwned { get
        {
            var total = 0;
            for (int i = 0; i < ownedTileCount.Length; i++)
            {
                total += ownedTileCount[i];
            }
            return total;
        }
    }
    public List<Hex> hexCoords;

    void Awake()
    {
        if (Checker == null)
        {
            Checker = this;
        }
        else if (Checker != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// internal stuff
    /// </summary>
    // Start is called before the first frame update
    void Start()
    {
        hexCoords = BoardHelperFns.HexList(TileManager.TM.size);
        ownedTileCount = new int[Repository.maxPlayers];
        soldierCount = new int[Repository.maxPlayers];
    }

    public void StartChecking()
    {
        StartCoroutine("CheckBoard");
    }

    public IEnumerator CheckBoard()
    {
        while (Repository.Central.gameIsRunning)
        {
            UpdateTileCounts();
            if (GameManager.GM.totalPlayersAndBots > 1 && IsServer) // Is there at least one player to win against
            {
                int winningPlayer = CheckForAnyWin();
                if (winningPlayer != -1)
                {
                    Repository.Central.gameIsRunning = false;
                    EndGameClientRpc(winningPlayer);
                    yield return null;
                }
            }
            yield return new WaitForEndOfFrame(); ;
        }
    }

    bool CheckForWin(int playerId)
    {
        for (int id = 0; id<Repository.maxPlayers; id++)
        {
            if (id != playerId && ownedTileCount[id] != 0)
            {
                return false;
            }
        }
        return true;
    }

    public int CheckForAnyWin()
    {
        for (int playerId = 0; playerId < GameManager.GM.totalPlayersAndBots; playerId++)
        {
            if (CheckForWin(playerId)) // Have we actually won
            {
                return playerId;
            }
        }
        return -1;
    }

    [ClientRpc]
    public void EndGameClientRpc(int winningPlayer)
    {
        Repository.Central.gameIsRunning = false;
        bool won = Repository.Central.localPlayerId == winningPlayer;
        gameEndDisplay.EndDisplay(won);

        if (won)
        {
            Debug.Log("You won!");
        }
        else
        {
            Debug.Log("You lost :(");
        }

        if (winningPlayer != -1)
            Debug.Log("Player " + winningPlayer.ToString() + " has won");
    }

    void UpdateTileCounts()
    {
        ownedTileCount = new int[Repository.maxPlayers];
        var soldiers = new int[Repository.maxPlayers];
        foreach (var coord in hexCoords)
        {
            
            TileTemp tile = cropTiles[coord];
            if (tile.tileOwner != -1)
            {
                ownedTileCount[tile.tileOwner]++;

                foreach (var player in tile.SortedSoldiers)
                    soldierCount[player.Key] += player.Value.Count;
            }
        }

        soldierCount = soldiers;
    }

    
}
