using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadingAnimFwd : MonoBehaviour
{
    public void Faded()
    {
        PlayerCallback.PlayerBrain.Faded = true;
    }

    public void Started()
    {
        PlayerCallback.PlayerBrain.Faded = false;
    }
}
