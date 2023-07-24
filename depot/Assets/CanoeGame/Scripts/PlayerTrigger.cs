using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerTrigger : MonoBehaviour
{
    [SerializeField] private UnityEvent OnEnter;
    [SerializeField] bool DestroyOnEnter;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            OnEnter.Invoke();
            if (DestroyOnEnter)
            {
                Destroy(this.gameObject);
            }
        }
    }
}
