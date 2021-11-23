using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;

public class CostUpdate : MonoBehaviour
{
    public float cost;
    public TMP_Text TmPro;
    public CropType crop;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    
    void FixedUpdate()
    {
        cost = Market.market.CropValues[crop];
        TmPro.text = "$" + Math.Round(cost, 2).ToString() ;
        if (Repository.Central.money >= cost)
            TmPro.color = Color.green;
        else
            TmPro.color = Color.red;
    }
}
