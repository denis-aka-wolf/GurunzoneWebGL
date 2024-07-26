using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

namespace Gurunzone.EditorTool
{
    /// <summary>
    /// Use this tool to easily dupplicate a CraftData object and all links
    /// </summary>

    public class DuplicateObject : ScriptableWizard
    {
        [Header("New Object")]
        public CSData source;
        public string object_title;

        private Dictionary<int, string> copied_prefabs = new Dictionary<int, string>();

        [MenuItem("Gurunzone/Duplicate Object", priority = 2)]
        static void ScriptableWizardMenu()
        {
            ScriptableWizard.DisplayWizard<DuplicateObject>("DuplicateObject", "DuplicateObject");
        }

        void DoDuplicateObject()
        {
            if (source == null)
            {
                Debug.LogError("A source must be assigned!");
                return;
            }

            if (string.IsNullOrEmpty(object_title.Trim()))
            {
                Debug.LogError("Title can't be blank");
                return;
            }
;
            copied_prefabs.Clear();

            if (source is ItemData)
            {
                ItemData nitem = null;
                if (source is ItemUseData)
                {
                    nitem = CopyCraftAsset<ItemUseData>((ItemUseData)source, object_title);
                }
                else if (source is ItemEquipData)
                {
                    nitem = CopyCraftAsset<ItemEquipData>((ItemEquipData)source, object_title);
                }
                else
                {
                    nitem = CopyCraftAsset<ItemData>((ItemData)source, object_title);
                }

                if (nitem != null && nitem.prefab != null)
                {
                    GameObject nprefab = CopyPrefab(nitem.prefab, object_title);
                    nitem.prefab = nprefab;
                    Item item = nprefab.GetComponent<Item>();
                    if (item != null)
                        item.data = nitem;
                }

                if (nitem != null && nitem is ItemEquipData)
                {
                    ItemEquipData eitem = (ItemEquipData)nitem;
                    if (eitem.equip_prefab != null)
                    {
                        GameObject nprefab = CopyPrefab(eitem.equip_prefab, object_title + "Equip");
                        eitem.equip_prefab = nprefab;
                        EquipItem item = nprefab.GetComponent<EquipItem>();
                        if (item != null)
                            item.data = eitem;
                    }
                }

                Selection.activeObject = nitem;
            }

            if (source is ColonistData)
            {
                ColonistData nitem = CopyCraftAsset<ColonistData>((ColonistData)source, object_title);
                Selection.activeObject = nitem;
            }

            if (source is NPCData)
            {
                NPCData nitem = CopyCraftAsset<NPCData>((NPCData)source, object_title);
                Selection.activeObject = nitem;
            }

            if (source is ConstructionData)
            {
                ConstructionData nitem = CopyCraftAsset<ConstructionData>((ConstructionData)source, object_title);

                if (nitem != null && nitem.prefab != null)
                {
                    GameObject nprefab = CopyPrefab(nitem.prefab, object_title);
                    nitem.prefab = nprefab;
                    Construction construct = nprefab.GetComponent<Construction>();
                    if (construct != null)
                        construct.data = nitem;
                }

                Selection.activeObject = nitem;
            }

            if (source is SpawnData)
            {
                SpawnData nitem = CopySpawnAsset<SpawnData>((SpawnData)source, object_title);
                if (nitem.prefab != null)
                {
                    GameObject nprefab = CopyPrefab(nitem.prefab, object_title);
                    nitem.prefab = nprefab;
                    Spawnable spawnable = nprefab.GetComponent<Spawnable>();
                    if (spawnable != null)
                        spawnable.data = nitem;
                }
            }

            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        }

        private GameObject CopyPrefab(GameObject prefab, string title)
        {
            if (prefab != null)
            {
                //Fast access to already copied prefab
                if (copied_prefabs.ContainsKey(prefab.GetInstanceID()))
                {
                    string cpath = copied_prefabs[prefab.GetInstanceID()];
                    GameObject cprefab = AssetDatabase.LoadAssetAtPath<GameObject>(cpath);
                    if(cprefab != null)
                        return cprefab;
                }

                //Otherwise copy
                string path = AssetDatabase.GetAssetPath(prefab);
                string folder = Path.GetDirectoryName(path);
                string ext = Path.GetExtension(path);
                string filename = title.Replace(" ", "").Replace("/", "");
                string npath = folder + "/" + filename + ext;

                if (!Directory.Exists(folder))
                {
                    Debug.LogError("Folder does not exist: " + folder);
                    return null;
                }

                if (File.Exists(npath))
                {
                    Debug.LogError("File already exists: " + npath);
                    return null;
                }

                AssetDatabase.CopyAsset(path, npath);
                GameObject nprefab = AssetDatabase.LoadAssetAtPath<GameObject>(npath);
                if (nprefab != null)
                {
                    nprefab.name = filename;
                    copied_prefabs[prefab.GetInstanceID()] = npath;
                    return nprefab;
                }
            }
            return null;
        }

        private T CopySpawnAsset<T>(T asset, string title) where T : SpawnData
        {
            string filename = GetFileName(title);
            string fileid = GetFileID(title);
            T nasset = CopyAsset(asset, title);
            if (nasset != null)
            {
                nasset.name = filename;
                nasset.title = title;
                nasset.id = fileid;
                return nasset;
            }
            return null;
        }

        private T CopyCraftAsset<T>(T asset, string title) where T : CraftData
        {
            string filename = GetFileName(title); 
            string fileid = GetFileID(title);
            T nasset = CopyAsset(asset, title);
            if (nasset != null)
            {
                nasset.name = filename;
                nasset.title = title;
                nasset.id = fileid;
                return nasset;
            }
            return null;
        }

        private T CopyAsset<T>(T asset, string title) where T : CSData
        {
            if (asset != null)
            {
                string path = AssetDatabase.GetAssetPath(asset);
                string folder = Path.GetDirectoryName(path);
                string ext = Path.GetExtension(path);
                string filename = GetFileName(title);
                string npath = folder + "/" + filename + ext;

                if (!Directory.Exists(folder))
                {
                    Debug.LogError("Folder does not exist: " + folder);
                    return null;
                }

                if (File.Exists(npath))
                {
                    Debug.LogError("File already exists: " + npath);
                    return null;
                }

                AssetDatabase.CopyAsset(path, npath);
                T nasset = AssetDatabase.LoadAssetAtPath<T>(npath);
                return nasset;
            }
            return null;
        }

        private string GetFileID(string title)
        {
            return title.Trim().Replace(" ", "_").ToLower();
        }

        private string GetFileName(string title)
        {
            return title.Replace(" ", "").Replace("/", "");
        }

        void OnWizardCreate()
        {
            DoDuplicateObject();
        }

        void OnWizardUpdate()
        {
            helpString = "Use this tool to duplicate a prefab and its data file.";
        }
    }

}