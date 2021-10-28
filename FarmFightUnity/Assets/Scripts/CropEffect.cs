using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CropEffect : MonoBehaviour
{
    // Start is called before the first frame update
    public Sprite[] effectSprites;
    private GameObject SpriteFolder;

    Vector3 startPos;

    public SpriteRenderer spriteRenderer = null;
    public Color flickerColor = Color.white;
    private bool effectActive = false;

    private Color startingColor = Color.clear;
    public void init(Hex start, string type)
    {
        if (effectActive) { return; }

        spriteRenderer = GetComponent<SpriteRenderer>();
        
        transform.parent = SpriteRepo.Sprites.transform;

        startPos = TileManager.TM.HexToWorld(start);

        transform.position = startPos;

        if (type == "sparkle"){
            spriteRenderer.sprite = effectSprites[0];
            effectActive = true;
            //StartCoroutine("Sparkle");
        }else if(type=="rot"){
            spriteRenderer.sprite = effectSprites[1];
            StartCoroutine("SpinObject");
            effectActive = true;
        }
    }

    private IEnumerator Sparkle()
    {
        spriteRenderer.color = flickerColor;
 
        yield return new WaitForSeconds(0.05f);
 
        spriteRenderer.color = startingColor;
    }

    private IEnumerator SpinObject () {
 
     float duration = 30f;
     float elapsed = 0f;
     float spinSpeed = 100f;
     while (elapsed < duration)
     {
         elapsed += Time.deltaTime;
         transform.Rotate(Vector3.forward*1/2, spinSpeed * Time.deltaTime);
         yield return new WaitForEndOfFrame();
     }
     yield return null;
 }

}

