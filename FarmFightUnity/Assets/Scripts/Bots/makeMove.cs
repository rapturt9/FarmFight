using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class makeMove : MonoBehaviour
{
    public bool gameIsRunning = true;
    public GameState gameData;
    public List<Hex> hexCoords;

    // Start is called before the first frame update
    public void Init()
    {
        hexCoords = BoardHelperFns.HexList(3);
        StartCoroutine(startGame());
    }
    
    IEnumerator startGame ()
    {

        while (gameIsRunning){
            yield return new WaitForSeconds(2.0F);
            pickMove();
        }

    }

    void pickMove () {
        gameData.updateGameState();
        List<(Hex,string)> possibleMoves = getMoves(0);
        (Hex, string) bestMove = evaluateStates(possibleMoves);
    }

    (Hex, string) evaluateStates(List<(Hex,string)> possibleMoves) {
        int bestVal = -1;
        (Hex,string) bestMove = (new Hex(0,0), "harvest");

        foreach (var elem in possibleMoves){
            var (loc,move) = elem;
            var newState = getState(loc,move);

            if (-1 >= bestVal){
                bestVal = -1;
                bestMove = elem;
            }
        }
        return bestMove;
        return (new Hex(0,0),"NotImplemented");
    }

    Dictionary<Hex, TileSyncData> getState(Hex loc, string move){
        Dictionary<Hex, TileSyncData> res = gameData.cropTiles;
        TileSyncData updatedTile = res[loc];
        /*
        if (move == "plantRice"){
            updatedTile.cropNum == 1;
        }
        else if (move == "plantCarrot"){
            updatedTile.cropNum == 2;
        }
        else if (move == "plantPotato"){
            updatedTile.cropNum == 3;
        }
        */
        res[loc] = updatedTile;

        return res;
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

                    if (hexCoords.Contains(newLoc) && gameData.cropTiles[newLoc].tileOwner == -1){
                        if (!res.Contains((newLoc,"plantRice"))){
                            res.Add((newLoc,"plantRice"));
                        }
                        if (!res.Contains((newLoc,"plantCarrot"))){
                            res.Add((newLoc,"plantCarrot"));
                        }
                        if (!res.Contains((newLoc,"plantPotato"))){
                            res.Add((newLoc,"plantPotato"));
                        }
                    }
                }
            }
        }

        return res;
    }
}
