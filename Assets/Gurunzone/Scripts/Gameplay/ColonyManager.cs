using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    /// <summary>
    /// Manages stats that are global to the Colony (instead of for individual characters
    /// </summary>

    public class ColonyManager : MonoBehaviour
    {
        [Header("Attributes")]
        public AttributeData[] attributes;  //List of available GLOBAL attributes (one generic value, not per character)


        private static ColonyManager instance;

        private void Awake()
        {
            instance = this;
        }

        void Start()
        {
            //Init attributes
            foreach (AttributeData attr in attributes)
            {
                if (!SaveData.Get().HasAttribute(attr.type))
                    SaveData.Get().SetAttributeValue(attr.type, attr.start_value, attr.max_value);
            }
        }

        void Update()
        {
            if (TheGame.Get().IsPaused())
                return;

            //Update attributes
            float game_speed = TheGame.Get().GetGameTimeSpeed();

            //Update Attributes
            foreach (AttributeData attr in attributes)
            {
                float update_value = attr.value_per_hour;
                update_value = update_value * game_speed * Time.deltaTime;
                SaveData.Get().AddAttributeValue(attr.type, update_value, attr.max_value);
            }
        }

        public void AddAttribute(AttributeType type, float value)
        {
            if (HasAttribute(type))
            {
                SaveData.Get().AddAttributeValue(type, value, GetAttributeMax(type));
            }
        }

        public void SetAttribute(AttributeType type, float value)
        {
            if (HasAttribute(type))
            {
                SaveData.Get().SetAttributeValue(type, value, GetAttributeMax(type));
            }
        }

        public bool HasAttribute(AttributeType type)
        {
            return SaveData.Get().HasAttribute(type) && GetAttribute(type) != null;
        }

        public float GetAttributeValue(AttributeType type)
        {
            return SaveData.Get().GetAttributeValue(type);
        }

        public float GetAttributeMax(AttributeType type)
        {
            AttributeData adata = GetAttribute(type);
            if (adata != null)
                return adata.max_value;
            return 100f;
        }

        public AttributeData GetAttribute(AttributeType type)
        {
            foreach (AttributeData attr in attributes)
            {
                if (attr.type == type)
                    return attr;
            }
            return null;
        }

        public bool IsLow(AttributeType type)
        {
            AttributeData attr = GetAttribute(type);
            float val = GetAttributeValue(type);
            return (val <= attr.low_threshold);
        }

        public bool IsDepleted(AttributeType type)
        {
            float val = GetAttributeValue(type);
            return (val <= 0f);
        }

        public bool IsAnyDepleted()
        {
            foreach (AttributeData attr in attributes)
            {
                float val = GetAttributeValue(attr.type);
                if (val <= 0f)
                    return true;
            }
            return false;
        }

        public Inventory GetInventory()
        {
            return Inventory.GetGlobal();
        }

        public string GetStatusText()
        {
            List<string> tlist = new List<string>();
            foreach (AttributeData attr in attributes)
            {
                if (IsLow(attr.type))
                    tlist.Add(attr.low_status);
            }
            return string.Join(", ", tlist.ToArray());
        }

        public static ColonyManager Get()
        {
            return instance;
        }
    }
}
