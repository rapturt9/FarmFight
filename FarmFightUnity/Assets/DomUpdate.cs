using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class DomUpdate : MonoBehaviour
{
    public TMP_Text text;

    // Update is called once per frame
    void Update()
    {
        float owned;
        if (BoardChecker.Checker.totalOwned == 0)
            owned = 0;
        else
            owned = BoardChecker.Checker.ownedTileCount[Repository.Central.localPlayerId]
                        * 100 / BoardChecker.Checker.totalOwned;

        text.text = "Domination: " + Mathf.RoundToInt(owned).ToString() + "%";
    }
}
