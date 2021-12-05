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
    public bool startOnNetworkStart = false; // DEBUG
    public List<int> realPlayers = new List<int>();
    private List<Hex> openCorners;
    Repository central;
    PhotonRealtimeTransport transport;

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
        {
            GameStart();
        }
    }

    // Starts the game for everyone, called from server
    [ClientRpc]
    public void GameStartClientRpc()
    {
        if (!IsServer)
        {
            GameStart();
        }
    }

    public void GameStart()
    {
        interstitial.SetActive(false);
        TileArtRepository.Art.Init();
        TileManager.TM.Init();
        Repository.Central.timer.init(Repository.Central.time.x, Repository.Central.time.y);

        if (IsServer)
        {
            if (SceneVariables.cameThroughMenu)
            {
                HostStartGame();
            }
            else
            {
                SetupCorners();
            }
        }

        // Adds a new player and gets their ID
        addNewPlayerServerRpc(NetworkManager.Singleton.LocalClientId);

        // Now, let things update
        central.gameIsRunning = true;
        BoardChecker.Checker.StartChecking();
    }

    // Host starting game does server stuff first, then sends a message to clients to connect
    public void HostStartGame()
    {
        transport.Client.CurrentRoom.IsOpen = false;
        transport.Client.CurrentRoom.IsVisible = false;

        // Bots
        int numPlayers = NetworkManager.Singleton.ConnectedClientsList.Count;
        botsToAdd = SceneVariables.maxBots;
        if (botsToAdd + numPlayers > Repository.maxPlayers)
        {
            botsToAdd = Repository.maxPlayers - numPlayers;
        }
        totalPlayersAndBots = numPlayers + botsToAdd;
        SetupCorners();
        gameState.Init(botsToAdd, numPlayers);
        //print(TileManager.TM["CropTiles"][new Hex(0, -3)].tileOwner);

        GameStartClientRpc();
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

#if UNITY_EDITOR || UNITY_STANDALONE
        Hex hex = TileManager.TM.getMouseHex();
        if (Input.GetMouseButtonDown(0) &
            TileManager.TM.isValidHex(hex))
        {
            Repository.Central.selectedHex = hex;
        }
#elif UNITY_IOS || UNITY_ANDROID
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Hex hex = TileManager.TM.getTouchHex(0);
            // Clicking tile
            if (touch.phase == TouchPhase.Began && TileManager.TM.isValidHex(hex))
            {
                Repository.Central.selectedHex = hex;
            }
        }
#endif
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
        if (totalPlayersAndBots <= 1 || totalPlayersAndBots >= 5)
        {
            openCorners.Add(new Hex(1, 0) * TileManager.TM.size);
            openCorners.Add(new Hex(-1, 0) * TileManager.TM.size);
            openCorners.Add(new Hex(0, 1) * TileManager.TM.size);
            openCorners.Add(new Hex(0, -1) * TileManager.TM.size);
            openCorners.Add(new Hex(1, -1) * TileManager.TM.size);
            openCorners.Add(new Hex(-1, 1) * TileManager.TM.size);
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
        if (!SceneVariables.cameThroughMenu)
        {
            totalPlayersAndBots++;
        }
    }

    [ClientRpc]
    void addNewPlayerClientRpc(int localPlayerId, TileSyncData[] allTiles, ClientRpcParams clientRpcParams = default)
    {
        central.localPlayerId = localPlayerId;
        gameState.DeserializeBoard(allTiles);
    }

    public IEnumerator StartFromMainScene()
    {
        yield return new WaitForSeconds(2);
        GameStart();
        yield return null;
    }

    // Actually adds player at corner, can be used for bots
    public void addNewPlayer(int playerId)
    {
        // Sets the player in a random corner
        int index = Random.Range(0, openCorners.Count);
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