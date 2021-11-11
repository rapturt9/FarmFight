using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class CropEffect : MonoBehaviour
{
    // Start is called before the first frame update
    public Sprite[] effectSprites;

    

    Vector3 startPos { get { return TileManager.TM.HexToWorld(tile.hexCoord); } }

    

    public Color flickerColor = Color.white;


    private Color startingColor = Color.clear;


    public Hex Hexcoord { get { return tile.hexCoord; } }


    public TileTemp tile;


    public void init(TileTemp tile)
    {

        if(tile == null)
        {
            Destroy(gameObject);
        }
        

        transform.parent = SpriteRepo.Sprites.transform;

        this.tile = tile;

        transform.position = startPos;

        

        Stop();
    }

    VisualEffect effects { get { return GetComponent<VisualEffect>(); } }
    public void Sparkle()
    {
        if (!sparkling)
        {
            transform.position = startPos;
            sparkling = true;
            rotting = false;
            
            

            effects.SendEvent("Sparkle");
            
        }

    }

    public void Rotting()
    {
        if (!rotting)
        {
            sparkling = false;
            rotting = true;
            
            transform.position = startPos;
            
            

            effects.SendEvent("StartFlies");
        }
        
    }

    public void Stop()
    {
        
        
        sparkling = false;
        rotting = false;
        

        effects.SendEvent("StopFlies");
    }

    public bool sparkling, rotting;

    

}

