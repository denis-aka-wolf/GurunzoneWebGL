using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{

    [RequireComponent(typeof(Selectable))]
    [RequireComponent(typeof(Inventory))]
    public class Trader : MonoBehaviour
    {
        public string title;
        public ItemData currency;
        public GroupData sell_group; //Sell Items, if null, can sell anything
        public float buy_factor = 1f; //Price will be multiplied by this when buying
        public float sell_factor = 0.5f;  //Price will be multiplied by this when selling

        private Selectable selectable;
        private Inventory inventory;

        private void Awake()
        {
            selectable = GetComponent<Selectable>();
            inventory = GetComponent<Inventory>();
        }

        public void OpenTrade(Character character)
        {
            TheGame.Get().PauseScript();
            ShopPanel.Get().ShowShop(character, this);
        }

        public void CloseTrade()
        {
            TheGame.Get().UnpauseScript();
            ShopPanel.Get().Hide();
        }

        public int GetBuyCost(ItemData item)
        {
            return Mathf.RoundToInt(buy_factor * item.trade_cost);
        }

        public int GetSellCost(ItemData item)
        {
            return Mathf.RoundToInt(sell_factor * item.trade_cost);
        }

        public Selectable Selectable { get { return selectable; } }
        public Inventory Inventory { get { return inventory; } }
    }

}
