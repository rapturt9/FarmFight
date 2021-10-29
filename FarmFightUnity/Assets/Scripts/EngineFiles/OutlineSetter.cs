using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class OutlineSetter : MonoBehaviour
{
    public OutlineTile[] Outlines;
    private Dictionary<int, OutlineTile> SortedTiles;

    public TileHandler crops;

    public Tilemap[] Tilemaps;

    List<Hex> relatives = new List<Hex>()
        {
            Hex.up,
            Hex.right,
            Hex.right + Hex.down,
            Hex.down,
            Hex.left,
            Hex.left + Hex.up
        };


    public Color[] TeamColors;
    public static OutlineSetter OS;

    // Start is called before the first frame update
    private void Awake()
    {
        if (OS == null)
        {
            OS = this;
        }
        else if (OS != this)
        {
            Destroy(gameObject);
        }

        SortedTiles = new Dictionary<int, OutlineTile>();

        foreach(var outline in Outlines)
        {
            for(int i = 0; i < 6; i++)
            {
                OutlineTile tile = outline.rotate(i);


                if (!SortedTiles.ContainsKey(tile.index))
                    SortedTiles.Add(tile.index, tile);

            }
        }

        for(int i = 0; i<6; i++)
        {
            Tilemaps[i].color = TeamColors[i];
        }

        relatives = new List<Hex>()
        {
            Hex.up,
            Hex.right,
            Hex.right + Hex.down,
            Hex.down,
            Hex.left,
            Hex.left + Hex.up
        };

        
        
    }

    

    
    public void RedrawTiles(int owner)
    {
        

        Tilemaps[owner].ClearAllTiles();

        foreach(var tile in crops.TileDict.Values)
        {
            

            if(tile.Tile.tileOwner == owner)
            {
                int index = getOutlineIndex(tile.hexCoord, owner);

                if(SortedTiles.ContainsKey(index))
                    SortedTiles[index].Draw(Tilemaps[owner], tile.hexCoord);
                else
                    SortedTiles[0].Draw(Tilemaps[owner], tile.hexCoord);
            }
            
        }
    }
    
    public Tilemap tilemap;

    public void RedrawTiles()
    {


        tilemap.ClearAllTiles();

        foreach (var tile in crops.TileDict.Values)
        {

            int owner = tile.Tile.tileOwner;

            if (owner == -1)
                continue;
            
            int index = getOutlineIndex(tile.hexCoord, owner);

            if (SortedTiles.ContainsKey(index))
                SortedTiles[index].Draw(tilemap, tile.hexCoord, TeamColors[owner]);
            else
                SortedTiles[0].Draw(Tilemaps[owner], tile.hexCoord, TeamColors[owner]);
            

        }
    }

    public int getOutlineIndex(Hex hex, int owner)
    {

        int final = 0;

        for (int i = 0; i < 6; i++)
        {
            if(!TileManager.TM.isValidHex(relatives[i] + hex) || crops[relatives[i] + hex].tileOwner != owner )
                    final += 1 << (i);

        }

        return final;
    }

    private void Start()
    {

        //Debug.Log(SortedTiles.Count);
        //StartCoroutine("RefreshOutlines");
    }


    int owner = 0;

    private void Update()
    {

        RedrawTiles(owner);

        //RedrawTiles();

        owner++;
        owner %= 6;
    }

    private IEnumerator RefreshOutlines()
    {

        while (true)
        {
            //RedrawTiles(owner);

            RedrawTiles();

            owner++;
            owner %= 6;

            yield return new WaitForSeconds(.05f);
        }



    }
}
