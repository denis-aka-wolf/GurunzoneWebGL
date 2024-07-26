using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    public enum ZoneType
    {
        None=0,
        Harvest=10,
    }

    /// <summary>
    /// Zones are areas defined by the player where colonist will go work automatically. 
    /// 
    /// </summary>

    [RequireComponent(typeof(Selectable))]
    [RequireComponent(typeof(Interactable))]
    [RequireComponent(typeof(Buildable))]
    [RequireComponent(typeof(Workable))]
    [RequireComponent(typeof(UniqueID))]
    public class Zone : MonoBehaviour
    {
        public ZoneType type;           //Zone type (not implemented yet, for now they are all harvest zones)
        public GameObject zone_circle;  //Visual circle FX
        public float size_min = 10f;    //Minimum size (radius)
        public float size_max = 100f;   //Maximum size (radius)

        private Selectable select;
        private Interactable interact;
        private Buildable buildable;
        private Workable workable;
        private UniqueID uid;
        
        private float zone_size = 40f;
        private ItemData priority_item = null;

        private List<Selectable> active_in_zone = new List<Selectable>();
        private List<Destructible> destructibles_in_zone = new List<Destructible>();
        private List<Gatherable> gatherables_in_zone = new List<Gatherable>();
        private List<Item> items_in_zone = new List<Item>();

        private float update_timer = 0f;

        private static List<Zone> zone_list = new List<Zone>();

        private void Awake()
        {
            zone_list.Add(this);
            select = GetComponent<Selectable>();
            interact = GetComponent<Interactable>();
            buildable = GetComponent<Buildable>();
            workable = GetComponent<Workable>();
            uid = GetComponent<UniqueID>();
            buildable.onBuild += OnPlaced;
            select.onRemove += OnRemove;
        }

        private void OnDestroy()
        {
            zone_list.Remove(this);
        }

        private void Start()
        {
            string priorty_id = uid.GetString("priority");
            ItemData idata = ItemData.Get(priorty_id);
            SetPriority(idata);

            SlowUpdate();
        }

        void Update()
        {
            //Set circle visible
            bool visible = buildable.IsBuilding() || select.IsSelected();
            if (visible != zone_circle.activeSelf)
                zone_circle.SetActive(visible);

            //Change position and size
            if (buildable.IsBuilding())
            {
                TheControls controls = TheControls.Get();
                zone_size += -controls.GetMouseScroll() * size_max * 2f * Time.deltaTime;
                zone_size = Mathf.Clamp(zone_size, size_min, size_max);
                UpdateCircleSize();
            }

            //Update outlines
            if (active_in_zone.Count > 0)
            {
                foreach (Selectable select in active_in_zone)
                    select.SetOutline(false);
                active_in_zone.Clear();
            }

            if (visible)
            {
                foreach (Selectable select in Selectable.GetAllActive())
                {
                    if (IsInsideZone(select.transform.position))
                    {
                        active_in_zone.Add(select);
                        select.SetOutline(true);
                    }
                }
            }

            update_timer += Time.deltaTime;
            if (update_timer > 1f)
            {
                update_timer = 0f;
                SlowUpdate();
            }
        }

        private void SlowUpdate()
        {
            gatherables_in_zone.Clear();
            foreach (Gatherable gather in Gatherable.GetAll())
            {
                if (IsInsideZone(gather.transform.position))
                    gatherables_in_zone.Add(gather);
            }

            destructibles_in_zone.Clear();
            foreach (Destructible destruct in Destructible.GetAll())
            {
                if (destruct.CanBeAttacked() && !destruct.IsAlly() && IsInsideZone(destruct.transform.position))
                    destructibles_in_zone.Add(destruct);
            }

            items_in_zone.Clear();
            foreach (Item item in Item.GetAll())
            {
                if (IsInsideZone(item.transform.position))
                    items_in_zone.Add(item);
            }

            bool found_item = false;
            foreach (Gatherable gather in gatherables_in_zone)
            {
                if (gather.item == priority_item)
                    found_item = true;
            }
            foreach (Item item in items_in_zone)
            {
                if (item.data == priority_item)
                    found_item = true;
            }

            if (!found_item && priority_item != null)
                SetPriority(null);
        }

        public void Kill()
        {
            select.Kill();
        }

        private void OnRemove()
        {
            foreach (Selectable select in active_in_zone)
                select.SetOutline(false);
        }

        private void OnPlaced()
        {
            SaveZoneData zdata = SData;
            zdata.scene = SceneNav.GetCurrentScene();
            zdata.pos = transform.position;
            zdata.spawned = true;
            zdata.size = zone_size;

            workable.SetWorkerAmount(1);
            Selectable.Select();
        }

        private void UpdateCircleSize()
        {
            zone_circle.transform.localScale = new Vector3(zone_size, 1f, zone_size);
        }

        public void SetPriority(ItemData item)
        {
            priority_item = item;
            uid.SetString("priority", item != null ? item.id : "");
        }

        public bool IsInsideZone(Vector3 pos)
        {
            pos.y = transform.position.y;
            Vector3 dist = (transform.position - pos);
            return dist.magnitude < zone_size;
        }

        public float GetSize()
        {
            return zone_size;
        }

        public ItemData GetPriority()
        {
            return priority_item;
        }

        public bool CanBeGathered()
        {
            bool has_items = CountGatherablesInZone() > 0 || CountItemsInZone() > 0 || CountDestructiblesInZone() > 0;
            return has_items;
        }

        public bool CanBeGathered(Character character)
        {
            bool has_items = CountGatherablesInZone(character) > 0 || CountItemsInZone() > 0 || CountDestructiblesInZone() > 0;
            return has_items;
        }

        public Item GetNearestItem(Vector3 pos, float range = 999f)
        {
            float min_dist = range;
            Item nearest = null;
            foreach (Item item in items_in_zone)
            {
                if (item != null)
                {
                    if (priority_item == null || priority_item == item.data)
                    {
                        float dist = (pos - item.transform.position).magnitude;
                        if (dist < min_dist && !item.Interactable.IsInteractFull())
                        {
                            min_dist = dist;
                            nearest = item;
                        }
                    }
                }
            }
            return nearest;
        }

        public Gatherable GetNearestGatherable(Character character, Vector3 pos, float range = 999f)
        {
            float min_dist = range;
            Gatherable nearest = null;
            foreach (Gatherable gather in gatherables_in_zone)
            {
                if (gather != null && gather.CanHarvest(character))
                {
                    if (priority_item == null || priority_item == gather.item)
                    {
                        float dist = (pos - gather.transform.position).magnitude;
                        if (dist < min_dist && !gather.Interactable.IsInteractFull())
                        {
                            min_dist = dist;
                            nearest = gather;
                        }
                    }
                }
            }
            return nearest;
        }

        public Destructible GetNearestDestructible(Vector3 pos, float range = 999f)
        {
            float min_dist = range;
            Destructible nearest = null;
            foreach (Destructible destruct in destructibles_in_zone)
            {
                if (destruct != null)
                {
                    float dist = (pos - destruct.transform.position).magnitude;
                    if (dist < min_dist && !destruct.Interactable.IsInteractFull())
                    {
                        min_dist = dist;
                        nearest = destruct;
                    }
                }
            }
            return nearest;
        }

        public int CountGatherablesInZone()
        {
            return gatherables_in_zone.Count;
        }

        public int CountDestructiblesInZone()
        {
            return destructibles_in_zone.Count;
        }

        public int CountItemsInZone()
        {
            return items_in_zone.Count;
        }

        public int CountAllInZone()
        {
            return gatherables_in_zone.Count + destructibles_in_zone.Count + items_in_zone.Count;
        }


        public int CountGatherablesInZone(Character character)
        {
            int count = 0;
            foreach (Gatherable gather in gatherables_in_zone)
            {
                if (gather.CanHarvest(character))
                    count++;
            }
            return count;
        }


        public List<Selectable> GetActiveInZone()
        {
            return active_in_zone;
        }

        public List<Gatherable> GetGatherablesInZone()
        {
            return gatherables_in_zone;
        }

        public List<Destructible> GetDestructiblesInZone()
        {
            return destructibles_in_zone;
        }

        public List<Item> GetItemsInZone()
        {
            return items_in_zone;
        }

        public Selectable Selectable { get { return select; } }
        public Interactable Interactable { get { return interact; } }
        public Workable Workable { get { return workable; } }
        public SaveZoneData SData { get { return SaveData.Get().GetZone(uid.uid); } } //SData is the saved data linked to this object

        public static Zone GetNearestUnassigned(Vector3 pos, float range = 999f)
        {
            float min_dist = range;
            Zone nearest = null;
            foreach (Zone zone in zone_list)
            {
                float dist = (pos - zone.transform.position).magnitude;
                if (zone.CanBeGathered() && zone.Workable.CanBeAssigned() && dist < min_dist)
                {
                    min_dist = dist;
                    nearest = zone;
                }
            }
            return nearest;
        }

        public static Zone Spawn(string uid, SaveZoneData data)
        {
            if (data.scene == SceneNav.GetCurrentScene())
            {
                GameObject obj = Instantiate(AssetData.Get().zone_prefab, data.pos, AssetData.Get().zone_prefab.transform.rotation);
                Zone zone = obj.GetComponent<Zone>();
                zone.zone_size = data.size;
                UniqueID uniqueid = obj.GetComponent<UniqueID>();
                uniqueid.uid = uid;
                zone.UpdateCircleSize();
                return zone;
            }
            return null;
        }

        public static Zone Create(ZoneType type, Vector3 pos, float size)
        {
            GameObject prefab = AssetData.Get().zone_prefab;
            GameObject obj = Instantiate(prefab, pos, prefab.transform.rotation);
            Zone zone = obj.GetComponent<Zone>();
            zone.type = type;
            zone.zone_size = size;
            UniqueID uniqueid = obj.GetComponent<UniqueID>();
            uniqueid.uid = UniqueID.GenerateUniqueID();
            SaveData sdata = SaveData.Get();
            SaveZoneData zdata = sdata.GetZone(uniqueid.uid);
            zdata.pos = pos;
            zdata.size = size;
            zdata.spawned = true;
            return zone;
        }

        public static Zone CreateBuildMode(ZoneType type)
        {
            Vector3 pos = TheControls.Get().GetMouseWorldPos();
            Zone zone = Create(type, pos, 1f);
            Buildable build = zone.GetComponent<Buildable>();
            build.StartBuild();
            return zone;
        }

        public static List<Zone> GetAll()
        {
            return zone_list;
        }
    }
}
