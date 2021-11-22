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
        text.text = "Money: ₣" + Math.Round(Repository.Central.money, 2);
    }
}
