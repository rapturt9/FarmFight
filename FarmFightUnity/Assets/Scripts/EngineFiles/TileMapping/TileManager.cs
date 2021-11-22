using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public int size;
    public static int hexagonSides = 6;

    public TileHandler[] Handlers;

    public Dictionary<string, TileHandler> handlers;

    public TileHandler this[string name]
    {
        get
        {
            return handlers[name];
        }
    }

    public TileTemp this[string name, Hex coord ]
    {
        get
        {
            return getTile(name,coord);
        }

        set
        {
            SetTile(name, coord, value);
        }
    }

    public void Init()
    {
        handlers = new Dictionary<string, TileHandler>();
        foreach (var TH in Handlers)
        {
            TH.Init(size);
            handlers.Add(TH.Name,TH);
        }

        validHexes = new HashSet<Hex>(BoardHelperFns.HexList(size)) ;
    }

    public HashSet<Hex> validHexes;

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

    public string[] TileMapNames 
    {
        get
        {
            return new List<string>(handlers.Keys).ToArray();
        }
    }

    /// <summary>
    /// convert screen coords to hex coords
    /// </summary>
    /// <param name="screenPos"></param>
    /// <returns></returns>
    public Hex screenToHex(Vector3Int screenPos )
    {
        Vector3 Point = Camera.main.ScreenToWorldPoint(screenPos);

        return Hex.fromWorld(Point);
    }

    /// <summary>
    /// convert viewport coords to hex coords
    /// </summary>
    /// <param name="viewPos"></param>
    /// <returns></returns>
    public Hex viewPortToHex(Vector3 viewPos)
    {
        return Hex.fromWorld(Camera.main.ViewportToWorldPoint(viewPos));
    }

    /// <summary>
    /// convert world coords to hex coords
    /// </summary>
    /// <param name="worldPos"></param>
    /// <returns></returns>
    public Hex worldToHex(Vector3 worldPos)
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
    public Hex move(Hex current,int x, int y)
    {
        return current + (Hex.right * x) + (Hex.up * y);
    }


    /// <summary>
    /// use to get a tile on a certain tile map
    /// </summary>
    /// <param name="mapName"></param>
    /// <param name="coord"></param>
    /// <returns></returns>
    public TileTemp getTile(string mapName, Hex coord)
    {
        return this[mapName][coord];
    }

    /// <summary>
    /// use to set a tile on a certain tile map
    /// </summary>
    /// <param name="mapName"></param>
    /// <param name="coord"></param>
    /// <param name="tile"></param>
    public void SetTile(string mapName, Hex coord, TileTemp tile)
    {
        this[mapName][coord] = tile;
        
    }

    public Hex getMouseHex()
    {
        return screenToHex(Vector3Int.FloorToInt(Input.mousePosition));
    }

    public bool isValidHex(Hex hex)
    {
        
        return validHexes.Contains(hex);
    }

    public Hex[] getNeighbors(Hex hex)
    {
        List<Hex> relatives = new List<Hex>()
        {
            Hex.down,
            Hex.up,
            Hex.right,
            Hex.left,
            Hex.right + Hex.down,
            Hex.left + Hex.up
        };

        List<Hex> final = new List<Hex>();

        foreach (var rel in relatives)
        {
            
            final.Add(hex + rel);

        }

        return final.ToArray();
    }

    public Hex[] getValidNeighbors(Hex hex)
    {
        List<Hex> relatives = new List<Hex>(getNeighbors(hex));

        int i = 0;

        while (i < relatives.Count)
        {
            if (isValidHex(relatives[i]))
                i += 1;
            else
                relatives.RemoveAt(i);
        }

        return relatives.ToArray();
    }

    public Vector3 HexToWorld(Hex hex)
    {
        return hex.world(transform.lossyScale.y);
    }
}
