using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;

public class SoldierManager : NetworkBehaviour
{
    public TileHandler crops;

    public static SoldierManager SM;

    public void Awake()
    {
        if (SM == null)
        {
            SM = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start/Stop battle
    [ClientRpc]
    public void StartBattleClientRpc(int[] hexArray)
    {
        Hex hex = BoardHelperFns.ArrayToHex(hexArray);
        crops[hex].StartBattle();
    }

    [ClientRpc]
    public void StopBattleClientRpc(int[] hexArray)
    {
        Hex hex = BoardHelperFns.ArrayToHex(hexArray);
        crops[hex].StopBattle();
    }


    // Add soldier
    public bool addSoldier(Hex hex, int owner = -1)
    {
        if (owner == -1)
            owner = Repository.Central.localPlayerId;

        int[] hexArray = BoardHelperFns.HexToArray(hex);
        if (crops[hex].tileOwner == owner)
        {
            addSoldierServerRpc(hexArray, owner);
            return true;
        }
        return false;
    }

    [ServerRpc(RequireOwnership = false)]
    void addSoldierServerRpc(int[] hexArray, int owner)
    {
        Hex hex = BoardHelperFns.ArrayToHex(hexArray);
        crops[hex].addSoldier(owner);
    }


    // Send soldier
    public void SendSoldier(Hex start, Hex end, int number = 1, int owner = -1)
    {
        if (owner == -1)
            owner = Repository.Central.localPlayerId;

        sendSoldierServerRpc(BoardHelperFns.HexToArray(start),
                BoardHelperFns.HexToArray(end),
                owner);
    }

    [ServerRpc(RequireOwnership = false)]
    public void sendSoldierServerRpc(int[] startArray, int[] endArray, int owner)
    {
        Hex start = BoardHelperFns.ArrayToHex(startArray);
        Hex end = BoardHelperFns.ArrayToHex(endArray);

        TileTemp startTile = crops[start];
        startTile.sendSoldier(end, owner);
    }
}
