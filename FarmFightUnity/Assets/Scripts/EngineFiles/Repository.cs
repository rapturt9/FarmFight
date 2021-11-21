﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Repository : MonoBehaviour
{
    ///Place Variables here

    [SerializeField]
    public Hex selectedHex = Hex.zero;

    public int localPlayerId;
    public bool gameIsRunning = false;
    public double money = 10.0;

    public TileInfo tileinfo = new TileInfo();

    public const int maxPlayers = 6;

    public Color[] TeamColors { get { return OutlineSetter.OS.TeamColors; } }

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

    /// just for ease of access
    ///
    public TileHandler cropHandler
    {
        get
        {
            return TileManager.TM["Crops"];
        }
    }

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