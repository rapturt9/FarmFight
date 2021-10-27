using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;

public class BoardChecker : NetworkBehaviour
{
    public TileHandler cropTiles;

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

    public IEnumerator CheckBoard()
    {
        while (true)
        {
            
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
    public void changeTileOwnershipCountServerRpc(int playerId, int count, bool checkForWin = true)
    {
        ownedTileCount[playerId] += count;

        if (checkForWin)
        {
            if (CheckForWin(playerId))
            {
                Debug.Log("Player " + playerId.ToString() + " has won");
            }
        }
    }
}
