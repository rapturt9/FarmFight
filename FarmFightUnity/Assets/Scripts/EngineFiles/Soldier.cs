using System;
using UnityEngine;
using System.Collections;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Messaging;
using MLAPI.Prototyping;
using System.Collections.Generic;

public class Soldier: NetworkBehaviour
{
    public NetworkVariable<float> Health = new NetworkVariable<float>(100);
    public NetworkVariable<int> owner = new NetworkVariable<int>(-1);

    public TileHandler handler;

    public float travelSpeed = 0.1f;
    
    public float fadesSpeed = .01f;

    public Hex Position;

    void Awake()
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

    // Remove from all client tiles
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
    public void _RemoveFromTile(int[] coordArray)
    {
        Hex coord = BoardHelperFns.ArrayToHex(coordArray);
        if (handler[coord].SortedSoldiers[owner.Value].Contains(this))
        {
            handler[coord].SortedSoldiers[owner.Value].Remove(this);
        }
    }

    // Add to all client tiles
    public void AddToTile(Hex coord, bool wasMoving = false)
    {
        if (IsClient)
        {
            AddToTileServerRpc(BoardHelperFns.HexToArray(coord), wasMoving);
        }
        else if (IsServer)
        {
            AddToTileClientRpc(BoardHelperFns.HexToArray(coord), wasMoving);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void AddToTileServerRpc(int[] coord, bool wasMoving = false)
    {
        AddToTileClientRpc(coord, wasMoving);
    }

    [ClientRpc]
    void AddToTileClientRpc(int[] coord, bool wasMoving = false)
    {
        _AddToTile(coord, wasMoving);
    }

    // Internal function, actually changes the tile
    void _AddToTile(int[] coordArray, bool wasMoving = false)
    {
        Hex coord = BoardHelperFns.ArrayToHex(coordArray);
        List<Soldier> fellowSoldiers = handler[coord].SortedSoldiers[owner.Value];
        
        // Adds to SortedSoldiers
        if (!fellowSoldiers.Contains(this))
        {
            fellowSoldiers.Add(this);
            if (wasMoving)
            {
                BoardChecker.Checker.movingSoldierCount[owner.Value]--;
            }
        }
        // Fades out if battling
        if (handler[coord].battleOccurring)
        {
            FadeOut();
        }
        // Changes position
        transform.position = TileManager.TM.HexToWorld(coord) + .25f * Vector3.left;
    }

    // Starting trip as a client, for smoothness
    [ClientRpc]
    public void StartTripAsClientRpc(int[] startArray, int[] endArray)
    {
        Hex start = BoardHelperFns.ArrayToHex(startArray);
        Hex end = BoardHelperFns.ArrayToHex(endArray);

        // Removing from tile if the rpc hasn't been processed
        if (!IsServer)
        {
            BoardChecker.Checker.movingSoldierCount[owner.Value]++;
        }
        _RemoveFromTile(startArray);

        // Fading
        if (!handler[start].battleOccurring)
        {
            Soldier newTopSoldier;
            if ((newTopSoldier = handler[start].FindFirstSoldierWithID(owner.Value)) != null)
            {
                newTopSoldier.FadeIn();
            }
        }

        // Trip smoothness, only on client
        if (IsServer)
        {
            return;
        }
        FadeIn();
        // Starts trip
        SoldierTrip trip;
        if (!TryGetComponent(out trip))
        {
            trip = gameObject.AddComponent<SoldierTrip>();
        }
        // We don't want the server overriding position ever again
        GetComponent<NetworkTransform>().enabled = false;
        trip.init(start, end);
    }

    // Ending trip as client, for smoothness
    [ClientRpc]
    public void EndTripAsClientRpc(bool fade)
    {
        if (fade)
            FadeOut();
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
        if (gameObject != null)
            Destroy(gameObject);
    }

}
