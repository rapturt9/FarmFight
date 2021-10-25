using System;
using UnityEngine;
using System.Collections;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Messaging;

public class Soldier: NetworkBehaviour
{
    public NetworkVariable<float> Health = new NetworkVariable<float>(100);
    public NetworkVariable<int> owner = new NetworkVariable<int>(-1);

    public TileHandler handler;

    public float travelSpeed = 0.1f;
    
    public float fadesSpeed = .01f;

    public Hex Position;

    public void Start()
    {
        
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
    public void RemoveFromTile(Hex coord)
    {
        if (IsClient)
        {
            RemoveFromTileServerRpc(BoardHelperFns.HexToArray(coord));
        }
        else if (IsServer)
        {
            RemoveFromTileClientRpc(BoardHelperFns.HexToArray(coord));
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void RemoveFromTileServerRpc(int[] coord)
    {
        RemoveFromTileClientRpc(coord);
    }

    [ClientRpc]
    void RemoveFromTileClientRpc(int[] coord)
    {
        _RemoveFromTile(coord);
    }

    // Internal function, actually changes the tile
    void _RemoveFromTile(int[] coordArray)
    {
        Hex coord = BoardHelperFns.ArrayToHex(coordArray);
        if (handler[coord].SortedSoldiers[owner.Value].Contains(this))
            handler[coord].SortedSoldiers[owner.Value].Remove(this);
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
        if (!handler[coord].SortedSoldiers[owner.Value].Contains(this))
            handler[coord].SortedSoldiers[owner.Value].Add(this);
        transform.position = TileManager.TM.HexToWorld(coord) + .25f * Vector3.left;
        handler[coord].BattleFunctionality();
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

    public void Kill()
    {
        //RemoveFromTile(Position);
        Destroy(gameObject);
    }

}
