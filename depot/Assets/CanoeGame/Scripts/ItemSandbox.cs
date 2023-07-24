using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Sandbox", menuName = "ArcticGame/ItemSandbox")]
public class ItemSandbox : ScriptableObject
{
    public SandboxPool[] ItemPool;

    [Serializable]
    public struct SandboxPool
    {
        public Weapon BaseClass;
        public GameObject Pickup;
    }

    public Weapon UniqueItem(int Pool)
    {
        ItemPickup Item = Instantiate(ItemPool[Pool].Pickup).GetComponent<ItemPickup>();
        Item.DeriveFrom = ItemPool[Pool].BaseClass;
        Item.CreateUnique();

        Weapon UniqueWep = Item.UniqueItem;
        Destroy(Item.gameObject);
        return UniqueWep;
    }

    public Weapon UniqueUnregistered(Weapon BaseClass)
    {
        ItemPickup Item = Instantiate(ItemPool[0].Pickup).GetComponent<ItemPickup>();
        Item.DeriveFrom = BaseClass;
        Item.CreateUnique();

        Weapon UniqueWep = Item.UniqueItem;
        Destroy(Item.gameObject);
        return UniqueWep;
    }

    public ItemPickup UniquePickup(Weapon BaseClass)
    {
        ItemPickup Item = Instantiate(ItemPool[0].Pickup).GetComponent<ItemPickup>();
        Item.DeriveFrom = BaseClass;
        Item.CreateUnique();
        Instantiate(BaseClass.PickupModel, Item.transform.Find("-Visuals-"));
        return Item;
    }

    public ItemPickup Pickup(Weapon BaseClass)
    {
        ItemPickup Item = Instantiate(BaseClass.PickupModel).GetComponent<ItemPickup>();
        Item.DeriveFrom = BaseClass;
        Item.UniqueItem = BaseClass;
        Instantiate(BaseClass.PickupModel, Item.transform.Find("-Visuals-"));
        return Item;
    }
}
