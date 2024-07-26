using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gurunzone
{

    public class UpgradeInfoPanel : UIPanel
    {
        public CraftSlot icon;
        public Text title;
        public Text description;
        public CraftSlot[] cost_slots;
        public Button build_button;

        private Construction building;
        private ConstructionData upgrade;

        private static UpgradeInfoPanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;
        }

        protected override void Start()
        {
            base.Start();
            TheControls.Get().onRClick += (Vector3) => { Hide(); };
        }

        protected override void Update()
        {
            base.Update();

            if (upgrade != null && IsVisible())
            {
                build_button.interactable = upgrade.HasRequirements() && upgrade.HasCraftCost();
                ShowCost(upgrade);
            }
        }

        public void OnClickBuild()
        {
            if (upgrade != null && upgrade.HasRequirements() && upgrade.HasCraftCost())
            {
                building.Upgrade(upgrade);
                UpgradePanel.Get().Hide();
                Hide();
            }
        }

        public void ShowUpgrade(Construction building, ConstructionData upgrade)
        {
            this.building = building;
            this.upgrade = upgrade;
            icon.SetSlot(upgrade);
            title.text = upgrade.title;
            description.text = upgrade.desc;

            ShowCost(upgrade);
            Show();
        }


        private void ShowCost(CraftData item)
        {
            if (item != null)
            {
                Inventory inventory = Inventory.GetGlobal();
                CraftCostData cost = item.GetCraftCost();

                foreach (CraftSlot cslot in cost_slots)
                    cslot.Hide();

                int index = 0;
                foreach (KeyValuePair<ItemData, int> pair in cost.items)
                {
                    if (index < cost_slots.Length)
                    {
                        CraftSlot cslot = cost_slots[index];
                        cslot.SetSlot(pair.Key, pair.Value);
                        cslot.SetQuantityValid(inventory.HasItem(pair.Key, pair.Value));
                        index++;
                    }
                }
                foreach (KeyValuePair<GroupData, int> pair in cost.fillers)
                {
                    if (index < cost_slots.Length)
                    {
                        CraftSlot cslot = cost_slots[index];
                        cslot.SetSlot(pair.Key, pair.Value);
                        cslot.SetQuantityValid(inventory.HasItemGroup(pair.Key, pair.Value));
                        index++;
                    }
                }
                foreach (KeyValuePair<CraftData, int> pair in cost.requirements)
                {
                    if (index < cost_slots.Length)
                    {
                        CraftSlot cslot = cost_slots[index];
                        cslot.SetSlot(pair.Key);
                        index++;
                    }
                }
            }
        }

        public static UpgradeInfoPanel Get()
        {
            return instance;
        }
    }

}
