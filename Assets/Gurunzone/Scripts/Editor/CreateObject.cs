using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

namespace Gurunzone.EditorTool
{
    public enum CreateObjectType
    {
        None,
        Item,
        Construction,
        NPC,
        Spawnable,
        Selectable
    }

    /// <summary>
    /// Use this tool to easily generate a new object.
    /// It will link the data file to the prefab and attach all necessary components for the type of object you want to create.
    /// </summary>

    public class CreateObject : ScriptableWizard
    {
        [Header("New Object")]
        public string object_title;
        public CreateObjectType type;
        public GameObject mesh;
        public Sprite icon;

        [Header("Items Only")]
        public ItemType item_type;
        public EquipSlot equip_slot;

        [Header("Default Settings")]
        public CreateObjectSettings settings;

        private GameObject last_mesh = null;

        [MenuItem("Gurunzone/Create New Object", priority = 1)]
        static void ScriptableWizardMenu()
        {
            ScriptableWizard.DisplayWizard<CreateObject>("CreateObject", "CreateObject");
        }

        void DoCreateObject()
        {
            if (settings == null)
            {
                Debug.LogError("Settings must not be null");
                return;
            }

            //Find data folder
            string folder = "";
            if (type == CreateObjectType.Item)
                folder = settings.items_folder;
            if (type == CreateObjectType.Construction)
                folder = settings.constructions_folder;

            if (type == CreateObjectType.None)
            {
                Debug.LogError("Type can't be none!");
                return;
            }

            if (mesh == null)
            {
                Debug.LogError("A mesh must be assigned!");
                return;
            }

            if (string.IsNullOrEmpty(object_title.Trim()))
            {
                Debug.LogError("Title can't be blank");
                return;
            }

            //Make sure folder is valid
            string full_folder = Application.dataPath + "/" + folder;
            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(full_folder))
            {
                Debug.LogError("Error, folder can't be found: " + full_folder);
                return;
            }

            //Make sure folder is valid
            string full_folder_equip = Application.dataPath + "/" + settings.prefab_equip_folder;
            if (type == CreateObjectType.Item && item_type == ItemType.Equipment && !Directory.Exists(full_folder_equip))
            {
                Debug.LogError("Error, folder can't be found: " + full_folder_equip);
                return;
            }

            //Make sure file don't already exists
            string file_title = object_title.Replace(" ", "").Replace("/", "");
            string full_file = full_folder + "/" + file_title + ".asset";
            if (!string.IsNullOrEmpty(folder) && File.Exists(full_file))
            {
                Debug.LogError("Error, file already exists: " + full_file);
                return;
            }

            //Make sure prefab don't already exists
            string full_prefab = Application.dataPath + "/" + settings.prefab_folder + "/" + file_title + ".prefab";
            if (File.Exists(full_prefab))
            {
                Debug.LogError("Error, prefab file already exists: " + full_prefab);
                return;
            }

            //---------------

            string file_data = "Assets/" + folder + "/" + file_title + ".asset";
            string file_prefab = "Assets/" + settings.prefab_folder + "/" + file_title + ".prefab";
            string file_prefab_equip = "Assets/" + settings.prefab_equip_folder + "/" + file_title + "Equip.prefab";

            GameObject obj = CreateBasicObject(object_title, mesh);
            Selectable select = obj.GetComponent<Selectable>();
            GameObject prefab = null;

            //Create data
            if (type == CreateObjectType.Item)
            {
                UniqueID uid = obj.AddComponent<UniqueID>();
                Interactable interact = obj.AddComponent<Interactable>();
                Item item = obj.AddComponent<Item>();
                uid.uid_prefix = "item_";
                interact.use_range = 2f;
                select.selection_size = 1f;

                if (item_type == ItemType.Default)
                {
                    ItemData data = CreateCraftAsset<ItemData>(file_data);
                    item.data = data;
                    prefab = CreatePrefab(obj, file_prefab);
                    data.prefab = prefab;
					EditorUtility.SetDirty(data);
                }

                if (item_type == ItemType.Consumable)
                {
                    ItemUseData data = CreateCraftAsset<ItemUseData>(file_data);
                    item.data = data;
                    prefab = CreatePrefab(obj, file_prefab);
                    data.prefab = prefab;
                    data.health = 1;
                    data.hunger = 5;
					EditorUtility.SetDirty(data);
                }

                if (item_type == ItemType.Equipment)
                {
                    ItemEquipData data = CreateCraftAsset<ItemEquipData>(file_data);
                    item.data = data;
                    prefab = CreatePrefab(obj, file_prefab);
                    data.prefab = prefab;

                    GameObject obj_equip = new GameObject(object_title + "Equip");
                    obj_equip.transform.position = FindPosition();

                    GameObject mesh_equip = Instantiate(mesh, obj_equip.transform.position, mesh.transform.rotation);
                    mesh_equip.name = object_title + "Mesh";
                    mesh_equip.transform.SetParent(obj_equip.transform);
                    mesh_equip.transform.localRotation = Quaternion.Euler(90f, 0f, 0f) * mesh.transform.rotation;

                    EquipItem equip_item = obj_equip.AddComponent<EquipItem>();
                    equip_item.data = data;
                    equip_item.slot = equip_slot;

                    data.cargo_size = 1;
                    data.equip_slot = equip_slot;

                    if (equip_slot == EquipSlot.Hand)
                        data.equip_type = EquipType.WeaponMelee;

                    GameObject equip_prefab = CreatePrefab(obj_equip, file_prefab_equip);
                    data.equip_prefab = equip_prefab;
                    DestroyImmediate(obj_equip);
					EditorUtility.SetDirty(data);
                }
				
            }

            else if (type == CreateObjectType.Construction)
            {
                UniqueID uid = obj.AddComponent<UniqueID>();
                Interactable interact = obj.AddComponent<Interactable>();
                Buildable buildable = obj.AddComponent<Buildable>();
                Construction construct = obj.AddComponent<Construction>();
                ConstructionData data = CreateCraftAsset<ConstructionData>(file_data);
                construct.data = data;
                buildable.build_audio = settings.build_audio;
                buildable.build_fx = settings.build_fx;
                construct.built_audio = settings.build_audio;
                construct.built_fx = settings.build_fx;
                construct.built_mesh = last_mesh;
                uid.uid_prefix = "construction_";
                prefab = CreatePrefab(obj, file_prefab);
                data.prefab = prefab;
                interact.use_range = 5f;
                select.selection_size = 5f;
				EditorUtility.SetDirty(data);
            }

            else if (type == CreateObjectType.NPC)
            {
                UniqueID uid = obj.AddComponent<UniqueID>();
                Interactable interact = obj.AddComponent<Interactable>();
                Character character = obj.AddComponent<Character>();
                NPC npc = obj.AddComponent<NPC>();
                obj.AddComponent<CharacterPathfind>();

                NPCData data = CreateCraftAsset<NPCData>(file_data);
                npc.data = data;
                uid.uid_prefix = "npc_";
                prefab = CreatePrefab(obj, file_prefab);
                data.prefab = prefab;
                interact.use_range = 1f;
                select.selection_size = 1f;
				EditorUtility.SetDirty(data);
            }

            else if (type == CreateObjectType.Spawnable)
            {
                UniqueID uid = obj.AddComponent<UniqueID>();
                Interactable interact = obj.AddComponent<Interactable>();
                Spawnable spawnable = obj.AddComponent<Spawnable>();
                SpawnData data = CreateSpawnAsset<SpawnData>(file_data);
                spawnable.data = data;
                uid.uid_prefix = "spawnable_";
                prefab = CreatePrefab(obj, file_prefab);
                data.prefab = prefab;
                interact.use_range = 5f;
                select.selection_size = 5f;
				EditorUtility.SetDirty(data);
            }

            else
            {
                prefab = CreatePrefab(obj, file_prefab);
            }

            if (prefab != null)
                Selection.activeObject = prefab;

            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        }

        private GameObject CreateBasicObject(string otitle, GameObject mesh_prefab)
        {
            //Create object
            GameObject obj = new GameObject(otitle);
            obj.transform.position = FindPosition();

            //Mesh
            GameObject msh = Instantiate(mesh_prefab, obj.transform.position, mesh_prefab.transform.rotation);
            msh.name = otitle + "Mesh";
            msh.transform.SetParent(obj.transform);
            msh.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            last_mesh = msh;

            //Create rigid
            if (type == CreateObjectType.Construction)
            {
                Rigidbody rigid = obj.AddComponent<Rigidbody>();
                rigid.useGravity = false;
                rigid.isKinematic = type == CreateObjectType.Construction;
                rigid.mass = 100f;
                rigid.drag = 5f;
                rigid.angularDrag = 5f;
                rigid.interpolation = RigidbodyInterpolation.None;
                rigid.constraints = RigidbodyConstraints.FreezeAll;
            }

            //Collider
            SphereCollider collide = obj.AddComponent<SphereCollider>();
            collide.isTrigger = true;
            collide.radius = 0.4f;
            collide.center = new Vector3(0f, 0.2f, 0f);

            //Create selectable
            Selectable select = obj.AddComponent<Selectable>();
            //select.generate_outline = true;
            //select.outline_material = settings.outline;

            return obj;
        }

        private GameObject CreatePrefab(GameObject obj, string file)
        {
            GameObject prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(obj, file, InteractionMode.AutomatedAction);
            prefab.transform.position = Vector3.zero;
            return prefab;
        }

        private T CreateCraftAsset<T>(string file) where T : CraftData
        {
            T asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, file);
            asset.id = object_title.ToLower().Replace(" ", "_");
            asset.title = object_title;
            asset.icon = icon;
            return asset;
        }

        private T CreateSpawnAsset<T>(string file) where T : SpawnData
        {
            T asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, file);
            asset.id = object_title.ToLower().Replace(" ", "_");
            asset.title = object_title;
            return asset;
        }

        private Vector3 FindPosition()
        {
            if (SceneView.lastActiveSceneView != null && SceneView.lastActiveSceneView.camera != null)
            {
                Camera cam = SceneView.lastActiveSceneView.camera;
                Ray ray = new Ray(cam.transform.position, cam.transform.forward);
                Plane plane = new Plane(Vector3.up, 0f);
                float dist;
                bool success = plane.Raycast(ray, out dist);
                if (success)
                {
                    Vector3 pos = ray.origin + ray.direction * dist;
                    return new Vector3(pos.x, 0f, pos.z);
                }
            }
            return Vector3.zero;
        }

        void OnWizardCreate()
        {
            DoCreateObject();
        }

        void OnWizardUpdate()
        {
            helpString = "Use this tool to create a new prefab and its data file.";
        }
    }

}