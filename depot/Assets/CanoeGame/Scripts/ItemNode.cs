using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ItemNode : MonoBehaviour
{
    public UnityEvent NodeTrigger;

    public void Interact()
    {
        NodeTrigger.Invoke();
    }
}
