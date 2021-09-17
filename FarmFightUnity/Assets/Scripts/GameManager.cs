using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    

    public TileHandler[] TileHandler;

    // Start is called before the first frame update
    private void Awake()
    {
        
    }

    private void Start()
    {
        TileArtRepository.Art.Init();


        MapManager.MM.Init();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
