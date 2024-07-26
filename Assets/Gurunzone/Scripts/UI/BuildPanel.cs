using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gurunzone
{

    public class BuildPanel : UISlotPanel
    {
        public string tab_group = "build";

        private GroupData filter;

        private static BuildPanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;
        }

        protected override void Start()
        {
            base.Start();
            onClickSlot += OnClickSlot;
            TheControls.Get().onRClick += (Vector3) => { Hide(); };

            foreach (TabFilter filter in TabFilter.GetAll(tab_group))
                filter.onClick += OnClickTab;
        }

        public void Refresh()
        {
            BuildInfoPanel.Get().Hide();
            foreach (UISlot slot in slots)
                slot.Hide();

            TabFilter.Select(tab_group, filter);

            Inventory global = Inventory.GetGlobal();

            int index = 0;
            List<ConstructionData> buildings = ConstructionData.GetAll();
            foreach (ConstructionData building in buildings)
            {
                bool filter_valid = filter == null || building.HasGroup(filter);
                if (index < slots.Length && building.craftable && filter_valid)
                {
                    CraftSlot bslot = (CraftSlot)slots[index];
                    bslot.SetSlot(building);
                    bslot.SetValid(building.HasRequirements());
                    index++;
                }
            }
        }

        private void OnClickSlot(UISlot slot)
        {
            CraftSlot bslot = (CraftSlot)slot;
            UnhighlightSlots();
            bslot.SetHighlight(true);
            BuildInfoPanel.Get().ShowBuilding((ConstructionData) bslot.GetItem());
        }

        private void OnClickTab(TabFilter filter)
        {
            this.filter = filter.filter_group;
            Refresh();
        }

        private void UnhighlightSlots()
        {
            foreach (UISlot slot in slots)
            {
                CraftSlot cslot = (CraftSlot)slot;
                cslot.SetHighlight(false);
            }
        }

        public override void Show(bool instant = false)
        {
            base.Show(instant);
            filter = null;
            Refresh();
        }

        public static BuildPanel Get()
        {
            return instance;
        }
    }

}
