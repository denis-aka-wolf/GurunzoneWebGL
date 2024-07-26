using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    public enum EquipSlot
    {
        None = 0,
        Hand = 5,
        Head = 10,
        Body = 20,
        Feet = 30,
        Ammo = 40,
    }

    public enum EquipType
    {
        Equip = 0,
        WeaponMelee = 5,
        WeaponRanged = 10,
    }

    /// <summary>
    /// An item that can be equipped
    /// </summary>

    [CreateAssetMenu(fileName = "ItemEquipData", menuName = "Gurunzone/AppData/ItemEquipData", order = 10)]
    public class ItemEquipData : ItemData
    {
        [Header("Equip")]
        public EquipType equip_type;   //What type of item is that, a weapon or no?
        public EquipSlot equip_slot;   //Can only have 1 equipped item per slot
        public int armor;              //When equipped, will add to the character armor

        [Header("Equip Bonus")]
        public BonusType bonus;     //When equipped, will provide this bonus
        public float bonus_value;
        public GroupData bonus_target; //If null, apply to all, if not apply to this target only (for gathering speed mostly)

        [Header("Equip Weapon")]
        public int attack_damage;           //When equipped, will add this attack damage
        public float attack_range = 1f;     //Attack range while equipped
        public GroupData projectile_group;  //Projectile item used for ranged weapons
        public GameObject projectile_default; //Default projectile, cost no ammunition

        [Header("Equip Prefab")]
        public GameObject equip_prefab;     //Prefab spawned attached to the character

        public override ItemType Type { get { return ItemType.Equipment; } }
        public bool IsWeapon() { return equip_type == EquipType.WeaponMelee || equip_type == EquipType.WeaponRanged; }
        public bool IsRanged() { return equip_type == EquipType.WeaponRanged; }

        public static new ItemEquipData Get(string id)
        {
            foreach (ItemData data in ilist)
            {
                if (data.id == id && data is ItemEquipData)
                    return (ItemEquipData) data;
            }
            return null;
        }
    }
}
