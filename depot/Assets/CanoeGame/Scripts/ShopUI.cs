using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopUI : MonoBehaviour
{
    public TextMeshProUGUI BuyItemName, BuyItemCost, SellItemName, SellItemCost, PlayerCash;
    [SerializeField] GameObject BuyPage, SellPage;
    [HideInInspector] public Shop CurrentShop;

    private void OnEnable()
    {
        BuyPage.SetActive(true);
        SellPage.SetActive(false);
    }

    public void Buy()
    {
        CurrentShop.Buy();
    }

    public void Sell()
    {
        CurrentShop.Sell();
    }

    public void SwapPage(bool NewPage)
    {
        CurrentShop.SwapPage(NewPage);
    }

    public void IncreaseSell(int Incriment)
    {
        CurrentShop.IncreaseSell(Incriment);
    }

    public void ResetSell()
    {
        CurrentShop.SetSell(0);
    }

    public void AllSell()
    {
        CurrentShop.AllSell();
    }
}
