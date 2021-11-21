using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MLAPI;
using MLAPI.Transports.PhotonRealtime;
using TMPro;

public class LobbyMenu : MonoBehaviour
{
    PhotonRealtimeTransport transport;

    public TextMeshProUGUI maxBotsText;

    public void PlayGame(bool isHosting)
    {
        SceneManager.LoadScene("MainScene");
        SceneVariables.isHosting = isHosting;
        SceneVariables.cameThroughMenu = true;
    }

    public void EditLobbyId(string lobbyId)
    {
        SceneVariables.lobbyId = lobbyId;
    }

    public void EditMaxBots(float maxBotsF)
    {
        int maxBots = (int)maxBotsF;
        SceneVariables.maxBots = maxBots;
        maxBotsText.text = maxBots.ToString();
    }
}
