using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtoBatteryCharger : MonoBehaviour
{
    [SerializeField] ProtoBatterySlot[] Slots;
    [SerializeField] Animator Anims;
    [SerializeField] string FloatName;
    float Power = 0;
    int ReachablePower = 0;
    private void LateUpdate()
    {
        ReachablePower = 0;
        foreach(ProtoBatterySlot Slot in Slots) {
            if (Slot.Powered) {
                ReachablePower++;
            }
        }

        //draining and powering
        if (Power < ReachablePower)
        {
            Power += Time.deltaTime;
        }
        else if (Power > ReachablePower)
        {
            Power -= Time.deltaTime;
        }

        Anims.SetFloat(FloatName, Power / Slots.Length);

        //Debug.Log("POWER: " + Power + " REACHABLE: " + ReachablePower);
    }
}
