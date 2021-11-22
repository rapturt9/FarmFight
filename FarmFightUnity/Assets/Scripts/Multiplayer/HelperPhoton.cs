using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using System.Threading;
using System.Linq;

public partial class HelperPhoton : MonoBehaviour
{
    public List<RoomInfo> availableRooms = new List<RoomInfo>();
    public IEnumerable<string> availableRoomNames => availableRooms.Select(i => i.Name);

    void Start()
    {
        StartClient();
    }

    void OnDisable()
    {
        StopClient();
    }

    private void Update()
    {
        client.Service();
        Thread.Sleep(33);
    }
}

partial class HelperPhoton : IConnectionCallbacks
{
    private readonly LoadBalancingClient client = new LoadBalancingClient();
    private bool quit;

    public void StartClient()
    {
        client.AddCallbackTarget(this);
        client.StateChanged += OnStateChange;

        client.ConnectUsingSettings(PhotonAppSettings.Instance.AppSettings);
    }

    public void StopClient()
    {
        if (client.IsConnected)
        {
            client.Disconnect();
        }
        client.RemoveCallbackTarget(this);
    }

    private void OnStateChange(ClientState arg1, ClientState arg2)
    {
        //Debug.Log(arg1 + " -> " + arg2);
    }

    public void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster Server: " + client.LoadBalancingPeer.ServerIpAddress);
        client.OpJoinLobby(null);
    }

    public void OnConnected()
    {
    }

    public void OnDisconnected(DisconnectCause cause)
    {
    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
    }

    public void OnCustomAuthenticationFailed(string debugMessage)
    {
    }
}

partial class HelperPhoton : ILobbyCallbacks
{
    public void OnJoinedLobby()
    {
    }

    public void OnLeftLobby()
    {
    }

    // When we get a new room list, change the internal cache
    public void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("Updated Room List");
        Debug.Log("Room Count: " + roomList.Count);

        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList)
            {
                availableRooms.Remove(room);
            }
            else
            {
                availableRooms.Add(room);
            }
        }

        // We refresh the buttons
        if (!(LobbyMenu.LBMenu is null))
            LobbyMenu.LBMenu.RefreshJoinableLobbies();
    }

    public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {
    }
}