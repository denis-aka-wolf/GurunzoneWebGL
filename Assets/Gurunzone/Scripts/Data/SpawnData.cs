using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    /// <summary>
    /// Data for objects that can be spawned (and saved) that are not CraftData
    /// </summary>

    [CreateAssetMenu(fileName = "SpawnData", menuName = "Gurunzon/AppData/SpawnData", order = 10)]
    public class SpawnData : CSData
    {
        public string id;
        public string title;

        [Header("Prefab")]
        public GameObject prefab;   //Spawned object when Created

        private static List<SpawnData> list = new List<SpawnData>();

        public static void Load(string folder = "")
        {
            list.Clear();
            list.AddRange(Resources.LoadAll<SpawnData>(folder));
        }

        public static SpawnData Get(string id)
        {
            foreach (SpawnData data in list)
            {
                if (data.id == id)
                    return data;
            }
            return null;
        }

        public static List<SpawnData> GetAll()
        {
            return list;
        }
    }
}