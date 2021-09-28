using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using MLAPI;

public class GameManager : NetworkBehaviour
{
    public TileHandler[] TileHandler;
    public GameState gameState;
    Repository central;

    // Start is called before the first frame update
    private void Awake()
    {
        
    }

    public override void NetworkStart()
    {
        TileArtRepository.Art.Init();
        TileManager.TM.Init();
        gameState.Init();
        central = Repository.Central;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer && !IsClient) { return; }

        Hex hex = TileManager.TM.getMouseHex();

        if (Input.GetMouseButtonDown(0) &
                TileManager.TM.isValidHex(hex))
        {
            Repository.Central.selectedHex = hex;
        }

        //Debug.Log(central.selectedHex);
    }
}
