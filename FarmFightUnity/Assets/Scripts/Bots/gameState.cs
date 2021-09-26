using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameState : MonoBehaviour
{
    public GameObject tiles;
    public TileHandler tileHandler;
    // Hex coord, (crop#, time planted/time last clicked, farmer or not)
    public Dictionary<Hex, (int,float,bool)> dict = new Dictionary<Hex, (int,float,bool)>();
    public List<Hex> hexCoords;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(delayStart(0.5F));
    }

    IEnumerator delayStart (float f) {
        yield return new WaitForSeconds(f);
        tileHandler = tiles.GetComponent<TileHandler>();
        hexCoords = BoardHelperFns.HexList(3);
        updateGameState();
    }

    public void updateGameState () {
        foreach (var coord in hexCoords){
            if (tileHandler.TileDict[coord].Tile is BlankTile){
                dict[coord] = (-1,0.0F,false);
            }
            else {
                TileTemp tileInfo = tileHandler.TileDict[coord].Tile;
                string artName = tileInfo.currentArt.artName;
                int cropNum = -1;
                
                if (artName == "carrot"){
                    cropNum = 0;
                }

                dict[coord] = (cropNum,0.0F,false);
            }
            
        }

        print(dict[new Hex (0,0)]);
        

    }
}
