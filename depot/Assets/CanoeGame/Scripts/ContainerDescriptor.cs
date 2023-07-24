using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerDescriptor : MonoBehaviour
{
    [SerializeField] Transform SlotRoot;
    [SerializeField] GameObject SlotObject, UIObject;
    List<ContainerSlot> Slots = new List<ContainerSlot>();

    bool IsOpen;
    [HideInInspector] public Transform ContainerPos;
    [HideInInspector] public ContainerObject ContainerObject;


    private void Update()
    {
        if (IsOpen && Input.GetButtonDown("ShopExit")) {
            CloseContainer();
        }
    }

    public void OpenContainer(ContainerObject Container)
    {
        ContainerObject = Container;
        UIObject.SetActive(true);
        Slots.Clear(); //failsafe
        ContainerPos = Container.transform;
        IsOpen = true;

        int PosOffset = 0;

        int OffsetColumn= 0; //x
        int OffsetRow = 0; //y
        
        //Instantiates container slots
        foreach (Weapon Item in Container.ItemInventory)
        {
            ContainerSlot NewSlot = Instantiate(SlotObject, SlotRoot).GetComponent<ContainerSlot>();
            //Positions slot
            NewSlot.transform.position = SlotRoot.position; //just to be sure
            NewSlot.transform.localPosition = new Vector3(OffsetColumn * 85, OffsetRow * -85, 0);//offset
            NewSlot.SlotNum = Mathf.Abs(PosOffset / 85);
            PosOffset -= 85;
            OffsetColumn++;
            if (OffsetColumn >= 3) {
                OffsetColumn = 0;
                OffsetRow++;
            }
            //Sets up slot variables
            NewSlot.ITEM = Item;
            NewSlot.Descriptor = this;
            NewSlot.UpdateContainer();
            //indexes slot for later cleanup
            Slots.Add(NewSlot);
        }
    }

    public void CloseContainer() //cleanup
    {
        IsOpen = false;
        ContainerPos = null;
        foreach (ContainerSlot Slot in Slots) {
            Destroy(Slot.gameObject);
        }
        Slots.Clear();
        UIObject.SetActive(false);
        ContainerObject = null;
        PlayerCallback.PlayerBrain.CurrentCharBrain.Animated = false;
        PlayerCallback.Inventory.Offset = 0;
        PlayerCallback.PlayerBrain.IsInInventory = false;
        PlayerCallback.PlayerBrain.OpenInventory();
    }
}
