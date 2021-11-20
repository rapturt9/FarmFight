using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;

public class SetMoneyDisplay : MonoBehaviour
{
    private void FixedUpdate()
    {
        text.text = $"Money: ₴ {getMoney()}\n" +
                    $"Dominations: {getDomination()}%";
    }

    private string getMoney()
    {
        var money = Repository.Central.money;
        return Math.Round(money, 2).ToString();
    }

    private string getDomination()
    {

        return "0";
    }

    public TMP_Text text;
}
