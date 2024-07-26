using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    /// <summary>
    /// Location where character can drop items at, it should go into the Global Inventory
    /// </summary>

    [RequireComponent(typeof(Selectable))]
    [RequireComponent(typeof(Interactable))]
    [RequireComponent(typeof(Inventory))]
    public class Storage : MonoBehaviour
    {
        private Selectable select;
        private Interactable interact;
        private Inventory inventory;
        private Construction construct;

        private static List<Storage> storage_list = new List<Storage>();

        void Awake()
        {
            storage_list.Add(this);
            select = GetComponent<Selectable>();
            interact = GetComponent<Interactable>();
            inventory = GetComponent<Inventory>();
            construct = GetComponent<Construction>();
        }

        private void OnDestroy()
        {
            storage_list.Remove(this);
        }

        void Update()
        {

        }

        public bool IsMax()
        {
            if(inventory)
                return inventory.IsMax();
            return false;
        }

        public bool IsActive()
        {
            if (construct != null)
                return construct.IsCompleted() && !IsMax();
            return !IsMax();
        }

        public Selectable Selectable { get { return select; } }
        public Interactable Interactable { get { return interact; } }
        public Inventory Inventory { get { return inventory;  } }

        //Nearest with space in inventory
        public static Storage GetNearestActive(Vector3 pos, float range = 999f)
        {
            float min_dist = range;
            Storage nearest = null;
            foreach (Storage storage in storage_list)
            {
                float dist = (pos - storage.transform.position).magnitude;
                if (dist < min_dist && storage.IsActive())
                {
                    min_dist = dist;
                    nearest = storage;
                }
            }
            return nearest;
        }

        public static Storage GetNearestGlobal(Vector3 pos, float range = 999f)
        {
            float min_dist = range;
            Storage nearest = null;
            foreach (Storage storage in storage_list)
            {
                float dist = (pos - storage.transform.position).magnitude;
                if (dist < min_dist && storage.Inventory.global)
                {
                    min_dist = dist;
                    nearest = storage;
                }
            }
            return nearest;
        }

        public static Storage GetNearest(Vector3 pos, float range = 999f)
        {
            float min_dist = range;
            Storage nearest = null;
            foreach (Storage storage in storage_list)
            {
                float dist = (pos - storage.transform.position).magnitude;
                if (dist < min_dist)
                {
                    min_dist = dist;
                    nearest = storage;
                }
            }
            return nearest;
        }

        public static List<Storage> GetAll()
        {
            return storage_list;
        }
    }
}
