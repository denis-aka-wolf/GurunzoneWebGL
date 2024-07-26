using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    /// <summary>
    /// Manages character equipment, and display them
    /// </summary>

    [RequireComponent(typeof(Character))]
    [RequireComponent(typeof(Inventory))]
    [RequireComponent(typeof(UniqueID))]
    public class CharacterEquip : MonoBehaviour
    {
        public ItemEquipData[] starting_items; //Item that start equipped
        public ItemEquipData[] default_items; //Not really existing in inventory, permanently available
        
        private Character character;
        private Inventory inventory;
        private UniqueID uid;
        private Animator animator;
        private EquipAttach[] equip_attachments;
        private AttachFX[] attachments_fx;

        private Dictionary<string, EquipItem> visible_items = new Dictionary<string, EquipItem>();
        private HashSet<GroupData> active_tools = new HashSet<GroupData>();

        private AttachFX carry_mesh;

        void Awake()
        {
            character = GetComponent<Character>();
            inventory = GetComponent<Inventory>();
            animator = GetComponentInChildren<Animator>();
            uid = GetComponent<UniqueID>();
            equip_attachments = GetComponentsInChildren<EquipAttach>();
            attachments_fx = GetComponentsInChildren<AttachFX>(true);
            carry_mesh = GetAttachFX("carry");

            if (carry_mesh != null)
                carry_mesh.gameObject.SetActive(false);
        }

        private void Start()
        {
            if (!SaveData.Get().HasInventory(GetEquipUID()))
            {
                foreach (ItemEquipData item in starting_items)
                    EquipItem(item);
            }
        }

        void Update()
        {
            if (character == null || character.Inventory == null)
                return;

            HashSet<string> equipped_data = new HashSet<string>();
            HashSet<EquipSlot> equipped_slots = new HashSet<EquipSlot>();
            List<string> remove_list = new List<string>();

            //Show/Hide default
            foreach (GroupData tool_group in GetActiveTools())
            {
                ItemEquipData eitem = GetEquippedItem(tool_group);
                ItemEquipData default_item = GetDefaultItem(tool_group);
                if (eitem != null && eitem.Type == ItemType.Equipment && !equipped_slots.Contains(eitem.equip_slot))
                {
                    equipped_data.Add(eitem.id);
                    equipped_slots.Add(default_item.equip_slot);
                    AddVisibleItem(eitem.id);
                }
                else if (default_item != null && default_item.Type == ItemType.Equipment && !equipped_slots.Contains(default_item.equip_slot))
                {
                    equipped_data.Add(default_item.id);
                    equipped_slots.Add(default_item.equip_slot);
                    AddVisibleItem(default_item.id);
                }
            }

            //Show/Hide equipped items
            foreach (ItemSet item in GetEquippedItems())
            {
                if (item != null && item.item != null && item.item.Type == ItemType.Equipment)
                {
                    ItemEquipData eitem = (ItemEquipData)item.item;
                    if (!equipped_slots.Contains(eitem.equip_slot))
                    {
                        equipped_data.Add(eitem.id);
                        equipped_slots.Add(eitem.equip_slot);
                        AddVisibleItem(eitem.id);
                    }
                }
            }

            //Create remove list
            foreach (KeyValuePair<string, EquipItem> item in visible_items)
            {
                if (!equipped_data.Contains(item.Key))
                    remove_list.Add(item.Key);
            }

            //Remove
            foreach (string item_id in remove_list)
            {
                RemoveVisibleItem(item_id);
            }

            //Anims
            bool has_item = character.Inventory != null && character.Inventory.CountItems() > 0 
                && !character.IsCustomAnim() && !character.IsFighting();
            animator?.SetBool("carry", has_item);
            if (carry_mesh != null && carry_mesh.gameObject.activeSelf != has_item)
                carry_mesh.gameObject.SetActive(has_item);
        }

        public void EquipItem(ItemEquipData item)
        {
            EquipItem(inventory, item);
        }

        public void UnequipItem(ItemEquipData item)
        {
            UnequipItem(inventory, item);
        }

        public void UnequipItem(EquipSlot slot)
        {
            UnequipItem(inventory, slot);
        }

        //Equip FROM a specific inventory
        public void EquipItem(Inventory inventory, ItemEquipData item)
        {
            if (item != null && item.equip_slot != EquipSlot.None && inventory.HasItem(item))
            {
                UnequipItem(inventory, item.equip_slot); //Cant have another item of same slot
                int stack = GetEquipStack(inventory.SData, item);
                InventoryData einventory = EquipData;
                einventory.AddItem(item.id, stack);
                inventory.AddItem(item, -stack);
            }
        }

        //Unequip TO a specific inventory
        public void UnequipItem(Inventory inventory, ItemEquipData item)
        {
            InventoryData einventory = EquipData;
            if (item != null && einventory.HasItem(item.id))
            {
                int stack = GetEquipStack(einventory, item);
                einventory.AddItem(item.id, -stack);
                inventory.AddItem(item, stack);
            }
        }

        //Unequip slot TO a specific inventory
        public void UnequipItem(Inventory inventory, EquipSlot slot)
        {
            ItemEquipData edata = GetEquippedItem(slot);
            UnequipItem(inventory, edata);
        }

        public bool HasEquip(GroupData group)
        {
            foreach (KeyValuePair<string, int> equip in EquipData.items)
            {
                ItemEquipData edata = ItemEquipData.Get(equip.Key);
                if (edata != null && edata.HasGroup(group))
                    return true;
            }
            return false;
        }

        public List<ItemSet> GetEquippedItems()
        {
            List<ItemSet> items = new List<ItemSet>();
            foreach (KeyValuePair<string, int> pair in EquipData.items)
            {
                ItemData idata = ItemData.Get(pair.Key);
                if (idata != null)
                {
                    ItemSet set = new ItemSet();
                    set.item = idata;
                    set.quantity = pair.Value;
                    items.Add(set);
                }
            }
            return items;
        }

        private void AddVisibleItem(string item_id)
        {
            if (visible_items.ContainsKey(item_id))
                return;

            ItemEquipData idata = ItemEquipData.Get(item_id);
            if (idata != null && idata.equip_prefab != null)
            {
                GameObject equip_obj = Instantiate(idata.equip_prefab, transform.position, Quaternion.identity);
                EquipItem eitem = equip_obj.GetComponent<EquipItem>();
                if (eitem != null)
                {
                    eitem.data = idata;
                    eitem.target = GetEquipAttachment(eitem.slot);
                    if (eitem.child_left != null)
                        eitem.target_left = GetEquipAttachment(eitem.slot);
                    if (eitem.child_right != null)
                        eitem.target_right = GetEquipAttachment(eitem.slot);
                }
                visible_items.Add(item_id, eitem);
            }
        }

        private void RemoveVisibleItem(string item_id)
        {
            if (visible_items.ContainsKey(item_id))
            {
                EquipItem eitem = visible_items[item_id];
                visible_items.Remove(item_id);
                if (eitem != null)
                    Destroy(eitem.gameObject);
            }
        }

        public bool HasEquippedItem(ItemData item)
        {
            InventoryData einventory = EquipData;
            foreach (KeyValuePair<string, int> pair in einventory.items)
            {
                ItemData idata = ItemData.Get(pair.Key);
                if (idata == item)
                    return true;
            }
            return false;
        }

        public ItemEquipData GetEquippedItem(EquipSlot slot)
        {
            InventoryData einventory = EquipData;
            foreach (KeyValuePair<string, int> item in einventory.items)
            {
                ItemData idata = ItemData.Get(item.Key);
                if (idata is ItemEquipData)
                {
                    ItemEquipData edata = (ItemEquipData)idata;
                    if (edata.equip_slot == slot)
                        return edata;
                }
            }
            return null;
        }

        public ItemEquipData GetEquippedItem(GroupData group)
        {
            InventoryData einventory = EquipData;
            foreach (KeyValuePair<string, int> item in einventory.items)
            {
                ItemData idata = ItemData.Get(item.Key);
                if (idata is ItemEquipData)
                {
                    ItemEquipData edata = (ItemEquipData)idata;
                    if (edata.HasGroup(group))
                        return edata;
                }
            }
            return null;
        }

        //Get the EquipAttach on the character (positions on the body where the equipments can spawn)
        public EquipAttach GetEquipAttachment(EquipSlot slot)
        {
            if (slot == EquipSlot.None)
                return null;

            foreach (EquipAttach attach in equip_attachments)
            {
                if (attach.slot == slot)
                {
                    return attach;
                }
            }
            return null;
        }

        public EquipItem GetEquipItemObject(EquipSlot slot)
        {
            if (slot == EquipSlot.None)
                return null;

            foreach (KeyValuePair<string, EquipItem> item in visible_items)
            {
                if (item.Value.slot == slot)
                {
                    return item.Value;
                }
            }
            return null;
        }

        public AttachFX GetAttachFX(string id)
        {
            if (attachments_fx != null)
            {
                foreach (AttachFX fx in attachments_fx)
                {
                    if (fx.id == id)
                        return fx;
                }
            }
            return null;
        }

        public ItemEquipData GetDefaultItem(GroupData group)
        {
            foreach (ItemEquipData idata in default_items)
            {
                if (idata.HasGroup(group))
                    return idata;
            }
            return null;
        }

        private int GetEquipStack(InventoryData inventory, ItemEquipData item)
        {
            if (item is ItemProjData)
            {
                ItemProjData proj = (ItemProjData)item;
                int avail = inventory.GetQuantity(item.id);
                return Mathf.Min(avail, proj.equip_stack);
            }
            return 1;
        }

        public int GetEquipAttackBonus()
        {
            int value = 0;
            foreach (ItemEquipData item in GetItems())
            {
                value += item.attack_damage;
            }
            return value;
        }

        public int GetEquipArmorBonus()
        {
            int value = 0;
            foreach (ItemEquipData item in GetItems())
            {
                value += item.armor;
            }
            return value;
        }

        public float GetEquipBonus(BonusType bonus, Interactable target = null, CraftData itarget =null)
        {
            float value = 0;
            foreach (ItemEquipData item in GetItems())
            {
                bool is_any = target == null && itarget == null;
                bool is_valid_select = target != null && target.Selectable.HasGroup(item.bonus_target);
                bool is_valid_item = itarget != null && itarget.HasGroup(item.bonus_target);

                //Only one of the target need to be valid, or have no target
                if (item.bonus == bonus && (is_any || is_valid_select || is_valid_item))
                    value += item.bonus_value;
            }
            return value;
        }

        //Includes equipped items + default items
        public List<ItemEquipData> GetItems()
        {
            List<ItemEquipData> list = new List<ItemEquipData>(default_items);
            InventoryData einventory = EquipData;
            foreach (KeyValuePair<string, int> item in einventory.items)
            {
                ItemData idata = ItemData.Get(item.Key);
                if (idata is ItemEquipData)
                {
                    ItemEquipData edata = (ItemEquipData)idata;
                    list.Add(edata);
                }
            }
            return list;
        }

        public void ShowTool(GroupData tool)
        {
            if (tool != null && !active_tools.Contains(tool))
                active_tools.Add(tool);
        }

        public void HideTool(GroupData tool)
        {
            active_tools.Remove(tool);
        }

        public void HideTools()
        {
            active_tools.Clear();
        }

        public bool IsToolVisible(GroupData group)
        {
            return active_tools.Contains(group);
        }

        public HashSet<GroupData> GetActiveTools()
        {
            return active_tools;
        }

        public Character GetCharacter()
        {
            return character;
        }
        
        public bool HasUID()
        {
            return !string.IsNullOrEmpty(uid.uid);
        }

        public string GetEquipUID()
        {
            if(HasUID())
                return uid.uid + "_equip";
            return "";
        }

        public InventoryData EquipData { get { return SaveData.Get().GetInventory(GetEquipUID()); } }
    }

}
