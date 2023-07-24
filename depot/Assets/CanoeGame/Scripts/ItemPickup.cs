using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Weapon DeriveFrom;
    [HideInInspector] public Weapon UniqueItem;

    Interactable Interactor;

    public void Start()
    {
        Interactor = GetComponent<Interactable>();

        CreateUnique();
    }

    public void CreateUnique()
    {
        UniqueItem = ScriptableObject.CreateInstance<Weapon>();

        UniqueItem.BurstRate = DeriveFrom.BurstRate;
        UniqueItem.FirstPersonOffset = DeriveFrom.FirstPersonOffset;
        UniqueItem.i_FireRate = DeriveFrom.i_FireRate;
        UniqueItem.FireRate = DeriveFrom.FireRate;
        UniqueItem.AmmoClip = DeriveFrom.AmmoClip;
        UniqueItem.AmmoSpare = DeriveFrom.AmmoSpare;
        UniqueItem.Damage = DeriveFrom.Damage;
        UniqueItem.WeaponAnimator = DeriveFrom.WeaponAnimator;
        UniqueItem.a_CurrentClip = DeriveFrom.AmmoClip;//DeriveFrom.a_CurrentClip;
        UniqueItem.a_Extra = DeriveFrom.AmmoSpare;//DeriveFrom.a_Extra;
        UniqueItem.BulletDistance = DeriveFrom.BulletDistance;
        UniqueItem.BurstAmount = DeriveFrom.BurstAmount;
        UniqueItem.InteractOnly = DeriveFrom.InteractOnly;
        UniqueItem.Model = DeriveFrom.Model;
        UniqueItem.RecoilBloom = DeriveFrom.RecoilBloom;
        UniqueItem.RecoilSpring = DeriveFrom.RecoilSpring;
        UniqueItem.TagName = DeriveFrom.TagName;
        UniqueItem.UnarmedWhenEmpty = DeriveFrom.UnarmedWhenEmpty;
        UniqueItem.SwayIntensity = DeriveFrom.SwayIntensity;
        UniqueItem.Icon = DeriveFrom.Icon;
        UniqueItem.ParentClass = DeriveFrom;
        UniqueItem.PickupModel= DeriveFrom.PickupModel;
    }

    public void Pickup()
    {
        if (PlayerCallback.Inventory.AddItem(UniqueItem))
        {
            Destroy(this.gameObject);
        }
        /*//Interactor.Interactor
        Interactor.Interactor.Weapons[Interactor.Interactor.CurrentWeapon] = UniqueItem;
        Interactor.Interactor.UpdateWeapon();
        Destroy(this.gameObject);*/
    }
}
