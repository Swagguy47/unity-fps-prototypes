using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCallback : MonoBehaviour
{
    [HideInInspector] public static PlayerBrain PlayerBrain;

    private void Start()
    {
        PlayerBrain = GetComponent<PlayerBrain>();
    }
}
