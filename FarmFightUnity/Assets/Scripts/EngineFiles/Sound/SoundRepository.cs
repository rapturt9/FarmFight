using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundRepository : MonoBehaviour
{
    public GameObject SoundFolder;

    [SerializeField]
    Sound[] Sounds;

    Dictionary<string, Sound> sounds;

    public Sound this[string name]
    {
        get
        {
            return sounds[name];
        }
    }

    
    
}
