using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable;

public class GameStartCountdown : NetworkBehaviour
{
    TextMeshProUGUI text;

    NetworkVariable<float> startTime;
    float timeLeft = 1;
    [SerializeField] float startTimeLeft = 31;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    public override void NetworkStart()
    {
        if (IsServer)
        {
            startTime.Value = NetworkManager.Singleton.NetworkTime;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // When the client is initially connected it isn't synced with startTime
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
            print("Time ran out");
            GameManager.GM.GameStartClientRpc();
            Destroy(gameObject);
        }
    }
}
