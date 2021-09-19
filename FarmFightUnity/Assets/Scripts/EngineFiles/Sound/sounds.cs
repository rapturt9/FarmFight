using System;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "NewSound", menuName = "CreateNewSound", order = 2)]
public class Sound: ScriptableObject
{
    public AudioClip audio;

    public string SoundName;

    public AudioSource CreateSoundInstance(GameObject soundFolder, float volume = .5f,
                                        bool loop = false, bool mute = false)
    {
        GameObject obj = new GameObject();

        obj.transform.parent = soundFolder.transform;

        AudioSource audioSource = obj.AddComponent<AudioSource>();

        audioSource.clip = audio;

        audioSource.volume = volume;

        audioSource.loop = loop;

        audioSource.mute = mute;

        
        return audioSource;
    }

    

    
    
}
