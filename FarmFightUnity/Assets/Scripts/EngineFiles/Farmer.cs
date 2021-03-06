using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;

public class Farmer : NetworkBehaviour
{
    public TileHandler handler;

    public Hex hex;

    public void Awake()
    {
        Owner = new NetworkVariable<int>(-1);
    }

    public NetworkVariable<int> Owner = new NetworkVariable<int>(-1);

    private void Start()
    {
        handler = TileManager.TM["Crops"];
    }

    // Add to all client tiles
    public void AddToTile(Hex coord)
    {
        if (IsClient)
        {
            AddToTileServerRpc(BoardHelperFns.HexToArray(coord));
        }
        else if (IsServer)
        {
            AddToTileClientRpc(BoardHelperFns.HexToArray(coord));
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void AddToTileServerRpc(int[] coord)
    {
        AddToTileClientRpc(coord);
    }

    [ClientRpc]
    void AddToTileClientRpc(int[] coord)
    {
        _AddToTile(coord);
    }

    // Internal function, actually changes the tile
    void _AddToTile(int[] coordArray)
    {
        Hex coord = BoardHelperFns.ArrayToHex(coordArray);
        handler[coord].farmerObj = this;
    }

    private void Update()
    {
        
        
    }

    public void kill()
    {
        Destroy(gameObject);
    }
}
