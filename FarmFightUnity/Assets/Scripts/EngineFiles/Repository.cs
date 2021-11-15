using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.NetworkVariable.Collections;
using MLAPI.Messaging;

public class Repository : NetworkBehaviour
{
    ///Place Variables here

    [SerializeField]
    public Hex selectedHex = Hex.zero;

    public int localPlayerId;
    public bool gameIsRunning = false;

    public MLAPINetworkDictionary<int, double> allMoney = new MLAPINetworkDictionary<int, double>();
    public double startingMoney = 10.0;
    [HideInInspector] public double money
    {
        get => allMoney[localPlayerId];

        set
        {
            UpdateMoneyServerRpc(localPlayerId, value);
        }
    }

    public TileInfo tileinfo = new TileInfo();

    public const int maxPlayers = 6;

    /// <summary>
    /// the central repository to store necessary game info inside
    /// </summary>
    public static Repository Central;

    /// <summary>
    /// ensures that should a usuper attempt to arise,
    /// it shall be strangled in the cradle
    /// </summary>
    void Awake()
    {
        if (Central == null)
        {
            Central = this;
        }
        else if (Central != this)
        {
            Destroy(gameObject);
        }
    }

    public override void NetworkStart()
    {
        if (IsServer)
        {
            for (int playerId = 0; playerId < maxPlayers; playerId++)
            {
                allMoney[playerId] = startingMoney;
            }
        }
        //else
        //{
        //    allMoney = new NetworkDictionary<int, double>();
        //}
    }

    private void Update()
    {
        Debug.Log(allMoney.Values.Count);
    }

    [ServerRpc(RequireOwnership = false)]
    void UpdateMoneyServerRpc(int playerId, double newMoney)
    {
        allMoney[playerId] = newMoney;
    }

    /// just for ease of access
    ///
    public TileHandler cropHandler;

    public PlayState GamesMode;

    public bool flyingVegies = true;
}

/// modes

public class TileInfo {
    public Dictionary<int, Dictionary<string, int>> soldierInfo =
     new Dictionary<int, Dictionary<string, int>> ();
    public int homePlayer = -1;

    public TileInfo(){
        for(int i=0;i<Repository.maxPlayers;i++){
             soldierInfo[i]=new Dictionary<string, int>();
             soldierInfo[i]["num"]=0;
             soldierInfo[i]["health"]=0;
         }
    }
}
public enum PlayState
{
    NormalGame,
    PauseGame,
    SoldierSend
}