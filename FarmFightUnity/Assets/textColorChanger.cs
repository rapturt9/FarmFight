using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class textColorChanger : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        text.color = Repository.Central.TeamColors[Repository.Central.localPlayerId];
    }

    public TMP_Text text;
}
