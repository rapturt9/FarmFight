using System.Collections;
using System.IO;
using UnityEngine;
using MLAPI;
using MLAPI.Serialization;
using MLAPI.Serialization.Pooled;
using MLAPI.Transports.PhotonRealtime;

public class MultiplayerWorldManager : MonoBehaviour
{
    public bool startAsHost = true;
    private bool hostStarted = false;

    PhotonRealtimeTransport transport;

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

    void StartButtons()
    {
        if (GUILayout.Button("Host")) Host();
        if (GUILayout.Button("Client")) Join();
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

    public void Host()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.StartHost();
    }

    private void ApprovalCheck(byte[] connectionData, ulong clientID, NetworkManager.ConnectionApprovedDelegate callback)
    {
        Debug.Log("Trying to join");
        bool approve = System.Text.Encoding.ASCII.GetString(connectionData) == "FarmFight";
        callback(true, null, approve, Vector3.zero, Quaternion.identity);
    }

    public void Join()
    {
        transport = NetworkManager.Singleton.GetComponent<PhotonRealtimeTransport>();
        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes("FarmFight");
        NetworkManager.Singleton.StartClient();
    }
}