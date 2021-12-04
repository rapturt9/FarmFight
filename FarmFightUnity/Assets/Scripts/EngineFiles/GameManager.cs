using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.Transports.PhotonRealtime;

public class GameManager : NetworkBehaviour
{
    public TileManager tileManager;
    public TileHandler[] TileHandler;
    public GameState gameState;
    public GameObject interstitial;
    public GameObject disconnected;

    public int currMaxLocalPlayerId = 0;
    public int totalPlayersAndBots = 0;
    public int botsToAdd = 0;
    public List<int> realPlayers = new List<int>();
    private List<Hex> openCorners;
    Repository central;
    PhotonRealtimeTransport transport;
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
        transport = NetworkManager.Singleton.GetComponent<PhotonRealtimeTransport>();

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
        Repository.Central.timer.init(Repository.Central.time.x, Repository.Central.time.y);

        if (IsServer)
        {
            // Only make private if starting from the menu
            if (SceneVariables.cameThroughMenu)
            {
                transport.Client.CurrentRoom.IsOpen = false;
                transport.Client.CurrentRoom.IsVisible = false;
            }

            // Bots
            int numPlayers = NetworkManager.Singleton.ConnectedClientsList.Count;
            botsToAdd = SceneVariables.maxBots;
            if (botsToAdd + numPlayers > Repository.maxPlayers)
            {
                botsToAdd = Repository.maxPlayers - numPlayers;
            }
            gameState.Init(botsToAdd, numPlayers);
            totalPlayersAndBots += botsToAdd;

            SetupCorners();
        }

        // Adds a new player and gets their ID
        addNewPlayerServerRpc(NetworkManager.Singleton.LocalClientId);

        // Now, let things update
        central.gameIsRunning = true;
        BoardChecker.Checker.StartChecking();
    }

    // Update is called once per frame
    void Update()
    {
        if (!central.gameIsRunning) { return; }

        // Exit game if host or we disconnect
        if (!IsClient)
        {
            central.gameIsRunning = false;
            StartCoroutine("Disconnect");
        }

        Hex hex = TileManager.TM.getMouseHex();

        if (Input.GetMouseButtonDown(0) &
                TileManager.TM.isValidHex(hex))
        {
            Repository.Central.selectedHex = hex;
        }

        // Moved to BoardChecker
        //if (BoardChecker.Checker.ownedTileCount[central.localPlayerId] > 0)
        //    gameStarted = true;
        //gameLost(); 
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
        openCorners = new List<Hex>();

        // Configration changes depending on how many players there are

        // Singleplayer or >= 5 -> everything
        if (totalPlayersAndBots == 1 || totalPlayersAndBots >= 5)
        {
            foreach (var relative in TileManager.TM.getValidNeighbors(new Hex(0, 0)))
            {
                openCorners.Add(relative * TileManager.TM.size);
            }
        }
        // 2 -> top and bottom
        else if (totalPlayersAndBots == 2)
        {
            openCorners.Add(new Hex(0, 1) * TileManager.TM.size);
            openCorners.Add(new Hex(0, -1) * TileManager.TM.size);
        }
        // 3 -> triangle
        else if (totalPlayersAndBots == 3)
        {
            openCorners.Add(new Hex(0, 1) * TileManager.TM.size);
            openCorners.Add(new Hex(1, -1) * TileManager.TM.size);
            openCorners.Add(new Hex(-1, 0) * TileManager.TM.size);
        }
        // 4 -> rectangle
        else if (totalPlayersAndBots == 4)
        {
            openCorners.Add(new Hex(1, 0) * TileManager.TM.size);
            openCorners.Add(new Hex(1, -1) * TileManager.TM.size);
            openCorners.Add(new Hex(-1, 0) * TileManager.TM.size);
            openCorners.Add(new Hex(-1, 1) * TileManager.TM.size);
        }
        // Zero players shouldn't happen
        else
        {
            Debug.LogWarning("Invalid number of players!");
        }
    }

    // Handles a new client connecting
    [ServerRpc(RequireOwnership = false)]
    void addNewPlayerServerRpc(ulong targetClientId)
    {
        realPlayers.Add(currMaxLocalPlayerId);
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
        totalPlayersAndBots++;
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

    private bool gameStarted;

    private void gameLost()
    {
        if(gameStarted &&
            BoardChecker.Checker.ownedTileCount[central.localPlayerId] == 0 &&
            BoardChecker.Checker.soldierCount[central.localPlayerId] == 0)
        {
            Debug.Log("YouLost");
            central.money = 0;
        }
    }

    [ClientRpc]
    public void StartFromMainSceneClientRpc(ClientRpcParams clientRpcParams = default)
    {
        GameStart();
    }

    IEnumerator Disconnect()
    {
        yield return new WaitForSeconds(2);
        disconnected.SetActive(true);
        yield return new WaitForSeconds(5);
        GetComponent<ExitFunction>().exit();
        print("Failed to reconnect");
        yield return null;
    }
}