using UnityEngine;
using System.Collections;

public class Repository : MonoBehaviour
{
    ///Place Variables here

    [SerializeField]
    public Hex selectedHex = Hex.zero;
    

    public int localPlayerId;
    public bool gameIsRunning = false;
    public double money = 100.0;

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



    /// just for ease of access
    ///
    public TileHandler cropHandler;



    public PlayState GamesMode;

    public bool flyingVegies = true;
}

/// modes
public enum PlayState
{
    NormalGame,
    PauseGame,
    SoldierSend


}