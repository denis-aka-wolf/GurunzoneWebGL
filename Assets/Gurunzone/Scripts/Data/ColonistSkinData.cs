using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    [CreateAssetMenu(fileName = "ColonistSkinData", menuName = "Gurunzone/AppData/ColonistSkinData", order = 10)]
    public class ColonistSkinData : ScriptableObject
    {
        public string id;

        [Header("Prefab")]
        public GameObject skin_prefab;

        [Header("Growth")]
        public ColonistSkinData[] grow_to;

        [Header("Names")]
        public string[] names; //Possible names, overrite the default name

        private static List<ColonistSkinData> skin_list = new List<ColonistSkinData>();

        public string GetRandomName()
        {
            if (names.Length > 0)
                return names[Random.Range(0, names.Length)];
            return "";
        }

        //Get a name that has not been used before
        public string GetRandomUniqueName()
        {
            HashSet<string> existing_names = new HashSet<string>();
            foreach (Colonist colonist in Colonist.GetAll())
                existing_names.Add(colonist.SData.name);

            List<string> valid_names = new List<string>();
            foreach (string aname in names)
            {
                if (!existing_names.Contains(aname))
                    valid_names.Add(aname);
            }

            if (valid_names.Count > 0)
                return valid_names[Random.Range(0, valid_names.Count)];
            return GetRandomName();
        }

        public static void Load(string folder = "")
        {
            skin_list.Clear();
            skin_list.AddRange(Resources.LoadAll<ColonistSkinData>(folder));
        }

        public static ColonistSkinData Get(string id)
        {
            foreach (ColonistSkinData data in skin_list)
            {
                if (data.id == id)
                    return data;
            }
            return null;
        }

        public static List<ColonistSkinData> GetAll()
        {
            return skin_list;
        }
    }
}
