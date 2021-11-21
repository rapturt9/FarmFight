using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteSwitch : MonoBehaviour
{
    public Sprite[] sprites;

    public int owner = -1;

    private Material material;
    public float fadespeed;
    private void Awake()
    {
        owner = -1;

        GetComponent<SpriteRenderer>().enabled = false;

        StartCoroutine("change");

        material = GetComponent<SpriteRenderer>().material;
    }

    private bool OwnerIsSet()
    {
        Soldier soldier;
        if (TryGetComponent(out soldier))
        {
            owner = soldier.owner.Value;
            
        }

        else
            owner = GetComponent<Farmer>().Owner.Value;


        return owner != -1;
    }

    

    private IEnumerator change()
    {
        while (true)
        {
            if (OwnerIsSet())
            {
                break;
            }
            yield return null;
        }


        

        GetComponent<SpriteRenderer>().sprite = sprites[owner];

        GetComponent<SpriteRenderer>().enabled = true;

        StartCoroutine(_fadeIn());

    }


    private IEnumerator _fadeIn()
    {
        material.SetFloat("fadeAmount", .3f);
        while (material.GetFloat("fadeAmount") < 1)
        {
            material.SetFloat("fadeAmount", material.GetFloat("fadeAmount") + fadespeed);
            yield return null;
        }
    }

    private IEnumerator _fadeOut()
    {
        material.SetFloat("fadeAmount", .3f);
        while (material.GetFloat("fadeAmount") < 1)
        {
            material.SetFloat("fadeAmount", material.GetFloat("fadeAmount") + fadespeed);
            yield return null;
        }
    }

    public void FadeIn()
    {
        StopCoroutine(_fadeOut());
        StopCoroutine(_fadeIn());
    }

    public void FadeOut()
    {
        StopCoroutine(_fadeIn());
        StopCoroutine(_fadeOut());
    }
}


 
