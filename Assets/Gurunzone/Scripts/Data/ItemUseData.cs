using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    /// <summary>
    /// An item that can be consumed
    /// </summary>

    [CreateAssetMenu(fileName = "ItemUseData", menuName = "Gurunzone/AppData/ItemUseData", order = 10)]
    public class ItemUseData : ItemData
    {
        [Header("Use")]            //Attributes added when consumed
        public int health = 0;
        public int energy = 0;
        public int hunger = 0;
        public int thirst = 0;
        public int happiness = 0;

        public override ItemType Type { get { return ItemType.Consumable; } }

        public int GetUseValue(AttributeType attribute)
        {
            if (attribute == AttributeType.Health)
                return health;
            if (attribute == AttributeType.Energy)
                return energy;
            if (attribute == AttributeType.Hunger)
                return hunger;
            if (attribute == AttributeType.Thirst)
                return thirst;
            if (attribute == AttributeType.Happiness)
                return happiness;
            return 0;
        }

        public static new ItemUseData Get(string id)
        {
            foreach (ItemData data in ilist)
            {
                if (data.id == id && data is ItemUseData)
                    return (ItemUseData)data;
            }
            return null;
        }
    }
}
