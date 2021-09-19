using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{

    public TileHandler[] Handlers;

    public static TileManager TM;

    private Dictionary<string, TileHandler> handlers;

    public Hex selected;


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

    

    private void Awake()
    {
        if(TM == null)
        {
            TM = this;
        }
        else
        {
            Destroy(gameObject);
        }

        
    }

}
