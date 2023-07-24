using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchLightMaster : MonoBehaviour
{
    Animator Anim;
    [HideInInspector] public Transform tracking;
    float WaitTime = 10;
    [SerializeField] Transform[] Searchlights;

    private void Start()
    {
        Anim = gameObject.GetComponent<Animator>();
    }

    private void LateUpdate()
    {
        if (tracking != null)
        {
            Anim.SetBool("Track", true);
            foreach (Transform Light in Searchlights)
            {
                RaycastHit hit;
                if (Physics.Linecast(Light.position, tracking.transform.position, out hit))
                {
                    if (hit.collider.gameObject.layer == 6)
                    {
                        Quaternion LookRot = Quaternion.LookRotation(Light.position - tracking.position);
                        Light.rotation = Quaternion.Slerp(Light.rotation, LookRot, 3f * Time.deltaTime);
                    }
                    else
                    {
                        WaitTime -= Time.deltaTime;
                    }
                }
                else
                {
                    WaitTime -= Time.deltaTime;
                }

                if (WaitTime <= 0)
                {
                    tracking = null;
                    Debug.Log("Must've been the wind");
                }
            }
        }
        else
        {
            WaitTime = 10;
        }
    }
}
