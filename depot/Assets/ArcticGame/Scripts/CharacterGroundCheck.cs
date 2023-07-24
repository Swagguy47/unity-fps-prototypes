using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterGroundCheck : MonoBehaviour
{
    CharacterBrain brain;
    int EnvCount;

    private void Start()
    {
        brain = transform.parent.GetComponent<CharacterBrain>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 0)
        {
            EnvCount++;
            UpdateGrounded();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 0)
        {
            EnvCount--;
            UpdateGrounded();
        }
    }

    private void UpdateGrounded()
    {
        if (EnvCount > 0)
        {
            brain.Grounded = true;
        }
        else
        {
            brain.Grounded = false;
        }
    }
}
