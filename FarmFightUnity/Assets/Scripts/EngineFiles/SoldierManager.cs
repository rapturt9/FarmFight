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
}
