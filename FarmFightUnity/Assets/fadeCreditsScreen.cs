using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fadeCreditsScreen : MonoBehaviour
{
    public CanvasGroup canvas;
    public float fadeduration;

    private bool fadedIn;

    public void Start()
    {
        canvas.gameObject.SetActive(false);
        FadeOut();
    }

    private void Update()
    {
        if (Input.GetMouseButton(0) & canvas.gameObject.activeInHierarchy)
        {
            FadeOut();
        }
    }
    private IEnumerator fadeIn()
    {
        while(canvas.alpha < 1 & fadedIn )
        {
            canvas.alpha += 1 / fadeduration;
            yield return null;
        }
    }

    
    private IEnumerator fadeOut()
    {

        while (canvas.alpha > 0 & !fadedIn )
        {
            canvas.alpha -= 1 / fadeduration;

            yield return null;
        }
        if(!fadedIn)
            canvas.gameObject.SetActive(false);
    }

    public void FadeIn()
    {
        fadedIn = true;
        canvas.gameObject.SetActive(true);
        StopCoroutine(fadeOut());
        StartCoroutine(fadeIn());

    }
    public void FadeOut()
    {
        fadedIn = false;
        StopCoroutine(fadeIn());
        StartCoroutine(fadeOut());
        
    }
}

