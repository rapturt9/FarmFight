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

        float displaymoney = (float)Repository.Central.money - Repository.Central.vegetableSum;

        displaymoney = (float)Math.Round( Mathf.Max(displaymoney, 0), 2);

        //Debug.Log(displaymoney);

        if (Mathf.Round(displaymoney) < 10)
        {
            if (displaymoney % 1f == 0)
            {
                text.text = "Money: ₣" + displaymoney + ".00";
            }
            else if (displaymoney % .1f == 0)
                text.text = "Money: ₣" + displaymoney + "0";
            else
                text.text = "Money: ₣" + displaymoney;
        }
            
        else if (Mathf.Round(displaymoney) < 100)
        {
            if(displaymoney% 1f == 0)
                text.text = "Money: ₣" + displaymoney + ".0";
            else
                text.text = "Money: ₣" + Math.Round(displaymoney,1);

         
        }

        else if(Mathf.Round(displaymoney) < 1000)
        {
            text.text = "Money: ₣" + Mathf.Round(displaymoney);
        }
        else if(Mathf.Round(displaymoney) < 10000 )
        {
            float display = (float)Math.Round(displaymoney / 1000f, 1);
            if (display % 1 == 0)
            {
                text.text = "Money: ₣" + display + "0k";
            }
            else
                text.text = "Money: ₣" + Math.Round(displaymoney / 1000f,1) + "k";
            
            
        }
        else
        {
            text.text = "Money: ₣10k+";
        }
    }
}
