using MLAPI;
using MLAPI.Messaging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class CropEffect : NetworkBehaviour
{
    // Start is called before the first frame update
    public Sprite[] effectSprites;

    Vector3 startPos { get { return TileManager.TM.HexToWorld(tile.hexCoord); } }

    public Color flickerColor = Color.white;

    public Hex Hexcoord { get { return tile.hexCoord; } }

    public AudioSource Harvest, Plant, Capture, Battle, TimeToHarvest, Flies;

    public TileTemp tile;

    public bool sparkling, rotting;

    public float crackAmount;

    public SpriteRenderer crackRender;
    public VisualEffect visual;


    public void LateUpdate()
    {
        BattleCloud();

        if (tile == null || tile.effect != this || Repository.Central.cropHandler[tile.hexCoord] != tile)
        {
            Destroy(gameObject);
        }

        crackAmount = tile.tileDamage/10f;

        transform.position = startPos;

        crackRender.transform.position = startPos;

        crackRender.material.SetFloat("CrackAmt", crackAmount);
    }


    public void init(TileTemp tile)
    {

        
        
        transform.parent = SpriteRepo.Sprites.transform;

        


        this.tile = tile;

        transform.position = startPos;

        effects.SetVector4("CloudColor", Color.white);

        Stop();
    }

    VisualEffect effects { get { return visual; } }
    public void Sparkle()
    {
        if (!sparkling && !tile.battleOccurring)
        {
            TimeToHarvest.Play();
            Flies.Stop();
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

            Flies.Play();

        }
        if (rotting)
        {
            
        }
            
        
    }

    public void HarvestSound()
    {
        Harvest.Play();
    }

    public void Pause()
    {
        effects.SendEvent("StopFlies");
        effects.SetBool("FliesAlive", false);
    }

    public void Resume()
    {
        if (rotting)
        {
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
        Flies.Stop();
        
    }


    public bool CloudActive;
    public Color CloudColor { set { effects.SetVector4("CloudColor", value); } }
    public float CloudSpeed = 0.01f;
    public float cloudsize;
    private void BattleCloud()
    {
        transform.position = startPos;

        float size = effects.GetFloat("CloudSize");

        //CloudActive = tile.battleOccurring;

        

        if (CloudActive && size < 1)
        {

            Color color = getCloudColor();
            
            effects.SetVector4("CloudColor", color);

            effects.SetFloat("CloudSize", size + CloudSpeed);
        }
        else if(!CloudActive && size > 0)
        {
            effects.SetVector4("CloudColor", getCloudColor());
            effects.SetFloat("CloudSize", size - CloudSpeed);
        }
        else
        {
            //effects.SetVector4("CloudColor", Color.white);
        }
    }

    float[] getSoldierHealthFractions()
    {
        float[] soldierHealths = new float[Repository.maxPlayers];
        float[] soldierHealthFractions = new float[Repository.maxPlayers];
        float totalHealth = 0;

        // Get combined health for each time
        foreach (var soldier in tile.getSoldierEnumerator())
        {
            soldierHealths[soldier.owner.Value] += soldier.Health.Value;
            totalHealth += soldier.Health.Value;
        }
        // Rescale
        for (int playerId = 0; playerId < Repository.maxPlayers; playerId++)
        {
            soldierHealthFractions[playerId] = soldierHealths[playerId] / totalHealth;
        }
        return soldierHealthFractions;
    }

    Color getCloudColor()
    {
        Color newColor = new Color();
        float[] fractions = getSoldierHealthFractions();
        for (int playerId = 0; playerId < Repository.maxPlayers; playerId++)
        {
            if (fractions[playerId] == float.NaN)
                return Color.white;

            newColor += OutlineSetter.OS.TeamColors[playerId] * fractions[playerId];
        }
        // Make it look a bit more pastel
        newColor += Color.white * 0.2f;
        return newColor;
    }

    public void StartCapture(float time, Color color)
    {
        transform.position = startPos;
        effects.SetBool("Capturing", true);

        effects.SendEvent("Capture");
        
        effects.SetVector4("CaptureColor", color);
        effects.SetFloat("CaptureTime", time);
        effects.SendEvent("Capture");
        
    }
    public void StopCapture()
    {
        effects.SetBool("Capturing", false);
    }

    public void Capturing()
    {
        Capture.Play();
    }


    private void UpdatePosition()
    {
        
    }

}

