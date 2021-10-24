using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardChecker : MonoBehaviour
{
    public TileHandler cropTiles;

    public static BoardChecker Checker;

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
    }

    public IEnumerator CheckBoard()
    {
        while (true)
        {
            
        }
    }

    public void CheckForWin(int playerId)
    {
        foreach (Hex hex in hexCoords)
        {
            if (cropTiles[hex].tileOwner != playerId)
            {
                return;
            }
        }
        Debug.Log("Player " + playerId.ToString() + " has won");
    }
}
