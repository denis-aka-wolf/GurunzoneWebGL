using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{

    /// <summary>
    /// Base class for all Colony Simulator data (items, construction, colonist..)
    /// </summary>

    public abstract class CSData : ScriptableObject {}

    [System.Serializable]
    public class ItemSet
    {
        public ItemData item;
        public int quantity;
    }

    [System.Serializable]
    public class GroupSet
    {
        public GroupData group;
        public int quantity;
    }

    /// <summary>
    /// Base class for any craftables (has cost and requirements)
    /// </summary>

    public abstract class CraftData : CSData
    {
        public string id;       //id (for the save file)
        public string title;    //Display title
        public Sprite icon;     //Display icon
        public int sort_order;  //Order that the icons will appear in UI (for example in the build menu)

        [TextArea(5, 7)]
        public string desc;

        [Header("Prefab")]
        public GameObject prefab;   //Prefab spawned when creating/crafting this object

        [Header("Crafting")]
        public bool craftable = true;           //Can be built or crafted
        public ItemSet[] craft_items;           //Items required to craft
        public GroupSet[] craft_items_groups;   //Items required to craft (by group, so can be any filler item in that group)
        public CraftData[] craft_requirements;  //Constructions or Tech that the player must have to craft this
        public float craft_duration = 1f;   //In game hours

        [Header("Groups")]
        public GroupData[] groups;          //Groups associated with this 

        private static List<CraftData> clist = new List<CraftData>();

        public bool HasGroup(GroupData group)
        {
            if (group == null)
                return true;

            foreach (GroupData grp in groups)
            {
                if (grp == group)
                    return true;
            }
            return false;
        }

        public CraftCostData GetCraftCost()
        {
            CraftCostData cost = new CraftCostData();

            foreach (ItemSet item in craft_items)
            {
                if (item?.item != null)
                {
                    if (cost.items.ContainsKey(item.item))
                        cost.items[item.item] += item.quantity;
                    else
                        cost.items[item.item] = item.quantity;
                }
            }

            foreach (GroupSet group in craft_items_groups)
            {
                if (group?.group != null)
                {
                    if (cost.fillers.ContainsKey(group.group))
                        cost.fillers[group.group] += group.quantity;
                    else
                        cost.fillers[group.group] = group.quantity;
                }
            }

            foreach (CraftData requirement in craft_requirements)
            {
                if (requirement != null)
                {
                    if (cost.requirements.ContainsKey(requirement))
                        cost.requirements[requirement] += 1;
                    else
                        cost.requirements[requirement] = 1;
                }
            }

            return cost;
        }

        public bool HasCraftCost()
        {
            Inventory global = Inventory.GetGlobal();
            CraftCostData cost = GetCraftCost();
            bool has_cost = true;
            foreach (KeyValuePair<ItemData, int> pair in cost.items)
            {
                if (!global.HasItem(pair.Key, pair.Value))
                    has_cost = false;
            }
            foreach (KeyValuePair<GroupData, int> pair in cost.fillers)
            {
                if (!global.HasItemGroup(pair.Key, pair.Value))
                    has_cost = false;
            }
            return has_cost;
        }

        public bool HasRequirements()
        {
            CraftCostData cost = GetCraftCost();
            bool has_requirements = true;
            foreach (KeyValuePair<CraftData, int> item in cost.requirements)
            {
                if (item.Key is TechData)
                {
                    if (!TechManager.Get().IsTechCompleted((TechData)item.Key))
                        has_requirements = false;
                }
                if (item.Key is ConstructionData)
                {
                    if (Construction.CountConstructions((ConstructionData)item.Key) < item.Value)
                        has_requirements = false;
                }
            }
            if (this is ConstructionData)
            {
                ConstructionData construct = (ConstructionData)this;
                if (construct.build_limit > 0 && Construction.CountConstructions((ConstructionData)this) >= construct.build_limit)
                    has_requirements = false;
            }
            return has_requirements;
        }

        public static void Load(string folder = "")
        {
            clist.Clear();
            clist.AddRange(Resources.LoadAll<CraftData>(folder));
            clist.Sort((CraftData a, CraftData b) => {
                if (a.sort_order == b.sort_order)
                    return a.title.CompareTo(b.title);
                return a.sort_order.CompareTo(b.sort_order);
            });
        }

        public static CraftData Get(string id)
        {
            foreach (CraftData data in clist)
            {
                if (data.id == id)
                    return data;
            }
            return null;
        }

        public static List<CraftData> GetAll()
        {
            return clist;
        }
    }

    public class CraftCostData
    {
        public Dictionary<ItemData, int> items = new Dictionary<ItemData, int>();
        public Dictionary<GroupData, int> fillers = new Dictionary<GroupData, int>();
        public Dictionary<CraftData, int> requirements = new Dictionary<CraftData, int>();
    }
}
