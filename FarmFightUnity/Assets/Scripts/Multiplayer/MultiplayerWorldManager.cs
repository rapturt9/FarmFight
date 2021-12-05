using System.Collections;
using System.IO;
using UnityEngine;
using MLAPI;
using MLAPI.Serialization;
using MLAPI.Serialization.Pooled;
using MLAPI.Transports.PhotonRealtime;
using Photon.Realtime;
using ParrelSync;

public class MultiplayerWorldManager : MonoBehaviour
{
    public PhotonRealtimeTransport transport;

    public GameObject interstitial;
    public GameObject startGameButton;

    private void Start()
    {
        transport = NetworkManager.Singleton.GetComponent<PhotonRealtimeTransport>();

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

        // Starting from the main menu means host/client is dictated by user deciding to host
        if (SceneVariables.cameThroughMenu)
        {
            transport.RoomName = SceneVariables.lobbyId;
            transport.IsVisible = !SceneVariables.isPrivate;

            // Are we hosting
            if (SceneVariables.isHosting)
            {
                Host();
            }
            else
            {
                Join();
            }
        }
        // Starting directly from MainScene means host/client is dictated by clone or not
        else
        {
            if (ClonesManager.IsClone())
            {
                Join();
            }
            else
            {
                Host();
            }
        }
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer && !SceneVariables.cameThroughMenu)
        {
            //StartButtons();
        }
        else
        {
            StatusLabels();
        }

        GUILayout.EndArea();
    }

    void StartButtons()
    {
        if (GUILayout.Button("Host")) Host();
        if (GUILayout.Button("Client")) Join();
        if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
    }

    void StatusLabels()
    {
        var mode = NetworkManager.Singleton.IsHost ?
            "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";
        
        //GUILayout.Label("Transport: " +
        //    NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
        //GUILayout.Label("Mode: " + mode);
    }

    public void Host()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.StartHost();

        // Do different stuff if we started from menu or directly from scene
        if (SceneVariables.cameThroughMenu)
            startGameButton.SetActive(true);
        else
            GameManager.GM.StartFromMainSceneServerRpc(NetworkManager.Singleton.LocalClientId);

        print("Joining as Host");
    }

    private void ApprovalCheck(byte[] connectionData, ulong clientID, NetworkManager.ConnectionApprovedDelegate callback)
    {
        Debug.Log("Trying to join");
        bool approve = System.Text.Encoding.ASCII.GetString(connectionData) == "FarmFight";
        callback(true, null, approve, Vector3.zero, Quaternion.identity);
    }

    public void Join()
    {
        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes("FarmFight");
        NetworkManager.Singleton.StartClient();

        if (!SceneVariables.cameThroughMenu)
            StartCoroutine(GameManager.GM.StartFromMainScene());

        print("Joining as Client");
    }
}