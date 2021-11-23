using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class FarmerCostUpdate : MonoBehaviour
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
        cost = Market.market.farmerCost;
        
        if (Repository.Central.cropHandler[Repository.Central.selectedHex].containsFarmer)
        {
            cost /= 2;
            TmPro.color = Color.blue;
        }
        else if (Repository.Central.money >= cost)
            TmPro.color = Color.green;
        else
            TmPro.color = Color.red;
        TmPro.text = "$" + Mathf.Round(cost).ToString();
    }
}
