using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;




public class CostUpdateSoldier : MonoBehaviour
{
    public float cost;
    public TMP_Text TmPro;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    
    void FixedUpdate()
    {
        cost = Market.market.weightedSoldierCost;

        TmPro.text = "$" + Mathf.Floor(cost).ToString() ;
        if (Repository.Central.money >= cost)
            TmPro.color = Color.green;
        else
            TmPro.color = Color.red;
    }
}
