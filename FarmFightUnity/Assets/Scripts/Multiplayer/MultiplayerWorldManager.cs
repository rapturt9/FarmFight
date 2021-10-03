using System.Collections;
using System.IO;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Serialization;
using MLAPI.Serialization.Pooled;

public class MultiplayerWorldManager : MonoBehaviour
{
    public bool startAsHost = true;
    private bool hostStarted = false;

    private void Start()
    {
        // Makes Hex serializable on the network
        SerializationManager.RegisterSerializationHandlers<Hex>((Stream stream, Hex coord) =>
        {
            using (var writer = PooledNetworkWriter.Get(stream))
            {
                writer.WriteIntArrayPacked(new int[] { coord.x, coord.y });
            }
        }, (Stream stream) =>
        {
            using (var reader = PooledNetworkReader.Get(stream))
            {
                int[] newCoord = reader.ReadIntArrayPacked();
                return new Hex(newCoord[0], newCoord[1]);
            }
        });
    }
    void OnGUI()
    {
        if (!startAsHost)
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                StartButtons();
            }
            else
            {
                StatusLabels();
            }

            GUILayout.EndArea();
        }
        else if (!hostStarted)
        {
            NetworkManager.Singleton.StartHost();
            hostStarted = true;
        }
    }

    static void StartButtons()
    {
        if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
        if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();
        if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
    }

    static void StatusLabels()
    {
        var mode = NetworkManager.Singleton.IsHost ?
            "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

        GUILayout.Label("Transport: " +
            NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);

        if (NetworkManager.Singleton.IsHost)
        {
            if (GUILayout.Button("Start Game"))
            {
                print("This doesn't do anything yet!");
            }
        }
    }
}