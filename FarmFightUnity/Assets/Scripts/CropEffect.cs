using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


    public void Sparkle()
    {
        if (!sparkling)
        {
            transform.position = startPos;
            sparkling = true;
            rotting = false;
            StopCoroutine(_SpinObject());
            spriteRenderer.sprite = effectSprites[0];
            
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
        }
        
    }

    public void Stop()
    {
        
        StopCoroutine(_SpinObject());
        sparkling = false;
        rotting = false;
        spriteRenderer.sprite = null;
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
         
         while (rotting)
         {
            elapsed += 1;

            transform.position = startPos
                    + Vector3.right * .125f * Mathf.Cos(spinSpeed * elapsed)
                    + Vector3.up * .125f * Mathf.Sin(spinSpeed * elapsed)
                    + Vector3.up * .2f;
             //transform.Rotate(Vector3.forward, spinSpeed * Time.deltaTime);
             yield return null;
         }


        transform.position = startPos;


    }

}

