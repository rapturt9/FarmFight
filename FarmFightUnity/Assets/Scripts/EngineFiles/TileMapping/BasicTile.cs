using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicTile : TileTemp
{

    TileArt offHand;

    public override void LoadArt()
    {
        currentArt = TileArtRepository.Art["Test"];
        offHand = TileArtRepository.Art["Select"];
    }

    public override void Behavior()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if(TileManager.TM.getMouseHex() == hexCoord)
            {
                TileArt hold = currentArt;
                currentArt = offHand;
                offHand = hold;
            }
            
        }
    }
}
