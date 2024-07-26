using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    [System.Serializable]
    public struct LootItemData
    {
        public ItemData item;
        public float probability; //1f = 100%,   0.5f = 50%
    }

    [CreateAssetMenu(fileName = "LootData", menuName = "Gurunzone/AppData/LootData", order = 15)]
    public class LootData : CSData
    {
        public LootItemData[] items;

        public ItemData GetRandomItem()
        {
            float val = Random.value;
            foreach (LootItemData litem in items)
            {
                if (val < litem.probability)
                    return litem.item;
                val -= litem.probability;
            }
            return null;
        }
    }
}
