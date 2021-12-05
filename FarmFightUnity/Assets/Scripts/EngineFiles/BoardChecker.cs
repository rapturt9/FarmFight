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
        // Initializing
        do
        {
            UpdateTileCounts();
            yield return new WaitForEndOfFrame();
        }
        while (ownedTileCount[Repository.Central.localPlayerId] == 0);

        // Actual gameplay
        while (Repository.Central.gameIsRunning)
        {
            UpdateTileCounts();

            // Losing
            if (CheckForLost(Repository.Central.localPlayerId))
            {
                MarkAsDeadServerRpc(Repository.Central.localPlayerId);
                EndGame(false);
            }
            // Winning
            if (IsServer && 

                ((!SceneVariables.cameThroughMenu && GameManager.GM.currMaxLocalPlayerId > 1) || // Started from MainScene

                (SceneVariables.cameThroughMenu && // Started from menu
                (GameManager.GM.currMaxLocalPlayerId + GameManager.GM.botsToAdd) >= GameManager.GM.totalPlayersAndBots) // Actual players equal to expected players
                ))
            {
                int winningPlayer = CheckForAnyWin();
                if (winningPlayer != -1)
                {
                    Repository.Central.gameIsRunning = false;
                    EndGameWinClientRpc(winningPlayer);
                    gameEndDisplay.EnableHostPlayAgain();
                    yield return null;
                }
            }

            yield return new WaitForEndOfFrame();
        }
    }

    private bool CheckForLost(int playerId)
    {
        if (ownedTileCount[playerId] == 0 &&
            soldierCount[playerId] == 0)
        {
            return true;
        }
        return false;
    }

    bool CheckForWin(int playerId)
    {
        for (int id = 0; id<Repository.maxPlayers; id++)
        {
            if (id != playerId && !CheckForLost(id))
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

    public void EndGame(bool won)
    {
        if (!IsServer)
        {
            Repository.Central.gameIsRunning = false;
        }
        gameEndDisplay.EndingDisplay(won);
    }

    [ClientRpc]
    public void EndGameWinClientRpc(int winningPlayer)
    {
        bool won = winningPlayer == Repository.Central.localPlayerId;
        if (won)
        {
            EndGame(won);
        }
    }

    void UpdateTileCounts()
    {
        ownedTileCount = new int[Repository.maxPlayers];
        soldierCount = new int[Repository.maxPlayers];
        foreach (var coord in hexCoords)
        {
            
            TileTemp tile = cropTiles[coord];
            if (tile.tileOwner != -1)
            {
                ownedTileCount[tile.tileOwner]++;
            }
            foreach (var player in tile.SortedSoldiers)
                soldierCount[player.Key] += player.Value.Count;
        }
    }

    [ServerRpc]
    void MarkAsDeadServerRpc(int playerId)
    {
        GameManager.GM.realPlayers.Remove(playerId);
    }
}
