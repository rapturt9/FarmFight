using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class onlyOneSoundtrack : MonoBehaviour
{
    public static onlyOneSoundtrack track;
    private void Awake()
    {
        if(track != null)
        {
            Destroy(gameObject);
        }
        else
        {
            track = this;
        }
    }

   
}
