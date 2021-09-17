using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{




    public TileHandler[] Handlers;


    private Dictionary<string, TileHandler> handlers;


    public TileHandler this[string name]
    {
        get
        {
            return handlers[name];
        }
    }

    public void Init()
    {
        foreach(var TH in Handlers)
        {
            TH.Init();
            handlers[TH.Name] = TH;
        }
    }

    public static MapManager MM;

    private void Awake()
    {
        if(MM == null)
        {
            MM = this;
        }
        else
        {
            Destroy(gameObject);
        }

        
    }
}
