using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    /// <summary>
    /// Craft group data will craft a random object within the group, instead of crafting a specific one
    /// </summary>

    [CreateAssetMenu(fileName = "CraftGroupData", menuName = "Gurunzone/AppData/CraftGroupData", order = 10)]
    public class CraftGroupData : CraftData
    {
        [Header("Spawn Data")]
        public CraftData[] values;  //Random possibilities

        private static List<CraftGroupData> glist = new List<CraftGroupData>();

        public CraftData GetRandomData()
        {
            if (values.Length > 0)
                return values[Random.Range(0, values.Length)];
            return null;
        }

        public CraftData GetRandomUniqueData(List<CraftData> existing)
        {
            List<CraftData> valid_items = new List<CraftData>();
            foreach (CraftData data in values)
            {
                if (!existing.Contains(data))
                    valid_items.Add(data);
            }
            if (valid_items.Count > 0)
                return valid_items[Random.Range(0, valid_items.Count)];
            return null;
        }

        public static new void Load(string folder = "")
        {
            glist.Clear();
            glist.AddRange(Resources.LoadAll<CraftGroupData>(folder));
        }

        public static new CraftGroupData Get(string id)
        {
            foreach (CraftGroupData data in glist)
            {
                if (data.id == id)
                    return data;
            }
            return null;
        }

        public static new List<CraftGroupData> GetAll()
        {
            return glist;
        }
    }
}
