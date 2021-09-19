using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
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
            handlers = new Dictionary<string, TileHandler>();

            
            handlers.Add(TH.Name,TH);
        }
    }

    public static TileManager TM;

    public void Awake()
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

    public Hex ScreenToHex(Vector3Int screenPos )
    {
        Vector3 Point = Camera.main.ScreenToWorldPoint(screenPos);

        return Hex.fromWorld(Point);
    }

    public Hex ViewPortToHex(Vector3 viewPos)
    {
        return Hex.fromWorld(Camera.main.ViewportToWorldPoint(viewPos));
    }

    public Hex WorldToHex(Vector3 worldPos)
    {
        return Hex.fromWorld(worldPos);
    }

    public Hex Move(Hex current,int x, int y)
    {
        return current +  (Hex.right * x) + (Hex.up * y);
    }


    
}
