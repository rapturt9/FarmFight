using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class CropEffect : MonoBehaviour
{
    // Start is called before the first frame update
    public Sprite[] effectSprites;

    

    Vector3 startPos { get { return TileManager.TM.HexToWorld(tile.hexCoord); } }

    public SpriteRenderer spriteRenderer = null;

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
        spriteRenderer = GetComponent<SpriteRenderer>();

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
            StopCoroutine(_SpinObject());
            spriteRenderer.sprite = effectSprites[0];

            effects.SendEvent("Sparkle");
            
        }

    }

    public void Rotting()
    {
        if (!rotting)
        {
            sparkling = false;
            rotting = true;
            StopCoroutine(_Sparkle());
            transform.position = startPos;
            spriteRenderer.sprite = effectSprites[1];
            StartCoroutine(_SpinObject());

            effects.SendEvent("StartFlies");
        }
        
    }

    public void Stop()
    {
        
        StopCoroutine(_SpinObject());
        sparkling = false;
        rotting = false;
        spriteRenderer.sprite = null;

        effects.SendEvent("StopFlies");
    }

    public bool sparkling, rotting;

    private IEnumerator _Sparkle()
    {
        spriteRenderer.color = Color.white;
 
        yield return new WaitForSeconds(0.05f);
 
        spriteRenderer.color = startingColor;
    }

    private IEnumerator _SpinObject ()
    {
 
         //float duration = 2000f;
         float elapsed = 0f;
         float spinSpeed = Mathf.PI/20;
        float Dx = Random.Range(.8f, 1.2f), Dy = Random.Range(.8f, 1.2f);
        int Direction = -1;
        if(Random.Range(0,2) == 1)
        {
            Direction = 1;
        }
        
         
         while (rotting)
         {
            elapsed += 1;

            transform.position = startPos
                    + Vector3.right * Dx * .125f * Mathf.Cos(Direction * spinSpeed * elapsed)
                    + Vector3.up * Dy * .125f * Mathf.Sin(Direction * spinSpeed * elapsed)
                    + Vector3.up * .2f;
             //transform.Rotate(Vector3.forward, spinSpeed * Time.deltaTime);
             yield return null;
         }


        transform.position = startPos;


    }

}

