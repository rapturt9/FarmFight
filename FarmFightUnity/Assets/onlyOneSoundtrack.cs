using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class onlyOneSoundtrack : MonoBehaviour
{
    // Start is called before the first frame update
    private void Awake()
    {
        if(GameObject.Find("BackgroundMusic") != null)
        {
            Destroy(gameObject);
        }
    }

   
}
