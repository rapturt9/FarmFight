using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyTeamColor : MonoBehaviour
{

    public OutlineSetter outline;
    // Update is called once per frame
    void FixedUpdate()
    {
        GetComponent<SpriteRenderer>().color = outline.TeamColors[Repository.Central.localPlayerId];
    }
}
