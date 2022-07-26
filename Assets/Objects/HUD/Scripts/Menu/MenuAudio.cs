using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuAudio : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] audioClips;

    public void OnClick(string _soundType)
    {
        if (_soundType == "Accept") 
            audioSource.clip = audioClips[0];
        else if (_soundType == "Back")
            audioSource.clip = audioClips[1];

        audioSource.Play();
    }

}
