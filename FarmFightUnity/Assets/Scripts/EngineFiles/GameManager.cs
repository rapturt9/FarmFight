using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using MLAPI;
using MLAPI.Messaging;

public class GameManager : NetworkBehaviour
{
    public TileManager tileManager;
    public TileHandler[] TileHandler;
    public GameState gameState;
    public GameObject interstitial;

    public int currMaxLocalPlayerId = 0;
    private List<Hex> openCorners;
    Repository central;
    public bool startOnNetworkStart = false; // DEBUG

    public static GameManager GM;

    // Start is called before the first frame update
    private void Awake()
    {
        Application.targetFrameRate = 30;

        if (GM == null)
        {
            GM = this;
        }
        else if (GM != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        central = Repository.Central;

        central.GamesMode = PlayState.NormalGame;
    }

    public override void NetworkStart()
    {
        if (startOnNetworkStart)
            StartFromMainSceneServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    [ClientRpc]
    public void GameStartClientRpc()
    {
        GameStart();
    }

    public void GameStart()
    {
        interstitial.SetActive(false);
        TileArtRepository.Art.Init();
        TileManager.TM.Init();
        SetupCorners();

        if (IsServer)
        {
            gameState.Init(2, NetworkManager.Singleton.ConnectedClientsList.Count);
        }

        // Adds a new player and gets their ID
        addNewPlayerServerRpc(NetworkManager.Singleton.LocalClientId);

        // Now, lets things update
        central.gameIsRunning = true;
        BoardChecker.Checker.StartChecking();
    }

    // Update is called once per frame
    void Update()
    {
        if (!central.gameIsRunning) { return; }

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
        addNewPlayer(currMaxLocalPlayerId);

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
        addNewPlayerClientRpc(currMaxLocalPlayerId, allTiles, clientRpcParams);

        // Adds player index
        currMaxLocalPlayerId++;
    }

    [ClientRpc]
    void addNewPlayerClientRpc(int localPlayerId, TileSyncData[] allTiles, ClientRpcParams clientRpcParams = default)
    {
        central.localPlayerId = localPlayerId;
        gameState.DeserializeBoard(allTiles);
    }

    // Actually adds player at corner, can be used for bots
    public void addNewPlayer(int playerId)
    {
        // Sets the player in a random corner
        int index = Random.Range(0, openCorners.Count - 1);
        Hex newCorner = openCorners[index];
        openCorners.RemoveAt(index);

        Potato startingTile = new Potato();
        startingTile.tileOwner = playerId;
        int cropTileHandlerIndex = 0;
        TileManager.TM.Handlers[cropTileHandlerIndex][newCorner] = startingTile;
    }


    // Starting directly from the main scene
    [ServerRpc(RequireOwnership = false)]
    public void StartFromMainSceneServerRpc(ulong targetClientId)
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { targetClientId }
            }
        };
        StartFromMainSceneClientRpc(clientRpcParams);
    }

    [ClientRpc]
    public void StartFromMainSceneClientRpc(ClientRpcParams clientRpcParams = default)
    {
        GameStart();
    }
}