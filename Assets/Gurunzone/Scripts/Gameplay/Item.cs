using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    [RequireComponent(typeof(Selectable))]
    [RequireComponent(typeof(Interactable))]
    [RequireComponent(typeof(UniqueID))]
    public class Item : CSObject
    {
        public ItemData data;       //Item Data
        public int quantity = 1;    //Current quantity

        private Selectable select;
        private Interactable interact;
        private UniqueID uid;

        private static List<Item> item_list = new List<Item>();

        protected override void Awake()
        {
            base.Awake();
            item_list.Add(this);
            select = GetComponent<Selectable>();
            interact = GetComponent<Interactable>();
            uid = GetComponent<UniqueID>();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            item_list.Remove(this);
        }

        public override void Kill()
        {
            base.Kill();
            SaveData.Get().RemoveItem(uid.uid);
            item_list.Remove(this); //Remove from list immediately to avoid targeting removed item
            select.Kill();
        }

        public Selectable Selectable { get { return select; } }
        public Interactable Interactable { get { return interact; } }
        public SaveItemData SData { get { return SaveData.Get().GetItem(uid.uid); } } //SData is the saved data linked to this object

        public static new List<Item> GetAll()
        {
            return item_list;
        }

        public static new Item GetNearest(Vector3 pos, float range = 999f)
        {
            Item nearest = null;
            float min_dist = range;
            foreach (Item item in item_list)
            {
                float dist = (item.transform.position - pos).magnitude;
                if (dist < min_dist)
                {
                    min_dist = dist;
                    nearest = item;
                }
            }
            return nearest;
        }

        public static Item Spawn(string uid, SaveItemData data)
        {
            ItemData idata = ItemData.Get(data.id);
            if (idata != null && data.scene == SceneNav.GetCurrentScene())
            {
                GameObject obj = Instantiate(idata.prefab, data.pos, Quaternion.identity);
                Item item = obj.GetComponent<Item>();
                UniqueID uniqueid = obj.GetComponent<UniqueID>();
                uniqueid.uid = uid;
                return item;
            }
            return null;
        }

        public static Item Create(ItemData data, Vector3 pos, int quantity=1)
        {
            GameObject obj = Instantiate(data.prefab, pos, Quaternion.identity);
            Item item = obj.GetComponent<Item>();
            item.data = data;
            item.quantity = quantity;
            UniqueID uniqueid = obj.GetComponent<UniqueID>();
            uniqueid.uid = UniqueID.GenerateUniqueID();
            SaveData sdata = SaveData.Get();
            SaveItemData idata = sdata.GetItem(uniqueid.uid);
            idata.id = data.id;
            idata.scene = SceneNav.GetCurrentScene();
            idata.pos = pos;
            idata.quantity = quantity;
            idata.spawned = true;
            return item;
        }
    }
}
