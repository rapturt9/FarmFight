using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class JoinableLobby : MonoBehaviour
{
    public TextMeshProUGUI buttonText;
    public TextMeshProUGUI playerCountText;

    string lobbyId;
    int playerCount;

    public void Init(string lobbyId, int playerCount)
    {
        this.lobbyId = lobbyId;
        this.playerCount = playerCount;

        buttonText.text = $"Join \"{lobbyId}\"";
        playerCountText.text = playerCount.ToString() + "/6";
    }

    public void JoinLobby()
    {
        SceneVariables.lobbyId = lobbyId;
        LobbyMenu.LBMenu.PlayGame(false);
    }

}
