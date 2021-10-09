using System;
using UnityEngine;
using System.Collections;
public class Soldier: MonoBehaviour
{
    public int Health = 100;

    public float travelSpeed = .005f;
    
    public float fadesSpeed = .01f;

    public void Start()
    {
        //GetComponent<SpriteRenderer>().color = Color.clear;

        
    }
    
    public IEnumerator FadeInCoroutine()
    {
        GetComponent<SpriteRenderer>().enabled = true;

        while (GetComponent<SpriteRenderer>().color.a < 1)
        {
            GetComponent<SpriteRenderer>().color += (Color.black * fadesSpeed);
            
            yield return null;
        }
    }

    private IEnumerator FadeOutCoroutine()
    {
        while (GetComponent<SpriteRenderer>().color.a > 0)
        {
            GetComponent<SpriteRenderer>().color -= (Color.black * fadesSpeed);


            yield return null;
        }

        GetComponent<SpriteRenderer>().enabled = false;

    }

    public void Update()
    {
        
    }

    public void FadeIn()
    {
        StopCoroutine("FadeOutCoroutine");
        StartCoroutine("FadeInCoroutine");
    }

    public void FadeOut()
    {
        StopCoroutine("FadeInCoroutine");
        StartCoroutine("FadeOutCoroutine");
    }

}
