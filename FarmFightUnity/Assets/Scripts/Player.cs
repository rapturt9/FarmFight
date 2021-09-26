using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine;

public class Player : NetworkBehaviour
{

    public override void NetworkStart()
    {

    }

    // Only edit this if you want something to simultaneously update for clients and server
    // Ex. Changing appearance
    void Update()
    {
        if (IsServer)
        {
            UpdateServer();
        }

        if (IsClient)
        {
            UpdateClient();
        }
    }

    // Changing what the player doesn't have control over
    void UpdateServer()
    {
            
    }

    // Changing what the player does have control over
    void UpdateClient()
    {
        if (!IsLocalPlayer) { return;}
    }


    // ServerRpcs
    // Use these to send data to the server

    [ServerRpc]
    void TestServerRpc()
    {
        // Do nothing
    }
}