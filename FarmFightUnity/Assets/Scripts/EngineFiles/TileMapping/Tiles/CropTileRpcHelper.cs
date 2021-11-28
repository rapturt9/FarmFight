using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;

public class CropTileRpcHelper : NetworkBehaviour
{
    public TileHandler crops;

    public static CropTileRpcHelper CTRPC;

    private void Awake()
    {
        Application.targetFrameRate = 30;

        if (CTRPC == null)
        {
            CTRPC = this;
        }
        else if (CTRPC != this)
        {
            Destroy(gameObject);
        }
    }

    [ClientRpc]
    public void StartCapturingClientRpc(int[] hexArray, int newowner)
    {
        Hex hex = BoardHelperFns.ArrayToHex(hexArray);
        crops[hex].StartCapturingClientRpc(newowner);
    }

    [ClientRpc]
    public void StopCapturingClientRpc(int[] hexArray)
    {
        Hex hex = BoardHelperFns.ArrayToHex(hexArray);
        crops[hex].StopCapturingClientRpc();
    }
}
