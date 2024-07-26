using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone.Events
{
    [System.Serializable]
    public class SaveEventData
    {
        //Dialogue data
        public Dictionary<string, int> trigger_counts = new Dictionary<string, int>();
        public Dictionary<string, float> trigger_interval = new Dictionary<string, float>();

        //Custom data
        public Dictionary<string, int> custom_values_int = new Dictionary<string, int>();
        public Dictionary<string, float> custom_values_float = new Dictionary<string, float>();
        public Dictionary<string, string> custom_values_str = new Dictionary<string, string>();

        //Current event
        public string current_event;

        public void FixData()
        {
            //Fix data when data version is different
            if (trigger_counts == null)
                trigger_counts = new Dictionary<string, int>();
            if (trigger_interval == null)
                trigger_interval = new Dictionary<string, float>();
            if (custom_values_int == null)
                custom_values_int = new Dictionary<string, int>();
            if (custom_values_float == null)
                custom_values_float = new Dictionary<string, float>();
            if (custom_values_str == null)
                custom_values_str = new Dictionary<string, string>();
        }

        public void SetTriggerCount(string event_id, int value)
        {
            if (!string.IsNullOrWhiteSpace(event_id))
            {
                trigger_counts[event_id] = value;
            }
        }

        public int GetTriggerCount(string event_id)
        {
            if (trigger_counts.ContainsKey(event_id))
                return trigger_counts[event_id];
            return 0;
        }

        public void SetLastInterval(string event_id, float value)
        {
            if (!string.IsNullOrWhiteSpace(event_id))
                trigger_interval[event_id] = value;
        }

        public float GetLastInterval(string event_id)
        {
            if (trigger_interval.ContainsKey(event_id))
                return trigger_interval[event_id];
            return float.MinValue;
        }

        public void SetCustomInt(string val_id, int value)
        {
            if (!string.IsNullOrWhiteSpace(val_id))
            {
                custom_values_int[val_id] = value;
            }
        }

        public void SetCustomFloat(string val_id, float value)
        {
            if (!string.IsNullOrWhiteSpace(val_id))
            {
                custom_values_float[val_id] = value;
            }
        }

        public void SetCustomString(string val_id, string value)
        {
            if (!string.IsNullOrWhiteSpace(val_id))
            {
                custom_values_str[val_id] = value;
            }
        }

        public bool HasCustomInt(string val_id)
        {
            return custom_values_int.ContainsKey(val_id);
        }

        public bool HasCustomFloat(string val_id)
        {
            return custom_values_float.ContainsKey(val_id);
        }

        public bool HasCustomString(string val_id)
        {
            return custom_values_str.ContainsKey(val_id);
        }

        public int GetCustomInt(string val_id)
        {
            if (custom_values_int.ContainsKey(val_id))
            {
                return custom_values_int[val_id];
            }
            return 0;
        }

        public float GetCustomFloat(string val_id)
        {
            if (custom_values_float.ContainsKey(val_id))
            {
                return custom_values_float[val_id];
            }
            return 0;
        }

        public string GetCustomString(string val_id)
        {
            if (custom_values_str.ContainsKey(val_id))
            {
                return custom_values_str[val_id];
            }
            return "";
        }

        public void DeleteCustomInt(string val_id)
        {
            custom_values_int.Remove(val_id);
        }

        public void DeleteCustomFloat(string val_id)
        {
            custom_values_float.Remove(val_id);
        }

        public void DeleteCustomString(string val_id)
        {
            custom_values_str.Remove(val_id);
        }

        public static SaveEventData Get()
        {
            return SaveData.Get().event_data;
        }

    }

}