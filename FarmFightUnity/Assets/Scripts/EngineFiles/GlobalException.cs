using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// stops the game destroying an object when loading
/// </summary>
public class GlobalException : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    
}
