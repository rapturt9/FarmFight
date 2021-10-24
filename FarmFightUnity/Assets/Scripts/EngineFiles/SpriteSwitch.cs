using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteSwitch : MonoBehaviour
{
    public Sprite[] sprites;

    public int owner = -1;

    private void Awake()
    {
        owner = -1;

        GetComponent<SpriteRenderer>().enabled = false;

        StartCoroutine("change");
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

        //Destroy(this);

    }

}


 
