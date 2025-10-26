using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAudio : MonoBehaviour
{
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (!audioSource.isPlaying)
            gameObject.SetActive(false);
    }
}

