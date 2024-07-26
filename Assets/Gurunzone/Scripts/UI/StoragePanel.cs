using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{

    public class StoragePanel : SelectPanel
    {
        public ItemSlot[] slots;

        private Storage storage;

        private static StoragePanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;
        }

        protected override void Update()
        {
            base.Update();

            if (IsVisible())
            {
                foreach (ItemSlot slot in slots)
                    slot.Hide();

                int index = 0;
                Inventory inventory = storage.Inventory;
                foreach (ItemSet item in storage.Inventory.GetItems())
                {
                    if (index < slots.Length)
                    {
                        ItemSlot slot = slots[index];
                        slot.SetSlot(item.item, item.quantity);
                        index++;
                    }
                }
            }
        }

        public override void Show(bool instant = false)
        {
            base.Show(instant);
            storage = select.GetComponent<Storage>();
        }

        public override bool IsShowable(Selectable select)
        {
            Storage storage = select.GetComponent<Storage>();
            return storage != null;
        }

        public override int GetPriority()
        {
            return 5;
        }

        public static StoragePanel Get()
        {
            return instance;
        }
    }
}
