using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Transports.PhotonRealtime;
using TMPro;

public class IngamePlayerCount : NetworkBehaviour
{
    public GameStartCountdown countdown;

    PhotonRealtimeTransport transport;
    TextMeshProUGUI playerCountText;

    // Start is called before the first frame update
    void Start()
    {
        transport = NetworkManager.Singleton.GetComponent<PhotonRealtimeTransport>();
        playerCountText = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!(transport.Client.CurrentRoom is null))
        {
            // At least one other player, so can start
            if (transport.Client.CurrentRoom.PlayerCount > 1)
            {
                playerCountText.text = transport.Client.CurrentRoom.PlayerCount.ToString() + "/6 Players";

                if (!countdown.started)
                {
                    countdown.StartTimer();
                }
            }
            // Nobody else, so won't automatically start
            else
            {
                playerCountText.text = "Waiting for players...";
                if (countdown.started)
                {
                    countdown.StopTimer();
                }
            }
        }
    }
}
