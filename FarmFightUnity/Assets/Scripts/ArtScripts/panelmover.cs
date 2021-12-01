using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class panelmover : MonoBehaviour
{
    // Start is called before the first frame update
    private bool started;
    private Vector2 startPos;
    private int Player;
    public void Init(int player)
    {
        GetComponent<Image>().color = Repository.Central.TeamColors[player];
        

        if (started == true)
        {
            return;
        }

        startPos = GetComponent<RectTransform>().anchoredPosition;
        started = true;
        
        

        StartCoroutine(moveToPoint());
        Player = player;
    }

    public TMP_Text Count, health, percent;

    public float maxspeed;
    public Vector3 goalposition;
    public (float, float, float) values;

    public void setcolor(int player)
    {
        GetComponent<Image>().color = Repository.Central.TeamColors[player];
        Player = player;
    }

    private IEnumerator moveToPoint()
    {

        while (true)
        {

            Count.text = values.Item1.ToString();
            health.text = Mathf.Round(values.Item2).ToString();
            // If we've started from MainScene, totalOwned will be 0. This fixes this on the client.
            int ownedByAnybody = BoardChecker.Checker.totalOwned > 0 ? BoardChecker.Checker.totalOwned : 1;
            float owned = BoardChecker.Checker.ownedTileCount[Player]
                        * 100 / ownedByAnybody;
            percent.text = Mathf.Round(owned).ToString() + "%";

            Vector2 position = GetComponent<RectTransform>().anchoredPosition;
            
                
            GetComponent<RectTransform>().anchoredPosition =
                Vector2.MoveTowards(position, goalposition, maxspeed);

            yield return null;
        }
    }
}
