using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Radio : MonoBehaviour
{
    [SerializeField] AudioClip[] Tracklist;
    [SerializeField] AudioClip[] EnableDisable;
    [SerializeField] AudioSource Src;

    private void LateUpdate()
    {
        if (!Src.isPlaying)
        {
            Src.clip = Tracklist[Random.Range(0, Tracklist.Length)];
            Src.Play();
        }
    }

    private void OnEnable()
    {
        Src.volume = 1;
        Src.PlayOneShot(EnableDisable[0]);
    }

    public void OnDisable()
    {
        PlayerCallback.PlayerBrain.UIOneShotSrc.PlayOneShot(EnableDisable[1]);
        Src.volume = 0;
    }
}
