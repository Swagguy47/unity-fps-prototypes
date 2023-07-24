using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContainerSlot : MonoBehaviour
{
    public Image ICON;
    public TextMeshProUGUI COUNT;
    public Weapon ITEM;
    [HideInInspector] public ContainerDescriptor Descriptor;
    [HideInInspector] public int SlotNum;

    //to be called by UIButton events
    public void ItemAdd()
    {
        if (ITEM.ParentClass != StaticItemPool.Items.ItemPool[0].BaseClass)
        { //Taking items
            PlayerCallback.Inventory.AddExternal(this);
        }
        else {//Adding items
            CharacterBrain Char = PlayerCallback.PlayerBrain.CurrentCharBrain;

            //sets container item to player held one
            ITEM = Char.Weapons[Char.CurrentWeapon];
            UpdateContainer();

            //removes held weapon
            Char.Weapons[Char.CurrentWeapon] = StaticItemPool.Items.ItemPool[0].BaseClass;
            Char.UpdateWeapon();
            PlayerCallback.Inventory.UpdateInventory();
        }

        Descriptor.ContainerObject.ItemInventory[SlotNum] = ITEM;
    }

    public void UpdateContainer() //updates ui elements to accomidate for change in items
    {
        COUNT.text = ITEM.a_CurrentClip.ToString();
        ICON.sprite = ITEM.Icon;
    }
}
