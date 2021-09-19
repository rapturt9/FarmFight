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

    /// <summary>
    /// convert screen coords to hex coords
    /// </summary>
    /// <param name="screenPos"></param>
    /// <returns></returns>
    public Hex ScreenToHex(Vector3Int screenPos )
    {
        Vector3 Point = Camera.main.ScreenToWorldPoint(screenPos);

        return Hex.fromWorld(Point);
    }

    /// <summary>
    /// convert viewport coords to hex coords
    /// </summary>
    /// <param name="viewPos"></param>
    /// <returns></returns>
    public Hex ViewPortToHex(Vector3 viewPos)
    {
        return Hex.fromWorld(Camera.main.ViewportToWorldPoint(viewPos));
    }

    /// <summary>
    /// convert world coords to hex coords
    /// </summary>
    /// <param name="worldPos"></param>
    /// <returns></returns>
    public Hex WorldToHex(Vector3 worldPos)
    {
        return Hex.fromWorld(worldPos);
    }

    /// <summary>
    /// Move the hex 
    /// </summary>
    /// <param name="current"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public Hex Move(Hex current,int x, int y)
    {
        return current +  (Hex.right * x) + (Hex.up * y);
    }


    /// <summary>
    /// use to get a tile on a certain tile map
    /// </summary>
    /// <param name="mapName"></param>
    /// <param name="coord"></param>
    /// <returns></returns>
    public TileTemp GetTile(string mapName, Hex coord)
    {
        return this[mapName][coord];
    }

    /// <summary>
    /// use to set a tile on a certain tile map
    /// </summary>
    /// <param name="mapName"></param>
    /// <param name="coord"></param>
    /// <param name="tile"></param>
    public void setTile(string mapName, Hex coord, TileTemp tile)
    {
        this[mapName][coord] = tile;
    }
}
