using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class CinematicVideoManager : MonoBehaviour
{
    public VideoPlayer Player;

    float Wait= -1;
    [HideInInspector]public bool Playing;
    private void Update()
    {
        if (Wait >= 0)
        {
            Playing= true;
            Wait -= Time.deltaTime;
            Player.enabled= true;
            PlayerCallback.PlayerBrain.Fading = true;
            //PlayerCallback.PlayerBrain.Faded = true;
            PlayerCallback.PlayerBrain.FadeInOut.enabled = false;
            //PlayerCallback.PlayerBrain.FadeInOut.color = new Color(0, 0, 0, 1);
        }
        else if (Playing)
        {
            Playing=false;
            Player.Stop();
            Player.enabled = false;
            Wait = -1;
            PlayerCallback.AudioMix.SetVolume(9, 1);
            PlayerCallback.PlayerBrain.Fading = false;
            PlayerCallback.PlayerBrain.FadeInOut.enabled = true;
        }
    }
    public void PlayCinematic(VideoClip Cinematic)
    {
        PlayerCallback.AudioMix.SetVolume(9, 0.0001f);
        Player.enabled= true;
        Player.clip = Cinematic;
        Player.Play();
        Wait = ((float)Player.length);
    }
}
