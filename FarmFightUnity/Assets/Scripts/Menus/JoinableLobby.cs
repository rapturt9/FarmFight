using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class JoinableLobby : MonoBehaviour
{
    private TextMeshProUGUI buttonText;
    public string lobbyId;

    void Start()
    {
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
        buttonText.text = $"Join \"{lobbyId}\"";
    }

    public void JoinLobby()
    {
        SceneVariables.lobbyId = lobbyId;
        LobbyMenu.LBMenu.PlayGame(false);
    }

}
