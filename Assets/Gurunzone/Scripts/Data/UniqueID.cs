using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{

    /// <summary>
    /// Helps to generate unique ids for each individual instance of objects in the scene. 
    /// Unique IDs are mostly used in the save file to keep track of the state of an object.
    /// </summary>

    public class UniqueID : MonoBehaviour
    {
        public string uid_prefix; //Will be added to the front of every ID of this type of object, set in the prefab

        [TextArea(1, 2)]
        public string uid; //The unique ID, should be empty in the prefab. Should only be added to instances in the scene. Can be automatically generated

        private Dictionary<string, string> sub_dict = new Dictionary<string, string>();

        private static Dictionary<string, UniqueID> dict_id = new Dictionary<string, UniqueID>();

        void Awake()
        {
            if (!string.IsNullOrEmpty(uid))
            {
                dict_id[uid] = this;
            }
        }

        private void OnDestroy()
        {
            dict_id.Remove(uid);
        }

        private void Start()
        {
            if (!HasUID() && Time.time < 0.1f)
                Debug.LogWarning("UID is empty on " + gameObject.name + ". Make sure to generate UIDs with Gurunzone->Generate UID");
            if (HasUID() && SaveData.Get().IsObjectHidden(uid))
                gameObject.SetActive(false);
        }

        public void SetUID(string uid)
        {
            if (dict_id.ContainsKey(this.uid))
                dict_id.Remove(this.uid);
            this.uid = uid;
            if (!string.IsNullOrEmpty(this.uid))
                dict_id[this.uid] = this;
            sub_dict.Clear();
        }

        public bool HasUID()
        {
            return !string.IsNullOrEmpty(uid);
        }

        public void GenerateUID()
        {
            SetUID(uid_prefix + GenerateUniqueID());
        }

        public void GenerateUIDEditor()
        {
            uid = uid_prefix + GenerateUniqueID(); //Dont save to dict in editor mode
        }

        public string GetSubUID(string sub_tag)
        {
            if (sub_dict.ContainsKey(sub_tag))
                return sub_dict[sub_tag]; //Dict prevents GC alloc
            if (string.IsNullOrEmpty(uid))
                return ""; //No UID

            string sub_uid = uid + "_" + sub_tag;
            sub_dict[sub_tag] = sub_uid;
            return sub_uid;
        }

        public void SetString(string sub_tag, string value)
        {
            SaveData.Get().SetCustomString(GetSubUID(sub_tag), value);
        }

        public string GetString(string sub_tag)
        {
            return SaveData.Get().GetCustomString(GetSubUID(sub_tag)); ;
        }

        public bool HasString(string sub_tag)
        {
            return SaveData.Get().HasCustomString(GetSubUID(sub_tag));
        }

        public void SetInt(string sub_tag, int value)
        {
            SaveData.Get().SetCustomInt(GetSubUID(sub_tag), value);
        }

        public int GetInt(string sub_tag)
        {
            return SaveData.Get().GetCustomInt(GetSubUID(sub_tag)); ;
        }

        public bool HasInt(string sub_tag)
        {
            return SaveData.Get().HasCustomInt(GetSubUID(sub_tag));
        }

        public void SetFloat(string sub_tag, float value)
        {
            SaveData.Get().SetCustomFloat(GetSubUID(sub_tag), value);
        }

        public float GetFloat(string sub_tag)
        {
            return SaveData.Get().GetCustomFloat(GetSubUID(sub_tag)); ;
        }

        public bool HasFloat(string sub_tag)
        {
            return SaveData.Get().HasCustomFloat(GetSubUID(sub_tag));
        }

        public void RemoveSubUID(string sub_tag)
        {
            SaveData.Get().RemoveAllCustom(GetSubUID(sub_tag));
        }

        public void RemoveAllSubUIDs()
        {
            SaveData sdata = SaveData.Get();
            foreach (KeyValuePair<string, string> pair in sub_dict)
            {
                string subuid = pair.Value;
                sdata.RemoveAllCustom(subuid);
            }
            sub_dict.Clear();
        }

        public void Show()
        {
            SaveData.Get().ClearHiddenObject(uid);
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            SaveData.Get().HideObject(uid);
            gameObject.SetActive(false);
        }

        public static string GenerateUniqueID(int min=11, int max=17)
        {
            int length = Random.Range(min, max);
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            string unique_id = "";
            for (int i = 0; i < length; i++)
            {
                unique_id += chars[Random.Range(0, chars.Length - 1)];
            }
            return unique_id;
        }

        public static void GenerateAll(UniqueID[] objs)
        {
            HashSet<string> existing_ids = new HashSet<string>();

            foreach (UniqueID uid_obj in objs)
            {
                if (uid_obj.uid != "")
                {
                    if (existing_ids.Contains(uid_obj.uid))
                        uid_obj.uid = "";
                    else
                        existing_ids.Add(uid_obj.uid);
                }
            }

            foreach (UniqueID uid_obj in objs)
            {
                if (uid_obj.uid == "")
                {
                    //Generate new ID
                    string new_id = "";
                    while (new_id == "" || existing_ids.Contains(new_id))
                    {
                        new_id = UniqueID.GenerateUniqueID();
                    }

                    //Add new id
                    uid_obj.uid = uid_obj.uid_prefix + new_id;
                    existing_ids.Add(new_id);

#if UNITY_EDITOR
                    if (Application.isEditor && !Application.isPlaying)
                        UnityEditor.EditorUtility.SetDirty(uid_obj);
#endif
                }
            }
        }

        public static void ClearAll(UniqueID[] objs)
        {
            foreach (UniqueID uid_obj in objs)
            {
                uid_obj.uid = "";

#if UNITY_EDITOR
                if (Application.isEditor && !Application.isPlaying)
                    UnityEditor.EditorUtility.SetDirty(uid_obj);
#endif
            }
        }

        public static bool HasID(string id)
        {
            return dict_id.ContainsKey(id);
        }

        public static GameObject GetByID(string id)
        {
            if (dict_id.ContainsKey(id))
            {
                return dict_id[id].gameObject;
            }
            return null;
        }
    }

}