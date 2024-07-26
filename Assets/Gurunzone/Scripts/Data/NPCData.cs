using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    /// <summary>
    /// Data class for Non-player characters
    /// </summary>

    [CreateAssetMenu(fileName = "NPCData", menuName = "Gurunzone/AppData/NPCData", order = 10)]
    public class NPCData : CraftData
    {
        private static List<NPCData> enemy_list = new List<NPCData>();

        public static new void Load(string folder = "")
        {
            enemy_list.Clear();
            enemy_list.AddRange(Resources.LoadAll<NPCData>(folder));
        }

        public static new NPCData Get(string id)
        {
            foreach (NPCData data in enemy_list)
            {
                if (data.id == id)
                    return data;
            }
            return null;
        }

        public static new List<NPCData> GetAll()
        {
            return enemy_list;
        }
    }
}
