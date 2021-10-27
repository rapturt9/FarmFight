using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;

public class BattleCloud : NetworkBehaviour
{
    public TileHandler handler;

    const float tint = 0.2f;
    private Hex hexCoord;
    private NetworkVariable<Color> color = new NetworkVariable<Color>();


    private void Start()
    {
        handler = TileManager.TM["Crops"];
        StartCoroutine("RefreshColor");
    }

    // Add to all client tiles
    public void AddToTile(Hex coord)
    {
        if (IsClient)
        {
            AddToTileServerRpc(BoardHelperFns.HexToArray(coord));
        }
        else if (IsServer)
        {
            AddToTileClientRpc(BoardHelperFns.HexToArray(coord));
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void AddToTileServerRpc(int[] coord)
    {
        AddToTileClientRpc(coord);
    }

    [ClientRpc]
    void AddToTileClientRpc(int[] coord)
    {
        _AddToTile(coord);
    }

    // Internal function, actually changes the tile
    void _AddToTile(int[] coordArray)
    {
        Hex coord = BoardHelperFns.ArrayToHex(coordArray);
        handler[coord].battleCloud = gameObject;
        hexCoord = coord;
    }

    void Update()
    {
        
    }

    // Gets the percentage each team has of soldier health
    float[] getSoldierHealthFractions()
    {
        float[] soldierHealths = new float[Repository.maxPlayers];
        float[] soldierHealthFractions = new float[Repository.maxPlayers];
        float totalHealth = 0;

        // Get combined health for each time
        foreach (var soldier in handler[hexCoord].getSoldierEnumerator())
        {
            soldierHealths[soldier.owner.Value] += soldier.Health.Value;
            totalHealth += soldier.Health.Value;
        }
        // Rescale
        for (int playerId = 0; playerId<Repository.maxPlayers; playerId++)
        {
            soldierHealthFractions[playerId] = soldierHealths[playerId] / totalHealth;
        }
        return soldierHealthFractions;
    }

    Color getColor()
    {
        Color newColor = new Color();
        float[] fractions = getSoldierHealthFractions();
        for (int playerId = 0; playerId<Repository.maxPlayers; playerId++)
        {
            newColor += OutlineSetter.OS.TeamColors[playerId] * fractions[playerId];
        }
        // Make it look a bit more pastel
        newColor += Color.white * tint;
        return newColor;
    }

    private IEnumerator RefreshColor()
    {
        while (true)
        {
            GetComponent<SpriteRenderer>().color = getColor();
            yield return new WaitForSeconds(.1f);
        }
    }
}
