using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour
{
    public InventorySlot[] Slots;
    public InventorySlot Primary, Secondary;
    [SerializeField] RawImage PrimarySlot, SecondarySlot;
    [SerializeField] RectTransform InventoryUI;
    [HideInInspector] public float Offset = 0;

    [Serializable] 
    public struct InventorySlot
    {
        public Image ICON;
        public TextMeshProUGUI COUNT;
        [SerializeField] public Weapon ITEM;
    }

    /*private void Start() //resets inventory, so probably will need to be removed in the future
    {
        for(int i = 0; i < Slots.Length; i++)
        {
            Slots[i].ITEM = StaticItemPool.Items.ItemPool[0].BaseClass;
        }
    }*/

    public void UpdateInventory()
    {
        foreach(InventorySlot slot in Slots)
        {
            slot.ICON.sprite = slot.ITEM.Icon;
            slot.COUNT.text = slot.ITEM.a_CurrentClip.ToString();
        }
        //Primaryslot
        Primary.ICON.sprite = PlayerCallback.PlayerBrain.CurrentCharBrain.Weapons[0].Icon;
        Primary.COUNT.text = PlayerCallback.PlayerBrain.CurrentCharBrain.Weapons[0].a_CurrentClip.ToString();
        //Secondaryslot
        Secondary.ICON.sprite = PlayerCallback.PlayerBrain.CurrentCharBrain.Weapons[1].Icon;
        Secondary.COUNT.text = PlayerCallback.PlayerBrain.CurrentCharBrain.Weapons[1].a_CurrentClip.ToString();
        //Equipped slot color correction
        if (PlayerCallback.PlayerBrain.CurrentCharBrain.CurrentWeapon == 0) {
            PrimarySlot.color = new Color(1, 0.6784314f, 0.2784314f, 0.1490196f);
            SecondarySlot.color = new Color(1, 0.8449131f, 0.652f, 0.1490196f);
        }
        else {
            SecondarySlot.color = new Color(1, 0.6784314f, 0.2784314f, 0.1490196f);
            PrimarySlot.color = new Color(1, 0.8449131f, 0.652f, 0.1490196f);
        }

        //Moves inventory ui based on 'Offset' to fit other menus (-600, 600 max values before going offscreen)
        InventoryUI.offsetMin = new Vector2(Offset, InventoryUI.offsetMin.y);
    }

    public void EquipItem(int Slot)
    {
        Weapon SlotWep = Slots[Slot].ITEM;
        CharacterBrain CharBrain = PlayerCallback.PlayerBrain.CurrentCharBrain;
        Slots[Slot].ITEM = CharBrain.Weapons[CharBrain.CurrentWeapon];
        UpdateInventory();
        CharBrain.Weapons[CharBrain.CurrentWeapon] = SlotWep;
        CharBrain.UpdateWeapon();
    }

    public void EquipExternal(ContainerSlot Slot) //equips to weapon slot item from external container
    {

        Weapon SlotWep = Slot.ITEM;
        CharacterBrain CharBrain = PlayerCallback.PlayerBrain.CurrentCharBrain;
        Slot.ITEM = CharBrain.Weapons[CharBrain.CurrentWeapon];
        UpdateInventory();
        CharBrain.Weapons[CharBrain.CurrentWeapon] = SlotWep;
        CharBrain.UpdateWeapon();
    }

    public void AddExternal(ContainerSlot Slot) //adds item from external container to inventory
    {

        if (AddItem(Slot.ITEM))
        {
            Slot.ITEM = StaticItemPool.Items.ItemPool[0].BaseClass;
            Slot.UpdateContainer();
        }
    }

    public void SwapTo(int Hand)
    {
        CharacterBrain CharBrain = PlayerCallback.PlayerBrain.CurrentCharBrain;
        if (Hand != CharBrain.CurrentWeapon)
        {
            CharBrain.SwapWeapon();
        }
        else
        {
            DropItem(Hand + 8);
        }
    }

    public void ChangeItem(int Slot, Weapon NewItem) //swaps inventory slot for new item
    {
        Slots[Slot].ITEM = NewItem;
        UpdateInventory();
    }

    public bool AddItem(Weapon NewItem) //attempts to recursively add new item to inventory/hands, returns true/false
    {
        if (PlayerCallback.PlayerBrain.CurrentCharBrain.Weapons[0].ParentClass == StaticItemPool.Items.ItemPool[0].BaseClass)
        { //Check if primary empty

            Primary.ITEM = NewItem;
            PlayerCallback.PlayerBrain.CurrentCharBrain.Weapons[0] = NewItem;
            PlayerCallback.PlayerBrain.CurrentCharBrain.UpdateWeapon();
            UpdateInventory();
            return true;
            /*//prioritizes currently held slot first
            if (!(Secondary.ITEM.name == StaticItemPool.Items.ItemPool[0].BaseClass.name && PlayerCallback.PlayerBrain.CurrentCharBrain.CurrentWeapon == 1))
            {
                
            }*/
        }
        else if (PlayerCallback.PlayerBrain.CurrentCharBrain.Weapons[1] == StaticItemPool.Items.ItemPool[0].BaseClass)
        { //Check if secondary empty
            Secondary.ITEM = NewItem;
            PlayerCallback.PlayerBrain.CurrentCharBrain.Weapons[1] = NewItem;
            PlayerCallback.PlayerBrain.CurrentCharBrain.UpdateWeapon();
            UpdateInventory();
            return true;
        }

        for (int i = 0; i < Slots.Length; i++)
        {
            if (Slots[i].ITEM.ParentClass == StaticItemPool.Items.ItemPool[0].BaseClass) //if slot is empty
            {
                ChangeItem(i, NewItem);
                return true; //shows that a slot was found and set for item
            }
        }
        Debug.Log("INVENTORY FULL!");
        return false; //shows that no slot is avaliable
    }

    //make sure you're searching based off the parent class or through sandbox pool
    public int CountItem(Weapon Item)
    {
        int Total = 0;
        if (PlayerCallback.PlayerBrain.CurrentCharBrain.Weapons[0].ParentClass == Item)
        {
            Total += int.Parse(Primary.COUNT.text);
        }
        if (PlayerCallback.PlayerBrain.CurrentCharBrain.Weapons[1].ParentClass == Item)
        {
            Total += int.Parse(Secondary.COUNT.text);
        }
        foreach (InventorySlot Slot in Slots)
        {
            if (Slot.ITEM.ParentClass == Item)
            {
                Total += int.Parse(Slot.COUNT.text);
            }
        }
        return Total;
    }

    public void SubtractItem(Weapon Item, int Loss)
    {
        int Total = 0;
        int Decrease = Loss;
        if (PlayerCallback.PlayerBrain.CurrentCharBrain.Weapons[0].ParentClass == Item) //Primary
        {
            Total += int.Parse(Primary.COUNT.text);
            if (Total >= Decrease)
            {
                PlayerCallback.PlayerBrain.CurrentCharBrain.Weapons[0].a_CurrentClip -= Decrease;
                PlayerCallback.PlayerBrain.CurrentCharBrain.CheckItemEmpty(0);
            }
            else
            {
                PlayerCallback.PlayerBrain.CurrentCharBrain.Weapons[0].a_CurrentClip = 0;
                PlayerCallback.PlayerBrain.CurrentCharBrain.CheckItemEmpty(0);
                
            }
            Decrease -= Total;
            //Debug.Log("PRIMARY: " + PlayerCallback.PlayerBrain.CurrentCharBrain.Weapons[0].ParentClass.name + " TOTAL: " + Total + " DECREASE: " + Decrease);
            Total = 0;
        }
        if (PlayerCallback.PlayerBrain.CurrentCharBrain.Weapons[1].ParentClass == Item && Decrease > 0) //secondary
        {
            Total += int.Parse(Secondary.COUNT.text);
            if (Total >= Decrease)
            {
                PlayerCallback.PlayerBrain.CurrentCharBrain.Weapons[1].a_CurrentClip -= Decrease;
                PlayerCallback.PlayerBrain.CurrentCharBrain.CheckItemEmpty(1);
            }
            else
            {
                PlayerCallback.PlayerBrain.CurrentCharBrain.Weapons[1].a_CurrentClip = 0;
                PlayerCallback.PlayerBrain.CurrentCharBrain.CheckItemEmpty(1);
            }
            Decrease -= Total;
            //Debug.Log("SECONDARY: " + PlayerCallback.PlayerBrain.CurrentCharBrain.Weapons[1].ParentClass.name + " TOTAL: " + Total + " DECREASE: " + Decrease);
            Total = 0;
        }
        for (int i = 0; i < Slots.Length; i++) //all inventory slot
        {
            if (Slots[i].ITEM.ParentClass == Item && Decrease > 0) //ensures item is of correct type
            {
                Total += int.Parse(Slots[i].COUNT.text);
                if (Total >= Decrease)
                {
                    Slots[i].ITEM.a_CurrentClip -= Decrease;
                    if (Slots[i].ITEM.UnarmedWhenEmpty)
                    {
                        Slots[i].ITEM = StaticItemPool.Items.UniqueUnregistered(StaticItemPool.Items.ItemPool[0].BaseClass);
                    }
                }
                else
                {
                    Slots[i].ITEM.a_CurrentClip = 0;
                    if (Slots[i].ITEM.UnarmedWhenEmpty)
                    {
                        Slots[i].ITEM = StaticItemPool.Items.UniqueUnregistered(StaticItemPool.Items.ItemPool[0].BaseClass);
                    }
                }
                Decrease -= Total;
                //Debug.Log("SLOT: " + Slots[i].ITEM.ParentClass.name + " TOTAL: " + Total + " DECREASE: " + Decrease);
                Total = 0;
            }
        }

        UpdateInventory();
    }

    public void DropItem(int Index)
    {
        if (Index <= 7) { //is inventory slot
            ItemPickup Item = StaticItemPool.Items.Pickup(Slots[Index].ITEM);
            Item.transform.position = PlayerCallback.PlayerBrain.CurrentCharBrain.transform.position;
            Slots[Index].ITEM = StaticItemPool.Items.ItemPool[0].BaseClass;
            UpdateInventory();

            //checks to see if player dropped nothing
            if (Item.DeriveFrom == StaticItemPool.Items.ItemPool[0].BaseClass){
                Destroy(Item.gameObject);
            }
        }
        else { //is primary/secondary, 8=primary, 9=secondary
            ItemPickup Item = StaticItemPool.Items.Pickup(PlayerCallback.PlayerBrain.CurrentCharBrain.Weapons[Index - 8]);
            Item.transform.position = PlayerCallback.PlayerBrain.CurrentCharBrain.transform.position;
            PlayerCallback.PlayerBrain.CurrentCharBrain.Weapons[Index - 8] = StaticItemPool.Items.ItemPool[0].BaseClass;
            UpdateInventory();
            PlayerCallback.PlayerBrain.CurrentCharBrain.UpdateWeapon();

            //checks to see if player dropped nothing
            if (Item.DeriveFrom == StaticItemPool.Items.ItemPool[0].BaseClass) {
                Destroy(Item.gameObject);
            }
        }
        //Debug.Log("Dropping: " + Index + " Corrected: " + (Index - 8));
    }
}
