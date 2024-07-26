using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    /// <summary>
    /// Type of level that can be increased (hunting, mining, etc).
    /// </summary>

    [CreateAssetMenu(fileName = "LevelData", menuName = "Gurunzone/AppData/LevelData", order = 11)]
    public class LevelData : ScriptableObject
    {
        public string id;
        public string title;
        public int xp_per_level;
        public int level_max;

        [Header("Level Bonus")]
        public BonusType bonus;     //When equipped, will provide this bonus
        public float bonus_value;   //Per level
        public GroupData bonus_target; //If null, apply to all, if not apply to this target only (for gathering speed mostly)

        private static List<LevelData> level_data = new List<LevelData>();

        public int GetLevel(int xp)
        {
            return Mathf.Clamp(Mathf.FloorToInt(xp / xp_per_level) + 1, 1, level_max);
        }

        public int GetRequiredXP(int level)
        {
            return (Mathf.Clamp(level, 1, level_max) - 1) * xp_per_level;
        }

        public float GetBonusValue(int level)
        {
            return bonus_value * (Mathf.Clamp(level, 1, level_max) - 1);
        }

        public static void Load(string folder = "")
        {
            level_data.Clear();
            level_data.AddRange(Resources.LoadAll<LevelData>(folder));
        }

        public static int GetLevel(string id, int xp)
        {
            foreach (LevelData data in level_data)
            {
                if (data.id == id)
                {
                    return data.GetLevel(xp);
                }
            }
            return 0;
        }

        public static LevelData GetLevelData(string id)
        {
            foreach (LevelData data in level_data)
            {
                if (data.id == id)
                {
                    return data;
                }
            }
            return null;
        }
    }

}