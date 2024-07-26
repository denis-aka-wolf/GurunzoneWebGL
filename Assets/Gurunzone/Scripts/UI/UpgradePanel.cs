using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gurunzone
{

    public class UpgradePanel : UISlotPanel
    {
        private Construction building;

        private static UpgradePanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;
        }

        protected override void Start()
        {
            base.Start();
            onClickSlot += OnClickSlot;
            TheControls.Get().onRClick += (Vector3) => { Refresh(); };
        }

        public void Refresh()
        {
            UpgradeInfoPanel.Get().Hide();
            foreach (UISlot slot in slots)
                slot.Hide();

            if (building == null || building.data == null || building.data.upgrades == null)
                return;

            int index = 0;
            foreach (ConstructionData upgrade in building.data.upgrades)
            {
                if (index < slots.Length)
                {
                    CraftSlot bslot = (CraftSlot)slots[index];
                    bslot.SetSlot(upgrade);
                    bslot.SetValid(upgrade.HasRequirements());
                    index++;
                }
            }
        }

        private void OnClickSlot(UISlot slot)
        {
            CraftSlot bslot = (CraftSlot)slot;
            UnhighlightSlots();
            bslot.SetHighlight(true);
            UpgradeInfoPanel.Get().ShowUpgrade(building, (ConstructionData) bslot.GetItem());
        }

        private void UnhighlightSlots()
        {
            foreach (UISlot slot in slots)
            {
                CraftSlot cslot = (CraftSlot)slot;
                cslot.SetHighlight(false);
            }
        }

        public void ShowBuilding(Construction building)
        {
            this.building = building;
            Refresh();
            Show();
        }

        public static UpgradePanel Get()
        {
            return instance;
        }
    }

}
