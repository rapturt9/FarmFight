using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;

public class MoneyUpdate : MonoBehaviour
{

    public TMP_Text text;
    // Update is called once per frame
    void FixedUpdate()
    {
        if(Repository.Central.money<10)
            text.text = "Money: ₣" + Math.Round(Repository.Central.money, 2);
        else if (Repository.Central.money < 1000)
        {
            text.text = "Money: ₣" + Mathf.RoundToInt((float)Repository.Central.money);

         
        }
        else if(Repository.Central.money < 10000 )
        {
            text.text = "Money: ₣" + Math.Round(Repository.Central.money / 1000,1) + "k";
            
        }
        else
        {
            text.text = "Money: ₣10k+";
        }
    }
}
