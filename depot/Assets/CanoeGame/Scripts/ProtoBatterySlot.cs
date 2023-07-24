using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtoBatterySlot : MonoBehaviour
{
    [SerializeField] Weapon BatteryCallback;
    [SerializeField] GameObject BatteryVisual;
    public bool Powered;

    private void Start()
    {
        BatteryVisual.SetActive(Powered);
    }

    public void InsertBattery()
    {
        if (!Powered)
        {
            if (PlayerCallback.Inventory.CountItem(BatteryCallback) > 0) //player has battery
            {
                PlayerCallback.Inventory.SubtractItem(BatteryCallback, 1); //remove 1 battery from inventory
                Powered = true;
                BatteryVisual.SetActive(true);
            }
        }
        else
        {
            Powered = false;
            BatteryVisual.SetActive(false);

            //instantiates unique variant of purchased item
            Weapon UniqueItem = StaticItemPool.Items.UniqueUnregistered(BatteryCallback);

            PlayerCallback.Inventory.AddItem(UniqueItem);
        }
    }
}
