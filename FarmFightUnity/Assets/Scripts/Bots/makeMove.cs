using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class makeMove : MonoBehaviour
{
    public bool gameIsRunning = true;
    public GameState gameData;
    public List<Hex> hexCoords;

    // Start is called before the first frame update
    void Start()
    {
        hexCoords = BoardHelperFns.HexList(3);
        StartCoroutine(startGame());
    }
    
    IEnumerator startGame ()
    {

        while (gameIsRunning){
            yield return new WaitForSeconds(2.0F);
            pickMove();
            //print("picking move");
        }

    }

    void pickMove () {
        //gameData.updateGameState();
        gameData.tileHandler[new Hex(3,0)] = GameState.DeserializeTile(new TileSyncData(CropType.carrot, 0.0f, false, 0));
        gameData.tileHandler.SyncTile(new Hex(3,0));
        gameData.updateGameState();
        List<(Hex,string)> possibleMoves = getMoves(0);
        /*foreach (var coord in hexCoords)
        {
            print(coord);
            print(gameData.cropTiles[coord]);
        }*/
        /*foreach (var elem in possibleMoves){
            print(elem);
        }*/
    }

    List<(Hex,string)> getMoves(int owner) {
        List<(Hex,string)> res = new List<(Hex,string)>();

        List<Hex> dirs = new List<Hex>();
        dirs.Add(new Hex(1, 0));
        dirs.Add(new Hex(-1, 0));
        dirs.Add(new Hex(0, 1));
        dirs.Add(new Hex(0, -1));

        foreach (var coord in hexCoords){
            TileSyncData tile = gameData.cropTiles[coord];
            if (tile.tileOwner == owner) {
                //print("owned tile");
                if (tile.cropType == CropType.blankTile){
                    res.Add((coord,"plantRice"));
                    res.Add((coord,"plantCarrot"));
                    res.Add((coord,"plantPotato"));
                }
                else{
                    res.Add((coord,"harvest"));
                }

                foreach (var dir in dirs){
                    Hex newLoc = coord+dir;

                    //print(newLoc);

                    if (hexCoords.Contains(newLoc) && gameData.cropTiles[newLoc].tileOwner == -1){
                        if (!res.Contains((newLoc,"plantRice"))){
                            res.Add((coord,"plantRice"));
                        }
                        if (!res.Contains((newLoc,"plantCarrot"))){
                            res.Add((coord,"plantCarrot"));
                        }
                        if (!res.Contains((newLoc,"plantPotato"))){
                            res.Add((coord,"plantPotato"));
                        }
                    }
                }
            }
        }

        return res;
    }
}
