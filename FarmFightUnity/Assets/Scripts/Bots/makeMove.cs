using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;

public class makeMove : MonoBehaviour
{
    public bool gameIsRunning = true;
    public GameState gameData;
    public List<Hex> hexCoords;
    public CropManager cropManager;
    public TileManager tileManager;
    public TileHandler tileHandler;

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
            pickMove(0);
        }

    }

    void pickMove (int player) {
        gameData.updateGameState();
        List<(Hex,string)> possibleMoves = getMoves(player);
        var (loc, bestMove) = evaluateStates(possibleMoves, player);
        print(loc);
        print(bestMove);

        if (bestMove == "harvest"){
            double add = cropManager.harvest(loc);
            if (add > 0)
            {
                Repository.Central.money += add;
            }
        }
        
        /*if (bestMove == "plantRice"){
            //update gameState
        }
        else if (bestMove == "plantCarrot"){
            //update gameState
        }
        else if (bestMove == "plantPotato"){
            //update gameState
        }
        else if (bestMove == "harvest"){
            //update gameState
        }*/
    }

    (Hex, string) evaluateStates(List<(Hex,string)> possibleMoves, int player) {
        float bestVal = -1.0f;
        (Hex,string) bestMove = (new Hex(0,0), "harvest");

        foreach (var elem in possibleMoves){
            var (loc,move) = elem;
            var (newState,moneyGained) = getState(loc,move, player);
            float currentVal = (float)moneyGained;
            
            foreach (var coord in hexCoords){
                TileSyncData tile = newState[coord];
                if (tile.tileOwner == player) {
                    currentVal += 12.0f;
                    if (tile.cropType == CropType.potato){
                        currentVal += 1.5f;
                    }
                    else if (tile.cropType == CropType.carrot){
                        currentVal += 3.0f;
                    }
                    else if (tile.cropType == CropType.rice){
                        currentVal += 15.0f;
                    }
                }
            }

            if (currentVal >= bestVal){
                bestVal = currentVal;
                bestMove = elem;
            }
        }
        return bestMove;
    }

    (Dictionary<Hex, TileSyncData>,double) getState(Hex loc, string move, int player){
        Dictionary<Hex, TileSyncData> res = new Dictionary<Hex, TileSyncData>(gameData.cropTiles);
        TileSyncData oldTile = res[loc];
        TileSyncData updatedTile = new TileSyncData(CropType.blankTile, 0.0f, false, -1);
        double gainedMoney = 0.0;
        if (move == "plantRice"){
            updatedTile = new TileSyncData(CropType.rice, 0.0f, oldTile.containsFarmer, player);
            gainedMoney = -10.0f;
        }
        else if (move == "plantCarrot"){
            updatedTile = new TileSyncData(CropType.carrot, 0.0f, oldTile.containsFarmer, player);
            gainedMoney = -2.0f;
        }
        else if (move == "plantPotato"){
            updatedTile = new TileSyncData(CropType.potato, 0.0f, oldTile.containsFarmer, player);
            gainedMoney = -1.0f;
        }
        /*
        else if (move == "plantEggplant"){
            updatedTile = new TileSyncData(CropType.eggplant, oldTile.timeLastPlanted, oldTile.containsFarmer, player);
        }*/
        else if (move == "harvest") {
            updatedTile = new TileSyncData(oldTile.cropType, NetworkManager.Singleton.NetworkTime, oldTile.containsFarmer, oldTile.tileOwner);
            gainedMoney = getCropVal(oldTile.cropType, oldTile.timeLastPlanted);
        }
        else{
            updatedTile = oldTile;
        }

        res[loc] = updatedTile;

        return (res,gainedMoney);
    }

    double getCropVal (CropType cropType, float timeLastPlanted) {
        int hLevel = 0;
        if (cropType == CropType.potato)
        {
            hLevel = 1;
        }
        if (cropType == CropType.carrot)
        {
            hLevel = 2;
        }
        if (cropType == CropType.rice)
        {
            hLevel = 4;
        }
        /*if (cropType == CropType.potato)
        {
            hLevel = 10;
        }*/

        double mid = 5.5; //optimal harvest level

        double calc;
        int frameRate = 60;
        int frameInternal = (int)((NetworkManager.Singleton.NetworkTime - timeLastPlanted) * frameRate);
        double stage = frameInternal / frameRate; //get frameInternal and frameRate

        calc = Mathf.Abs((float)(stage - mid));
        calc = mid - calc;

        if (hLevel > 0)
        {
            double add = calc * hLevel;
            return add;
        }

        return 0;
    }

    List<(Hex,string)> getMoves(int owner) {
        List<(Hex,string)> res = new List<(Hex,string)>();
        foreach (var coord in hexCoords){
            TileSyncData tile = gameData.cropTiles[coord];
            if (tile.tileOwner == owner) {
                if (tile.cropType == CropType.blankTile){
                    if (Repository.Central.money >= 10) {
                        res.Add((coord,"plantRice"));
                    }
                    else if (Repository.Central.money >= 2) {
                        res.Add((coord,"plantCarrot"));
                    }
                    else if (Repository.Central.money >= 1) {
                        res.Add((coord,"plantPotato"));
                    }                    
                }
                else{
                    res.Add((coord,"harvest"));
                }

                foreach (var newLoc in tileManager.getValidNeighbors(coord)){
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
                        if (!res.Contains((newLoc,"plantEggplant"))){
                            res.Add((coord,"plantEggplant"));
                        }
                    }
                }
            }
        }

        return res;
    }
}
