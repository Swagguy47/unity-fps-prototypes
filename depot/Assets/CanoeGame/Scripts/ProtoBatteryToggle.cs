using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtoBatteryToggle : MonoBehaviour
{
    [SerializeField] ProtoBatterySlot Slot;
    [SerializeField] GameObject Toggeler, OppositeToggle;
    private void LateUpdate()
    {
        if (Toggeler != null) {
            Toggeler.SetActive(Slot.Powered);
        } 
        if(OppositeToggle != null) { 
            OppositeToggle.SetActive(!Slot.Powered);
        }
    }
}
