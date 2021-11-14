using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTile : MonoBehaviour
{
    public Sprite[] sprites;
    public SpriteRenderer spriteRenderer = null;
    Vector3 startPos { get { return TileManager.TM.HexToWorld(tile.hexCoord); } }

    public TileTemp tile;  
    public void init(TileTemp tile)
    {

        //GetComponent<SpriteRenderer>().enabled = false;
        if(tile == null)
        {
            Destroy(gameObject);
        }
        //StartCoroutine("change");
        spriteRenderer = GetComponent<SpriteRenderer>();
    
        transform.parent = SpriteRepo.Sprites.transform;

        this.tile = tile;

        transform.position = startPos;
        spriteRenderer.sprite = null;
    }

    int fading = 0;
    float aTime = 10.0f;
    float aValue = 0.0f;



    public void FadeIn() {
        
        if(fading == 0 && spriteRenderer.sprite == null){
            transform.position = startPos;
            spriteRenderer.sprite = sprites[0];
            Color newColor = new Color(1, 1, 1, 0);
            transform.GetComponent<Renderer>().material.color = newColor;
            fading = 1;
        }
        aValue = 1.0f;
        StopCoroutine("FadeTo");
        StartCoroutine("FadeTo");
        /*if(fading == 1){
            StartCoroutine("FadeTo");
            fading = 2;
        } else {
            StopCoroutine("FadeTo");
            StartCoroutine("FadeTo");
        }*/
    }


    public void FadeOut() {
    
        if(fading == 0 && spriteRenderer.sprite == null){
            transform.position = startPos;
            spriteRenderer.sprite = sprites[0];
            Color newColor = new Color(1, 1, 1, 0);
            transform.GetComponent<Renderer>().material.color = newColor;
            fading = 1;
        }
        aValue = 0.0f;
        StopCoroutine("FadeTo");
        StartCoroutine("FadeTo");
        /*if(fading == 1){
            StartCoroutine("FadeTo");
            fading = 2;
        } else {
            StopCoroutine("FadeTo");
            StartCoroutine("FadeTo");
        }*/
    }


    IEnumerator FadeTo()
 {
     float alpha = transform.GetComponent<Renderer>().material.color.a;
     for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / aTime)
     {
         Color newColor = new Color(1, 1, 1, Mathf.Lerp(alpha,aValue,t));
         transform.GetComponent<Renderer>().material.color = newColor;
         yield return null;
     }
 }
}
