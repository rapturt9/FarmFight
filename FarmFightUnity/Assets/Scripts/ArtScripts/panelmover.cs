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
        if (started == true)
        {
            return;
        }
        GetComponent<Image>().color = Repository.Central.TeamColors[player];

        started = true;

        startPos = GetComponent<RectTransform>().anchoredPosition;

        StartCoroutine(moveToPoint());
    }

    public TMP_Text Count, health, percent;

    public float maxspeed;
    public Vector3 goalposition;
    public (float, float, float) values;

    private IEnumerator moveToPoint()
    {

        while (true)
        {

            Count.text = values.Item1.ToString();
            health.text = Mathf.Round(values.Item2).ToString();
            float owned = BoardChecker.Checker.ownedTileCount[Player]
                        * 100 / BoardChecker.Checker.totalOwned;
            percent.text = Mathf.Round(owned).ToString() + "%";

            Vector2 position = GetComponent<RectTransform>().anchoredPosition;
            
                
            GetComponent<RectTransform>().anchoredPosition =
                Vector2.MoveTowards(position, goalposition, maxspeed);

            yield return null;
        }
    }
}
