using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CursorChanger : MonoBehaviour
{
    // Start is called before the first frame update

    public OutlineSetter Outline;

    void FixedUpdate()
    {
        Color color = Outline.TeamColors[Repository.Central.localPlayerId];
        GetComponent<Tilemap>().color = new Color(color.r, color.g, color.b, .5f);
        
    }

    
}
