using System;
using UnityEngine;
using System.Collections;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Messaging;

public class Soldier: NetworkBehaviour
{
    public NetworkVariable<int> Health = new NetworkVariable<int>(100);
    public NetworkVariable<int> owner = new NetworkVariable<int>(-1);

    public TileHandler handler;

    public float travelSpeed = .005f;
    
    public float fadesSpeed = .01f;

    public void Start()
    {
        //GetComponent<SpriteRenderer>().color = Color.clear;
        handler = TileManager.TM["Crops"];
    }
    
    public IEnumerator FadeInCoroutine()
    {
        GetComponent<SpriteRenderer>().enabled = true;

        while (GetComponent<SpriteRenderer>().color.a < 1)
        {
            GetComponent<SpriteRenderer>().color += (Color.black * fadesSpeed);
            
            yield return null;
        }
    }

    private IEnumerator FadeOutCoroutine()
    {
        while (GetComponent<SpriteRenderer>().color.a > 0)
        {
            GetComponent<SpriteRenderer>().color -= (Color.black * fadesSpeed);


            yield return null;
        }

        GetComponent<SpriteRenderer>().enabled = false;

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
        if (!handler[coord].soldiers.Contains(this))
            handler[coord].soldiers.Add(this);
    }

    public void Update()
    {
        
    }

    public void FadeIn()
    {
        StopCoroutine("FadeOutCoroutine");
        StartCoroutine("FadeInCoroutine");
    }

    public void FadeOut()
    {
        StopCoroutine("FadeInCoroutine");
        StartCoroutine("FadeOutCoroutine");
    }

}
