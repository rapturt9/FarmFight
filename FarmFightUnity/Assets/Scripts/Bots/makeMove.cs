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
    public SoldierManager soldierManager;
    public int actionTimer = 0;
    public Hex startingLoc;

    public double money;
    private int botPlayerId;

    private int numActions = 0;

    // Start is called before the first frame update
    public void Init(int botPlayerId, GameState gameData, CropManager cropManager, TileManager tileManager, TileHandler tileHandler, SoldierManager soldierManager)
    {
        this.botPlayerId = botPlayerId;
        this.gameData = gameData;
        this.cropManager = cropManager;
        this.tileManager = tileManager;
        this.tileHandler = tileHandler;
        this.soldierManager = soldierManager;
        money = Repository.Central.money;
        hexCoords = BoardHelperFns.HexList(3);
        StartCoroutine(startGame());
    }

    IEnumerator startGame ()
    {
        yield return new WaitForSeconds(0.5F);
        gameData.updateGameState();
        getStartingLoc(true);
        while (gameIsRunning){
            getStartingLoc(false);
            pickMove(botPlayerId);
            numActions += 1;
            actionTimer = (actionTimer + 1) % 4;
            yield return new WaitForSeconds(0.5F);
        }
    }

    void getStartingLoc (bool start){
        if (start) {
            foreach (var coord in hexCoords){
                TileSyncData tile = gameData.cropTiles[coord];
                if (tile.tileOwner == botPlayerId) {
                    startingLoc = coord;
                }
            }
        }
        else {
            TileSyncData tile = gameData.cropTiles[startingLoc];
            if (tile.tileOwner != botPlayerId) {
                getStartingLoc(true);
            }
        }
    }

    void pickMove (int player) {
        gameData.updateGameState();
        List<(Hex,string)> possibleMoves = getMoves(player);
        var (loc, bestMove) = evaluateStates(possibleMoves, player);
        print(bestMove);

        if (bestMove == "harvest"){
            double add = cropManager.harvest(loc, player);
            if (add > 0)
            {
                money += add;
            }
        }
        else if (bestMove == "soldier"){
            soldierManager.addSoldier(startingLoc, player);
            soldierManager.SendSoldier(startingLoc, loc, player);
            money -= 10;
        }
        else if (bestMove == "farmer"){
            cropManager.addFarmer(loc, player);
            money -= 5;
        }
        else if (bestMove == "plantRice" || bestMove == "plantRiceOver"){
            cropManager.addCrop(loc,CropType.rice, player);
            money -= 2.0;
        }
        else if (bestMove == "plantCarrot" || bestMove == "plantCarrotOver"){
            cropManager.addCrop(loc,CropType.carrot, player);
            money -= 2.0;
        }
        else if (bestMove == "plantPotato"){
            cropManager.addCrop(loc,CropType.potato, player);
            money -= 1.0;
        }
        else if (bestMove == "plantEggplant" || bestMove == "plantEggplantOver"){
            cropManager.addCrop(loc,CropType.eggplant, player);
            money -= 10;
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
            else if (move == "farmer"){
                currentVal = 15.2f;
            }
            else if (move == "soldier"){
                currentVal = 15.1f;
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

            currentVal += 0.01f - 0.001f*BoardHelperFns.distance(startingLoc,loc);

            if (currentVal == bestVal){
                if (Random.Range(0,2) == 1){
                    bestVal = currentVal;
                    bestMove = elem;
                }
            }
            else if (currentVal > bestVal){
                bestVal = currentVal;
                bestMove = elem;
            }
        }
        var (x,y) = bestMove;
        if (bestVal < 0.75){
            return (new Hex (0,0),"None");
        }
        return bestMove;
    }

    (Dictionary<Hex, TileSyncData>,float) getState(Hex loc, string move, int player){
        Dictionary<Hex, TileSyncData> res = new Dictionary<Hex, TileSyncData>(gameData.cropTiles);
        TileSyncData oldTile = res[loc];
        TileSyncData updatedTile = new TileSyncData(CropType.blankTile, 0.0f, false, -1, true, oldTile.tileDamage);
        float moneyGained = 0.0f;
        if (move == "plantRice" || move == "plantRiceOver"){
            updatedTile = new TileSyncData(CropType.rice, 0.0f, oldTile.containsFarmer, player, true, 0.0f);
            moneyGained = -2.0f;
        }
        else if (move == "plantCarrot" || move == "plantCarrotOver"){
            updatedTile = new TileSyncData(CropType.carrot, 0.0f, oldTile.containsFarmer, player, true, 0.0f);
            moneyGained = -2.0f;
        }
        else if (move == "plantPotato"){
            updatedTile = new TileSyncData(CropType.potato, 0.0f, oldTile.containsFarmer, player, true, 0.0f);
            moneyGained = -1.0f;
        }
        else if (move == "plantEggplant" || move == "plantEggplantOver"){
            updatedTile = new TileSyncData(CropType.eggplant, 0.0f, oldTile.containsFarmer, player, true, 0.0f);
            moneyGained = -10.0f;
        }
        else if (move == "harvest") {
            updatedTile = new TileSyncData(oldTile.cropType, NetworkManager.Singleton.NetworkTime, oldTile.containsFarmer, oldTile.tileOwner, true, oldTile.tileDamage);
            moneyGained = (float)getCropVal(oldTile.cropType, loc);
        }
        else if (move == "farmer") {
            updatedTile = new TileSyncData(oldTile.cropType, NetworkManager.Singleton.NetworkTime, true, oldTile.tileOwner, true, oldTile.tileDamage);
            moneyGained = 0.0f;
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
                if (!tileHandler[coord].containsFarmer){
                    res.Add((coord,"harvest"));
                }
                
                if (tileHandler[coord].containsFarmer == false && 
                tileHandler[coord].tileOwner == botPlayerId && 
                !tileHandler[coord].hostileOccupation && money >= 5){
                    res.Add((coord,"farmer"));
                }
                
                if (actionTimer == 0){
                    if (tile.cropType == CropType.potato){
                        if (money >= 2) {
                            res.Add((coord,"plantRiceOver"));
                        }
                        else if (money >= 2) {
                            res.Add((coord,"plantCarrotOver"));
                        }
                        else if (money >= 10) {
                            res.Add((coord,"plantEggplantOver"));
                        }
                    }
                    else if (tile.cropType == CropType.rice){
                        if (money >= 2) {
                            res.Add((coord,"plantCarrotOver"));
                        }
                        if (money >= 10) {
                            res.Add((coord,"plantEggplantOver"));
                        }
                    }
                    else if (tile.cropType == CropType.carrot){
                        if (money >= 10) {
                            res.Add((coord,"plantEggplantOver"));
                        }
                    }

                    foreach (var newLoc in tileManager.getValidNeighbors(coord)){
                        if (hexCoords.Contains(newLoc) && (gameData.cropTiles[newLoc].tileOwner == -1) && (gameData.cropTiles[newLoc].cropType == CropType.blankTile)){
                            if (!res.Contains((newLoc,"plantRice")) && money >= 2){
                                res.Add((newLoc,"plantRice"));
                            }
                            if (!res.Contains((newLoc,"plantCarrot")) && money >= 2){
                                res.Add((newLoc,"plantCarrot"));
                            }
                            if (!res.Contains((newLoc,"plantPotato")) && money >= 1){
                                res.Add((newLoc,"plantPotato"));
                            }
                            if (!res.Contains((newLoc,"plantEggplant")) && money >= 10){
                                res.Add((newLoc,"plantEggplant"));
                            }
                        }
                    }
                }
            }
            else if (tile.tileOwner != -1 && money >= 10){
                foreach (var newLoc in tileManager.getValidNeighbors(coord)){
                    if (tileHandler[coord].tileOwner == botPlayerId){
                        res.Add((coord,"soldier"));
                    }
                }
            }
        }
        return res;
    }
}
