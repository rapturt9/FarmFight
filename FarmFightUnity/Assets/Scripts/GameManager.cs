using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{


    public TileHandler[] TileHandler;
    Repository central;

    // Start is called before the first frame update
    private void Awake()
    {

    }

    private void Start()
    {
        TileArtRepository.Art.Init();
        TileManager.TM.Init();
        central = Repository.Central;
    }

    // Update is called once per frame
    void Update()
    {
        Hex hex = TileManager.TM.getMouseHex();

        if (Input.GetMouseButtonDown(0) &
                TileManager.TM.isValidHex(hex))
        {
            Repository.Central.selectedHex = hex;
        }

        //Debug.Log(central.selectedHex);
    }
}
