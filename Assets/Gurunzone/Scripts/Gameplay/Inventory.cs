using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    public class Inventory : MonoBehaviour
    {
        public bool global = true;          //If true, shares the same one global inventory
        public int cargo_max = 100;         //Max cargo, ignored for global inventory
        public ItemSet[] starting_items;    //Starting items

        private Selectable select; //Can be null
        private UniqueID uid; //Can be null
        private int cargo_bonus = 0;

        private static List<Inventory> inventory_list = new List<Inventory>();

        void Awake()
        {
            inventory_list.Add(this);
            select = GetComponent<Selectable>(); //Can be null
            uid = GetComponent<UniqueID>(); //Can be null
        }

        private void OnDestroy()
        {
            inventory_list.Remove(this);
        }

        private void Start()
        {
            if (!SaveData.Get().HasInventory(UID))
            {
                SData.AddItem(starting_items);
            }
        }

        public void AddItem(ItemData item, int quantity = 1)
        {
            if (item == null)
                return;

            InventoryData inventory = SData;
            if (inventory.items.ContainsKey(item.id))
                inventory.items[item.id] += quantity;
            else
                inventory.items[item.id] = quantity;

            if (inventory.items[item.id] <= 0)
                inventory.items.Remove(item.id);
        }

        public void Transfer(Storage storage)
        {
            Transfer(storage.Inventory);
        }

        public void Transfer(Inventory inventory)
        {
            if (inventory.SData == SData)
                return;

            Dictionary<ItemData, int> tranfered = new Dictionary<ItemData, int>();
            int tcargo = inventory.GetCargo();
            int max = inventory.GetCargoMax();
            foreach (KeyValuePair<string, int> pair in SData.items)
            {
                ItemData idata = ItemData.Get(pair.Key);
                if (idata != null)
                {
                    int icargo = idata.cargo_size;
                    int available = max - tcargo;
                    int avail_quantity = icargo > 0 ? available / icargo : 0;
                    int quantity = Mathf.Min(pair.Value, avail_quantity);

                    if (quantity > 0)
                    {
                        inventory.AddItem(idata, quantity);
                        tcargo += quantity * icargo;
                        if (tranfered.ContainsKey(idata))
                            tranfered[idata] += quantity;
                        else
                            tranfered[idata] = quantity;
                    }
                }
            }
            foreach (KeyValuePair<ItemData, int> pair in tranfered)
            {
                AddItem(pair.Key, -pair.Value);
            }
        }

        public void DropItem(ItemData item, Vector3 pos, int quantity = int.MaxValue)
        {
            ItemSet set = GetItem(item);
            if (set != null)
            {
                int quant = Mathf.Min(set.quantity, quantity);
                AddItem(item, -quant);
                Item.Create(item, pos, quant);
            }
        }

        public bool HasCraftCost(CraftData craftable, int multiply = 1)
        {
            if (craftable != null)
            {
                CraftCostData cost = craftable.GetCraftCost();
                bool has_cost = true;
                foreach (KeyValuePair<ItemData, int> item in cost.items)
                {
                    if (!HasItem(item.Key, item.Value * multiply))
                        has_cost = false;
                }
                foreach (KeyValuePair<GroupData, int> item in cost.fillers)
                {
                    if (!HasItemGroup(item.Key, item.Value * multiply))
                        has_cost = false;
                }
                return has_cost;
            }
            return false;
        }

        public void PayCraftCost(CraftData craftable, int multiply = 1)
        {
            if (craftable != null)
            {
                CraftCostData cost = craftable.GetCraftCost();
                foreach (KeyValuePair<ItemData, int> item in cost.items)
                {
                    AddItem(item.Key, -item.Value * multiply);
                }
                foreach (KeyValuePair<GroupData, int> item in cost.fillers)
                {
                    PayItemGroup(item.Key, item.Value * multiply);
                }
            }
        }

        public void RefundCraftCost(CraftData craftable, int multiply = 1)
        {
            if (craftable != null)
            {
                CraftCostData cost = craftable.GetCraftCost();
                foreach (KeyValuePair<ItemData, int> item in cost.items)
                {
                    AddItem(item.Key, item.Value * multiply);
                }
            }
        }

        public void PayItemGroup(GroupData group, int quantity = 1)
        {
            if (group != null && quantity > 0)
            {
                //Find which items should be used (by group)
                List<ItemData> sorted_items = new List<ItemData>(ItemData.GetAll());
                Dictionary<ItemData, int> remove_list = new Dictionary<ItemData, int>(); //Item, Quantity

                sorted_items.Sort((ItemData a, ItemData b) => { return a.priority.CompareTo(b.priority); });

                //Select which items to use
                foreach (ItemData item in sorted_items)
                {
                    if (item != null && item.HasGroup(group) && quantity > 0)
                    {
                        int iquantity = SData.GetQuantity(item.id);
                        int amount = Mathf.Min(iquantity, quantity);
                        if (amount > 0)
                        {
                            quantity -= amount;
                            remove_list.Add(item, amount);
                        }
                    }
                }

                //Use those specific items
                foreach (KeyValuePair<ItemData, int> pair in remove_list)
                {
                    AddItem(pair.Key, -pair.Value);
                }
            }
        }

        public ItemSet GetItem(ItemData item)
        {
            foreach (KeyValuePair<string, int> pair in SData.items)
            {
                ItemData idata = ItemData.Get(pair.Key);
                if (idata != null && item == idata)
                {
                    ItemSet aitem = new ItemSet();
                    aitem.item = idata;
                    aitem.quantity = pair.Value;
                    return aitem;
                }
            }
            return null;
        }

        //Get by max priority
        public ItemSet GetItem(ItemType type, GroupData group)
        {
            int max_priority = int.MinValue;
            ItemSet item = null;
            foreach (KeyValuePair<string, int> pair in SData.items)
            {
                ItemData idata = ItemData.Get(pair.Key);
                if (idata != null && idata.Type == type && idata.HasGroup(group) && idata.priority > max_priority)
                {
                    ItemSet aitem = new ItemSet();
                    aitem.item = idata;
                    aitem.quantity = pair.Value;
                    item = aitem;
                    max_priority = idata.priority;
                }
            }
            return item;
        }

        //Get by max attribute
        public ItemSet GetItem(ItemType type, GroupData group, AttributeType attr)
        {
            if (attr == AttributeType.None)
                return GetItem(type, group); //Attribute is not set, use the regular function

            int max_priority = int.MinValue;
            int max_attribute = int.MinValue;
            ItemSet item = null;
            foreach (KeyValuePair<string, int> pair in SData.items)
            {
                ItemData idata = ItemData.Get(pair.Key);
                if (idata != null && idata.Type == type && idata.HasGroup(group) && idata is ItemUseData)
                {
                    ItemUseData uitem = (ItemUseData)idata;
                    int value = uitem.GetUseValue(attr);
                    if (value > max_attribute)
                    {
                        ItemSet aitem = new ItemSet();
                        aitem.item = uitem;
                        aitem.quantity = pair.Value;
                        item = aitem;
                        max_attribute = value;
                        max_priority = uitem.priority;
                    }
                    else if (value == max_attribute && uitem.priority > max_priority)
                    {
                        ItemSet aitem = new ItemSet();
                        aitem.item = uitem;
                        aitem.quantity = pair.Value;
                        item = aitem;
                        max_priority = uitem.priority;
                    }
                }
            }
            return item;
        }

        public ItemSet GetItem(GroupData group)
        {
            int max_priority = int.MinValue;
            ItemSet item = null;
            foreach (KeyValuePair<string, int> pair in SData.items)
            {
                ItemData idata = ItemData.Get(pair.Key);
                if (idata != null && idata.HasGroup(group) && idata.priority > max_priority)
                {
                    ItemSet aitem = new ItemSet();
                    aitem.item = idata;
                    aitem.quantity = pair.Value;
                    item = aitem;
                    max_priority = idata.priority;
                }
            }
            return item;
        }

        public List<ItemSet> GetItems(GroupData group)
        {
            List<ItemSet> items = new List<ItemSet>();
            foreach (KeyValuePair<string, int> pair in SData.items)
            {
                ItemData idata = ItemData.Get(pair.Key);
                if (idata != null && idata.HasGroup(group))
                {
                    ItemSet aitem = new ItemSet();
                    aitem.item = idata;
                    aitem.quantity = pair.Value;
                    items.Add(aitem);
                }
            }
            return items;
        }

        public List<ItemSet> GetItems()
        {
            List<ItemSet> items = new List<ItemSet>();
            foreach (KeyValuePair<string, int> pair in SData.items)
            {
                ItemData idata = ItemData.Get(pair.Key);
                if (idata != null)
                {
                    ItemSet aitem = new ItemSet();
                    aitem.item = idata;
                    aitem.quantity = pair.Value;
                    items.Add(aitem);
                }
            }
            return items;
        }

        public bool HasItem(ItemData item, int quantity = 1)
        {
            return CountItem(item) >= quantity;
        }

        public int CountItem(ItemData item)
        {
            return SData.GetQuantity(item.id);
        }

        public bool HasItemGroup(GroupData group, int quantity = 1)
        {
            return CountItemGroup(group) >= quantity;
        }

        public int CountItemGroup(GroupData group)
        {
            int count = 0;
            foreach (KeyValuePair<string, int> pair in SData.items)
            {
                ItemData idata = ItemData.Get(pair.Key);
                if (idata.HasGroup(group))
                    count += pair.Value;
            }
            return count;
        }

        public int CountItems()
        {
            return SData.GetTotal();
        }

        public bool IsEmpty()
        {
            return CountItems() == 0;
        }

        public void SetCargoBonus(int bonus)
        {
            cargo_bonus = bonus;
        }

        public int GetCargo()
        {
            return SData.GetTotalCargo();
        }

        public int CountAvailableCargo()
        {
            return GetCargoMax() - GetCargo();
        }

        public bool IsMax()
        {
            return GetCargo() >= GetCargoMax();
        }

        public int GetCargoMax()
        {
            return (cargo_max > 0 && !global) ? (cargo_max + cargo_bonus) : int.MaxValue;
        }

        public Selectable Selectable { get { return select; } } //Can be null
        public InventoryData SData { get { return SaveData.Get().GetInventory(UID); } }
        public string UID { get { return (global || uid == null) ? GetGlobalInventoryUID() : uid.uid; } }

        public static Inventory GetGlobal() 
        {
            foreach (Inventory inventory in inventory_list)
            {
                if (inventory.global)
                    return inventory;
            }
            return null;
        }

        public static Inventory Get(string uid)
        {
            foreach (Inventory inventory in inventory_list)
            {
                if (inventory.UID == uid)
                    return inventory;
            }
            return null;
        }

        public static string GetGlobalInventoryUID(){ return "_GLOBAL_";}
        public static InventoryData GetGlobalData() { return SaveData.Get().GetGlobalInventory(); }
        public static List<Inventory> GetAll() { return inventory_list; }
    }
}
