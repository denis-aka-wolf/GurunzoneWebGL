using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Gurunzone.Events;

namespace Gurunzone
{
    /// <summary>
    /// SaveData is the main save file data script. Everything contained in this script is what will be saved. 
    /// It also contains a lot of functions to easily access the saved data. Make sure to call TheGame.Get().Save() to write the data to a file on the disk.
    /// The latest save file will be loaded automatically when starting the game
    /// </summary>

    [System.Serializable]
    public class SaveData
    {
        public string filename;
        public string version;
        public DateTime last_save;

        //-------------------

        public Vector3Data camera_pos;
        public QuaternionData camera_rot;
        public string current_scene = ""; //Scene loaded
        public int world_seed;

        public int day = 0;
        public float day_time = 0f; // 0 = midnight, 24 = end of day
        public float play_time = 0f; //total play time in actual seconds

        public float master_volume = 1f;
        public float music_volume = 1f;
        public float sfx_volume = 1f;

        public SaveEventData event_data = new SaveEventData();
        public SaveQuestData quest_data = new SaveQuestData();

        public Dictionary<string, int> unique_ints = new Dictionary<string, int>(); //Unique ints
        public Dictionary<string, float> unique_floats = new Dictionary<string, float>();
        public Dictionary<string, string> unique_strings = new Dictionary<string, string>();
        public Dictionary<string, int> removed_ids = new Dictionary<string, int>(); //1 = removed
        public Dictionary<string, int> hidden_ids = new Dictionary<string, int>(); //1 = hidden

        public Dictionary<string, SaveSpawnedData> spawned_objects = new Dictionary<string, SaveSpawnedData>();
        public Dictionary<string, SaveCharacterData> characters = new Dictionary<string, SaveCharacterData>();
        public Dictionary<string, SaveColonistData> colonists = new Dictionary<string, SaveColonistData>(); //Colonists
        public Dictionary<string, SaveNPCData> npcs = new Dictionary<string, SaveNPCData>(); //NPCs
        public Dictionary<string, SaveZoneData> zones = new Dictionary<string, SaveZoneData>(); //Zones
        public Dictionary<string, SaveConstructionData> buildings = new Dictionary<string, SaveConstructionData>(); //Buildings
        public Dictionary<string, SaveItemData> items = new Dictionary<string, SaveItemData>(); //Dropped Items
        public Dictionary<string, InventoryData> inventories = new Dictionary<string, InventoryData>(); //Items inside storage/character
        public Dictionary<AttributeType, float> attributes = new Dictionary<AttributeType, float>(); //Generic colony attributes, not specific to characters

        public Dictionary<string, float> tech_progress = new Dictionary<string, float>(); //Tech being researched
        public Dictionary<string, int> tech_completed = new Dictionary<string, int>(); //Tech completed

        //-------------------

        private static string file_loaded = "";
        public static SaveData player_data = null;

        public SaveData(string name)
        {
            filename = name;
            version = Application.version;
            last_save = DateTime.Now;

            day = 1;
            day_time = 6f; // Start game at 6 in the morning

            master_volume = 1f;
            music_volume = 1f;
            sfx_volume = 1f;
        }

        public void FixData()
        {
            //Fix data to make sure old save files compatible with new game version
            if (unique_ints == null)
                unique_ints = new Dictionary<string, int>();
            if (unique_floats == null)
                unique_floats = new Dictionary<string, float>();
            if (unique_strings == null)
                unique_strings = new Dictionary<string, string>();
            if (removed_ids == null)
                removed_ids = new Dictionary<string, int>();
            if (hidden_ids == null)
                hidden_ids = new Dictionary<string, int>();
            if (spawned_objects == null)
                spawned_objects = new Dictionary<string, SaveSpawnedData>();
            if (characters == null)
                characters = new Dictionary<string, SaveCharacterData>();
            if (colonists == null)
                colonists = new Dictionary<string, SaveColonistData>();
            if (npcs == null)
                npcs = new Dictionary<string, SaveNPCData>();
            if (zones == null)
                zones = new Dictionary<string, SaveZoneData>();
            if (buildings == null)
                buildings = new Dictionary<string, SaveConstructionData>();
            if (items == null)
                items = new Dictionary<string, SaveItemData>();
            if (inventories == null)
                inventories = new Dictionary<string, InventoryData>();
            if (attributes == null)
                attributes = new Dictionary<AttributeType, float>();
            if (tech_progress == null)
                tech_progress = new Dictionary<string, float>();
            if (tech_completed == null)
                tech_completed = new Dictionary<string, int>();

            if (event_data == null)
                event_data = new SaveEventData();
            event_data.FixData();
            if (quest_data == null)
                quest_data = new SaveQuestData();
            quest_data.FixData();

            foreach (KeyValuePair<string, SaveColonistData> colonist in colonists)
                colonist.Value.FixData();
        }

        // ---- Specific objects -----

        public SaveSpawnedData GetSpawnedObject(string uid)
        {
            if (spawned_objects.ContainsKey(uid))
                return spawned_objects[uid];
            SaveSpawnedData spawned_obj = new SaveSpawnedData();
            spawned_objects[uid] = spawned_obj;
            return spawned_obj;
        }

        public bool HasSpawnedObject(string uid)
        {
            return spawned_objects.ContainsKey(uid);
        }

        public void RemoveSpawnedObject(string uid)
        {
            spawned_objects.Remove(uid);
        }

        public SaveCharacterData GetCharacter(string uid)
        {
            if (characters.ContainsKey(uid))
                return characters[uid];
            SaveCharacterData character = new SaveCharacterData();
            characters[uid] = character;
            return character;
        }

        public bool HasCharacter(string uid)
        {
            return characters.ContainsKey(uid);
        }

        public void RemoveCharacter(string uid)
        {
            characters.Remove(uid);
        }

        public SaveColonistData GetColonist(string uid)
        {
            if (colonists.ContainsKey(uid))
                return colonists[uid];
            SaveColonistData colonist = new SaveColonistData();
            colonists[uid] = colonist;
            return colonist;
        }

        public bool HasColonist(string uid)
        {
            return colonists.ContainsKey(uid);
        }

        public void RemoveColonist(string uid)
        {
            colonists.Remove(uid);
        }

        public SaveNPCData GetNPC(string uid)
        {
            if (npcs.ContainsKey(uid))
                return npcs[uid];
            SaveNPCData npc = new SaveNPCData();
            npcs[uid] = npc;
            return npc;
        }

        public bool HasNPC(string uid)
        {
            return npcs.ContainsKey(uid);
        }

        public void RemoveNPC(string uid)
        {
            npcs.Remove(uid);
        }

        public SaveZoneData GetZone(string uid)
        {
            if (zones.ContainsKey(uid))
                return zones[uid];
            SaveZoneData zone = new SaveZoneData();
            zones[uid] = zone;
            return zone;
        }

        public bool HasZone(string uid)
        {
            return zones.ContainsKey(uid);
        }

        public void RemoveZone(string uid)
        {
            zones.Remove(uid);
        }

        public SaveConstructionData GetBuilding(string uid)
        {
            if (buildings.ContainsKey(uid))
                return buildings[uid];
            SaveConstructionData building = new SaveConstructionData();
            buildings[uid] = building;
            return building;
        }

        public bool HasBuilding(string uid)
        {
            return buildings.ContainsKey(uid);
        }

        public void RemoveBuilding(string uid)
        {
            buildings.Remove(uid);
        }

        public SaveItemData GetItem(string uid)
        {
            if (items.ContainsKey(uid))
                return items[uid];
            SaveItemData item = new SaveItemData();
            items[uid] = item;
            return item;
        }

        public bool HasItem(string uid)
        {
            return items.ContainsKey(uid);
        }

        public void RemoveItem(string uid)
        {
            items.Remove(uid);
        }

        public InventoryData GetGlobalInventory()
        {
            return GetInventory(Inventory.GetGlobalInventoryUID());
        }

        public InventoryData GetInventory(string uid)
        {
            if (inventories.ContainsKey(uid))
                return inventories[uid];
            InventoryData inv = new InventoryData();
            inventories[uid] = inv;
            return inv;
        }

        public bool HasInventory(string uid)
        {
            return inventories.ContainsKey(uid);
        }

        public void RemoveAll(string uid)
        {
            RemoveSpawnedObject(uid); //Remove object if it was spawned
            RemoveObject(uid); //Remove object if it was in initial scene
            RemoveItem(uid); //Remove item if its an item
            RemoveBuilding(uid); //Remove building
            RemoveZone(uid); //Remove zone
            RemoveCharacter(uid); //Remove character
        }

        //---- Techs ------

        public void AddTechProgress(string id, float value)
        {
            if (tech_progress.ContainsKey(id))
                tech_progress[id] += value;
            else
                tech_progress[id] = value;
        }

        public void SetTechProgress(string id, float value)
        {
            tech_progress[id] = value;
        }

        public void RemoveTechProgress(string id)
        {
            if (tech_progress.ContainsKey(id))
                tech_progress.Remove(id);
        }

        public float GetTechProgress(string id)
        {
            if (tech_progress.ContainsKey(id))
                return tech_progress[id];
            return 0f;
        }

        public bool HasTechProgress(string id)
        {
            return tech_progress.ContainsKey(id);
        }

        public void CompleteTech(string id, int level = 1)
        {
            tech_completed[id] = level;
        }

        public int GetTechLevel(string id)
        {
            if (tech_completed.ContainsKey(id))
                return tech_completed[id];
            return 0;
        }

        public bool IsTechCompleted(string id)
        {
            return GetTechLevel(id) > 0;
        }

        //--- Global Colony Attributes ----

        public bool HasAttribute(AttributeType type)
        {
            return attributes.ContainsKey(type);
        }

        public float GetAttributeValue(AttributeType type)
        {
            if (attributes.ContainsKey(type))
                return attributes[type];
            return 0f;
        }

        public void SetAttributeValue(AttributeType type, float value)
        {
            attributes[type] = value;
        }

        public void AddAttributeValue(AttributeType type, float value)
        {
            if (attributes.ContainsKey(type))
                attributes[type] += value;
        }

        public void SetAttributeValue(AttributeType type, float value, float max)
        {
            attributes[type] = Mathf.Clamp(value, 0f, max);
        }

        public void AddAttributeValue(AttributeType type, float value, float max)
        {
            if (attributes.ContainsKey(type))
            {
                attributes[type] += value;
                attributes[type] = Mathf.Clamp(attributes[type], 0f, max);
            }
        }

        //---- Removed objects -----

        public void RemoveObject(string uid)
        {
            if (!string.IsNullOrEmpty(uid))
                removed_ids[uid] = 1;
        }

        public void ClearRemovedObject(string uid) {
            if (removed_ids.ContainsKey(uid))
                removed_ids.Remove(uid);
        }

        public bool IsObjectRemoved(string uid)
        {
            if (removed_ids.ContainsKey(uid))
                return removed_ids[uid] > 0;
            return false;
        }

        //---- Hidden objects -----

        public void HideObject(string uid)
        {
            if (!string.IsNullOrEmpty(uid))
                hidden_ids[uid] = 1;
        }

        public void ClearHiddenObject(string uid)
        {
            if (hidden_ids.ContainsKey(uid))
                hidden_ids.Remove(uid);
        }

        public bool IsObjectHidden(string uid)
        {
            if (hidden_ids.ContainsKey(uid))
                return hidden_ids[uid] > 0;
            return false;
        }

        // ---- Unique Ids (Custom data) ----
        public void SetCustomInt(string unique_id, int val)
        {
            if (!string.IsNullOrEmpty(unique_id))
                unique_ints[unique_id] = val;
        }

        public void RemoveCustomInt(string unique_id)
        {
            if (unique_ints.ContainsKey(unique_id))
                unique_ints.Remove(unique_id);
        }

        public int GetCustomInt(string unique_id)
        {
            if (unique_ints.ContainsKey(unique_id))
                return unique_ints[unique_id];
            return 0;
        }

        public bool HasCustomInt(string unique_id)
        {
            return unique_ints.ContainsKey(unique_id);
        }

        public void SetCustomFloat(string unique_id, float val)
        {
            if (!string.IsNullOrEmpty(unique_id))
                unique_floats[unique_id] = val;
        }

        public void RemoveCustomFloat(string unique_id)
        {
            if (unique_floats.ContainsKey(unique_id))
                unique_floats.Remove(unique_id);
        }

        public float GetCustomFloat(string unique_id)
        {
            if (unique_floats.ContainsKey(unique_id))
                return unique_floats[unique_id];
            return 0;
        }

        public bool HasCustomFloat(string unique_id)
        {
            return unique_floats.ContainsKey(unique_id);
        }

        public void SetCustomString(string unique_id, string val)
        {
            if (!string.IsNullOrEmpty(unique_id))
                unique_strings[unique_id] = val;
        }

        public void RemoveCustomString(string unique_id)
        {
            if (unique_strings.ContainsKey(unique_id))
                unique_strings.Remove(unique_id);
        }

        public string GetCustomString(string unique_id)
        {
            if (unique_strings.ContainsKey(unique_id))
                return unique_strings[unique_id];
            return "";
        }

        public bool HasCustomString(string unique_id)
        {
            return unique_strings.ContainsKey(unique_id);
        }

        public void RemoveAllCustom(string unique_id)
        {
            RemoveCustomString(unique_id);
            RemoveCustomFloat(unique_id);
            RemoveCustomInt(unique_id);
        }

        public bool IsNewGame()
        {
            return play_time < 0.0001f;
        }

        public bool IsWorldGenerated()
        {
            return world_seed != 0;
        }

        public float GetTotalTime()
        {
            return (day - 1) * 24f + day_time;
        }

        //--- Save / load -----

        public bool IsVersionValid()
        {
            return version == Application.version;
        }

        public void Save()
        {
            Save(file_loaded, this);
        }

        public static void Save(string filename, SaveData data)
        {
            if (!string.IsNullOrEmpty(filename) && data != null)
            {
                data.filename = filename;
                data.last_save = DateTime.Now;
                data.version = Application.version;
                player_data = data;
                file_loaded = filename;

                SaveSystem.SaveFile<SaveData>(filename, data);
                SaveSystem.SetLastSave(filename);
            }
        }

        public static void NewGame()
        {
            NewGame(GetLastSave()); //default name
        }

        //You should reload the scene right after NewGame
        public static SaveData NewGame(string filename)
        {
            file_loaded = filename;
            player_data = new SaveData(filename);
            player_data.FixData();
            return player_data;
        }

        public static SaveData Load(string filename)
        {
            if (player_data == null || file_loaded != filename)
            {
                player_data = SaveSystem.LoadFile<SaveData>(filename);
                if (player_data != null)
                {
                    file_loaded = filename;
                    player_data.FixData();
                }
            }
            return player_data;
        }

        public static SaveData LoadLast()
        {
            return AutoLoad(GetLastSave());
        }

        //Load if found, otherwise new game
        public static SaveData AutoLoad(string filename)
        {
            if (player_data == null)
                player_data = Load(filename);
            if (player_data == null)
                player_data = NewGame(filename);
            return player_data;
        }

        public static string GetLastSave()
        {
            string name = SaveSystem.GetLastSave();
            if (string.IsNullOrEmpty(name))
                name = "player"; //Default name
            return name;
        }

        public static bool HasLastSave()
        {
            return SaveSystem.DoesFileExist(GetLastSave());
        }

        public static bool HasSave(string filename)
        {
            return SaveSystem.DoesFileExist(filename);
        }

        public static void Unload()
        {
            player_data = null;
            file_loaded = "";
        }

        public static void Delete(string filename)
        {
            if (file_loaded == filename)
            {
                player_data = new SaveData(filename);
                player_data.FixData();
            }

            SaveSystem.DeleteFile(filename);
        }

        public static bool IsLoaded()
        {
            return player_data != null && !string.IsNullOrEmpty(file_loaded);
        }

        public static SaveData Get()
        {
            return player_data;
        }
    }

    [Serializable]
    public class SaveSpawnedData
    {
        public string id;
        public string scene;
        public Vector3Data pos;
        public QuaternionData rot;
        public bool spawned = false;
    }

    [Serializable]
    public class SaveCharacterData
    {
        public string id;
        public string scene;
        public Vector3Data pos;
        public QuaternionData rot;
        public int xp = 0;
        public int gold = 0;
        public bool spawned = false;
    }

    [Serializable]
    public class SaveZoneData
    {
        public string scene;
        public Vector3Data pos;
        public float size;
        public bool spawned = false;
    }

    [Serializable]
    public class SaveConstructionData
    {
        public string id;
        public string scene;
        public Vector3Data pos;
        public QuaternionData rot;
        public bool spawned = false;
        public bool built = true;
        public bool paid = true;
        public float build_progress = 0f;
        public string upgraded_from;
    }

    [Serializable]
    public class SaveItemData
    {
        public string id;
        public string scene;
        public Vector3Data pos;
        public int quantity;
        public bool spawned = false;
    }

    [Serializable]
    public class SaveNPCData
    {
        public string id;
        public string scene;
        public bool spawned = false;
        public string action;
        public string target;
        public Vector3Data tpos;
    }
    
    [Serializable]
    public class SaveColonistData
    {
        public string id;
        public string scene;
        public bool spawned = false;

        public string skin;
        public string name;

        public string action;
        public string target;
        public string work;
        public Vector3Data tpos;
        public float growth;

        public Dictionary<AttributeType, float> attributes = new Dictionary<AttributeType, float>();
        public Dictionary<string, float> xps = new Dictionary<string, float>();
        public Dictionary<string, int> disabled_actions = new Dictionary<string, int>();

        public void FixData()
        {
            //Fix data to make sure old save files compatible with new game version
            if (attributes == null)
                attributes = new Dictionary<AttributeType, float>();
            if (xps == null)
                xps = new Dictionary<string, float>();
            if (disabled_actions == null)
                disabled_actions = new Dictionary<string, int>();
        }

        //--- Colonist Character Attributes ----

        public bool HasAttribute(AttributeType type)
        {
            return attributes.ContainsKey(type);
        }

        public float GetAttributeValue(AttributeType type)
        {
            if (attributes.ContainsKey(type))
                return attributes[type];
            return 0f;
        }

        public void SetAttributeValue(AttributeType type, float value)
        {
            attributes[type] = value;
        }

        public void AddAttributeValue(AttributeType type, float value)
        {
            if (attributes.ContainsKey(type))
                attributes[type] += value;
        }

        public void SetAttributeValue(AttributeType type, float value, float max)
        {
            attributes[type] = Mathf.Clamp(value, 0f, max);
        }

        public void AddAttributeValue(AttributeType type, float value, float max)
        {
            if (attributes.ContainsKey(type))
            {
                attributes[type] += value;
                attributes[type] = Mathf.Clamp(attributes[type], 0f, max);
            }
        }

        //XPS

        public bool HasXP(string id)
        {
            return xps.ContainsKey(id);
        }

        public float GetXP(string id)
        {
            if (xps.ContainsKey(id))
                return xps[id];
            return 0f;
        }

        public int GetXPI(string id)
        {
            return Mathf.FloorToInt(GetXP(id));
        }

        public void SetXP(string id, float value)
        {
            xps[id] = value;
        }

        public void AddXP(string id, float value)
        {
            if (xps.ContainsKey(id))
                xps[id] += value;
        }


        //Valid actions
        public void SetActionEnabled(string action, bool enabled)
        {
            disabled_actions[action] = enabled ? 0 : 1;
        }

        public bool IsActionEnabled(string action)
        {
            if (disabled_actions.ContainsKey(action))
                return disabled_actions[action] == 0;
            return true;
        }
    }

}