using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Messaging;

public class GameManager : NetworkBehaviour
{
    public TileManager tileManager;
    public TileHandler[] TileHandler;
    public GameState gameState;

    public bool gameIsRunning = false;
    int localPlayerId = 0;
    private List<Hex> openCorners;
    Repository central;

    // Start is called before the first frame update
    private void Awake()
    {

    }

    private void Start()
    {
        central = Repository.Central;

        central.GamesMode = PlayState.NormalGame;

    }

    public override void NetworkStart()
    {
        TileArtRepository.Art.Init();
        TileManager.TM.Init();
        gameState.Init();
        SetupCorners();

        // Adds a new player and gets their ID
        addNewPlayerServerRpc(NetworkManager.Singleton.LocalClientId);

        // Now, lets things update
        gameIsRunning = true;
        central.gameIsRunning = gameIsRunning;
    }

    // Update is called once per frame
    void Update()
    {

        if (!gameIsRunning) { return; }

        Hex hex = TileManager.TM.getMouseHex();

        if (Input.GetMouseButtonDown(0) &
                TileManager.TM.isValidHex(hex))
        {
            Repository.Central.selectedHex = hex;
        }

    }


    /// <summary>
    /// normalGame Function
    /// </summary>

    public void NormalGameFunction()
    {
        Market.market.MarketUpdateFunctionality();
    }





    // Sets up corners for players to start. Only called server-side
    void SetupCorners()
    {
        if (!IsServer) { return; }

        openCorners = new List<Hex>();

        foreach (var relative in TileManager.TM.getValidNeighbors(new Hex(0,0)))
        {
            openCorners.Add(relative * TileManager.TM.size);
        }
    }

    // Handles a new client connecting
    [ServerRpc(RequireOwnership = false)]
    void addNewPlayerServerRpc(ulong targetClientId)
    {
        // Sets the player in a random corner
        int index = Random.Range(0, openCorners.Count - 1);
        Hex newCorner = openCorners[index];
        openCorners.RemoveAt(index);

        Potato startingTile = new Potato();
        startingTile.tileOwner = localPlayerId;
        int cropTileHandlerIndex = 0;
        TileManager.TM.Handlers[cropTileHandlerIndex][newCorner] = startingTile;

        // Serializes entire board
        TileSyncData[] allTiles = gameState.SerializeBoard();

        // Only targets the most recently connected player
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { targetClientId }
            }
        };
        addNewPlayerClientRpc(localPlayerId, allTiles, clientRpcParams);

        // Adds player index
        localPlayerId++;
    }

    [ClientRpc]
    void addNewPlayerClientRpc(int localPlayerId, TileSyncData[] allTiles, ClientRpcParams clientRpcParams = default)
    {
        central.localPlayerId = localPlayerId;
        gameState.DeserializeBoard(allTiles);
    }
}