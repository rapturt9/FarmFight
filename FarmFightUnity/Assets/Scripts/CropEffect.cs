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

    public Hex Hexcoord { get { return tile.hexCoord; } }


    public TileTemp tile;

    public bool sparkling, rotting;


    public void LateUpdate()
    {
        BattleCloud();
    }


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

            effects.SendEvent("StopFlies");

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

            effects.SetBool("FliesAlive", true);

            effects.SendEvent("StartFlies");
        }
        
    }

    public void Stop()
    {
        
        
        sparkling = false;
        rotting = false;
        

        effects.SendEvent("StopFlies");
        effects.SetBool("FliesAlive",false);
    }


    public bool CloudActive;
    public Color CloudColor { set { effects.SetVector4("CloudColor", value); } }
    public float CloudSpeed = 0.01f;

    private void BattleCloud()
    {
        var size = effects.GetFloat("CloudSize");

        if (CloudActive & size < 1)
        {
            effects.SetFloat("CloudSize", size + CloudSpeed);
        }
        else if(!CloudActive & size > 0)
        {
            effects.SetFloat("CloudSize", size - CloudSpeed);
        }
    }

    public void StartCapture(float time,Color color)
    {
        effects.SetVector4("CaptureColor", color);
        effects.SetFloat("CaptureTime", time);
        effects.SendEvent("Capture");
    }
    public void StopCapture()
    {

    }


}

