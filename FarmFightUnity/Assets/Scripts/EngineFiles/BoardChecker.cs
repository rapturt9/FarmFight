using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;

public class BoardChecker : NetworkBehaviour
{
    public TileHandler cropTiles;
    public GameManager gameManager;
    public GameEndDisplay gameEndDisplay;

    public static BoardChecker Checker;

    public int[] ownedTileCount;

    public List<Hex> hexCoords;
    int totalTiles;

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
        totalTiles = hexCoords.Count;
        ownedTileCount = new int[Repository.maxPlayers];
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
            for (int playerId = 0; playerId < gameManager.currMaxLocalPlayerId; playerId++)
            {
                if (gameManager.currMaxLocalPlayerId > 1 && // Is there at least one player to win against
                    CheckForWin(playerId)) // Have we actually won
                {
                    Repository.Central.gameIsRunning = false;
                    EndGameClientRpc(playerId);
                    yield return null;
                }
            }

            yield return new WaitForSeconds(1f);
        }
    }

    public bool CheckForWin(int playerId)
    {
        // Total domination of all tiles
        if (ownedTileCount[playerId] == totalTiles)
        {
            return true;
        }
        // Nobody else has anything
        else
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
    }

    // Keeps track of tiles owned by everybody
    [ServerRpc(RequireOwnership = false)]
    public void ChangeTileOwnershipCountServerRpc(int playerId, int count, bool checkForWin = true)
    {
        return;
        ownedTileCount[playerId] += count;

        if (checkForWin && // Do we want to check
            gameManager.currMaxLocalPlayerId > 1 && // Is there at least one player to win against
            CheckForWin(playerId)) // Have we actually one
        {
            Repository.Central.gameIsRunning = false;
            EndGameClientRpc(playerId);
        }
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

        Debug.Log("Player " + winningPlayer.ToString() + " has won");
    }

    void UpdateTileCounts()
    {
        ownedTileCount = new int[Repository.maxPlayers];
        foreach (var coord in hexCoords)
        {
            TileTemp tile = cropTiles[coord];
            if (tile.tileOwner != -1)
            {
                ownedTileCount[tile.tileOwner]++;
            }
        }
    }
}
