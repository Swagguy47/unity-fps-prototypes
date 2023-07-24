using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaddleForwarder : MonoBehaviour
{
    [SerializeField] BoatController Boat;

    public void RightPaddleIn()
    {
        Boat.RightPaddleIn();
    }

    public void RightPaddleOut()
    {
        Boat.RightPaddleOut();
    }

    public void LeftPaddleIn()
    {
        Boat.LeftPaddleIn();
    }

    public void LeftPaddleOut()
    {
        Boat.LeftPaddleOut();
    }
}
