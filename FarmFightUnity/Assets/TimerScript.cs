using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimerScript : MonoBehaviour
{

    public bool ended = false;
    public int time;
    public TMP_Text text;

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
        time = (minutes * 60) + seconds;
        StartCoroutine(timerControl());
    }

    private IEnumerator timerControl()
    {
        while(time > 0 )
        {
            int minutes = time / 60;
            int seconds = time % 60;


            if(seconds < 10)
                text.text = $"{minutes} : 0{seconds}" ;
            else
                text.text = $"{minutes} : {seconds}";

            time--;

            yield return new WaitForSeconds(1);
        }
    }
}
