using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Messaging;
using System.Linq;

public class TimerScript : NetworkBehaviour
{

    public bool ended = false;
    public int time;
    public TMP_Text text;

    NetworkVariable<float> startTime;
    int maxTime;

    public void Start()
    {
        var septime = Repository.Central.time;
        int minutes = septime.x;
        int seconds = septime.y;


        if (seconds < 10)
            text.text = $"{minutes} : 0{seconds}";
        else
            text.text = $"{minutes} : {seconds}";

        
    }
    public void init(int minutes, int seconds)
    {
        if (IsServer)
        {
            startTime.Value = NetworkManager.Singleton.NetworkTime;
        }
        time = (minutes * 60) + seconds;
        maxTime = time;
        StartCoroutine(timerControl());
    }

    private IEnumerator timerControl()
    {
        while(time > 0)
        {
            if (startTime.Value == 0)
            {
                yield return null;
            }
            int minutes = time / 60;
            int seconds = time % 60;


            if(seconds < 10)
                text.text = $"{minutes} : 0{seconds}" ;
            else
                text.text = $"{minutes} : {seconds}";

            time = maxTime - (int)(NetworkManager.Singleton.NetworkTime - startTime.Value);

            yield return new WaitForEndOfFrame();
        }

        EndGameTimerDeath();
    }

    void EndGameTimerDeath()
    {
        BoardChecker c = BoardChecker.Checker;
        // If we have the most tiles/tie we win
        if (c.ownedTileCount[Repository.Central.localPlayerId] >= c.ownedTileCount.Max())
        {
            c.EndGame(true);
        }
        // Otherwise we lose
        else
        {
            c.EndGame(false);
        }

        if (IsHost)
        {
            GameEndDisplay.EndDisp.EnableHostPlayAgain();
        }
    }
}
