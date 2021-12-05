using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class LobbyIDInterstitial : MonoBehaviour
{
    TextMeshProUGUI lobbyIdText;

    // Start is called before the first frame update
    void Start()
    {
        lobbyIdText = GetComponent<TextMeshProUGUI>();
        lobbyIdText.text = SceneVariables.lobbyId;
    }
}
