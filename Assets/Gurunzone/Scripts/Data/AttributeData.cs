using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{

    [System.Serializable]
    public enum AttributeType
    {
        None = 0,
        Health = 2,
        Energy = 3,
        Happiness = 4,
        Hunger = 6,
        Thirst = 8,
        Heat = 10, 

    }

    /// <summary>
    /// Attribute data (health, hunger, thirst, etc)
    /// </summary>

    [CreateAssetMenu(fileName = "AttributeData", menuName = "Gurunzone/AppData/AttributeData", order = 11)]
    public class AttributeData : ScriptableObject
    {
        public AttributeType type;      //Which attribute is this?
        public string title;            //Visual Text of the attribute

        [Space(5)]

        public float start_value = 100f; //Starting value
        public float max_value = 100f;  //Maximum value

        public float value_per_hour = -100f; //How much is gained (or lost) per in-game hour

        [Header("Low")]
        public string low_status;           //When low, will display this status on the character
        public float low_threshold = 25f;   //Will be considered "low" below this value

        [Header("Depleted")]
        public float deplete_hp_loss = -100f; //When this attribute is at 0, will lose this amount of health per game-hour
        public float deplete_move_mult = 1f;  //When this attribute is at 0, will multiply the move speed by this.   1f = normal speed
        public float deplete_gather_mult = 1f; //When this attribute is at 0, will multiply the gather speed by this.  1f = normal speed

        private static List<AttributeData> list = new List<AttributeData>();

        public static void Load(string folder = "")
        {
            list.Clear();
            list.AddRange(Resources.LoadAll<AttributeData>(folder));
        }

        public static AttributeData Get(AttributeType type)
        {
            foreach (AttributeData data in list)
            {
                if (data.type == type)
                    return data;
            }
            return null;
        }

        public static List<AttributeData> GetAll()
        {
            return list;
        }
    }

}
