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
    [HideInInspector] public bool started = false;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        
    }

    public override void NetworkStart()
    {
        
    }

    public void StartTimer()
    {
        if (IsServer)
        {
            startTime.Value = NetworkManager.Singleton.NetworkTime;
        }
        started = true;
    }

    public void StopTimer()
    {
        if (IsServer)
        {
            startTime.Value = 0;
        }
        started = false;
        text.text = (startTimeLeft - 1).ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (started)
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
                GameManager.GM.GameStart();
                Destroy(gameObject);
            }
        }
    }
}
