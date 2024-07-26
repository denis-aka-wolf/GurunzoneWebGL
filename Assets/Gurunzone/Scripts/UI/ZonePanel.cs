using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{

    public class ZonePanel : SelectPanel
    {
        [Header("Zone")]
        public IntSelector workers_select;
        public CraftSlot[] slots;

        private Zone zone;
        private int zone_count = 0;

        private static ZonePanel instance;
    
        protected override void Awake()
        {
            base.Awake();
            instance = this;
        }

        protected override void Start()
        {
            base.Start();
            workers_select.onChange += OnChangeWorkers;

            foreach (CraftSlot slot in slots)
                slot.onClick += OnClickSlot;
        }

        protected override void Update()
        {
            base.Update();

            if (zone != null && zone_count != zone.CountAllInZone())
                RefreshPanel();
        }

        private void RefreshPanel()
        {
            zone_count = zone.CountAllInZone();
            workers_select.value_max = zone.Workable.workers_max;
            workers_select.SetValue(zone.Workable.GetWorkerAmount());

            foreach (CraftSlot slot in slots)
                slot.Hide();

            List<Gatherable> gathers = zone.GetGatherablesInZone();
            List<Item> items = zone.GetItemsInZone();
            List<ItemData> idatalist = new List<ItemData>();

            foreach (Gatherable gather in gathers)
            {
                if (gather.item != null && !idatalist.Contains(gather.item))
                    idatalist.Add(gather.item);
            }

            foreach (Item item in items)
            {
                if (item.data != null && !idatalist.Contains(item.data))
                    idatalist.Add(item.data);
            }

            int index = 0;
            foreach (ItemData item in idatalist)
            {
                if (index < slots.Length)
                {
                    CraftSlot slot = slots[index];
                    slot.SetSlot(item, 0);
                    slot.SetHighlight(item == zone.GetPriority());
                    index++;
                }
            }
        }

        private void OnClickSlot(UISlot slot)
        {
            CraftSlot cslot = (CraftSlot)slot;
            ItemData idata = (ItemData) cslot.GetItem();
            ItemData priority = zone.GetPriority();
            zone.SetPriority((idata == priority) ? null : idata);
            RefreshPanel();

            foreach (Colonist colonist in zone.Workable.GetAssignedWorkers())
                colonist.StopWork();
        }

        public override void Show(bool instant = false)
        {
            base.Show(instant);
            zone = select.GetComponent<Zone>();
            RefreshPanel();
        }

        private void OnChangeWorkers(int value)
        {
            if (zone)
            {
                zone.Workable.SetWorkerAmount(value);
            }
        }

        public void OnClickRemove()
        {
            if (zone != null)
                zone.Kill();
            Hide();
        }

        public override bool IsShowable(Selectable select)
        {
            Zone zone = select.GetComponent<Zone>();
            return zone != null;
        }

        public static ZonePanel Get()
        {
            return instance;
        }
    }

}
