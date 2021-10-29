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

    private int numActions = 0;

    // Start is called before the first frame update
    public void Init()
    {
        hexCoords = BoardHelperFns.HexList(3);
        StartCoroutine(startGame());
    }
    
    public List<Hex> updateHexCoords() {
        List<Hex> neighbors = new List<Hex>();
        foreach (var coord in hexCoords){
            if (gameData.cropTiles[coord].tileOwner == Repository.Central.localPlayerId){
                print("found start");
                neighbors.Add(coord);
            }
            print(coord);
            print(gameData.cropTiles[coord].tileOwner);
        }
        
        List<Hex> temp = new List<Hex>();
        while (neighbors.Count != 0){
            Hex currentNeighbor = neighbors[0];
            neighbors.RemoveAt(0);
            temp.Add(currentNeighbor);
            foreach (var n in tileManager.getValidNeighbors(currentNeighbor)){
                if (!neighbors.Contains(n)){
                    neighbors.Add(n);
                }
            }
        }
        return temp;
    }

    IEnumerator startGame ()
    {
        yield return new WaitForSeconds(0.1F);
        //In progress stuff
        //updateHexCoords();
        /*print(hexCoords.Count);
        print(updateHexCoords().Count);
        foreach (var x in updateHexCoords()){
            print(x);
        }*/
        //gameData.updateGameState();
        //hexCoords = updateHexCoords();
        while (gameIsRunning){
            yield return new WaitForSeconds(0.5F);
            pickMove(Repository.Central.localPlayerId);
            numActions += 1;
        }
    }

    void pickMove (int player) {
        gameData.updateGameState();
        List<(Hex,string)> possibleMoves = getMoves(player);
        var (loc, bestMove) = evaluateStates(possibleMoves, player);

        if (bestMove == "harvest"){
            double add = cropManager.harvest(loc);
            if (add > 0)
            {
                Repository.Central.money += add;
            }
        }
        else if (bestMove == "plantRice" || bestMove == "plantRiceOver"){
            cropManager.addCrop(loc,CropType.rice);
            Repository.Central.money -= 2.0;
        }
        else if (bestMove == "plantCarrot" || bestMove == "plantCarrotOver"){
            cropManager.addCrop(loc,CropType.carrot);
            Repository.Central.money -= 2.0;
        }
        else if (bestMove == "plantPotato"){
            cropManager.addCrop(loc,CropType.potato);
            Repository.Central.money -= 1.0;
        }
        else if (bestMove == "plantEggplant" || bestMove == "plantEggplantOver"){
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
                currentVal = moneyGained;
            }
            else if (move == "plantPotato") {
                currentVal = 1.5f;
                if (numActions > 40){
                    currentVal = 0.0f;
                }
            }
            else if (move == "plantRice") {
                currentVal = 3.0f;
                if (numActions > 60){
                    currentVal = 0.0f;
                }
            }
            else if (move == "plantCarrot") {
                currentVal = 6.0f;
                if (numActions > 90){
                    currentVal = 0.0f;
                }
            }
            else if (move == "plantEggplant") {
                currentVal = 15.0f;
            }
            else if (move == "plantRiceOver") {
                currentVal = 2.9f;
                if (numActions > 60){
                    currentVal = 0.0f;
                }
            }
            else if (move == "plantCarrotOver") {
                currentVal = 5.9f;
                if (numActions > 90){
                    currentVal = 0.0f;
                }
            }
            else if (move == "plantEggplantOver") {
                currentVal = 14.9f;
            }

            if (currentVal >= bestVal){
                bestVal = currentVal;
                bestMove = elem;
            }
        }
        //print(bestMove);
        //print(bestVal);
        var (x,y) = bestMove;
        if (bestVal < 0.75){
            return (new Hex (0,0),"None");
        }
        return bestMove;
    }

    (Dictionary<Hex, TileSyncData>,float) getState(Hex loc, string move, int player){
        Dictionary<Hex, TileSyncData> res = new Dictionary<Hex, TileSyncData>(gameData.cropTiles);
        TileSyncData oldTile = res[loc];
        TileSyncData updatedTile = new TileSyncData(CropType.blankTile, 0.0f, false, -1, true);
        float moneyGained = 0.0f;
        if (move == "plantRice" || move == "plantRiceOver"){
            updatedTile = new TileSyncData(CropType.rice, 0.0f, oldTile.containsFarmer, player, true);
            moneyGained = -2.0f;
        }
        else if (move == "plantCarrot" || move == "plantCarrotOver"){
            updatedTile = new TileSyncData(CropType.carrot, 0.0f, oldTile.containsFarmer, player, true);
            moneyGained = -2.0f;
        }
        else if (move == "plantPotato"){
            updatedTile = new TileSyncData(CropType.potato, 0.0f, oldTile.containsFarmer, player, true);
            moneyGained = -1.0f;
        }
        else if (move == "plantEggplant" || move == "plantEggplantOver"){
            updatedTile = new TileSyncData(CropType.eggplant, 0.0f, oldTile.containsFarmer, player, true);
            moneyGained = -10.0f;
        }
        else if (move == "harvest") {
            updatedTile = new TileSyncData(oldTile.cropType, NetworkManager.Singleton.NetworkTime, oldTile.containsFarmer, oldTile.tileOwner, true);
            moneyGained = (float)getCropVal(oldTile.cropType, loc);
        }
        else{
            updatedTile = oldTile;
        }

        res[loc] = updatedTile;

        return (res,moneyGained);
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
                
                if (tile.cropType == CropType.potato){
                    if (Repository.Central.money >= 2) {
                        res.Add((coord,"plantRiceOver"));
                    }
                    if (Repository.Central.money >= 2) {
                        res.Add((coord,"plantCarrotOver"));
                    }
                    if (Repository.Central.money >= 10) {
                        res.Add((coord,"plantEggplantOver"));
                    }
                }
                else if (tile.cropType == CropType.rice){
                    if (Repository.Central.money >= 2) {
                        res.Add((coord,"plantCarrotOver"));
                    }
                    if (Repository.Central.money >= 10) {
                        res.Add((coord,"plantEggplantOver"));
                    }
                }
                else if (tile.cropType == CropType.carrot){
                    if (Repository.Central.money >= 10) {
                        res.Add((coord,"plantEggplantOver"));
                    }
                }
                res.Add((coord,"harvest"));
                
                foreach (var newLoc in tileManager.getValidNeighbors(coord)){
                    if (hexCoords.Contains(newLoc) && (gameData.cropTiles[newLoc].tileOwner == -1) && (gameData.cropTiles[newLoc].cropType == CropType.blankTile)){
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
                            res.Add((newLoc,"plantEggplant"));
                        }
                    }
                }
            }
        }
        return res;
    }
}
