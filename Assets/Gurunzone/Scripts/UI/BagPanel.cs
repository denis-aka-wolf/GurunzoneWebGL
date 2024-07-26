using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gurunzone
{

    /// <summary>
    /// Colonist Inventory
    /// </summary>

    public class BagPanel : UIPanel
    {
        public Text cargo;
        public ItemSlot[] bag_slots;
        public ItemSlot[] global_slots;
        public ItemSlot[] equipped_slots;

        private Colonist colonist;
        private float timer = 0f;

        private static BagPanel instance;

        protected override void Awake()
        {
            instance = this;
            base.Awake();

            foreach (ItemSlot slot in bag_slots)
                slot.onClick += OnClickBagSlot;
            foreach (ItemSlot slot in global_slots)
                slot.onClick += OnClickGlobalSlot;
            foreach (ItemSlot slot in equipped_slots)
                slot.onClick += OnClickEquipped;

            foreach (ItemSlot slot in bag_slots)
                slot.onClickRight += OnClickRightBagSlot;
            foreach (ItemSlot slot in global_slots)
                slot.onClickRight += OnClickRightGlobalSlot;
            foreach (ItemSlot slot in equipped_slots)
                slot.onClickRight += OnClickRightEquipped;

            foreach (ItemSlot slot in bag_slots)
                slot.Hide();
            foreach (ItemSlot slot in global_slots)
                slot.Hide();
            foreach (ItemSlot slot in equipped_slots)
                slot.Hide();
        }

        protected override void Update()
        {
            base.Update();

            if (!IsVisible() || colonist == null)
                return;

            timer += Time.deltaTime;
            if (timer > 1f)
            {
                RefreshPanel();
            }
        }

        private void RefreshPanel()
        {
            foreach (ItemSlot slot in global_slots)
                slot.Hide();
            foreach (ItemSlot slot in equipped_slots)
                slot.Hide();
            foreach (ItemSlot slot in bag_slots)
                slot.Hide();

            Inventory inventory = colonist.Inventory;

            int cargo = inventory.GetCargo();
            int cargo_max = inventory.GetCargoMax();
            this.cargo.text = cargo + " / " + cargo_max;

            Inventory global = Inventory.GetGlobal();

            int index = 0;
            foreach (ItemSet item in inventory.GetItems())
            {
                if (index < bag_slots.Length)
                {
                    ItemSlot slot = bag_slots[index];
                    slot.SetSlot(item.item, item.quantity);
                    index++;
                }
            }

            index = 0;
            foreach (ItemSet item in global.GetItems())
            {
                if (index < global_slots.Length)
                {
                    ItemSlot slot = global_slots[index];
                    slot.SetSlot(item.item, item.quantity);
                    index++;
                }
            }

            index = 0;
            foreach (ItemSet item in colonist.Character.Equip.GetEquippedItems())
            {
                if (index < equipped_slots.Length)
                {
                    ItemSlot slot = equipped_slots[index];
                    slot.SetSlot(item.item, item.quantity);
                    index++;
                }
            }
        }

        public void ShowColonist(Colonist colonist)
        {
            this.colonist = colonist;
            if (colonist != null && colonist.Inventory != null && colonist.Character.Equip != null)
            {
                Show();
                RefreshPanel();
            }
        }

        private void UnequipItem(Inventory inventory, ItemData item)
        {
            if (inventory != null && item != null && item is ItemEquipData)
            {
                if (colonist.Character.Equip.HasEquippedItem(item))
                {
                    ItemEquipData eitem = (ItemEquipData)item;
                    colonist.Character.Equip.UnequipItem(inventory, eitem);
                    RefreshPanel();
                }
            }
        }

        private void UseItem(Inventory inventory, ItemData item)
        {
            if (inventory != null && item != null)
            {
                //Equip
                if (item is ItemEquipData)
                {
                    ItemEquipData eitem = (ItemEquipData)item;
                    if (inventory.HasItem(item))
                    {
                        colonist.Character.Equip.EquipItem(inventory, eitem);
                        RefreshPanel();
                    }
                }
                //Consume
                if (item is ItemUseData)
                {
                    ItemUseData eitem = (ItemUseData)item;
                    if (inventory.HasItem(item))
                    {
                        colonist.UseItem(inventory, eitem);
                        RefreshPanel();
                    }
                }
            }
        }

        private void DropItem(Inventory inventory, ItemData item)
        {
            if (inventory != null && item != null)
            {
                inventory.DropItem(item, colonist.transform.position);
            }
        }

        //Left click to unequip item
        private void OnClickEquipped(UISlot slot)
        {
            Inventory global = Inventory.GetGlobal();
            ItemSlot islot = (ItemSlot)slot;
            ItemData item = islot.GetItem();
            UnequipItem(global, item);
        }

        //Left click to equip or eat item
        private void OnClickGlobalSlot(UISlot slot)
        {
            Inventory global = Inventory.GetGlobal();
            ItemSlot islot = (ItemSlot)slot;
            ItemData item = islot.GetItem();
            UseItem(global, item);
        }

        //Left click to equip or eat item
        private void OnClickBagSlot(UISlot slot)
        {
            ItemSlot islot = (ItemSlot)slot;
            ItemData item = islot.GetItem();
            UseItem(colonist.Inventory, item);
        }

        //Right click to unequip item
        private void OnClickRightEquipped(UISlot slot)
        {
            Inventory global = Inventory.GetGlobal();
            ItemSlot islot = (ItemSlot)slot;
            ItemData item = islot.GetItem();
            UnequipItem(global, item);
        }

        //Right click to drop item
        private void OnClickRightGlobalSlot(UISlot slot)
        {
            Inventory global = Inventory.GetGlobal();
            ItemSlot islot = (ItemSlot)slot;
            ItemData item = islot.GetItem();
            DropItem(global, item);
        }

        //Right click to drop item
        private void OnClickRightBagSlot(UISlot slot)
        {
            ItemSlot islot = (ItemSlot)slot;
            ItemData item = islot.GetItem();
            DropItem(colonist.Inventory, item);
        }

        public Colonist GetColonist()
        {
            return colonist;
        }

        public static BagPanel Get()
        {
            return instance;
        }
    }
}
