using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class onlyOneSoundtrack : MonoBehaviour
{
    bool muted = false;
    AudioSource music;

    public static onlyOneSoundtrack track;
    private void Awake()
    {
        if(track != null)
        {
            Destroy(gameObject);
        }
        else
        {
            track = this;
        }
    }

    private void Start()
    {
        music = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // Mute with m
        if (Input.GetKeyDown("m"))
        {
            muted = !muted;
            if (muted)
            {
                music.Pause();
            }
            else
            {
                music.Play();
            }
        }
    }


}
