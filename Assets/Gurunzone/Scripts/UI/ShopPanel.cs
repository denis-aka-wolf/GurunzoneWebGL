using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Gurunzone
{
    /// <summary>
    /// UI panel for buy/sell items (npc shop)
    /// </summary>

    public class ShopPanel : UISlotPanel
    {
        public Text shop_title;
        public Text gold_value;
        public ShopSlot[] buy_slots;
        public ShopSlot[] sell_slots;
        public AudioClip buy_sell_audio;
        public string tab_group = "shop";

        [Header("Description")]
        public Text title;
        public Text desc;
        public Text buy_cost;
        public Button button;
        public Text button_text;
        public GameObject desc_group;

        private Character character;
        private Trader trader;
        private ShopSlot selected = null;
        private GroupData filter;

        private static ShopPanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;

            for (int i = 0; i < slots.Length; i++)
                ((ShopSlot)slots[i]).Hide();

            onClickSlot += OnClickSlot;
            onRightClickSlot += OnRightClickSlot;
        }

        protected override void Start()
        {
            base.Start();

            foreach (TabFilter filter in TabFilter.GetAll(tab_group))
                filter.onClick += OnClickTab;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

        }

        protected override void RefreshPanel()
        {
            base.RefreshPanel();

            TabFilter.Select(tab_group, filter);

            gold_value.text = "0";

            foreach (ShopSlot slot in buy_slots)
                slot.Hide();

            foreach (ShopSlot slot in sell_slots)
                slot.Hide();

            Inventory inventory = Inventory.GetGlobal();

            if (inventory != null)
            {
                int money = inventory.CountItem(trader.currency);
                gold_value.text = money.ToString();

                //Buy items
                int index = 0;
                foreach (ItemSet item in trader.Inventory.GetItems())
                {
                    if (filter == null || item.item.HasGroup(filter))
                    {
                        if (index < buy_slots.Length && item.item != trader.currency)
                        {
                            ShopSlot slot = buy_slots[index];
                            slot.SetBuySlot(item.item, trader.GetBuyCost(item.item), item.quantity);
                            slot.SetSelected(selected == slot);
                            index++;
                        }
                    }
                }

                //Sell items
                index = 0;
                foreach (ItemSet item in inventory.GetItems())
                {
                    if (index < sell_slots.Length && item.item != trader.currency)
                    {
                        ItemData idata = item?.item;
                        bool can_sell = CanSell(idata);
                        ShopSlot slot = sell_slots[index];
                        slot.SetSellSlot(idata, trader.GetSellCost(idata), item.quantity, can_sell);
                        slot.SetSelected(selected == slot);
                        index++;
                    }
                }

                //Description
                ItemData select_item = selected?.GetItem();
                desc_group.SetActive(select_item != null);
                if (select_item != null)
                {
                    title.text = select_item.title;
                    desc.text = select_item.desc;
                    bool sell = selected.IsSell();
                    int cost = (sell ? trader.GetSellCost(select_item) : trader.GetBuyCost(select_item));
                    buy_cost.text = cost.ToString();
                    button_text.text = sell ? "SELL" : "BUY";
                    button.interactable = (sell && cost > 0 && CanSell(select_item)) || (!sell && cost <= money); 
                }
            }
        }

        private bool CanSell(ItemData item)
        {
            return trader.sell_group == null || item.HasGroup(trader.sell_group);
        }

        public void ShowShop(Character character, Trader trader)
        {
            if (character != null && trader != null)
            {
                this.character = character;
                this.trader = trader;
                shop_title.text = trader.title;
                selected = null;
                filter = null;
                RefreshPanel();
                Show();
            }
        }

        public void HideShop()
        {
            if (trader != null)
                trader.CloseTrade();
        }

        public override void Hide(bool instant = false)
        {
            base.Hide(instant);
            character = null;
        }

        private void OnClickSlot(UISlot islot)
        {
            ShopSlot slot = (ShopSlot)islot;
            ItemData item = slot.GetItem();

            if (slot != null && item != null && selected != slot)
                selected = slot;
            else
                selected = null;
           
            RefreshPanel();
        }

        public void OnClickBuy()
        {
            ShopSlot slot = selected;
            bool sell = slot.IsSell();
            ItemData item = slot.GetItem();
            Inventory inventory = Inventory.GetGlobal();

            if (sell)
            {
                int cost = trader.GetSellCost(item);
                if (inventory.HasItem(item, 1) && cost > 0 && CanSell(item))
                {
                    inventory.AddItem(trader.currency, cost);
                    inventory.AddItem(item, -1);
                    trader.Inventory.AddItem(item, 1);

                    TheAudio.Get().PlaySFX("shop", buy_sell_audio);
                }
            }
            else
            {
                int cost = trader.GetBuyCost(item);
                if (inventory.HasItem(trader.currency, cost) && trader.Inventory.HasItem(item, 1))
                {
                    inventory.AddItem(trader.currency, -cost);
                    inventory.AddItem(item, 1);
                    trader.Inventory.AddItem(item, -1);

                    TheAudio.Get().PlaySFX("shop", buy_sell_audio);
                }
            }
            RefreshPanel();
        }

        private void OnRightClickSlot(UISlot islot)
        {
            
        }

        private void OnClickTab(TabFilter filter)
        {
            this.filter = filter.filter_group;
            RefreshPanel();
        }

        public Character GetCharacter()
        {
            return character;
        }

        public static ShopPanel Get()
        {
            return instance;
        }

        public static bool IsAnyVisible()
        {
            if (instance)
                return instance.IsVisible();
            return false;
        }
    }

}