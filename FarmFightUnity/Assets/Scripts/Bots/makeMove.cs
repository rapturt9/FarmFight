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
            yield return new WaitForSeconds(0.5F);
            pickMove(Repository.Central.localPlayerId);
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
        else if (bestMove == "plantRice"){
            cropManager.addCrop(loc,CropType.rice);
            Repository.Central.money -= 2.0;
        }
        else if (bestMove == "plantCarrot"){
            cropManager.addCrop(loc,CropType.carrot);
            Repository.Central.money -= 2.0;
        }
        else if (bestMove == "plantPotato"){
            cropManager.addCrop(loc,CropType.potato);
            Repository.Central.money -= 1.0;
        }
        else if (bestMove == "plantEggplant"){
            cropManager.addCrop(loc,CropType.eggplant);
            Repository.Central.money -= 10;
        }
    }

    (Hex, string) evaluateStates(List<(Hex,string)> possibleMoves, int player) {
        float bestVal = -1.0f;
        (Hex,string) bestMove = (new Hex(0,0), "None");

        foreach (var elem in possibleMoves){
            var (loc,move) = elem;
            float currentVal = 0.0f;
            if (move == "harvest"){
                var (newState,moneyGained) = getState(loc,move, player);
                currentVal = (float)(moneyGained);
            }
            else if (move == "plantPotato") {
                currentVal = 1.0f;
            }
            else if (move == "plantRice") {
                currentVal = 2.0f;
            }
            else if (move == "plantCarrot") {
                currentVal = 4.0f;
            }
            else if (move == "plantEggplant") {
                currentVal = 10.0f;
            }

            if (currentVal >= bestVal){
                bestVal = currentVal;
                bestMove = elem;
            }
        }
        print(bestMove);
        print(bestVal);
        if (bestVal < 0.5){
            return (new Hex (0,0),"None");
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
            gainedMoney = -2.0f;
        }
        else if (move == "plantCarrot"){
            updatedTile = new TileSyncData(CropType.carrot, 0.0f, oldTile.containsFarmer, player);
            gainedMoney = -2.0f;
        }
        else if (move == "plantPotato"){
            updatedTile = new TileSyncData(CropType.potato, 0.0f, oldTile.containsFarmer, player);
            gainedMoney = -1.0f;
        }
        else if (move == "plantEggplant"){
            updatedTile = new TileSyncData(CropType.eggplant, oldTile.timeLastPlanted, oldTile.containsFarmer, player);
            gainedMoney = -10.0f;
        }
        else if (move == "harvest") {
            updatedTile = new TileSyncData(oldTile.cropType, NetworkManager.Singleton.NetworkTime, oldTile.containsFarmer, oldTile.tileOwner);
            gainedMoney = getCropVal(oldTile.cropType, loc);
        }
        else{
            updatedTile = oldTile;
        }

        res[loc] = updatedTile;

        return (res,gainedMoney);
    }

    double getCropVal (CropType cropType, Hex loc) {
        int hLevel = 0;
        if (cropType == CropType.potato)
        {
            hLevel = 1;
        }
        else if (cropType == CropType.carrot)
        {
            hLevel = 4;
        }
        else if (cropType == CropType.rice)
        {
            hLevel = 2;
        }
        else if (cropType == CropType.eggplant)
        {
            hLevel = 10;
        }

        double calc = tileHandler[loc].botReset();
        calc = (double)Mathf.Pow((float)calc,3.0f);

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
                        res.Add((coord,"plantEggplant"));
                    }
                    else if (Repository.Central.money >= 2) {
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
                        if (!res.Contains((newLoc,"plantRice")) && Repository.Central.money >= 2){
                            res.Add((newLoc,"plantRice"));
                        }
                        if (!res.Contains((newLoc,"plantCarrot")) && Repository.Central.money >= 2){
                            res.Add((newLoc,"plantCarrot"));
                        }
                        if (!res.Contains((newLoc,"plantPotato")) && Repository.Central.money >= 1){
                            res.Add((newLoc,"plantPotato"));
                        }
                        if (!res.Contains((newLoc,"plantEggplant")) && Repository.Central.money >= 10){
                            res.Add((coord,"plantEggplant"));
                        }
                    }
                }
            }
        }

        return res;
    }
}
