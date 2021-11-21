using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable;

public class GameStartCountdown : NetworkBehaviour
{
    public GameManager gameManager;

    TextMeshProUGUI text;

    NetworkVariable<float> startTime;
    float timeLeft;
    [SerializeField] float startTimeLeft = 31;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    void OnEnable()
    {
        if (IsServer)
        {
            startTime.Value = NetworkManager.Singleton.NetworkTime;
        }
    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            timeLeft = startTimeLeft - (NetworkManager.Singleton.NetworkTime - startTime.Value);
            text.text = ((int)timeLeft).ToString();
        }
        catch
        {
            text.text = "Connecting...";
        }

        // Start the game if the timer runs out
        if (timeLeft <= 0 && IsServer)
        {
            gameManager.GameStartClientRpc();
        }
    }
}
