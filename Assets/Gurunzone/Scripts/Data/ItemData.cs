using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    public enum ItemType
    {
        Default = 0,
        Equipment = 10,
        Consumable = 20,
        Projectile = 30,
    }

    /// <summary>
    /// Data for items (and resources)
    /// </summary>

    [CreateAssetMenu(fileName = "ItemData", menuName = "Gurunzone/AppData/ItemData", order = 10)]
    public class ItemData : CraftData
    {
        [Header("Stats")]
        public int priority = 0;        //Priority for using (like it will eat highest priority items first)
        public int cargo_size = 1;      //How much space this item takes in inventories?
        public int trade_cost = 1;      //Base cost when trading the item

        protected static List<ItemData> ilist = new List<ItemData>();

        public virtual ItemType Type { get { return ItemType.Default; } }


        public int GetQuantity(int cargo)
        {
            return Mathf.FloorToInt(cargo / Mathf.Max(cargo_size, 1));
        }

        public static new void Load(string folder = "")
        {
            ilist.Clear();
            ilist.AddRange(Resources.LoadAll<ItemData>(folder));
            ilist.Sort((ItemData a, ItemData b) => {
                if (a.sort_order == b.sort_order)
                    return a.title.CompareTo(b.title);
                return a.sort_order.CompareTo(b.sort_order);
            });
        }

        public static new ItemData Get(string id)
        {
            foreach (ItemData data in ilist)
            {
                if (data.id == id)
                    return data;
            }
            return null;
        }

        public static new List<ItemData> GetAll()
        {
            return ilist;
        }
    }
}
