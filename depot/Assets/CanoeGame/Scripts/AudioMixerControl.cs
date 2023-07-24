using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Audio;

public class AudioMixerControl : MonoBehaviour
{
    [Tooltip("0-MASTER, 1-Amb, 2-Dialogue, 3-Music, 4-Sfx, 5-Ui, 6-UiMusic, 7-GlobalMuffle, 8-AllUi, 9-AllGame")]
    public AudioMixer Mixer;

    public void SetVolume(int Group, float volume)
    {
        if (Group == 0)
        {
            Mixer.SetFloat("Volume_MASTER", VolumeFinder(volume));
        }
        else if (Group == 1)
        {
            Mixer.SetFloat("Volume_Amb", VolumeFinder(volume));
        }
        else if (Group == 2)
        {
            Mixer.SetFloat("Volume_Dialogue", VolumeFinder(volume));
        }
        else if (Group == 3)
        {
            Mixer.SetFloat("Volume_Music", VolumeFinder(volume));
        }
        else if (Group == 4)
        {
            Mixer.SetFloat("Volume_Sfx", VolumeFinder(volume));
        }
        else if (Group == 5)
        {
            Mixer.SetFloat("Volume_Ui", VolumeFinder(volume));
        }
        else if (Group == 6)
        {
            Mixer.SetFloat("Volume_UiMusic", VolumeFinder(volume));
        }
        else if (Group == 7) //Underwater muffling use only
        {
            Mixer.SetFloat("LowPass_MASTER", volume * 22000);
        }
        else if (Group == 8) // All UI Audio
        {
            Mixer.SetFloat("Volume_AllUi", VolumeFinder(volume));
        }
        else if (Group == 9) // All Game Audio
        {
            Mixer.SetFloat("Volume_AllGame", VolumeFinder(volume));
        }
    }

    private float VolumeFinder(float Input)
    {
        return Mathf.Log10(Input) * 20;
    }
}
