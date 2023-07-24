using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour
{
    [SerializeField] ShopUI ShoppingUI;
    [SerializeField] AudioClip PurchaseSFX, SellSFX;
    bool Shopping, BuySell;
    int ItemIndex, ItemAmount = 1; //Amount only used for selling, index is current item from stock
    [SerializeField] ShopItem[] BuyableItems;
    [SerializeField] ShopItem[] SellableItems;

    Interactable Interactor;

    [Serializable]
    public struct ShopItem
    {
        [Header("Item info")]
        public string Name;
        public Transform LookAtPos;
        public int Cost;
        public Weapon Item;

        [Header("Shop info")]
        public int TotalStock;
        public int DailyRecharge;
    }

    private void Start()
    {
        Interactor = GetComponent<Interactable>();
    }

    private void Update()
    {
        if (Shopping) //not well optimized
        {
            if (!BuySell) //buying
            {
                CharacterBrain PlayerChar = PlayerCallback.PlayerBrain.CurrentCharBrain;

                Quaternion LookRot = Quaternion.LookRotation(PlayerChar.ViewRoot.position - BuyableItems[ItemIndex].LookAtPos.position);

                //Orients playercam towards item
                //PlayerChar.ViewRoot.rotation = Quaternion.Slerp(PlayerChar.ViewRoot.rotation, LookRot, 5f * Time.deltaTime);
                PlayerChar.ViewRoot.rotation = LookRot;
                PlayerChar.ViewRoot.LookAt(BuyableItems[ItemIndex].LookAtPos);

                //Moving between items
                if (Input.GetButtonDown("ShopLeft") && ItemIndex > 0)
                {
                    ItemIndex--;
                    UpdateShopUI();
                }
                if (Input.GetButtonDown("ShopRight") && ItemIndex < BuyableItems.Length - 1)
                {
                    ItemIndex++;
                    UpdateShopUI();
                }
                if (Input.GetButtonDown("ShopExit"))
                {
                    ExitShop();
                }
                if (Input.GetButtonDown("Interact") || Input.GetButtonDown("Jump"))
                {
                    Buy();
                }
            }
            else //selling
            {
                CharacterBrain PlayerChar = PlayerCallback.PlayerBrain.CurrentCharBrain;

                Quaternion LookRot = Quaternion.LookRotation(PlayerChar.ViewRoot.position - SellableItems[ItemIndex].LookAtPos.position);

                //Orients playercam towards item
                //PlayerChar.ViewRoot.rotation = Quaternion.Slerp(PlayerChar.ViewRoot.rotation, LookRot, 5f * Time.deltaTime);
                PlayerChar.ViewRoot.rotation = LookRot;
                PlayerChar.ViewRoot.LookAt(SellableItems[ItemIndex].LookAtPos);

                //Moving between items
                if (Input.GetButtonDown("ShopLeft") && ItemIndex > 0)
                {
                    ItemIndex--;
                    ItemAmount = 1;
                    UpdateShopUI();
                }
                if (Input.GetButtonDown("ShopRight") && ItemIndex < SellableItems.Length - 1)
                {
                    ItemIndex++;
                    ItemAmount = 1;
                    UpdateShopUI();
                }
                if (Input.GetButtonDown("ShopExit"))
                {
                    ExitShop();
                }
                if (Input.GetButtonDown("Interact") || Input.GetButtonDown("Jump"))
                {
                    Sell();
                }
            }
        }
    }

    public void EnterShop()
    {
        BuySell = false;
        ItemIndex = 0; ItemAmount = 1;
        PlayerCallback.PlayerBrain.CurrentCharBrain.Animated = true;
        Shopping = true;
        ShoppingUI.gameObject.SetActive(true);
        ShoppingUI.CurrentShop = this;
        UpdateShopUI();
        PlayerCallback.PlayerBrain.IsInInventory = false;
        PlayerCallback.PlayerBrain.OpenInventory();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Interactor.enabled = false;
        gameObject.layer = 0;
    }

    public void ExitShop()
    {
        PlayerCallback.PlayerBrain.CurrentCharBrain.Animated = false;
        Shopping = false;
        ShoppingUI.gameObject.SetActive(false);
        ShoppingUI.CurrentShop = null;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        PlayerCallback.PlayerBrain.CurrentCharBrain.ViewRoot.rotation = transform.rotation;//Quaternion.Euler(0, 180, 0);
        Interactor.enabled = true;
        gameObject.layer = 7;
    }

    private void UpdateShopUI()
    {
        if (!BuySell) {
            //Buying text
            ShoppingUI.BuyItemName.text = BuyableItems[ItemIndex].Name;
            ShoppingUI.BuyItemCost.text = "$" + BuyableItems[ItemIndex].Cost;
        }
        else {
            //Selling text
            ShoppingUI.SellItemName.text = SellableItems[ItemIndex].Name;
            ShoppingUI.SellItemCost.text = "$" + (SellableItems[ItemIndex].Cost * ItemAmount + "( " + ItemAmount + "x )");
        }
        //Cash label
        ShoppingUI.PlayerCash.text = "$" + PlayerCallback.PlayerBrain.PlayerCash;
    }

    public void Buy()
    {
        PlayerBrain Brain = PlayerCallback.PlayerBrain;
        if (Brain.PlayerCash >= BuyableItems[ItemIndex].Cost) //Successful purchase
        {
            Brain.PlayerCash -= BuyableItems[ItemIndex].Cost;
            UpdateShopUI();

            //ItemPickupSfx
            PlayerCallback.PlayerBrain.UIOneShotSrc.PlayOneShot(PurchaseSFX);

            //instantiates unique variant of purchased item
            Weapon UniqueItem = StaticItemPool.Items.UniqueUnregistered(BuyableItems[ItemIndex].Item);

            //Attempts to give player item
            if (PlayerCallback.Inventory.AddItem(UniqueItem)) //add to inventory
            { }
            else //inventory full
            {
                Destroy(UniqueItem); //nolonger needed

                //creates unique pickup variant of purchased item at player location
                ItemPickup Pickup = StaticItemPool.Items.UniquePickup(BuyableItems[ItemIndex].Item);
                Pickup.transform.position = PlayerCallback.PlayerBrain.CurrentCharBrain.transform.position;
            }
        }
    }

    public void Sell()
    {
        if (PlayerCallback.Inventory.CountItem(SellableItems[ItemIndex].Item) >= ItemAmount) //checks player's stock
        {
            PlayerCallback.Inventory.SubtractItem(SellableItems[ItemIndex].Item, ItemAmount); //removes items
            PlayerCallback.PlayerBrain.PlayerCash += (SellableItems[ItemIndex].Cost * ItemAmount); //pays player

            //Sell sfx
            PlayerCallback.PlayerBrain.UIOneShotSrc.PlayOneShot(SellSFX);
            
            //update ui to show new cash
            UpdateShopUI();
        }
    }

    public void SwapPage(bool NewPage)
    {
        BuySell = NewPage;
        ItemIndex = 0;
        UpdateShopUI();
    }

    public void IncreaseSell(int Incriment)
    {
        if (!(Incriment < 0 && ItemAmount <= 0)) //stops you from selling negative amounts of items
        {
            ItemAmount += Incriment;
        }
        UpdateShopUI();
    }
    public void SetSell(int SetLvl)
    {
        ItemAmount = SetLvl;
        UpdateShopUI();
    }

    public void AllSell()
    {
        ItemAmount = PlayerCallback.Inventory.CountItem(SellableItems[ItemIndex].Item);
        UpdateShopUI();
    }
}
