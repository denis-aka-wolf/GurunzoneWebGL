using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    /// <summary>
    /// Gatherables are resources that can be harvested by colonists, and depleted.
    /// </summary>


    [RequireComponent(typeof(Selectable))]
    [RequireComponent(typeof(Interactable))]
    [RequireComponent(typeof(Workable))]
    [RequireComponent(typeof(UniqueID))]
    public class Gatherable : MonoBehaviour
    {
        public ItemData item;       //Item collected at this resource.
        public int value = 1000;    //Total quantity of this resource.

        [Header("Harvest")]
        public bool harvestable = true;     //If false, require a building to harvest (like building a Oil Pump on top of it)
        public float harvest_speed = 10f;   //Amount per game-hour harvested
        public GroupData harvest_tool;
        public bool tool_required = false; //If true, wont be able to harvest without harvest_tool equipped

        [Header("Loot")]
        public CSData[] loots;    //Additional item that could be found

        [Header("Anims")]
        public string harvest_anim;
        public AudioClip harvest_audio;
        public float destroy_delay = 0.5f;

        protected Selectable select;
        protected Interactable interact;
        protected Workable workable;
        protected Construction construction; //Can be null
        protected UniqueID uid;
        protected Transform transf; //Optimization
        protected int initial_value = 0;
        protected bool destroyed = false;

        private static List<Gatherable> resource_list = new List<Gatherable>();

        protected virtual void Awake()
        {
            resource_list.Add(this);
            select = GetComponent<Selectable>();
            interact = GetComponent<Interactable>();
            workable = GetComponent<Workable>();
            construction = GetComponent<Construction>();
            uid = GetComponent<UniqueID>();
            transf = transform;
            initial_value = value;
        }

        protected virtual void OnDestroy()
        {
            resource_list.Remove(this);
        }

        protected virtual void Start()
        {
            if (uid.HasInt("value"))
                value = uid.GetInt("value");
        }

        public virtual void AddValue(int value)
        {
            this.value += value;
            uid.SetInt("value", this.value);

            if (this.value <= 0)
                Kill();
        }

        public virtual void Kill()
        {
            destroyed = true;
            select.Kill(destroy_delay);
        }

        public virtual int GetValue()
        {
            return value;
        }

        public virtual int GetMaxValue()
        {
            return initial_value;
        }

        public virtual bool IsAlive()
        {
            return !destroyed && gameObject != null && gameObject.activeSelf;
        }

        public virtual ItemData GetHarvestItem()
        {
            if (loots.Length > 0)
            {
                CSData loot = loots[Random.Range(0, loots.Length)];
                if (loot is ItemData)
                {
                    return (ItemData)loot;
                }
                if (loot is LootData)
                {
                    LootData ldata = (LootData)loot;
                    ItemData idata = ldata.GetRandomItem();
                    if (idata != null)
                    {
                        return idata;
                    }
                }
            }
            return item;
        }

        public virtual ItemData GetItem()
        {
            return item;
        }

        public bool CanHarvest()
        {
            bool valid = construction == null || construction.IsCompleted();
            return valid && harvestable && GetValue() > 0;
        }

        public bool CanHarvest(Character character)
        {
            bool tool = !tool_required || character.Equip.HasEquip(harvest_tool);
            return tool && CanHarvest();
        }

        public Selectable Selectable { get { return select; } }
        public Interactable Interactable { get { return interact; } }
        public Workable Workable { get { return workable; } }

        public static Gatherable GetNearestUnassigned(Vector3 pos, float range = 999f)
        {
            float min_dist = range;
            Gatherable nearest = null;
            foreach (Gatherable gather in resource_list)
            {
                float dist = (pos - gather.transf.position).magnitude;
                if (dist < min_dist && gather.CanHarvest() && gather.Workable.CanBeAssigned())
                {
                    min_dist = dist;
                    nearest = gather;
                }
            }
            return nearest;
        }


        public static Gatherable GetNearest(Vector3 pos, float range = 999f, Gatherable ignore = null)
        {
            float min_dist = range;
            Gatherable nearest = null;
            foreach (Gatherable gather in resource_list)
            {
                float dist = (pos - gather.transf.position).magnitude;
                if (dist < min_dist && gather.IsAlive() && gather != ignore)
                {
                    min_dist = dist;
                    nearest = gather;
                }
            }
            return nearest;
        }

        public static Gatherable GetNearest(ItemData item, Vector3 pos, float range = 999f, Gatherable ignore = null)
        {
            float min_dist = range;
            Gatherable nearest = null;
            foreach (Gatherable gather in resource_list)
            {
                float dist = (pos - gather.transf.position).magnitude;
                if (dist < min_dist && item == gather.item && gather.IsAlive() && gather != ignore)
                {
                    min_dist = dist;
                    nearest = gather;
                }
            }
            return nearest;
        }

        public static Gatherable GetNearest(GroupData group, Vector3 pos, float range = 999f, Gatherable ignore = null)
        {
            float min_dist = range;
            Gatherable nearest = null;
            foreach (Gatherable gather in resource_list)
            {
                float dist = (pos - gather.transf.position).magnitude;
                if (dist < min_dist && group == gather.Selectable.HasGroup(group) && gather.IsAlive() && gather != ignore)
                {
                    min_dist = dist;
                    nearest = gather;
                }
            }
            return nearest;
        }

        public static List<Gatherable> GetAll()
        {
            return resource_list;
        }
    }
}
