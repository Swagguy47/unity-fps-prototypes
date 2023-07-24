using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtoBatteryAnimator : MonoBehaviour
{
    [SerializeField] ProtoBatterySlot Slot;
    [SerializeField] Animator Anims;
    [SerializeField] string BoolName;
    private void LateUpdate()
    {
        Anims.SetBool(BoolName, Slot.Powered);
    }
}
