using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    /// <summary>
    /// An item that can shot (ammo, arrows, etc.)
    /// </summary>

    [CreateAssetMenu(fileName = "ItemProjData", menuName = "Gurunzone/AppData/ItemProjData", order = 10)]
    public class ItemProjData : ItemEquipData
    {
        [Header("Projectile")]
        public int damage_bonus;        //Bonus damage provided by this projectile, default is 0
        public int equip_stack = 10;    //Stack limit when equipped
        public GameObject projectile_prefab;    //Prefab that is spawned when projectile is shot

        public override ItemType Type { get { return ItemType.Projectile; } }

        public static new ItemProjData Get(string id)
        {
            foreach (ItemData data in ilist)
            {
                if (data.id == id && data is ItemProjData)
                    return (ItemProjData)data;
            }
            return null;
        }
    }
}
