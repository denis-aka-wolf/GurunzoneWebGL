using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    [CreateAssetMenu(fileName = "ColonistData", menuName = "Gurunzone/AppData/ColonistData", order = 10)]
    public class ColonistData : CraftData
    {
        [Header("Class Bonus")]
        public BonusType bonus;     //When this class, will provide this bonus
        public float bonus_value;
        public GroupData bonus_target; //If null, apply to all, if not apply to this target only (for gathering speed mostly)

        [Header("Growing")]
        public ColonistData[] grow_to;
        public float grow_time; //In game-hours

        [Header("Starting levels")]
        public ColonistLevelData[] starting_levels;

        [Header("Skins")]
        public ColonistSkinData[] skins; //Possible skins when spawning

        private static List<ColonistData> colonist_list = new List<ColonistData>();

        public ColonistSkinData GetRandomSkin()
        {
            if (skins.Length > 0)
                return skins[Random.Range(0, skins.Length)];
            return null;
        }

        public string GetSkinID(ColonistSkinData skin)
        {
            return skin != null ? skin.id : "";
        }

        public GameObject GetPrefab(ColonistSkinData skin)
        {
            return skin != null ? skin.skin_prefab : prefab;
        }

        public string GetRandomName(ColonistSkinData skin)
        {
            if (skin != null)
                return skin.GetRandomName();
            return title;
        }

        //Get a name that has not been used before
        public string GetRandomUniqueName(ColonistSkinData skin)
        {
            if (skin != null)
                return skin.GetRandomUniqueName();
            return title;
        }

        public int GetStartingLevel(string level_id)
        {
            if (starting_levels == null)
                return 1;

            foreach (ColonistLevelData clvl in starting_levels)
            {
                if (clvl.id == level_id)
                    return clvl.level;
            }
            return 1;
        }

        public static new void Load(string folder = "")
        {
            colonist_list.Clear();
            colonist_list.AddRange(Resources.LoadAll<ColonistData>(folder));
        }

        public static new ColonistData Get(string id)
        {
            foreach (ColonistData data in colonist_list)
            {
                if (data.id == id)
                    return data;
            }
            return null;
        }

        public static new List<ColonistData> GetAll()
        {
            return colonist_list;
        }
    }

    [System.Serializable]
    public class ColonistLevelData
    {
        public string id;
        public int level;
    }
}
