using System.Collections.Generic;

namespace Gurunzone
{
    /// <summary>
    /// Part of the SaveData, contain a specific inventory and its items
    /// </summary>

    [System.Serializable]
    public class InventoryData
    {
        public Dictionary<string, int> items = new Dictionary<string, int>();

        public void AddItem(ItemSet[] items)
        {
            foreach (ItemSet item in items)
                AddItem(item);
        }

        public void AddItem(ItemSet item)
        {
            if(item?.item != null)
                AddItem(item.item.id, item.quantity);
        }

        public void AddItem(string item, int quantity)
        {
            if (items.ContainsKey(item))
                items[item] += quantity;
            else
                items[item] = quantity;

            if (items[item] <= 0)
                items.Remove(item);
        }

        public void SetItem(string item, int quantity)
        {
            items[item] = quantity;
            if (quantity <= 0)
                items.Remove(item);
        }

        public bool HasItem(string item, int quantity = 1)
        {
            return GetQuantity(item) >= quantity;
        }

        public int GetQuantity(string item)
        {
            if (items.ContainsKey(item))
                return items[item];
            return 0;
        }

        public int GetTotal()
        {
            int total = 0;
            foreach (KeyValuePair<string, int> pair in items)
            {
                total += pair.Value;
            }
            return total;
        }

        public int GetTotalCargo()
        {
            int total = 0;
            foreach (KeyValuePair<string, int> pair in items)
            {
                ItemData idata = ItemData.Get(pair.Key);
                int size = idata != null ? idata.cargo_size : 1;
                total += pair.Value * size;
            }
            return total;
        }
    }

}