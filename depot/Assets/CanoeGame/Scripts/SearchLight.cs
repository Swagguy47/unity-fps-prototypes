using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchLight : MonoBehaviour
{
    [SerializeField] SearchLightMaster Master;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            RaycastHit hit;
            //new Vector3(transform.position.x, transform.position.y + 0.15f, transform.position.z)
            if (Physics.Linecast(transform.position, other.transform.position, out hit))
            {
                if (hit.collider.gameObject.layer == 6)
                {
                    Master.tracking = hit.collider.transform;
                    Debug.Log("We see you >:)");
                }
            }
        }
    }
}
